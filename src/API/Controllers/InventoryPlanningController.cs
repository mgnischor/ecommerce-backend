using Comex.API.Constants;
using Comex.Application.Interfaces;
using Comex.Application.Services;
using Comex.Domain.Entities;
using Comex.Domain.Policies;
using Comex.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Comex.API.Controllers;

/// <summary>
/// Provides HTTP endpoints for managing inventory plans and demand forecasting.
/// Supports creating, retrieving, updating, analyzing, and deleting inventory plans with role-based access control.
/// </summary>
/// <remarks>
/// This controller handles all inventory planning operations including:
/// <list type="bullet">
/// <item><description>Managing stock levels, reorder points, and lead times</description></item>
/// <item><description>Computing sales forecasts and safety stock requirements</description></item>
/// <item><description>Evaluating reorder needs, stock age, and inventory turnover</description></item>
/// <item><description>Soft-delete support for maintaining historical records</description></item>
/// </list>
/// All business rules are enforced via <see cref="InventoryPlanningPolicy"/>.
/// All responses are in JSON format and follow RESTful conventions.
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="InventoryPlanningController"/> class.
/// </remarks>
/// <param name="context">The database context for data access operations.</param>
/// <param name="logger">The logger for recording operational events and errors.</param>
/// <exception cref="ArgumentNullException">
/// Thrown when <paramref name="context"/> or <paramref name="logger"/> is null.
/// </exception>
[Tags("Inventory Planning")]
[ApiController]
[Route("api/v1/inventory-planning")]
[Produces("application/json")]
public sealed class InventoryPlanningController(
    PostgresqlContext context,
    LoggingService<InventoryPlanningController> logger
) : ControllerBase
{
    /// <summary>
    /// The database context for accessing inventory plan data.
    /// </summary>
    private readonly PostgresqlContext _context =
        context ?? throw new ArgumentNullException(nameof(context));

    /// <summary>
    /// Logger instance for recording controller operations and errors.
    /// </summary>
    private readonly ILoggingService _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Retrieves a specific inventory plan by its unique identifier.
    /// </summary>
    /// <param name="id">The unique GUID of the inventory plan to retrieve.</param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The inventory plan entity if found; otherwise, a 404 Not Found response.
    /// </returns>
    /// <remarks>
    /// Retrieves complete inventory plan details including stock levels, demand, and performance metrics.
    /// Excludes soft-deleted plans.
    /// </remarks>
    /// <response code="200">Returns the inventory plan entity.</response>
    /// <response code="400">Invalid inventory plan ID format provided.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="404">Inventory plan not found or has been deleted.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(InventoryPlanEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<InventoryPlanEntity>> GetInventoryPlanById(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid inventory plan GUID provided");
            return BadRequest(new { Message = ErrorMessages.InvalidId });
        }

        try
        {
            var plan = await _context
                .InventoryPlans.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);

            if (plan == null)
            {
                _logger.LogInformation("Inventory plan not found: {PlanId}", id);
                return NotFound(new { Message = ErrorMessages.InventoryPlanNotFound });
            }

            return Ok(plan);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving inventory plan: {PlanId}", id);
            return StatusCode(500, ErrorMessages.ProcessingRequestError);
        }
    }

    /// <summary>
    /// Retrieves an inventory plan by the associated product identifier.
    /// </summary>
    /// <param name="productId">The unique GUID of the product to look up.</param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The inventory plan entity if found; otherwise, a 404 Not Found response.
    /// </returns>
    /// <remarks>
    /// Each product has at most one inventory plan. Useful for product-level
    /// planning views and reorder dashboards.
    /// </remarks>
    /// <response code="200">Returns the inventory plan entity.</response>
    /// <response code="400">Invalid product ID format provided.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="404">Inventory plan not found or has been deleted.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpGet("product/{productId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(InventoryPlanEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<InventoryPlanEntity>> GetInventoryPlanByProductId(
        Guid productId,
        CancellationToken cancellationToken = default
    )
    {
        if (productId == Guid.Empty)
        {
            _logger.LogWarning("Invalid product GUID provided");
            return BadRequest(new { Message = ErrorMessages.InvalidId });
        }

        try
        {
            var plan = await _context
                .InventoryPlans.AsNoTracking()
                .FirstOrDefaultAsync(
                    p => p.ProductId == productId && !p.IsDeleted,
                    cancellationToken
                );

            if (plan == null)
            {
                _logger.LogInformation(
                    "Inventory plan not found for product: {ProductId}",
                    productId
                );
                return NotFound(new { Message = ErrorMessages.InventoryPlanNotFound });
            }

            return Ok(plan);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving inventory plan for product: {ProductId}",
                productId
            );
            return StatusCode(500, ErrorMessages.ProcessingRequestError);
        }
    }

    /// <summary>
    /// Creates a new inventory plan for a product.
    /// </summary>
    /// <param name="plan">
    /// The inventory plan entity containing stock levels, demand, and performance metrics.
    /// </param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The newly created inventory plan entity with server-generated ID and timestamps.
    /// </returns>
    /// <remarks>
    /// <para><b>Validation Rules:</b></para>
    /// <list type="bullet">
    /// <item><description>Product ID must be provided</description></item>
    /// <item><description>CurrentStock, ReorderPoint, and AverageDailySales must be non-negative</description></item>
    /// <item><description>Each product can have only one inventory plan</description></item>
    /// </list>
    /// <para><b>Automatic Field Assignment:</b></para>
    /// <list type="bullet">
    /// <item><description>Id: Auto-generated GUID</description></item>
    /// <item><description>ReceivedAt: Set to current UTC time when not provided</description></item>
    /// <item><description>CreatedAt/UpdatedAt: Set to current UTC time</description></item>
    /// </list>
    /// </remarks>
    /// <response code="201">Inventory plan created successfully. Returns the created entity with Location header.</response>
    /// <response code="400">Invalid input data or validation failure.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="403">Insufficient permissions - Admin, Manager, or Developer role required.</response>
    /// <response code="409">An inventory plan already exists for this product.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Developer")]
    [ProducesResponseType(typeof(InventoryPlanEntity), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<InventoryPlanEntity>> CreateInventoryPlan(
        [FromBody] InventoryPlanEntity plan,
        CancellationToken cancellationToken = default
    )
    {
        if (plan == null)
        {
            _logger.LogWarning("Null inventory plan data received");
            return BadRequest(new { Message = ErrorMessages.InventoryPlanDataRequired });
        }

        if (plan.ProductId == Guid.Empty)
        {
            _logger.LogWarning("Invalid product ID provided for inventory plan");
            return BadRequest(new { Message = ErrorMessages.InvalidId });
        }

        if (
            plan.CurrentStock < 0
            || plan.ReorderPoint < 0
            || plan.AverageDailySales < 0
            || plan.SalesInLast90Days < 0
        )
        {
            _logger.LogWarning("Negative stock values provided for inventory plan");
            return BadRequest(new { Message = ErrorMessages.InvalidCurrentStock });
        }

        try
        {
            var existingPlan = await _context.InventoryPlans.AnyAsync(
                p => p.ProductId == plan.ProductId && !p.IsDeleted,
                cancellationToken
            );

            if (existingPlan)
            {
                _logger.LogWarning(
                    "Duplicate inventory plan attempt for product: {ProductId}",
                    plan.ProductId
                );
                return Conflict(
                    new { Message = "An inventory plan already exists for this product" }
                );
            }

            var newPlan = new InventoryPlanEntity
            {
                Id = Guid.NewGuid(),
                ProductId = plan.ProductId,
                CurrentStock = plan.CurrentStock,
                ReorderPoint = plan.ReorderPoint,
                AverageDailySales = plan.AverageDailySales,
                LeadTimeDays = plan.LeadTimeDays,
                ServiceLevelPercent = plan.ServiceLevelPercent,
                ReceivedAt = plan.ReceivedAt == default ? DateTime.UtcNow : plan.ReceivedAt,
                CostOfGoodsSold = plan.CostOfGoodsSold,
                AverageInventoryValue = plan.AverageInventoryValue,
                SalesInLast90Days = plan.SalesInLast90Days,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            _context.InventoryPlans.Add(newPlan);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Inventory plan created: {PlanId}, Product: {ProductId}",
                newPlan.Id,
                newPlan.ProductId
            );

            return CreatedAtAction(nameof(GetInventoryPlanById), new { id = newPlan.Id }, newPlan);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error creating inventory plan for product: {ProductId}",
                plan.ProductId
            );
            return StatusCode(500, new { Message = ErrorMessages.ProcessingRequestError });
        }
    }

    /// <summary>
    /// Updates an existing inventory plan's stock levels, demand, and performance metrics.
    /// </summary>
    /// <param name="id">The unique GUID of the inventory plan to update.</param>
    /// <param name="plan">
    /// The inventory plan entity with updated values. The Id must match the route parameter.
    /// </param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// No content on success (204).
    /// </returns>
    /// <remarks>
    /// <para><b>Updatable Fields:</b></para>
    /// <list type="bullet">
    /// <item><description>CurrentStock, ReorderPoint, AverageDailySales</description></item>
    /// <item><description>LeadTimeDays, ServiceLevelPercent, ReceivedAt</description></item>
    /// <item><description>CostOfGoodsSold, AverageInventoryValue, SalesInLast90Days</description></item>
    /// </list>
    /// <para><b>Protected Fields (Not Updated):</b></para>
    /// <list type="bullet">
    /// <item><description>Id, ProductId: Cannot be changed</description></item>
    /// <item><description>CreatedAt: Audit field remains unchanged</description></item>
    /// </list>
    /// </remarks>
    /// <response code="204">Inventory plan updated successfully. No content returned.</response>
    /// <response code="400">Invalid input data, ID mismatch, or validation failure.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="403">Insufficient permissions - Admin, Manager, or Developer role required.</response>
    /// <response code="404">Inventory plan not found or has been deleted.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager,Developer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateInventoryPlan(
        Guid id,
        [FromBody] InventoryPlanEntity plan,
        CancellationToken cancellationToken = default
    )
    {
        if (plan == null)
        {
            _logger.LogWarning("Null inventory plan data received for update");
            return BadRequest(new { Message = ErrorMessages.InventoryPlanDataRequired });
        }

        if (id == Guid.Empty || id != plan.Id)
        {
            _logger.LogWarning(
                "ID mismatch in inventory plan update. Route: {RouteId}, Body: {BodyId}",
                id,
                plan.Id
            );
            return BadRequest(new { Message = ErrorMessages.IdMismatch });
        }

        if (plan.CurrentStock < 0 || plan.ReorderPoint < 0 || plan.AverageDailySales < 0)
        {
            _logger.LogWarning("Negative stock values provided for inventory plan update");
            return BadRequest(new { Message = ErrorMessages.InvalidCurrentStock });
        }

        try
        {
            var existingPlan = await _context.InventoryPlans.FirstOrDefaultAsync(
                p => p.Id == id && !p.IsDeleted,
                cancellationToken
            );

            if (existingPlan == null)
            {
                _logger.LogWarning("Inventory plan not found for update: {PlanId}", id);
                return NotFound(new { Message = ErrorMessages.InventoryPlanNotFound });
            }

            existingPlan.CurrentStock = plan.CurrentStock;
            existingPlan.ReorderPoint = plan.ReorderPoint;
            existingPlan.AverageDailySales = plan.AverageDailySales;
            existingPlan.LeadTimeDays = plan.LeadTimeDays;
            existingPlan.ServiceLevelPercent = plan.ServiceLevelPercent;
            existingPlan.ReceivedAt = plan.ReceivedAt;
            existingPlan.CostOfGoodsSold = plan.CostOfGoodsSold;
            existingPlan.AverageInventoryValue = plan.AverageInventoryValue;
            existingPlan.SalesInLast90Days = plan.SalesInLast90Days;
            existingPlan.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Inventory plan updated: {PlanId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating inventory plan: {PlanId}", id);
            return StatusCode(500, new { Message = ErrorMessages.ProcessingRequestError });
        }
    }

    /// <summary>
    /// Computes the inventory planning analysis for a product, including forecasts and stock health metrics.
    /// </summary>
    /// <param name="id">The unique GUID of the inventory plan to analyze.</param>
    /// <param name="historicalSales">
    /// Optional comma-separated list of historical sales values used to compute the demand forecast.
    /// When omitted, the forecast is based on the plan's average daily sales.
    /// </param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// An object containing the forecast, safety stock, reorder status, and stock health metrics.
    /// </returns>
    /// <remarks>
    /// <para><b>Computed Values:</b></para>
    /// <list type="bullet">
    /// <item><description>Forecast: Sales forecast via <see cref="InventoryPlanningPolicy.CalculateSalesForecast"/> using the supplied historical sales</description></item>
    /// <item><description>SafetyStock: Required safety stock via <see cref="InventoryPlanningPolicy.CalculateSafetyStock"/></description></item>
    /// <item><description>ReorderNeeded: True when current stock is at or below the reorder point via <see cref="InventoryPlanningPolicy.IsReorderNeeded"/></description></item>
    /// <item><description>DaysOfSupply: Estimated days of stock remaining via <see cref="InventoryPlanningPolicy.CalculateDaysOfSupply"/></description></item>
    /// <item><description>StockAgeExcessive: True when stock age exceeds the maximum via <see cref="InventoryPlanningPolicy.IsStockAgeExcessive"/></description></item>
    /// <item><description>InventoryTurnover: Turnover ratio via <see cref="InventoryPlanningPolicy.CalculateInventoryTurnover"/></description></item>
    /// <item><description>LiquidateSlowMoving: True when stock should be liquidated via <see cref="InventoryPlanningPolicy.ShouldLiquidateSlowMoving"/></description></item>
    /// </list>
    /// </remarks>
    /// <response code="200">Returns the inventory planning analysis.</response>
    /// <response code="400">Invalid inventory plan ID format provided.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="404">Inventory plan not found or has been deleted.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpGet("{id:guid}/analysis")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetInventoryPlanAnalysis(
        Guid id,
        [FromQuery] string? historicalSales = null,
        CancellationToken cancellationToken = default
    )
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid inventory plan GUID provided for analysis");
            return BadRequest(new { Message = ErrorMessages.InvalidId });
        }

        try
        {
            var plan = await _context
                .InventoryPlans.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);

            if (plan == null)
            {
                _logger.LogInformation("Inventory plan not found for analysis: {PlanId}", id);
                return NotFound(new { Message = ErrorMessages.InventoryPlanNotFound });
            }

            int[] salesHistory = Array.Empty<int>();
            if (!string.IsNullOrWhiteSpace(historicalSales))
            {
                salesHistory = historicalSales
                    .Split(
                        ',',
                        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
                    )
                    .Select(v => int.TryParse(v, out var value) ? value : 0)
                    .ToArray();
            }

            var hasHistoricalSales = salesHistory.Length > 0;
            var forecast = hasHistoricalSales
                ? InventoryPlanningPolicy.CalculateSalesForecast(salesHistory, null)
                : plan.AverageDailySales * 30;

            return Ok(
                new
                {
                    plan.Id,
                    plan.ProductId,
                    plan.CurrentStock,
                    plan.ReorderPoint,
                    Forecast = forecast,
                    SafetyStock = InventoryPlanningPolicy.CalculateSafetyStock(
                        plan.AverageDailySales,
                        plan.LeadTimeDays,
                        plan.ServiceLevelPercent
                    ),
                    ReorderNeeded = InventoryPlanningPolicy.IsReorderNeeded(
                        plan.CurrentStock,
                        plan.ReorderPoint
                    ),
                    DaysOfSupply = InventoryPlanningPolicy.CalculateDaysOfSupply(
                        plan.CurrentStock,
                        plan.AverageDailySales
                    ),
                    StockAgeExcessive = InventoryPlanningPolicy.IsStockAgeExcessive(
                        plan.ReceivedAt,
                        null
                    ),
                    InventoryTurnover = InventoryPlanningPolicy.CalculateInventoryTurnover(
                        plan.CostOfGoodsSold,
                        plan.AverageInventoryValue
                    ),
                    LiquidateSlowMoving = InventoryPlanningPolicy.ShouldLiquidateSlowMoving(
                        plan.CurrentStock,
                        plan.SalesInLast90Days
                    ),
                    ForecastBasis = hasHistoricalSales ? "HistoricalSales" : "AverageDailySales",
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error computing inventory plan analysis: {PlanId}", id);
            return StatusCode(500, ErrorMessages.ProcessingRequestError);
        }
    }

    /// <summary>
    /// Soft deletes an inventory plan, marking it as deleted while preserving the record for audit purposes.
    /// </summary>
    /// <param name="id">The unique GUID of the inventory plan to delete.</param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// No content on success (204).
    /// </returns>
    /// <remarks>
    /// <para><b>Soft Delete Behavior:</b></para>
    /// <list type="bullet">
    /// <item><description>Sets IsDeleted = true</description></item>
    /// <item><description>Updates UpdatedAt to current UTC time</description></item>
    /// <item><description>Preserves all plan data for reporting and audit trails</description></item>
    /// </list>
    /// <para>
    /// A deleted plan no longer appears in lookups or analysis dashboards.
    /// </para>
    /// </remarks>
    /// <response code="204">Inventory plan deleted successfully. No content returned.</response>
    /// <response code="400">Invalid inventory plan ID format provided.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="403">Insufficient permissions - Admin, Manager, or Developer role required.</response>
    /// <response code="404">Inventory plan not found or already deleted.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Manager,Developer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteInventoryPlan(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid inventory plan GUID provided for deletion");
            return BadRequest(new { Message = ErrorMessages.InvalidId });
        }

        try
        {
            var plan = await _context.InventoryPlans.FirstOrDefaultAsync(
                p => p.Id == id && !p.IsDeleted,
                cancellationToken
            );

            if (plan == null)
            {
                _logger.LogWarning("Inventory plan not found for deletion: {PlanId}", id);
                return NotFound(new { Message = ErrorMessages.InventoryPlanNotFound });
            }

            plan.IsDeleted = true;
            plan.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogWarning(
                "Inventory plan deleted: {PlanId}, Product: {ProductId}",
                id,
                plan.ProductId
            );
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting inventory plan: {PlanId}", id);
            return StatusCode(500, new { Message = ErrorMessages.ProcessingRequestError });
        }
    }
}
