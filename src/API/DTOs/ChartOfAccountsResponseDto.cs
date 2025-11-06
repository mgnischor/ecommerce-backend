namespace ECommerce.API.DTOs;

/// <summary>
/// Data Transfer Object (DTO) for chart of accounts response
/// </summary>
/// <remarks>
/// <para>
/// Represents an accounting account in the chart of accounts (COA) system.
/// The chart of accounts is the foundation of the accounting system, organizing
/// all financial transactions into categorized accounts.
/// </para>
/// <para>
/// <strong>Account Types:</strong>
/// </para>
/// <list type="bullet">
/// <item><description><strong>Asset:</strong> Resources owned by the business (cash, inventory, receivables)</description></item>
/// <item><description><strong>Liability:</strong> Obligations owed to others (payables, loans)</description></item>
/// <item><description><strong>Equity:</strong> Owner's stake in the business</description></item>
/// <item><description><strong>Revenue:</strong> Income from business operations</description></item>
/// <item><description><strong>Expense:</strong> Costs incurred in business operations</description></item>
/// </list>
/// <para>
/// <strong>Hierarchical Structure:</strong> Accounts can have parent-child relationships,
/// allowing for grouped reporting and detailed breakdown of financial data.
/// </para>
/// </remarks>
public sealed class ChartOfAccountsResponseDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the account
    /// </summary>
    /// <value>
    /// A globally unique identifier (GUID) that uniquely identifies this account in the system.
    /// </value>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the account code used for identification and reporting
    /// </summary>
    /// <value>
    /// A unique string code identifying the account, typically numeric or alphanumeric
    /// (e.g., "1000" for Cash, "4000" for Sales Revenue).
    /// </value>
    /// <remarks>
    /// Account codes are often structured hierarchically (e.g., 1000-1999 for Assets).
    /// This code is used in financial reports and journal entries for easy reference.
    /// </remarks>
    /// <example>1000</example>
    public string AccountCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the descriptive name of the account
    /// </summary>
    /// <value>
    /// A human-readable name that clearly describes the account's purpose
    /// (e.g., "Cash on Hand", "Accounts Receivable", "Sales Revenue").
    /// </value>
    /// <example>Cash on Hand</example>
    public string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the account type classification
    /// </summary>
    /// <value>
    /// The fundamental accounting category of the account. Valid values are:
    /// Asset, Liability, Equity, Revenue, or Expense.
    /// </value>
    /// <remarks>
    /// The account type determines the account's normal balance (debit or credit)
    /// and its position in financial statements.
    /// </remarks>
    /// <example>Asset</example>
    public string AccountType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the parent account identifier for hierarchical organization
    /// </summary>
    /// <value>
    /// The ID of the parent account if this is a sub-account, or <c>null</c> if this is a top-level account.
    /// </value>
    /// <remarks>
    /// Hierarchical accounts allow for detailed tracking while maintaining summary reporting.
    /// For example, "Cash on Hand" might be a child of "Current Assets".
    /// </remarks>
    /// <example>null</example>
    public Guid? ParentAccountId { get; set; }

    /// <summary>
    /// Gets or sets whether the account is analytic and can receive journal entries
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the account can receive journal entries (analytic/detail account);
    /// <see langword="false"/> if the account is only for grouping/summary purposes.
    /// </value>
    /// <remarks>
    /// Non-analytic accounts (also called header accounts) are used only for organization
    /// and reporting. Their balances are calculated from their child accounts.
    /// </remarks>
    /// <example>true</example>
    public bool IsAnalytic { get; set; }

    /// <summary>
    /// Gets or sets whether the account is currently active
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the account is active and can be used;
    /// <see langword="false"/> if the account is inactive and should not be used for new entries.
    /// </value>
    /// <remarks>
    /// Inactive accounts are retained for historical purposes but prevented from receiving
    /// new transactions. This maintains data integrity for past transactions.
    /// </remarks>
    /// <example>true</example>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the current balance of the account
    /// </summary>
    /// <value>
    /// The current monetary balance of the account in the system's base currency.
    /// For Asset and Expense accounts, positive balances represent debits.
    /// For Liability, Equity, and Revenue accounts, positive balances represent credits.
    /// </value>
    /// <remarks>
    /// This balance is calculated from all posted journal entries affecting this account.
    /// The balance is updated automatically when journal entries are posted or reversed.
    /// </remarks>
    /// <example>50000.00</example>
    public decimal Balance { get; set; }

    /// <summary>
    /// Gets or sets the optional detailed description of the account
    /// </summary>
    /// <value>
    /// Additional information about the account's purpose, usage guidelines, or special notes.
    /// Can be <c>null</c> if no description is provided.
    /// </value>
    /// <example>Main cash account for daily operations</example>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the account was created
    /// </summary>
    /// <value>
    /// The UTC date and time when this account was first created in the system.
    /// </value>
    /// <example>2025-01-15T10:30:00Z</example>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the last update to the account
    /// </summary>
    /// <value>
    /// The UTC date and time when this account was last modified.
    /// This includes changes to any account property but not balance updates from transactions.
    /// </value>
    /// <example>2025-11-06T14:45:30Z</example>
    public DateTime UpdatedAt { get; set; }
}
