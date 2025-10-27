namespace ECommerce.Domain.Enums;

/// <summary>
/// Represents the payment method used for an order
/// </summary>
public enum PaymentMethod
{
    /// <summary>
    /// Not specified
    /// </summary>
    NotSpecified = 0,

    /// <summary>
    /// Credit card payment
    /// </summary>
    CreditCard = 1,

    /// <summary>
    /// Debit card payment
    /// </summary>
    DebitCard = 2,

    /// <summary>
    /// PayPal payment
    /// </summary>
    PayPal = 3,

    /// <summary>
    /// Bank transfer
    /// </summary>
    BankTransfer = 4,

    /// <summary>
    /// Cash on delivery
    /// </summary>
    CashOnDelivery = 5,

    /// <summary>
    /// PIX (Brazilian instant payment)
    /// </summary>
    Pix = 6,

    /// <summary>
    /// Boleto (Brazilian payment slip)
    /// </summary>
    Boleto = 7,

    /// <summary>
    /// Cryptocurrency
    /// </summary>
    Cryptocurrency = 8,

    /// <summary>
    /// Store credit or gift card
    /// </summary>
    StoreCredit = 9
}
