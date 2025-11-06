namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents a line item within an order, capturing a snapshot of product details at the time of purchase.
/// Each order item records the product, quantity, pricing, and discounts applied to a specific item in an order.
/// </summary>
/// <remarks>
/// Order items use a snapshot pattern to preserve historical accuracy:
/// <list type="bullet">
/// <item><description>Product details (name, SKU, price, image) are captured at order time</description></item>
/// <item><description>Changes to the product catalog don't affect existing orders</description></item>
/// <item><description>Pricing and discount information is frozen for audit purposes</description></item>
/// <item><description>Each item tracks its own tax and discount calculations</description></item>
/// </list>
/// The total price includes quantity calculations, discounts, and taxes.
/// </remarks>
/// <example>
/// An order item might represent "2x Wireless Mouse @ $29.99 each, 10% discount, $5.40 tax = $59.38 total"
/// </example>
public sealed class OrderItemEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the order item.
    /// </summary>
    /// <value>A GUID that uniquely identifies this line item within the system.</value>
    /// <remarks>
    /// Each line item in an order has its own unique identifier, separate from the order ID.
    /// Used for tracking individual items through fulfillment, returns, and refunds.
    /// </remarks>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the parent order that contains this item.
    /// </summary>
    /// <value>The GUID of the order to which this item belongs.</value>
    /// <remarks>
    /// This foreign key establishes the relationship between the order item and its parent order.
    /// All items with the same OrderId are part of a single customer order.
    /// Required field - every order item must belong to exactly one order.
    /// </remarks>
    /// <example>a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d</example>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the product being ordered.
    /// </summary>
    /// <value>The GUID of the product from the product catalog.</value>
    /// <remarks>
    /// References the original product in the catalog. While product details are snapshotted
    /// in other properties, this ID maintains the link to the current product record.
    /// Useful for inventory management, analytics, and determining which products were sold.
    /// </remarks>
    /// <example>b2c3d4e5-f6a7-5b6c-9d0e-1f2a3b4c5d6e</example>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Gets or sets the name of the product as it appeared at the time of order.
    /// </summary>
    /// <value>
    /// A snapshot of the product name from the catalog at order time.
    /// Preserved even if the product name changes later.
    /// </value>
    /// <remarks>
    /// This snapshot ensures order history remains accurate even if products are renamed,
    /// removed from the catalog, or their details are modified. Essential for:
    /// <list type="bullet">
    /// <item><description>Displaying accurate order history to customers</description></item>
    /// <item><description>Generating historical invoices and receipts</description></item>
    /// <item><description>Legal and accounting record-keeping requirements</description></item>
    /// </list>
    /// </remarks>
    /// <example>Wireless Bluetooth Mouse</example>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Stock Keeping Unit (SKU) of the product at the time of order.
    /// </summary>
    /// <value>
    /// A snapshot of the product's SKU code from the catalog at order time.
    /// Typically an alphanumeric identifier used for inventory tracking.
    /// </value>
    /// <remarks>
    /// SKU snapshot is critical for:
    /// <list type="bullet">
    /// <item><description>Warehouse fulfillment and picking operations</description></item>
    /// <item><description>Inventory reconciliation and auditing</description></item>
    /// <item><description>Identifying exact product variants (size, color, model)</description></item>
    /// <item><description>Returns and exchanges processing</description></item>
    /// </list>
    /// Preserved even if the SKU is changed or discontinued in the catalog.
    /// </remarks>
    /// <example>WM-BT-001-BLK</example>
    public string ProductSku { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the quantity of the product ordered.
    /// </summary>
    /// <value>
    /// The number of units of this product included in the order.
    /// Must be a positive integer (typically 1 or greater).
    /// </value>
    /// <remarks>
    /// Quantity is used to calculate the total price: (Quantity × UnitPrice) - DiscountAmount + TaxAmount.
    /// For bundle items or cases, this represents the number of bundles/cases, not individual items.
    /// Should be validated to ensure it doesn't exceed available inventory at order time.
    /// </remarks>
    /// <example>2</example>
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the price per unit of the product at the time of order.
    /// </summary>
    /// <value>
    /// The monetary amount charged for a single unit of the product.
    /// Captured at order time and may differ from the current catalog price.
    /// </value>
    /// <remarks>
    /// Unit price snapshot ensures pricing accuracy regardless of future price changes.
    /// This is the base price before applying:
    /// <list type="bullet">
    /// <item><description>Quantity multipliers</description></item>
    /// <item><description>Item-level or order-level discounts</description></item>
    /// <item><description>Taxes</description></item>
    /// </list>
    /// May reflect promotional pricing, volume discounts, or customer-specific pricing at order time.
    /// </remarks>
    /// <example>29.99</example>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Gets or sets the total discount amount applied to this order item.
    /// </summary>
    /// <value>
    /// The monetary amount deducted from the subtotal (Quantity × UnitPrice) due to discounts.
    /// Zero if no discount was applied. Always a non-negative value.
    /// </value>
    /// <remarks>
    /// Discount can originate from various sources:
    /// <list type="bullet">
    /// <item><description>Product-specific promotions or sales</description></item>
    /// <item><description>Order-level coupon codes (proportionally allocated to items)</description></item>
    /// <item><description>Volume discounts or bulk pricing</description></item>
    /// <item><description>Customer loyalty rewards or tier-based discounts</description></item>
    /// </list>
    /// This is the final calculated discount amount, not a percentage or rate.
    /// </remarks>
    /// <example>5.00</example>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// Gets or sets the total tax amount applied to this order item.
    /// </summary>
    /// <value>
    /// The monetary amount of sales tax, VAT, or other taxes applicable to this item.
    /// Calculated based on post-discount price and applicable tax rates.
    /// Zero if the item is tax-exempt or in a jurisdiction without sales tax.
    /// </value>
    /// <remarks>
    /// Tax calculation typically follows the formula:
    /// TaxAmount = (Quantity × UnitPrice - DiscountAmount) × TaxRate
    /// Tax rates may vary based on:
    /// <list type="bullet">
    /// <item><description>Shipping destination (state, province, country)</description></item>
    /// <item><description>Product category (some items may be tax-exempt)</description></item>
    /// <item><description>Customer type (wholesale, retail, tax-exempt organizations)</description></item>
    /// </list>
    /// </remarks>
    /// <example>5.40</example>
    public decimal TaxAmount { get; set; }

    /// <summary>
    /// Gets or sets the final total price for this order item including all adjustments.
    /// </summary>
    /// <value>
    /// The complete monetary amount for this line item after applying quantity, discounts, and taxes.
    /// Calculated as: (Quantity × UnitPrice) - DiscountAmount + TaxAmount
    /// </value>
    /// <remarks>
    /// This is the line item total that contributes to the overall order total.
    /// Should always equal: (Quantity × UnitPrice) - DiscountAmount + TaxAmount
    /// <para>Calculation breakdown:</para>
    /// <list type="number">
    /// <item><description>Subtotal = Quantity × UnitPrice</description></item>
    /// <item><description>After Discount = Subtotal - DiscountAmount</description></item>
    /// <item><description>Final Total = After Discount + TaxAmount</description></item>
    /// </list>
    /// Used for invoice generation, payment processing, and financial reporting.
    /// </remarks>
    /// <example>59.38</example>
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// Gets or sets the URL of the product image as it appeared at the time of order.
    /// </summary>
    /// <value>
    /// A snapshot of the product's primary image URL, or null if no image was available.
    /// Typically points to a CDN or image storage service.
    /// </value>
    /// <remarks>
    /// Image URL snapshot ensures order history displays correctly even if:
    /// <list type="bullet">
    /// <item><description>Product images are updated or replaced</description></item>
    /// <item><description>Products are discontinued and images removed</description></item>
    /// <item><description>Image hosting locations change</description></item>
    /// </list>
    /// Used for displaying order confirmation emails, invoices, and order history pages.
    /// Should be a complete, absolute URL for reliable long-term access.
    /// </remarks>
    /// <example>https://cdn.example.com/products/wireless-mouse-black-thumb.jpg</example>
    public string? ProductImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the order item was created.
    /// </summary>
    /// <value>
    /// The UTC timestamp when this line item was added to the order.
    /// Default value is the current UTC time when the entity is instantiated.
    /// </value>
    /// <remarks>
    /// Typically set when the item is added to the order (which may occur during checkout).
    /// For orders created in a single transaction, all items will have similar CreatedAt timestamps.
    /// For shopping carts that evolve into orders, this may reflect when items were added to the cart.
    /// Used for audit trails and tracking order processing timelines.
    /// </remarks>
    /// <example>2024-01-15T10:30:00Z</example>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the order item was last updated.
    /// </summary>
    /// <value>
    /// The UTC timestamp of the most recent modification to this line item.
    /// Default value is the current UTC time when the entity is instantiated.
    /// </value>
    /// <remarks>
    /// Updated whenever any property of the order item changes, such as:
    /// <list type="bullet">
    /// <item><description>Quantity adjustments before order finalization</description></item>
    /// <item><description>Price corrections or adjustments</description></item>
    /// <item><description>Discount or tax recalculations</description></item>
    /// <item><description>Status changes during fulfillment</description></item>
    /// </list>
    /// Initially set to the same value as <see cref="CreatedAt"/>.
    /// Once an order is finalized, items typically should not be modified (except for cancellations/returns).
    /// </remarks>
    /// <example>2024-01-15T10:35:22Z</example>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
