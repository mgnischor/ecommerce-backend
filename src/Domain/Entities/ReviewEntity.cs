namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents a product review by a customer
/// </summary>
public sealed class ReviewEntity
{
    /// <summary>
    /// Unique identifier for the review
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Product being reviewed
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Customer who wrote the review
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Order that this review is associated with (optional)
    /// </summary>
    public Guid? OrderId { get; set; }

    /// <summary>
    /// Rating (1-5 stars)
    /// </summary>
    public int Rating { get; set; }

    /// <summary>
    /// Review title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Review content/comment
    /// </summary>
    public string Comment { get; set; } = string.Empty;

    /// <summary>
    /// Whether the review is verified (customer purchased the product)
    /// </summary>
    public bool IsVerifiedPurchase { get; set; } = false;

    /// <summary>
    /// Whether the review is approved by admin
    /// </summary>
    public bool IsApproved { get; set; } = false;

    /// <summary>
    /// Whether the review is flagged for moderation
    /// </summary>
    public bool IsFlagged { get; set; } = false;

    /// <summary>
    /// Number of helpful votes
    /// </summary>
    public int HelpfulCount { get; set; } = 0;

    /// <summary>
    /// Number of not helpful votes
    /// </summary>
    public int NotHelpfulCount { get; set; } = 0;

    /// <summary>
    /// Admin response to the review
    /// </summary>
    public string? AdminResponse { get; set; }

    /// <summary>
    /// Date when admin responded
    /// </summary>
    public DateTime? AdminRespondedAt { get; set; }

    /// <summary>
    /// Whether the review is deleted (soft delete)
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Date and time when the review was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the review was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
