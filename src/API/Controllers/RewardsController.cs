using ECommerce.API.Constants;
using ECommerce.API.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Policies;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Controllers;

/// <summary>
/// Provides HTTP endpoints for managing the customer loyalty rewards program.
/// Supports creating, retrieving, updating, earning, redeeming, and deleting loyalty accounts with role-based access control.
/// </summary>
/// <remarks>
/// This controller handles all rewards program operations including:
/// <list type="bullet">
/// <item><description>Managing loyalty account point balances and tiers</description></item>
/// <item><description>Earning points from purchases and reviews</description></item>
/// <item><description>Redeeming points for rewards</description></item>
/// <item><description>Calculating reward values and tier multipliers</description></item>
/// </list>
/// All business rules are enforced via <see cref="RewardsProgramPolicy"/>.
/// All responses are in JSON format and follow RESTful conventions.
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="RewardsController"/> class.
/// </remarks>
/// <param name="context">The database context for data access operations.</param>
/// <param name="logger">The logger for recording operational events and errors.</param>
/// <exception cref="ArgumentNullException">
/// Thrown when <paramref name="context"/> or <paramref name="logger"/> is null.
/// </exception>
[Tags("Rewards")]
[ApiController]
[Route("api/v1/rewards")]
[Produces("application/json")]
public sealed class RewardsController(
    PostgresqlContext context,
    LoggingService<RewardsController> logger
) : ControllerBase
{
    /// <summary>
    /// The database context for accessing loyalty reward data.
    /// </summary>
    private readonly PostgresqlContext _context =
        context ?? throw new ArgumentNullException(nameof(context));

    /// <summary>
    /// Logger instance for recording controller operations and errors.
    /// </summary>
    private readonly ILoggingService _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Retrieves a specific loyalty reward account by its unique identifier.
    /// </summary>
    /// <param name="id">The unique GUID of the loyalty account to retrieve.</param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The loyalty reward entity if found; otherwise, a 404 Not Found response.
    /// </returns>
    /// <remarks>
    /// Retrieves complete loyalty account details including points balance, tier, and lifetime totals.
    /// Excludes soft-deleted accounts.
    /// </remarks>
    /// <response code="200">Returns the loyalty reward entity.</response>
    /// <response code="400">Invalid loyalty account ID format provided.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="404">Loyalty account not found or has been deleted.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(LoyaltyRewardEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LoyaltyRewardEntity>> GetRewardById(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid loyalty reward GUID provided");
            return BadRequest(new { Message = ErrorMessages.InvalidId });
        }

        try
        {
            var reward = await _context
                .LoyaltyRewards.AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted, cancellationToken);

            if (reward == null)
            {
                _logger.LogInformation("Loyalty reward not found: {RewardId}", id);
                return NotFound(new { Message = ErrorMessages.LoyaltyRewardNotFound });
            }

            return Ok(reward);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving loyalty reward: {RewardId}", id);
            return StatusCode(500, ErrorMessages.ProcessingRequestError);
        }
    }

    /// <summary>
    /// Retrieves a loyalty reward account by the associated customer identifier.
    /// </summary>
    /// <param name="customerId">The unique GUID of the customer to look up.</param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The loyalty reward entity if found; otherwise, a 404 Not Found response.
    /// </returns>
    /// <remarks>
    /// Each customer has at most one loyalty account. Useful for customer-facing
    /// balance displays and program enrollment verification.
    /// </remarks>
    /// <response code="200">Returns the loyalty reward entity.</response>
    /// <response code="400">Invalid customer ID format provided.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="404">Loyalty account not found or has been deleted.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpGet("customer/{customerId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(LoyaltyRewardEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LoyaltyRewardEntity>> GetRewardByCustomerId(
        Guid customerId,
        CancellationToken cancellationToken = default
    )
    {
        if (customerId == Guid.Empty)
        {
            _logger.LogWarning("Invalid customer GUID provided");
            return BadRequest(new { Message = ErrorMessages.InvalidCustomerId });
        }

        try
        {
            var reward = await _context
                .LoyaltyRewards.AsNoTracking()
                .FirstOrDefaultAsync(
                    l => l.CustomerId == customerId && !l.IsDeleted,
                    cancellationToken
                );

            if (reward == null)
            {
                _logger.LogInformation(
                    "Loyalty reward not found for customer: {CustomerId}",
                    customerId
                );
                return NotFound(new { Message = ErrorMessages.LoyaltyRewardNotFound });
            }

            return Ok(reward);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving loyalty reward for customer: {CustomerId}",
                customerId
            );
            return StatusCode(500, ErrorMessages.ProcessingRequestError);
        }
    }

    /// <summary>
    /// Creates a new loyalty reward account for a customer.
    /// </summary>
    /// <param name="reward">
    /// The loyalty reward entity containing the customer ID and initial points.
    /// </param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The newly created loyalty reward entity with server-generated ID and timestamps.
    /// </returns>
    /// <remarks>
    /// <para><b>Validation Rules:</b></para>
    /// <list type="bullet">
    /// <item><description>Customer ID must be provided</description></item>
    /// <item><description>Point balance must be non-negative</description></item>
    /// <item><description>Each customer can have only one loyalty account</description></item>
    /// </list>
    /// <para><b>Automatic Field Assignment:</b></para>
    /// <list type="bullet">
    /// <item><description>Tier: Derived from the point balance using <see cref="RewardsProgramPolicy.GetTierFromPoints"/></description></item>
    /// <item><description>Id: Auto-generated GUID</description></item>
    /// <item><description>CreatedAt/UpdatedAt: Set to current UTC time</description></item>
    /// </list>
    /// </remarks>
    /// <response code="201">Loyalty account created successfully. Returns the created entity with Location header.</response>
    /// <response code="400">Invalid input data or validation failure.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="403">Insufficient permissions - Admin, Manager, or Developer role required.</response>
    /// <response code="409">A loyalty account already exists for this customer.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Developer")]
    [ProducesResponseType(typeof(LoyaltyRewardEntity), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LoyaltyRewardEntity>> CreateReward(
        [FromBody] LoyaltyRewardEntity reward,
        CancellationToken cancellationToken = default
    )
    {
        if (reward == null)
        {
            _logger.LogWarning("Null loyalty reward data received");
            return BadRequest(new { Message = ErrorMessages.LoyaltyRewardDataRequired });
        }

        if (reward.CustomerId == Guid.Empty)
        {
            _logger.LogWarning("Invalid customer ID provided for loyalty reward");
            return BadRequest(new { Message = ErrorMessages.InvalidCustomerId });
        }

        if (!RewardsProgramPolicy.IsValidPointBalance(reward.PointBalance))
        {
            _logger.LogWarning("Invalid point balance provided");
            return BadRequest(new { Message = ErrorMessages.InvalidPointBalance });
        }

        try
        {
            var existingReward = await _context.LoyaltyRewards.AnyAsync(
                l => l.CustomerId == reward.CustomerId && !l.IsDeleted,
                cancellationToken
            );

            if (existingReward)
            {
                _logger.LogWarning(
                    "Duplicate loyalty account attempt for customer: {CustomerId}",
                    reward.CustomerId
                );
                return Conflict(
                    new { Message = "A loyalty account already exists for this customer" }
                );
            }

            var pointBalance = reward.PointBalance;
            var newReward = new LoyaltyRewardEntity
            {
                Id = Guid.NewGuid(),
                CustomerId = reward.CustomerId,
                PointBalance = pointBalance,
                TotalPointsEarned = pointBalance,
                TotalPointsRedeemed = 0,
                Tier = RewardsProgramPolicy.GetTierFromPoints(pointBalance),
                LastEarnedAt = pointBalance > 0 ? DateTime.UtcNow : null,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            _context.LoyaltyRewards.Add(newReward);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Loyalty reward created: {RewardId}, Customer: {CustomerId}",
                newReward.Id,
                newReward.CustomerId
            );

            return CreatedAtAction(nameof(GetRewardById), new { id = newReward.Id }, newReward);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error creating loyalty reward for customer: {CustomerId}",
                reward.CustomerId
            );
            return StatusCode(500, new { Message = ErrorMessages.ProcessingRequestError });
        }
    }

    /// <summary>
    /// Updates the point balance and recomputes the loyalty tier for an account.
    /// </summary>
    /// <param name="id">The unique GUID of the loyalty account to update.</param>
    /// <param name="reward">
    /// The loyalty reward entity with updated values. The Id must match the route parameter.
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
    /// <item><description>PointBalance (must be non-negative)</description></item>
    /// </list>
    /// <para><b>Automatic Field Assignment:</b></para>
    /// <list type="bullet">
    /// <item><description>Tier: Recalculated from the point balance</description></item>
    /// <item><description>UpdatedAt: Set to current UTC time</description></item>
    /// </list>
    /// <para><b>Protected Fields (Not Updated):</b></para>
    /// <list type="bullet">
    /// <item><description>Id, CustomerId: Cannot be changed</description></item>
    /// <item><description>TotalPointsEarned, TotalPointsRedeemed: Updated through earn/redeem operations only</description></item>
    /// </list>
    /// </remarks>
    /// <response code="204">Loyalty account updated successfully. No content returned.</response>
    /// <response code="400">Invalid input data, ID mismatch, or validation failure.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="403">Insufficient permissions - Admin, Manager, or Developer role required.</response>
    /// <response code="404">Loyalty account not found or has been deleted.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager,Developer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateReward(
        Guid id,
        [FromBody] LoyaltyRewardEntity reward,
        CancellationToken cancellationToken = default
    )
    {
        if (reward == null)
        {
            _logger.LogWarning("Null loyalty reward data received for update");
            return BadRequest(new { Message = ErrorMessages.LoyaltyRewardDataRequired });
        }

        if (id == Guid.Empty || id != reward.Id)
        {
            _logger.LogWarning(
                "ID mismatch in loyalty reward update. Route: {RouteId}, Body: {BodyId}",
                id,
                reward.Id
            );
            return BadRequest(new { Message = ErrorMessages.IdMismatch });
        }

        if (!RewardsProgramPolicy.IsValidPointBalance(reward.PointBalance))
        {
            _logger.LogWarning("Invalid point balance provided for update");
            return BadRequest(new { Message = ErrorMessages.InvalidPointBalance });
        }

        try
        {
            var existingReward = await _context.LoyaltyRewards.FirstOrDefaultAsync(
                l => l.Id == id && !l.IsDeleted,
                cancellationToken
            );

            if (existingReward == null)
            {
                _logger.LogWarning("Loyalty reward not found for update: {RewardId}", id);
                return NotFound(new { Message = ErrorMessages.LoyaltyRewardNotFound });
            }

            existingReward.PointBalance = reward.PointBalance;
            existingReward.Tier = RewardsProgramPolicy.GetTierFromPoints(reward.PointBalance);
            existingReward.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Loyalty reward updated: {RewardId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating loyalty reward: {RewardId}", id);
            return StatusCode(500, new { Message = ErrorMessages.ProcessingRequestError });
        }
    }

    /// <summary>
    /// Earns points on a loyalty account based on purchases and reviews.
    /// </summary>
    /// <param name="id">The unique GUID of the loyalty account to credit.</param>
    /// <param name="request">The earn request containing the spend amount and review details.</param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The updated loyalty reward entity with the new point balance.
    /// </returns>
    /// <remarks>
    /// <para><b>Points Calculation:</b></para>
    /// <list type="bullet">
    /// <item><description>Spend points: Calculated via <see cref="RewardsProgramPolicy.CalculatePointsForSpend"/></description></item>
    /// <item><description>Review points: Calculated via <see cref="RewardsProgramPolicy.CalculatePointsForReview"/></description></item>
    /// <item><description>Tier multiplier: Applied via <see cref="RewardsProgramPolicy.GetTierEarningMultiplier"/></description></item>
    /// </list>
    /// <para>
    /// The account tier is recalculated after earning, and LastEarnedAt is refreshed to the current UTC time.
    /// </para>
    /// </remarks>
    /// <response code="200">Returns the updated loyalty reward entity.</response>
    /// <response code="400">Invalid input data or validation failure.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="403">Insufficient permissions - Admin, Manager, or Developer role required.</response>
    /// <response code="404">Loyalty account not found or has been deleted.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpPost("{id:guid}/earn")]
    [Authorize(Roles = "Admin,Manager,Developer")]
    [ProducesResponseType(typeof(LoyaltyRewardEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LoyaltyRewardEntity>> EarnPoints(
        Guid id,
        [FromBody] EarnPointsRequestDto request,
        CancellationToken cancellationToken = default
    )
    {
        if (request == null)
        {
            _logger.LogWarning("Null earn points request received");
            return BadRequest(new { Message = ErrorMessages.LoyaltyRewardDataRequired });
        }

        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid loyalty reward GUID provided for earning");
            return BadRequest(new { Message = ErrorMessages.InvalidId });
        }

        try
        {
            var reward = await _context.LoyaltyRewards.FirstOrDefaultAsync(
                l => l.Id == id && !l.IsDeleted,
                cancellationToken
            );

            if (reward == null)
            {
                _logger.LogWarning("Loyalty reward not found for earning: {RewardId}", id);
                return NotFound(new { Message = ErrorMessages.LoyaltyRewardNotFound });
            }

            var spendPoints = RewardsProgramPolicy.CalculatePointsForSpend(request.AmountSpent);
            var reviewPoints = RewardsProgramPolicy.CalculatePointsForReview(
                request.ReviewCount,
                request.AverageRating
            );
            var multiplier = RewardsProgramPolicy.GetTierEarningMultiplier(reward.Tier);

            var earnedPoints = (int)Math.Floor((spendPoints + reviewPoints) * multiplier);
            reward.PointBalance += earnedPoints;
            reward.TotalPointsEarned += earnedPoints;
            reward.LastEarnedAt = DateTime.UtcNow;
            reward.Tier = RewardsProgramPolicy.GetTierFromPoints(reward.PointBalance);
            reward.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Points earned on loyalty reward: {RewardId}, Points: {Points}, User: {UserId}",
                id,
                earnedPoints,
                User.Identity?.Name ?? "Unknown"
            );
            return Ok(reward);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error earning points on loyalty reward: {RewardId}", id);
            return StatusCode(500, new { Message = ErrorMessages.ProcessingRequestError });
        }
    }

    /// <summary>
    /// Redeems points from a loyalty account for a reward.
    /// </summary>
    /// <param name="id">The unique GUID of the loyalty account to debit.</param>
    /// <param name="request">The redeem request containing the reward cost in points.</param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The updated loyalty reward entity with the new point balance.
    /// </returns>
    /// <remarks>
    /// <para><b>Validation Rules:</b></para>
    /// <list type="bullet">
    /// <item><description>Reward cost must be greater than zero</description></item>
    /// <item><description>Account must have enough points via <see cref="RewardsProgramPolicy.HasEnoughPointsForReward"/></description></item>
    /// <item><description>Points must not be expired via <see cref="RewardsProgramPolicy.ArePointsExpired"/></description></item>
    /// </list>
    /// <para>
    /// The account tier is recalculated after redemption and TotalPointsRedeemed is incremented.
    /// </para>
    /// </remarks>
    /// <response code="200">Returns the updated loyalty reward entity.</response>
    /// <response code="400">Insufficient points, expired points, or invalid reward cost.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="403">Insufficient permissions - Admin, Manager, or Developer role required.</response>
    /// <response code="404">Loyalty account not found or has been deleted.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpPost("{id:guid}/redeem")]
    [Authorize(Roles = "Admin,Manager,Developer")]
    [ProducesResponseType(typeof(LoyaltyRewardEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LoyaltyRewardEntity>> RedeemPoints(
        Guid id,
        [FromBody] RedeemPointsRequestDto request,
        CancellationToken cancellationToken = default
    )
    {
        if (request == null)
        {
            _logger.LogWarning("Null redeem points request received");
            return BadRequest(new { Message = ErrorMessages.LoyaltyRewardDataRequired });
        }

        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid loyalty reward GUID provided for redemption");
            return BadRequest(new { Message = ErrorMessages.InvalidId });
        }

        try
        {
            var reward = await _context.LoyaltyRewards.FirstOrDefaultAsync(
                l => l.Id == id && !l.IsDeleted,
                cancellationToken
            );

            if (reward == null)
            {
                _logger.LogWarning("Loyalty reward not found for redemption: {RewardId}", id);
                return NotFound(new { Message = ErrorMessages.LoyaltyRewardNotFound });
            }

            if (RewardsProgramPolicy.ArePointsExpired(reward.LastEarnedAt))
            {
                _logger.LogWarning("Redemption attempted with expired points: {RewardId}", id);
                return BadRequest(new { Message = ErrorMessages.PointsExpired });
            }

            if (
                !RewardsProgramPolicy.HasEnoughPointsForReward(
                    reward.PointBalance,
                    request.RewardCost
                )
            )
            {
                _logger.LogWarning("Insufficient points for redemption: {RewardId}", id);
                return BadRequest(new { Message = ErrorMessages.InsufficientPoints });
            }

            reward.PointBalance -= request.RewardCost;
            reward.TotalPointsRedeemed += request.RewardCost;
            reward.Tier = RewardsProgramPolicy.GetTierFromPoints(reward.PointBalance);
            reward.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Points redeemed on loyalty reward: {RewardId}, Points: {Points}, User: {UserId}",
                id,
                request.RewardCost,
                User.Identity?.Name ?? "Unknown"
            );
            return Ok(reward);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error redeeming points on loyalty reward: {RewardId}", id);
            return StatusCode(500, new { Message = ErrorMessages.ProcessingRequestError });
        }
    }

    /// <summary>
    /// Calculates the monetary value of the current point balance on a loyalty account.
    /// </summary>
    /// <param name="id">The unique GUID of the loyalty account to value.</param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// An object containing the current point balance, the tier, and the calculated monetary value.
    /// </returns>
    /// <remarks>
    /// The value is calculated via <see cref="RewardsProgramPolicy.CalculateRewardValue"/>,
    /// which converts points to dollars using the program's points-per-dollar rate.
    /// </remarks>
    /// <response code="200">Returns the reward value calculation.</response>
    /// <response code="400">Invalid loyalty account ID format provided.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="404">Loyalty account not found or has been deleted.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpGet("{id:guid}/value")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRewardValue(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid loyalty reward GUID provided for value lookup");
            return BadRequest(new { Message = ErrorMessages.InvalidId });
        }

        try
        {
            var reward = await _context
                .LoyaltyRewards.AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted, cancellationToken);

            if (reward == null)
            {
                _logger.LogInformation("Loyalty reward not found for value lookup: {RewardId}", id);
                return NotFound(new { Message = ErrorMessages.LoyaltyRewardNotFound });
            }

            return Ok(
                new
                {
                    reward.PointBalance,
                    reward.Tier,
                    Value = RewardsProgramPolicy.CalculateRewardValue(reward.PointBalance),
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating reward value: {RewardId}", id);
            return StatusCode(500, ErrorMessages.ProcessingRequestError);
        }
    }

    /// <summary>
    /// Soft deletes a loyalty reward account, marking it as deleted while preserving the record for audit purposes.
    /// </summary>
    /// <param name="id">The unique GUID of the loyalty account to delete.</param>
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
    /// <item><description>Preserves all point balance history for audit trails</description></item>
    /// </list>
    /// <para>
    /// A deleted account can no longer earn or redeem points and no longer appears in lookups.
    /// </para>
    /// </remarks>
    /// <response code="204">Loyalty account deleted successfully. No content returned.</response>
    /// <response code="400">Invalid loyalty account ID format provided.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="403">Insufficient permissions - Admin, Manager, or Developer role required.</response>
    /// <response code="404">Loyalty account not found or already deleted.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Manager,Developer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteReward(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid loyalty reward GUID provided for deletion");
            return BadRequest(new { Message = ErrorMessages.InvalidId });
        }

        try
        {
            var reward = await _context.LoyaltyRewards.FirstOrDefaultAsync(
                l => l.Id == id && !l.IsDeleted,
                cancellationToken
            );

            if (reward == null)
            {
                _logger.LogWarning("Loyalty reward not found for deletion: {RewardId}", id);
                return NotFound(new { Message = ErrorMessages.LoyaltyRewardNotFound });
            }

            reward.IsDeleted = true;
            reward.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogWarning(
                "Loyalty reward deleted: {RewardId}, Customer: {CustomerId}",
                id,
                reward.CustomerId
            );
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting loyalty reward: {RewardId}", id);
            return StatusCode(500, new { Message = ErrorMessages.ProcessingRequestError });
        }
    }
}
