namespace ECommerce.Domain.Policies;

/// <summary>
/// Defines business rules for wishlist management
/// </summary>
public static class WishlistPolicy
{
    private const int MaximumItemsPerWishlist = 100;
    private const int MaximumWishlistsPerUser = 10;
    private const int WishlistNameMinLength = 1;
    private const int WishlistNameMaxLength = 100;
    private const int WishlistExpirationDays = 90;

    /// <summary>
    /// Validates if an item can be added to a wishlist
    /// </summary>
    public static bool CanAddItemToWishlist(int currentItemCount)
    {
        return currentItemCount < MaximumItemsPerWishlist;
    }

    /// <summary>
    /// Validates if a user can create more wishlists
    /// </summary>
    public static bool CanCreateMoreWishlists(int currentWishlistCount)
    {
        return currentWishlistCount < MaximumWishlistsPerUser;
    }

    /// <summary>
    /// Validates wishlist name
    /// </summary>
    public static bool IsValidWishlistName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        return name.Length >= WishlistNameMinLength && name.Length <= WishlistNameMaxLength;
    }

    /// <summary>
    /// Checks if a wishlist has expired
    /// </summary>
    public static bool IsWishlistExpired(DateTime lastUpdatedAt)
    {
        var daysSinceUpdate = (DateTime.UtcNow - lastUpdatedAt).TotalDays;
        return daysSinceUpdate > WishlistExpirationDays;
    }

    /// <summary>
    /// Checks if a product is already in the wishlist
    /// </summary>
    public static bool IsProductInWishlist(List<Guid> wishlistProductIds, Guid productId)
    {
        return wishlistProductIds.Contains(productId);
    }

    /// <summary>
    /// Validates if a wishlist can be shared
    /// </summary>
    public static bool CanShareWishlist(bool isPublic, bool userAllowsSharing)
    {
        return isPublic && userAllowsSharing;
    }

    /// <summary>
    /// Checks if wishlist is empty
    /// </summary>
    public static bool IsWishlistEmpty(int itemCount)
    {
        return itemCount == 0;
    }

    /// <summary>
    /// Validates if items can be moved from wishlist to cart
    /// </summary>
    public static bool CanMoveToCart(int cartItemCount, int wishlistItemsToMove, int maxCartItems)
    {
        return (cartItemCount + wishlistItemsToMove) <= maxCartItems;
    }

    /// <summary>
    /// Checks if wishlist item should be auto-removed
    /// </summary>
    public static bool ShouldAutoRemoveItem(
        bool isProductActive,
        bool isProductDeleted,
        bool isProductDiscontinued
    )
    {
        return !isProductActive || isProductDeleted || isProductDiscontinued;
    }

    /// <summary>
    /// Validates if wishlist can be merged with another
    /// </summary>
    public static bool CanMergeWishlists(int wishlist1ItemCount, int wishlist2ItemCount)
    {
        return (wishlist1ItemCount + wishlist2ItemCount) <= MaximumItemsPerWishlist;
    }

    /// <summary>
    /// Checks if user can access a wishlist
    /// </summary>
    public static bool CanAccessWishlist(Guid wishlistOwnerId, Guid currentUserId, bool isPublic)
    {
        // Owner can always access
        if (wishlistOwnerId == currentUserId)
            return true;

        // Others can access only if public
        return isPublic;
    }

    /// <summary>
    /// Validates if notifications should be sent for price drops
    /// </summary>
    public static bool ShouldNotifyPriceDrop(
        decimal previousPrice,
        decimal currentPrice,
        bool userEnabledNotifications
    )
    {
        if (!userEnabledNotifications)
            return false;

        // Notify if price dropped by at least 5%
        var priceDropPercentage = ((previousPrice - currentPrice) / previousPrice) * 100;
        return priceDropPercentage >= 5m;
    }

    /// <summary>
    /// Validates if notifications should be sent for back in stock
    /// </summary>
    public static bool ShouldNotifyBackInStock(
        int previousStock,
        int currentStock,
        bool userEnabledNotifications
    )
    {
        if (!userEnabledNotifications)
            return false;

        // Notify when item comes back in stock
        return previousStock == 0 && currentStock > 0;
    }

    /// <summary>
    /// Checks if wishlist priority is valid
    /// </summary>
    public static bool IsValidPriority(int priority)
    {
        return priority >= 1 && priority <= 5;
    }

    /// <summary>
    /// Determines default wishlist name
    /// </summary>
    public static string GetDefaultWishlistName(int userWishlistCount)
    {
        return userWishlistCount == 0 ? "My Wishlist" : $"Wishlist {userWishlistCount + 1}";
    }
}
