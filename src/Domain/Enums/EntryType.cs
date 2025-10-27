namespace ECommerce.Domain.Enums;

/// <summary>
/// Accounting entry type (Debit or Credit)
/// </summary>
public enum EntryType
{
    /// <summary>
    /// Debit - Increases Asset/Expense, Decreases Liability/Revenue/Equity
    /// </summary>
    Debit = 1,
    
    /// <summary>
    /// Credit - Decreases Asset/Expense, Increases Liability/Revenue/Equity
    /// </summary>
    Credit = 2
}
