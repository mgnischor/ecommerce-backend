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
    /// <param name="logger">Logger instance for recording authorization failures and security events</param>
    /// <param name="requiredRoles">Variable-length array of <see cref="UserAccessLevel"/> values representing the roles that are authorized to access the action. User must have at least one of these roles.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> or <paramref name="requiredRoles"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="requiredRoles"/> is empty. At least one role must be specified for authorization to function.</exception>
    /// <remarks>
    /// <para>
    /// The filter uses the params keyword to accept a variable number of role arguments, making it flexible
    /// for different authorization scenarios. At least one role must be provided to ensure proper authorization.
    /// </para>
    /// <para>
    /// <strong>Usage Examples:</strong>
    /// </para>
    /// <code>
    /// // Single role requirement
    /// new RequireRoleFilter(logger, UserAccessLevel.Admin)
    ///
    /// // Multiple role requirement (user needs any one of these)
    /// new RequireRoleFilter(logger, UserAccessLevel.Admin, UserAccessLevel.Manager)
    ///
    /// // ServiceFilter attribute usage in controllers
    /// [ServiceFilter(typeof(RequireRoleFilter), Arguments = new object[] { UserAccessLevel.Admin })]
    /// </code>
    /// </remarks>
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
    /// <param name="context">The authorization filter context containing HTTP context, user claims, and action metadata</param>
    /// <remarks>
    /// <para>
    /// This method implements role-based access control (RBAC) by validating the authenticated user's role
    /// against the required roles for the action. It is called before the action method executes.
    /// </para>
    /// <para>
    /// <strong>Authorization Flow:</strong>
    /// </para>
    /// <list type="number">
    /// <item><description><strong>Authentication Check:</strong> Verifies user is authenticated via Identity.IsAuthenticated</description></item>
    /// <item><description><strong>Claim Extraction:</strong> Searches for role in multiple claim types (ClaimTypes.Role, "role", "AccessLevel")</description></item>
    /// <item><description><strong>Role Parsing:</strong> Converts string claim value to <see cref="UserAccessLevel"/> enum</description></item>
    /// <item><description><strong>Authorization Check:</strong> Validates if user's role matches any of the <see cref="_requiredRoles"/></description></item>
    /// <item><description><strong>Response Generation:</strong> Returns appropriate HTTP status code and error details</description></item>
    /// </list>
    /// <para>
    /// <strong>Response Codes:</strong>
    /// </para>
    /// <list type="table">
    /// <listheader>
    /// <term>Status Code</term>
    /// <description>Condition</description>
    /// </listheader>
    /// <item>
    /// <term>401 Unauthorized</term>
    /// <description>User is not authenticated or authentication token is invalid/missing</description>
    /// </item>
    /// <item>
    /// <term>403 Forbidden</term>
    /// <description>User is authenticated but lacks required role, has no role claim, or has invalid role claim</description>
    /// </item>
    /// <item>
    /// <term>Success (no result)</term>
    /// <description>User has one of the required roles and authorization succeeds</description>
    /// </item>
    /// </list>
    /// <para>
    /// <strong>Security Notes:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>All authorization failures are logged with user ID and attempted action for audit trails</description></item>
    /// <item><description>403 Forbidden responses include details about required roles and user's current role for debugging</description></item>
    /// <item><description>Case-insensitive role parsing improves compatibility with various JWT implementations</description></item>
    /// <item><description>Multiple claim type checks support different JWT token formats</description></item>
    /// </list>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if context is null (handled by framework)</exception>
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
