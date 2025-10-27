namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents an item in a wishlist
/// </summary>
public sealed class WishlistItemEntity
{
    /// <summary>
    /// Unique identifier for the wishlist item
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Wishlist that this item belongs to
    /// </summary>
    public Guid WishlistId { get; set; }

    /// <summary>
    /// Product in the wishlist
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Customer notes about this product
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Priority level (1-5, where 5 is highest)
    /// </summary>
    public int Priority { get; set; } = 3;

    /// <summary>
    /// Date and time when the item was added
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the item was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
