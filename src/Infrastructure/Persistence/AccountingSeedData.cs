using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence;

/// <summary>
/// Database seeding for Chart of Accounts and Accounting Rules
/// </summary>
public static class AccountingSeedData
{
    /// <summary>
    /// Seeds the default chart of accounts and accounting rules
    /// </summary>
    public static void SeedAccountingData(this ModelBuilder modelBuilder)
    {
        // Seed Chart of Accounts
        modelBuilder
            .Entity<ChartOfAccountsEntity>()
            .HasData(
                // Assets
                new ChartOfAccountsEntity
                {
                    Id = new Guid("10000000-0000-0000-0000-000000000001"),
                    AccountCode = "1.1.01.001",
                    AccountName = "Cash and Cash Equivalents",
                    Description = "Bank accounts and petty cash",
                    AccountType = AccountType.Asset,
                    IsAnalytic = true,
                    IsActive = true,
                    Balance = 0,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                },
                new ChartOfAccountsEntity
                {
                    Id = new Guid("10000000-0000-0000-0000-000000000002"),
                    AccountCode = "1.1.03.001",
                    AccountName = "Inventory",
                    Description = "Merchandise inventory for resale",
                    AccountType = AccountType.Asset,
                    IsAnalytic = true,
                    IsActive = true,
                    Balance = 0,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                },
                // Liabilities
                new ChartOfAccountsEntity
                {
                    Id = new Guid("20000000-0000-0000-0000-000000000001"),
                    AccountCode = "2.1.01.001",
                    AccountName = "Accounts Payable - Suppliers",
                    Description = "Amounts owed to suppliers for inventory purchases",
                    AccountType = AccountType.Liability,
                    IsAnalytic = true,
                    IsActive = true,
                    Balance = 0,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                },
                // Expenses
                new ChartOfAccountsEntity
                {
                    Id = new Guid("30000000-0000-0000-0000-000000000001"),
                    AccountCode = "3.1.01.001",
                    AccountName = "Cost of Goods Sold",
                    Description = "Direct costs of goods sold to customers",
                    AccountType = AccountType.Expense,
                    IsAnalytic = true,
                    IsActive = true,
                    Balance = 0,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                },
                new ChartOfAccountsEntity
                {
                    Id = new Guid("30000000-0000-0000-0000-000000000002"),
                    AccountCode = "3.2.01.001",
                    AccountName = "Inventory Loss",
                    Description = "Inventory shrinkage, loss, and write-offs",
                    AccountType = AccountType.Expense,
                    IsAnalytic = true,
                    IsActive = true,
                    Balance = 0,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                },
                new ChartOfAccountsEntity
                {
                    Id = new Guid("30000000-0000-0000-0000-000000000003"),
                    AccountCode = "3.2.01.002",
                    AccountName = "Other Operating Expenses",
                    Description =
                        "Miscellaneous operating expenses including negative inventory adjustments",
                    AccountType = AccountType.Expense,
                    IsAnalytic = true,
                    IsActive = true,
                    Balance = 0,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                },
                // Revenue
                new ChartOfAccountsEntity
                {
                    Id = new Guid("40000000-0000-0000-0000-000000000001"),
                    AccountCode = "4.2.01.001",
                    AccountName = "Other Operating Income",
                    Description = "Miscellaneous income including positive inventory adjustments",
                    AccountType = AccountType.Revenue,
                    IsAnalytic = true,
                    IsActive = true,
                    Balance = 0,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                }
            );

        // Seed Accounting Rules
        modelBuilder
            .Entity<AccountingRuleEntity>()
            .HasData(
                // Purchase: Debit Inventory, Credit Accounts Payable
                new AccountingRuleEntity
                {
                    Id = new Guid("a0000000-0000-0000-0000-000000000001"),
                    TransactionType = InventoryTransactionType.Purchase,
                    RuleCode = "PURCHASE",
                    Description = "Purchase of inventory from suppliers",
                    DebitAccountCode = "1.1.03.001", // Inventory
                    CreditAccountCode = "2.1.01.001", // Accounts Payable
                    Condition = null,
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                },
                // Sale: Debit COGS, Credit Inventory
                new AccountingRuleEntity
                {
                    Id = new Guid("a0000000-0000-0000-0000-000000000002"),
                    TransactionType = InventoryTransactionType.Sale,
                    RuleCode = "SALE",
                    Description = "Sale of inventory to customers (COGS recognition)",
                    DebitAccountCode = "3.1.01.001", // Cost of Goods Sold
                    CreditAccountCode = "1.1.03.001", // Inventory
                    Condition = null,
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                },
                // Sale Return: Debit Inventory, Credit COGS
                new AccountingRuleEntity
                {
                    Id = new Guid("a0000000-0000-0000-0000-000000000003"),
                    TransactionType = InventoryTransactionType.SaleReturn,
                    RuleCode = "SALE_RETURN",
                    Description = "Customer returns inventory (COGS reversal)",
                    DebitAccountCode = "1.1.03.001", // Inventory
                    CreditAccountCode = "3.1.01.001", // Cost of Goods Sold
                    Condition = null,
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                },
                // Purchase Return: Debit Accounts Payable, Credit Inventory
                new AccountingRuleEntity
                {
                    Id = new Guid("a0000000-0000-0000-0000-000000000004"),
                    TransactionType = InventoryTransactionType.PurchaseReturn,
                    RuleCode = "PURCHASE_RETURN",
                    Description = "Return of inventory to suppliers",
                    DebitAccountCode = "2.1.01.001", // Accounts Payable
                    CreditAccountCode = "1.1.03.001", // Inventory
                    Condition = null,
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                },
                // Adjustment - Positive: Debit Inventory, Credit Other Income
                new AccountingRuleEntity
                {
                    Id = new Guid("a0000000-0000-0000-0000-000000000005"),
                    TransactionType = InventoryTransactionType.Adjustment,
                    RuleCode = "ADJUSTMENT_POSITIVE",
                    Description = "Positive inventory adjustment (overage)",
                    DebitAccountCode = "1.1.03.001", // Inventory
                    CreditAccountCode = "4.2.01.001", // Other Operating Income
                    Condition = "Quantity > 0",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                },
                // Adjustment - Negative: Debit Other Expenses, Credit Inventory
                new AccountingRuleEntity
                {
                    Id = new Guid("a0000000-0000-0000-0000-000000000006"),
                    TransactionType = InventoryTransactionType.Adjustment,
                    RuleCode = "ADJUSTMENT_NEGATIVE",
                    Description = "Negative inventory adjustment (shortage)",
                    DebitAccountCode = "3.2.01.002", // Other Operating Expenses
                    CreditAccountCode = "1.1.03.001", // Inventory
                    Condition = "Quantity < 0",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                },
                // Loss: Debit Inventory Loss, Credit Inventory
                new AccountingRuleEntity
                {
                    Id = new Guid("a0000000-0000-0000-0000-000000000007"),
                    TransactionType = InventoryTransactionType.Loss,
                    RuleCode = "LOSS",
                    Description = "Inventory loss, shrinkage, or write-off",
                    DebitAccountCode = "3.2.01.001", // Inventory Loss
                    CreditAccountCode = "1.1.03.001", // Inventory
                    Condition = null,
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                }
            );
    }
}
