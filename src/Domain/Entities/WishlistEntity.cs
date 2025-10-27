namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents a customer's wishlist
/// </summary>
public sealed class WishlistEntity
{
    /// <summary>
    /// Unique identifier for the wishlist
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Customer who owns this wishlist
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Wishlist name (e.g., "My Wishlist", "Christmas List")
    /// </summary>
    public string Name { get; set; } = "My Wishlist";

    /// <summary>
    /// Whether the wishlist is public
    /// </summary>
    public bool IsPublic { get; set; } = false;

    /// <summary>
    /// Whether the wishlist is the default one
    /// </summary>
    public bool IsDefault { get; set; } = true;

    /// <summary>
    /// Date and time when the wishlist was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the wishlist was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
