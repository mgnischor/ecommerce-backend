namespace ECommerce.Domain.Enums;

/// <summary>
/// Accounting account types (Chart of Accounts - Brazilian GAAP NBC TG)
/// </summary>
public enum AccountType
{
    /// <summary>
    /// Asset - Resources owned by the entity
    /// </summary>
    Asset = 1,
    
    /// <summary>
    /// Liability - Obligations owed by the entity
    /// </summary>
    Liability = 2,
    
    /// <summary>
    /// Equity - Owner's equity / Net worth
    /// </summary>
    Equity = 3,
    
    /// <summary>
    /// Revenue - Income from operations
    /// </summary>
    Revenue = 4,
    
    /// <summary>
    /// Expense - Costs and operating expenses
    /// </summary>
    Expense = 5
}
