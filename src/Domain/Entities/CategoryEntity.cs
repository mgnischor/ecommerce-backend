namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents a product category (hierarchical)
/// </summary>
public sealed class CategoryEntity
{
    /// <summary>
    /// Unique identifier for the category
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User who created this category
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// User who last updated this category
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// Parent category ID (null for root categories)
    /// </summary>
    public Guid? ParentCategoryId { get; set; }

    /// <summary>
    /// Category name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// URL-friendly slug
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Category description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Category image URL
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Display order for sorting
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// Whether the category is active and visible
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether the category is deleted (soft delete)
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Meta title for SEO
    /// </summary>
    public string? MetaTitle { get; set; }

    /// <summary>
    /// Meta description for SEO
    /// </summary>
    public string? MetaDescription { get; set; }

    /// <summary>
    /// Meta keywords for SEO
    /// </summary>
    public string? MetaKeywords { get; set; }

    /// <summary>
    /// Date and time when the category was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the category was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
