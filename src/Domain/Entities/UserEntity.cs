using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents a user account in the e-commerce system.
/// </summary>
/// <remarks>
/// This entity stores user authentication details, profile information, access control,
/// and account status. Every user is identified by a unique GUID and can have different
/// access levels (Customer, Admin, Manager, etc.). The entity supports soft deletion,
/// account suspension (banning), and email verification workflows.
/// </remarks>
public class UserEntity
{
    /// <summary>
    /// Gets or sets the identifier of the user who created this user account.
    /// </summary>
    /// <value>A <see cref="Guid"/> referencing the creating user, or the user's own ID for self-registration.</value>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for this user.
    /// </summary>
    /// <value>A <see cref="Guid"/> that uniquely identifies the user account.</value>
    /// <example>7c9e6679-7425-40de-944b-e07fc1f90ae7</example>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last updated this user account.
    /// </summary>
    /// <value>A <see cref="Guid"/> referencing the user who performed the last modification.</value>
    /// <example>8b3c8d7a-9c4e-4f5a-8d3c-9e5f8a7b6c5d</example>
    public Guid UpdatedBy { get; set; }

    /// <summary>
    /// Gets or sets the access level defining the user's permissions.
    /// </summary>
    /// <value>
    /// A <see cref="UserAccessLevel"/> enumeration value determining the user's role and permissions.
    /// Defaults to <see cref="UserAccessLevel.Customer"/>.
    /// Valid values: Customer, Admin, Manager, Support, etc.
    /// </value>
    public UserAccessLevel AccessLevel { get; set; } = UserAccessLevel.Customer;

    /// <summary>
    /// Gets or sets the user's street address.
    /// </summary>
    /// <value>A <see cref="string"/> containing the user's physical address.</value>
    /// <example>123 Main Street, Apt 4B</example>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's city of residence.
    /// </summary>
    /// <value>A <see cref="string"/> containing the city name.</value>
    /// <example>New York, London, São Paulo</example>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's country of residence.
    /// </summary>
    /// <value>A <see cref="string"/> containing the country name or ISO code.</value>
    /// <example>United States, USA, BR, GB</example>
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> containing a valid email address.
    /// Used for authentication, communication, and password recovery.
    /// Must be unique across all users.
    /// </value>
    /// <example>user@example.com</example>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the hashed password for user authentication.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> containing the securely hashed password.
    /// The plain-text password should never be stored; only the hash is persisted.
    /// Typically uses bcrypt, PBKDF2, or Argon2 hashing algorithms.
    /// </value>
    /// <example>$2a$11$CwTycUXWue0Thq9StjUM0u</example>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique username for this user account.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> containing the username.
    /// Must be unique across all users and typically follows specific format rules.
    /// </value>
    /// <example>johndoe, user123, admin</example>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the user account is active.
    /// </summary>
    /// <value>
    /// <c>true</c> if the account is active and the user can log in;
    /// otherwise, <c>false</c> for deactivated accounts. Defaults to <c>true</c>.
    /// </value>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the user account is banned.
    /// </summary>
    /// <value>
    /// <c>true</c> if the account is banned and the user cannot access the system;
    /// otherwise, <c>false</c>. Defaults to <c>false</c>.
    /// Banned users may require admin intervention to regain access.
    /// </value>
    public bool IsBanned { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether debug mode is enabled for this user.
    /// </summary>
    /// <value>
    /// <c>true</c> if debug features and verbose logging should be enabled for this user;
    /// otherwise, <c>false</c>. Defaults to <c>false</c>.
    /// This is typically used for development and troubleshooting purposes.
    /// </value>
    public bool IsDebugEnabled { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the user account has been soft-deleted.
    /// </summary>
    /// <value>
    /// <c>true</c> if the account is deleted but retained for historical or audit purposes;
    /// otherwise, <c>false</c>. Defaults to <c>false</c>.
    /// </value>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the user's email address has been verified.
    /// </summary>
    /// <value>
    /// <c>true</c> if the email has been verified through a confirmation process;
    /// otherwise, <c>false</c>. Defaults to <c>false</c>.
    /// Unverified emails may have restricted functionality.
    /// </value>
    public bool IsEmailVerified { get; set; } = false;

    /// <summary>
    /// Gets or sets the collection of group identifiers this user belongs to.
    /// </summary>
    /// <value>
    /// A <see cref="List{T}"/> of <see cref="Guid"/> values representing user groups or roles.
    /// Used for group-based permissions and access control.
    /// </value>
    /// <example>[3fa85f64-5717-4562-b3fc-2c963f66afa6, 7c9e6679-7425-40de-944b-e07fc1f90ae7]</example>
    public List<Guid> Groups { get; set; } = new List<Guid>();

    /// <summary>
    /// Gets or sets the collection of product identifiers marked as favorites by this user.
    /// </summary>
    /// <value>
    /// A <see cref="List{T}"/> of <see cref="Guid"/> values representing favorite products.
    /// Used for quick access to frequently viewed or preferred products.
    /// </value>
    /// <example>[8b3c8d7a-9c4e-4f5a-8d3c-9e5f8a7b6c5d, 9d4e5f8b-0d5f-4a6b-9e8d-0f1a2b3c4d5e]</example>
    public List<Guid> FavoriteProducts { get; set; } = new List<Guid>();

    /// <summary>
    /// Gets or sets the user's date of birth.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> representing the user's birth date.
    /// Used for age verification and personalized marketing.
    /// </value>
    /// <example>1990-05-15</example>
    public DateTime BirthDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this user account was created.
    /// </summary>
    /// <value>A <see cref="DateTime"/> in UTC format representing the account creation timestamp.</value>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this user account was last updated.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> in UTC format representing the last modification timestamp.
    /// Updated whenever any user information is changed.
    /// </value>
    public DateTime UpdatedAt { get; set; }
}
