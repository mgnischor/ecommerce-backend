using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents a payment transaction associated with an order, tracking the complete payment lifecycle
/// from authorization through capture, refund, or failure.
/// </summary>
/// <remarks>
/// Payment entities follow the financial transaction lifecycle:
/// <list type="bullet">
/// <item><description>Authorization: Payment method is validated and funds are reserved</description></item>
/// <item><description>Capture: Funds are transferred from customer to merchant</description></item>
/// <item><description>Refund: Funds are returned to the customer (partial or full)</description></item>
/// <item><description>Failure: Payment attempt is rejected or encounters an error</description></item>
/// </list>
/// Each payment is linked to a specific order and integrates with third-party payment providers.
/// Multiple payment attempts may exist for a single order, but typically only one is successful.
/// <para>
/// The entity stores both internal state (Status) and external provider information (TransactionId, ProviderResponse)
/// to maintain complete audit trails for financial reconciliation and dispute resolution.
/// </para>
/// </remarks>
/// <example>
/// A successful payment flow: Pending → Authorized → Captured
/// A failed payment: Pending → Failed
/// A refunded payment: Captured → Refunded
/// </example>
public sealed class PaymentEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the payment transaction.
    /// </summary>
    /// <value>A GUID that uniquely identifies this payment record in the system.</value>
    /// <remarks>
    /// This is the internal identifier used within the e-commerce system.
    /// Different from <see cref="TransactionId"/> which is the payment provider's identifier.
    /// Used for tracking payments in reports, reconciliation, and customer service operations.
    /// </remarks>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the order for which this payment is being processed.
    /// </summary>
    /// <value>The GUID of the parent order associated with this payment transaction.</value>
    /// <remarks>
    /// Establishes the relationship between the payment and the order being purchased.
    /// An order may have multiple payment attempts (e.g., if the first payment fails),
    /// but typically only one successful payment. Required field - every payment must belong to an order.
    /// Used to:
    /// <list type="bullet">
    /// <item><description>Link payment confirmations to order fulfillment</description></item>
    /// <item><description>Calculate total payments received for an order</description></item>
    /// <item><description>Handle partial payments or payment plan scenarios</description></item>
    /// </list>
    /// </remarks>
    /// <example>a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d</example>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Gets or sets the unique transaction identifier provided by the payment processor.
    /// </summary>
    /// <value>
    /// The external transaction ID from the payment provider (Stripe, PayPal, etc.),
    /// or null if the payment hasn't reached the provider yet.
    /// </value>
    /// <remarks>
    /// This identifier is essential for:
    /// <list type="bullet">
    /// <item><description>Reconciling payments with provider statements</description></item>
    /// <item><description>Processing refunds through the payment provider's API</description></item>
    /// <item><description>Investigating payment disputes or chargebacks</description></item>
    /// <item><description>Verifying payment status with the provider</description></item>
    /// </list>
    /// Format varies by provider. May be null for pending payments or failed attempts
    /// that never reached the payment processor.
    /// </remarks>
    /// <example>pi_3MtwBwLkdIwHu7ix28a3tqPa</example>
    public string? TransactionId { get; set; }

    /// <summary>
    /// Gets or sets the payment method used for this transaction.
    /// </summary>
    /// <value>
    /// An enumeration value representing the payment method type.
    /// Default value is <see cref="PaymentMethod.NotSpecified"/>.
    /// </value>
    /// <remarks>
    /// Common payment methods include:
    /// <list type="bullet">
    /// <item><description>Credit/Debit cards (Visa, Mastercard, Amex)</description></item>
    /// <item><description>Digital wallets (PayPal, Apple Pay, Google Pay)</description></item>
    /// <item><description>Bank transfers and direct debits</description></item>
    /// <item><description>Buy now, pay later services (Klarna, Affirm)</description></item>
    /// <item><description>Cryptocurrency payments</description></item>
    /// </list>
    /// The payment method affects processing fees, settlement times, and refund capabilities.
    /// </remarks>
    /// <example>PaymentMethod.CreditCard</example>
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.NotSpecified;

    /// <summary>
    /// Gets or sets the current status of the payment transaction.
    /// </summary>
    /// <value>
    /// An enumeration value representing the payment's current state in its lifecycle.
    /// Default value is <see cref="PaymentStatus.Pending"/>.
    /// </value>
    /// <remarks>
    /// Payment status lifecycle:
    /// <list type="bullet">
    /// <item><description><b>Pending</b>: Payment initiated but not yet processed</description></item>
    /// <item><description><b>Authorized</b>: Payment method validated, funds reserved but not captured</description></item>
    /// <item><description><b>Captured/Completed</b>: Funds successfully transferred to merchant</description></item>
    /// <item><description><b>Failed</b>: Payment attempt rejected or encountered an error</description></item>
    /// <item><description><b>Refunded</b>: Funds returned to customer (check <see cref="RefundAmount"/>)</description></item>
    /// <item><description><b>Cancelled</b>: Payment attempt was cancelled before completion</description></item>
    /// </list>
    /// Status changes should be logged with corresponding timestamp fields
    /// (<see cref="AuthorizedAt"/>, <see cref="CapturedAt"/>, <see cref="RefundedAt"/>).
    /// </remarks>
    /// <example>PaymentStatus.Captured</example>
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    /// <summary>
    /// Gets or sets the monetary amount paid or attempted to be paid in this transaction.
    /// </summary>
    /// <value>
    /// The payment amount in the currency specified by <see cref="Currency"/>.
    /// Must be a positive value matching or not exceeding the order total.
    /// </value>
    /// <remarks>
    /// This amount should match the order total unless:
    /// <list type="bullet">
    /// <item><description>Partial payments are supported</description></item>
    /// <item><description>Multiple payment methods are used for a single order</description></item>
    /// <item><description>Payment plan or installment options are available</description></item>
    /// </list>
    /// For refunds, see <see cref="RefundAmount"/> which may differ from the original amount
    /// in cases of partial refunds or restocking fees.
    /// Amount precision should match currency standards (typically 2 decimal places for most currencies).
    /// </remarks>
    /// <example>149.99</example>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the three-letter ISO 4217 currency code for this payment.
    /// </summary>
    /// <value>
    /// The currency code in which the payment amount is denominated.
    /// Default value is "USD" (United States Dollar).
    /// </value>
    /// <remarks>
    /// Must be a valid ISO 4217 currency code. Common examples:
    /// <list type="bullet">
    /// <item><description>USD - United States Dollar</description></item>
    /// <item><description>EUR - Euro</description></item>
    /// <item><description>GBP - British Pound Sterling</description></item>
    /// <item><description>BRL - Brazilian Real</description></item>
    /// <item><description>JPY - Japanese Yen (typically 0 decimal places)</description></item>
    /// </list>
    /// Currency must match the order currency and should be supported by the payment provider.
    /// Different currencies have different decimal place requirements and processing considerations.
    /// </remarks>
    /// <example>USD</example>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Gets or sets the name of the payment service provider that processed this transaction.
    /// </summary>
    /// <value>
    /// The payment processor or gateway name, or null if not yet assigned to a provider.
    /// </value>
    /// <remarks>
    /// Common payment providers include:
    /// <list type="bullet">
    /// <item><description>Stripe - Popular for card payments and digital wallets</description></item>
    /// <item><description>PayPal - Digital wallet and card processing</description></item>
    /// <item><description>Square - Point of sale and online payments</description></item>
    /// <item><description>Mercado Pago - Latin American payment solutions</description></item>
    /// <item><description>Adyen - Enterprise payment platform</description></item>
    /// <item><description>Braintree - PayPal-owned payment gateway</description></item>
    /// </list>
    /// Used for routing refunds, webhooks, and API calls to the correct provider.
    /// Multiple providers may be supported, with selection based on payment method, region, or customer preference.
    /// </remarks>
    /// <example>Stripe</example>
    public string? PaymentProvider { get; set; }

    /// <summary>
    /// Gets or sets the raw response data received from the payment provider.
    /// </summary>
    /// <value>
    /// The complete response from the payment provider, typically in JSON format.
    /// May be null for pending payments or those that haven't reached the provider.
    /// </value>
    /// <remarks>
    /// Stores the full provider response for:
    /// <list type="bullet">
    /// <item><description>Debugging payment issues and provider integration problems</description></item>
    /// <item><description>Audit trails and compliance requirements</description></item>
    /// <item><description>Extracting additional metadata not captured in other fields</description></item>
    /// <item><description>Investigating disputes, chargebacks, or fraud claims</description></item>
    /// </list>
    /// Response format varies by provider but is typically JSON containing transaction details,
    /// risk scores, authentication results (3D Secure), and metadata.
    /// Should be sanitized to remove sensitive data (full card numbers, CVV) before storage.
    /// </remarks>
    /// <example>{"id":"pi_3MtwBw...","status":"succeeded","amount":14999,"currency":"usd"}</example>
    public string? ProviderResponse { get; set; }

    /// <summary>
    /// Gets or sets the error message describing why a payment failed.
    /// </summary>
    /// <value>
    /// A human-readable error message, or null if the payment was successful or is still pending.
    /// </value>
    /// <remarks>
    /// Error messages help identify payment failure reasons:
    /// <list type="bullet">
    /// <item><description>Insufficient funds in customer account</description></item>
    /// <item><description>Card declined by issuing bank</description></item>
    /// <item><description>Expired or invalid payment method</description></item>
    /// <item><description>Fraud detection or risk rules triggered</description></item>
    /// <item><description>Technical errors or provider downtime</description></item>
    /// <item><description>Authentication failures (3D Secure, CVV mismatch)</description></item>
    /// </list>
    /// Used for customer support, retry logic, and payment analytics.
    /// Should be clear enough for customer service but avoid exposing sensitive security details.
    /// </remarks>
    /// <example>Your card was declined. Please try another payment method.</example>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the payment was authorized by the payment provider.
    /// </summary>
    /// <value>
    /// The UTC timestamp when funds were reserved/authorized, or null if not yet authorized.
    /// </value>
    /// <remarks>
    /// Authorization means:
    /// <list type="bullet">
    /// <item><description>Payment method was validated (card is active, has sufficient funds)</description></item>
    /// <item><description>Funds are reserved but not yet transferred (hold placed on account)</description></item>
    /// <item><description>Authorization may expire if not captured within a timeframe (typically 7 days)</description></item>
    /// </list>
    /// The time between authorization and capture varies by business model:
    /// <list type="bullet">
    /// <item><description>Immediate capture: authorize and capture in same transaction</description></item>
    /// <item><description>Delayed capture: authorize at order, capture at shipment</description></item>
    /// </list>
    /// Important for inventory management and order fulfillment workflows.
    /// </remarks>
    /// <example>2024-01-15T10:30:00Z</example>
    public DateTime? AuthorizedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the payment was captured and funds were transferred.
    /// </summary>
    /// <value>
    /// The UTC timestamp when funds were actually transferred from customer to merchant,
    /// or null if not yet captured.
    /// </value>
    /// <remarks>
    /// Capture represents the actual movement of money and triggers:
    /// <list type="bullet">
    /// <item><description>Funds settlement into merchant account (may take 1-3 business days)</description></item>
    /// <item><description>Order fulfillment and shipping processes</description></item>
    /// <item><description>Invoice and receipt generation</description></item>
    /// <item><description>Revenue recognition for accounting purposes</description></item>
    /// </list>
    /// Some payment methods (e.g., PayPal, digital wallets) may combine authorization and capture.
    /// The capture timestamp is critical for financial reporting and reconciliation.
    /// Once captured, refund process must be used to return funds.
    /// </remarks>
    /// <example>2024-01-15T10:35:22Z</example>
    public DateTime? CapturedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the payment was refunded to the customer.
    /// </summary>
    /// <value>
    /// The UTC timestamp when the refund was initiated, or null if no refund has been processed.
    /// </value>
    /// <remarks>
    /// Refund processing typically takes 5-10 business days to appear in customer's account.
    /// Multiple partial refunds may occur for a single payment, tracked by <see cref="RefundAmount"/>.
    /// Refund triggers:
    /// <list type="bullet">
    /// <item><description>Customer returns or cancels order</description></item>
    /// <item><description>Product defects or service issues</description></item>
    /// <item><description>Merchant error (wrong item, pricing mistake)</description></item>
    /// <item><description>Fraud prevention or dispute resolution</description></item>
    /// </list>
    /// Check <see cref="RefundReason"/> for the explanation of why the refund was issued.
    /// Refunded payments should have Status updated to reflect the refund state.
    /// </remarks>
    /// <example>2024-01-20T14:20:00Z</example>
    public DateTime? RefundedAt { get; set; }

    /// <summary>
    /// Gets or sets the amount of money refunded to the customer.
    /// </summary>
    /// <value>
    /// The refund amount in the same currency as the original payment,
    /// or null if no refund has been issued.
    /// </value>
    /// <remarks>
    /// Refund amount may differ from the original <see cref="Amount"/>:
    /// <list type="bullet">
    /// <item><description><b>Partial refund</b>: Customer keeps some items, returns others</description></item>
    /// <item><description><b>Full refund</b>: Entire order returned or cancelled</description></item>
    /// <item><description><b>Refund with deductions</b>: Restocking fees, return shipping costs</description></item>
    /// </list>
    /// Multiple partial refunds can be issued; this field should track the cumulative refund amount.
    /// RefundAmount should never exceed the original Amount.
    /// Payment processor fees are typically not refunded to the merchant.
    /// </remarks>
    /// <example>149.99</example>
    public decimal? RefundAmount { get; set; }

    /// <summary>
    /// Gets or sets the reason why the payment was refunded.
    /// </summary>
    /// <value>
    /// A description of why the refund was issued, or null if no refund has been processed.
    /// </value>
    /// <remarks>
    /// Common refund reasons include:
    /// <list type="bullet">
    /// <item><description>Customer requested cancellation</description></item>
    /// <item><description>Product returned - defective or damaged</description></item>
    /// <item><description>Product returned - customer changed mind</description></item>
    /// <item><description>Order cancelled by merchant (out of stock, unable to fulfill)</description></item>
    /// <item><description>Duplicate charge or billing error</description></item>
    /// <item><description>Fraudulent transaction detected</description></item>
    /// <item><description>Goodwill gesture or customer service resolution</description></item>
    /// </list>
    /// Used for analytics, fraud detection, and identifying product quality issues.
    /// Should be specific enough for operational insights but professional for customer visibility.
    /// </remarks>
    /// <example>Customer returned product - did not meet expectations</example>
    public string? RefundReason { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the payment record was created.
    /// </summary>
    /// <value>
    /// The UTC timestamp when this payment entity was first created in the system.
    /// Default value is the current UTC time when the entity is instantiated.
    /// </value>
    /// <remarks>
    /// Represents when the payment attempt was initiated, not when it was authorized or captured.
    /// Used for:
    /// <list type="bullet">
    /// <item><description>Calculating payment processing time and performance metrics</description></item>
    /// <item><description>Audit trails and compliance reporting</description></item>
    /// <item><description>Identifying payment timing patterns for fraud detection</description></item>
    /// </list>
    /// Should not be modified after initial creation.
    /// Compare with <see cref="AuthorizedAt"/> and <see cref="CapturedAt"/> to understand the payment timeline.
    /// </remarks>
    /// <example>2024-01-15T10:29:45Z</example>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the payment record was last updated.
    /// </summary>
    /// <value>
    /// The UTC timestamp of the most recent modification to this payment record.
    /// Default value is the current UTC time when the entity is instantiated.
    /// </value>
    /// <remarks>
    /// Updated whenever any payment property changes, including:
    /// <list type="bullet">
    /// <item><description>Status transitions (pending → authorized → captured)</description></item>
    /// <item><description>Provider response updates</description></item>
    /// <item><description>Error message additions</description></item>
    /// <item><description>Refund processing</description></item>
    /// </list>
    /// Initially set to the same value as <see cref="CreatedAt"/>.
    /// Useful for tracking when the most recent status change occurred and troubleshooting timing issues.
    /// Helps identify stale payment records that may need investigation.
    /// </remarks>
    /// <example>2024-01-15T10:35:22Z</example>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
