using Comex.API.DTOs;
using Comex.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Comex.API.Controllers;

/// <summary>
/// Database backup and disaster recovery endpoints (admin only).
/// </summary>
/// <remarks>
/// <para>
/// Provides on-demand database backups, backup listing, restore, and deletion using the
/// PostgreSQL <c>pg_dump</c> / <c>pg_restore</c> tooling. All operations require the
/// <c>Admin</c> role.
/// </para>
/// </remarks>
[Tags("Backup")]
[ApiController]
[Route("api/v1/backups")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public sealed class BackupController : ControllerBase
{
    private readonly IDatabaseBackupService _backupService;
    private readonly ILoggingService _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackupController"/> class.
    /// </summary>
    /// <param name="backupService">Service handling backup and restore operations.</param>
    /// <param name="logger">Logger for tracking backup operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when any required dependency is null.</exception>
    public BackupController(IDatabaseBackupService backupService, ILoggingService logger)
    {
        _backupService = backupService ?? throw new ArgumentNullException(nameof(backupService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Lists all available database backup files.
    /// </summary>
    /// <remarks>
    /// Returns the backup files present in the configured backup directory, ordered newest first.
    /// </remarks>
    /// <response code="200">List of backup files.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BackupFileResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BackupFileResponseDto>>> GetBackups()
    {
        var backups = await _backupService.ListBackupsAsync();

        var response = backups
            .Select(b => new BackupFileResponseDto
            {
                FileName = b.FileName,
                SizeBytes = b.SizeBytes,
                CreatedAtUtc = b.CreatedAtUtc,
            })
            .ToList();

        return Ok(response);
    }

    /// <summary>
    /// Triggers an on-demand database backup.
    /// </summary>
    /// <remarks>
    /// Creates a new <c>pg_dump</c> snapshot and returns its file name and size.
    /// </remarks>
    /// <response code="200">Backup created successfully.</response>
    /// <response code="500">The backup failed (for example, <c>pg_dump</c> is not available).</response>
    [HttpPost]
    [ProducesResponseType(typeof(BackupFileResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BackupFileResponseDto>> CreateBackup(
        CancellationToken cancellationToken
    )
    {
        _logger.LogInformation(
            "Backup requested by user: {UserId}",
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "unknown"
        );

        var backup = await _backupService.CreateBackupAsync(cancellationToken);

        return Ok(
            new BackupFileResponseDto
            {
                FileName = backup.FileName,
                SizeBytes = backup.SizeBytes,
                CreatedAtUtc = backup.CreatedAtUtc,
            }
        );
    }

    /// <summary>
    /// Restores the database from a backup file.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Warning:</strong> Restore overwrites the current database contents. Existing
    /// objects are dropped before the backup is replayed.
    /// </para>
    /// </remarks>
    /// <param name="request">Request containing the backup file name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Database restored successfully.</response>
    /// <response code="400">The backup file name is invalid or the restore failed.</response>
    [HttpPost("restore")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Restore(
        [FromBody] RestoreBackupRequestDto request,
        CancellationToken cancellationToken
    )
    {
        if (request is null || string.IsNullOrWhiteSpace(request.FileName))
        {
            return BadRequest(new { Message = "Backup file name is required" });
        }

        _logger.LogWarning(
            "Database restore requested by user: {UserId}, File: {FileName}",
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "unknown",
            request.FileName
        );

        var restored = await _backupService.RestoreAsync(request.FileName, cancellationToken);
        if (!restored)
        {
            return BadRequest(
                new { Message = $"Backup file '{request.FileName}' not found or restore failed" }
            );
        }

        return Ok(new { Message = "Database restored successfully" });
    }

    /// <summary>
    /// Deletes a database backup file.
    /// </summary>
    /// <param name="fileName">Name of the backup file to delete.</param>
    /// <response code="200">Backup file deleted.</response>
    /// <response code="404">Backup file not found.</response>
    [HttpDelete("{fileName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBackup(string fileName)
    {
        var deleted = await _backupService.DeleteBackupAsync(fileName);
        if (!deleted)
        {
            return NotFound(new { Message = $"Backup file '{fileName}' not found" });
        }

        _logger.LogInformation(
            "Backup deleted by user: {UserId}, File: {FileName}",
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "unknown",
            fileName
        );

        return Ok(new { Message = "Backup file deleted" });
    }
}
