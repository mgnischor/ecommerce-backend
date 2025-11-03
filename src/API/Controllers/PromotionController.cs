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

    public PromotionController(PostgresqlContext context, ILogger<PromotionController> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves all active promotions
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PromotionEntity>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PromotionEntity>>> GetActivePromotions(
        CancellationToken cancellationToken = default
    )
    {
        var now = DateTime.UtcNow;
        var promotions = await _context
            .Promotions
            .Where(
                p =>
                    p.IsActive
                    && !p.IsDeleted
                    && p.StartDate <= now
                    && p.EndDate >= now
            )
            .OrderByDescending(p => p.Priority)
            .ThenByDescending(p => p.StartDate)
            .ToListAsync(cancellationToken);

        return Ok(promotions);
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
        var now = DateTime.UtcNow;
        var promotions = await _context
            .Promotions
            .Where(
                p =>
                    p.IsFeatured
                    && p.IsActive
                    && !p.IsDeleted
                    && p.StartDate <= now
                    && p.EndDate >= now
            )
            .OrderByDescending(p => p.Priority)
            .ToListAsync(cancellationToken);

        return Ok(promotions);
    }

    /// <summary>
    /// Retrieves a specific promotion by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PromotionEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PromotionEntity>> GetPromotionById(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var promotion = await _context.Promotions.FirstOrDefaultAsync(
            p => p.Id == id && !p.IsDeleted,
            cancellationToken
        );

        if (promotion == null)
            return NotFound(new { Message = $"Promotion with ID '{id}' not found" });

        return Ok(promotion);
    }

    /// <summary>
    /// Retrieves a promotion by code
    /// </summary>
    [HttpGet("code/{code}")]
    [ProducesResponseType(typeof(PromotionEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PromotionEntity>> GetPromotionByCode(
        string code,
        CancellationToken cancellationToken = default
    )
    {
        var promotion = await _context.Promotions.FirstOrDefaultAsync(
            p => p.Code == code && !p.IsDeleted,
            cancellationToken
        );

        if (promotion == null)
            return NotFound(new { Message = $"Promotion with code '{code}' not found" });

        return Ok(promotion);
    }

    /// <summary>
    /// Creates a new promotion
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(PromotionEntity), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PromotionEntity>> CreatePromotion(
        [FromBody] PromotionEntity promotion,
        CancellationToken cancellationToken = default
    )
    {
        if (promotion == null)
            return BadRequest("Promotion data is required");

        promotion.Id = Guid.NewGuid();
        promotion.CreatedAt = DateTime.UtcNow;
        promotion.UpdatedAt = DateTime.UtcNow;

        _context.Promotions.Add(promotion);
        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetPromotionById), new { id = promotion.Id }, promotion);
    }

    /// <summary>
    /// Updates an existing promotion
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePromotion(
        Guid id,
        [FromBody] PromotionEntity promotion,
        CancellationToken cancellationToken = default
    )
    {
        if (promotion == null)
            return BadRequest("Promotion data is required");

        if (id != promotion.Id)
            return BadRequest("ID mismatch");

        var existingPromotion = await _context.Promotions.FindAsync(
            new object[] { id },
            cancellationToken
        );

        if (existingPromotion == null || existingPromotion.IsDeleted)
            return NotFound(new { Message = $"Promotion with ID '{id}' not found" });

        promotion.UpdatedAt = DateTime.UtcNow;
        _context.Entry(existingPromotion).CurrentValues.SetValues(promotion);

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Deletes a promotion (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePromotion(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var promotion = await _context.Promotions.FindAsync(
            new object[] { id },
            cancellationToken
        );

        if (promotion == null || promotion.IsDeleted)
            return NotFound(new { Message = $"Promotion with ID '{id}' not found" });

        promotion.IsDeleted = true;
        promotion.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
