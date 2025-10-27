using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Aggregates;

/// <summary>
/// Shopping cart aggregate root - encapsulates cart business logic
/// </summary>
public sealed class CartAggregate
{
    private readonly List<CartItemEntity> _items = new();

    /// <summary>
    /// Cart entity (aggregate root)
    /// </summary>
    public CartEntity Cart { get; private set; }

    /// <summary>
    /// Cart items (read-only collection)
    /// </summary>
    public IReadOnlyList<CartItemEntity> Items => _items.AsReadOnly();

    /// <summary>
    /// Total items count
    /// </summary>
    public int TotalItemsCount => _items.Sum(i => i.Quantity);

    /// <summary>
    /// Subtotal (before discounts)
    /// </summary>
    public decimal SubTotal => _items.Sum(i => i.Quantity * i.UnitPrice);

    /// <summary>
    /// Total (after discounts)
    /// </summary>
    public decimal Total => SubTotal - Cart.DiscountAmount;

    private CartAggregate(CartEntity cart)
    {
        Cart = cart ?? throw new ArgumentNullException(nameof(cart));
    }

    /// <summary>
    /// Creates a new cart aggregate for a customer
    /// </summary>
    public static CartAggregate CreateForCustomer(Guid customerId)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer ID cannot be empty", nameof(customerId));

        var cart = new CartEntity
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return new CartAggregate(cart);
    }

    /// <summary>
    /// Creates a new cart aggregate for an anonymous session
    /// </summary>
    public static CartAggregate CreateForSession(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("Session ID cannot be empty", nameof(sessionId));

        var cart = new CartEntity
        {
            Id = Guid.NewGuid(),
            CustomerId = Guid.Empty,
            SessionId = sessionId,
            IsActive = true,
            ExpiresAt = DateTime.UtcNow.AddDays(7), // Anonymous carts expire after 7 days
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return new CartAggregate(cart);
    }

    /// <summary>
    /// Loads an existing cart with its items
    /// </summary>
    public static CartAggregate Load(CartEntity cart, IEnumerable<CartItemEntity> items)
    {
        var aggregate = new CartAggregate(cart);
        aggregate._items.AddRange(items);
        return aggregate;
    }

    /// <summary>
    /// Adds a product to the cart
    /// </summary>
    public void AddItem(Guid productId, decimal unitPrice, int quantity = 1)
    {
        ValidateCartIsActive();

        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty", nameof(productId));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative", nameof(unitPrice));

        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);

        if (existingItem != null)
        {
            // Update quantity if item already exists
            existingItem.Quantity += quantity;
            existingItem.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            // Add new item
            var item = new CartItemEntity
            {
                Id = Guid.NewGuid(),
                CartId = Cart.Id,
                ProductId = productId,
                Quantity = quantity,
                UnitPrice = unitPrice,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _items.Add(item);
        }

        Cart.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the quantity of an item in the cart
    /// </summary>
    public void UpdateItemQuantity(Guid productId, int newQuantity)
    {
        ValidateCartIsActive();

        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(newQuantity));

        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
            throw new InvalidOperationException($"Product {productId} not found in cart");

        item.Quantity = newQuantity;
        item.UpdatedAt = DateTime.UtcNow;
        Cart.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes an item from the cart
    /// </summary>
    public void RemoveItem(Guid productId)
    {
        ValidateCartIsActive();

        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            _items.Remove(item);
            Cart.UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Clears all items from the cart
    /// </summary>
    public void Clear()
    {
        ValidateCartIsActive();

        _items.Clear();
        Cart.CouponCode = null;
        Cart.DiscountAmount = 0;
        Cart.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Applies a coupon to the cart
    /// </summary>
    public void ApplyCoupon(string couponCode, decimal discountAmount)
    {
        ValidateCartIsActive();

        if (string.IsNullOrWhiteSpace(couponCode))
            throw new ArgumentException("Coupon code cannot be empty", nameof(couponCode));

        if (discountAmount < 0)
            throw new ArgumentException("Discount amount cannot be negative", nameof(discountAmount));

        if (discountAmount > SubTotal)
            throw new ArgumentException("Discount amount cannot exceed subtotal", nameof(discountAmount));

        Cart.CouponCode = couponCode;
        Cart.DiscountAmount = discountAmount;
        Cart.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes the applied coupon
    /// </summary>
    public void RemoveCoupon()
    {
        ValidateCartIsActive();

        Cart.CouponCode = null;
        Cart.DiscountAmount = 0;
        Cart.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Merges an anonymous cart with a customer cart
    /// </summary>
    public void MergeWith(CartAggregate anonymousCart)
    {
        if (anonymousCart == null)
            throw new ArgumentNullException(nameof(anonymousCart));

        if (Cart.CustomerId == Guid.Empty)
            throw new InvalidOperationException("Cannot merge into an anonymous cart");

        foreach (var anonymousItem in anonymousCart.Items)
        {
            var existingItem = _items.FirstOrDefault(i => i.ProductId == anonymousItem.ProductId);
            
            if (existingItem != null)
            {
                // Add quantities together
                existingItem.Quantity += anonymousItem.Quantity;
                existingItem.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Add the item from anonymous cart
                var newItem = new CartItemEntity
                {
                    Id = Guid.NewGuid(),
                    CartId = Cart.Id,
                    ProductId = anonymousItem.ProductId,
                    Quantity = anonymousItem.Quantity,
                    UnitPrice = anonymousItem.UnitPrice,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _items.Add(newItem);
            }
        }

        Cart.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Converts anonymous cart to customer cart
    /// </summary>
    public void ConvertToCustomerCart(Guid customerId)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer ID cannot be empty", nameof(customerId));

        if (Cart.CustomerId != Guid.Empty)
            throw new InvalidOperationException("Cart is already associated with a customer");

        Cart.CustomerId = customerId;
        Cart.SessionId = null;
        Cart.ExpiresAt = null; // Customer carts don't expire
        Cart.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if the cart is expired
    /// </summary>
    public bool IsExpired()
    {
        return Cart.ExpiresAt.HasValue && Cart.ExpiresAt.Value < DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the cart
    /// </summary>
    public void Deactivate()
    {
        Cart.IsActive = false;
        Cart.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Validates if the cart is active
    /// </summary>
    private void ValidateCartIsActive()
    {
        if (!Cart.IsActive)
            throw new InvalidOperationException("Cannot modify an inactive cart");

        if (IsExpired())
            throw new InvalidOperationException("Cannot modify an expired cart");
    }

    /// <summary>
    /// Validates the cart state
    /// </summary>
    public bool IsValid(out List<string> errors)
    {
        errors = new List<string>();

        if (Cart.CustomerId == Guid.Empty && string.IsNullOrWhiteSpace(Cart.SessionId))
            errors.Add("Cart must have either a customer ID or session ID");

        if (_items.Any(i => i.Quantity <= 0))
            errors.Add("All cart items must have positive quantities");

        if (_items.Any(i => i.UnitPrice < 0))
            errors.Add("All cart items must have non-negative prices");

        if (Cart.DiscountAmount < 0)
            errors.Add("Discount amount cannot be negative");

        if (Cart.DiscountAmount > SubTotal)
            errors.Add("Discount amount cannot exceed subtotal");

        return errors.Count == 0;
    }
}
