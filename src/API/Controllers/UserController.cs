using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

/// <summary>
/// User management endpoints
/// </summary>
/// <remarks>
/// Provides comprehensive user account management including CRUD operations.
/// All endpoints require authentication. User passwords are stored securely using BCrypt hashing.
/// </remarks>
[Tags("Users")]
[ApiController]
[Route("api/v1/users")]
[Produces("application/json")]
[Authorize]
public sealed class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly PostgresqlContext _context;
    private readonly ILoggingService _logger;

    public UserController(
        IUserRepository userRepository,
        PostgresqlContext context,
        LoggingService<UserController> logger
    )
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves all users with pagination support
    /// </summary>
    /// <remarks>
    /// Returns a paginated list of all registered users. Pagination headers are included in the response.
    ///
    /// Sample request:
    ///
    ///     GET /api/v1/users?pageNumber=1&amp;pageSize=20
    ///
    /// Response headers:
    /// - X-Total-Count: Total number of users in the system
    /// - X-Page-Number: Current page number
    /// - X-Page-Size: Number of items per page
    ///
    /// **Required permissions:** Authenticated user
    ///
    /// </remarks>
    /// <param name="pageNumber">Page number (1-based, default: 1, minimum: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 10, range: 1-100)</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>List of users for the requested page</returns>
    /// <response code="200">Successfully retrieved the user list. Check response headers for pagination details.</response>
    /// <response code="400">Invalid pagination parameters. Page number must be >= 1, page size must be between 1 and 100.</response>
    /// <response code="401">Unauthorized. Authentication required.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UserEntity>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
    /// Retrieves a specific user by their unique identifier
    /// </summary>
    /// <remarks>
    /// Returns detailed information about a single user including profile data and roles.
    ///
    /// Sample request:
    ///
    ///     GET /api/v1/users/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///
    /// **Required permissions:** Authenticated user
    ///
    /// **Note:** Password hash is included in the response but should never be exposed to clients.
    ///
    /// </remarks>
    /// <param name="id">User unique identifier (GUID)</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>The requested user with all details</returns>
    /// <response code="200">Successfully retrieved the user.</response>
    /// <response code="401">Unauthorized. Authentication required.</response>
    /// <response code="404">User not found with the specified ID.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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
    /// Creates a new user account
    /// </summary>
    /// <remarks>
    /// Creates a new user with the provided data. Email and username must be unique.
    /// Password will be hashed using BCrypt before storage.
    ///
    /// Sample request:
    ///
    ///     POST /api/v1/users
    ///     {
    ///        "username": "johndoe",
    ///        "email": "john.doe@example.com",
    ///        "password": "SecureP@ssw0rd",
    ///        "firstName": "John",
    ///        "lastName": "Doe",
    ///        "role": "Customer",
    ///        "isActive": true
    ///     }
    ///
    /// **Required permissions:** Authenticated user
    ///
    /// **Available roles:** Admin, Manager, Customer
    ///
    /// The response includes a Location header with the URI of the newly created user.
    ///
    /// </remarks>
    /// <param name="newUser">User data to create (email and username must be unique)</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>The created user with generated ID</returns>
    /// <response code="201">Successfully created the user. Location header contains the URI of the new resource.</response>
    /// <response code="400">Invalid request. User data is required and must pass validation.</response>
    /// <response code="401">Unauthorized. Authentication required.</response>
    /// <response code="409">Conflict. A user with the same email or username already exists.</response>
    [HttpPost]
    [ProducesResponseType(typeof(UserEntity), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
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
    /// Updates an existing user account
    /// </summary>
    /// <remarks>
    /// Replaces all properties of an existing user with the provided data.
    /// The user ID in the URL must match the ID in the request body.
    ///
    /// Sample request:
    ///
    ///     PUT /api/v1/users/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///     {
    ///        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///        "username": "johndoe",
    ///        "email": "john.doe.updated@example.com",
    ///        "password": "NewSecureP@ssw0rd",
    ///        "firstName": "John",
    ///        "lastName": "Doe",
    ///        "role": "Manager",
    ///        "isActive": true
    ///     }
    ///
    /// **Required permissions:** Authenticated user
    ///
    /// **Note:** If changing the email, ensure the new email is not already in use.
    /// If updating the password, it will be re-hashed using BCrypt.
    ///
    /// </remarks>
    /// <param name="id">User unique identifier (must match the ID in request body)</param>
    /// <param name="updatedUser">Complete updated user data</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Successfully updated the user. No content returned.</response>
    /// <response code="400">Invalid request. ID mismatch or validation errors.</response>
    /// <response code="401">Unauthorized. Authentication required.</response>
    /// <response code="404">User not found with the specified ID.</response>
    /// <response code="409">Conflict. The new email is already in use by another user.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
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
    /// Permanently deletes a user account
    /// </summary>
    /// <remarks>
    /// Permanently removes a user from the database. This action cannot be undone.
    ///
    /// Sample request:
    ///
    ///     DELETE /api/v1/users/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///
    /// **Required permissions:** Authenticated user
    ///
    /// **Warning:** This is a destructive operation. All user data and associated records will be removed.
    /// Consider deactivating the account instead by setting isActive to false.
    ///
    /// </remarks>
    /// <param name="id">User unique identifier</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Successfully deleted the user permanently.</response>
    /// <response code="401">Unauthorized. Authentication required.</response>
    /// <response code="404">User not found with the specified ID.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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
    /// Returns a list of all available endpoints for the Users API
    /// </summary>
    /// <remarks>
    /// Provides API discovery by listing all available endpoints with their HTTP methods and paths.
    /// Useful for client applications to dynamically discover API capabilities.
    ///
    /// Sample request:
    ///
    ///     GET /api/v1/users/endpoints
    ///
    /// **Required permissions:** Authenticated user
    ///
    /// </remarks>
    /// <returns>List of available endpoints with methods and paths</returns>
    /// <response code="200">Successfully retrieved the endpoints list.</response>
    /// <response code="401">Unauthorized. Authentication required.</response>
    [HttpGet("endpoints")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
    /// Returns allowed HTTP methods for the Users API
    /// </summary>
    /// <remarks>
    /// Returns the allowed HTTP methods in the Allow response header.
    /// Useful for CORS preflight requests and API capability discovery.
    ///
    /// Sample request:
    ///
    ///     OPTIONS /api/v1/users
    ///
    /// **Required permissions:** Authenticated user
    ///
    /// </remarks>
    /// <returns>No content with Allow header containing supported methods</returns>
    /// <response code="204">No content. Check the Allow header for supported HTTP methods.</response>
    /// <response code="401">Unauthorized. Authentication required.</response>
    [HttpOptions]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetOptions()
    {
        Response.Headers.Append("Allow", "GET,POST,PUT,DELETE,OPTIONS");
        return NoContent();
    }
}
