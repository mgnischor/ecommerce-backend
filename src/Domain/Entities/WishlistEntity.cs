namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents a customer's wishlist in the e-commerce system.
/// </summary>
/// <remarks>
/// Wishlists allow customers to save products for future purchase consideration.
/// Customers can create multiple wishlists for different purposes (gift lists, favorites, etc.).
/// Wishlists can be marked as public for sharing with friends and family.
/// Each customer has one default wishlist, with the ability to create additional named lists.
/// </remarks>
public sealed class WishlistEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for this wishlist.
    /// </summary>
    /// <value>A <see cref="Guid"/> that uniquely identifies the wishlist.</value>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the customer who owns this wishlist.
    /// </summary>
    /// <value>A <see cref="Guid"/> referencing the customer entity.</value>
    /// <remarks>
    /// Each wishlist belongs to exactly one customer. A customer may have multiple wishlists.
    /// </remarks>
    /// <example>7c9e6679-7425-40de-944b-e07fc1f90ae7</example>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the display name of the wishlist.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> containing the wishlist name.
    /// Defaults to "My Wishlist" for the primary wishlist.
    /// </value>
    /// <remarks>
    /// Customers can create multiple wishlists with different names for organization:
    /// birthday gifts, wedding registry, Christmas shopping, general favorites, etc.
    /// </remarks>
    /// <example>My Wishlist, Christmas Gifts 2025, Birthday Ideas, Dream Products</example>
    public string Name { get; set; } = "My Wishlist";

    /// <summary>
    /// Gets or sets a value indicating whether the wishlist can be viewed by others.
    /// </summary>
    /// <value>
    /// <c>true</c> if the wishlist is public and can be shared via URL;
    /// otherwise, <c>false</c> for private wishlists visible only to the owner.
    /// Defaults to <c>false</c>.
    /// </value>
    /// <remarks>
    /// Public wishlists enable gift-giving scenarios where friends and family
    /// can view items without needing to log in or have special access.
    /// Common for wedding registries, baby showers, and holiday gift lists.
    /// </remarks>
    public bool IsPublic { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether this is the customer's default wishlist.
    /// </summary>
    /// <value>
    /// <c>true</c> if this is the primary/default wishlist for quick "Add to Wishlist" actions;
    /// otherwise, <c>false</c> for secondary wishlists. Defaults to <c>true</c>.
    /// </value>
    /// <remarks>
    /// Each customer should have exactly one default wishlist.
    /// When a customer clicks "Add to Wishlist" without specifying which list,
    /// the item is added to the default wishlist.
    /// The first wishlist created for a customer is typically set as default.
    /// </remarks>
    public bool IsDefault { get; set; } = true;

    /// <summary>
    /// Gets or sets the date and time when this wishlist was created.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> in UTC format representing the creation timestamp.
    /// Defaults to <see cref="DateTime.UtcNow"/> when the entity is instantiated.
    /// </value>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when this wishlist was last updated.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> in UTC format representing the last modification timestamp.
    /// Defaults to <see cref="DateTime.UtcNow"/> when the entity is instantiated.
    /// Updated when wishlist name, visibility, or default status changes.
    /// </value>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
