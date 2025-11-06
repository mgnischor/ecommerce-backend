namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents a shopping cart for a customer in the e-commerce system.
/// </summary>
/// <remarks>
/// Shopping carts can be associated with registered customers or anonymous sessions.
/// Carts support coupon codes for discounts and have configurable expiration times.
/// Inactive carts may be automatically cleaned up based on business rules.
/// </remarks>
public sealed class CartEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for this shopping cart.
    /// </summary>
    /// <value>A <see cref="Guid"/> that uniquely identifies the cart.</value>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the customer who owns this cart.
    /// </summary>
    /// <value>A <see cref="Guid"/> referencing the customer (user) entity.</value>
    /// <example>7c9e6679-7425-40de-944b-e07fc1f90ae7</example>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the session identifier for anonymous shopping carts.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> containing the session ID for guest users,
    /// or <c>null</c> if the cart belongs to a registered customer.
    /// </value>
    /// <example>sess_1234567890abcdef</example>
    public string? SessionId { get; set; }

    /// <summary>
    /// Gets or sets the coupon code applied to this cart.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> containing the promotional coupon code,
    /// or <c>null</c> if no coupon is applied.
    /// </value>
    /// <example>SAVE20, FREESHIP, WELCOME10</example>
    public string? CouponCode { get; set; }

    /// <summary>
    /// Gets or sets the total discount amount applied from the coupon.
    /// </summary>
    /// <value>
    /// A <see cref="decimal"/> representing the discount amount in the system's base currency.
    /// Defaults to 0 if no coupon is applied.
    /// </value>
    /// <example>15.50</example>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this cart is currently active.
    /// </summary>
    /// <value>
    /// <c>true</c> if the cart is active and can be modified;
    /// otherwise, <c>false</c> if the cart has been converted to an order or abandoned.
    /// Defaults to <c>true</c>.
    /// </value>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the date and time when this cart expires.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> in UTC format representing when the cart should be considered expired,
    /// or <c>null</c> if the cart has no expiration.
    /// Expired carts may be automatically cleaned up by background processes.
    /// </value>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this cart was created.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> in UTC format representing the creation timestamp.
    /// Defaults to <see cref="DateTime.UtcNow"/> when the entity is instantiated.
    /// </value>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when this cart was last updated.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> in UTC format representing the last modification timestamp.
    /// Defaults to <see cref="DateTime.UtcNow"/> when the entity is instantiated.
    /// This should be updated whenever cart items are added, removed, or modified.
    /// </value>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
