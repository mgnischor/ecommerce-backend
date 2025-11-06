using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Policies;

/// <summary>
/// Defines business rules for product availability and access control
/// </summary>
public static class ProductAvailabilityPolicy
{
    private const int MinimumPublishDelayHours = 1;
    private const int MaximumPreOrderDays = 180;
    private const int DiscontinuedGracePeriodDays = 30;

    /// <summary>
    /// Checks if a product is available for purchase
    /// </summary>
    public static bool IsAvailableForPurchase(ProductStatus status, int stockLevel, bool isDeleted)
    {
        if (isDeleted)
            return false;

        if (status != ProductStatus.Active)
            return false;

        return stockLevel > 0;
    }

    /// <summary>
    /// Checks if a product is visible to customers
    /// </summary>
    public static bool IsVisibleToCustomers(
        ProductStatus status,
        bool isDeleted,
        DateTime? publishDate
    )
    {
        if (isDeleted)
            return false;

        if (status == ProductStatus.Draft)
            return false;

        // Check if scheduled publish date has passed
        if (publishDate.HasValue && DateTime.UtcNow < publishDate.Value)
            return false;

        return true;
    }

    /// <summary>
    /// Validates product publish date
    /// </summary>
    public static bool IsValidPublishDate(DateTime? publishDate)
    {
        if (!publishDate.HasValue)
            return true; // Publish immediately

        // Must be at least minimum delay in the future
        var minimumPublishTime = DateTime.UtcNow.AddHours(MinimumPublishDelayHours);
        return publishDate.Value >= minimumPublishTime;
    }

    /// <summary>
    /// Checks if a product can be pre-ordered
    /// </summary>
    public static bool CanBePreOrdered(
        ProductStatus status,
        bool allowsPreOrder,
        DateTime? expectedRestockDate
    )
    {
        if (status != ProductStatus.Active && status != ProductStatus.OutOfStock)
            return false;

        if (!allowsPreOrder)
            return false;

        // Must have a restock date set
        if (!expectedRestockDate.HasValue)
            return false;

        // Restock date must be within acceptable range
        var daysUntilRestock = (expectedRestockDate.Value - DateTime.UtcNow).TotalDays;
        return daysUntilRestock > 0 && daysUntilRestock <= MaximumPreOrderDays;
    }

    /// <summary>
    /// Validates if a product status transition is allowed
    /// </summary>
    public static bool IsValidStatusTransition(ProductStatus currentStatus, ProductStatus newStatus)
    {
        return (currentStatus, newStatus) switch
        {
            (ProductStatus.Draft, ProductStatus.Active) => true,
            (ProductStatus.Draft, ProductStatus.Inactive) => true,
            (ProductStatus.Active, ProductStatus.Inactive) => true,
            (ProductStatus.Active, ProductStatus.OutOfStock) => true,
            (ProductStatus.Active, ProductStatus.Discontinued) => true,
            (ProductStatus.Inactive, ProductStatus.Active) => true,
            (ProductStatus.Inactive, ProductStatus.Discontinued) => true,
            (ProductStatus.OutOfStock, ProductStatus.Active) => true,
            (ProductStatus.OutOfStock, ProductStatus.Discontinued) => true,
            _ => false,
        };
    }

    /// <summary>
    /// Checks if a product can be edited
    /// </summary>
    public static bool CanEditProduct(ProductStatus status, bool hasActiveOrders)
    {
        // Draft products can always be edited
        if (status == ProductStatus.Draft)
            return true;

        // Products with active orders have restrictions
        if (hasActiveOrders)
            return false;

        return true;
    }

    /// <summary>
    /// Checks if a product can be deleted
    /// </summary>
    public static bool CanDeleteProduct(
        ProductStatus status,
        bool hasActiveOrders,
        int totalOrderCount
    )
    {
        // Cannot delete if there are active orders
        if (hasActiveOrders)
            return false;

        // Products with order history should be discontinued instead
        if (totalOrderCount > 0)
            return false;

        // Only draft and inactive products without history can be deleted
        return status == ProductStatus.Draft || status == ProductStatus.Inactive;
    }

    /// <summary>
    /// Checks if discontinued product can still be viewed
    /// </summary>
    public static bool CanViewDiscontinuedProduct(DateTime? discontinuedDate)
    {
        if (!discontinuedDate.HasValue)
            return true;

        // Allow viewing for grace period after discontinuation
        var daysSinceDiscontinued = (DateTime.UtcNow - discontinuedDate.Value).TotalDays;
        return daysSinceDiscontinued <= DiscontinuedGracePeriodDays;
    }

    /// <summary>
    /// Validates if product is accessible by user role
    /// </summary>
    public static bool IsAccessibleByUserRole(ProductStatus status, UserAccessLevel userLevel)
    {
        return userLevel switch
        {
            UserAccessLevel.Admin => true,
            UserAccessLevel.Manager => true,
            UserAccessLevel.Developer => true,
            UserAccessLevel.Customer => status == ProductStatus.Active,
            UserAccessLevel.Company => status == ProductStatus.Active,
            UserAccessLevel.Guest => status == ProductStatus.Active,
            _ => false,
        };
    }

    /// <summary>
    /// Checks if product requires age verification
    /// </summary>
    public static bool RequiresAgeVerification(string? category, bool isRestricted)
    {
        if (isRestricted)
            return true;

        // Add specific categories that require age verification
        var restrictedCategories = new[] { "Alcohol", "Tobacco", "Adult Content" };
        return category != null && restrictedCategories.Contains(category);
    }

    /// <summary>
    /// Validates if product images can be updated
    /// </summary>
    public static bool CanUpdateImages(ProductStatus status, bool hasActiveOrders)
    {
        // Can update images except for discontinued products with active orders
        if (status == ProductStatus.Discontinued && hasActiveOrders)
            return false;

        return true;
    }

    /// <summary>
    /// Checks if product should show "coming soon" badge
    /// </summary>
    public static bool ShouldShowComingSoon(ProductStatus status, DateTime? publishDate)
    {
        if (status != ProductStatus.Inactive)
            return false;

        if (!publishDate.HasValue)
            return false;

        // Show "coming soon" if publish date is in the future
        return DateTime.UtcNow < publishDate.Value;
    }
}
