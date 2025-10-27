using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

/// <summary>
/// Chart of Accounts - Defines the accounting accounts in the system
/// Based on NBC TG 26 (Presentation of Financial Statements)
/// </summary>
public sealed class ChartOfAccountsEntity
{
    /// <summary>
    /// Unique account identifier
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Account code (e.g., 1.1.01.001)
    /// </summary>
    public string AccountCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Account name
    /// </summary>
    public string AccountName { get; set; } = string.Empty;
    
    /// <summary>
    /// Account description
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Account type (Asset, Liability, Equity, Revenue, Expense)
    /// </summary>
    public AccountType AccountType { get; set; }
    
    /// <summary>
    /// Parent account ID (for hierarchical structure)
    /// </summary>
    public Guid? ParentAccountId { get; set; }
    
    /// <summary>
    /// Indicates if the account accepts direct entries (analytic account)
    /// </summary>
    public bool IsAnalytic { get; set; } = true;
    
    /// <summary>
    /// Indicates if the account is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Current account balance
    /// </summary>
    public decimal Balance { get; set; }
    
    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
