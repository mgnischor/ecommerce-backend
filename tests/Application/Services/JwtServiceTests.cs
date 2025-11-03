using ECommerce.Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Tests.Application.Services;

/// <summary>
/// Tests for JwtService
/// </summary>
[TestFixture]
public class JwtServiceTests : BaseTestFixture
{
    private Mock<IConfiguration> _mockConfiguration;
    private Mock<ILoggingService> _mockLogger;
    private Mock<IConfigurationSection> _mockJwtSection;
    private JwtService _jwtService;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();

        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILoggingService>();
        _mockJwtSection = new Mock<IConfigurationSection>();

        // Setup JWT configuration
        _mockConfiguration
            .Setup(c => c["Jwt:SecretKey"])
            .Returns("ThisIsAVerySecureSecretKeyForJWTToken12345");
        _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
        _mockConfiguration.Setup(c => c["Jwt:ExpirationMinutes"]).Returns("60");

        _jwtService = new JwtService(_mockConfiguration.Object, _mockLogger.Object);
    }

    [Test]
    public void Constructor_WithNullConfiguration_ThrowsException()
    {
        // Act
        Action act = () => new JwtService(null, _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("configuration");
    }

    [Test]
    public void Constructor_WithNullLogger_ThrowsException()
    {
        // Act
        Action act = () => new JwtService(_mockConfiguration.Object, null);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Test]
    public void Constructor_WithMissingSecretKey_ThrowsException()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["Jwt:SecretKey"]).Returns((string)null);
        mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        mockConfig.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");

        // Act
        Action act = () => new JwtService(mockConfig.Object, _mockLogger.Object);

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*JWT SecretKey is not configured*");
    }

    [Test]
    public void GenerateToken_WithValidUser_ReturnsToken()
    {
        // Arrange
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Username = "testuser",
            AccessLevel = UserAccessLevel.Customer,
        };

        // Act
        var token = _jwtService.GenerateToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Should().Contain(".");
    }

    [Test]
    public void GenerateToken_WithNullUser_ThrowsException()
    {
        // Act
        Action act = () => _jwtService.GenerateToken(null);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("user");
    }

    [Test]
    public void GenerateToken_WithDifferentUsers_GeneratesDifferentTokens()
    {
        // Arrange
        var user1 = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = "user1@example.com",
            Username = "user1",
            AccessLevel = UserAccessLevel.Customer,
        };

        var user2 = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = "user2@example.com",
            Username = "user2",
            AccessLevel = UserAccessLevel.Admin,
        };

        // Act
        var token1 = _jwtService.GenerateToken(user1);
        var token2 = _jwtService.GenerateToken(user2);

        // Assert
        token1.Should().NotBe(token2);
    }

    [Test]
    public void GetTokenExpirationSeconds_ReturnsCorrectValue()
    {
        // Act
        var expirationSeconds = _jwtService.GetTokenExpirationSeconds();

        // Assert
        expirationSeconds.Should().Be(3600); // 60 minutes * 60 seconds
    }

    [Test]
    public void GenerateToken_LogsInformation()
    {
        // Arrange
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Username = "testuser",
            AccessLevel = UserAccessLevel.Customer,
        };

        // Act
        _jwtService.GenerateToken(user);

        // Assert
        _mockLogger.Verify(
            x => x.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()),
            Times.AtLeastOnce
        );
    }
}
