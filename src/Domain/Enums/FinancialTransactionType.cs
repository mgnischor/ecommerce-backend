namespace ECommerce.Domain.Enums;

/// <summary>
/// Represents the type of financial transaction
/// </summary>
/// <remarks>
/// <para>
/// Financial transactions track the movement of money in the e-commerce system,
/// providing a comprehensive view of cash flow, accounts receivable, and accounts payable.
/// Each transaction type has specific accounting and financial reporting implications.
/// </para>
/// <para>
/// <strong>Cash Flow Impact:</strong>
/// </para>
/// <list type="bullet">
/// <item><description><strong>Inflows:</strong> CustomerPayment, SaleRevenue, RefundReversal</description></item>
/// <item><description><strong>Outflows:</strong> SupplierPayment, PurchasePayment, CustomerRefund, OperatingExpense</description></item>
/// <item><description><strong>Non-Cash:</strong> AccountsReceivable, AccountsPayable (accrual basis)</description></item>
/// </list>
/// </remarks>
public enum FinancialTransactionType
{
    /// <summary>
    /// Payment received from customer for an order
    /// </summary>
    /// <remarks>
    /// Increases cash/bank balance and reduces accounts receivable.
    /// Debit: Cash/Bank, Credit: Accounts Receivable
    /// </remarks>
    CustomerPayment = 1,

    /// <summary>
    /// Payment made to supplier for inventory purchase
    /// </summary>
    /// <remarks>
    /// Decreases cash/bank balance and reduces accounts payable.
    /// Debit: Accounts Payable, Credit: Cash/Bank
    /// </remarks>
    SupplierPayment = 2,

    /// <summary>
    /// Accounts receivable created from a sale
    /// </summary>
    /// <remarks>
    /// Records revenue recognition when goods are sold on credit.
    /// Debit: Accounts Receivable, Credit: Sales Revenue
    /// </remarks>
    AccountsReceivable = 3,

    /// <summary>
    /// Accounts payable created from a purchase
    /// </summary>
    /// <remarks>
    /// Records obligation to pay supplier for purchased inventory.
    /// Debit: Inventory, Credit: Accounts Payable
    /// </remarks>
    AccountsPayable = 4,

    /// <summary>
    /// Refund issued to customer
    /// </summary>
    /// <remarks>
    /// Decreases cash/bank balance and reduces revenue.
    /// Debit: Sales Returns/Revenue, Credit: Cash/Bank
    /// </remarks>
    CustomerRefund = 5,

    /// <summary>
    /// Revenue recognized from sale
    /// </summary>
    /// <remarks>
    /// Records income from product sales, separate from cash collection.
    /// Debit: Accounts Receivable/Cash, Credit: Sales Revenue
    /// </remarks>
    SaleRevenue = 6,

    /// <summary>
    /// Expense incurred for inventory purchase
    /// </summary>
    /// <remarks>
    /// Records the cost associated with purchasing inventory.
    /// Debit: Purchases/Inventory, Credit: Cash/Accounts Payable
    /// </remarks>
    PurchaseExpense = 7,

    /// <summary>
    /// Operating expense payment (shipping, fees, etc.)
    /// </summary>
    /// <remarks>
    /// Tracks business operating expenses reducing profitability.
    /// Debit: Operating Expenses, Credit: Cash/Bank
    /// </remarks>
    OperatingExpense = 8,

    /// <summary>
    /// Payment processing fee
    /// </summary>
    /// <remarks>
    /// Records fees charged by payment processors (Stripe, PayPal, etc.).
    /// Debit: Payment Processing Fees (Expense), Credit: Cash/Bank
    /// </remarks>
    PaymentFee = 9,

    /// <summary>
    /// Shipping cost for order delivery
    /// </summary>
    /// <remarks>
    /// Tracks shipping and logistics expenses.
    /// Debit: Shipping Expense, Credit: Cash/Accounts Payable
    /// </remarks>
    ShippingCost = 10,

    /// <summary>
    /// Tax payment or collection
    /// </summary>
    /// <remarks>
    /// Records sales tax collected or paid.
    /// Collection: Debit: Cash, Credit: Sales Tax Payable
    /// Payment: Debit: Sales Tax Payable, Credit: Cash
    /// </remarks>
    TaxTransaction = 11,

    /// <summary>
    /// Discount applied to sale
    /// </summary>
    /// <remarks>
    /// Reduces revenue due to promotional discounts or coupons.
    /// Debit: Sales Discounts (Contra-Revenue), Credit: Accounts Receivable
    /// </remarks>
    SalesDiscount = 12,

    /// <summary>
    /// Adjustment to financial records
    /// </summary>
    /// <remarks>
    /// Corrects errors or adjusts balances (write-offs, bad debts, etc.).
    /// Impact varies depending on adjustment reason.
    /// </remarks>
    Adjustment = 13,

    /// <summary>
    /// Transfer between bank accounts
    /// </summary>
    /// <remarks>
    /// Internal transfer with no net impact on total cash position.
    /// Debit: Destination Account, Credit: Source Account
    /// </remarks>
    BankTransfer = 14,

    /// <summary>
    /// Commission or affiliate payment
    /// </summary>
    /// <remarks>
    /// Payment to sales affiliates, influencers, or commission-based partners.
    /// Debit: Commission Expense, Credit: Cash/Accounts Payable
    /// </remarks>
    CommissionPayment = 15,
}
