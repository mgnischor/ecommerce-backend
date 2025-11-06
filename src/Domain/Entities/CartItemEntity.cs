namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents an individual item within a shopping cart.
/// </summary>
/// <remarks>
/// Each cart item captures the product, quantity, and price at the time it was added to the cart.
/// Price snapshots ensure consistency even if product prices change before checkout.
/// </remarks>
public sealed class CartItemEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for this cart item.
    /// </summary>
    /// <value>A <see cref="Guid"/> that uniquely identifies the cart item.</value>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the cart that this item belongs to.
    /// </summary>
    /// <value>A <see cref="Guid"/> referencing the parent <see cref="CartEntity"/>.</value>
    /// <example>7c9e6679-7425-40de-944b-e07fc1f90ae7</example>
    public Guid CartId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the product in this cart item.
    /// </summary>
    /// <value>A <see cref="Guid"/> referencing the <see cref="ProductEntity"/>.</value>
    /// <example>8b3c8d7a-9c4e-4f5a-8d3c-9e5f8a7b6c5d</example>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Gets or sets the quantity of the product in the cart.
    /// </summary>
    /// <value>
    /// An <see cref="int"/> representing the number of units of this product.
    /// Must be a positive integer greater than zero.
    /// </value>
    /// <example>3</example>
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the price per unit at the time the product was added to the cart.
    /// </summary>
    /// <value>
    /// A <see cref="decimal"/> representing the unit price in the system's base currency.
    /// This is a snapshot of the price at the time of addition and may differ from the current product price.
    /// </value>
    /// <example>29.99</example>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this item was added to the cart.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> in UTC format representing the creation timestamp.
    /// Defaults to <see cref="DateTime.UtcNow"/> when the entity is instantiated.
    /// </value>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when this cart item was last updated.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> in UTC format representing the last modification timestamp.
    /// Defaults to <see cref="DateTime.UtcNow"/> when the entity is instantiated.
    /// This should be updated when the quantity or price is modified.
    /// </value>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
