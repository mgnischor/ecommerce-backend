namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents an individual item within a customer's wishlist.
/// </summary>
/// <remarks>
/// Each wishlist item links a specific product to a wishlist with additional metadata
/// such as priority level and personal notes. Items can be prioritized to help customers
/// organize their purchasing decisions and share preferences with gift-givers.
/// </remarks>
public sealed class WishlistItemEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for this wishlist item.
    /// </summary>
    /// <value>A <see cref="Guid"/> that uniquely identifies the wishlist item.</value>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the wishlist that contains this item.
    /// </summary>
    /// <value>A <see cref="Guid"/> referencing the parent <see cref="WishlistEntity"/>.</value>
    /// <remarks>
    /// Establishes the relationship between the item and its parent wishlist.
    /// All items with the same WishlistId belong to the same wishlist.
    /// </remarks>
    /// <example>7c9e6679-7425-40de-944b-e07fc1f90ae7</example>
    public Guid WishlistId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the product saved in this wishlist item.
    /// </summary>
    /// <value>A <see cref="Guid"/> referencing the <see cref="ProductEntity"/>.</value>
    /// <remarks>
    /// References the product in the catalog. The same product can appear in multiple
    /// wishlists or even multiple times in different lists owned by the same customer.
    /// Product availability, pricing, and details are retrieved from the live product catalog.
    /// </remarks>
    /// <example>8b3c8d7a-9c4e-4f5a-8d3c-9e5f8a7b6c5d</example>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Gets or sets optional personal notes about this product.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> containing the customer's notes, comments, or preferences,
    /// or <c>null</c> if no notes have been added.
    /// </value>
    /// <remarks>
    /// Notes allow customers to:
    /// <list type="bullet">
    /// <item><description>Remember why they wanted the product</description></item>
    /// <item><description>Specify size, color, or variant preferences</description></item>
    /// <item><description>Add gift-giving context ("for dad's birthday")</description></item>
    /// <item><description>Track when they first saw the product or special details</description></item>
    /// </list>
    /// For public wishlists, notes help gift-givers understand preferences and requirements.
    /// </remarks>
    /// <example>Size large in blue, Perfect for the new apartment, Remember to buy before December</example>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets the priority level indicating how much the customer wants this item.
    /// </summary>
    /// <value>
    /// An <see cref="int"/> between 1 and 5, where 5 is highest priority.
    /// Defaults to 3 (medium priority).
    /// </value>
    /// <remarks>
    /// Priority helps customers organize their wishlist by importance:
    /// <list type="bullet">
    /// <item><description><b>5</b> - Must have, highest priority, gift registry items</description></item>
    /// <item><description><b>4</b> - Really want, high priority</description></item>
    /// <item><description><b>3</b> - Moderate interest, default priority</description></item>
    /// <item><description><b>2</b> - Nice to have, low priority</description></item>
    /// <item><description><b>1</b> - Lowest priority, considering but not urgent</description></item>
    /// </list>
    /// Priority levels are especially useful for:
    /// <list type="bullet">
    /// <item><description>Gift registries where high-priority items are essential</description></item>
    /// <item><description>Budget planning and purchase decisions</description></item>
    /// <item><description>Sorting and displaying items in meaningful order</description></item>
    /// </list>
    /// </remarks>
    /// <example>5</example>
    public int Priority { get; set; } = 3;

    /// <summary>
    /// Gets or sets the date and time when this item was added to the wishlist.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> in UTC format representing the creation timestamp.
    /// Defaults to <see cref="DateTime.UtcNow"/> when the entity is instantiated.
    /// </value>
    /// <remarks>
    /// Used for:
    /// <list type="bullet">
    /// <item><description>Tracking how long an item has been wishlisted</description></item>
    /// <item><description>Price tracking and comparison over time</description></item>
    /// <item><description>Sorting items by date added</description></item>
    /// <item><description>Analytics on wishlist behavior patterns</description></item>
    /// </list>
    /// </remarks>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when this wishlist item was last updated.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> in UTC format representing the last modification timestamp.
    /// Defaults to <see cref="DateTime.UtcNow"/> when the entity is instantiated.
    /// </value>
    /// <remarks>
    /// Updated when the customer modifies notes or changes the priority level.
    /// Does not update when the product itself is modified in the catalog.
    /// </remarks>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
