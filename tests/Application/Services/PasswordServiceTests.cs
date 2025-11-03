using ECommerce.Tests.TestFixtures;

namespace ECommerce.Tests.Application.Services;

/// <summary>
/// Unit tests for PasswordService
/// </summary>
[TestFixture]
public class PasswordServiceTests : BaseTestFixture
{
    private PasswordService? _passwordService;
    private Mock<ILoggingService>? _mockLogger;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        _mockLogger = new Mock<ILoggingService>();
        _passwordService = new PasswordService(_mockLogger.Object);
    }

    [Test]
    public void HashPassword_WhenCalledWithValidPassword_ShouldReturnHashedPassword()
    {
        // Arrange
        var password = "SecurePassword123!";

        // Act
        var hashedPassword = _passwordService!.HashPassword(password);

        // Assert
        hashedPassword.Should().NotBeNullOrEmpty();
        hashedPassword.Should().NotBe(password);
    }

    [Test]
    public void HashPassword_WhenCalledMultipleTimes_ShouldGenerateDifferentHashes()
    {
        // Arrange
        var password = "SecurePassword123!";

        // Act
        var hash1 = _passwordService!.HashPassword(password);
        var hash2 = _passwordService.HashPassword(password);

        // Assert
        hash1.Should().NotBe(hash2);
        // Each hash should be unique due to salt
    }

    [Test]
    public void VerifyPassword_WhenPasswordMatches_ShouldReturnTrue()
    {
        // Arrange
        var password = "SecurePassword123!";
        var hashedPassword = _passwordService!.HashPassword(password);

        // Act
        var result = _passwordService.VerifyPassword(password, hashedPassword);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void VerifyPassword_WhenPasswordDoesNotMatch_ShouldReturnFalse()
    {
        // Arrange
        var password = "SecurePassword123!";
        var wrongPassword = "WrongPassword456!";
        var hashedPassword = _passwordService!.HashPassword(password);

        // Act
        var result = _passwordService.VerifyPassword(wrongPassword, hashedPassword);

        // Assert
        result.Should().BeFalse();
    }

    [TestCase("")]
    [TestCase(" ")]
    public void HashPassword_WhenCalledWithEmptyPassword_ShouldThrowArgumentException(
        string password
    )
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _passwordService!.HashPassword(password));
    }

    [Test]
    public void VerifyPassword_WhenHashIsInvalid_ShouldReturnFalse()
    {
        // Arrange
        var password = "SecurePassword123!";
        var invalidHash = "not-a-valid-hash";

        // Act
        var result = _passwordService!.VerifyPassword(password, invalidHash);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void VerifyPassword_WhenPasswordIsNull_ShouldThrowArgumentException()
    {
        // Arrange
        var hashedPassword = _passwordService!.HashPassword("SomePassword123!");

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _passwordService.VerifyPassword(null!, hashedPassword)
        );
    }
}
