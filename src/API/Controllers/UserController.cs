using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using ECommerce.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/v1/users")]
[Produces("application/json")]
[Authorize]
public sealed class UserController : ControllerBase
{
    private readonly UserRepository _userRepository;
    private readonly PostgresqlContext _context;
    private readonly ILogger<UserController> _logger;

    public UserController(
        UserRepository userRepository,
        PostgresqlContext context,
        ILogger<UserController> logger
    )
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves all users with pagination support
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of users</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<UserEntity>>> GetAllUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation(
            "Getting all users - Page: {PageNumber}, PageSize: {PageSize}",
            pageNumber,
            pageSize
        );

        if (pageNumber < 1)
        {
            _logger.LogWarning("Invalid page number: {PageNumber}", pageNumber);
            return BadRequest("Page number must be greater than 0");
        }

        if (pageSize < 1 || pageSize > 100)
        {
            _logger.LogWarning("Invalid page size: {PageSize}", pageSize);
            return BadRequest("Page size must be between 1 and 100");
        }

        var users = await _userRepository.GetPagedAsync(pageNumber, pageSize, cancellationToken);
        var totalCount = await _userRepository.GetCountAsync(cancellationToken);

        _logger.LogInformation(
            "Retrieved {Count} users out of {TotalCount} total",
            users.Count,
            totalCount
        );

        Response.Headers.Append("X-Total-Count", totalCount.ToString());
        Response.Headers.Append("X-Page-Number", pageNumber.ToString());
        Response.Headers.Append("X-Page-Size", pageSize.ToString());

        return Ok(users);
    }

    /// <summary>
    /// Retrieves a user by ID
    /// </summary>
    /// <param name="id">User unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User entity</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserEntity>> GetUserById(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Getting user by ID: {UserId}", id);

        var user = await _userRepository.GetByIdAsync(id, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User not found with ID: {UserId}", id);
            return NotFound(new { Message = $"User with ID '{id}' not found" });
        }

        _logger.LogInformation("Successfully retrieved user: {UserId}", id);
        return Ok(user);
    }

    /// <summary>
    /// Creates a new user
    /// </summary>
    /// <param name="newUser">User data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created user</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserEntity>> CreateUser(
        [FromBody] UserEntity newUser,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Creating new user with email: {Email}", newUser?.Email ?? "null");

        if (newUser == null)
        {
            _logger.LogWarning("Attempt to create user with null data");
            return BadRequest("User data is required");
        }

        if (await _userRepository.ExistsByEmailAsync(newUser.Email, cancellationToken))
        {
            _logger.LogWarning(
                "Attempt to create user with duplicate email: {Email}",
                newUser.Email
            );
            return Conflict(new { Message = $"User with email '{newUser.Email}' already exists" });
        }

        if (await _userRepository.ExistsByUsernameAsync(newUser.Username, cancellationToken))
        {
            _logger.LogWarning(
                "Attempt to create user with duplicate username: {Username}",
                newUser.Username
            );
            return Conflict(
                new { Message = $"User with username '{newUser.Username}' already exists" }
            );
        }

        await _userRepository.AddAsync(newUser, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Successfully created user: {UserId}, Email: {Email}",
            newUser.Id,
            newUser.Email
        );

        return CreatedAtAction(nameof(GetUserById), new { id = newUser.Id }, newUser);
    }

    /// <summary>
    /// Updates an existing user
    /// </summary>
    /// <param name="id">User unique identifier</param>
    /// <param name="updatedUser">Updated user data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateUser(
        Guid id,
        [FromBody] UserEntity updatedUser,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Updating user: {UserId}", id);

        if (updatedUser == null)
        {
            _logger.LogWarning("Attempt to update user with null data for ID: {UserId}", id);
            return BadRequest("User data is required");
        }

        if (id != updatedUser.Id)
        {
            _logger.LogWarning(
                "User ID mismatch in update request. URL ID: {UrlId}, Body ID: {BodyId}",
                id,
                updatedUser.Id
            );
            return BadRequest("ID mismatch");
        }

        var existingUser = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (existingUser == null)
        {
            _logger.LogWarning("Attempt to update non-existent user: {UserId}", id);
            return NotFound(new { Message = $"User with ID '{id}' not found" });
        }

        var emailExists = await _userRepository.ExistsByEmailAsync(
            updatedUser.Email,
            cancellationToken
        );
        if (emailExists && existingUser.Email != updatedUser.Email)
        {
            _logger.LogWarning(
                "Attempt to update user {UserId} with duplicate email: {Email}",
                id,
                updatedUser.Email
            );
            return Conflict(
                new { Message = $"User with email '{updatedUser.Email}' already exists" }
            );
        }

        _userRepository.Update(updatedUser);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully updated user: {UserId}", id);

        return NoContent();
    }

    /// <summary>
    /// Deletes a user by ID
    /// </summary>
    /// <param name="id">User unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Deleting user: {UserId}", id);

        var deleted = await _userRepository.RemoveByIdAsync(id, cancellationToken);

        if (!deleted)
        {
            _logger.LogWarning("Attempt to delete non-existent user: {UserId}", id);
            return NotFound(new { Message = $"User with ID '{id}' not found" });
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully deleted user: {UserId}", id);

        return NoContent();
    }

    /// <summary>
    /// Returns available endpoints
    /// </summary>
    /// <returns>
    /// List of available endpoints
    /// </returns>
    [HttpGet("endpoints")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetEndpoints()
    {
        var endpoints = new[]
        {
            new { Method = "GET", Path = "/api/v1/users" },
            new { Method = "POST", Path = "/api/v1/users" },
            new { Method = "DELETE", Path = "/api/v1/users" },
            new { Method = "GET", Path = "/api/v1/users/{id}" },
            new { Method = "PUT", Path = "/api/v1/users/{id}" },
            new { Method = "DELETE", Path = "/api/v1/users/{id}" },
        };

        return Ok(endpoints);
    }

    /// <summary>
    /// Returns allowed HTTP methods for this endpoint
    /// </summary>
    [HttpOptions]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult GetOptions()
    {
        Response.Headers.Append("Allow", "GET,POST,PUT,DELETE,OPTIONS");
        return NoContent();
    }
}
