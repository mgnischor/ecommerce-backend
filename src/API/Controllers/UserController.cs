using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using ECommerce.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/v1/users")]
[Produces("application/json")]
public sealed class UserController : ControllerBase
{
    private readonly UserRepository _userRepository;
    private readonly PostgresqlContext _context;

    public UserController(UserRepository userRepository, PostgresqlContext context)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _context = context ?? throw new ArgumentNullException(nameof(context));
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
        if (pageNumber < 1)
            return BadRequest("Page number must be greater than 0");

        if (pageSize < 1 || pageSize > 100)
            return BadRequest("Page size must be between 1 and 100");

        var users = await _userRepository.GetPagedAsync(pageNumber, pageSize, cancellationToken);
        var totalCount = await _userRepository.GetCountAsync(cancellationToken);

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
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);

        if (user == null)
            return NotFound(new { Message = $"User with ID '{id}' not found" });

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
        if (newUser == null)
            return BadRequest("User data is required");

        if (await _userRepository.ExistsByEmailAsync(newUser.Email, cancellationToken))
            return Conflict(new { Message = $"User with email '{newUser.Email}' already exists" });

        if (await _userRepository.ExistsByUsernameAsync(newUser.Username, cancellationToken))
            return Conflict(
                new { Message = $"User with username '{newUser.Username}' already exists" }
            );

        await _userRepository.AddAsync(newUser, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

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
        if (updatedUser == null)
            return BadRequest("User data is required");

        if (id != updatedUser.Id)
            return BadRequest("ID mismatch");

        var existingUser = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (existingUser == null)
            return NotFound(new { Message = $"User with ID '{id}' not found" });

        var emailExists = await _userRepository.ExistsByEmailAsync(
            updatedUser.Email,
            cancellationToken
        );
        if (emailExists && existingUser.Email != updatedUser.Email)
            return Conflict(
                new { Message = $"User with email '{updatedUser.Email}' already exists" }
            );

        _userRepository.Update(updatedUser);
        await _context.SaveChangesAsync(cancellationToken);

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
        var deleted = await _userRepository.RemoveByIdAsync(id, cancellationToken);

        if (!deleted)
            return NotFound(new { Message = $"User with ID '{id}' not found" });

        await _context.SaveChangesAsync(cancellationToken);

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
