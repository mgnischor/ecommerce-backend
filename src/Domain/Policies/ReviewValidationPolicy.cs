namespace ECommerce.Domain.Policies;

/// <summary>
/// Defines business rules for review moderation and validation
/// </summary>
public static class ReviewValidationPolicy
{
    private const int MinRating = 1;
    private const int MaxRating = 5;
    private const int MinTitleLength = 3;
    private const int MaxTitleLength = 200;
    private const int MinCommentLength = 10;
    private const int MaxCommentLength = 5000;
    private const int ReviewEditWindowHours = 24;

    /// <summary>
    /// Validates if a rating value is within acceptable range
    /// </summary>
    public static bool IsValidRating(int rating)
    {
        return rating >= MinRating && rating <= MaxRating;
    }

    /// <summary>
    /// Validates if review title meets length requirements
    /// </summary>
    public static bool IsValidTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return false;

        var trimmedLength = title.Trim().Length;
        return trimmedLength >= MinTitleLength && trimmedLength <= MaxTitleLength;
    }

    /// <summary>
    /// Validates if review comment meets length requirements
    /// </summary>
    public static bool IsValidComment(string comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
            return false;

        var trimmedLength = comment.Trim().Length;
        return trimmedLength >= MinCommentLength && trimmedLength <= MaxCommentLength;
    }

    /// <summary>
    /// Checks if a customer is eligible to review a product
    /// </summary>
    public static bool CanCustomerReview(bool hasPurchased, bool hasExistingReview)
    {
        // Customer must have purchased and not already reviewed
        return hasPurchased && !hasExistingReview;
    }

    /// <summary>
    /// Checks if a review can be edited based on time elapsed
    /// </summary>
    public static bool IsWithinEditWindow(DateTime reviewCreatedAt)
    {
        var timeElapsed = DateTime.UtcNow - reviewCreatedAt;
        return timeElapsed.TotalHours <= ReviewEditWindowHours;
    }

    /// <summary>
    /// Checks if a review requires moderation based on content
    /// </summary>
    public static bool RequiresModeration(string title, string comment, int rating)
    {
        var combinedText = $"{title} {comment}".ToLowerInvariant();

        // List of words that trigger moderation
        var flaggedWords = new[]
        {
            "spam",
            "fake",
            "scam",
            "fraud",
            "terrible",
            "worst",
            "horrible",
        };

        // Extreme ratings (1 or 5 stars) might need moderation
        var extremeRating = rating == 1 || rating == 5;

        // Check for flagged words
        var containsFlaggedWords = flaggedWords.Any(word => combinedText.Contains(word));

        return extremeRating && containsFlaggedWords;
    }

    /// <summary>
    /// Calculates if a review is helpful based on votes
    /// </summary>
    public static bool IsHelpful(int helpfulCount, int notHelpfulCount)
    {
        var totalVotes = helpfulCount + notHelpfulCount;

        if (totalVotes < 5) // Need minimum votes to determine
            return false;

        var helpfulPercentage = (decimal)helpfulCount / totalVotes;
        return helpfulPercentage >= 0.6m; // 60% threshold
    }

    /// <summary>
    /// Validates if a review can be deleted
    /// </summary>
    public static bool CanDeleteReview(Guid customerId, Guid reviewAuthorId, bool isAdmin)
    {
        // Users can delete their own reviews, admins can delete any review
        return customerId == reviewAuthorId || isAdmin;
    }
}
