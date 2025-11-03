using System.Threading;
using ECommerce.API.Controllers;
using ECommerce.API.DTOs;
using ECommerce.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Tests.API.Controllers;

/// <summary>
/// Tests for AuthController
/// </summary>
[TestFixture]
public class AuthControllerTests : BaseTestFixture
{
    private Mock<IUserRepository> _mockUserRepository;
    private Mock<IJwtService> _mockJwtService;
    private Mock<IPasswordService> _mockPasswordService;
    private Mock<ILogger<AuthController>> _mockLogger;
    private AuthController _controller;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockJwtService = new Mock<IJwtService>();
        _mockPasswordService = new Mock<IPasswordService>();
        _mockLogger = new Mock<ILogger<AuthController>>();

        _controller = new AuthController(
            _mockUserRepository.Object,
            _mockJwtService.Object,
            _mockPasswordService.Object,
            _mockLogger.Object
        );
    }

    [Test]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "admin@ecommerce.com.br",
            Password = "admin",
        };

        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = loginRequest.Email,
            PasswordHash = "hashed_password",
            Username = "testuser",
            AccessLevel = UserAccessLevel.Customer,
            IsActive = true,
            IsBanned = false,
            IsDeleted = false,
        };

        var token = "jwt_token_here";

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(loginRequest.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockPasswordService
            .Setup(x => x.VerifyPassword(loginRequest.Password, user.PasswordHash))
            .Returns(true);

        _mockJwtService.Setup(x => x.GenerateToken(user)).Returns(token);

        _mockJwtService.Setup(x => x.GetTokenExpirationSeconds()).Returns(3600);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeOfType<LoginResponseDto>();

        var response = okResult.Value as LoginResponseDto;
        response!.Token.Should().Be(token);
        response.UserId.Should().Be(user.Id);
        response.Email.Should().Be(user.Email);
        response.TokenType.Should().Be("Bearer");
    }

    [Test]
    public async Task Login_WithNullRequest_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.Login(null!);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task Login_WithNonExistentUser_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "nonexistent@example.com",
            Password = "password123",
        };

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(loginRequest.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Test]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "wrongpassword",
        };

        var user = new UserEntity
        {
            Email = loginRequest.Email,
            PasswordHash = "hashed_password",
            IsActive = true,
        };

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(loginRequest.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockPasswordService
            .Setup(x => x.VerifyPassword(loginRequest.Password, user.PasswordHash))
            .Returns(false);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Test]
    public async Task Login_WithInactiveUser_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "password123",
        };

        var user = new UserEntity
        {
            Email = loginRequest.Email,
            PasswordHash = "hashed_password",
            IsActive = false,
        };

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(loginRequest.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockPasswordService
            .Setup(x => x.VerifyPassword(loginRequest.Password, user.PasswordHash))
            .Returns(true);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Test]
    public async Task Login_WithBannedUser_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "password123",
        };

        var user = new UserEntity
        {
            Email = loginRequest.Email,
            PasswordHash = "hashed_password",
            IsActive = true,
            IsBanned = true,
        };

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(loginRequest.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockPasswordService
            .Setup(x => x.VerifyPassword(loginRequest.Password, user.PasswordHash))
            .Returns(true);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Test]
    public void GetEndpoints_ReturnsListOfEndpoints()
    {
        // Act
        var result = _controller.GetEndpoints();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();
    }

    // Note: GetOptions test skipped as it requires HttpContext which is difficult to mock in unit tests
    // This is more suitable for integration testing
}
