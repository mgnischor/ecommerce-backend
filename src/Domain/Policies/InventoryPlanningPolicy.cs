namespace Comex.Domain.Policies;

/// <summary>
/// Defines business rules for inventory planning and demand forecasting
/// </summary>
public static class InventoryPlanningPolicy
{
    private const int DefaultForecastPeriodDays = 30;
    private const int DefaultLeadTimeDays = 7;
    private const int DefaultServiceLevelPercent = 95;
    private const int DefaultMaximumStockAgeDays = 180;
    private const decimal DefaultSlowMovingThreshold = 10m;

    /// <summary>
    /// Calculates a simple moving average sales forecast
    /// </summary>
    public static int CalculateSalesForecast(int[] historicalSales, int? forecastPeriodDays)
    {
        if (historicalSales == null || historicalSales.Length == 0)
            return 0;

        var period = forecastPeriodDays ?? DefaultForecastPeriodDays;
        var dailyAverage = (decimal)historicalSales.Sum() / historicalSales.Length;
        return (int)Math.Ceiling(dailyAverage * period);
    }

    /// <summary>
    /// Calculates the safety stock required to cover lead time variability
    /// </summary>
    public static int CalculateSafetyStock(
        int averageDailySales,
        int? leadTimeDays,
        int? serviceLevelPercent
    )
    {
        if (averageDailySales <= 0)
            return 0;

        var effectiveLeadTime = leadTimeDays ?? DefaultLeadTimeDays;
        var effectiveServiceLevel = serviceLevelPercent ?? DefaultServiceLevelPercent;

        // Service level factor: 90% = 1.28, 95% = 1.65, 99% = 2.33
        var factor = effectiveServiceLevel switch
        {
            >= 99 => 2.33m,
            >= 95 => 1.65m,
            >= 90 => 1.28m,
            _ => 1.0m,
        };

        return (int)Math.Ceiling(averageDailySales * effectiveLeadTime * factor);
    }

    /// <summary>
    /// Checks if a reorder should be triggered
    /// </summary>
    public static bool IsReorderNeeded(int currentStock, int reorderPoint)
    {
        if (reorderPoint <= 0)
            return false;

        return currentStock <= reorderPoint;
    }

    /// <summary>
    /// Calculates how many days the current stock will last
    /// </summary>
    public static int CalculateDaysOfSupply(int currentStock, int averageDailySales)
    {
        if (currentStock <= 0)
            return 0;

        if (averageDailySales <= 0)
            return DefaultForecastPeriodDays; // No demand, treat as full period

        return currentStock / averageDailySales;
    }

    /// <summary>
    /// Checks if stock has aged beyond the acceptable threshold
    /// </summary>
    public static bool IsStockAgeExcessive(DateTime receivedAt, int? maximumAgeDays)
    {
        var effectiveMaxAge = maximumAgeDays ?? DefaultMaximumStockAgeDays;
        var ageInDays = (DateTime.UtcNow - receivedAt).TotalDays;
        return ageInDays > effectiveMaxAge;
    }

    /// <summary>
    /// Calculates the inventory turnover ratio
    /// </summary>
    public static decimal CalculateInventoryTurnover(
        decimal costOfGoodsSold,
        decimal averageInventoryValue
    )
    {
        if (averageInventoryValue <= 0)
            return 0m;

        return Math.Round(costOfGoodsSold / averageInventoryValue, 2);
    }

    /// <summary>
    /// Checks if slow-moving stock should be liquidated
    /// </summary>
    public static bool ShouldLiquidateSlowMoving(int currentStock, int salesInLast90Days)
    {
        if (currentStock <= 0)
            return false;

        var turnoverRatio = (decimal)salesInLast90Days / currentStock;
        return turnoverRatio < DefaultSlowMovingThreshold / 100;
    }
}
