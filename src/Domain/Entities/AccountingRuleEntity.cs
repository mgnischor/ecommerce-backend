using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents an accounting rule that maps transaction types to chart of accounts.
/// </summary>
/// <remarks>
/// This entity defines the double-entry bookkeeping rules for each transaction type.
/// Instead of hardcoding account codes in the application logic, rules are stored
/// in the database and can be configured per transaction type.
/// Each rule specifies which accounts should be debited and credited for a given operation.
/// </remarks>
public sealed class AccountingRuleEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for this accounting rule.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the transaction type this rule applies to.
    /// </summary>
    /// <value>
    /// An <see cref="InventoryTransactionType"/> enumeration value.
    /// </value>
    public InventoryTransactionType TransactionType { get; set; }

    /// <summary>
    /// Gets or sets the rule code for identification.
    /// </summary>
    /// <value>
    /// A unique string identifier for the rule (e.g., "PURCHASE", "SALE", "ADJUSTMENT_POSITIVE").
    /// </value>
    public string RuleCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of this accounting rule.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the account code to be debited.
    /// </summary>
    /// <value>
    /// References <see cref="ChartOfAccountsEntity.AccountCode"/>.
    /// </value>
    public string DebitAccountCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the account code to be credited.
    /// </summary>
    /// <value>
    /// References <see cref="ChartOfAccountsEntity.AccountCode"/>.
    /// </value>
    public string CreditAccountCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional condition for applying this rule.
    /// </summary>
    /// <value>
    /// Used for conditional rules (e.g., "Quantity > 0" for positive adjustments).
    /// Null if the rule always applies to the transaction type.
    /// </value>
    public string? Condition { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this rule is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the date and time when this rule was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when this rule was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
