namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents an item in a shopping cart
/// </summary>
public sealed class CartItemEntity
{
    /// <summary>
    /// Unique identifier for the cart item
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Cart that this item belongs to
    /// </summary>
    public Guid CartId { get; set; }

    /// <summary>
    /// Product in the cart
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Quantity of the product
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Price per unit at the time of adding to cart
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Date and time when the item was added to cart
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the item was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
