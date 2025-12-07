using ECommerce.API.DTOs;

namespace ECommerce.API.Services;

/// <summary>
/// Defines read-only accounting query operations for controllers.
/// </summary>
public interface IAccountingQueryService
{
    /// <summary>
    /// Retrieves the full chart of accounts as response DTOs.
    /// </summary>
    Task<IReadOnlyList<ChartOfAccountsResponseDto>> GetChartOfAccountsAsync(
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves a single account by identifier.
    /// </summary>
    Task<ChartOfAccountsResponseDto?> GetAccountByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves paged journal entries with their accounting lines.
    /// </summary>
    Task<IReadOnlyList<JournalEntryResponseDto>> GetJournalEntriesAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves a specific journal entry with accounting lines.
    /// </summary>
    Task<JournalEntryResponseDto?> GetJournalEntryByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves journal entries related to a given product.
    /// </summary>
    Task<IReadOnlyList<JournalEntryResponseDto>> GetJournalEntriesByProductAsync(
        Guid productId,
        CancellationToken cancellationToken = default
    );
}
