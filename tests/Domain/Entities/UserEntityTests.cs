using ECommerce.Domain.Enums;

namespace ECommerce.Tests.Domain.Entities;

/// <summary>
/// Tests for UserEntity
/// </summary>
[TestFixture]
public class UserEntityTests : BaseTestFixture
{
    [Test]
    public void UserEntity_Creation_SetsDefaultValues()
    {
        // Act
        var user = new UserEntity();

        // Assert
        user.AccessLevel.Should().Be(UserAccessLevel.Customer);
        user.IsActive.Should().BeTrue();
        user.IsBanned.Should().BeFalse();
        user.IsDebugEnabled.Should().BeFalse();
        user.IsDeleted.Should().BeFalse();
        user.IsEmailVerified.Should().BeFalse();
        user.Groups.Should().NotBeNull();
        user.FavoriteProducts.Should().NotBeNull();
    }

    [Test]
    public void UserEntity_SetEmail_UpdatesCorrectly()
    {
        // Arrange
        var user = new UserEntity();
        var email = "test@example.com";

        // Act
        user.Email = email;

        // Assert
        user.Email.Should().Be(email);
    }

    [Test]
    public void UserEntity_SetUsername_UpdatesCorrectly()
    {
        // Arrange
        var user = new UserEntity();
        var username = "testuser";

        // Act
        user.Username = username;

        // Assert
        user.Username.Should().Be(username);
    }

    [Test]
    public void UserEntity_SetPasswordHash_UpdatesCorrectly()
    {
        // Arrange
        var user = new UserEntity();
        var passwordHash = "hashedpassword123";

        // Act
        user.PasswordHash = passwordHash;

        // Assert
        user.PasswordHash.Should().Be(passwordHash);
    }

    [Test]
    public void UserEntity_SetAccessLevel_UpdatesCorrectly()
    {
        // Arrange
        var user = new UserEntity();

        // Act
        user.AccessLevel = UserAccessLevel.Admin;

        // Assert
        user.AccessLevel.Should().Be(UserAccessLevel.Admin);
    }

    [Test]
    public void UserEntity_CanBeDeactivated()
    {
        // Arrange
        var user = new UserEntity { IsActive = true };

        // Act
        user.IsActive = false;

        // Assert
        user.IsActive.Should().BeFalse();
    }

    [Test]
    public void UserEntity_CanBeBanned()
    {
        // Arrange
        var user = new UserEntity { IsBanned = false };

        // Act
        user.IsBanned = true;

        // Assert
        user.IsBanned.Should().BeTrue();
    }

    [Test]
    public void UserEntity_CanVerifyEmail()
    {
        // Arrange
        var user = new UserEntity { IsEmailVerified = false };

        // Act
        user.IsEmailVerified = true;

        // Assert
        user.IsEmailVerified.Should().BeTrue();
    }

    [Test]
    public void UserEntity_CanAddToGroups()
    {
        // Arrange
        var user = new UserEntity();
        var groupId = Guid.NewGuid();

        // Act
        user.Groups.Add(groupId);

        // Assert
        user.Groups.Should().Contain(groupId);
    }

    [Test]
    public void UserEntity_CanAddFavoriteProducts()
    {
        // Arrange
        var user = new UserEntity();
        var productId = Guid.NewGuid();

        // Act
        user.FavoriteProducts.Add(productId);

        // Assert
        user.FavoriteProducts.Should().Contain(productId);
    }

    [Test]
    public void UserEntity_CanSetAddress()
    {
        // Arrange
        var user = new UserEntity();
        var address = "123 Main St";
        var city = "New York";
        var country = "USA";

        // Act
        user.Address = address;
        user.City = city;
        user.Country = country;

        // Assert
        user.Address.Should().Be(address);
        user.City.Should().Be(city);
        user.Country.Should().Be(country);
    }

    [Test]
    public void UserEntity_HasTimestamps()
    {
        // Arrange & Act
        var user = new UserEntity { CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };

        // Assert
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Test]
    public void UserEntity_CanSoftDelete()
    {
        // Arrange
        var user = new UserEntity { IsDeleted = false };

        // Act
        user.IsDeleted = true;

        // Assert
        user.IsDeleted.Should().BeTrue();
    }
}
