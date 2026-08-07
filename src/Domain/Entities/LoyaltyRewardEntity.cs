namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents a customer loyalty account in the rewards program.
/// Tracks points balance, earning history, and loyalty tier.
/// </summary>
public class LoyaltyRewardEntity
{
    /// <summary>
    /// Unique identifier for the loyalty account
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Customer associated with this loyalty account
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Current available points balance
    /// </summary>
    public int PointBalance { get; set; }

    /// <summary>
    /// Total points earned over the lifetime of the account
    /// </summary>
    public int TotalPointsEarned { get; set; }

    /// <summary>
    /// Total points redeemed over the lifetime of the account
    /// </summary>
    public int TotalPointsRedeemed { get; set; }

    /// <summary>
    /// Date when points were last earned
    /// </summary>
    public DateTime? LastEarnedAt { get; set; }

    /// <summary>
    /// Current loyalty tier (Bronze, Silver, Gold, Platinum)
    /// </summary>
    public string Tier { get; set; } = "Bronze";

    /// <summary>
    /// Whether the loyalty account is deleted (soft delete)
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Date and time when the loyalty account was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the loyalty account was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
