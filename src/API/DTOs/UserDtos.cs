using System.ComponentModel.DataAnnotations;
using ECommerce.Domain.Enums;

namespace ECommerce.API.DTOs;

public sealed class UserResponseDto
{
    public Guid Id { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid UpdatedBy { get; set; }
    public UserAccessLevel AccessLevel { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsBanned { get; set; }
    public bool IsDebugEnabled { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsEmailVerified { get; set; }
    public List<Guid> Groups { get; set; } = new();
    public List<Guid> FavoriteProducts { get; set; } = new();
    public DateTime BirthDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockedUntil { get; set; }
    public DateTime? LastFailedLoginAt { get; set; }
    public DateTime? LastSuccessfulLoginAt { get; set; }
    public string? LastLoginIpAddress { get; set; }
}

public sealed class CreateUserRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(3)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    public UserAccessLevel AccessLevel { get; set; } = UserAccessLevel.Customer;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsBanned { get; set; } = false;
    public bool IsDebugEnabled { get; set; } = false;
    public bool IsDeleted { get; set; } = false;
    public bool IsEmailVerified { get; set; } = false;
    public List<Guid>? Groups { get; set; }
    public List<Guid>? FavoriteProducts { get; set; }
    public DateTime BirthDate { get; set; }
}

public sealed class UpdateOwnUserRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(3)]
    public string Username { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
}

public sealed class UpdateUserAdminRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(3)]
    public string Username { get; set; } = string.Empty;

    public UserAccessLevel AccessLevel { get; set; } = UserAccessLevel.Customer;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsBanned { get; set; } = false;
    public bool IsDebugEnabled { get; set; } = false;
    public bool IsDeleted { get; set; } = false;
    public bool IsEmailVerified { get; set; } = false;
    public List<Guid>? Groups { get; set; }
    public List<Guid>? FavoriteProducts { get; set; }
    public DateTime BirthDate { get; set; }
}
