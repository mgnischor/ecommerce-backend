using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence;

/// <summary>
/// Seeds initial data into the database
/// </summary>
public static class DatabaseSeeder
{
    /// <summary>
    /// Seeds the admin user if it doesn't exist
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="passwordService">Password hashing service</param>
    public static async Task SeedAdminUserAsync(
        PostgresqlContext context,
        IPasswordService passwordService
    )
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        if (passwordService == null)
            throw new ArgumentNullException(nameof(passwordService));

        // Check if admin user already exists
        var adminEmail = "admin@ecommerce.com.br";
        var adminExists = await context.Users.AnyAsync(u => u.Email == adminEmail);

        if (!adminExists)
        {
            var adminUser = new UserEntity
            {
                Id = Guid.NewGuid(),
                Username = "admin",
                Email = adminEmail,
                PasswordHash = passwordService.HashPassword("admin"),
                AccessLevel = UserAccessLevel.Admin,
                IsActive = true,
                IsEmailVerified = true,
                IsBanned = false,
                IsDeleted = false,
                IsDebugEnabled = false,
                Address = "Admin Address",
                City = "São Paulo",
                Country = "Brazil",
                BirthDate = new DateTime(1990, 1, 1),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = Guid.Empty,
                UpdatedBy = Guid.Empty,
                Groups = new List<Guid>(),
                FavoriteProducts = new List<Guid>(),
            };

            await context.Users.AddAsync(adminUser);
            await context.SaveChangesAsync();
        }
    }
}
