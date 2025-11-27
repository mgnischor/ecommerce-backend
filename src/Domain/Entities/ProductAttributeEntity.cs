namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents a product attribute definition in the e-commerce system.
/// </summary>
/// <remarks>
/// Product attributes define dynamic properties that can be assigned to products
/// such as color, size, material, weight, dimensions, etc.
/// Attributes support type validation, filtering, search functionality, and product variants.
/// This entity defines the attribute schema, while actual values are stored per product or variant.
/// </remarks>
public class ProductAttributeEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for this product attribute.
    /// </summary>
    /// <value>A <see cref="Guid"/> that uniquely identifies the attribute definition.</value>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who created this attribute.
    /// </summary>
    /// <value>A <see cref="Guid"/> referencing the creating user.</value>
    /// <remarks>
    /// Typically an administrator or catalog manager who defines the attribute schema.
    /// </remarks>
    /// <example>7c9e6679-7425-40de-944b-e07fc1f90ae7</example>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last updated this attribute.
    /// </summary>
    /// <value>
    /// A <see cref="Guid"/> referencing the user who performed the last modification,
    /// or <c>null</c> if never updated since creation.
    /// </value>
    /// <example>8b3c8d7a-9c4e-4f5a-8d3c-9e5f8a7b6c5d</example>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// Attribute name (e.g., "Color", "Size", "Material")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Attribute code (unique identifier for programmatic use)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Attribute description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Data type of the attribute (Text, Number, Boolean, Date, List)
    /// </summary>
    public string DataType { get; set; } = "Text";

    /// <summary>
    /// List of possible values (for List type attributes)
    /// </summary>
    public List<string> PossibleValues { get; set; } = new List<string>();

    /// <summary>
    /// Default value for this attribute
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Whether this attribute is required for products
    /// </summary>
    public bool IsRequired { get; set; } = false;

    /// <summary>
    /// Whether this attribute is used for product variants
    /// </summary>
    public bool IsVariantAttribute { get; set; } = false;

    /// <summary>
    /// Whether this attribute is filterable in product search
    /// </summary>
    public bool IsFilterable { get; set; } = true;

    /// <summary>
    /// Whether this attribute is searchable
    /// </summary>
    public bool IsSearchable { get; set; } = true;

    /// <summary>
    /// Whether this attribute is visible on product page
    /// </summary>
    public bool IsVisibleOnProductPage { get; set; } = true;

    /// <summary>
    /// Display order for sorting
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// Unit of measurement (if applicable, e.g., "cm", "kg")
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// Validation pattern (regex) for the attribute value
    /// </summary>
    public string? ValidationPattern { get; set; }

    /// <summary>
    /// Categories this attribute applies to (empty = all categories)
    /// </summary>
    public List<Guid> ApplicableCategoryIds { get; set; } = new List<Guid>();

    /// <summary>
    /// Whether the attribute is deleted (soft delete)
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Date and time when the attribute was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the attribute was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
