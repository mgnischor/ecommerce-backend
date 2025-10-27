using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence;

/// <summary>
/// Seeds initial data into the database
/// </summary>
public static class DatabaseSeeder
{
    /// <summary>
    /// Seeds the admin user if it doesn't exist
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="passwordService">Password hashing service</param>
    public static async Task SeedAdminUserAsync(
        PostgresqlContext context,
        IPasswordService passwordService
    )
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        if (passwordService == null)
            throw new ArgumentNullException(nameof(passwordService));

        // Check if admin user already exists
        var adminEmail = "admin@ecommerce.com.br";
        var adminExists = await context.Users.AnyAsync(u => u.Email == adminEmail);

        if (!adminExists)
        {
            var adminUser = new UserEntity
            {
                Id = Guid.NewGuid(),
                Username = "admin",
                Email = adminEmail,
                PasswordHash = passwordService.HashPassword("admin"),
                AccessLevel = UserAccessLevel.Admin,
                IsActive = true,
                IsEmailVerified = true,
                IsBanned = false,
                IsDeleted = false,
                IsDebugEnabled = false,
                Address = "Admin Address",
                City = "São Paulo",
                Country = "Brazil",
                BirthDate = new DateTime(1990, 1, 1),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = Guid.Empty,
                UpdatedBy = Guid.Empty,
                Groups = new List<Guid>(),
                FavoriteProducts = new List<Guid>(),
            };

            await context.Users.AddAsync(adminUser);
            await context.SaveChangesAsync();
        }
    }
    
    /// <summary>
    /// Seeds the chart of accounts if it doesn't exist
    /// Based on Brazilian GAAP (NBC TG) - Simplified structure for e-commerce
    /// </summary>
    /// <param name="context">Database context</param>
    public static async Task SeedChartOfAccountsAsync(PostgresqlContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        // Check if chart of accounts already exists
        var accountsExist = await context.ChartOfAccounts.AnyAsync();
        if (accountsExist)
            return;

        var accounts = new List<ChartOfAccountsEntity>();

        // ===================================================================
        // ASSETS - Resources owned by the entity
        // ===================================================================

        // Current Assets - Realizable within 12 months
        accounts.AddRange(new[]
        {
            CreateAccount("1.1.01.001", "Cash", "Cash on hand and equivalents", AccountType.Asset),
            CreateAccount("1.1.02.001", "Bank - Checking Account", "Balance in checking accounts", AccountType.Asset),
            CreateAccount("1.1.02.002", "Short-term Investments", "Short-term financial investments", AccountType.Asset),
            CreateAccount("1.1.03.001", "Inventory", "Merchandise for resale", AccountType.Asset),
            CreateAccount("1.1.04.001", "Accounts Receivable - Customers", "Amounts to be received from credit sales", AccountType.Asset),
            CreateAccount("1.1.04.002", "Credit Card Receivables", "Amounts to be received from card processors", AccountType.Asset),
        });

        // Non-Current Assets - Realizable after 12 months
        accounts.AddRange(new[]
        {
            CreateAccount("1.2.01.001", "Property, Plant & Equipment - Computers", "Computers, servers, equipment", AccountType.Asset),
            CreateAccount("1.2.01.002", "Property, Plant & Equipment - Furniture", "Office furniture and fixtures", AccountType.Asset),
            CreateAccount("1.2.02.001", "Intangible - Software", "Software licenses and development", AccountType.Asset),
        });

        // ===================================================================
        // LIABILITIES - Obligations owed by the entity
        // ===================================================================

        // Current Liabilities - Due within 12 months
        accounts.AddRange(new[]
        {
            CreateAccount("2.1.01.001", "Accounts Payable - Suppliers", "Payables to merchandise suppliers", AccountType.Liability),
            CreateAccount("2.1.02.001", "Salaries Payable", "Salaries and employment charges payable", AccountType.Liability),
            CreateAccount("2.1.03.001", "Taxes Payable", "Taxes and contributions payable", AccountType.Liability),
            CreateAccount("2.1.04.001", "Loans and Financing", "Short-term loans", AccountType.Liability),
            CreateAccount("2.1.05.001", "Customer Advances", "Amounts received in advance", AccountType.Liability),
        });

        // Non-Current Liabilities - Due after 12 months
        accounts.AddRange(new[]
        {
            CreateAccount("2.2.01.001", "Long-term Loans", "Financing due after 12 months", AccountType.Liability),
        });

        // ===================================================================
        // EQUITY - Owner's equity / Net worth
        // ===================================================================

        accounts.AddRange(new[]
        {
            CreateAccount("3.1.01.001", "Share Capital", "Capital invested by shareholders", AccountType.Equity),
            CreateAccount("3.2.01.001", "Retained Earnings", "Profits retained in the company", AccountType.Equity),
            CreateAccount("3.3.01.001", "Accumulated Profit/Loss", "Current period accumulated result", AccountType.Equity),
        });

        // ===================================================================
        // REVENUE - Income from operations
        // ===================================================================

        accounts.AddRange(new[]
        {
            CreateAccount("4.1.01.001", "Product Sales Revenue", "Gross sales revenue", AccountType.Revenue),
            CreateAccount("4.1.02.001", "Service Revenue", "Revenue from services", AccountType.Revenue),
            CreateAccount("4.1.03.001", "Sales Returns", "Cancelled sales and returns (deduction)", AccountType.Revenue),
            CreateAccount("4.1.04.001", "Sales Discounts", "Discounts given to customers (deduction)", AccountType.Revenue),
            CreateAccount("4.1.05.001", "Sales Taxes", "VAT, sales tax (deduction)", AccountType.Revenue),
            CreateAccount("4.2.01.001", "Other Operating Income", "Miscellaneous operating income", AccountType.Revenue),
            CreateAccount("4.3.01.001", "Financial Income", "Interest, investment income, foreign exchange gains", AccountType.Revenue),
        });

        // ===================================================================
        // EXPENSES - Costs and operating expenses
        // ===================================================================

        // Cost of Goods Sold
        accounts.AddRange(new[]
        {
            CreateAccount("5.1.01.001", "Cost of Goods Sold", "COGS - Direct cost of sales", AccountType.Expense),
            CreateAccount("5.1.02.001", "Freight on Sales", "Shipping cost for deliveries", AccountType.Expense),
        });

        // Operating Expenses
        accounts.AddRange(new[]
        {
            CreateAccount("5.2.01.001", "Inventory Loss", "Loss, shrinkage and obsolescence", AccountType.Expense),
            CreateAccount("5.2.01.002", "Other Operating Expenses", "Miscellaneous operating expenses", AccountType.Expense),
            CreateAccount("5.2.02.001", "Personnel Expenses", "Salaries, benefits and payroll taxes", AccountType.Expense),
            CreateAccount("5.2.03.001", "Marketing Expenses", "Advertising, promotion and digital marketing", AccountType.Expense),
            CreateAccount("5.2.04.001", "Technology Expenses", "Servers, hosting, SaaS subscriptions", AccountType.Expense),
            CreateAccount("5.2.05.001", "Administrative Expenses", "Rent, utilities, telephone", AccountType.Expense),
            CreateAccount("5.2.06.001", "Credit Card Fees", "Fees from card processors", AccountType.Expense),
            CreateAccount("5.2.07.001", "Marketplace Fees", "Commissions from marketplaces and platforms", AccountType.Expense),
        });

        // Financial Expenses
        accounts.AddRange(new[]
        {
            CreateAccount("5.3.01.001", "Interest Expense", "Interest on loans and financing", AccountType.Expense),
            CreateAccount("5.3.02.001", "Bank Fees", "Bank charges and transaction fees", AccountType.Expense),
        });

        // Add all accounts to database
        await context.ChartOfAccounts.AddRangeAsync(accounts);
        await context.SaveChangesAsync();
    }

    private static ChartOfAccountsEntity CreateAccount(
        string code,
        string name,
        string description,
        AccountType type)
    {
        return new ChartOfAccountsEntity
        {
            Id = Guid.NewGuid(),
            AccountCode = code,
            AccountName = name,
            Description = description,
            AccountType = type,
            IsAnalytic = true,
            IsActive = true,
            Balance = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
