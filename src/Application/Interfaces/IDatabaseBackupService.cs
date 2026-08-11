namespace ECommerce.Application.Interfaces;

/// <summary>
/// Defines operations for PostgreSQL database backup and disaster recovery.
/// </summary>
/// <remarks>
/// <para>
/// Provides on-demand backup, listing, restore, and deletion of database snapshot files.
/// A hosted service (see <c>DatabaseBackupService</c>) additionally performs scheduled backups
/// when <c>Backup:Enabled</c> is <see langword="true"/>.
/// </para>
/// <para>
/// Backups are produced with <c>pg_dump</c> using the PostgreSQL custom format (<c>-Fc</c>),
/// which is compressed and supports selective/parallel restore via <c>pg_restore</c>.
/// </para>
/// </remarks>
public interface IDatabaseBackupService
{
    /// <summary>
    /// Creates a database backup and persists it to the configured backup directory.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Information about the created backup file.</returns>
    Task<BackupFileInfo> CreateBackupAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists the backup files currently present in the backup directory.
    /// </summary>
    /// <returns>A read-only list of backup files, ordered newest first.</returns>
    Task<IReadOnlyList<BackupFileInfo>> ListBackupsAsync();

    /// <summary>
    /// Restores the database from a previously created backup file.
    /// </summary>
    /// <remarks>
    /// The restore drops existing objects (<c>--clean --if-exists</c>) before recreating them.
    /// Use with caution; it overwrites the current database contents.
    /// </remarks>
    /// <param name="fileName">Name of the backup file (as returned by <see cref="ListBackupsAsync"/>).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see langword="true"/> when the restore completed successfully.</returns>
    Task<bool> RestoreAsync(string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a backup file from the backup directory.
    /// </summary>
    /// <param name="fileName">Name of the backup file to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see langword="true"/> when the file was removed.</returns>
    Task<bool> DeleteBackupAsync(string fileName, CancellationToken cancellationToken = default);
}
