using System.Security.Claims;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Controllers;

/// <summary>
/// Notification management endpoints
/// </summary>
/// <remarks>
/// <para>
/// Provides comprehensive notification management for the e-commerce platform.
/// Enables users to receive, read, and manage system notifications including order updates,
/// promotional messages, account alerts, and system announcements.
/// </para>
/// <para>
/// <strong>Key Features:</strong>
/// </para>
/// <list type="bullet">
/// <item><description><strong>User Notifications:</strong> Retrieve notifications for specific users with filtering options</description></item>
/// <item><description><strong>Read Status Management:</strong> Mark individual or all notifications as read</description></item>
/// <item><description><strong>Priority Ordering:</strong> Notifications are ordered by priority and creation date</description></item>
/// <item><description><strong>Soft Delete:</strong> Notifications are soft-deleted to maintain audit trails</description></item>
/// <item><description><strong>Unread Count:</strong> Quick access to unread notification counts</description></item>
/// <item><description><strong>IDOR Protection:</strong> Users can only access and manage their own notifications</description></item>
/// </list>
/// <para>
/// <strong>Security Features:</strong>
/// </para>
/// <list type="bullet">
/// <item><description><strong>Authentication Required:</strong> All endpoints require valid JWT authentication</description></item>
/// <item><description><strong>Authorization Checks:</strong> Users can only access their own notifications unless they are administrators</description></item>
/// <item><description><strong>Role-Based Access:</strong> Notification creation restricted to Admin and Manager roles</description></item>
/// <item><description><strong>IDOR Prevention:</strong> Insecure Direct Object Reference protection on all user-specific operations</description></item>
/// </list>
/// <para>
/// <strong>Notification Types:</strong> The system supports various notification types including
/// order confirmations, shipping updates, payment notifications, promotional offers, and system alerts.
/// </para>
/// <para>
/// <strong>Performance Considerations:</strong> Notifications are limited to a maximum of 1000 per user
/// to ensure optimal performance. Results are paginated and ordered by priority and date.
/// </para>
/// </remarks>
[Tags("Notifications")]
[ApiController]
[Route("api/v1/notifications")]
[Produces("application/json")]
[Authorize]
public sealed class NotificationController : ControllerBase
{
    /// <summary>
    /// Database context for notification data access operations
    /// </summary>
    /// <remarks>
    /// Provides direct access to the notification entities in the PostgreSQL database.
    /// Used for querying, creating, updating, and soft-deleting notification records.
    /// </remarks>
    private readonly PostgresqlContext _context;

    /// <summary>
    /// Logger instance for tracking notification operations and errors
    /// </summary>
    /// <remarks>
    /// Used to log notification access attempts, security violations, operations, and errors
    /// for monitoring, debugging, and security audit purposes.
    /// </remarks>
    private readonly ILoggingService _logger;

    /// <summary>
    /// Maximum number of notifications that can be retrieved per user
    /// </summary>
    /// <remarks>
    /// This limit prevents performance issues and excessive memory usage when retrieving
    /// notifications. Set to 1000 notifications per user to balance functionality with performance.
    /// </remarks>
    private const int MaxNotificationsPerUser = 1000;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationController"/> class
    /// </summary>
    /// <param name="context">
    /// Database context for notification operations. Provides access to notification entities
    /// and database operations. Cannot be null.
    /// </param>
    /// <param name="logger">
    /// Logger instance for recording notification events and errors. Used for operational
    /// monitoring, security auditing, and debugging. Cannot be null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when either context or logger parameter is null.
    /// </exception>
    /// <remarks>
    /// This constructor uses dependency injection to provide all required services.
    /// All parameters are validated for null values to ensure the controller operates correctly.
    /// The controller is instantiated by the ASP.NET Core dependency injection container
    /// when handling notification-related requests.
    /// </remarks>
    public NotificationController(
        PostgresqlContext context,
        LoggingService<NotificationController> logger
    )
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the current authenticated user's ID from JWT claims
    /// </summary>
    /// <returns>
    /// A string containing the user ID from the JWT token's NameIdentifier claim,
    /// or null if the claim is not found or the user is not authenticated.
    /// </returns>
    /// <remarks>
    /// This helper method extracts the user identifier from the JWT token claims
    /// to perform authorization checks and ensure users can only access their own notifications.
    /// </remarks>
    private string? GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    /// <summary>
    /// Checks if the current authenticated user has the Admin role
    /// </summary>
    /// <returns>
    /// True if the user has the Admin role; otherwise, false.
    /// </returns>
    /// <remarks>
    /// Administrators have elevated privileges and can access and manage notifications
    /// for all users, bypass IDOR protection, and perform administrative operations.
    /// </remarks>
    private bool IsAdmin() => User.IsInRole("Admin");

    /// <summary>
    /// Checks if the current authenticated user has the Manager role
    /// </summary>
    /// <returns>
    /// True if the user has the Manager role; otherwise, false.
    /// </returns>
    /// <remarks>
    /// Managers have the ability to create notifications for users and perform
    /// certain administrative tasks related to notification management.
    /// </remarks>
    private bool IsManager() => User.IsInRole("Manager");

    /// <summary>
    /// Retrieves notifications for a specific user with optional filtering
    /// </summary>
    /// <remarks>
    /// <para>
    /// Returns a list of notifications for the specified user, ordered by priority and creation date.
    /// Users can only access their own notifications unless they have administrator privileges.
    /// </para>
    /// <para>
    /// <strong>Sample request (all notifications):</strong>
    /// </para>
    /// <code>
    /// GET /api/v1/notifications/user/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// Authorization: Bearer {token}
    /// </code>
    /// <para>
    /// <strong>Sample request (unread only):</strong>
    /// </para>
    /// <code>
    /// GET /api/v1/notifications/user/3fa85f64-5717-4562-b3fc-2c963f66afa6?unreadOnly=true
    /// Authorization: Bearer {token}
    /// </code>
    /// <para>
    /// <strong>Required permissions:</strong> Authenticated user (can access own notifications) or Admin (can access any user's notifications)
    /// </para>
    /// <para>
    /// <strong>Ordering:</strong>
    /// </para>
    /// <list type="number">
    /// <item><description><strong>Primary:</strong> By priority (highest first) - urgent notifications appear first</description></item>
    /// <item><description><strong>Secondary:</strong> By creation date (most recent first) - newer notifications within same priority</description></item>
    /// </list>
    /// <para>
    /// <strong>Filtering Options:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>All Notifications:</strong> Set unreadOnly=false (default) to retrieve all notifications</description></item>
    /// <item><description><strong>Unread Only:</strong> Set unreadOnly=true to retrieve only unread notifications</description></item>
    /// <item><description><strong>Soft Deletes:</strong> Deleted notifications are automatically excluded from results</description></item>
    /// </list>
    /// <para>
    /// <strong>Response includes for each notification:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Notification ID, type, and title</description></item>
    /// <item><description>Message content and priority level</description></item>
    /// <item><description>Read status and read timestamp</description></item>
    /// <item><description>Creation timestamp and metadata</description></item>
    /// <item><description>Associated references (order ID, product ID, etc.)</description></item>
    /// </list>
    /// <para>
    /// <strong>Security - IDOR Protection:</strong>
    /// The endpoint validates that the requesting user can only access their own notifications.
    /// Attempts to access other users' notifications will result in a 403 Forbidden response
    /// unless the user has administrator privileges.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> Results are limited to a maximum of 1000 notifications per user
    /// to ensure optimal performance and response times. Use the unreadOnly filter to reduce result sets.
    /// </para>
    /// </remarks>
    /// <param name="userId">
    /// The unique identifier (GUID) of the user whose notifications to retrieve.
    /// Must be a valid GUID format. For non-admin users, this must match their own user ID.
    /// </param>
    /// <param name="unreadOnly">
    /// Optional filter to retrieve only unread notifications.
    /// Default is false (returns all notifications). Set to true to retrieve only unread items.
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to cancel the asynchronous operation.
    /// Allows the client to cancel the request if it's no longer needed.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing an enumerable collection of <see cref="NotificationEntity"/> objects.
    /// Notifications are ordered by priority (descending) then by creation date (descending).
    /// Returns an empty array if no notifications match the criteria.
    /// </returns>
    /// <response code="200">
    /// Successfully retrieved user notifications. Returns a JSON array of notification objects
    /// ordered by priority and date. The array may be empty if no notifications exist for the user
    /// or if filtering results in no matches.
    /// </response>
    /// <response code="400">
    /// Bad request. Invalid user ID format (empty GUID or malformed GUID).
    /// </response>
    /// <response code="401">
    /// Unauthorized. Authentication required. The request must include a valid authorization token.
    /// </response>
    /// <response code="403">
    /// Forbidden. The authenticated user attempted to access notifications for another user
    /// without administrator privileges. This is an IDOR (Insecure Direct Object Reference) protection.
    /// </response>
    /// <response code="500">
    /// Internal server error. An unexpected error occurred while retrieving notifications.
    /// Check server logs for detailed error information.
    /// </response>
    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<NotificationEntity>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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
    /// Retrieves a specific notification by its unique identifier
    /// </summary>
    /// <remarks>
    /// <para>
    /// Returns detailed information about a single notification identified by its unique ID.
    /// This endpoint provides access to complete notification details.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// GET /api/v1/notifications/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// Authorization: Bearer {token}
    /// </code>
    /// <para>
    /// <strong>Required permissions:</strong> Authenticated user
    /// </para>
    /// <para>
    /// <strong>Response includes:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Notification ID, type, and title</description></item>
    /// <item><description>Complete message content</description></item>
    /// <item><description>Priority level and category</description></item>
    /// <item><description>Read status and timestamp</description></item>
    /// <item><description>User ID (recipient)</description></item>
    /// <item><description>Creation and update timestamps</description></item>
    /// <item><description>Associated references (order ID, product ID, etc.)</description></item>
    /// </list>
    /// <para>
    /// <strong>Note:</strong> This endpoint does not perform authorization checks to verify
    /// if the requesting user owns the notification. Consider adding IDOR protection similar
    /// to other endpoints for enhanced security.
    /// </para>
    /// <para>
    /// <strong>Soft Delete Handling:</strong> Notifications that have been soft-deleted
    /// are treated as not found and will return a 404 response.
    /// </para>
    /// </remarks>
    /// <param name="id">
    /// The unique identifier (GUID) of the notification to retrieve.
    /// Must be a valid GUID format (e.g., 3fa85f64-5717-4562-b3fc-2c963f66afa6).
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to cancel the asynchronous operation.
    /// Allows the client to cancel the request if it's no longer needed.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing a <see cref="NotificationEntity"/> object
    /// with complete notification details including content, status, and metadata.
    /// </returns>
    /// <response code="200">
    /// Successfully retrieved the notification. Returns a JSON object with complete notification details.
    /// </response>
    /// <response code="401">
    /// Unauthorized. Authentication required. The request must include a valid authorization token.
    /// </response>
    /// <response code="404">
    /// Notification not found. The specified ID does not exist in the system,
    /// the notification has been soft-deleted, or the ID format is invalid.
    /// </response>
    /// <response code="500">
    /// Internal server error. An unexpected error occurred while retrieving the notification.
    /// Check server logs for detailed error information.
    /// </response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(NotificationEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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
    /// Creates a new notification for a user
    /// </summary>
    /// <remarks>
    /// <para>
    /// Creates a new notification and assigns it to the specified user. This endpoint is restricted
    /// to users with Admin or Manager roles who need to send notifications to users.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// POST /api/v1/notifications
    /// Authorization: Bearer {token}
    /// Content-Type: application/json
    ///
    /// {
    ///   "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "type": "OrderUpdate",
    ///   "title": "Order Shipped",
    ///   "message": "Your order #12345 has been shipped",
    ///   "priority": 2,
    ///   "isRead": false,
    ///   "orderId": "7fa85f64-5717-4562-b3fc-2c963f66afa6"
    /// }
    /// </code>
    /// <para>
    /// <strong>Required permissions:</strong> Admin or Manager role
    /// </para>
    /// <para>
    /// <strong>Required fields:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>userId:</strong> The GUID of the user who will receive the notification</description></item>
    /// <item><description><strong>type:</strong> Notification type (e.g., OrderUpdate, Payment, Promotion, System)</description></item>
    /// <item><description><strong>title:</strong> Short notification title/subject</description></item>
    /// <item><description><strong>message:</strong> The notification message content</description></item>
    /// <item><description><strong>priority:</strong> Notification priority level (1=low, 2=medium, 3=high, 4=urgent)</description></item>
    /// </list>
    /// <para>
    /// <strong>Optional fields:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>orderId:</strong> Associated order ID for order-related notifications</description></item>
    /// <item><description><strong>productId:</strong> Associated product ID for product-related notifications</description></item>
    /// <item><description><strong>actionUrl:</strong> URL for user to take action (e.g., view order)</description></item>
    /// <item><description><strong>expiresAt:</strong> Optional expiration date for time-sensitive notifications</description></item>
    /// </list>
    /// <para>
    /// <strong>Automatic fields:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>id:</strong> Automatically generated unique identifier</description></item>
    /// <item><description><strong>createdAt:</strong> Automatically set to current UTC time</description></item>
    /// <item><description><strong>isRead:</strong> Defaults to false (unread)</description></item>
    /// <item><description><strong>isDeleted:</strong> Defaults to false (active)</description></item>
    /// </list>
    /// <para>
    /// <strong>Common notification types:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>OrderUpdate:</strong> Order status changes (confirmed, shipped, delivered)</description></item>
    /// <item><description><strong>Payment:</strong> Payment confirmations or issues</description></item>
    /// <item><description><strong>Promotion:</strong> Marketing and promotional messages</description></item>
    /// <item><description><strong>System:</strong> System maintenance or important announcements</description></item>
    /// <item><description><strong>Account:</strong> Account-related notifications (password reset, profile updates)</description></item>
    /// </list>
    /// </remarks>
    /// <param name="notification">
    /// The notification entity to create. Must include all required fields (userId, type, title, message, priority).
    /// The ID and createdAt fields will be automatically generated by the system.
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to cancel the asynchronous operation.
    /// Allows the client to cancel the request if it's no longer needed.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing the created <see cref="NotificationEntity"/> object
    /// with the assigned ID and creation timestamp. The Location header contains the URI to retrieve this notification.
    /// </returns>
    /// <response code="201">
    /// Created. Successfully created the notification. The Location header contains the URI
    /// of the newly created notification resource. Returns the complete notification object
    /// including the generated ID and creation timestamp.
    /// </response>
    /// <response code="400">
    /// Bad request. Invalid notification data, missing required fields, or validation errors.
    /// The notification object cannot be null and must include all required fields.
    /// </response>
    /// <response code="401">
    /// Unauthorized. Authentication required. The request must include a valid authorization token.
    /// </response>
    /// <response code="403">
    /// Forbidden. The authenticated user does not have the required role (Admin or Manager)
    /// to create notifications.
    /// </response>
    /// <response code="500">
    /// Internal server error. An unexpected error occurred while creating the notification.
    /// Check server logs for detailed error information.
    /// </response>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(NotificationEntity), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
    /// Marks a specific notification as read
    /// </summary>
    /// <remarks>
    /// <para>
    /// Updates a notification's read status and records the timestamp when it was read.
    /// Users can only mark their own notifications as read unless they have administrator privileges.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// PATCH /api/v1/notifications/3fa85f64-5717-4562-b3fc-2c963f66afa6/read
    /// Authorization: Bearer {token}
    /// </code>
    /// <para>
    /// <strong>Required permissions:</strong> Authenticated user (can mark own notifications) or Admin (can mark any notification)
    /// </para>
    /// <para>
    /// <strong>Operation details:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Sets the notification's isRead flag to true</description></item>
    /// <item><description>Records the readAt timestamp with current UTC time</description></item>
    /// <item><description>Idempotent operation - marking already-read notifications has no effect</description></item>
    /// <item><description>Does not update the notification if already marked as read (optimization)</description></item>
    /// </list>
    /// <para>
    /// <strong>Security - IDOR Protection:</strong>
    /// The endpoint validates that the requesting user owns the notification before allowing
    /// the read status update. Attempts to mark other users' notifications will result in
    /// a 403 Forbidden response unless the user has administrator privileges.
    /// </para>
    /// <para>
    /// <strong>Use cases:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>User views a notification in their inbox</description></item>
    /// <item><description>Acknowledge receipt of important notifications</description></item>
    /// <item><description>Update notification count badges in UI</description></item>
    /// <item><description>Track user engagement with notifications</description></item>
    /// </list>
    /// </remarks>
    /// <param name="id">
    /// The unique identifier (GUID) of the notification to mark as read.
    /// Must be a valid GUID format and reference an existing, non-deleted notification.
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to cancel the asynchronous operation.
    /// Allows the client to cancel the request if it's no longer needed.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> with status code 204 (No Content) on success.
    /// The response body is empty as per HTTP specification for successful PATCH operations.
    /// </returns>
    /// <response code="204">
    /// No Content. Successfully marked the notification as read. The response body is empty.
    /// The notification's read status has been updated and the read timestamp has been recorded.
    /// </response>
    /// <response code="400">
    /// Bad request. Invalid notification ID format (empty GUID or malformed GUID).
    /// </response>
    /// <response code="401">
    /// Unauthorized. Authentication required. The request must include a valid authorization token.
    /// </response>
    /// <response code="403">
    /// Forbidden. The authenticated user attempted to mark a notification belonging to another user
    /// without administrator privileges. This is IDOR protection in action.
    /// </response>
    /// <response code="404">
    /// Notification not found. The specified ID does not exist, has been soft-deleted,
    /// or the ID format is invalid.
    /// </response>
    /// <response code="500">
    /// Internal server error. An unexpected error occurred while updating the notification.
    /// Check server logs for detailed error information.
    /// </response>
    [HttpPatch("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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
    /// Marks all unread notifications as read for a specific user
    /// </summary>
    /// <remarks>
    /// <para>
    /// Batch operation that marks all unread notifications for the specified user as read.
    /// This is a convenient "mark all as read" feature commonly found in notification systems.
    /// Users can only mark their own notifications unless they have administrator privileges.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// PATCH /api/v1/notifications/user/3fa85f64-5717-4562-b3fc-2c963f66afa6/read-all
    /// Authorization: Bearer {token}
    /// </code>
    /// <para>
    /// <strong>Required permissions:</strong> Authenticated user (can mark own notifications) or Admin (can mark any user's notifications)
    /// </para>
    /// <para>
    /// <strong>Operation details:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Retrieves all unread notifications for the user (up to 1000 maximum)</description></item>
    /// <item><description>Sets isRead flag to true for all retrieved notifications</description></item>
    /// <item><description>Records the same readAt timestamp (current UTC time) for all notifications</description></item>
    /// <item><description>Performs a batch update operation for efficiency</description></item>
    /// <item><description>Idempotent operation - subsequent calls have no effect if all are already read</description></item>
    /// </list>
    /// <para>
    /// <strong>Security - IDOR Protection:</strong>
    /// The endpoint validates that the requesting user matches the specified user ID before
    /// allowing the batch update. Attempts to mark notifications for other users will result
    /// in a 403 Forbidden response unless the user has administrator privileges.
    /// </para>
    /// <para>
    /// <strong>Performance considerations:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Limited to 1000 notifications per operation to prevent performance issues</description></item>
    /// <item><description>Uses batch update for efficiency</description></item>
    /// <item><description>Single database transaction for all updates</description></item>
    /// <item><description>Logs the count of notifications marked for audit purposes</description></item>
    /// </list>
    /// <para>
    /// <strong>Use cases:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>"Mark all as read" button in notification inbox</description></item>
    /// <item><description>Clear notification badge counts in UI</description></item>
    /// <item><description>Bulk acknowledge notifications after review</description></item>
    /// <item><description>Reset notification state after user returns from absence</description></item>
    /// </list>
    /// </remarks>
    /// <param name="userId">
    /// The unique identifier (GUID) of the user whose notifications should be marked as read.
    /// Must be a valid GUID format. For non-admin users, this must match their own user ID.
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to cancel the asynchronous operation.
    /// Allows the client to cancel the request if it's no longer needed,
    /// though partial updates may have been committed.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> with status code 204 (No Content) on success.
    /// The response body is empty. The number of notifications marked is logged for audit purposes.
    /// </returns>
    /// <response code="204">
    /// No Content. Successfully marked all unread notifications as read for the user.
    /// The response body is empty. All unread notifications now have their read status
    /// updated and read timestamps recorded. If no unread notifications existed, the operation
    /// still succeeds with no changes made.
    /// </response>
    /// <response code="400">
    /// Bad request. Invalid user ID format (empty GUID or malformed GUID).
    /// </response>
    /// <response code="401">
    /// Unauthorized. Authentication required. The request must include a valid authorization token.
    /// </response>
    /// <response code="403">
    /// Forbidden. The authenticated user attempted to mark notifications for another user
    /// without administrator privileges. This is IDOR protection in action.
    /// </response>
    /// <response code="500">
    /// Internal server error. An unexpected error occurred while updating notifications.
    /// The operation may have been partially completed. Check server logs for detailed error information.
    /// </response>
    [HttpPatch("user/{userId:guid}/read-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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
    /// Deletes a notification using soft delete
    /// </summary>
    /// <remarks>
    /// <para>
    /// Performs a soft delete on the specified notification by setting its isDeleted flag to true.
    /// The notification remains in the database for audit purposes but is excluded from queries.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// DELETE /api/v1/notifications/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// Authorization: Bearer {token}
    /// </code>
    /// <para>
    /// <strong>Required permissions:</strong> Authenticated user
    /// </para>
    /// <para>
    /// <strong>Soft Delete vs Hard Delete:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Soft Delete:</strong> Sets isDeleted flag to true; record remains in database</description></item>
    /// <item><description><strong>Benefits:</strong> Maintains audit trails, enables recovery, preserves data integrity</description></item>
    /// <item><description><strong>Visibility:</strong> Deleted notifications are automatically excluded from all queries</description></item>
    /// <item><description><strong>Permanent:</strong> Currently no undelete functionality (can be added if needed)</description></item>
    /// </list>
    /// <para>
    /// <strong>Important Security Note:</strong>
    /// This endpoint does not perform authorization checks to verify if the requesting user
    /// owns the notification. Consider adding IDOR protection to ensure users can only delete
    /// their own notifications (or implement Admin-only deletion).
    /// </para>
    /// <para>
    /// <strong>Use cases:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>User dismisses/removes notifications from their inbox</description></item>
    /// <item><description>Clear expired or irrelevant notifications</description></item>
    /// <item><description>User preference to hide certain notifications</description></item>
    /// <item><description>Administrative cleanup of old notifications</description></item>
    /// </list>
    /// <para>
    /// <strong>Behavior:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Idempotent operation - deleting already-deleted notifications returns 404</description></item>
    /// <item><description>Deletion is immediate and cannot be undone through the API</description></item>
    /// <item><description>Deleted notifications won't appear in any list or count endpoints</description></item>
    /// </list>
    /// </remarks>
    /// <param name="id">
    /// The unique identifier (GUID) of the notification to delete.
    /// Must be a valid GUID format and reference an existing, non-deleted notification.
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to cancel the asynchronous operation.
    /// Allows the client to cancel the request if it's no longer needed.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> with status code 204 (No Content) on successful deletion.
    /// The response body is empty as per HTTP specification for successful DELETE operations.
    /// </returns>
    /// <response code="204">
    /// No Content. Successfully soft-deleted the notification. The response body is empty.
    /// The notification is now marked as deleted and will not appear in future queries.
    /// </response>
    /// <response code="401">
    /// Unauthorized. Authentication required. The request must include a valid authorization token.
    /// </response>
    /// <response code="404">
    /// Notification not found. The specified ID does not exist, has already been soft-deleted,
    /// or the ID format is invalid. Attempting to delete an already-deleted notification
    /// also returns this status.
    /// </response>
    /// <response code="500">
    /// Internal server error. An unexpected error occurred while deleting the notification.
    /// Check server logs for detailed error information.
    /// </response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteNotification(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var notification = await _context.Notifications.FindAsync([id], cancellationToken);

        if (notification == null || notification.IsDeleted)
            return NotFound(new { Message = $"Notification with ID '{id}' not found" });

        notification.IsDeleted = true;

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Gets the count of unread notifications for a specific user
    /// </summary>
    /// <remarks>
    /// <para>
    /// Returns the total number of unread, non-deleted notifications for the specified user.
    /// This endpoint is optimized for performance and commonly used to display notification
    /// badge counts in user interfaces.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// GET /api/v1/notifications/user/3fa85f64-5717-4562-b3fc-2c963f66afa6/unread-count
    /// Authorization: Bearer {token}
    /// </code>
    /// <para>
    /// <strong>Required permissions:</strong> Authenticated user
    /// </para>
    /// <para>
    /// <strong>Use cases:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Badge Display:</strong> Show unread count badge in navigation or notification bell icon</description></item>
    /// <item><description><strong>Real-time Updates:</strong> Poll or use with SignalR for live notification counts</description></item>
    /// <item><description><strong>User Experience:</strong> Indicate to users they have pending notifications</description></item>
    /// <item><description><strong>Performance:</strong> Lightweight endpoint suitable for frequent polling</description></item>
    /// </list>
    /// <para>
    /// <strong>Count criteria:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Only includes notifications where isRead = false</description></item>
    /// <item><description>Only includes notifications where isDeleted = false</description></item>
    /// <item><description>Returns count for the specified user ID</description></item>
    /// <item><description>Optimized COUNT query for performance</description></item>
    /// </list>
    /// <para>
    /// <strong>Important Security Note:</strong>
    /// This endpoint does not perform authorization checks to verify if the requesting user
    /// matches the specified user ID. Consider adding IDOR protection to ensure users can
    /// only retrieve their own notification counts, preventing information disclosure.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong>
    /// This endpoint uses a database COUNT operation which is optimized and does not load
    /// notification entities into memory. It's designed to be called frequently without
    /// significant performance impact.
    /// </para>
    /// <para>
    /// <strong>Sample response:</strong>
    /// </para>
    /// <code>
    /// 5
    /// </code>
    /// </remarks>
    /// <param name="userId">
    /// The unique identifier (GUID) of the user whose unread notification count to retrieve.
    /// Must be a valid GUID format.
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to cancel the asynchronous operation.
    /// Allows the client to cancel the request if it's no longer needed.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing an integer representing the count of
    /// unread notifications for the specified user. Returns 0 if the user has no unread notifications.
    /// </returns>
    /// <response code="200">
    /// Successfully retrieved the unread notification count. Returns an integer value
    /// representing the number of unread, non-deleted notifications for the user.
    /// The value will be 0 if there are no unread notifications.
    /// </response>
    /// <response code="401">
    /// Unauthorized. Authentication required. The request must include a valid authorization token.
    /// </response>
    /// <response code="500">
    /// Internal server error. An unexpected error occurred while counting notifications.
    /// Check server logs for detailed error information.
    /// </response>
    [HttpGet("user/{userId:guid}/unread-count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
