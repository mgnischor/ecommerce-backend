namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents a customer address (shipping or billing)
/// </summary>
public sealed class AddressEntity
{
    /// <summary>
    /// Unique identifier for the address
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Customer who owns this address
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Address type label (e.g., "Home", "Work", "Billing", "Shipping")
    /// </summary>
    public string AddressType { get; set; } = "Shipping";

    /// <summary>
    /// Full name of the recipient
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Phone number
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Street address line 1
    /// </summary>
    public string AddressLine1 { get; set; } = string.Empty;

    /// <summary>
    /// Street address line 2 (optional)
    /// </summary>
    public string? AddressLine2 { get; set; }

    /// <summary>
    /// City
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// State or province
    /// </summary>
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// Postal code or ZIP code
    /// </summary>
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>
    /// Country
    /// </summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is the default address
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Whether the address is deleted (soft delete)
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Date and time when the address was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the address was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
