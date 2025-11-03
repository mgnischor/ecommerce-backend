using ECommerce.API.Controllers;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Tests.API.Controllers;

/// <summary>
/// Tests for UserController
/// </summary>
[TestFixture]
public class UserControllerTests : DatabaseTestFixture
{
    private Mock<IUserRepository> _mockUserRepository;
    private Mock<ILogger<UserController>> _mockLogger;
    private UserController _controller;
    private PostgresqlContext _context;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockLogger = new Mock<ILogger<UserController>>();
        _context = CreateInMemoryDbContext();

        _controller = new UserController(_mockUserRepository.Object, _context, _mockLogger.Object);

        // Set up HttpContext for Response.Headers access
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext(),
        };
    }

    [TearDown]
    public override void TearDown()
    {
        _context?.Dispose();
        base.TearDown();
    }

    [Test]
    public async Task GetAllUsers_WithValidPagination_ReturnsOkWithUsers()
    {
        // Arrange
        int pageNumber = 1;
        int pageSize = 10;

        var users = new List<UserEntity>
        {
            new UserEntity
            {
                Id = Guid.NewGuid(),
                Email = "user1@example.com",
                Username = "user1",
                PasswordHash = "hash1",
                AccessLevel = UserAccessLevel.Customer,
                IsActive = true,
            },
            new UserEntity
            {
                Id = Guid.NewGuid(),
                Email = "user2@example.com",
                Username = "user2",
                PasswordHash = "hash2",
                AccessLevel = UserAccessLevel.Customer,
                IsActive = true,
            },
        };

        _mockUserRepository
            .Setup(x => x.GetPagedAsync(pageNumber, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        _mockUserRepository
            .Setup(x => x.GetCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users.Count);

        // Act
        var result = await _controller.GetAllUsers(pageNumber, pageSize);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var returnedUsers = okResult!.Value as List<UserEntity>;
        returnedUsers.Should().HaveCount(2);
    }

    [Test]
    public async Task GetAllUsers_WithInvalidPageNumber_ReturnsBadRequest()
    {
        // Arrange
        int pageNumber = 0;
        int pageSize = 10;

        // Act
        var result = await _controller.GetAllUsers(pageNumber, pageSize);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task GetUserById_WithExistingId_ReturnsOkWithUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new UserEntity
        {
            Id = userId,
            Email = "test@example.com",
            Username = "testuser",
            PasswordHash = "hash",
            AccessLevel = UserAccessLevel.Customer,
            IsActive = true,
        };

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.GetUserById(userId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var returnedUser = okResult!.Value as UserEntity;
        returnedUser.Should().NotBeNull();
        returnedUser!.Id.Should().Be(userId);
    }

    [Test]
    public async Task GetUserById_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        // Act
        var result = await _controller.GetUserById(userId);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task CreateUser_WithValidUser_ReturnsCreated()
    {
        // Arrange
        var newUser = new UserEntity
        {
            Email = "newuser@example.com",
            Username = "newuser",
            PasswordHash = "hash",
            AccessLevel = UserAccessLevel.Customer,
            IsActive = true,
        };

        _mockUserRepository
            .Setup(x => x.ExistsByEmailAsync(newUser.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockUserRepository
            .Setup(x => x.ExistsByUsernameAsync(newUser.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.CreateUser(newUser);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result.Result as CreatedAtActionResult;
        createdResult.Should().NotBeNull();
    }

    [Test]
    public async Task CreateUser_WithDuplicateEmail_ReturnsConflict()
    {
        // Arrange
        var newUser = new UserEntity { Email = "existing@example.com", Username = "newuser" };

        _mockUserRepository
            .Setup(x => x.ExistsByEmailAsync(newUser.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.CreateUser(newUser);

        // Assert
        result.Result.Should().BeOfType<ConflictObjectResult>();
    }

    [Test]
    public async Task CreateUser_WithDuplicateUsername_ReturnsConflict()
    {
        // Arrange
        var newUser = new UserEntity { Email = "new@example.com", Username = "existinguser" };

        _mockUserRepository
            .Setup(x => x.ExistsByEmailAsync(newUser.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockUserRepository
            .Setup(x => x.ExistsByUsernameAsync(newUser.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.CreateUser(newUser);

        // Assert
        result.Result.Should().BeOfType<ConflictObjectResult>();
    }

    [Test]
    public async Task UpdateUser_WithValidUser_ReturnsNoContent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new UserEntity
        {
            Id = userId,
            Email = "existing@example.com",
            Username = "existinguser",
        };

        var updatedUser = new UserEntity
        {
            Id = userId,
            Email = "updated@example.com",
            Username = "updateduser",
        };

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(updatedUser.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        _mockUserRepository
            .Setup(x => x.GetByUsernameAsync(updatedUser.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        // Act
        var result = await _controller.UpdateUser(userId, updatedUser);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Test]
    public async Task UpdateUser_WithIdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var differentId = Guid.NewGuid();

        var updatedUser = new UserEntity { Id = differentId, Email = "test@example.com" };

        // Act
        var result = await _controller.UpdateUser(userId, updatedUser);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task UpdateUser_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updatedUser = new UserEntity { Id = userId, Email = "test@example.com" };

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        // Act
        var result = await _controller.UpdateUser(userId, updatedUser);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task DeleteUser_WithExistingId_ReturnsNoContent()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockUserRepository
            .Setup(x => x.RemoveByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteUser(userId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Test]
    public async Task DeleteUser_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockUserRepository
            .Setup(x => x.RemoveByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteUser(userId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    // Note: GetEndpoints and GetOptions tests are integration tests
    // These are metadata endpoints that don't require extensive unit testing
}
