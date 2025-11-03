using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Controllers;

/// <summary>
/// Notification management endpoints
/// </summary>
[Tags("Notifications")]
[ApiController]
[Route("api/v1/notifications")]
[Produces("application/json")]
[Authorize]
public sealed class NotificationController : ControllerBase
{
    private readonly PostgresqlContext _context;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(PostgresqlContext context, ILogger<NotificationController> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves notifications for the authenticated user
    /// </summary>
    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<NotificationEntity>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<NotificationEntity>>> GetUserNotifications(
        Guid userId,
        [FromQuery] bool unreadOnly = false,
        CancellationToken cancellationToken = default
    )
    {
        var query = _context.Notifications
            .Where(n => n.UserId == userId && !n.IsDeleted);

        if (unreadOnly)
            query = query.Where(n => !n.IsRead);

        var notifications = await query
            .OrderByDescending(n => n.Priority)
            .ThenByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(notifications);
    }

    /// <summary>
    /// Retrieves a specific notification by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(NotificationEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificationEntity>> GetNotificationById(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var notification = await _context.Notifications.FirstOrDefaultAsync(
            n => n.Id == id && !n.IsDeleted,
            cancellationToken
        );

        if (notification == null)
            return NotFound(new { Message = $"Notification with ID '{id}' not found" });

        return Ok(notification);
    }

    /// <summary>
    /// Creates a new notification
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(NotificationEntity), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<NotificationEntity>> CreateNotification(
        [FromBody] NotificationEntity notification,
        CancellationToken cancellationToken = default
    )
    {
        if (notification == null)
            return BadRequest("Notification data is required");

        notification.Id = Guid.NewGuid();
        notification.CreatedAt = DateTime.UtcNow;

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetNotificationById), new { id = notification.Id }, notification);
    }

    /// <summary>
    /// Marks a notification as read
    /// </summary>
    [HttpPatch("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var notification = await _context.Notifications.FindAsync(
            new object[] { id },
            cancellationToken
        );

        if (notification == null || notification.IsDeleted)
            return NotFound(new { Message = $"Notification with ID '{id}' not found" });

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Marks all user notifications as read
    /// </summary>
    [HttpPatch("user/{userId:guid}/read-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAllAsRead(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead && !n.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Deletes a notification (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteNotification(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var notification = await _context.Notifications.FindAsync(
            new object[] { id },
            cancellationToken
        );

        if (notification == null || notification.IsDeleted)
            return NotFound(new { Message = $"Notification with ID '{id}' not found" });

        notification.IsDeleted = true;

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Gets unread notification count for a user
    /// </summary>
    [HttpGet("user/{userId:guid}/unread-count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> GetUnreadCount(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var count = await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead && !n.IsDeleted, cancellationToken);

        return Ok(count);
    }
}
