namespace Comex.Domain.Policies;

/// <summary>
/// Defines business rules for the customer rewards and loyalty program
/// </summary>
public static class RewardsProgramPolicy
{
    private const int PointsEarnedPerDollar = 1;
    private const int PointsEarnedPerReview = 25;
    private const int MaxPointsEarnedPerReview = 100;
    private const int PointsExpirationDays = 365;
    private const int PointsPerRewardDollar = 100;

    /// <summary>
    /// Validates if a point balance is acceptable
    /// </summary>
    public static bool IsValidPointBalance(int pointBalance)
    {
        return pointBalance >= 0;
    }

    /// <summary>
    /// Calculates the number of points earned for a purchase
    /// </summary>
    public static int CalculatePointsForSpend(decimal amountSpent)
    {
        if (amountSpent <= 0)
            return 0;

        return (int)Math.Floor(amountSpent * PointsEarnedPerDollar);
    }

    /// <summary>
    /// Calculates the number of points earned for product reviews
    /// </summary>
    public static int CalculatePointsForReview(int reviewCount, decimal averageRating)
    {
        if (reviewCount <= 0)
            return 0;

        var basePoints = reviewCount * PointsEarnedPerReview;

        // Reviews with high ratings earn additional points
        if (averageRating >= 4.5m)
            basePoints += reviewCount * 10;

        return Math.Min(basePoints, MaxPointsEarnedPerReview);
    }

    /// <summary>
    /// Checks if a customer has enough points for a reward
    /// </summary>
    public static bool HasEnoughPointsForReward(int pointBalance, int rewardCost)
    {
        if (rewardCost <= 0)
            return false;

        return pointBalance >= rewardCost;
    }

    /// <summary>
    /// Calculates the monetary value of a given number of points
    /// </summary>
    public static decimal CalculateRewardValue(int points)
    {
        if (points <= 0)
            return 0m;

        return Math.Round((decimal)points / PointsPerRewardDollar, 2);
    }

    /// <summary>
    /// Checks if earned points have expired
    /// </summary>
    public static bool ArePointsExpired(DateTime? lastEarnedAt)
    {
        if (!lastEarnedAt.HasValue)
            return false;

        var daysSinceEarned = (DateTime.UtcNow - lastEarnedAt.Value).TotalDays;
        return daysSinceEarned > PointsExpirationDays;
    }

    /// <summary>
    /// Determines the loyalty tier based on the point balance
    /// </summary>
    public static string GetTierFromPoints(int points)
    {
        return points switch
        {
            >= 10000 => "Platinum",
            >= 5000 => "Gold",
            >= 2000 => "Silver",
            _ => "Bronze",
        };
    }

    /// <summary>
    /// Determines the point earning multiplier for a loyalty tier
    /// </summary>
    public static decimal GetTierEarningMultiplier(string tier)
    {
        return tier switch
        {
            "Platinum" => 2.0m,
            "Gold" => 1.5m,
            "Silver" => 1.25m,
            "Bronze" => 1.0m,
            _ => 1.0m,
        };
    }
}
