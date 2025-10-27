using ECommerce.Domain.Entities;
using ECommerce.Domain.Policies;

namespace ECommerce.Domain.Services;

/// <summary>
/// Domain service for shopping cart management and validation
/// </summary>
public static class CartManagementService
{
    private const int MaxCartItemQuantity = 999;
    private const int CartExpirationDays = 30;

    /// <summary>
    /// Validates if an item can be added to cart
    /// </summary>
    public static (bool isValid, string? errorMessage) ValidateAddToCart(
        ProductEntity product,
        int requestedQuantity
    )
    {
        if (!product.IsActive)
            return (false, "Product is not available");

        if (product.IsDeleted)
            return (false, "Product not found");

        if (requestedQuantity <= 0)
            return (false, "Quantity must be greater than zero");

        if (requestedQuantity > MaxCartItemQuantity)
            return (false, $"Maximum quantity per item is {MaxCartItemQuantity}");

        if (!StockManagementPolicy.CanReserveStock(product.StockQuantity, requestedQuantity))
            return (false, "Insufficient stock available");

        if (
            !StockManagementPolicy.IsQuantityWithinOrderLimits(
                requestedQuantity,
                product.MaxOrderQuantity
            )
        )
            return (
                false,
                $"Maximum order quantity for this product is {product.MaxOrderQuantity}"
            );

        return (true, null);
    }

    /// <summary>
    /// Validates if cart is ready for checkout
    /// </summary>
    public static (bool isValid, string? errorMessage) ValidateCartForCheckout(
        List<(ProductEntity product, int quantity)> cartItems
    )
    {
        if (cartItems == null || cartItems.Count == 0)
            return (false, "Cart is empty");

        foreach (var (product, quantity) in cartItems)
        {
            var validation = ValidateAddToCart(product, quantity);
            if (!validation.isValid)
                return (false, $"Product {product.Name}: {validation.errorMessage}");
        }

        return (true, null);
    }

    /// <summary>
    /// Calculates cart expiration date
    /// </summary>
    public static DateTime CalculateCartExpiration()
    {
        return DateTime.UtcNow.AddDays(CartExpirationDays);
    }

    /// <summary>
    /// Checks if cart has expired
    /// </summary>
    public static bool IsCartExpired(DateTime? expiresAt)
    {
        if (!expiresAt.HasValue)
            return false;

        return DateTime.UtcNow > expiresAt.Value;
    }

    /// <summary>
    /// Merges anonymous cart with user cart after login
    /// </summary>
    public static List<(Guid productId, int quantity)> MergeCarts(
        List<(Guid productId, int quantity)> anonymousCart,
        List<(Guid productId, int quantity)> userCart
    )
    {
        var mergedCart = new Dictionary<Guid, int>();

        // Add user cart items
        foreach (var (productId, quantity) in userCart)
        {
            mergedCart[productId] = quantity;
        }

        // Merge with anonymous cart (add quantities for same products)
        foreach (var (productId, quantity) in anonymousCart)
        {
            if (mergedCart.ContainsKey(productId))
                mergedCart[productId] += quantity;
            else
                mergedCart[productId] = quantity;
        }

        return mergedCart.Select(kvp => (kvp.Key, kvp.Value)).ToList();
    }

    /// <summary>
    /// Calculates total cart weight for shipping
    /// </summary>
    public static decimal CalculateTotalWeight(
        List<(ProductEntity product, int quantity)> cartItems
    )
    {
        return cartItems.Sum(item => item.product.Weight * item.quantity);
    }

    /// <summary>
    /// Checks if cart qualifies for free shipping
    /// </summary>
    public static bool QualifiesForFreeShipping(
        decimal cartSubtotal,
        decimal freeShippingThreshold = 100m
    )
    {
        return cartSubtotal >= freeShippingThreshold;
    }

    /// <summary>
    /// Calculates how much more is needed for free shipping
    /// </summary>
    public static decimal AmountNeededForFreeShipping(
        decimal cartSubtotal,
        decimal freeShippingThreshold = 100m
    )
    {
        if (QualifiesForFreeShipping(cartSubtotal, freeShippingThreshold))
            return 0;

        return freeShippingThreshold - cartSubtotal;
    }

    /// <summary>
    /// Suggests products to reach free shipping threshold
    /// </summary>
    public static List<ProductEntity> SuggestProductsForFreeShipping(
        List<ProductEntity> availableProducts,
        decimal amountNeeded
    )
    {
        return availableProducts
            .Where(p => p.IsActive && !p.IsDeleted && p.StockQuantity > 0)
            .Where(p =>
            {
                var effectivePrice = PricingPolicy.GetEffectivePrice(p.Price, p.DiscountPrice);
                return effectivePrice <= amountNeeded * 1.2m; // Within 120% of needed amount
            })
            .OrderBy(p =>
                Math.Abs(PricingPolicy.GetEffectivePrice(p.Price, p.DiscountPrice) - amountNeeded)
            )
            .Take(5)
            .ToList();
    }
}
