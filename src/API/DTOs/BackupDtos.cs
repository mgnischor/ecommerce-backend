using System.ComponentModel.DataAnnotations;

namespace ECommerce.API.DTOs;

/// <summary>
/// Response returned for a database backup file.
/// </summary>
public sealed class BackupFileResponseDto
{
    public string FileName { get; init; } = string.Empty;
    public long SizeBytes { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}

/// <summary>
/// Request payload to restore the database from an existing backup file.
/// </summary>
public sealed class RestoreBackupRequestDto
{
    /// <summary>Name of the backup file to restore (as returned by the backup list endpoint).</summary>
    [Required]
    public string FileName { get; set; } = string.Empty;
}
