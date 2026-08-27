namespace Comex.Domain.Policies;

/// <summary>
/// Defines business rules for store configuration and operation
/// </summary>
public static class StorePolicy
{
    private const int MinStoreNameLength = 2;
    private const int MaxStoreNameLength = 100;

    /// <summary>
    /// Validates a store name
    /// </summary>
    public static bool IsValidStoreName(string? storeName)
    {
        if (string.IsNullOrWhiteSpace(storeName))
            return false;

        return storeName.Trim().Length is >= MinStoreNameLength and <= MaxStoreNameLength;
    }

    /// <summary>
    /// Checks if a store is currently active
    /// </summary>
    public static bool IsStoreActive(bool isActive, bool isDeleted, DateTime? suspendedUntil)
    {
        if (!isActive || isDeleted)
            return false;

        // Store is considered inactive while under a suspension period
        if (suspendedUntil.HasValue && DateTime.UtcNow < suspendedUntil.Value)
            return false;

        return true;
    }

    /// <summary>
    /// Checks if a store can accept new orders
    /// </summary>
    public static bool CanAcceptOrders(bool isActive, bool isWithinBusinessHours)
    {
        // The store must be active and within business hours to accept orders
        return isActive && isWithinBusinessHours;
    }

    /// <summary>
    /// Checks if the current time falls within the store's business hours
    /// </summary>
    public static bool IsWithinBusinessHours(
        TimeOnly currentTime,
        TimeOnly openingTime,
        TimeOnly closingTime
    )
    {
        // Handle stores that close after midnight
        if (closingTime < openingTime)
            return currentTime >= openingTime || currentTime < closingTime;

        return currentTime >= openingTime && currentTime < closingTime;
    }
}
