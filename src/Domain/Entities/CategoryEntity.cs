namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents a hierarchical product category in the e-commerce system.
/// </summary>
/// <remarks>
/// Categories support unlimited nesting levels with parent-child relationships.
/// Each category can have its own image, SEO metadata, and display ordering.
/// The entity includes soft delete capability and activity status management.
/// URL-friendly slugs enable clean and search-engine-optimized category pages.
/// </remarks>
public sealed class CategoryEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for this category.
    /// </summary>
    /// <value>A <see cref="Guid"/> that uniquely identifies the category.</value>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who created this category.
    /// </summary>
    /// <value>A <see cref="Guid"/> referencing the user who created the category.</value>
    /// <example>7c9e6679-7425-40de-944b-e07fc1f90ae7</example>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last updated this category.
    /// </summary>
    /// <value>
    /// A <see cref="Guid"/> referencing the user who performed the last modification,
    /// or <c>null</c> if the category has never been updated since creation.
    /// </value>
    /// <example>8b3c8d7a-9c4e-4f5a-8d3c-9e5f8a7b6c5d</example>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the parent category in the hierarchy.
    /// </summary>
    /// <value>
    /// A <see cref="Guid"/> referencing the parent <see cref="CategoryEntity"/>,
    /// or <c>null</c> if this is a root-level category.
    /// </value>
    /// <example>9d4e5f8b-0d5f-4a6b-9e8d-0f1a2b3c4d5e</example>
    public Guid? ParentCategoryId { get; set; }

    /// <summary>
    /// Gets or sets the display name of the category.
    /// </summary>
    /// <value>A <see cref="string"/> containing the category's human-readable name.</value>
    /// <example>Electronics, Men's Clothing, Sports & Outdoors</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL-friendly slug for this category.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> containing the SEO-friendly URL slug.
    /// Must be unique and typically lowercase with hyphens instead of spaces.
    /// </value>
    /// <example>electronics, mens-clothing, sports-outdoors</example>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional description of the category.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> containing the category description or overview,
    /// or <c>null</c> if no description is provided.
    /// </value>
    /// <example>Browse our complete selection of electronic devices and accessories.</example>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the optional image URL for the category.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> containing the URL or path to the category image,
    /// or <c>null</c> if no image is associated.
    /// </value>
    /// <example>/images/categories/electronics.jpg, https://cdn.example.com/cat/123.png</example>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the display order for sorting categories.
    /// </summary>
    /// <value>
    /// An <see cref="int"/> representing the sort order.
    /// Lower numbers appear first. Defaults to 0.
    /// </value>
    /// <example>0, 10, 100</example>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// Gets or sets a value indicating whether the category is active and visible.
    /// </summary>
    /// <value>
    /// <c>true</c> if the category is active and should be displayed to customers;
    /// otherwise, <c>false</c>. Defaults to <c>true</c>.
    /// Inactive categories are hidden but not deleted.
    /// </value>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the category has been soft-deleted.
    /// </summary>
    /// <value>
    /// <c>true</c> if the category is deleted but retained for historical purposes;
    /// otherwise, <c>false</c>. Defaults to <c>false</c>.
    /// </value>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Gets or sets the optional meta title for search engine optimization.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> containing the SEO meta title,
    /// or <c>null</c> to use the category name as the title.
    /// </value>
    /// <example>Buy Electronics Online - Best Prices | StoreName</example>
    public string? MetaTitle { get; set; }

    /// <summary>
    /// Gets or sets the optional meta description for search engine optimization.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> containing the SEO meta description,
    /// or <c>null</c> if no custom description is provided.
    /// Typically 150-160 characters for optimal SEO.
    /// </value>
    /// <example>Shop the latest electronics with free shipping. Find great deals on phones, laptops, and more.</example>
    public string? MetaDescription { get; set; }

    /// <summary>
    /// Gets or sets the optional meta keywords for search engine optimization.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> containing comma-separated SEO keywords,
    /// or <c>null</c> if no keywords are specified.
    /// Note: Meta keywords have limited SEO value in modern search engines.
    /// </value>
    /// <example>electronics, gadgets, smartphones, laptops, tablets</example>
    public string? MetaKeywords { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this category was created.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> in UTC format representing the creation timestamp.
    /// Defaults to <see cref="DateTime.UtcNow"/> when the entity is instantiated.
    /// </value>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when this category was last updated.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> in UTC format representing the last modification timestamp.
    /// Defaults to <see cref="DateTime.UtcNow"/> when the entity is instantiated.
    /// Updated whenever any category information is changed.
    /// </value>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
