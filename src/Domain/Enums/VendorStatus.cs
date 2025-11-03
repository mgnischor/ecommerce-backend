namespace ECommerce.Domain.Enums;

/// <summary>
/// Represents the status of a vendor
/// </summary>
public enum VendorStatus
{
    /// <summary>
    /// Vendor application is pending approval
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Vendor is active and can sell
    /// </summary>
    Active = 1,

    /// <summary>
    /// Vendor account is suspended
    /// </summary>
    Suspended = 2,

    /// <summary>
    /// Vendor account is inactive
    /// </summary>
    Inactive = 3,

    /// <summary>
    /// Vendor account is rejected
    /// </summary>
    Rejected = 4
}
