namespace ECommerce.Domain.Enums;

/// <summary>
/// Represents the type of address
/// </summary>
public enum AddressType
{
    /// <summary>
    /// Shipping address
    /// </summary>
    Shipping = 0,

    /// <summary>
    /// Billing address
    /// </summary>
    Billing = 1,

    /// <summary>
    /// Both shipping and billing
    /// </summary>
    Both = 2,

    /// <summary>
    /// Home address
    /// </summary>
    Home = 3,

    /// <summary>
    /// Work/office address
    /// </summary>
    Work = 4,

    /// <summary>
    /// Other address type
    /// </summary>
    Other = 5
}
