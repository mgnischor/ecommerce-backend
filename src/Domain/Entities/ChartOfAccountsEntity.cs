using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents an accounting account in the Chart of Accounts.
/// </summary>
/// <remarks>
/// The Chart of Accounts defines the structure of all accounting accounts in the system.
/// This implementation follows NBC TG 26 (Brazilian GAAP - Presentation of Financial Statements)
/// and supports hierarchical account structures with parent-child relationships.
/// Accounts are classified by type (Asset, Liability, Equity, Revenue, Expense) and can be
/// either analytic (accepts direct journal entries) or synthetic (summary-only accounts).
/// The balance is automatically maintained through journal entry postings.
/// </remarks>
public sealed class ChartOfAccountsEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for this account.
    /// </summary>
    /// <value>A <see cref="Guid"/> that uniquely identifies the accounting account.</value>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the structured account code.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> containing the hierarchical account code.
    /// Typically follows a dot-separated numeric format indicating the account level and position.
    /// </value>
    /// <example>1.1.01.001 (Current Assets > Cash > Bank Account), 4.1.01.001 (Revenue > Sales)</example>
    public string AccountCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the descriptive name of the account.
    /// </summary>
    /// <value>A <see cref="string"/> containing the account name as it appears in reports.</value>
    /// <example>Cash and Cash Equivalents, Accounts Receivable, Sales Revenue, Cost of Goods Sold</example>
    public string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional detailed description of the account's purpose and usage.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> providing additional information about the account,
    /// or <c>null</c> if no description is provided.
    /// </value>
    /// <example>This account tracks all cash held in checking accounts and includes bank deposits and withdrawals.</example>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the classification type of this account.
    /// </summary>
    /// <value>
    /// An <see cref="AccountType"/> enumeration value categorizing the account per the accounting equation.
    /// Valid values: <see cref="AccountType.Asset"/>, <see cref="AccountType.Liability"/>,
    /// <see cref="AccountType.Equity"/>, <see cref="AccountType.Revenue"/>, <see cref="AccountType.Expense"/>.
    /// </value>
    public AccountType AccountType { get; set; }

    /// <summary>
    /// Gets or sets the optional identifier of the parent account in the hierarchy.
    /// </summary>
    /// <value>
    /// A <see cref="Guid"/> referencing the parent <see cref="ChartOfAccountsEntity"/>,
    /// or <c>null</c> if this is a top-level account.
    /// Parent accounts typically consolidate balances from child accounts.
    /// </value>
    /// <example>7c9e6679-7425-40de-944b-e07fc1f90ae7</example>
    public Guid? ParentAccountId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this account accepts direct journal entries.
    /// </summary>
    /// <value>
    /// <c>true</c> if the account is analytic and can have transactions posted directly to it;
    /// <c>false</c> if the account is synthetic and only shows consolidated balances from child accounts.
    /// Defaults to <c>true</c>.
    /// </value>
    /// <remarks>
    /// Analytic accounts are leaf nodes in the account hierarchy and accept journal entry postings.
    /// Synthetic accounts are parent/summary accounts with balances calculated from their children.
    /// </remarks>
    public bool IsAnalytic { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether this account is currently active.
    /// </summary>
    /// <value>
    /// <c>true</c> if the account is active and available for use;
    /// <c>false</c> if the account is inactive and should not accept new transactions.
    /// Defaults to <c>true</c>.
    /// </value>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the current balance of this account.
    /// </summary>
    /// <value>
    /// A <see cref="decimal"/> representing the account balance in the system's base currency.
    /// For Asset and Expense accounts, debits increase the balance and credits decrease it.
    /// For Liability, Equity, and Revenue accounts, credits increase the balance and debits decrease it.
    /// This balance is automatically maintained through journal entry postings.
    /// </value>
    /// <example>15000.00, -5000.00, 0.00</example>
    public decimal Balance { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this account was created.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> in UTC format representing the creation timestamp.
    /// Defaults to <see cref="DateTime.UtcNow"/> when the entity is instantiated.
    /// </value>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when this account was last updated.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> in UTC format representing the last modification timestamp.
    /// Defaults to <see cref="DateTime.UtcNow"/> when the entity is instantiated.
    /// Updated when account properties are changed or when the balance is recalculated.
    /// </value>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
