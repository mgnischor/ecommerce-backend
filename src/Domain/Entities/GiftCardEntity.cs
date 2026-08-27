namespace Comex.Domain.Entities;

/// <summary>
/// Represents a gift card in the e-commerce system.
/// Tracks the card number, balance, redemption, and validity lifecycle.
/// </summary>
public class GiftCardEntity
{
    /// <summary>
    /// Unique identifier for the gift card
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User who created this gift card
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// User who last updated this gift card
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// Card number (13-19 digits, may include dashes)
    /// </summary>
    public string CardNumber { get; set; } = string.Empty;

    /// <summary>
    /// Current balance available on the card
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    /// Whether the card is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether the card has been revoked
    /// </summary>
    public bool IsRevoked { get; set; } = false;

    /// <summary>
    /// Whether the card can be reloaded with additional funds
    /// </summary>
    public bool AllowsReload { get; set; } = true;

    /// <summary>
    /// Date when the card was issued
    /// </summary>
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Expiration date of the card (defaults to 24 months after issue)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Whether the gift card is deleted (soft delete)
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Date and time when the gift card was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the gift card was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
