using Comex.Domain.Enums;

namespace Comex.Domain.Policies;

/// <summary>
/// Defines business rules for product catalog management
/// </summary>
public static class ProductCatalogPolicy
{
    private const int MinProductNameLength = 3;
    private const int MaxProductNameLength = 200;
    private const int MaxDescriptionLength = 5000;
    private const int MaxBrandLength = 100;
    private const int MaxCategoryNameLength = 100;
    private const int MaxTagsPerProduct = 10;
    private const decimal MaxProductWeightKg = 500m;

    /// <summary>
    /// Validates a product name
    /// </summary>
    public static bool IsValidProductName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        return name.Trim().Length is >= MinProductNameLength and <= MaxProductNameLength;
    }

    /// <summary>
    /// Validates a product description
    /// </summary>
    public static bool IsValidProductDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return true; // Description is optional

        return description.Trim().Length <= MaxDescriptionLength;
    }

    /// <summary>
    /// Validates a brand name
    /// </summary>
    public static bool IsValidBrand(string? brand)
    {
        if (string.IsNullOrWhiteSpace(brand))
            return true; // Brand is optional

        return brand.Trim().Length <= MaxBrandLength;
    }

    /// <summary>
    /// Validates a category name
    /// </summary>
    public static bool IsValidCategoryName(string? categoryName)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
            return false;

        return categoryName.Trim().Length <= MaxCategoryNameLength;
    }

    /// <summary>
    /// Checks if a product's category can be changed
    /// </summary>
    public static bool CanChangeCategory(ProductStatus status, bool hasActiveOrders)
    {
        // Draft products can always be recategorized
        if (status == ProductStatus.Draft)
            return true;

        // Products with active orders should not be recategorized
        return !hasActiveOrders;
    }

    /// <summary>
    /// Validates a product's weight
    /// </summary>
    public static bool IsValidProductWeight(decimal weight)
    {
        return weight > 0 && weight <= MaxProductWeightKg;
    }

    /// <summary>
    /// Validates product tags
    /// </summary>
    public static bool IsValidProductTags(List<string>? tags)
    {
        if (tags == null || tags.Count == 0)
            return true; // Tags are optional

        if (tags.Count > MaxTagsPerProduct)
            return false;

        // Tags must be non-empty and within reasonable length
        return tags.All(tag => !string.IsNullOrWhiteSpace(tag) && tag.Trim().Length <= 50);
    }
}
