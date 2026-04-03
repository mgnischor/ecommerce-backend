using System.Security.Claims;
using ECommerce.API.Controllers;
using ECommerce.API.DTOs;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Tests.API.Controllers;

[TestFixture]
public class UserControllerTests : DatabaseTestFixture
{
    private Mock<IUserRepository> _mockUserRepository = null!;
    private Mock<ILoggingService> _mockLogger = null!;
    private Mock<IPasswordService> _mockPasswordService = null!;
    private UserController _controller = null!;
    private PostgresqlContext _context = null!;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockLogger = new Mock<ILoggingService>();
        _mockPasswordService = new Mock<IPasswordService>();
        _context = CreateInMemoryDbContext();

        _controller = new UserController(
            _mockUserRepository.Object,
            _context,
            _mockLogger.Object,
            _mockPasswordService.Object
        );
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
    }

    [TearDown]
    public override void TearDown()
    {
        _context.Dispose();
        base.TearDown();
    }

    [Test]
    public async Task GetAllUsers_ReturnsSafeDto_WithoutPasswordHash()
    {
        var users = new List<UserEntity>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Email = "user1@example.com",
                Username = "user1",
                PasswordHash = "sensitive",
                AccessLevel = UserAccessLevel.Customer,
                IsActive = true,
            },
        };

        _mockUserRepository
            .Setup(x => x.GetPagedAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);
        _mockUserRepository.Setup(x => x.GetCountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _controller.GetAllUsers();

        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        var returnedUsers = ok!.Value as List<UserResponseDto>;
        returnedUsers.Should().NotBeNull();
        returnedUsers!.Should().HaveCount(1);
        returnedUsers[0].Email.Should().Be("user1@example.com");
        returnedUsers[0].GetType().GetProperty("PasswordHash").Should().BeNull();
    }

    [Test]
    public async Task GetUserById_WhenNotOwnerAndNotPrivileged_ReturnsForbid()
    {
        var targetUserId = Guid.NewGuid();
        var callerId = Guid.NewGuid();
        SetPrincipal(callerId, UserAccessLevel.Customer);

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(targetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserEntity { Id = targetUserId, Email = "a@b.com", Username = "u" });

        var result = await _controller.GetUserById(targetUserId);

        result.Result.Should().BeOfType<ForbidResult>();
    }

    [Test]
    public async Task GetUserById_WhenOwner_ReturnsOk()
    {
        var userId = Guid.NewGuid();
        SetPrincipal(userId, UserAccessLevel.Customer);

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserEntity { Id = userId, Email = "owner@example.com", Username = "owner" });

        var result = await _controller.GetUserById(userId);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task CreateUser_WithDto_ReturnsCreatedAndHashesPassword()
    {
        var request = new CreateUserRequestDto
        {
            Email = "new@example.com",
            Username = "newuser",
            Password = "password123",
        };

        _mockUserRepository
            .Setup(x => x.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _mockUserRepository
            .Setup(x => x.ExistsByUsernameAsync(request.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _mockPasswordService.Setup(x => x.HashPassword(request.Password)).Returns("hashed");

        var result = await _controller.CreateUser(request);

        result.Result.Should().BeOfType<CreatedAtActionResult>();
        _mockPasswordService.Verify(x => x.HashPassword(request.Password), Times.Once);
    }

    [Test]
    public async Task UpdateOwnUser_WhenOwner_UpdatesAllowedFieldsOnly()
    {
        var userId = Guid.NewGuid();
        SetPrincipal(userId, UserAccessLevel.Customer);
        var existing = new UserEntity
        {
            Id = userId,
            Email = "old@example.com",
            Username = "old",
            AccessLevel = UserAccessLevel.Admin,
        };
        var request = new UpdateOwnUserRequestDto
        {
            Email = "new@example.com",
            Username = "newname",
            Address = "Street",
            City = "City",
            Country = "Country",
            BirthDate = new DateTime(1990, 1, 1),
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _mockUserRepository.Setup(x => x.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _controller.UpdateOwnUser(userId, request);

        result.Should().BeOfType<NoContentResult>();
        existing.AccessLevel.Should().Be(UserAccessLevel.Admin);
    }

    [Test]
    public async Task UpdateUserAsAdmin_WithValidRequest_ReturnsNoContent()
    {
        var userId = Guid.NewGuid();
        var existing = new UserEntity { Id = userId, Email = "old@example.com", Username = "old" };
        var request = new UpdateUserAdminRequestDto
        {
            Email = "new@example.com",
            Username = "newname",
            AccessLevel = UserAccessLevel.Manager,
        };
        SetPrincipal(Guid.NewGuid(), UserAccessLevel.Admin);

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _mockUserRepository.Setup(x => x.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _controller.UpdateUserAsAdmin(userId, request);

        result.Should().BeOfType<NoContentResult>();
        existing.AccessLevel.Should().Be(UserAccessLevel.Manager);
    }

    [Test]
    public async Task DeleteUser_WithExistingId_ReturnsNoContent()
    {
        var userId = Guid.NewGuid();
        _mockUserRepository.Setup(x => x.RemoveByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _controller.DeleteUser(userId);

        result.Should().BeOfType<NoContentResult>();
    }

    private void SetPrincipal(Guid userId, UserAccessLevel accessLevel)
    {
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, accessLevel.ToString()),
            },
            "test"
        );
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);
    }
}
