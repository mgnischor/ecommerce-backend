using System.ComponentModel.DataAnnotations;

namespace ECommerce.API.DTOs;

/// <summary>
/// Data Transfer Object for earning reward points on a loyalty account
/// </summary>
public sealed class EarnPointsRequestDto
{
    /// <summary>
    /// Gets or sets the amount spent that generates the points
    /// </summary>
    /// <value>The purchase amount used to calculate points earned.</value>
    [Required(ErrorMessage = "Amount spent is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Amount spent must not be negative")]
    public decimal AmountSpent { get; set; }

    /// <summary>
    /// Gets or sets the number of reviews contributing to the points earned
    /// </summary>
    /// <value>The review count used to calculate review points.</value>
    [Range(0, int.MaxValue, ErrorMessage = "Review count must not be negative")]
    public int ReviewCount { get; set; }

    /// <summary>
    /// Gets or sets the average rating of the submitted reviews
    /// </summary>
    /// <value>The average rating used to calculate review bonus points.</value>
    [Range(0, 5, ErrorMessage = "Average rating must be between 0 and 5")]
    public decimal AverageRating { get; set; }
}

/// <summary>
/// Data Transfer Object for redeeming reward points on a loyalty account
/// </summary>
public sealed class RedeemPointsRequestDto
{
    /// <summary>
    /// Gets or sets the cost in points of the reward being redeemed
    /// </summary>
    /// <value>The point cost of the reward. Must be greater than zero.</value>
    [Required(ErrorMessage = "Reward cost is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Reward cost must be greater than zero")]
    public int RewardCost { get; set; }
}
