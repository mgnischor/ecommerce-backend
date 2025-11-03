using System.Security.Claims;
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
    private const int MaxNotificationsPerUser = 1000;

    public NotificationController(PostgresqlContext context, ILogger<NotificationController> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private string? GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private bool IsAdmin() => User.IsInRole("Admin");

    private bool IsManager() => User.IsInRole("Manager");

    /// <summary>
    /// Retrieves notifications for the authenticated user
    /// </summary>
    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<NotificationEntity>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<NotificationEntity>>> GetUserNotifications(
        Guid userId,
        [FromQuery] bool unreadOnly = false,
        CancellationToken cancellationToken = default
    )
    {
        if (userId == Guid.Empty)
        {
            _logger.LogWarning("Invalid user GUID provided for notifications");
            return BadRequest(new { Message = "Invalid user ID" });
        }

        try
        {
            // IDOR Protection: Users can only access their own notifications
            var currentUserId = GetCurrentUserId();
            if (!IsAdmin() && userId.ToString() != currentUserId)
            {
                _logger.LogWarning(
                    "Unauthorized access attempt to user notifications: {UserId}, User: {CurrentUserId}",
                    userId,
                    currentUserId
                );
                return Forbid();
            }

            var query = _context.Notifications.Where(n => n.UserId == userId && !n.IsDeleted);

            if (unreadOnly)
                query = query.Where(n => !n.IsRead);

            var notifications = await query
                .OrderByDescending(n => n.Priority)
                .ThenByDescending(n => n.CreatedAt)
                .Take(MaxNotificationsPerUser)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            _logger.LogInformation(
                "Retrieved {Count} notifications for user: {UserId}",
                notifications.Count,
                userId
            );

            return Ok(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notifications for user: {UserId}", userId);
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
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

        return CreatedAtAction(
            nameof(GetNotificationById),
            new { id = notification.Id },
            notification
        );
    }

    /// <summary>
    /// Marks a notification as read
    /// </summary>
    [HttpPatch("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid notification GUID provided");
            return BadRequest(new { Message = "Invalid notification ID" });
        }

        try
        {
            var notification = await _context.Notifications.FirstOrDefaultAsync(
                n => n.Id == id && !n.IsDeleted,
                cancellationToken
            );

            if (notification == null)
            {
                _logger.LogWarning("Notification not found: {NotificationId}", id);
                return NotFound(new { Message = "Notification not found" });
            }

            // IDOR Protection: Users can only mark their own notifications as read
            var currentUserId = GetCurrentUserId();
            if (!IsAdmin() && notification.UserId.ToString() != currentUserId)
            {
                _logger.LogWarning(
                    "Unauthorized attempt to mark notification as read: {NotificationId}, User: {UserId}",
                    id,
                    currentUserId
                );
                return Forbid();
            }

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Notification marked as read: {NotificationId}, User: {UserId}",
                    id,
                    currentUserId
                );
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification as read: {NotificationId}", id);
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
    }

    /// <summary>
    /// Marks all user notifications as read
    /// </summary>
    [HttpPatch("user/{userId:guid}/read-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> MarkAllAsRead(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        if (userId == Guid.Empty)
        {
            _logger.LogWarning("Invalid user GUID provided for mark all as read");
            return BadRequest(new { Message = "Invalid user ID" });
        }

        try
        {
            // IDOR Protection: Users can only mark their own notifications
            var currentUserId = GetCurrentUserId();
            if (!IsAdmin() && userId.ToString() != currentUserId)
            {
                _logger.LogWarning(
                    "Unauthorized attempt to mark all notifications as read: {UserId}, User: {CurrentUserId}",
                    userId,
                    currentUserId
                );
                return Forbid();
            }

            var notifications = await _context
                .Notifications.Where(n => n.UserId == userId && !n.IsRead && !n.IsDeleted)
                .Take(MaxNotificationsPerUser)
                .ToListAsync(cancellationToken);

            if (notifications.Any())
            {
                var now = DateTime.UtcNow;
                foreach (var notification in notifications)
                {
                    notification.IsRead = true;
                    notification.ReadAt = now;
                }

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Marked {Count} notifications as read for user: {UserId}",
                    notifications.Count,
                    userId
                );
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error marking all notifications as read for user: {UserId}",
                userId
            );
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
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
        var count = await _context.Notifications.CountAsync(
            n => n.UserId == userId && !n.IsRead && !n.IsDeleted,
            cancellationToken
        );

        return Ok(count);
    }
}
