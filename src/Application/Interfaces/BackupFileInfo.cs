namespace ECommerce.Application.Interfaces;

/// <summary>
/// Describes a single database backup file on disk.
/// </summary>
public sealed class BackupFileInfo
{
    /// <summary>File name of the backup, e.g. <c>20260614-120000_ecommerce.dump</c>.</summary>
    public string FileName { get; init; } = string.Empty;

    /// <summary>Full path of the backup file.</summary>
    public string FullPath { get; init; } = string.Empty;

    /// <summary>Size of the backup file in bytes.</summary>
    public long SizeBytes { get; init; }

    /// <summary>UTC timestamp when the backup was created.</summary>
    public DateTime CreatedAtUtc { get; init; }
}
