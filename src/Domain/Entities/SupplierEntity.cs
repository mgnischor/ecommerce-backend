namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents a supplier in the e-commerce system.
/// Manages product suppliers and their information.
/// </summary>
public class SupplierEntity
{
    /// <summary>
    /// Unique identifier for the supplier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User who created this supplier
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// User who last updated this supplier
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// Supplier company name
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Supplier code (unique identifier)
    /// </summary>
    public string SupplierCode { get; set; } = string.Empty;

    /// <summary>
    /// Contact person name
    /// </summary>
    public string ContactName { get; set; } = string.Empty;

    /// <summary>
    /// Contact email
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Contact phone number
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Alternative phone number
    /// </summary>
    public string? AlternatePhone { get; set; }

    /// <summary>
    /// Fax number
    /// </summary>
    public string? FaxNumber { get; set; }

    /// <summary>
    /// Website URL
    /// </summary>
    public string? Website { get; set; }

    /// <summary>
    /// Supplier address
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// City
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// State/Province
    /// </summary>
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// Postal code
    /// </summary>
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>
    /// Country
    /// </summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Tax identification number
    /// </summary>
    public string? TaxId { get; set; }

    /// <summary>
    /// Business registration number
    /// </summary>
    public string? RegistrationNumber { get; set; }

    /// <summary>
    /// Bank account number
    /// </summary>
    public string? BankAccountNumber { get; set; }

    /// <summary>
    /// Bank name
    /// </summary>
    public string? BankName { get; set; }

    /// <summary>
    /// Bank routing number
    /// </summary>
    public string? BankRoutingNumber { get; set; }

    /// <summary>
    /// Payment terms (e.g., "Net 30", "Net 60")
    /// </summary>
    public string? PaymentTerms { get; set; }

    /// <summary>
    /// Credit limit
    /// </summary>
    public decimal? CreditLimit { get; set; }

    /// <summary>
    /// Current balance owed to supplier
    /// </summary>
    public decimal CurrentBalance { get; set; } = 0m;

    /// <summary>
    /// Supplier rating (0-5)
    /// </summary>
    public decimal Rating { get; set; } = 0m;

    /// <summary>
    /// Lead time in days for orders
    /// </summary>
    public int LeadTimeDays { get; set; } = 0;

    /// <summary>
    /// Minimum order amount
    /// </summary>
    public decimal? MinimumOrderAmount { get; set; }

    /// <summary>
    /// Notes about the supplier
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Whether the supplier is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether the supplier is a preferred supplier
    /// </summary>
    public bool IsPreferred { get; set; } = false;

    /// <summary>
    /// Whether the supplier is deleted (soft delete)
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Date and time when the supplier was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the supplier was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
