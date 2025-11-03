namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents a product attribute in the e-commerce system.
/// Defines dynamic attributes that can be assigned to products (e.g., color, size, material).
/// </summary>
public class ProductAttributeEntity
{
    /// <summary>
    /// Unique identifier for the attribute
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User who created this attribute
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// User who last updated this attribute
    /// </summary>
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
