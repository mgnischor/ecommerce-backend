using System.Diagnostics;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Controllers;

/// <summary>
/// Application diagnostics and health information endpoints (admin only).
/// </summary>
/// <remarks>
/// <para>
/// Exposes operational information such as the application version, uptime, runtime environment,
/// assembly metadata, and live database connectivity. Useful for support and monitoring automation.
/// </para>
/// </remarks>
[Tags("Diagnostics")]
[ApiController]
[Route("api/v1/diagnostics")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public sealed class DiagnosticsController : ControllerBase
{
    private readonly PostgresqlContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<DiagnosticsController> _logger;
    private readonly DateTime _startedAtUtc;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiagnosticsController"/> class.
    /// </summary>
    /// <param name="context">Database context used for connectivity checks.</param>
    /// <param name="environment">Hosting environment information.</param>
    /// <param name="logger">Logger for tracking diagnostic operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when any required dependency is null.</exception>
    public DiagnosticsController(
        PostgresqlContext context,
        IWebHostEnvironment environment,
        ILogger<DiagnosticsController> logger
    )
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _startedAtUtc = Process.GetCurrentProcess().StartTime.ToUniversalTime();
    }

    /// <summary>
    /// Returns application and runtime diagnostics.
    /// </summary>
    /// <remarks>
    /// Returns assembly version, environment name, process uptime, memory usage, and database
    /// connectivity state without leaking configuration secrets.
    /// </remarks>
    /// <response code="200">Diagnostics information retrieved successfully.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> GetDiagnostics(CancellationToken cancellationToken)
    {
        var assembly = typeof(Program).Assembly;
        var version = assembly.GetName().Version?.ToString() ?? "unknown";
        var informationalVersion = assembly
            .GetCustomAttributes(
                typeof(System.Reflection.AssemblyInformationalVersionAttribute),
                false
            )
            .Cast<System.Reflection.AssemblyInformationalVersionAttribute>()
            .FirstOrDefault()
            ?.InformationalVersion;

        var uptime = DateTime.UtcNow - _startedAtUtc;

        var databaseStatus = "unknown";
        try
        {
            await _context.Database.CanConnectAsync(cancellationToken);
            databaseStatus = "healthy";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Database connectivity check failed during diagnostics request");
            databaseStatus = "unhealthy";
        }

        var currentProcess = Process.GetCurrentProcess();

        return Ok(
            new
            {
                Application = new
                {
                    Name = assembly.GetName().Name,
                    Version = version,
                    InformationalVersion = informationalVersion,
                    Environment = _environment.EnvironmentName,
                },
                Runtime = new
                {
                    Os = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
                    ProcessArchitecture = System
                        .Runtime
                        .InteropServices
                        .RuntimeInformation
                        .ProcessArchitecture,
                    Framework = System
                        .Runtime
                        .InteropServices
                        .RuntimeInformation
                        .FrameworkDescription,
                    WorkingSetBytes = currentProcess.WorkingSet64,
                    PrivateMemoryBytes = currentProcess.PrivateMemorySize64,
                },
                Uptime = new
                {
                    StartedAtUtc = _startedAtUtc,
                    Duration = uptime,
                    DurationSeconds = (long)uptime.TotalSeconds,
                },
                Database = new { Status = databaseStatus },
                ServerTimeUtc = DateTime.UtcNow,
            }
        );
    }
}
