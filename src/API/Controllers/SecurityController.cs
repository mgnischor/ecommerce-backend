using ECommerce.API.DTOs;
using ECommerce.API.Middlewares;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

/// <summary>
/// Security monitoring endpoints (admin only).
/// </summary>
/// <remarks>
/// <para>
/// Exposes the recent security audit trail and Web Application Firewall (WAF) block statistics
/// for operational monitoring and incident response.
/// </para>
/// </remarks>
[Tags("Security")]
[ApiController]
[Route("api/v1/security")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public sealed class SecurityController : ControllerBase
{
    private readonly ISecurityAuditService _auditService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityController"/> class.
    /// </summary>
    /// <param name="auditService">Service providing security audit events.</param>
    /// <exception cref="ArgumentNullException">Thrown when the audit service is null.</exception>
    public SecurityController(ISecurityAuditService auditService)
    {
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
    }

    /// <summary>
    /// Returns the most recent security audit events.
    /// </summary>
    /// <remarks>
    /// Events include login failures, account lockouts, WAF blocks, and administrative actions.
    /// </remarks>
    /// <param name="count">Maximum number of events to return (default 100, max 500).</param>
    /// <response code="200">Recent security audit events.</response>
    [HttpGet("audit")]
    [ProducesResponseType(typeof(IEnumerable<SecurityAuditEventDto>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<SecurityAuditEventDto>> GetAudit([FromQuery] int count = 100)
    {
        if (count is < 1 or > 500)
        {
            count = 100;
        }

        var events = _auditService.GetRecent(count);

        var response = events
            .Select(e => new SecurityAuditEventDto
            {
                Id = e.Id,
                TimestampUtc = e.TimestampUtc,
                Category = e.Category,
                Severity = e.Severity,
                Message = e.Message,
                IpAddress = e.IpAddress,
                UserAgent = e.UserAgent,
                Path = e.Path,
            })
            .ToList();

        return Ok(response);
    }

    /// <summary>
    /// Returns the Web Application Firewall block statistics.
    /// </summary>
    /// <remarks>
    /// Returns the number of requests blocked per WAF rule since application start.
    /// </remarks>
    /// <response code="200">WAF block counters.</response>
    [HttpGet("waf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<object> GetWafStats()
    {
        var stats = WafMiddleware.GetBlockedStats();
        return Ok(new { TotalBlocked = stats.Values.Sum(), Rules = stats });
    }
}
