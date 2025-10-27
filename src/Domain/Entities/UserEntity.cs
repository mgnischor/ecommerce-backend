using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents a user in the e-commerce system.
/// Every user is identified by a unique GUID.
/// Includes personal details, access level, status flags, and timestamps.
/// </summary>
public class UserEntity
{
    public Guid CreatedBy { get; set; }
    public Guid Id { get; set; }
    public Guid UpdatedBy { get; set; }

    public UserAccessLevel AccessLevel { get; set; } = UserAccessLevel.Customer;

    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public bool IsBanned { get; set; } = false;
    public bool IsDebugEnabled { get; set; } = false;
    public bool IsDeleted { get; set; } = false;
    public bool IsEmailVerified { get; set; } = false;

    public List<Guid> Groups { get; set; } = new List<Guid>();
    public List<Guid> FavoriteProducts { get; set; } = new List<Guid>();

    public DateTime BirthDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
