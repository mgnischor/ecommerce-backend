namespace Comex.Domain.Policies;

/// <summary>
/// Defines business rules for product search and catalog queries
/// </summary>
public static class SearchPolicy
{
    private const int MinSearchTermLength = 2;
    private const int MaxSearchTermLength = 100;
    private const int DefaultPageSize = 20;
    private const int MaximumPageSize = 100;
    private const decimal MaximumPrice = 999999.99m;

    /// <summary>
    /// Validates if a search term is acceptable
    /// </summary>
    public static bool IsValidSearchTerm(string? term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return true; // Empty term returns all products

        return term.Trim().Length >= MinSearchTermLength && term.Trim().Length <= MaxSearchTermLength;
    }

    /// <summary>
    /// Validates if a page number is acceptable
    /// </summary>
    public static bool IsValidPageNumber(int pageNumber)
    {
        return pageNumber >= 1;
    }

    /// <summary>
    /// Validates if a page size is within acceptable limits
    /// </summary>
    public static bool IsValidPageSize(int pageSize)
    {
        return pageSize >= 1 && pageSize <= MaximumPageSize;
    }

    /// <summary>
    /// Determines if out-of-stock products should be included in results
    /// </summary>
    public static bool ShouldIncludeOutOfStockProducts(bool userPreference, bool includeByDefault)
    {
        // Respect explicit user preference, otherwise use the default configuration
        return userPreference || includeByDefault;
    }

    /// <summary>
    /// Resolves the default sort order when the requested sort is not recognized
    /// </summary>
    public static string GetDefaultSortOrder(string? requestedSort, string[] validSortOptions)
    {
        if (!string.IsNullOrWhiteSpace(requestedSort) && validSortOptions.Contains(requestedSort))
            return requestedSort;

        return "relevance";
    }

    /// <summary>
    /// Validates a price range filter
    /// </summary>
    public static bool IsValidPriceRange(decimal? minPrice, decimal? maxPrice)
    {
        if (minPrice.HasValue && minPrice.Value < 0)
            return false;

        if (maxPrice.HasValue && (maxPrice.Value <= 0 || maxPrice.Value > MaximumPrice))
            return false;

        // Minimum price must not exceed maximum price
        if (minPrice.HasValue && maxPrice.HasValue && minPrice.Value > maxPrice.Value)
            return false;

        return true;
    }
}
