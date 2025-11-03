using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace ECommerce.API.Controllers;

/// <summary>
/// Product attribute management endpoints
/// </summary>
[Tags("ProductAttributes")]
[ApiController]
[Route("api/v1/product-attributes")]
[Produces("application/json")]
[Authorize(Roles = "Admin,Manager")]
public sealed class ProductAttributeController : ControllerBase
{
    private readonly PostgresqlContext _context;
    private readonly ILogger<ProductAttributeController> _logger;
    private const int MaxAttributes = 500;

    public ProductAttributeController(
        PostgresqlContext context,
        ILogger<ProductAttributeController> logger
    )
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private string? GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    /// <summary>
    /// Retrieves all product attributes (Public endpoint)
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<ProductAttributeEntity>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductAttributeEntity>>> GetAllAttributes(
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var attributes = await _context.ProductAttributes
                .Where(a => !a.IsDeleted)
                .OrderBy(a => a.DisplayOrder)
                .ThenBy(a => a.Name)
                .Take(MaxAttributes)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Retrieved {Count} product attributes", attributes.Count);
            return Ok(attributes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product attributes");
            return StatusCode(500, new { Message = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Retrieves a specific attribute by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProductAttributeEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductAttributeEntity>> GetAttributeById(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid product attribute GUID provided");
            return BadRequest(new { Message = "Invalid attribute ID" });
        }

        try
        {
            var attribute = await _context.ProductAttributes
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, cancellationToken);

            if (attribute == null)
            {
                _logger.LogInformation("Product attribute not found: {AttributeId}", id);
                return NotFound(new { Message = "Product attribute not found" });
            }

            return Ok(attribute);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product attribute: {AttributeId}", id);
            return StatusCode(500, new { Message = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Retrieves an attribute by code
    /// </summary>
    [HttpGet("code/{code}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProductAttributeEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductAttributeEntity>> GetAttributeByCode(
        string code,
        CancellationToken cancellationToken = default
    )
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(code))
        {
            _logger.LogWarning("Empty attribute code provided");
            return BadRequest(new { Message = "Attribute code is required" });
        }

        if (code.Length > 50)
        {
            _logger.LogWarning("Attribute code too long: {Length}", code.Length);
            return BadRequest(new { Message = "Attribute code must not exceed 50 characters" });
        }

        // Validate code format
        if (!Regex.IsMatch(code, @"^[a-zA-Z0-9\-_]+$"))
        {
            _logger.LogWarning("Invalid attribute code format: {Code}", code);
            return BadRequest(new { Message = "Invalid attribute code format" });
        }

        try
        {
            var attribute = await _context.ProductAttributes
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Code == code && !a.IsDeleted, cancellationToken);

            if (attribute == null)
            {
                _logger.LogInformation("Product attribute code not found: {Code}", code);
                return NotFound(new { Message = "Product attribute not found" });
            }

            return Ok(attribute);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product attribute by code");
            return StatusCode(500, new { Message = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Retrieves variant attributes only
    /// </summary>
    [HttpGet("variant")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<ProductAttributeEntity>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductAttributeEntity>>> GetVariantAttributes(
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var attributes = await _context.ProductAttributes
                .Where(a => a.IsVariantAttribute && !a.IsDeleted)
                .OrderBy(a => a.DisplayOrder)
                .Take(MaxAttributes)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Retrieved {Count} variant attributes", attributes.Count);
            return Ok(attributes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving variant attributes");
            return StatusCode(500, new { Message = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Creates a new product attribute
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProductAttributeEntity), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductAttributeEntity>> CreateAttribute(
        [FromBody] ProductAttributeEntity attribute,
        CancellationToken cancellationToken = default
    )
    {
        if (attribute == null)
        {
            _logger.LogWarning("Null product attribute data received");
            return BadRequest(new { Message = "Attribute data is required" });
        }

        // Input validation
        if (string.IsNullOrWhiteSpace(attribute.Name) || attribute.Name.Length > 100)
        {
            return BadRequest(new { Message = "Valid attribute name is required (max 100 characters)" });
        }

        if (string.IsNullOrWhiteSpace(attribute.Code) || attribute.Code.Length > 50)
        {
            return BadRequest(new { Message = "Valid attribute code is required (max 50 characters)" });
        }

        if (!Regex.IsMatch(attribute.Code, @"^[a-zA-Z0-9\-_]+$"))
        {
            return BadRequest(new { Message = "Invalid attribute code format. Use alphanumeric, hyphens, underscores only" });
        }

        if (attribute.DisplayOrder < 0)
        {
            return BadRequest(new { Message = "Display order must be a positive number" });
        }

        try
        {
            // Check for duplicate code
            var duplicateCode = await _context.ProductAttributes
                .AnyAsync(a => a.Code == attribute.Code && !a.IsDeleted, cancellationToken);

            if (duplicateCode)
            {
                _logger.LogWarning("Duplicate product attribute code attempt: {Code}", attribute.Code);
                return Conflict(new { Message = "Attribute code already exists" });
            }

            // Secure assignment
            var newAttribute = new ProductAttributeEntity
            {
                Id = Guid.NewGuid(),
                Name = attribute.Name,
                Code = attribute.Code,
                Description = attribute.Description,
                DataType = attribute.DataType,
                IsRequired = attribute.IsRequired,
                IsVariantAttribute = attribute.IsVariantAttribute,
                IsSearchable = attribute.IsSearchable,
                IsFilterable = attribute.IsFilterable,
                ValidationPattern = attribute.ValidationPattern,
                DefaultValue = attribute.DefaultValue,
                DisplayOrder = attribute.DisplayOrder,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = Guid.TryParse(GetCurrentUserId(), out var userId) ? userId : Guid.Empty
            };

            _context.ProductAttributes.Add(newAttribute);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Product attribute created: {AttributeId}, Code: {Code}, User: {UserId}", 
                newAttribute.Id, newAttribute.Code, GetCurrentUserId());

            return CreatedAtAction(nameof(GetAttributeById), new { id = newAttribute.Id }, newAttribute);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product attribute");
            return StatusCode(500, new { Message = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Updates an existing product attribute
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateAttribute(
        Guid id,
        [FromBody] ProductAttributeEntity attribute,
        CancellationToken cancellationToken = default
    )
    {
        if (attribute == null)
        {
            _logger.LogWarning("Null product attribute data received for update");
            return BadRequest(new { Message = "Attribute data is required" });
        }

        if (id == Guid.Empty || id != attribute.Id)
        {
            _logger.LogWarning("ID mismatch in attribute update. Route: {RouteId}, Body: {BodyId}", id, attribute.Id);
            return BadRequest(new { Message = "ID mismatch" });
        }

        // Input validation
        if (string.IsNullOrWhiteSpace(attribute.Name) || attribute.Name.Length > 100)
        {
            return BadRequest(new { Message = "Valid attribute name is required (max 100 characters)" });
        }

        try
        {
            var existingAttribute = await _context.ProductAttributes
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, cancellationToken);

            if (existingAttribute == null)
            {
                _logger.LogWarning("Product attribute not found for update: {AttributeId}", id);
                return NotFound(new { Message = "Product attribute not found" });
            }

            // Check for code conflict if code changed
            if (existingAttribute.Code != attribute.Code)
            {
                var duplicateCode = await _context.ProductAttributes
                    .AnyAsync(a => a.Code == attribute.Code && a.Id != id && !a.IsDeleted, cancellationToken);

                if (duplicateCode)
                {
                    _logger.LogWarning("Duplicate attribute code in update: {Code}", attribute.Code);
                    return Conflict(new { Message = "Attribute code already exists" });
                }
            }

            // Selective update
            existingAttribute.Name = attribute.Name;
            existingAttribute.Code = attribute.Code;
            existingAttribute.Description = attribute.Description;
            existingAttribute.DataType = attribute.DataType;
            existingAttribute.IsRequired = attribute.IsRequired;
            existingAttribute.IsVariantAttribute = attribute.IsVariantAttribute;
            existingAttribute.IsSearchable = attribute.IsSearchable;
            existingAttribute.IsFilterable = attribute.IsFilterable;
            existingAttribute.ValidationPattern = attribute.ValidationPattern;
            existingAttribute.DefaultValue = attribute.DefaultValue;
            existingAttribute.DisplayOrder = attribute.DisplayOrder;
            existingAttribute.UpdatedAt = DateTime.UtcNow;
            existingAttribute.UpdatedBy = Guid.TryParse(GetCurrentUserId(), out var userId) ? userId : Guid.Empty;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Product attribute updated: {AttributeId}, User: {UserId}", id, GetCurrentUserId());
            return NoContent();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict updating attribute: {AttributeId}", id);
            return Conflict(new { Message = "The attribute was modified by another user. Please refresh and try again" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product attribute: {AttributeId}", id);
            return StatusCode(500, new { Message = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Deletes a product attribute (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAttribute(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid product attribute GUID provided for deletion");
            return BadRequest(new { Message = "Invalid attribute ID" });
        }

        try
        {
            var attribute = await _context.ProductAttributes
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, cancellationToken);

            if (attribute == null)
            {
                _logger.LogWarning("Product attribute not found for deletion: {AttributeId}", id);
                return NotFound(new { Message = "Product attribute not found" });
            }

            attribute.IsDeleted = true;
            attribute.UpdatedAt = DateTime.UtcNow;
            attribute.UpdatedBy = Guid.TryParse(GetCurrentUserId(), out var userId) ? userId : Guid.Empty;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogWarning("Product attribute deleted: {AttributeId}, Code: {Code}, User: {UserId}", 
                id, attribute.Code, GetCurrentUserId());
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product attribute: {AttributeId}", id);
            return StatusCode(500, new { Message = "An error occurred while processing your request" });
        }
    }
}
