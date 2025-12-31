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
        using (Assert.EnterMultipleScope())
        {
            Assert.That(user.AccessLevel, Is.EqualTo(UserAccessLevel.Customer));
            Assert.That(user.IsActive, Is.True);
            Assert.That(user.IsBanned, Is.False);
            Assert.That(user.IsDebugEnabled, Is.False);
            Assert.That(user.IsDeleted, Is.False);
            Assert.That(user.IsEmailVerified, Is.False);
            Assert.That(user.Groups, Is.Not.Null);
            Assert.That(user.FavoriteProducts, Is.Not.Null);
        }
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
        Assert.That(user.Email, Is.EqualTo(email));
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
        Assert.That(user.Username, Is.EqualTo(username));
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
        Assert.That(user.PasswordHash, Is.EqualTo(passwordHash));
    }

    [Test]
    public void UserEntity_SetAccessLevel_UpdatesCorrectly()
    {
        // Arrange
        var user = new UserEntity();

        // Act
        user.AccessLevel = UserAccessLevel.Admin;

        // Assert
        Assert.That(user.AccessLevel, Is.EqualTo(UserAccessLevel.Admin));
    }

    [Test]
    public void UserEntity_CanBeDeactivated()
    {
        // Arrange
        var user = new UserEntity { IsActive = true };

        // Act
        user.IsActive = false;

        // Assert
        Assert.That(user.IsActive, Is.False);
    }

    [Test]
    public void UserEntity_CanBeBanned()
    {
        // Arrange
        var user = new UserEntity { IsBanned = false };

        // Act
        user.IsBanned = true;

        // Assert
        Assert.That(user.IsBanned, Is.True);
    }

    [Test]
    public void UserEntity_CanVerifyEmail()
    {
        // Arrange
        var user = new UserEntity { IsEmailVerified = false };

        // Act
        user.IsEmailVerified = true;

        // Assert
        Assert.That(user.IsEmailVerified, Is.True);
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
        Assert.That(user.Groups, Does.Contain(groupId));
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
        Assert.That(user.FavoriteProducts, Does.Contain(productId));
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
        using (Assert.EnterMultipleScope())
        {
            Assert.That(user.Address, Is.EqualTo(address));
            Assert.That(user.City, Is.EqualTo(city));
            Assert.That(user.Country, Is.EqualTo(country));
        }
    }

    [Test]
    public void UserEntity_HasTimestamps()
    {
        // Arrange & Act
        var user = new UserEntity { CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(
                user.CreatedAt,
                Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(1))
            );
            Assert.That(
                user.UpdatedAt,
                Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(1))
            );
        }
    }

    [Test]
    public void UserEntity_CanSoftDelete()
    {
        // Arrange
        var user = new UserEntity { IsDeleted = false };

        // Act
        user.IsDeleted = true;

        // Assert
        Assert.That(user.IsDeleted, Is.True);
    }
}
