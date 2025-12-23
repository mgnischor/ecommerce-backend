using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;

namespace ECommerce.API.Controllers;

/// <summary>
/// Provides HTTP endpoints for managing promotional campaigns and discount offers.
/// Supports creating, retrieving, updating, and deleting promotions with role-based access control.
/// </summary>
/// <remarks>
/// This controller handles all promotion-related operations including:
/// <list type="bullet">
/// <item><description>Retrieving active and featured promotions for customer display</description></item>
/// <item><description>Validating promotion codes during checkout</description></item>
/// <item><description>Administrative functions for promotion lifecycle management (Admin/Manager roles)</description></item>
/// <item><description>Soft-delete support for maintaining historical records</description></item>
/// </list>
/// All responses are in JSON format and follow RESTful conventions.
/// </remarks>
[Tags("Promotions")]
[ApiController]
[Route("api/v1/promotions")]
[Produces("application/json")]
public sealed class PromotionController : ControllerBase
{
    /// <summary>
    /// The database context for accessing promotion data.
    /// </summary>
    private readonly PostgresqlContext _context;

    /// <summary>
    /// Logger instance for recording controller operations and errors.
    /// </summary>
    private readonly ILoggingService _logger;

    /// <summary>
    /// Maximum number of promotions to return in a single query to prevent performance issues.
    /// </summary>
    private const int MaxPromotions = 500;

    /// <summary>
    /// Initializes a new instance of the <see cref="PromotionController"/> class.
    /// </summary>
    /// <param name="context">The database context for data access operations.</param>
    /// <param name="logger">The logger for recording operational events and errors.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="context"/> or <paramref name="logger"/> is null.
    /// </exception>
    public PromotionController(
        PostgresqlContext context,
        LoggingService<PromotionController> logger
    )
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves the unique identifier of the currently authenticated user.
    /// </summary>
    /// <returns>
    /// The user ID from the JWT token claims, or null if the user is not authenticated.
    /// </returns>
    /// <remarks>
    /// Extracts the NameIdentifier claim from the current user's claims principal.
    /// Used for audit trail tracking (CreatedBy, UpdatedBy fields).
    /// </remarks>
    private string? GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    /// <summary>
    /// Determines whether the current user has administrator privileges.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the user is in the Admin role; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Administrators have full access to all promotion management operations including deletion.
    /// </remarks>
    private bool IsAdmin() => User.IsInRole("Admin");

    /// <summary>
    /// Determines whether the current user has manager privileges.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the user is in the Manager role; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Managers can create and update promotions but cannot delete them.
    /// </remarks>
    private bool IsManager() => User.IsInRole("Manager");

    /// <summary>
    /// Retrieves all currently active promotions available for use.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// A collection of active promotion entities ordered by priority and start date.
    /// </returns>
    /// <remarks>
    /// Returns promotions that meet all of the following criteria:
    /// <list type="bullet">
    /// <item><description>IsActive = true</description></item>
    /// <item><description>IsDeleted = false</description></item>
    /// <item><description>StartDate ≤ current UTC time</description></item>
    /// <item><description>EndDate ≥ current UTC time</description></item>
    /// </list>
    /// Results are ordered by Priority (descending) then StartDate (descending).
    /// Limited to a maximum of 500 promotions to ensure performance.
    /// No authentication required - public endpoint for displaying available offers.
    /// </remarks>
    /// <response code="200">Returns the list of active promotions.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PromotionEntity>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
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
    /// Retrieves promotions marked as featured for prominent display on the website.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// A collection of featured promotion entities ordered by priority.
    /// </returns>
    /// <remarks>
    /// Featured promotions are typically displayed on the homepage, banners, or special promotion sections.
    /// Returns promotions that meet all of the following criteria:
    /// <list type="bullet">
    /// <item><description>IsFeatured = true</description></item>
    /// <item><description>IsActive = true</description></item>
    /// <item><description>IsDeleted = false</description></item>
    /// <item><description>StartDate ≤ current UTC time</description></item>
    /// <item><description>EndDate ≥ current UTC time</description></item>
    /// </list>
    /// Limited to 50 promotions to keep featured sections manageable and performant.
    /// No authentication required - public endpoint for marketing display purposes.
    /// </remarks>
    /// <response code="200">Returns the list of featured promotions.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpGet("featured")]
    [ProducesResponseType(typeof(IEnumerable<PromotionEntity>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
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
    /// Retrieves a specific promotion by its unique identifier.
    /// </summary>
    /// <param name="id">The unique GUID of the promotion to retrieve.</param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The promotion entity if found; otherwise, a 404 Not Found response.
    /// </returns>
    /// <remarks>
    /// Retrieves complete promotion details including all configuration fields.
    /// Returns promotions regardless of active status or date validity, but excludes soft-deleted promotions.
    /// Useful for administrative views, editing interfaces, and detailed promotion information pages.
    /// No authentication required - public endpoint for viewing promotion details.
    /// </remarks>
    /// <response code="200">Returns the promotion entity.</response>
    /// <response code="400">Invalid promotion ID format provided.</response>
    /// <response code="404">Promotion not found or has been deleted.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PromotionEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
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
    /// Retrieves a promotion by its promotional code, validating its current availability.
    /// </summary>
    /// <param name="code">
    /// The promotion code to look up (e.g., "SAVE20", "SUMMER2024").
    /// Must be alphanumeric with optional hyphens and underscores, max 50 characters.
    /// </param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The promotion entity if the code is valid and currently active.
    /// </returns>
    /// <remarks>
    /// This endpoint is designed for promotion code validation during checkout.
    /// Returns a promotion only if ALL of the following conditions are met:
    /// <list type="bullet">
    /// <item><description>Code matches exactly (case-sensitive database comparison)</description></item>
    /// <item><description>IsActive = true</description></item>
    /// <item><description>IsDeleted = false</description></item>
    /// <item><description>StartDate ≤ current UTC time</description></item>
    /// <item><description>EndDate ≥ current UTC time</description></item>
    /// </list>
    /// <para>Input Validation:</para>
    /// <list type="bullet">
    /// <item><description>Code cannot be empty or whitespace</description></item>
    /// <item><description>Maximum length: 50 characters</description></item>
    /// <item><description>Allowed characters: A-Z, a-z, 0-9, hyphen (-), underscore (_)</description></item>
    /// </list>
    /// Returns 404 for invalid, expired, or inactive codes to prevent information disclosure.
    /// No authentication required to support guest checkout scenarios.
    /// </remarks>
    /// <response code="200">Returns the valid promotion entity.</response>
    /// <response code="400">Invalid code format or validation failure.</response>
    /// <response code="404">Promotion code not found, inactive, or expired.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpGet("code/{code}")]
    [ProducesResponseType(typeof(PromotionEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
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
    /// Creates a new promotional campaign with the specified discount and validity rules.
    /// </summary>
    /// <param name="promotion">
    /// The promotion entity containing all campaign details including discount type, amounts, and validity period.
    /// </param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The newly created promotion entity with server-generated ID and timestamps.
    /// </returns>
    /// <remarks>
    /// <para><b>Required Fields:</b></para>
    /// <list type="bullet">
    /// <item><description>Name: 1-200 characters</description></item>
    /// <item><description>Code: 1-50 characters, alphanumeric with hyphens/underscores, must be unique</description></item>
    /// <item><description>StartDate: Must be before EndDate</description></item>
    /// <item><description>EndDate: Must be after StartDate</description></item>
    /// </list>
    /// <para><b>Validation Rules:</b></para>
    /// <list type="bullet">
    /// <item><description>Promotion code must be unique across all non-deleted promotions</description></item>
    /// <item><description>DiscountPercentage, if provided, must be 0-100</description></item>
    /// <item><description>StartDate must be before EndDate</description></item>
    /// <item><description>Code format: alphanumeric characters, hyphens, and underscores only</description></item>
    /// </list>
    /// <para><b>Automatic Field Assignment:</b></para>
    /// <list type="bullet">
    /// <item><description>Id: Auto-generated GUID</description></item>
    /// <item><description>UsageCount: Initialized to 0</description></item>
    /// <item><description>CreatedAt/UpdatedAt: Set to current UTC time</description></item>
    /// <item><description>CreatedBy: Set from authenticated user's ID</description></item>
    /// </list>
    /// <para><b>Authorization:</b></para>
    /// Requires Admin or Manager role. Authentication token must be provided in the Authorization header.
    /// </remarks>
    /// <response code="201">Promotion created successfully. Returns the created promotion with Location header.</response>
    /// <response code="400">Invalid input data or validation failure.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="403">Insufficient permissions - Admin or Manager role required.</response>
    /// <response code="409">Promotion code already exists.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Developer")]
    [ProducesResponseType(typeof(PromotionEntity), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
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
                GetCurrentUserId() ?? "Unknown"
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
    /// Updates an existing promotion's details, including discount amounts, validity dates, and status.
    /// </summary>
    /// <param name="id">The unique GUID of the promotion to update.</param>
    /// <param name="promotion">
    /// The promotion entity with updated values. The Id must match the route parameter.
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
    /// <item><description>Name, Code, Description</description></item>
    /// <item><description>Type, DiscountPercentage, DiscountAmount</description></item>
    /// <item><description>MinimumOrderAmount, MaximumDiscountAmount</description></item>
    /// <item><description>StartDate, EndDate</description></item>
    /// <item><description>IsActive, IsFeatured, Priority</description></item>
    /// <item><description>MaxUsageCount, IsCombinable</description></item>
    /// </list>
    /// <para><b>Protected Fields (Not Updated):</b></para>
    /// <list type="bullet">
    /// <item><description>Id: Cannot be changed</description></item>
    /// <item><description>UsageCount: Should be updated through order processing, not directly</description></item>
    /// <item><description>CreatedAt, CreatedBy: Audit fields remain unchanged</description></item>
    /// </list>
    /// <para><b>Validation Rules:</b></para>
    /// <list type="bullet">
    /// <item><description>ID in route must match ID in request body</description></item>
    /// <item><description>Name: 1-200 characters</description></item>
    /// <item><description>StartDate must be before EndDate</description></item>
    /// <item><description>Code must be unique (if changed from original)</description></item>
    /// <item><description>Promotion must exist and not be soft-deleted</description></item>
    /// </list>
    /// <para><b>Concurrency:</b></para>
    /// Returns 409 Conflict if the promotion was modified by another user during the update operation.
    /// Clients should refresh data and retry the operation.
    /// <para><b>Authorization:</b></para>
    /// Requires Admin or Manager role. Updates are tracked via UpdatedBy and UpdatedAt fields.
    /// </remarks>
    /// <response code="204">Promotion updated successfully. No content returned.</response>
    /// <response code="400">Invalid input data, ID mismatch, or validation failure.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="403">Insufficient permissions - Admin or Manager role required.</response>
    /// <response code="404">Promotion not found or has been deleted.</response>
    /// <response code="409">Conflict - promotion code already exists or concurrent modification detected.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager,Developer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
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
                        promotion.Code ?? "null"
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
                GetCurrentUserId() ?? "Unknown"
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
    /// Soft deletes a promotion, marking it as deleted while preserving the record for audit purposes.
    /// </summary>
    /// <param name="id">The unique GUID of the promotion to delete.</param>
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
    /// <item><description>Preserves all other promotion data</description></item>
    /// </list>
    /// <para><b>Effects:</b></para>
    /// <list type="bullet">
    /// <item><description>Promotion no longer appears in active/featured lists</description></item>
    /// <item><description>Code becomes available for reuse</description></item>
    /// <item><description>Historical order data referencing this promotion remains intact</description></item>
    /// <item><description>Promotion cannot be used for new orders</description></item>
    /// <item><description>Record remains in database for reporting and audit trails</description></item>
    /// </list>
    /// <para><b>Authorization:</b></para>
    /// Requires Admin role only. Managers cannot delete promotions - only Administrators have this permission
    /// to prevent accidental removal of active campaigns.
    /// <para><b>Best Practices:</b></para>
    /// Consider deactivating (IsActive = false) before deletion for promotions that may be reused.
    /// Deletion should be reserved for promotions that were created in error or are no longer relevant.
    /// </remarks>
    /// <response code="204">Promotion deleted successfully. No content returned.</response>
    /// <response code="400">Invalid promotion ID format provided.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="403">Insufficient permissions - Admin role required.</response>
    /// <response code="404">Promotion not found or already deleted.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Manager,Developer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
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
                promotion.Code ?? "null",
                GetCurrentUserId() ?? "Unknown"
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
