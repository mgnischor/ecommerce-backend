using System.Diagnostics;
using ECommerce.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace ECommerce.Infrastructure.Services;

/// <summary>
/// PostgreSQL database backup and disaster recovery service using <c>pg_dump</c> / <c>pg_restore</c>.
/// </summary>
/// <remarks>
/// <para>
/// Runs as a hosted service that creates a scheduled backup every
/// <c>Backup:IntervalHours</c> hours when <c>Backup:Enabled</c> is <see langword="true"/>.
/// Backups can also be triggered on demand through <see cref="CreateBackupAsync"/>.
/// </para>
/// <para>
/// The connection is parsed from <c>ConnectionStrings:DefaultConnection</c>. The password is
/// passed to the child process through the <c>PGPASSWORD</c> environment variable so it never
/// appears on the command line.
/// </para>
/// </remarks>
public sealed class DatabaseBackupService : BackgroundService, IDatabaseBackupService
{
    private const string DefaultBackupDirectory = "backups";
    private const int DefaultIntervalHours = 24;
    private const int DefaultRetentionDays = 7;

    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseBackupService> _logger;
    private readonly string _backupDirectory;
    private readonly int _intervalHours;
    private readonly int _retentionDays;
    private readonly string _pgDumpPath;
    private readonly string _pgRestorePath;

    public DatabaseBackupService(
        IConfiguration configuration,
        ILogger<DatabaseBackupService> logger
    )
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _backupDirectory = _configuration["Backup:Directory"] ?? DefaultBackupDirectory;
        _intervalHours = _configuration.GetValue("Backup:IntervalHours", DefaultIntervalHours);
        _retentionDays = _configuration.GetValue("Backup:RetentionDays", DefaultRetentionDays);
        _pgDumpPath = _configuration["Backup:PgDumpPath"] ?? "pg_dump";
        _pgRestorePath = _configuration["Backup:PgRestorePath"] ?? "pg_restore";

        Directory.CreateDirectory(_backupDirectory);
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var enabled = _configuration.GetValue("Backup:Enabled", false);
        if (!enabled)
        {
            _logger.LogInformation(
                "Database backup service is disabled (Backup:Enabled = false). No scheduled backups will run."
            );
            return;
        }

        _logger.LogInformation(
            "Database backup service started. Interval: {IntervalHours}h, Directory: {Directory}, Retention: {RetentionDays} days.",
            _intervalHours,
            Path.GetFullPath(_backupDirectory),
            _retentionDays
        );

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var backup = await CreateBackupAsync(stoppingToken);
                _logger.LogInformation(
                    "Scheduled backup completed: {FileName} ({SizeBytes} bytes)",
                    backup.FileName,
                    backup.SizeBytes
                );
                CleanupOldBackups();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Scheduled database backup failed");
            }

            try
            {
                await Task.Delay(TimeSpan.FromHours(_intervalHours), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    /// <inheritdoc />
    public async Task<BackupFileInfo> CreateBackupAsync(
        CancellationToken cancellationToken = default
    )
    {
        var connection = GetConnectionInfo();
        var fileName = $"{DateTime.UtcNow:yyyyMMdd-HHmmss}_{connection.Database}.dump";
        var outputPath = Path.Combine(_backupDirectory, fileName);

        var arguments =
            $"-h {connection.Host} -p {connection.Port} -U {connection.Username} -d {connection.Database} -Fc -f \"{outputPath}\"";

        _logger.LogInformation(
            "Starting database backup for database '{Database}' -> {Output}",
            connection.Database,
            outputPath
        );

        var (exitCode, error) = await RunProcessAsync(
            _pgDumpPath,
            arguments,
            connection.Password,
            cancellationToken
        );

        if (exitCode != 0)
        {
            throw new InvalidOperationException(
                $"pg_dump failed with exit code {exitCode}. {error}"
            );
        }

        var fileInfo = new FileInfo(outputPath);
        _logger.LogInformation(
            "Database backup completed: {FileName} ({SizeBytes} bytes)",
            fileInfo.Name,
            fileInfo.Length
        );

        return new BackupFileInfo
        {
            FileName = fileInfo.Name,
            FullPath = fileInfo.FullName,
            SizeBytes = fileInfo.Length,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<BackupFileInfo>> ListBackupsAsync()
    {
        var backups = Directory
            .EnumerateFiles(_backupDirectory, "*.dump", SearchOption.TopDirectoryOnly)
            .Select(path =>
            {
                var info = new FileInfo(path);
                return new BackupFileInfo
                {
                    FileName = info.Name,
                    FullPath = info.FullName,
                    SizeBytes = info.Length,
                    CreatedAtUtc = info.CreationTimeUtc,
                };
            })
            .OrderByDescending(b => b.CreatedAtUtc)
            .ToList();

        return Task.FromResult<IReadOnlyList<BackupFileInfo>>(backups);
    }

    /// <inheritdoc />
    public async Task<bool> RestoreAsync(
        string fileName,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        var fullPath = Path.GetFullPath(Path.Combine(_backupDirectory, fileName));

        // Prevent path traversal outside the backup directory.
        if (
            !fullPath.StartsWith(
                Path.GetFullPath(_backupDirectory),
                StringComparison.OrdinalIgnoreCase
            ) || !File.Exists(fullPath)
        )
        {
            _logger.LogWarning(
                "Restore attempt with invalid backup file name: {FileName}",
                fileName
            );
            return false;
        }

        var connection = GetConnectionInfo();
        var arguments =
            $"-h {connection.Host} -p {connection.Port} -U {connection.Username} -d {connection.Database} --clean --if-exists \"{fullPath}\"";

        _logger.LogWarning(
            "Restoring database '{Database}' from backup '{FileName}'. Existing data will be overwritten.",
            connection.Database,
            fileName
        );

        var (exitCode, error) = await RunProcessAsync(
            _pgRestorePath,
            arguments,
            connection.Password,
            cancellationToken
        );

        if (exitCode != 0)
        {
            _logger.LogError(
                "Database restore failed with exit code {ExitCode}. {Error}",
                exitCode,
                error
            );
            return false;
        }

        _logger.LogInformation(
            "Database restore completed successfully from '{FileName}'",
            fileName
        );
        return true;
    }

    /// <inheritdoc />
    public Task<bool> DeleteBackupAsync(
        string fileName,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        var fullPath = Path.GetFullPath(Path.Combine(_backupDirectory, fileName));

        if (
            !fullPath.StartsWith(
                Path.GetFullPath(_backupDirectory),
                StringComparison.OrdinalIgnoreCase
            ) || !File.Exists(fullPath)
        )
        {
            return Task.FromResult(false);
        }

        File.Delete(fullPath);
        _logger.LogInformation("Backup file deleted: {FileName}", fileName);
        return Task.FromResult(true);
    }

    private void CleanupOldBackups()
    {
        if (_retentionDays <= 0)
        {
            return;
        }

        var cutoff = DateTime.UtcNow.AddDays(-_retentionDays);
        var removed = 0;

        foreach (var file in Directory.EnumerateFiles(_backupDirectory, "*.dump"))
        {
            if (File.GetCreationTimeUtc(file) < cutoff)
            {
                File.Delete(file);
                removed++;
            }
        }

        if (removed > 0)
        {
            _logger.LogInformation(
                "Cleaned up {Count} backup file(s) older than {RetentionDays} day(s)",
                removed,
                _retentionDays
            );
        }
    }

    private (
        string Host,
        int Port,
        string Database,
        string Username,
        string? Password
    ) GetConnectionInfo()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured."
            );
        }

        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        return (
            builder.Host ?? "localhost",
            builder.Port,
            builder.Database ?? "postgres",
            builder.Username ?? "postgres",
            builder.Password
        );
    }

    private async Task<(int ExitCode, string Error)> RunProcessAsync(
        string fileName,
        string arguments,
        string? password,
        CancellationToken cancellationToken
    )
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        if (!string.IsNullOrEmpty(password))
        {
            startInfo.Environment["PGPASSWORD"] = password;
        }

        using var process = Process.Start(startInfo);
        if (process is null)
        {
            throw new InvalidOperationException($"Failed to start process '{fileName}'.");
        }

        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        var output = await outputTask;
        var error = await errorTask;

        if (!string.IsNullOrWhiteSpace(output))
        {
            _logger.LogDebug("pg_dump/pg_restore output: {Output}", output);
        }

        return (process.ExitCode, error.Trim());
    }
}
