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
    private readonly IRepository<ChartOfAccountsEntity> _chartOfAccountsRepository;
    private readonly IRepository<JournalEntryEntity> _journalEntryRepository;
    private readonly IRepository<AccountingEntryEntity> _accountingEntryRepository;

    // Standard account codes (simplified structure)
    private const string ACCOUNT_INVENTORY = "1.1.03.001"; // Current Assets - Inventory
    private const string ACCOUNT_CMV = "3.1.01.001"; // Cost of Goods Sold
    private const string ACCOUNT_SUPPLIERS = "2.1.01.001"; // Current Liabilities - Accounts Payable
    private const string ACCOUNT_CASH = "1.1.01.001"; // Current Assets - Cash
    private const string ACCOUNT_INVENTORY_LOSS = "3.2.01.001"; // Operating Expenses - Inventory Loss
    private const string ACCOUNT_OTHER_INCOME = "4.2.01.001"; // Other Income
    private const string ACCOUNT_OTHER_EXPENSES = "3.2.01.002"; // Other Expenses

    public AccountingService(
        IRepository<ChartOfAccountsEntity> chartOfAccountsRepository,
        IRepository<JournalEntryEntity> journalEntryRepository,
        IRepository<AccountingEntryEntity> accountingEntryRepository
    )
    {
        _chartOfAccountsRepository = chartOfAccountsRepository;
        _journalEntryRepository = journalEntryRepository;
        _accountingEntryRepository = accountingEntryRepository;
    }

    public async Task<JournalEntryEntity> RecordPurchaseAsync(
        InventoryTransactionEntity transaction,
        Guid createdBy,
        CancellationToken cancellationToken = default
    )
    {
        // Debit: Inventory (increases asset)
        // Credit: Suppliers/Cash (increases liability or decreases asset)

        var inventoryAccount = await GetOrCreateAccountAsync(
            ACCOUNT_INVENTORY,
            "Inventory",
            AccountType.Asset,
            cancellationToken
        );

        var suppliersAccount = await GetOrCreateAccountAsync(
            ACCOUNT_SUPPLIERS,
            "Accounts Payable - Suppliers",
            AccountType.Liability,
            cancellationToken
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

        // Debit entry - Inventory
        await CreateAccountingEntryAsync(
            journalEntry.Id,
            inventoryAccount.Id,
            EntryType.Debit,
            transaction.TotalCost,
            $"Purchase - {transaction.Quantity} units x {transaction.UnitCost:C}",
            transaction.ToLocation,
            cancellationToken
        );

        // Credit entry - Suppliers
        await CreateAccountingEntryAsync(
            journalEntry.Id,
            suppliersAccount.Id,
            EntryType.Credit,
            transaction.TotalCost,
            $"Supplier - Invoice {transaction.DocumentNumber}",
            null,
            cancellationToken
        );

        // Update account balances
        inventoryAccount.Balance += transaction.TotalCost;
        suppliersAccount.Balance += transaction.TotalCost;

        await _chartOfAccountsRepository.UpdateAsync(inventoryAccount, cancellationToken);
        await _chartOfAccountsRepository.UpdateAsync(suppliersAccount, cancellationToken);

        return journalEntry;
    }

    public async Task<JournalEntryEntity> RecordSaleAsync(
        InventoryTransactionEntity transaction,
        Guid createdBy,
        CancellationToken cancellationToken = default
    )
    {
        // Debit: COGS (increases expense/cost)
        // Credit: Inventory (decreases asset)

        var inventoryAccount = await GetOrCreateAccountAsync(
            ACCOUNT_INVENTORY,
            "Inventory",
            AccountType.Asset,
            cancellationToken
        );

        var cmvAccount = await GetOrCreateAccountAsync(
            ACCOUNT_CMV,
            "Cost of Goods Sold",
            AccountType.Expense,
            cancellationToken
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

        // Debit entry - COGS
        await CreateAccountingEntryAsync(
            journalEntry.Id,
            cmvAccount.Id,
            EntryType.Debit,
            transaction.TotalCost,
            $"Sale - {Math.Abs(transaction.Quantity)} units x {transaction.UnitCost:C}",
            transaction.FromLocation,
            cancellationToken
        );

        // Credit entry - Inventory
        await CreateAccountingEntryAsync(
            journalEntry.Id,
            inventoryAccount.Id,
            EntryType.Credit,
            transaction.TotalCost,
            $"Inventory withdrawal - Order {transaction.OrderId}",
            transaction.FromLocation,
            cancellationToken
        );

        // Update account balances
        cmvAccount.Balance += transaction.TotalCost;
        inventoryAccount.Balance -= transaction.TotalCost;

        await _chartOfAccountsRepository.UpdateAsync(inventoryAccount, cancellationToken);
        await _chartOfAccountsRepository.UpdateAsync(cmvAccount, cancellationToken);

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

        var inventoryAccount = await GetOrCreateAccountAsync(
            ACCOUNT_INVENTORY,
            "Inventory",
            AccountType.Asset,
            cancellationToken
        );

        var cmvAccount = await GetOrCreateAccountAsync(
            ACCOUNT_CMV,
            "Cost of Goods Sold",
            AccountType.Expense,
            cancellationToken
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

        // Debit entry - Inventory
        await CreateAccountingEntryAsync(
            journalEntry.Id,
            inventoryAccount.Id,
            EntryType.Debit,
            transaction.TotalCost,
            $"Return - {transaction.Quantity} units",
            transaction.ToLocation,
            cancellationToken
        );

        // Credit entry - COGS
        await CreateAccountingEntryAsync(
            journalEntry.Id,
            cmvAccount.Id,
            EntryType.Credit,
            transaction.TotalCost,
            $"COGS reversal - Order {transaction.OrderId}",
            null,
            cancellationToken
        );

        // Update account balances
        inventoryAccount.Balance += transaction.TotalCost;
        cmvAccount.Balance -= transaction.TotalCost;

        await _chartOfAccountsRepository.UpdateAsync(inventoryAccount, cancellationToken);
        await _chartOfAccountsRepository.UpdateAsync(cmvAccount, cancellationToken);

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

        var inventoryAccount = await GetOrCreateAccountAsync(
            ACCOUNT_INVENTORY,
            "Inventory",
            AccountType.Asset,
            cancellationToken
        );

        var suppliersAccount = await GetOrCreateAccountAsync(
            ACCOUNT_SUPPLIERS,
            "Accounts Payable - Suppliers",
            AccountType.Liability,
            cancellationToken
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

        // Debit entry - Suppliers
        await CreateAccountingEntryAsync(
            journalEntry.Id,
            suppliersAccount.Id,
            EntryType.Debit,
            transaction.TotalCost,
            $"Return to supplier - Invoice {transaction.DocumentNumber}",
            null,
            cancellationToken
        );

        // Credit entry - Inventory
        await CreateAccountingEntryAsync(
            journalEntry.Id,
            inventoryAccount.Id,
            EntryType.Credit,
            transaction.TotalCost,
            $"Return - {Math.Abs(transaction.Quantity)} units",
            transaction.FromLocation,
            cancellationToken
        );

        // Update account balances
        suppliersAccount.Balance -= transaction.TotalCost;
        inventoryAccount.Balance -= transaction.TotalCost;

        await _chartOfAccountsRepository.UpdateAsync(inventoryAccount, cancellationToken);
        await _chartOfAccountsRepository.UpdateAsync(suppliersAccount, cancellationToken);

        return journalEntry;
    }

    public async Task<JournalEntryEntity> RecordAdjustmentAsync(
        InventoryTransactionEntity transaction,
        Guid createdBy,
        CancellationToken cancellationToken = default
    )
    {
        var inventoryAccount = await GetOrCreateAccountAsync(
            ACCOUNT_INVENTORY,
            "Inventory",
            AccountType.Asset,
            cancellationToken
        );

        ChartOfAccountsEntity contraAccount;
        EntryType inventoryEntryType;
        EntryType contraEntryType;

        if (transaction.Quantity > 0)
        {
            // Positive adjustment (overage)
            // Debit: Inventory, Credit: Other Income
            contraAccount = await GetOrCreateAccountAsync(
                ACCOUNT_OTHER_INCOME,
                "Other Operating Income",
                AccountType.Revenue,
                cancellationToken
            );
            inventoryEntryType = EntryType.Debit;
            contraEntryType = EntryType.Credit;
        }
        else
        {
            // Negative adjustment (shortage)
            // Debit: Other Expenses, Credit: Inventory
            contraAccount = await GetOrCreateAccountAsync(
                ACCOUNT_OTHER_EXPENSES,
                "Other Operating Expenses",
                AccountType.Expense,
                cancellationToken
            );
            inventoryEntryType = EntryType.Credit;
            contraEntryType = EntryType.Debit;
        }

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

        // Inventory entry
        await CreateAccountingEntryAsync(
            journalEntry.Id,
            inventoryAccount.Id,
            inventoryEntryType,
            transaction.TotalCost,
            $"Adjustment - {transaction.Quantity} units | {transaction.Notes}",
            transaction.ToLocation,
            cancellationToken
        );

        // Contra entry
        await CreateAccountingEntryAsync(
            journalEntry.Id,
            contraAccount.Id,
            contraEntryType,
            transaction.TotalCost,
            $"Inventory adjustment - {transaction.ProductSku}",
            null,
            cancellationToken
        );

        // Update account balances
        if (transaction.Quantity > 0)
        {
            inventoryAccount.Balance += transaction.TotalCost;
            contraAccount.Balance += transaction.TotalCost;
        }
        else
        {
            inventoryAccount.Balance -= transaction.TotalCost;
            contraAccount.Balance += transaction.TotalCost;
        }

        await _chartOfAccountsRepository.UpdateAsync(inventoryAccount, cancellationToken);
        await _chartOfAccountsRepository.UpdateAsync(contraAccount, cancellationToken);

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

        var inventoryAccount = await GetOrCreateAccountAsync(
            ACCOUNT_INVENTORY,
            "Inventory",
            AccountType.Asset,
            cancellationToken
        );

        var lossAccount = await GetOrCreateAccountAsync(
            ACCOUNT_INVENTORY_LOSS,
            "Inventory Loss",
            AccountType.Expense,
            cancellationToken
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

        // Debit entry - Loss
        await CreateAccountingEntryAsync(
            journalEntry.Id,
            lossAccount.Id,
            EntryType.Debit,
            transaction.TotalCost,
            $"Loss/shrinkage - {Math.Abs(transaction.Quantity)} units | {transaction.Notes}",
            transaction.FromLocation,
            cancellationToken
        );

        // Credit entry - Inventory
        await CreateAccountingEntryAsync(
            journalEntry.Id,
            inventoryAccount.Id,
            EntryType.Credit,
            transaction.TotalCost,
            $"Write-off for loss - {transaction.ProductSku}",
            transaction.FromLocation,
            cancellationToken
        );

        // Update account balances
        lossAccount.Balance += transaction.TotalCost;
        inventoryAccount.Balance -= transaction.TotalCost;

        await _chartOfAccountsRepository.UpdateAsync(inventoryAccount, cancellationToken);
        await _chartOfAccountsRepository.UpdateAsync(lossAccount, cancellationToken);

        return journalEntry;
    }

    public async Task<ChartOfAccountsEntity> GetOrCreateAccountAsync(
        string accountCode,
        string accountName,
        AccountType accountType,
        CancellationToken cancellationToken = default
    )
    {
        var account = (
            await _chartOfAccountsRepository.GetAllAsync(cancellationToken)
        ).FirstOrDefault(a => a.AccountCode == accountCode);

        if (account == null)
        {
            account = new ChartOfAccountsEntity
            {
                Id = Guid.NewGuid(),
                AccountCode = accountCode,
                AccountName = accountName,
                AccountType = accountType,
                IsAnalytic = true,
                IsActive = true,
                Balance = 0,
                Description = $"Account automatically created by system",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            await _chartOfAccountsRepository.AddAsync(account, cancellationToken);
        }

        return account;
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
