namespace ECommerce.API.DTOs;

/// <summary>
/// DTO for chart of accounts response
/// </summary>
public sealed class ChartOfAccountsResponseDto
{
    /// <summary>
    /// Account identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Account code
    /// </summary>
    public string AccountCode { get; set; } = string.Empty;

    /// <summary>
    /// Account name
    /// </summary>
    public string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// Account type (Asset, Liability, Equity, Revenue, Expense)
    /// </summary>
    public string AccountType { get; set; } = string.Empty;

    /// <summary>
    /// Parent account identifier
    /// </summary>
    public Guid? ParentAccountId { get; set; }

    /// <summary>
    /// Account level in hierarchy
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// Whether the account is analytic (can receive entries)
    /// </summary>
    public bool IsAnalytic { get; set; }

    /// <summary>
    /// Whether the account is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Current balance
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    /// Account description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
