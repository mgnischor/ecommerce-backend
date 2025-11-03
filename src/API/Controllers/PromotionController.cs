using System.Security.Claims;
using System.Text.RegularExpressions;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Controllers;

/// <summary>
/// Promotion management endpoints
/// </summary>
[Tags("Promotions")]
[ApiController]
[Route("api/v1/promotions")]
[Produces("application/json")]
public sealed class PromotionController : ControllerBase
{
    private readonly PostgresqlContext _context;
    private readonly ILogger<PromotionController> _logger;
    private const int MaxPromotions = 500;

    public PromotionController(PostgresqlContext context, ILogger<PromotionController> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private string? GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private bool IsAdmin() => User.IsInRole("Admin");

    private bool IsManager() => User.IsInRole("Manager");

    /// <summary>
    /// Retrieves all active promotions
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PromotionEntity>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PromotionEntity>>> GetActivePromotions(
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var now = DateTime.UtcNow;
            var promotions = await _context
                .Promotions.Where(p =>
                    p.IsActive && !p.IsDeleted && p.StartDate <= now && p.EndDate >= now
                )
                .OrderByDescending(p => p.Priority)
                .ThenByDescending(p => p.StartDate)
                .Take(MaxPromotions)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Retrieved {Count} active promotions", promotions.Count);
            return Ok(promotions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active promotions");
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
    }

    /// <summary>
    /// Retrieves featured promotions
    /// </summary>
    [HttpGet("featured")]
    [ProducesResponseType(typeof(IEnumerable<PromotionEntity>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PromotionEntity>>> GetFeaturedPromotions(
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var now = DateTime.UtcNow;
            var promotions = await _context
                .Promotions.Where(p =>
                    p.IsFeatured
                    && p.IsActive
                    && !p.IsDeleted
                    && p.StartDate <= now
                    && p.EndDate >= now
                )
                .OrderByDescending(p => p.Priority)
                .Take(50) // Limit featured promotions
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Retrieved {Count} featured promotions", promotions.Count);
            return Ok(promotions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving featured promotions");
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
    }

    /// <summary>
    /// Retrieves a specific promotion by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PromotionEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PromotionEntity>> GetPromotionById(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid promotion GUID provided");
            return BadRequest(new { Message = "Invalid promotion ID" });
        }

        try
        {
            var promotion = await _context
                .Promotions.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);

            if (promotion == null)
            {
                _logger.LogInformation("Promotion not found: {PromotionId}", id);
                return NotFound(new { Message = "Promotion not found" });
            }

            return Ok(promotion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving promotion: {PromotionId}", id);
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
    }

    /// <summary>
    /// Retrieves a promotion by code
    /// </summary>
    [HttpGet("code/{code}")]
    [ProducesResponseType(typeof(PromotionEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PromotionEntity>> GetPromotionByCode(
        string code,
        CancellationToken cancellationToken = default
    )
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(code))
        {
            _logger.LogWarning("Empty promotion code provided");
            return BadRequest(new { Message = "Promotion code is required" });
        }

        if (code.Length > 50)
        {
            _logger.LogWarning("Promotion code too long: {Length}", code.Length);
            return BadRequest(new { Message = "Promotion code must not exceed 50 characters" });
        }

        // Validate code format (alphanumeric, hyphens, underscores)
        if (!Regex.IsMatch(code, @"^[a-zA-Z0-9\-_]+$"))
        {
            _logger.LogWarning("Invalid promotion code format: {Code}", code);
            return BadRequest(new { Message = "Invalid promotion code format" });
        }

        try
        {
            var promotion = await _context
                .Promotions.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Code == code && !p.IsDeleted, cancellationToken);

            if (promotion == null)
            {
                _logger.LogInformation("Promotion code not found: {Code}", code);
                return NotFound(new { Message = "Promotion not found" });
            }

            // Check if promotion is currently valid
            var now = DateTime.UtcNow;
            if (!promotion.IsActive || promotion.StartDate > now || promotion.EndDate < now)
            {
                _logger.LogInformation("Promotion code inactive or expired: {Code}", code);
                return NotFound(new { Message = "Promotion not found" });
            }

            _logger.LogInformation("Promotion code validated: {Code}", code);
            return Ok(promotion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving promotion by code");
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
    }

    /// <summary>
    /// Creates a new promotion
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(PromotionEntity), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PromotionEntity>> CreatePromotion(
        [FromBody] PromotionEntity promotion,
        CancellationToken cancellationToken = default
    )
    {
        if (promotion == null)
        {
            _logger.LogWarning("Null promotion data received");
            return BadRequest(new { Message = "Promotion data is required" });
        }

        // Input validation
        if (string.IsNullOrWhiteSpace(promotion.Name) || promotion.Name.Length > 200)
        {
            return BadRequest(
                new { Message = "Valid promotion name is required (max 200 characters)" }
            );
        }

        if (string.IsNullOrWhiteSpace(promotion.Code) || promotion.Code.Length > 50)
        {
            return BadRequest(
                new { Message = "Valid promotion code is required (max 50 characters)" }
            );
        }

        if (!Regex.IsMatch(promotion.Code, @"^[a-zA-Z0-9\-_]+$"))
        {
            return BadRequest(
                new
                {
                    Message = "Invalid promotion code format. Use alphanumeric, hyphens, underscores only",
                }
            );
        }

        if (promotion.StartDate >= promotion.EndDate)
        {
            return BadRequest(new { Message = "End date must be after start date" });
        }

        if (
            promotion.DiscountPercentage.HasValue
            && (promotion.DiscountPercentage < 0 || promotion.DiscountPercentage > 100)
        )
        {
            return BadRequest(new { Message = "Discount percentage must be between 0 and 100" });
        }

        try
        {
            // Check for duplicate code
            var duplicateCode = await _context.Promotions.AnyAsync(
                p => p.Code == promotion.Code && !p.IsDeleted,
                cancellationToken
            );

            if (duplicateCode)
            {
                _logger.LogWarning("Duplicate promotion code attempt: {Code}", promotion.Code);
                return Conflict(new { Message = "Promotion code already exists" });
            }

            // Secure assignment
            var newPromotion = new PromotionEntity
            {
                Id = Guid.NewGuid(),
                Name = promotion.Name,
                Code = promotion.Code,
                Description = promotion.Description,
                Type = promotion.Type,
                DiscountPercentage = promotion.DiscountPercentage,
                DiscountAmount = promotion.DiscountAmount,
                MinimumOrderAmount = promotion.MinimumOrderAmount,
                MaximumDiscountAmount = promotion.MaximumDiscountAmount,
                StartDate = promotion.StartDate,
                EndDate = promotion.EndDate,
                IsActive = promotion.IsActive,
                IsFeatured = promotion.IsFeatured,
                Priority = promotion.Priority,
                MaxUsageCount = promotion.MaxUsageCount,
                UsageCount = 0, // Always start at 0
                IsCombinable = promotion.IsCombinable,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = Guid.TryParse(GetCurrentUserId(), out var userId) ? userId : Guid.Empty,
            };

            _context.Promotions.Add(newPromotion);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Promotion created: {PromotionId}, Code: {Code}, User: {UserId}",
                newPromotion.Id,
                newPromotion.Code,
                GetCurrentUserId()
            );

            return CreatedAtAction(
                nameof(GetPromotionById),
                new { id = newPromotion.Id },
                newPromotion
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating promotion with code: {Code}", promotion.Code);
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
    }

    /// <summary>
    /// Updates an existing promotion
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdatePromotion(
        Guid id,
        [FromBody] PromotionEntity promotion,
        CancellationToken cancellationToken = default
    )
    {
        if (promotion == null)
        {
            _logger.LogWarning("Null promotion data received for update");
            return BadRequest(new { Message = "Promotion data is required" });
        }

        if (id == Guid.Empty || id != promotion.Id)
        {
            _logger.LogWarning(
                "ID mismatch in promotion update. Route: {RouteId}, Body: {BodyId}",
                id,
                promotion.Id
            );
            return BadRequest(new { Message = "ID mismatch" });
        }

        // Input validation
        if (string.IsNullOrWhiteSpace(promotion.Name) || promotion.Name.Length > 200)
        {
            return BadRequest(
                new { Message = "Valid promotion name is required (max 200 characters)" }
            );
        }

        if (promotion.StartDate >= promotion.EndDate)
        {
            return BadRequest(new { Message = "End date must be after start date" });
        }

        try
        {
            var existingPromotion = await _context.Promotions.FirstOrDefaultAsync(
                p => p.Id == id && !p.IsDeleted,
                cancellationToken
            );

            if (existingPromotion == null)
            {
                _logger.LogWarning("Promotion not found for update: {PromotionId}", id);
                return NotFound(new { Message = "Promotion not found" });
            }

            // Check for code conflict if code changed
            if (existingPromotion.Code != promotion.Code)
            {
                var duplicateCode = await _context.Promotions.AnyAsync(
                    p => p.Code == promotion.Code && p.Id != id && !p.IsDeleted,
                    cancellationToken
                );

                if (duplicateCode)
                {
                    _logger.LogWarning(
                        "Duplicate promotion code in update: {Code}",
                        promotion.Code
                    );
                    return Conflict(new { Message = "Promotion code already exists" });
                }
            }

            // Selective update - prevent mass assignment
            existingPromotion.Name = promotion.Name;
            existingPromotion.Code = promotion.Code;
            existingPromotion.Description = promotion.Description;
            existingPromotion.Type = promotion.Type;
            existingPromotion.DiscountPercentage = promotion.DiscountPercentage;
            existingPromotion.DiscountAmount = promotion.DiscountAmount;
            existingPromotion.MinimumOrderAmount = promotion.MinimumOrderAmount;
            existingPromotion.MaximumDiscountAmount = promotion.MaximumDiscountAmount;
            existingPromotion.StartDate = promotion.StartDate;
            existingPromotion.EndDate = promotion.EndDate;
            existingPromotion.IsActive = promotion.IsActive;
            existingPromotion.IsFeatured = promotion.IsFeatured;
            existingPromotion.Priority = promotion.Priority;
            existingPromotion.MaxUsageCount = promotion.MaxUsageCount;
            existingPromotion.IsCombinable = promotion.IsCombinable;
            existingPromotion.UpdatedAt = DateTime.UtcNow;
            existingPromotion.UpdatedBy = Guid.TryParse(GetCurrentUserId(), out var userId)
                ? userId
                : Guid.Empty;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Promotion updated: {PromotionId}, User: {UserId}",
                id,
                GetCurrentUserId()
            );
            return NoContent();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict updating promotion: {PromotionId}", id);
            return Conflict(
                new
                {
                    Message = "The promotion was modified by another user. Please refresh and try again",
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating promotion: {PromotionId}", id);
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
    }

    /// <summary>
    /// Deletes a promotion (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePromotion(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid promotion GUID provided for deletion");
            return BadRequest(new { Message = "Invalid promotion ID" });
        }

        try
        {
            var promotion = await _context.Promotions.FirstOrDefaultAsync(
                p => p.Id == id && !p.IsDeleted,
                cancellationToken
            );

            if (promotion == null)
            {
                _logger.LogWarning("Promotion not found for deletion: {PromotionId}", id);
                return NotFound(new { Message = "Promotion not found" });
            }

            promotion.IsDeleted = true;
            promotion.UpdatedAt = DateTime.UtcNow;
            promotion.UpdatedBy = Guid.TryParse(GetCurrentUserId(), out var userId)
                ? userId
                : Guid.Empty;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogWarning(
                "Promotion deleted: {PromotionId}, Code: {Code}, User: {UserId}",
                id,
                promotion.Code,
                GetCurrentUserId()
            );
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting promotion: {PromotionId}", id);
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
    }
}
