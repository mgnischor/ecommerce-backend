using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents a vendor/seller in the e-commerce system.
/// Supports marketplace model with multiple vendors.
/// </summary>
public class VendorEntity
{
    /// <summary>
    /// Unique identifier for the vendor
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User who created this vendor
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// User who last updated this vendor
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// User ID associated with this vendor account
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Business name
    /// </summary>
    public string BusinessName { get; set; } = string.Empty;

    /// <summary>
    /// Store name (display name)
    /// </summary>
    public string StoreName { get; set; } = string.Empty;

    /// <summary>
    /// Business email
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Contact phone number
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Business description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Logo URL
    /// </summary>
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Banner image URL
    /// </summary>
    public string? BannerUrl { get; set; }

    /// <summary>
    /// Business address
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
    /// Commission percentage charged by platform
    /// </summary>
    public decimal CommissionRate { get; set; } = 10.0m;

    /// <summary>
    /// Vendor rating (0-5)
    /// </summary>
    public decimal Rating { get; set; } = 0m;

    /// <summary>
    /// Total number of ratings
    /// </summary>
    public int TotalRatings { get; set; } = 0;

    /// <summary>
    /// Total sales amount
    /// </summary>
    public decimal TotalSales { get; set; } = 0m;

    /// <summary>
    /// Total number of orders
    /// </summary>
    public int TotalOrders { get; set; } = 0;

    /// <summary>
    /// Current status of the vendor
    /// </summary>
    public VendorStatus Status { get; set; } = VendorStatus.Pending;

    /// <summary>
    /// Bank account number for payouts
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
    /// PayPal email for payouts
    /// </summary>
    public string? PayPalEmail { get; set; }

    /// <summary>
    /// Whether the vendor is verified
    /// </summary>
    public bool IsVerified { get; set; } = false;

    /// <summary>
    /// Date when vendor was verified
    /// </summary>
    public DateTime? VerifiedAt { get; set; }

    /// <summary>
    /// Whether the vendor is featured
    /// </summary>
    public bool IsFeatured { get; set; } = false;

    /// <summary>
    /// Whether the vendor is deleted (soft delete)
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Date and time when the vendor was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the vendor was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
