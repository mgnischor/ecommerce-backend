using System.Security.Claims;
using Comex.API.Constants;
using Comex.API.DTOs;
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
/// Provides HTTP endpoints for managing gift cards.
/// Supports creating, retrieving, updating, redeeming, reloading, and deleting gift cards with role-based access control.
/// </summary>
/// <remarks>
/// This controller handles all gift card operations including:
/// <list type="bullet">
/// <item><description>Creating gift cards with validated card numbers and balances</description></item>
/// <item><description>Redeeming amounts from active, unexpired gift cards</description></item>
/// <item><description>Reloading gift card balances for reloadable cards</description></item>
/// <item><description>Soft-delete support for maintaining historical records</description></item>
/// </list>
/// All business rules are enforced via <see cref="GiftCardPolicy"/>.
/// All responses are in JSON format and follow RESTful conventions.
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="GiftCardController"/> class.
/// </remarks>
/// <param name="context">The database context for data access operations.</param>
/// <param name="logger">The logger for recording operational events and errors.</param>
/// <exception cref="ArgumentNullException">
/// Thrown when <paramref name="context"/> or <paramref name="logger"/> is null.
/// </exception>
[Tags("Gift Cards")]
[ApiController]
[Route("api/v1/giftcards")]
[Produces("application/json")]
public sealed class GiftCardController(
    PostgresqlContext context,
    LoggingService<GiftCardController> logger
) : ControllerBase
{
    /// <summary>
    /// The database context for accessing gift card data.
    /// </summary>
    private readonly PostgresqlContext _context =
        context ?? throw new ArgumentNullException(nameof(context));

    /// <summary>
    /// Logger instance for recording controller operations and errors.
    /// </summary>
    private readonly ILoggingService _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Number of months a gift card remains valid before expiring when no explicit expiration is set.
    /// </summary>
    private const int GiftCardValidityMonths = 24;

    /// <summary>
    /// Retrieves the unique identifier of the currently authenticated user.
    /// </summary>
    /// <returns>
    /// The user ID from the JWT token claims, or null if the user is not authenticated.
    /// </returns>
    private string? GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    /// <summary>
    /// Retrieves a specific gift card by its unique identifier.
    /// </summary>
    /// <param name="id">The unique GUID of the gift card to retrieve.</param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The gift card entity if found; otherwise, a 404 Not Found response.
    /// </returns>
    /// <remarks>
    /// Retrieves complete gift card details including balance, validity period, and status.
    /// Excludes soft-deleted gift cards.
    /// </remarks>
    /// <response code="200">Returns the gift card entity.</response>
    /// <response code="400">Invalid gift card ID format provided.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="404">Gift card not found or has been deleted.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(GiftCardEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GiftCardEntity>> GetGiftCardById(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid gift card GUID provided");
            return BadRequest(new { Message = ErrorMessages.InvalidId });
        }

        try
        {
            var giftCard = await _context
                .GiftCards.AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == id && !g.IsDeleted, cancellationToken);

            if (giftCard == null)
            {
                _logger.LogInformation("Gift card not found: {GiftCardId}", id);
                return NotFound(new { Message = ErrorMessages.GiftCardNotFound });
            }

            return Ok(giftCard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving gift card: {GiftCardId}", id);
            return StatusCode(500, ErrorMessages.ProcessingRequestError);
        }
    }

    /// <summary>
    /// Retrieves a gift card by its card number.
    /// </summary>
    /// <param name="cardNumber">
    /// The gift card number to look up. Must be between 13 and 19 digits (digits and dashes allowed).
    /// </param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The gift card entity if found; otherwise, a 404 Not Found response.
    /// </returns>
    /// <remarks>
    /// Useful for gift card validation during checkout and for customer service lookups.
    /// Input is validated against <see cref="GiftCardPolicy.IsValidCardNumber"/> before querying.
    /// </remarks>
    /// <response code="200">Returns the gift card entity.</response>
    /// <response code="400">Invalid card number format provided.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="404">Gift card not found or has been deleted.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpGet("number/{cardNumber}")]
    [Authorize]
    [ProducesResponseType(typeof(GiftCardEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GiftCardEntity>> GetGiftCardByNumber(
        string cardNumber,
        CancellationToken cancellationToken = default
    )
    {
        if (!GiftCardPolicy.IsValidCardNumber(cardNumber))
        {
            _logger.LogWarning("Invalid gift card number format provided");
            return BadRequest(new { Message = ErrorMessages.InvalidGiftCardNumber });
        }

        try
        {
            var giftCard = await _context
                .GiftCards.AsNoTracking()
                .FirstOrDefaultAsync(
                    g => g.CardNumber == cardNumber && !g.IsDeleted,
                    cancellationToken
                );

            if (giftCard == null)
            {
                _logger.LogInformation("Gift card number not found: {CardNumber}", cardNumber);
                return NotFound(new { Message = ErrorMessages.GiftCardNotFound });
            }

            return Ok(giftCard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving gift card by number");
            return StatusCode(500, ErrorMessages.ProcessingRequestError);
        }
    }

    /// <summary>
    /// Creates a new gift card with the specified number and balance.
    /// </summary>
    /// <param name="giftCard">
    /// The gift card entity containing the card number, initial balance, and status flags.
    /// </param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The newly created gift card entity with server-generated ID and timestamps.
    /// </returns>
    /// <remarks>
    /// <para><b>Validation Rules:</b></para>
    /// <list type="bullet">
    /// <item><description>Card number must be 13-19 digits (digits and dashes only)</description></item>
    /// <item><description>Balance must be between 0.01 and 5000</description></item>
    /// <item><description>Card number must be unique across all non-deleted gift cards</description></item>
    /// </list>
    /// <para><b>Automatic Field Assignment:</b></para>
    /// <list type="bullet">
    /// <item><description>ExpiresAt: Set to IssuedAt + 24 months when not explicitly provided</description></item>
    /// <item><description>Id: Auto-generated GUID</description></item>
    /// <item><description>CreatedAt/UpdatedAt: Set to current UTC time</description></item>
    /// <item><description>CreatedBy: Set from authenticated user's ID</description></item>
    /// </list>
    /// </remarks>
    /// <response code="201">Gift card created successfully. Returns the created gift card with Location header.</response>
    /// <response code="400">Invalid input data or validation failure.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="403">Insufficient permissions - Admin, Manager, or Developer role required.</response>
    /// <response code="409">Gift card number already exists.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Developer")]
    [ProducesResponseType(typeof(GiftCardEntity), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GiftCardEntity>> CreateGiftCard(
        [FromBody] GiftCardEntity giftCard,
        CancellationToken cancellationToken = default
    )
    {
        if (giftCard == null)
        {
            _logger.LogWarning("Null gift card data received");
            return BadRequest(new { Message = ErrorMessages.GiftCardDataRequired });
        }

        if (!GiftCardPolicy.IsValidCardNumber(giftCard.CardNumber))
        {
            _logger.LogWarning("Invalid gift card number provided");
            return BadRequest(new { Message = ErrorMessages.InvalidGiftCardNumber });
        }

        if (!GiftCardPolicy.IsValidBalance(giftCard.Balance))
        {
            _logger.LogWarning("Invalid gift card balance provided");
            return BadRequest(new { Message = ErrorMessages.GiftCardBalanceOutOfRange });
        }

        try
        {
            var duplicateNumber = await _context.GiftCards.AnyAsync(
                g => g.CardNumber == giftCard.CardNumber && !g.IsDeleted,
                cancellationToken
            );

            if (duplicateNumber)
            {
                _logger.LogWarning(
                    "Duplicate gift card number attempt: {CardNumber}",
                    giftCard.CardNumber
                );
                return Conflict(new { Message = ErrorMessages.GiftCardNumberAlreadyExists });
            }

            var issuedAt = giftCard.IssuedAt == default ? DateTime.UtcNow : giftCard.IssuedAt;
            var newGiftCard = new GiftCardEntity
            {
                Id = Guid.NewGuid(),
                CardNumber = giftCard.CardNumber,
                Balance = giftCard.Balance,
                IsActive = giftCard.IsActive,
                IsRevoked = giftCard.IsRevoked,
                AllowsReload = giftCard.AllowsReload,
                IssuedAt = issuedAt,
                ExpiresAt = giftCard.ExpiresAt ?? issuedAt.AddMonths(GiftCardValidityMonths),
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = Guid.TryParse(GetCurrentUserId(), out var userId) ? userId : Guid.Empty,
            };

            _context.GiftCards.Add(newGiftCard);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Gift card created: {GiftCardId}, CardNumber: {CardNumber}, User: {UserId}",
                newGiftCard.Id,
                newGiftCard.CardNumber,
                GetCurrentUserId() ?? "Unknown"
            );

            return CreatedAtAction(
                nameof(GetGiftCardById),
                new { id = newGiftCard.Id },
                newGiftCard
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error creating gift card with number: {CardNumber}",
                giftCard.CardNumber
            );
            return StatusCode(500, new { Message = ErrorMessages.ProcessingRequestError });
        }
    }

    /// <summary>
    /// Updates an existing gift card's details, including status flags, balance, and validity.
    /// </summary>
    /// <param name="id">The unique GUID of the gift card to update.</param>
    /// <param name="giftCard">
    /// The gift card entity with updated values. The Id must match the route parameter.
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
    /// <item><description>Balance (must remain within 0.01 and 5000)</description></item>
    /// <item><description>IsActive, IsRevoked, AllowsReload</description></item>
    /// <item><description>ExpiresAt</description></item>
    /// </list>
    /// <para><b>Protected Fields (Not Updated):</b></para>
    /// <list type="bullet">
    /// <item><description>Id: Cannot be changed</description></item>
    /// <item><description>CardNumber: Cannot be changed</description></item>
    /// <item><description>CreatedAt, CreatedBy: Audit fields remain unchanged</description></item>
    /// </list>
    /// </remarks>
    /// <response code="204">Gift card updated successfully. No content returned.</response>
    /// <response code="400">Invalid input data, ID mismatch, or validation failure.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="403">Insufficient permissions - Admin, Manager, or Developer role required.</response>
    /// <response code="404">Gift card not found or has been deleted.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager,Developer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateGiftCard(
        Guid id,
        [FromBody] GiftCardEntity giftCard,
        CancellationToken cancellationToken = default
    )
    {
        if (giftCard == null)
        {
            _logger.LogWarning("Null gift card data received for update");
            return BadRequest(new { Message = ErrorMessages.GiftCardDataRequired });
        }

        if (id == Guid.Empty || id != giftCard.Id)
        {
            _logger.LogWarning(
                "ID mismatch in gift card update. Route: {RouteId}, Body: {BodyId}",
                id,
                giftCard.Id
            );
            return BadRequest(new { Message = ErrorMessages.IdMismatch });
        }

        if (!GiftCardPolicy.IsValidBalance(giftCard.Balance))
        {
            _logger.LogWarning("Invalid gift card balance provided for update");
            return BadRequest(new { Message = ErrorMessages.GiftCardBalanceOutOfRange });
        }

        try
        {
            var existingGiftCard = await _context.GiftCards.FirstOrDefaultAsync(
                g => g.Id == id && !g.IsDeleted,
                cancellationToken
            );

            if (existingGiftCard == null)
            {
                _logger.LogWarning("Gift card not found for update: {GiftCardId}", id);
                return NotFound(new { Message = ErrorMessages.GiftCardNotFound });
            }

            existingGiftCard.Balance = giftCard.Balance;
            existingGiftCard.IsActive = giftCard.IsActive;
            existingGiftCard.IsRevoked = giftCard.IsRevoked;
            existingGiftCard.AllowsReload = giftCard.AllowsReload;
            existingGiftCard.ExpiresAt = giftCard.ExpiresAt;
            existingGiftCard.UpdatedAt = DateTime.UtcNow;
            existingGiftCard.UpdatedBy = Guid.TryParse(GetCurrentUserId(), out var userId)
                ? userId
                : Guid.Empty;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Gift card updated: {GiftCardId}, User: {UserId}",
                id,
                GetCurrentUserId() ?? "Unknown"
            );
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating gift card: {GiftCardId}", id);
            return StatusCode(500, new { Message = ErrorMessages.ProcessingRequestError });
        }
    }

    /// <summary>
    /// Redeems an amount from a gift card, deducting from the available balance.
    /// </summary>
    /// <param name="id">The unique GUID of the gift card to redeem from.</param>
    /// <param name="request">The redemption request containing the amount to redeem.</param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The updated gift card entity with the new balance.
    /// </returns>
    /// <remarks>
    /// <para><b>Validation Rules:</b></para>
    /// <list type="bullet">
    /// <item><description>Gift card must be active and not revoked</description></item>
    /// <item><description>Gift card must be within its validity period</description></item>
    /// <item><description>Redeem amount must be positive and not exceed the available balance</description></item>
    /// </list>
    /// <para>
    /// The balance is reduced using <see cref="GiftCardPolicy.CalculateRemainingBalance"/>,
    /// which never allows the balance to go below zero.
    /// </para>
    /// </remarks>
    /// <response code="200">Returns the updated gift card entity.</response>
    /// <response code="400">Invalid gift card, expired card, or invalid redeem amount.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="403">Insufficient permissions - Admin, Manager, or Developer role required.</response>
    /// <response code="404">Gift card not found or has been deleted.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpPost("{id:guid}/redeem")]
    [Authorize(Roles = "Admin,Manager,Developer")]
    [ProducesResponseType(typeof(GiftCardEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GiftCardEntity>> RedeemGiftCard(
        Guid id,
        [FromBody] GiftCardRedeemRequestDto request,
        CancellationToken cancellationToken = default
    )
    {
        if (request == null)
        {
            _logger.LogWarning("Null redeem request received");
            return BadRequest(new { Message = ErrorMessages.GiftCardDataRequired });
        }

        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid gift card GUID provided for redemption");
            return BadRequest(new { Message = ErrorMessages.InvalidId });
        }

        try
        {
            var giftCard = await _context.GiftCards.FirstOrDefaultAsync(
                g => g.Id == id && !g.IsDeleted,
                cancellationToken
            );

            if (giftCard == null)
            {
                _logger.LogWarning("Gift card not found for redemption: {GiftCardId}", id);
                return NotFound(new { Message = ErrorMessages.GiftCardNotFound });
            }

            if (!GiftCardPolicy.IsCardActive(giftCard.IsActive, giftCard.IsRevoked))
            {
                _logger.LogWarning("Redemption attempted on inactive gift card: {GiftCardId}", id);
                return BadRequest(new { Message = ErrorMessages.GiftCardNotActive });
            }

            if (GiftCardPolicy.IsCardExpired(giftCard.ExpiresAt))
            {
                _logger.LogWarning("Redemption attempted on expired gift card: {GiftCardId}", id);
                return BadRequest(new { Message = ErrorMessages.GiftCardExpired });
            }

            if (!GiftCardPolicy.IsWithinValidityPeriod(giftCard.IssuedAt, giftCard.ExpiresAt))
            {
                _logger.LogWarning(
                    "Redemption attempted outside validity period: {GiftCardId}",
                    id
                );
                return BadRequest(new { Message = ErrorMessages.GiftCardExpired });
            }

            if (!GiftCardPolicy.CanRedeemAmount(giftCard.Balance, request.RedeemAmount))
            {
                _logger.LogWarning(
                    "Insufficient balance for redemption on gift card: {GiftCardId}",
                    id
                );
                return BadRequest(new { Message = ErrorMessages.InsufficientGiftCardBalance });
            }

            giftCard.Balance = GiftCardPolicy.CalculateRemainingBalance(
                giftCard.Balance,
                request.RedeemAmount
            );
            giftCard.UpdatedAt = DateTime.UtcNow;
            giftCard.UpdatedBy = Guid.TryParse(GetCurrentUserId(), out var userId)
                ? userId
                : Guid.Empty;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Gift card redeemed: {GiftCardId}, Amount: {Amount}, User: {UserId}",
                id,
                request.RedeemAmount,
                GetCurrentUserId() ?? "Unknown"
            );
            return Ok(giftCard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error redeeming gift card: {GiftCardId}", id);
            return StatusCode(500, new { Message = ErrorMessages.ProcessingRequestError });
        }
    }

    /// <summary>
    /// Reloads a gift card balance to a new value, subject to reload eligibility.
    /// </summary>
    /// <param name="id">The unique GUID of the gift card to reload.</param>
    /// <param name="request">The reload request containing the new total balance.</param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The updated gift card entity with the reloaded balance.
    /// </returns>
    /// <remarks>
    /// <para><b>Validation Rules:</b></para>
    /// <list type="bullet">
    /// <item><description>Gift card must allow reloads</description></item>
    /// <item><description>Current balance must be below the maximum limit</description></item>
    /// <item><description>New balance must be between 0.01 and 5000</description></item>
    /// </list>
    /// <para>
    /// Reload eligibility is enforced via <see cref="GiftCardPolicy.CanReloadCard"/>.
    /// </para>
    /// </remarks>
    /// <response code="200">Returns the updated gift card entity.</response>
    /// <response code="400">Gift card not reloadable or invalid reload balance.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="403">Insufficient permissions - Admin, Manager, or Developer role required.</response>
    /// <response code="404">Gift card not found or has been deleted.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpPost("{id:guid}/reload")]
    [Authorize(Roles = "Admin,Manager,Developer")]
    [ProducesResponseType(typeof(GiftCardEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GiftCardEntity>> ReloadGiftCard(
        Guid id,
        [FromBody] GiftCardReloadRequestDto request,
        CancellationToken cancellationToken = default
    )
    {
        if (request == null)
        {
            _logger.LogWarning("Null reload request received");
            return BadRequest(new { Message = ErrorMessages.GiftCardDataRequired });
        }

        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid gift card GUID provided for reload");
            return BadRequest(new { Message = ErrorMessages.InvalidId });
        }

        if (!GiftCardPolicy.IsValidBalance(request.NewBalance))
        {
            _logger.LogWarning("Invalid reload balance provided");
            return BadRequest(new { Message = ErrorMessages.GiftCardBalanceOutOfRange });
        }

        try
        {
            var giftCard = await _context.GiftCards.FirstOrDefaultAsync(
                g => g.Id == id && !g.IsDeleted,
                cancellationToken
            );

            if (giftCard == null)
            {
                _logger.LogWarning("Gift card not found for reload: {GiftCardId}", id);
                return NotFound(new { Message = ErrorMessages.GiftCardNotFound });
            }

            if (!GiftCardPolicy.CanReloadCard(giftCard.AllowsReload, giftCard.Balance))
            {
                _logger.LogWarning("Reload not permitted on gift card: {GiftCardId}", id);
                return BadRequest(new { Message = ErrorMessages.GiftCardNotReloadable });
            }

            giftCard.Balance = request.NewBalance;
            giftCard.UpdatedAt = DateTime.UtcNow;
            giftCard.UpdatedBy = Guid.TryParse(GetCurrentUserId(), out var userId)
                ? userId
                : Guid.Empty;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Gift card reloaded: {GiftCardId}, NewBalance: {Balance}, User: {UserId}",
                id,
                request.NewBalance,
                GetCurrentUserId() ?? "Unknown"
            );
            return Ok(giftCard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reloading gift card: {GiftCardId}", id);
            return StatusCode(500, new { Message = ErrorMessages.ProcessingRequestError });
        }
    }

    /// <summary>
    /// Soft deletes a gift card, marking it as deleted while preserving the record for audit purposes.
    /// </summary>
    /// <param name="id">The unique GUID of the gift card to delete.</param>
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
    /// <item><description>Sets UpdatedBy to current user's ID</description></item>
    /// <item><description>Preserves all other gift card data</description></item>
    /// </list>
    /// <para>
    /// A deleted gift card can no longer be redeemed or reloaded and no longer appears in lookups.
    /// The record remains in the database for reporting and audit trails.
    /// </para>
    /// </remarks>
    /// <response code="204">Gift card deleted successfully. No content returned.</response>
    /// <response code="400">Invalid gift card ID format provided.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="403">Insufficient permissions - Admin, Manager, or Developer role required.</response>
    /// <response code="404">Gift card not found or already deleted.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Manager,Developer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteGiftCard(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid gift card GUID provided for deletion");
            return BadRequest(new { Message = ErrorMessages.InvalidId });
        }

        try
        {
            var giftCard = await _context.GiftCards.FirstOrDefaultAsync(
                g => g.Id == id && !g.IsDeleted,
                cancellationToken
            );

            if (giftCard == null)
            {
                _logger.LogWarning("Gift card not found for deletion: {GiftCardId}", id);
                return NotFound(new { Message = ErrorMessages.GiftCardNotFound });
            }

            giftCard.IsDeleted = true;
            giftCard.UpdatedAt = DateTime.UtcNow;
            giftCard.UpdatedBy = Guid.TryParse(GetCurrentUserId(), out var userId)
                ? userId
                : Guid.Empty;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogWarning(
                "Gift card deleted: {GiftCardId}, CardNumber: {CardNumber}, User: {UserId}",
                id,
                giftCard.CardNumber,
                GetCurrentUserId() ?? "Unknown"
            );
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting gift card: {GiftCardId}", id);
            return StatusCode(500, new { Message = ErrorMessages.ProcessingRequestError });
        }
    }
}
