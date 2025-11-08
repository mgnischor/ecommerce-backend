using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Interfaces;

/// <summary>
/// Interface for financial transaction operations and cash flow management
/// </summary>
/// <remarks>
/// <para>
/// This service manages financial transactions that track the movement of money
/// in the e-commerce system, providing a comprehensive view beyond accounting journal entries.
/// It handles cash flow, accounts receivable, accounts payable, payment reconciliation,
/// and financial reporting operations.
/// </para>
/// <para>
/// <strong>Key Responsibilities:</strong>
/// </para>
/// <list type="bullet">
/// <item><description>Record financial transactions related to inventory movements</description></item>
/// <item><description>Track customer payments and supplier payments</description></item>
/// <item><description>Manage accounts receivable and accounts payable</description></item>
/// <item><description>Generate cash flow reports and financial dashboards</description></item>
/// <item><description>Reconcile payments with bank statements</description></item>
/// <item><description>Calculate fees, taxes, and net amounts</description></item>
/// </list>
/// <para>
/// <strong>Integration Points:</strong>
/// </para>
/// <list type="bullet">
/// <item><description><see cref="IAccountingService"/>: Links to accounting journal entries</description></item>
/// <item><description><see cref="IInventoryTransactionService"/>: Connects inventory movements to financial impact</description></item>
/// <item><description>Payment processing: Tracks payment provider transactions</description></item>
/// </list>
/// </remarks>
public interface IFinancialService
{
    /// <summary>
    /// Records a financial transaction for a sale
    /// </summary>
    /// <param name="inventoryTransaction">The inventory transaction associated with the sale</param>
    /// <param name="journalEntry">The related journal entry</param>
    /// <param name="createdBy">User ID who initiated the transaction</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created financial transaction entity</returns>
    /// <remarks>
    /// <para>
    /// Creates financial transactions for revenue recognition and accounts receivable.
    /// If payment is received immediately, also creates a customer payment transaction.
    /// </para>
    /// <para>
    /// <strong>Financial Impact:</strong>
    /// </para>
    /// <list type="number">
    /// <item><description>Revenue recognition: Records sale revenue</description></item>
    /// <item><description>Accounts receivable: Creates receivable if not paid immediately</description></item>
    /// <item><description>Customer payment: Records payment if received</description></item>
    /// </list>
    /// </remarks>
    Task<FinancialTransactionEntity> RecordSaleTransactionAsync(
        InventoryTransactionEntity inventoryTransaction,
        JournalEntryEntity journalEntry,
        Guid createdBy,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Records a financial transaction for a purchase
    /// </summary>
    /// <param name="inventoryTransaction">The inventory transaction associated with the purchase</param>
    /// <param name="journalEntry">The related journal entry</param>
    /// <param name="createdBy">User ID who initiated the transaction</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created financial transaction entity</returns>
    /// <remarks>
    /// <para>
    /// Creates financial transactions for purchase expenses and accounts payable.
    /// If payment is made immediately, also creates a supplier payment transaction.
    /// </para>
    /// <para>
    /// <strong>Financial Impact:</strong>
    /// </para>
    /// <list type="number">
    /// <item><description>Purchase expense: Records cost of goods purchased</description></item>
    /// <item><description>Accounts payable: Creates payable if not paid immediately</description></item>
    /// <item><description>Supplier payment: Records payment if made</description></item>
    /// </list>
    /// </remarks>
    Task<FinancialTransactionEntity> RecordPurchaseTransactionAsync(
        InventoryTransactionEntity inventoryTransaction,
        JournalEntryEntity journalEntry,
        Guid createdBy,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Records a customer payment transaction
    /// </summary>
    /// <param name="payment">The payment entity</param>
    /// <param name="orderId">The order ID associated with the payment</param>
    /// <param name="createdBy">User ID who processed the payment</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created financial transaction entity</returns>
    /// <remarks>
    /// <para>
    /// Records cash inflow from customer payment, reducing accounts receivable
    /// and increasing cash/bank balance.
    /// </para>
    /// <para>
    /// Automatically calculates payment processing fees if applicable and creates
    /// a separate fee transaction.
    /// </para>
    /// </remarks>
    Task<FinancialTransactionEntity> RecordCustomerPaymentAsync(
        PaymentEntity payment,
        Guid orderId,
        Guid createdBy,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Records a supplier payment transaction
    /// </summary>
    /// <param name="amount">Payment amount</param>
    /// <param name="currency">Currency code</param>
    /// <param name="supplierName">Name of the supplier</param>
    /// <param name="referenceNumber">Reference or invoice number</param>
    /// <param name="paymentMethod">Payment method used</param>
    /// <param name="inventoryTransactionId">Optional related inventory transaction ID</param>
    /// <param name="createdBy">User ID who made the payment</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created financial transaction entity</returns>
    /// <remarks>
    /// Records cash outflow for supplier payment, reducing accounts payable
    /// and decreasing cash/bank balance.
    /// </remarks>
    Task<FinancialTransactionEntity> RecordSupplierPaymentAsync(
        decimal amount,
        string currency,
        string supplierName,
        string? referenceNumber,
        PaymentMethod paymentMethod,
        Guid? inventoryTransactionId,
        Guid createdBy,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Records a customer refund transaction
    /// </summary>
    /// <param name="originalPayment">The original payment being refunded</param>
    /// <param name="refundAmount">Amount to refund</param>
    /// <param name="reason">Reason for refund</param>
    /// <param name="inventoryTransactionId">Optional related inventory transaction ID (for returns)</param>
    /// <param name="createdBy">User ID who processed the refund</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created financial transaction entity</returns>
    /// <remarks>
    /// Records cash outflow for customer refund, reducing revenue and decreasing
    /// cash/bank balance. May also reverse accounts receivable if applicable.
    /// </remarks>
    Task<FinancialTransactionEntity> RecordCustomerRefundAsync(
        PaymentEntity originalPayment,
        decimal refundAmount,
        string reason,
        Guid? inventoryTransactionId,
        Guid createdBy,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Records an operating expense transaction
    /// </summary>
    /// <param name="amount">Expense amount</param>
    /// <param name="currency">Currency code</param>
    /// <param name="description">Description of the expense</param>
    /// <param name="category">Expense category (shipping, fees, marketing, etc.)</param>
    /// <param name="referenceNumber">Optional reference number</param>
    /// <param name="orderId">Optional related order ID</param>
    /// <param name="createdBy">User ID who recorded the expense</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created financial transaction entity</returns>
    /// <remarks>
    /// Records business operating expenses such as shipping costs, payment processing fees,
    /// marketing expenses, and other operational costs.
    /// </remarks>
    Task<FinancialTransactionEntity> RecordOperatingExpenseAsync(
        decimal amount,
        string currency,
        string description,
        string category,
        string? referenceNumber,
        Guid? orderId,
        Guid createdBy,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Reconciles a financial transaction with bank statement
    /// </summary>
    /// <param name="transactionId">ID of the transaction to reconcile</param>
    /// <param name="reconciledBy">User ID who performed reconciliation</param>
    /// <param name="notes">Optional reconciliation notes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated financial transaction</returns>
    /// <remarks>
    /// Marks a transaction as reconciled after verifying it matches bank records.
    /// Critical for fraud detection and ensuring financial accuracy.
    /// </remarks>
    Task<FinancialTransactionEntity> ReconcileTransactionAsync(
        Guid transactionId,
        Guid reconciledBy,
        string? notes,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves all financial transactions within a date range
    /// </summary>
    /// <param name="startDate">Start date for the period</param>
    /// <param name="endDate">End date for the period</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of financial transactions</returns>
    /// <remarks>
    /// Used for generating financial reports, cash flow statements,
    /// and period-based analysis.
    /// </remarks>
    Task<IEnumerable<FinancialTransactionEntity>> GetTransactionsByPeriodAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves financial transactions by type
    /// </summary>
    /// <param name="transactionType">The type of transactions to retrieve</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of financial transactions</returns>
    /// <remarks>
    /// Filters transactions by type for specific analysis such as
    /// all customer payments, all supplier payments, or all refunds.
    /// </remarks>
    Task<IEnumerable<FinancialTransactionEntity>> GetTransactionsByTypeAsync(
        FinancialTransactionType transactionType,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves unreconciled financial transactions
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of unreconciled transactions</returns>
    /// <remarks>
    /// Returns transactions that need to be matched with bank statements.
    /// Critical for identifying discrepancies and ensuring complete reconciliation.
    /// </remarks>
    Task<IEnumerable<FinancialTransactionEntity>> GetUnreconciledTransactionsAsync(
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Calculates cash flow summary for a period
    /// </summary>
    /// <param name="startDate">Start date for the period</param>
    /// <param name="endDate">End date for the period</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary with cash flow categories and amounts</returns>
    /// <remarks>
    /// <para>
    /// Provides a summary of cash inflows and outflows categorized by transaction type.
    /// Useful for cash flow analysis, forecasting, and financial planning.
    /// </para>
    /// <para>
    /// Returns categories such as:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Total Inflows (revenue, payments received)</description></item>
    /// <item><description>Total Outflows (expenses, payments made, refunds)</description></item>
    /// <item><description>Net Cash Flow (inflows - outflows)</description></item>
    /// <item><description>Breakdown by transaction type</description></item>
    /// </list>
    /// </remarks>
    Task<Dictionary<string, decimal>> GetCashFlowSummaryAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves accounts receivable summary
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary with aging analysis and total receivables</returns>
    /// <remarks>
    /// <para>
    /// Provides analysis of outstanding customer receivables including:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Total accounts receivable</description></item>
    /// <item><description>Aging buckets (0-30 days, 31-60 days, 61-90 days, 90+ days)</description></item>
    /// <item><description>Number of outstanding invoices</description></item>
    /// </list>
    /// </remarks>
    Task<Dictionary<string, decimal>> GetAccountsReceivableSummaryAsync(
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves accounts payable summary
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary with aging analysis and total payables</returns>
    /// <remarks>
    /// <para>
    /// Provides analysis of outstanding supplier payables including:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Total accounts payable</description></item>
    /// <item><description>Aging buckets (0-30 days, 31-60 days, 61-90 days, 90+ days)</description></item>
    /// <item><description>Number of outstanding bills</description></item>
    /// </list>
    /// </remarks>
    Task<Dictionary<string, decimal>> GetAccountsPayableSummaryAsync(
        CancellationToken cancellationToken = default
    );
}
