using System.Diagnostics;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Interfaces;

namespace ECommerce.Application.Services;

/// <summary>
/// Accounting service for inventory operations
/// Implements Brazilian GAAP (NBC TG) rules for inventory movements
/// </summary>
public class AccountingService : IAccountingService
{
    private readonly IChartOfAccountsRepository _chartOfAccountsRepository;
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IAccountingEntryRepository _accountingEntryRepository;
    private readonly IAccountingRuleRepository _accountingRuleRepository;
    private readonly ILoggingService _logger;

    // OpenTelemetry activity source for custom tracing
    private static readonly ActivitySource ActivitySource = new(
        "ECommerce.Application.Accounting",
        "1.0.0"
    );

    public AccountingService(
        IChartOfAccountsRepository chartOfAccountsRepository,
        IJournalEntryRepository journalEntryRepository,
        IAccountingEntryRepository accountingEntryRepository,
        IAccountingRuleRepository accountingRuleRepository,
        ILoggingService logger
    )
    {
        _chartOfAccountsRepository =
            chartOfAccountsRepository
            ?? throw new ArgumentNullException(nameof(chartOfAccountsRepository));
        _journalEntryRepository =
            journalEntryRepository
            ?? throw new ArgumentNullException(nameof(journalEntryRepository));
        _accountingEntryRepository =
            accountingEntryRepository
            ?? throw new ArgumentNullException(nameof(accountingEntryRepository));
        _accountingRuleRepository =
            accountingRuleRepository
            ?? throw new ArgumentNullException(nameof(accountingRuleRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<JournalEntryEntity> RecordPurchaseAsync(
        InventoryTransactionEntity transaction,
        Guid createdBy,
        CancellationToken cancellationToken = default
    )
    {
        using var activity = ActivitySource.StartActivity("RecordPurchase", ActivityKind.Internal);
        activity?.SetTag("transaction.id", transaction.Id);
        activity?.SetTag("product.id", transaction.ProductId);
        activity?.SetTag("product.sku", transaction.ProductSku);
        activity?.SetTag("transaction.quantity", transaction.Quantity);
        activity?.SetTag("transaction.total_cost", transaction.TotalCost);
        activity?.SetTag("transaction.type", transaction.TransactionType.ToString());

        _logger.LogInformation(
            "Recording purchase transaction: ProductId={ProductId}, Quantity={Quantity}, TotalCost={TotalCost}",
            transaction.ProductId,
            transaction.Quantity,
            transaction.TotalCost
        );

        try
        {
            // Debit: Inventory (increases asset)
            // Credit: Suppliers/Cash (increases liability or decreases asset)

            var rule = await GetAccountingRuleAsync(
                InventoryTransactionType.Purchase,
                transaction,
                cancellationToken
            );

            var debitAccount =
                await _chartOfAccountsRepository.GetByCodeAsync(
                    rule.DebitAccountCode,
                    cancellationToken
                )
                ?? throw new InvalidOperationException(
                    $"Debit account {rule.DebitAccountCode} not found for purchase transaction"
                );

            var creditAccount =
                await _chartOfAccountsRepository.GetByCodeAsync(
                    rule.CreditAccountCode,
                    cancellationToken
                )
                ?? throw new InvalidOperationException(
                    $"Credit account {rule.CreditAccountCode} not found for purchase transaction"
                );

            var journalEntry = await CreateJournalEntryAsync(
                "PURCHASE",
                transaction.DocumentNumber ?? transaction.TransactionNumber,
                $"Purchase of goods - {transaction.ProductName} ({transaction.ProductSku})",
                transaction.TotalCost,
                transaction.ProductId,
                transaction.Id,
                null,
                createdBy,
                cancellationToken
            );

            // Debit entry
            await CreateAccountingEntryAsync(
                journalEntry.Id,
                debitAccount.Id,
                EntryType.Debit,
                transaction.TotalCost,
                $"Purchase - {transaction.Quantity} units x {transaction.UnitCost:C}",
                transaction.ToLocation,
                cancellationToken
            );

            // Credit entry
            await CreateAccountingEntryAsync(
                journalEntry.Id,
                creditAccount.Id,
                EntryType.Credit,
                transaction.TotalCost,
                $"Supplier - Invoice {transaction.DocumentNumber}",
                null,
                cancellationToken
            );

            // Update account balances
            debitAccount.Balance += transaction.TotalCost;
            creditAccount.Balance += transaction.TotalCost;

            _chartOfAccountsRepository.Update(debitAccount);
            _chartOfAccountsRepository.Update(creditAccount);
            await _chartOfAccountsRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Purchase transaction recorded successfully: JournalEntryId={JournalEntryId}, EntryNumber={EntryNumber}",
                journalEntry.Id,
                journalEntry.EntryNumber
            );

            return journalEntry;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error recording purchase transaction: ProductId={ProductId}",
                transaction.ProductId
            );
            throw;
        }
    }

    public async Task<JournalEntryEntity> RecordSaleAsync(
        InventoryTransactionEntity transaction,
        Guid createdBy,
        CancellationToken cancellationToken = default
    )
    {
        // Debit: COGS (increases expense/cost)
        // Credit: Inventory (decreases asset)

        var rule = await GetAccountingRuleAsync(
            InventoryTransactionType.Sale,
            transaction,
            cancellationToken
        );

        var debitAccount =
            await _chartOfAccountsRepository.GetByCodeAsync(
                rule.DebitAccountCode,
                cancellationToken
            )
            ?? throw new InvalidOperationException(
                $"Debit account {rule.DebitAccountCode} not found for sale transaction"
            );

        var creditAccount =
            await _chartOfAccountsRepository.GetByCodeAsync(
                rule.CreditAccountCode,
                cancellationToken
            )
            ?? throw new InvalidOperationException(
                $"Credit account {rule.CreditAccountCode} not found for sale transaction"
            );

        var journalEntry = await CreateJournalEntryAsync(
            "COGS",
            transaction.DocumentNumber ?? transaction.TransactionNumber,
            $"Inventory withdrawal - Sale {transaction.ProductName} ({transaction.ProductSku})",
            transaction.TotalCost,
            transaction.ProductId,
            transaction.Id,
            transaction.OrderId,
            createdBy,
            cancellationToken
        );

        // Debit entry
        await CreateAccountingEntryAsync(
            journalEntry.Id,
            debitAccount.Id,
            EntryType.Debit,
            transaction.TotalCost,
            $"Sale - {Math.Abs(transaction.Quantity)} units x {transaction.UnitCost:C}",
            transaction.FromLocation,
            cancellationToken
        );

        // Credit entry
        await CreateAccountingEntryAsync(
            journalEntry.Id,
            creditAccount.Id,
            EntryType.Credit,
            transaction.TotalCost,
            $"Inventory withdrawal - Order {transaction.OrderId}",
            transaction.FromLocation,
            cancellationToken
        );

        // Update account balances
        debitAccount.Balance += transaction.TotalCost;
        creditAccount.Balance -= transaction.TotalCost;

        _chartOfAccountsRepository.Update(debitAccount);
        _chartOfAccountsRepository.Update(creditAccount);
        await _chartOfAccountsRepository.SaveChangesAsync(cancellationToken);

        return journalEntry;
    }

    public async Task<JournalEntryEntity> RecordSaleReturnAsync(
        InventoryTransactionEntity transaction,
        Guid createdBy,
        CancellationToken cancellationToken = default
    )
    {
        // Debit: Inventory (increases asset)
        // Credit: COGS (decreases expense/cost)

        var rule = await GetAccountingRuleAsync(
            InventoryTransactionType.SaleReturn,
            transaction,
            cancellationToken
        );

        var debitAccount =
            await _chartOfAccountsRepository.GetByCodeAsync(
                rule.DebitAccountCode,
                cancellationToken
            )
            ?? throw new InvalidOperationException(
                $"Debit account {rule.DebitAccountCode} not found for sale return transaction"
            );

        var creditAccount =
            await _chartOfAccountsRepository.GetByCodeAsync(
                rule.CreditAccountCode,
                cancellationToken
            )
            ?? throw new InvalidOperationException(
                $"Credit account {rule.CreditAccountCode} not found for sale return transaction"
            );

        var journalEntry = await CreateJournalEntryAsync(
            "SALE_RETURN",
            transaction.DocumentNumber ?? transaction.TransactionNumber,
            $"Sales return - {transaction.ProductName} ({transaction.ProductSku})",
            transaction.TotalCost,
            transaction.ProductId,
            transaction.Id,
            transaction.OrderId,
            createdBy,
            cancellationToken
        );

        // Debit entry
        await CreateAccountingEntryAsync(
            journalEntry.Id,
            debitAccount.Id,
            EntryType.Debit,
            transaction.TotalCost,
            $"Return - {transaction.Quantity} units",
            transaction.ToLocation,
            cancellationToken
        );

        // Credit entry
        await CreateAccountingEntryAsync(
            journalEntry.Id,
            creditAccount.Id,
            EntryType.Credit,
            transaction.TotalCost,
            $"COGS reversal - Order {transaction.OrderId}",
            null,
            cancellationToken
        );

        // Update account balances
        debitAccount.Balance += transaction.TotalCost;
        creditAccount.Balance -= transaction.TotalCost;

        _chartOfAccountsRepository.Update(debitAccount);
        _chartOfAccountsRepository.Update(creditAccount);
        await _chartOfAccountsRepository.SaveChangesAsync(cancellationToken);

        return journalEntry;
    }

    public async Task<JournalEntryEntity> RecordPurchaseReturnAsync(
        InventoryTransactionEntity transaction,
        Guid createdBy,
        CancellationToken cancellationToken = default
    )
    {
        // Debit: Suppliers (decreases liability)
        // Credit: Inventory (decreases asset)

        var rule = await GetAccountingRuleAsync(
            InventoryTransactionType.PurchaseReturn,
            transaction,
            cancellationToken
        );

        var debitAccount =
            await _chartOfAccountsRepository.GetByCodeAsync(
                rule.DebitAccountCode,
                cancellationToken
            )
            ?? throw new InvalidOperationException(
                $"Debit account {rule.DebitAccountCode} not found for purchase return transaction"
            );

        var creditAccount =
            await _chartOfAccountsRepository.GetByCodeAsync(
                rule.CreditAccountCode,
                cancellationToken
            )
            ?? throw new InvalidOperationException(
                $"Credit account {rule.CreditAccountCode} not found for purchase return transaction"
            );

        var journalEntry = await CreateJournalEntryAsync(
            "PURCHASE_RETURN",
            transaction.DocumentNumber ?? transaction.TransactionNumber,
            $"Purchase return - {transaction.ProductName} ({transaction.ProductSku})",
            transaction.TotalCost,
            transaction.ProductId,
            transaction.Id,
            null,
            createdBy,
            cancellationToken
        );

        // Debit entry
        await CreateAccountingEntryAsync(
            journalEntry.Id,
            debitAccount.Id,
            EntryType.Debit,
            transaction.TotalCost,
            $"Return to supplier - Invoice {transaction.DocumentNumber}",
            null,
            cancellationToken
        );

        // Credit entry
        await CreateAccountingEntryAsync(
            journalEntry.Id,
            creditAccount.Id,
            EntryType.Credit,
            transaction.TotalCost,
            $"Return - {Math.Abs(transaction.Quantity)} units",
            transaction.FromLocation,
            cancellationToken
        );

        // Update account balances
        debitAccount.Balance -= transaction.TotalCost;
        creditAccount.Balance -= transaction.TotalCost;

        _chartOfAccountsRepository.Update(debitAccount);
        _chartOfAccountsRepository.Update(creditAccount);
        await _chartOfAccountsRepository.SaveChangesAsync(cancellationToken);

        return journalEntry;
    }

    public async Task<JournalEntryEntity> RecordAdjustmentAsync(
        InventoryTransactionEntity transaction,
        Guid createdBy,
        CancellationToken cancellationToken = default
    )
    {
        var rule = await GetAccountingRuleAsync(
            InventoryTransactionType.Adjustment,
            transaction,
            cancellationToken
        );

        var debitAccount =
            await _chartOfAccountsRepository.GetByCodeAsync(
                rule.DebitAccountCode,
                cancellationToken
            )
            ?? throw new InvalidOperationException(
                $"Debit account {rule.DebitAccountCode} not found for adjustment transaction"
            );

        var creditAccount =
            await _chartOfAccountsRepository.GetByCodeAsync(
                rule.CreditAccountCode,
                cancellationToken
            )
            ?? throw new InvalidOperationException(
                $"Credit account {rule.CreditAccountCode} not found for adjustment transaction"
            );

        var journalEntry = await CreateJournalEntryAsync(
            "ADJUSTMENT",
            transaction.TransactionNumber,
            $"Inventory adjustment - {transaction.ProductName} ({transaction.ProductSku})",
            transaction.TotalCost,
            transaction.ProductId,
            transaction.Id,
            null,
            createdBy,
            cancellationToken
        );

        // Debit entry
        await CreateAccountingEntryAsync(
            journalEntry.Id,
            debitAccount.Id,
            EntryType.Debit,
            transaction.TotalCost,
            $"Adjustment - {transaction.Quantity} units | {transaction.Notes}",
            transaction.ToLocation,
            cancellationToken
        );

        // Credit entry
        await CreateAccountingEntryAsync(
            journalEntry.Id,
            creditAccount.Id,
            EntryType.Credit,
            transaction.TotalCost,
            $"Inventory adjustment - {transaction.ProductSku}",
            null,
            cancellationToken
        );

        // Update account balances
        if (transaction.Quantity > 0)
        {
            debitAccount.Balance += transaction.TotalCost;
            creditAccount.Balance += transaction.TotalCost;
        }
        else
        {
            debitAccount.Balance -= transaction.TotalCost;
            creditAccount.Balance += transaction.TotalCost;
        }

        _chartOfAccountsRepository.Update(debitAccount);
        _chartOfAccountsRepository.Update(creditAccount);
        await _chartOfAccountsRepository.SaveChangesAsync(cancellationToken);

        return journalEntry;
    }

    public async Task<JournalEntryEntity> RecordLossAsync(
        InventoryTransactionEntity transaction,
        Guid createdBy,
        CancellationToken cancellationToken = default
    )
    {
        // Debit: Inventory Loss (increases expense)
        // Credit: Inventory (decreases asset)

        var rule = await GetAccountingRuleAsync(
            InventoryTransactionType.Loss,
            transaction,
            cancellationToken
        );

        var debitAccount =
            await _chartOfAccountsRepository.GetByCodeAsync(
                rule.DebitAccountCode,
                cancellationToken
            )
            ?? throw new InvalidOperationException(
                $"Debit account {rule.DebitAccountCode} not found for loss transaction"
            );

        var creditAccount =
            await _chartOfAccountsRepository.GetByCodeAsync(
                rule.CreditAccountCode,
                cancellationToken
            )
            ?? throw new InvalidOperationException(
                $"Credit account {rule.CreditAccountCode} not found for loss transaction"
            );

        var journalEntry = await CreateJournalEntryAsync(
            "LOSS",
            transaction.TransactionNumber,
            $"Inventory loss/shrinkage - {transaction.ProductName} ({transaction.ProductSku})",
            transaction.TotalCost,
            transaction.ProductId,
            transaction.Id,
            null,
            createdBy,
            cancellationToken
        );

        // Debit entry
        await CreateAccountingEntryAsync(
            journalEntry.Id,
            debitAccount.Id,
            EntryType.Debit,
            transaction.TotalCost,
            $"Loss/shrinkage - {Math.Abs(transaction.Quantity)} units | {transaction.Notes}",
            transaction.FromLocation,
            cancellationToken
        );

        // Credit entry
        await CreateAccountingEntryAsync(
            journalEntry.Id,
            creditAccount.Id,
            EntryType.Credit,
            transaction.TotalCost,
            $"Write-off for loss - {transaction.ProductSku}",
            transaction.FromLocation,
            cancellationToken
        );

        // Update account balances
        debitAccount.Balance += transaction.TotalCost;
        creditAccount.Balance -= transaction.TotalCost;

        _chartOfAccountsRepository.Update(debitAccount);
        _chartOfAccountsRepository.Update(creditAccount);
        await _chartOfAccountsRepository.SaveChangesAsync(cancellationToken);

        return journalEntry;
    }

    /// <summary>
    /// Gets the applicable accounting rule for a transaction type and condition.
    /// </summary>
    private async Task<AccountingRuleEntity> GetAccountingRuleAsync(
        InventoryTransactionType transactionType,
        InventoryTransactionEntity transaction,
        CancellationToken cancellationToken
    )
    {
        var rules = await _accountingRuleRepository.GetByTransactionTypeAsync(
            transactionType,
            cancellationToken
        );

        if (rules.Count == 0)
        {
            throw new InvalidOperationException(
                $"No accounting rules found for transaction type: {transactionType}"
            );
        }

        // Find rule with matching condition or default rule (no condition)
        foreach (var rule in rules)
        {
            if (string.IsNullOrEmpty(rule.Condition))
            {
                return rule;
            }

            // Evaluate conditions for adjustments
            if (transactionType == InventoryTransactionType.Adjustment)
            {
                if (rule.Condition.Contains(">") && transaction.Quantity > 0)
                {
                    return rule;
                }
                if (rule.Condition.Contains("<") && transaction.Quantity < 0)
                {
                    return rule;
                }
            }
        }

        throw new InvalidOperationException(
            $"No applicable accounting rule found for transaction type: {transactionType}"
        );
    }

    private async Task<JournalEntryEntity> CreateJournalEntryAsync(
        string documentType,
        string documentNumber,
        string history,
        decimal totalAmount,
        Guid? productId,
        Guid? inventoryTransactionId,
        Guid? orderId,
        Guid createdBy,
        CancellationToken cancellationToken
    )
    {
        var entryNumber =
            $"{documentType}-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

        var journalEntry = new JournalEntryEntity
        {
            Id = Guid.NewGuid(),
            EntryNumber = entryNumber,
            EntryDate = DateTime.UtcNow,
            DocumentType = documentType,
            DocumentNumber = documentNumber,
            History = history,
            TotalAmount = totalAmount,
            ProductId = productId,
            InventoryTransactionId = inventoryTransactionId,
            OrderId = orderId,
            IsPosted = true,
            PostedAt = DateTime.UtcNow,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await _journalEntryRepository.AddAsync(journalEntry, cancellationToken);
        return journalEntry;
    }

    private async Task<AccountingEntryEntity> CreateAccountingEntryAsync(
        Guid journalEntryId,
        Guid accountId,
        EntryType entryType,
        decimal amount,
        string? description,
        string? costCenter,
        CancellationToken cancellationToken
    )
    {
        var entry = new AccountingEntryEntity
        {
            Id = Guid.NewGuid(),
            JournalEntryId = journalEntryId,
            AccountId = accountId,
            EntryType = entryType,
            Amount = amount,
            Description = description,
            CostCenter = costCenter,
            CreatedAt = DateTime.UtcNow,
        };

        await _accountingEntryRepository.AddAsync(entry, cancellationToken);
        return entry;
    }
}
