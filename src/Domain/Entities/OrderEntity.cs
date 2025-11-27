using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents a customer order in the e-commerce system.
/// </summary>
/// <remarks>
/// This entity tracks the complete lifecycle of an order from creation through delivery.
/// It includes pricing details, payment and shipping information, order status tracking,
/// and supports order modifications, cancellations, and audit trails.
/// Orders are immutable once confirmed to maintain data integrity for accounting purposes.
/// </remarks>
public sealed class OrderEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for this order.
    /// </summary>
    /// <value>A <see cref="Guid"/> that uniquely identifies the order.</value>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who created this order.
    /// </summary>
    /// <value>A <see cref="Guid"/> referencing the user who placed the order.</value>
    /// <remarks>
    /// Typically the customer's user ID, but may be a sales representative
    /// or administrator for orders placed on behalf of customers.
    /// </remarks>
    /// <example>7c9e6679-7425-40de-944b-e07fc1f90ae7</example>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last updated this order.
    /// </summary>
    /// <value>
    /// A <see cref="Guid"/> referencing the user who performed the last modification,
    /// or <c>null</c> if the order has never been updated since creation.
    /// </value>
    /// <remarks>
    /// Tracks who modified order details (status changes, address updates, etc.)
    /// for audit and accountability purposes.
    /// </remarks>
    /// <example>8b3c8d7a-9c4e-4f5a-8d3c-9e5f8a7b6c5d</example>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the customer who placed this order.
    /// </summary>
    /// <value>A <see cref="Guid"/> referencing the customer entity.</value>
    /// <remarks>
    /// Links the order to the customer account for order history, loyalty programs,
    /// and customer service operations.
    /// </remarks>
    /// <example>9d4e5f8b-0d5f-4a6b-9e8d-0f1a2b3c4d5e</example>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the human-readable order number for customer reference.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> containing the formatted order number.
    /// Typically follows a pattern like ORD-YYYYMMDD-XXXX.
    /// </value>
    /// <remarks>
    /// Displayed to customers in order confirmations, invoices, and tracking pages.
    /// Must be unique across all orders.
    /// </remarks>
    /// <example>ORD-20251126-0001</example>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current status of the order in its lifecycle.
    /// </summary>
    /// <value>
    /// An <see cref="OrderStatus"/> enumeration value.
    /// Defaults to <see cref="OrderStatus.Pending"/>.
    /// </value>
    /// <remarks>
    /// Order status progression typically follows:
    /// Pending → Confirmed → Processing → Shipped → Delivered
    /// Alternative paths: Cancelled, OnHold, Refunded
    /// </remarks>
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    /// <summary>
    /// Gets or sets the subtotal amount before taxes, shipping, and discounts.
    /// </summary>
    /// <value>
    /// A <see cref="decimal"/> representing the sum of all order items' prices.
    /// </value>
    /// <remarks>
    /// Calculated as the sum of (quantity × unit price) for all order items.
    /// This is the base amount before any adjustments.
    /// </remarks>
    /// <example>299.99</example>
    public decimal SubTotal { get; set; }

    /// <summary>
    /// Gets or sets the total tax amount for this order.
    /// </summary>
    /// <value>A <see cref="decimal"/> representing sales tax, VAT, or other applicable taxes.</value>
    /// <remarks>
    /// Tax calculation depends on jurisdiction, product type, and customer status.
    /// May include multiple tax types aggregated into a single amount.
    /// </remarks>
    /// <example>27.00</example>
    public decimal TaxAmount { get; set; }

    /// <summary>
    /// Gets or sets the shipping cost for this order.
    /// </summary>
    /// <value>A <see cref="decimal"/> representing the delivery charge.</value>
    /// <remarks>
    /// Shipping cost varies based on:
    /// destination, shipping method, package weight/dimensions, and promotions.
    /// May be zero for free shipping offers.
    /// </remarks>
    /// <example>15.00</example>
    public decimal ShippingCost { get; set; }

    /// <summary>
    /// Gets or sets the total discount amount applied to this order.
    /// </summary>
    /// <value>A <see cref="decimal"/> representing the discount deduction.</value>
    /// <remarks>
    /// Includes discounts from coupons, promotions, loyalty rewards, and special offers.
    /// This amount is subtracted from the total calculation.
    /// </remarks>
    /// <example>30.00</example>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// Gets or sets the final total amount for this order.
    /// </summary>
    /// <value>
    /// A <see cref="decimal"/> calculated as:
    /// SubTotal + TaxAmount + ShippingCost - DiscountAmount
    /// </value>
    /// <remarks>
    /// This is the amount the customer pays and must match the payment amount.
    /// Used for payment processing, invoicing, and financial reporting.
    /// </remarks>
    /// <example>311.99</example>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Gets or sets the payment method used for this order.
    /// </summary>
    /// <value>
    /// A <see cref="PaymentMethod"/> enumeration value.
    /// Defaults to <see cref="PaymentMethod.NotSpecified"/>.
    /// </value>
    /// <remarks>
    /// Payment method affects processing time, fees, and refund capabilities.
    /// Common methods: Credit Card, PayPal, Bank Transfer, Cash on Delivery.
    /// </remarks>
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.NotSpecified;

    /// <summary>
    /// Gets or sets the shipping method selected for delivery.
    /// </summary>
    /// <value>
    /// A <see cref="ShippingMethod"/> enumeration value.
    /// Defaults to <see cref="ShippingMethod.NotSpecified"/>.
    /// </value>
    /// <remarks>
    /// Shipping method determines delivery speed, cost, and carrier.
    /// Common methods: Standard, Express, Overnight, Store Pickup.
    /// </remarks>
    public ShippingMethod ShippingMethod { get; set; } = ShippingMethod.NotSpecified;

    /// <summary>
    /// Gets or sets the identifier of the shipping address.
    /// </summary>
    /// <value>
    /// A <see cref="Guid"/> referencing the <see cref="AddressEntity"/>,
    /// or <c>null</c> for digital or pickup orders.
    /// </value>
    /// <remarks>
    /// References the address where the order will be delivered.
    /// Address details are typically snapshotted in the order for historical accuracy.
    /// </remarks>
    /// <example>0e1f2a3b-4c5d-6e7f-8a9b-0c1d2e3f4a5b</example>
    public Guid? ShippingAddressId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the billing address.
    /// </summary>
    /// <value>
    /// A <see cref="Guid"/> referencing the <see cref="AddressEntity"/>,
    /// or <c>null</c> if using the shipping address for billing.
    /// </value>
    /// <remarks>
    /// Billing address is used for payment verification and invoicing.
    /// Often the same as shipping address but can differ.
    /// </remarks>
    /// <example>1f2a3b4c-5d6e-7f8a-9b0c-1d2e3f4a5b6c</example>
    public Guid? BillingAddressId { get; set; }

    /// <summary>
    /// Gets or sets the coupon code applied to this order.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> containing the promotional code,
    /// or <c>null</c> if no coupon was used.
    /// </value>
    /// <remarks>
    /// Coupon codes provide discounts and are validated at checkout.
    /// Captured for promotional tracking and customer analytics.
    /// </remarks>
    /// <example>SAVE20, FREESHIP, WELCOME10</example>
    public string? CouponCode { get; set; }

    /// <summary>
    /// Gets or sets optional notes or special instructions from the customer.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> containing customer comments,
    /// or <c>null</c> if no notes were provided.
    /// </value>
    /// <remarks>
    /// Customer notes may include:
    /// delivery instructions, gift messages, special requests, or preferences.
    /// Visible to fulfillment team and customer service.
    /// </remarks>
    /// <example>Please leave at front door, Gift wrap requested, Call before delivery</example>
    public string? CustomerNotes { get; set; }

    /// <summary>
    /// Gets or sets internal administrative notes about this order.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> containing internal comments,
    /// or <c>null</c> if no admin notes exist.
    /// </value>
    /// <remarks>
    /// Admin notes are for internal use only and not visible to customers.
    /// Used for tracking issues, special handling, fraud flags, or operational notes.
    /// </remarks>
    /// <example>VIP customer - priority handling, Verified address with customer</example>
    public string? AdminNotes { get; set; }

    /// <summary>
    /// Gets or sets the shipment tracking number.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> containing the carrier tracking number,
    /// or <c>null</c> if not yet shipped.
    /// </value>
    /// <remarks>
    /// Tracking number enables customers to monitor delivery progress.
    /// Format varies by carrier (FedEx, UPS, USPS, DHL, etc.).
    /// </remarks>
    /// <example>1Z999AA10123456784, 9400111899562941174826</example>
    public string? TrackingNumber { get; set; }

    /// <summary>
    /// Gets or sets the estimated delivery date.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> representing the expected delivery date,
    /// or <c>null</c> if not yet determined.
    /// </value>
    /// <remarks>
    /// Estimated based on shipping method, origin, destination, and carrier performance.
    /// Displayed to customers for delivery expectations.
    /// </remarks>
    public DateTime? ExpectedDeliveryDate { get; set; }

    /// <summary>
    /// Gets or sets the actual date and time when the order was delivered.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> in UTC format of successful delivery,
    /// or <c>null</c> if not yet delivered.
    /// </value>
    /// <remarks>
    /// Delivery confirmation triggers order completion workflow,
    /// review requests, and closes the order lifecycle.
    /// </remarks>
    public DateTime? DeliveredAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the order was cancelled.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> in UTC format of cancellation,
    /// or <c>null</c> if the order was not cancelled.
    /// </value>
    /// <remarks>
    /// Cancellation triggers refund processing and inventory restocking.
    /// Orders can only be cancelled before shipping in most cases.
    /// </remarks>
    public DateTime? CancelledAt { get; set; }

    /// <summary>
    /// Gets or sets the reason for order cancellation.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> explaining why the order was cancelled,
    /// or <c>null</c> if not cancelled.
    /// </value>
    /// <remarks>
    /// Cancellation reasons help identify operational issues:
    /// customer request, out of stock, payment failure, fraud detection, etc.
    /// Used for analytics and process improvement.
    /// </remarks>
    /// <example>Customer requested cancellation, Payment declined, Product discontinued</example>
    public string? CancellationReason { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this order has been soft-deleted.
    /// </summary>
    /// <value>
    /// <c>true</c> if the order is deleted but retained for historical purposes;
    /// otherwise, <c>false</c>. Defaults to <c>false</c>.
    /// </value>
    /// <remarks>
    /// Soft delete preserves order history for accounting, reporting, and compliance.
    /// Deleted orders are excluded from normal queries but remain in the database.
    /// </remarks>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Gets or sets the date and time when this order was created.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> in UTC format representing the creation timestamp.
    /// Defaults to <see cref="DateTime.UtcNow"/> when the entity is instantiated.
    /// </value>
    /// <remarks>
    /// Represents when the customer placed the order.
    /// Used for order age calculations, reporting periods, and fulfillment prioritization.
    /// </remarks>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when this order was last updated.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> in UTC format representing the last modification timestamp.
    /// Defaults to <see cref="DateTime.UtcNow"/> when the entity is instantiated.
    /// </value>
    /// <remarks>
    /// Updated whenever order status, address, or other details change.
    /// Helps identify recent order activity and track order progression.
    /// </remarks>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
