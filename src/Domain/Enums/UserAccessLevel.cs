namespace ECommerce.Domain.Enums;

/// <summary>
/// Defines various user access levels within the e-commerce system.
/// </summary>
public enum UserAccessLevel
{
    Guest = 0,
    Customer = 1,
    Company = 2,
    Admin = 3,
    Manager = 4,
    Developer = 99,
}
