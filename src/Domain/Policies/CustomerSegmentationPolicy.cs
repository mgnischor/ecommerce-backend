namespace Comex.Domain.Policies;

/// <summary>
/// Defines business rules for customer segmentation and lifecycle analysis
/// </summary>
public static class CustomerSegmentationPolicy
{
    private const decimal HighValueThreshold = 5000m;
    private const int NewCustomerOrderThreshold = 1;
    private const int ChurnThresholdDays = 90;
    private const int DormantThresholdDays = 365;

    /// <summary>
    /// Determines the customer segment based on spending and order history
    /// </summary>
    public static string GetCustomerSegment(decimal totalSpending, int totalOrders)
    {
        if (IsHighValueCustomer(totalSpending) && totalOrders >= 10)
            return "VIP";

        if (totalSpending >= 1000m && totalOrders >= 5)
            return "Regular";

        if (IsNewCustomer(totalOrders))
            return "New";

        return "Occasional";
    }

    /// <summary>
    /// Checks if a customer is considered high value
    /// </summary>
    public static bool IsHighValueCustomer(decimal totalSpending)
    {
        return totalSpending >= HighValueThreshold;
    }

    /// <summary>
    /// Checks if a customer is at risk of churning
    /// </summary>
    public static bool IsAtRiskCustomer(int daysSinceLastOrder, int? historicalAverageOrderDays)
    {
        if (historicalAverageOrderDays.HasValue && historicalAverageOrderDays.Value > 0)
        {
            // Customer is at risk when inactivity exceeds 2x their normal cadence
            return daysSinceLastOrder > (historicalAverageOrderDays.Value * 2);
        }

        return daysSinceLastOrder > (ChurnThresholdDays / 2);
    }

    /// <summary>
    /// Checks if a customer is new to the platform
    /// </summary>
    public static bool IsNewCustomer(int totalOrders)
    {
        return totalOrders <= NewCustomerOrderThreshold;
    }

    /// <summary>
    /// Checks if a customer has churned
    /// </summary>
    public static bool IsChurnedCustomer(int daysSinceLastOrder)
    {
        return daysSinceLastOrder > ChurnThresholdDays;
    }

    /// <summary>
    /// Determines the churn risk level for a customer
    /// </summary>
    public static string GetChurnRiskLevel(int daysSinceLastOrder, int totalOrders)
    {
        if (totalOrders <= 0)
            return "Critical";

        if (daysSinceLastOrder > ChurnThresholdDays)
            return "Critical";

        if (daysSinceLastOrder > (ChurnThresholdDays * 2 / 3))
            return "High";

        if (daysSinceLastOrder > (ChurnThresholdDays / 2))
            return "Medium";

        return "Low";
    }

    /// <summary>
    /// Checks if a customer is considered dormant
    /// </summary>
    public static bool IsDormantCustomer(int daysSinceLastLogin)
    {
        return daysSinceLastLogin > DormantThresholdDays;
    }
}
