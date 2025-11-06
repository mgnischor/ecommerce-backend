using System.Security.Claims;
using ECommerce.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ECommerce.API.Filters;

/// <summary>
/// Authorization filter that enforces role-based access control
/// </summary>
/// <remarks>
/// <para>
/// Validates that the authenticated user has one of the required roles to access the action.
/// Extracts role information from JWT claims and compares against allowed roles.
/// Provides more flexible role checking than the built-in [Authorize(Roles)] attribute.
/// </para>
/// <para>
/// <strong>Features:</strong>
/// </para>
/// <list type="bullet">
/// <item><description><strong>Multiple Roles:</strong> Accept any of the specified roles</description></item>
/// <item><description><strong>Enum-Based:</strong> Type-safe role checking using UserAccessLevel enum</description></item>
/// <item><description><strong>Clear Logging:</strong> Logs unauthorized access attempts</description></item>
/// <item><description><strong>Standardized Response:</strong> Returns 403 Forbidden with details</description></item>
/// </list>
/// <para>
/// <strong>Usage:</strong>
/// </para>
/// <code>
/// [ServiceFilter(typeof(RequireRoleFilter), Arguments = new object[] { UserAccessLevel.Admin, UserAccessLevel.Manager })]
/// public async Task&lt;IActionResult&gt; DeleteProduct(Guid id) { }
/// </code>
/// </remarks>
public sealed class RequireRoleFilter : IAuthorizationFilter
{
    /// <summary>
    /// Logger instance for tracking authorization failures
    /// </summary>
    private readonly ILogger<RequireRoleFilter> _logger;

    /// <summary>
    /// Required access levels for the action
    /// </summary>
    private readonly UserAccessLevel[] _requiredRoles;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequireRoleFilter"/> class
    /// </summary>
    /// <param name="logger">Logger for recording authorization failures</param>
    /// <param name="requiredRoles">Array of allowed access levels</param>
    /// <exception cref="ArgumentNullException">Thrown when logger or requiredRoles is null</exception>
    /// <exception cref="ArgumentException">Thrown when no roles are specified</exception>
    public RequireRoleFilter(
        ILogger<RequireRoleFilter> logger,
        params UserAccessLevel[] requiredRoles
    )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _requiredRoles = requiredRoles ?? throw new ArgumentNullException(nameof(requiredRoles));

        if (_requiredRoles.Length == 0)
        {
            throw new ArgumentException(
                "At least one role must be specified",
                nameof(requiredRoles)
            );
        }
    }

    /// <summary>
    /// Validates user authorization before action execution
    /// </summary>
    /// <param name="context">The authorization filter context</param>
    /// <remarks>
    /// Checks if the user is authenticated and has one of the required roles.
    /// Extracts role from JWT claims and validates against required roles.
    /// Returns 401 Unauthorized if not authenticated, or 403 Forbidden if lacking required role.
    /// </remarks>
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        // Check if user is authenticated
        if (user?.Identity?.IsAuthenticated != true)
        {
            _logger.LogWarning(
                "Unauthenticated access attempt to {ActionName}",
                context.ActionDescriptor.DisplayName
            );

            context.Result = new UnauthorizedObjectResult(
                new { Message = "Authentication required" }
            );
            return;
        }

        // Extract role from claims
        var roleClaim =
            user.FindFirst(ClaimTypes.Role)?.Value
            ?? user.FindFirst("role")?.Value
            ?? user.FindFirst("AccessLevel")?.Value;

        if (string.IsNullOrEmpty(roleClaim))
        {
            _logger.LogWarning(
                "No role claim found for user {UserId} accessing {ActionName}",
                user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown",
                context.ActionDescriptor.DisplayName
            );

            context.Result = new ForbidResult();
            return;
        }

        // Parse role from claim
        if (!Enum.TryParse<UserAccessLevel>(roleClaim, true, out var userRole))
        {
            _logger.LogWarning(
                "Invalid role claim '{RoleClaim}' for user {UserId}",
                roleClaim,
                user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown"
            );

            context.Result = new ForbidResult();
            return;
        }

        // Check if user has required role
        if (!_requiredRoles.Contains(userRole))
        {
            _logger.LogWarning(
                "User {UserId} with role {UserRole} attempted to access {ActionName} requiring roles: {RequiredRoles}",
                user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown",
                userRole,
                context.ActionDescriptor.DisplayName,
                string.Join(", ", _requiredRoles)
            );

            context.Result = new ObjectResult(
                new
                {
                    Message = "Insufficient permissions",
                    RequiredRoles = _requiredRoles.Select(r => r.ToString()).ToArray(),
                    UserRole = userRole.ToString(),
                }
            )
            {
                StatusCode = StatusCodes.Status403Forbidden,
            };
        }
    }
}
