namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents a product review submitted by a customer in the e-commerce system.
/// Manages customer feedback, ratings, and moderation of product reviews.
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
    /// Order that this review is associated with (optional).
    /// Used to verify purchase and mark as verified purchase.
    /// </summary>
    public Guid? OrderId { get; set; }

    /// <summary>
    /// Rating given by the customer (1-5 stars, where 5 is the highest)
    /// </summary>
    public int Rating { get; set; }

    /// <summary>
    /// Review title/headline
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed review content and customer feedback
    /// </summary>
    public string Comment { get; set; } = string.Empty;

    /// <summary>
    /// Whether the review is from a verified purchase.
    /// True if the customer actually purchased the product.
    /// </summary>
    public bool IsVerifiedPurchase { get; set; } = false;

    /// <summary>
    /// Whether the review has been approved by an administrator for public display
    /// </summary>
    public bool IsApproved { get; set; } = false;

    /// <summary>
    /// Whether the review has been flagged for moderation review.
    /// Used to mark potentially inappropriate or spam reviews.
    /// </summary>
    public bool IsFlagged { get; set; } = false;

    /// <summary>
    /// Number of users who found this review helpful
    /// </summary>
    public int HelpfulCount { get; set; } = 0;

    /// <summary>
    /// Number of users who did not find this review helpful
    /// </summary>
    public int NotHelpfulCount { get; set; } = 0;

    /// <summary>
    /// Official response from administrator or vendor to this review
    /// </summary>
    public string? AdminResponse { get; set; }

    /// <summary>
    /// Date and time when the administrator responded to this review
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
