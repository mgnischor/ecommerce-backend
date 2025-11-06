namespace ECommerce.Domain.Policies;

/// <summary>
/// Defines business rules for shopping cart management and validation
/// </summary>
public static class CartPolicy
{
    private const int MaximumItemsInCart = 50;
    private const int MaximumQuantityPerItem = 99;
    private const int CartExpirationDays = 30;
    private const int GuestCartExpirationDays = 7;
    private const decimal MaximumCartValue = 999999.99m;

    /// <summary>
    /// Validates if an item can be added to the cart
    /// </summary>
    public static bool CanAddItemToCart(int currentItemCount)
    {
        return currentItemCount < MaximumItemsInCart;
    }

    /// <summary>
    /// Validates if the quantity is within acceptable limits
    /// </summary>
    public static bool IsValidQuantity(int quantity)
    {
        return quantity > 0 && quantity <= MaximumQuantityPerItem;
    }

    /// <summary>
    /// Validates if quantity can be updated for an existing cart item
    /// </summary>
    public static bool CanUpdateQuantity(int newQuantity, int availableStock)
    {
        if (!IsValidQuantity(newQuantity))
            return false;

        return newQuantity <= availableStock;
    }

    /// <summary>
    /// Checks if a cart has expired
    /// </summary>
    public static bool IsCartExpired(DateTime lastUpdatedAt, bool isGuestCart)
    {
        var expirationDays = isGuestCart ? GuestCartExpirationDays : CartExpirationDays;
        var daysSinceUpdate = (DateTime.UtcNow - lastUpdatedAt).TotalDays;

        return daysSinceUpdate > expirationDays;
    }

    /// <summary>
    /// Validates if cart value is within limits
    /// </summary>
    public static bool IsValidCartValue(decimal totalValue)
    {
        return totalValue > 0 && totalValue <= MaximumCartValue;
    }

    /// <summary>
    /// Checks if a cart can be merged with another cart
    /// </summary>
    public static bool CanMergeCarts(int cart1ItemCount, int cart2ItemCount)
    {
        return (cart1ItemCount + cart2ItemCount) <= MaximumItemsInCart;
    }

    /// <summary>
    /// Validates if duplicate items should be merged
    /// </summary>
    public static bool ShouldMergeDuplicateItems(
        Guid productId1,
        Guid productId2,
        Guid? variant1,
        Guid? variant2
    )
    {
        // Items are considered duplicates if they have the same product and variant
        return productId1 == productId2 && variant1 == variant2;
    }

    /// <summary>
    /// Calculates merged quantity for duplicate items
    /// </summary>
    public static int CalculateMergedQuantity(int quantity1, int quantity2)
    {
        var mergedQuantity = quantity1 + quantity2;
        return Math.Min(mergedQuantity, MaximumQuantityPerItem);
    }

    /// <summary>
    /// Checks if cart is empty
    /// </summary>
    public static bool IsCartEmpty(int itemCount)
    {
        return itemCount == 0;
    }

    /// <summary>
    /// Validates if cart can be converted to an order
    /// </summary>
    public static bool CanConvertToOrder(int itemCount, decimal totalValue, bool hasOutOfStockItems)
    {
        if (IsCartEmpty(itemCount))
            return false;

        if (!IsValidCartValue(totalValue))
            return false;

        // Cannot convert if any item is out of stock
        return !hasOutOfStockItems;
    }

    /// <summary>
    /// Checks if cart items need stock revalidation
    /// </summary>
    public static bool RequiresStockRevalidation(DateTime lastValidatedAt)
    {
        var minutesSinceValidation = (DateTime.UtcNow - lastValidatedAt).TotalMinutes;
        return minutesSinceValidation > 15; // Revalidate every 15 minutes
    }

    /// <summary>
    /// Validates if cart can be saved for later
    /// </summary>
    public static bool CanSaveCartForLater(bool isAuthenticated)
    {
        // Only authenticated users can save carts
        return isAuthenticated;
    }

    /// <summary>
    /// Checks if item should be auto-removed due to unavailability
    /// </summary>
    public static bool ShouldAutoRemoveItem(
        bool isProductActive,
        bool isProductDeleted,
        int availableStock
    )
    {
        // Remove if product is inactive, deleted, or permanently out of stock
        return !isProductActive || isProductDeleted || availableStock == 0;
    }

    /// <summary>
    /// Calculates cart subtotal
    /// </summary>
    public static decimal CalculateSubtotal(List<(decimal unitPrice, int quantity)> items)
    {
        return items.Sum(item => item.unitPrice * item.quantity);
    }

    /// <summary>
    /// Validates if a product can be added multiple times with different variants
    /// </summary>
    public static bool AllowsMultipleVariants(bool hasVariants)
    {
        return hasVariants;
    }

    /// <summary>
    /// Checks if cart requires minimum order value
    /// </summary>
    public static bool MeetsMinimumOrderValue(decimal cartTotal, decimal minimumOrderValue)
    {
        if (minimumOrderValue <= 0)
            return true;

        return cartTotal >= minimumOrderValue;
    }
}
