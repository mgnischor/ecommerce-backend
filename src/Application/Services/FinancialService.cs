using System.Diagnostics;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Interfaces;

namespace ECommerce.Application.Services;

/// <summary>
/// Service for managing financial transactions and cash flow operations
/// </summary>
/// <remarks>
/// <para>
/// Implements comprehensive financial transaction management integrating inventory movements,
/// payments, accounts receivable, accounts payable, and cash flow tracking.
/// Follows Brazilian GAAP (NBC TG) and international accounting standards.
/// </para>
/// <para>
/// <strong>Core Principles:</strong>
/// </para>
/// <list type="bullet">
/// <item><description>Every inventory movement generates corresponding financial transaction(s)</description></item>
/// <item><description>Accrual basis accounting: revenue/expenses recognized when earned/incurred</description></item>
/// <item><description>Cash basis tracking: actual money movements recorded separately</description></item>
/// <item><description>Complete traceability: all transactions linked to source documents</description></item>
/// </list>
/// </remarks>
public class FinancialService : IFinancialService
{
    private readonly IFinancialTransactionRepository _financialTransactionRepository;
    private readonly ILoggingService _logger;
    private static int _transactionCounter = 0;
    private static readonly object _lock = new object();

    // OpenTelemetry activity source for custom tracing
    private static readonly ActivitySource ActivitySource = new(
        "ECommerce.Application.Financial",
        "1.0.0"
    );

    // Payment processing fee rates by provider (percentage)
    private const decimal STRIPE_FEE_RATE = 0.029m; // 2.9% + $0.30
    private const decimal STRIPE_FEE_FIXED = 0.30m;
    private const decimal PAYPAL_FEE_RATE = 0.0349m; // 3.49% + $0.49
    private const decimal PAYPAL_FEE_FIXED = 0.49m;
    private const decimal DEFAULT_FEE_RATE = 0.03m; // 3%

    /// <summary>
    /// Initializes a new instance of the <see cref="FinancialService"/> class
    /// </summary>
    /// <param name="financialTransactionRepository">Repository for financial transaction data access</param>
    /// <param name="logger">Logger for tracking operations and errors</param>
    /// <exception cref="ArgumentNullException">Thrown when any required dependency is null</exception>
    public FinancialService(
        IFinancialTransactionRepository financialTransactionRepository,
        ILoggingService logger
    )
    {
        _financialTransactionRepository =
            financialTransactionRepository
            ?? throw new ArgumentNullException(nameof(financialTransactionRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<FinancialTransactionEntity> RecordSaleTransactionAsync(
        InventoryTransactionEntity inventoryTransaction,
        JournalEntryEntity journalEntry,
        Guid createdBy,
        CancellationToken cancellationToken = default
    )
    {
        using var activity = ActivitySource.StartActivity("RecordSaleTransaction");

        _logger.LogInformation(
            "Recording sale financial transaction for inventory transaction {TransactionId}",
            inventoryTransaction.Id
        );

        // Create revenue recognition transaction
        var revenueTransaction = new FinancialTransactionEntity
        {
            Id = Guid.NewGuid(),
            TransactionNumber = GenerateTransactionNumber(),
            TransactionType = FinancialTransactionType.SaleRevenue,
            Amount = inventoryTransaction.TotalCost, // Positive for revenue
            Currency = "USD",
            TransactionDate = inventoryTransaction.TransactionDate,
            Description =
                $"Sale revenue - {inventoryTransaction.ProductName} (Qty: {Math.Abs(inventoryTransaction.Quantity)})",
            OrderId = inventoryTransaction.OrderId,
            InventoryTransactionId = inventoryTransaction.Id,
            JournalEntryId = journalEntry.Id,
            ProductId = inventoryTransaction.ProductId,
            ReferenceNumber = inventoryTransaction.DocumentNumber,
            Status = "Completed",
            NetAmount = inventoryTransaction.TotalCost,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await _financialTransactionRepository.AddAsync(revenueTransaction, cancellationToken);

        // Create accounts receivable transaction (will be reduced when payment is received)
        var arTransaction = new FinancialTransactionEntity
        {
            Id = Guid.NewGuid(),
            TransactionNumber = GenerateTransactionNumber(),
            TransactionType = FinancialTransactionType.AccountsReceivable,
            Amount = inventoryTransaction.TotalCost, // Positive for receivable
            Currency = "USD",
            TransactionDate = inventoryTransaction.TransactionDate,
            Description = $"Accounts receivable - Order {inventoryTransaction.OrderId}",
            OrderId = inventoryTransaction.OrderId,
            InventoryTransactionId = inventoryTransaction.Id,
            JournalEntryId = journalEntry.Id,
            ProductId = inventoryTransaction.ProductId,
            ReferenceNumber = inventoryTransaction.DocumentNumber,
            Status = "Pending", // Pending payment
            NetAmount = inventoryTransaction.TotalCost,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await _financialTransactionRepository.AddAsync(arTransaction, cancellationToken);
        await _financialTransactionRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created sale financial transactions: Revenue={RevenueId}, AR={ARId}",
            revenueTransaction.Id,
            arTransaction.Id
        );

        return revenueTransaction;
    }

    /// <inheritdoc />
    public async Task<FinancialTransactionEntity> RecordPurchaseTransactionAsync(
        InventoryTransactionEntity inventoryTransaction,
        JournalEntryEntity journalEntry,
        Guid createdBy,
        CancellationToken cancellationToken = default
    )
    {
        using var activity = ActivitySource.StartActivity("RecordPurchaseTransaction");

        _logger.LogInformation(
            "Recording purchase financial transaction for inventory transaction {TransactionId}",
            inventoryTransaction.Id
        );

        // Create purchase expense transaction
        var expenseTransaction = new FinancialTransactionEntity
        {
            Id = Guid.NewGuid(),
            TransactionNumber = GenerateTransactionNumber(),
            TransactionType = FinancialTransactionType.PurchaseExpense,
            Amount = -inventoryTransaction.TotalCost, // Negative for expense
            Currency = "USD",
            TransactionDate = inventoryTransaction.TransactionDate,
            Description =
                $"Purchase expense - {inventoryTransaction.ProductName} (Qty: {inventoryTransaction.Quantity})",
            InventoryTransactionId = inventoryTransaction.Id,
            JournalEntryId = journalEntry.Id,
            ProductId = inventoryTransaction.ProductId,
            ReferenceNumber = inventoryTransaction.DocumentNumber,
            Status = "Completed",
            NetAmount = -inventoryTransaction.TotalCost,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await _financialTransactionRepository.AddAsync(expenseTransaction, cancellationToken);

        // Create accounts payable transaction (will be reduced when payment is made)
        var apTransaction = new FinancialTransactionEntity
        {
            Id = Guid.NewGuid(),
            TransactionNumber = GenerateTransactionNumber(),
            TransactionType = FinancialTransactionType.AccountsPayable,
            Amount = inventoryTransaction.TotalCost, // Positive for liability
            Currency = "USD",
            TransactionDate = inventoryTransaction.TransactionDate,
            Description = $"Accounts payable - Purchase {inventoryTransaction.TransactionNumber}",
            InventoryTransactionId = inventoryTransaction.Id,
            JournalEntryId = journalEntry.Id,
            ProductId = inventoryTransaction.ProductId,
            ReferenceNumber = inventoryTransaction.DocumentNumber,
            Status = "Pending", // Pending payment
            NetAmount = inventoryTransaction.TotalCost,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await _financialTransactionRepository.AddAsync(apTransaction, cancellationToken);
        await _financialTransactionRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created purchase financial transactions: Expense={ExpenseId}, AP={APId}",
            expenseTransaction.Id,
            apTransaction.Id
        );

        return expenseTransaction;
    }

    /// <inheritdoc />
    public async Task<FinancialTransactionEntity> RecordCustomerPaymentAsync(
        PaymentEntity payment,
        Guid orderId,
        Guid createdBy,
        CancellationToken cancellationToken = default
    )
    {
        using var activity = ActivitySource.StartActivity("RecordCustomerPayment");

        _logger.LogInformation(
            "Recording customer payment {PaymentId} for order {OrderId}",
            payment.Id,
            orderId
        );

        // Calculate payment processing fee
        var (feeAmount, netAmount) = CalculatePaymentFee(
            payment.Amount,
            payment.PaymentMethod,
            payment.PaymentProvider
        );

        // Create customer payment transaction
        var paymentTransaction = new FinancialTransactionEntity
        {
            Id = Guid.NewGuid(),
            TransactionNumber = GenerateTransactionNumber(),
            TransactionType = FinancialTransactionType.CustomerPayment,
            Amount = payment.Amount, // Positive for cash inflow
            Currency = payment.Currency,
            TransactionDate = payment.CapturedAt ?? DateTime.UtcNow,
            Description = $"Payment received for Order #{orderId}",
            OrderId = orderId,
            PaymentId = payment.Id,
            ReferenceNumber = payment.TransactionId,
            PaymentMethod = payment.PaymentMethod,
            PaymentProvider = payment.PaymentProvider,
            Status = payment.Status == PaymentStatus.Completed ? "Completed" : "Pending",
            TaxAmount = 0m,
            FeeAmount = feeAmount,
            NetAmount = netAmount,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await _financialTransactionRepository.AddAsync(paymentTransaction, cancellationToken);

        // If there's a processing fee, record it as an operating expense
        if (feeAmount > 0)
        {
            var feeTransaction = new FinancialTransactionEntity
            {
                Id = Guid.NewGuid(),
                TransactionNumber = GenerateTransactionNumber(),
                TransactionType = FinancialTransactionType.PaymentFee,
                Amount = -feeAmount, // Negative for expense
                Currency = payment.Currency,
                TransactionDate = payment.CapturedAt ?? DateTime.UtcNow,
                Description = $"Payment processing fee - {payment.PaymentProvider ?? "Provider"}",
                OrderId = orderId,
                PaymentId = payment.Id,
                PaymentProvider = payment.PaymentProvider,
                Status = "Completed",
                NetAmount = -feeAmount,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            await _financialTransactionRepository.AddAsync(feeTransaction, cancellationToken);

            _logger.LogInformation(
                "Recorded payment processing fee: {FeeAmount} for provider {Provider}",
                feeAmount,
                payment.PaymentProvider ?? "Unknown"
            );
        }

        await _financialTransactionRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created customer payment transaction: {TransactionId}, Amount={Amount}, Fee={FeeAmount}",
            paymentTransaction.Id,
            payment.Amount,
            feeAmount
        );

        return paymentTransaction;
    }

    /// <inheritdoc />
    public async Task<FinancialTransactionEntity> RecordSupplierPaymentAsync(
        decimal amount,
        string currency,
        string supplierName,
        string? referenceNumber,
        PaymentMethod paymentMethod,
        Guid? inventoryTransactionId,
        Guid createdBy,
        CancellationToken cancellationToken = default
    )
    {
        using var activity = ActivitySource.StartActivity("RecordSupplierPayment");

        _logger.LogInformation(
            "Recording supplier payment: Amount={Amount}, Supplier={Supplier}",
            amount,
            supplierName
        );

        var paymentTransaction = new FinancialTransactionEntity
        {
            Id = Guid.NewGuid(),
            TransactionNumber = GenerateTransactionNumber(),
            TransactionType = FinancialTransactionType.SupplierPayment,
            Amount = -amount, // Negative for cash outflow
            Currency = currency,
            TransactionDate = DateTime.UtcNow,
            Description = $"Payment to supplier - {supplierName}",
            Counterparty = supplierName,
            InventoryTransactionId = inventoryTransactionId,
            ReferenceNumber = referenceNumber,
            PaymentMethod = paymentMethod,
            Status = "Completed",
            NetAmount = -amount,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await _financialTransactionRepository.AddAsync(paymentTransaction, cancellationToken);
        await _financialTransactionRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created supplier payment transaction: {TransactionId}",
            paymentTransaction.Id
        );

        return paymentTransaction;
    }

    /// <inheritdoc />
    public async Task<FinancialTransactionEntity> RecordCustomerRefundAsync(
        PaymentEntity originalPayment,
        decimal refundAmount,
        string reason,
        Guid? inventoryTransactionId,
        Guid createdBy,
        CancellationToken cancellationToken = default
    )
    {
        using var activity = ActivitySource.StartActivity("RecordCustomerRefund");

        _logger.LogInformation(
            "Recording customer refund: Amount={Amount}, Reason={Reason}",
            refundAmount,
            reason
        );

        var refundTransaction = new FinancialTransactionEntity
        {
            Id = Guid.NewGuid(),
            TransactionNumber = GenerateTransactionNumber(),
            TransactionType = FinancialTransactionType.CustomerRefund,
            Amount = -refundAmount, // Negative for cash outflow
            Currency = originalPayment.Currency,
            TransactionDate = DateTime.UtcNow,
            Description = $"Refund issued - {reason}",
            OrderId = originalPayment.OrderId,
            PaymentId = originalPayment.Id,
            InventoryTransactionId = inventoryTransactionId,
            ReferenceNumber = originalPayment.TransactionId,
            PaymentMethod = originalPayment.PaymentMethod,
            PaymentProvider = originalPayment.PaymentProvider,
            Status = "Completed",
            NetAmount = -refundAmount,
            Notes = reason,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await _financialTransactionRepository.AddAsync(refundTransaction, cancellationToken);
        await _financialTransactionRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created customer refund transaction: {TransactionId}",
            refundTransaction.Id
        );

        return refundTransaction;
    }

    /// <inheritdoc />
    public async Task<FinancialTransactionEntity> RecordOperatingExpenseAsync(
        decimal amount,
        string currency,
        string description,
        string category,
        string? referenceNumber,
        Guid? orderId,
        Guid createdBy,
        CancellationToken cancellationToken = default
    )
    {
        using var activity = ActivitySource.StartActivity("RecordOperatingExpense");

        _logger.LogInformation(
            "Recording operating expense: Amount={Amount}, Category={Category}",
            amount,
            category
        );

        // Determine transaction type based on category
        var transactionType = category.ToLowerInvariant() switch
        {
            "shipping" or "freight" or "delivery" => FinancialTransactionType.ShippingCost,
            "tax" or "taxes" => FinancialTransactionType.TaxTransaction,
            "discount" or "promotion" => FinancialTransactionType.SalesDiscount,
            "commission" or "affiliate" => FinancialTransactionType.CommissionPayment,
            _ => FinancialTransactionType.OperatingExpense,
        };

        var expenseTransaction = new FinancialTransactionEntity
        {
            Id = Guid.NewGuid(),
            TransactionNumber = GenerateTransactionNumber(),
            TransactionType = transactionType,
            Amount = -amount, // Negative for expense
            Currency = currency,
            TransactionDate = DateTime.UtcNow,
            Description = $"{category} - {description}",
            OrderId = orderId,
            ReferenceNumber = referenceNumber,
            Status = "Completed",
            NetAmount = -amount,
            Notes = category,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await _financialTransactionRepository.AddAsync(expenseTransaction, cancellationToken);
        await _financialTransactionRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created operating expense transaction: {TransactionId}",
            expenseTransaction.Id
        );

        return expenseTransaction;
    }

    /// <inheritdoc />
    public async Task<FinancialTransactionEntity> ReconcileTransactionAsync(
        Guid transactionId,
        Guid reconciledBy,
        string? notes,
        CancellationToken cancellationToken = default
    )
    {
        using var activity = ActivitySource.StartActivity("ReconcileTransaction");

        _logger.LogInformation("Reconciling financial transaction: {TransactionId}", transactionId);

        var transaction = await _financialTransactionRepository.GetByIdAsync(
            transactionId,
            cancellationToken
        );

        if (transaction == null)
        {
            throw new InvalidOperationException($"Financial transaction {transactionId} not found");
        }

        if (transaction.IsReconciled)
        {
            _logger.LogWarning("Transaction {TransactionId} is already reconciled", transactionId);
            return transaction;
        }

        transaction.IsReconciled = true;
        transaction.ReconciledAt = DateTime.UtcNow;
        transaction.ReconciledBy = reconciledBy;
        transaction.Notes = string.IsNullOrEmpty(transaction.Notes)
            ? notes
            : $"{transaction.Notes}\nReconciliation: {notes}";
        transaction.UpdatedAt = DateTime.UtcNow;

        await _financialTransactionRepository.UpdateAsync(transaction, cancellationToken);
        await _financialTransactionRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Transaction {TransactionId} reconciled by user {UserId}",
            transactionId,
            reconciledBy
        );

        return transaction;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<FinancialTransactionEntity>> GetTransactionsByPeriodAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogDebug(
            "Retrieving transactions for period: {StartDate} to {EndDate}",
            startDate,
            endDate
        );

        return await _financialTransactionRepository.GetByPeriodAsync(
            startDate,
            endDate,
            cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<IEnumerable<FinancialTransactionEntity>> GetTransactionsByTypeAsync(
        FinancialTransactionType transactionType,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogDebug("Retrieving transactions by type: {TransactionType}", transactionType);

        var allTransactions = await _financialTransactionRepository.GetAllAsync(cancellationToken);

        var filtered = allTransactions.Where(t => t.TransactionType == transactionType);

        if (startDate.HasValue)
        {
            filtered = filtered.Where(t => t.TransactionDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            filtered = filtered.Where(t => t.TransactionDate <= endDate.Value);
        }

        return filtered.OrderByDescending(t => t.TransactionDate);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<FinancialTransactionEntity>> GetUnreconciledTransactionsAsync(
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogDebug("Retrieving unreconciled transactions");

        return await _financialTransactionRepository.GetUnreconciledAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, decimal>> GetCashFlowSummaryAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation(
            "Calculating cash flow summary for period: {StartDate} to {EndDate}",
            startDate,
            endDate
        );

        var transactions = await _financialTransactionRepository.GetByPeriodAsync(
            startDate,
            endDate,
            cancellationToken
        );

        var summary = new Dictionary<string, decimal>
        {
            ["TotalInflows"] = 0m,
            ["TotalOutflows"] = 0m,
            ["NetCashFlow"] = 0m,
            ["CustomerPayments"] = 0m,
            ["SalesRevenue"] = 0m,
            ["SupplierPayments"] = 0m,
            ["PurchaseExpenses"] = 0m,
            ["Refunds"] = 0m,
            ["OperatingExpenses"] = 0m,
            ["PaymentFees"] = 0m,
            ["ShippingCosts"] = 0m,
            ["Taxes"] = 0m,
        };

        foreach (var transaction in transactions)
        {
            var amount = transaction.Amount;

            // Categorize by transaction type
            switch (transaction.TransactionType)
            {
                case FinancialTransactionType.CustomerPayment:
                    summary["CustomerPayments"] += amount;
                    summary["TotalInflows"] += amount;
                    break;

                case FinancialTransactionType.SaleRevenue:
                    summary["SalesRevenue"] += amount;
                    break;

                case FinancialTransactionType.SupplierPayment:
                    summary["SupplierPayments"] += Math.Abs(amount);
                    summary["TotalOutflows"] += Math.Abs(amount);
                    break;

                case FinancialTransactionType.PurchaseExpense:
                    summary["PurchaseExpenses"] += Math.Abs(amount);
                    break;

                case FinancialTransactionType.CustomerRefund:
                    summary["Refunds"] += Math.Abs(amount);
                    summary["TotalOutflows"] += Math.Abs(amount);
                    break;

                case FinancialTransactionType.OperatingExpense:
                    summary["OperatingExpenses"] += Math.Abs(amount);
                    summary["TotalOutflows"] += Math.Abs(amount);
                    break;

                case FinancialTransactionType.PaymentFee:
                    summary["PaymentFees"] += Math.Abs(amount);
                    summary["TotalOutflows"] += Math.Abs(amount);
                    break;

                case FinancialTransactionType.ShippingCost:
                    summary["ShippingCosts"] += Math.Abs(amount);
                    summary["TotalOutflows"] += Math.Abs(amount);
                    break;

                case FinancialTransactionType.TaxTransaction:
                    summary["Taxes"] += amount; // Can be positive (collected) or negative (paid)
                    if (amount > 0)
                        summary["TotalInflows"] += amount;
                    else
                        summary["TotalOutflows"] += Math.Abs(amount);
                    break;
            }
        }

        summary["NetCashFlow"] = summary["TotalInflows"] - summary["TotalOutflows"];

        _logger.LogInformation(
            "Cash flow summary calculated: NetCashFlow={NetCashFlow}",
            summary["NetCashFlow"]
        );

        return summary;
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, decimal>> GetAccountsReceivableSummaryAsync(
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Calculating accounts receivable summary");

        var allTransactions = await _financialTransactionRepository.GetAllAsync(cancellationToken);

        var arTransactions = allTransactions
            .Where(t => t.TransactionType == FinancialTransactionType.AccountsReceivable)
            .Where(t => t.Status == "Pending") // Only unpaid receivables
            .ToList();

        var summary = new Dictionary<string, decimal>
        {
            ["TotalReceivable"] = arTransactions.Sum(t => t.Amount),
            ["Current_0_30"] = 0m,
            ["Aging_31_60"] = 0m,
            ["Aging_61_90"] = 0m,
            ["Aging_Over90"] = 0m,
            ["Count"] = arTransactions.Count,
        };

        var today = DateTime.UtcNow;

        foreach (var transaction in arTransactions)
        {
            var daysPast = (today - transaction.TransactionDate).Days;

            if (daysPast <= 30)
                summary["Current_0_30"] += transaction.Amount;
            else if (daysPast <= 60)
                summary["Aging_31_60"] += transaction.Amount;
            else if (daysPast <= 90)
                summary["Aging_61_90"] += transaction.Amount;
            else
                summary["Aging_Over90"] += transaction.Amount;
        }

        _logger.LogInformation(
            "AR summary calculated: Total={Total}, Count={Count}",
            summary["TotalReceivable"],
            summary["Count"]
        );

        return summary;
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, decimal>> GetAccountsPayableSummaryAsync(
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Calculating accounts payable summary");

        var allTransactions = await _financialTransactionRepository.GetAllAsync(cancellationToken);

        var apTransactions = allTransactions
            .Where(t => t.TransactionType == FinancialTransactionType.AccountsPayable)
            .Where(t => t.Status == "Pending") // Only unpaid payables
            .ToList();

        var summary = new Dictionary<string, decimal>
        {
            ["TotalPayable"] = apTransactions.Sum(t => t.Amount),
            ["Current_0_30"] = 0m,
            ["Aging_31_60"] = 0m,
            ["Aging_61_90"] = 0m,
            ["Aging_Over90"] = 0m,
            ["Count"] = apTransactions.Count,
        };

        var today = DateTime.UtcNow;

        foreach (var transaction in apTransactions)
        {
            var daysPast = (today - transaction.TransactionDate).Days;

            if (daysPast <= 30)
                summary["Current_0_30"] += transaction.Amount;
            else if (daysPast <= 60)
                summary["Aging_31_60"] += transaction.Amount;
            else if (daysPast <= 90)
                summary["Aging_61_90"] += transaction.Amount;
            else
                summary["Aging_Over90"] += transaction.Amount;
        }

        _logger.LogInformation(
            "AP summary calculated: Total={Total}, Count={Count}",
            summary["TotalPayable"],
            summary["Count"]
        );

        return summary;
    }

    #region Private Helper Methods

    /// <summary>
    /// Generates a unique sequential transaction number
    /// </summary>
    /// <returns>Formatted transaction number (FIN-YYYYMMDD-XXXXXX)</returns>
    private static string GenerateTransactionNumber()
    {
        lock (_lock)
        {
            _transactionCounter++;
            return $"FIN-{DateTime.UtcNow:yyyyMMdd}-{_transactionCounter:D6}";
        }
    }

    /// <summary>
    /// Calculates payment processing fee based on provider and method
    /// </summary>
    /// <param name="amount">Payment amount</param>
    /// <param name="paymentMethod">Payment method used</param>
    /// <param name="provider">Payment provider name</param>
    /// <returns>Tuple with fee amount and net amount</returns>
    private static (decimal feeAmount, decimal netAmount) CalculatePaymentFee(
        decimal amount,
        PaymentMethod paymentMethod,
        string? provider
    )
    {
        decimal feeRate = DEFAULT_FEE_RATE;
        decimal fixedFee = 0m;

        // Determine fee structure based on provider
        if (!string.IsNullOrEmpty(provider))
        {
            if (provider.Equals("Stripe", StringComparison.OrdinalIgnoreCase))
            {
                feeRate = STRIPE_FEE_RATE;
                fixedFee = STRIPE_FEE_FIXED;
            }
            else if (provider.Equals("PayPal", StringComparison.OrdinalIgnoreCase))
            {
                feeRate = PAYPAL_FEE_RATE;
                fixedFee = PAYPAL_FEE_FIXED;
            }
        }

        // Some payment methods may have different or no fees
        if (
            paymentMethod == PaymentMethod.BankTransfer
            || paymentMethod == PaymentMethod.CashOnDelivery
        )
        {
            return (0m, amount); // No processing fee for these methods
        }

        var fee = Math.Round((amount * feeRate) + fixedFee, 2);
        var net = amount - fee;

        return (fee, net);
    }

    #endregion
}
