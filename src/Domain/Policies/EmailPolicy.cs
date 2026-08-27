using Comex.Domain.Enums;

namespace Comex.Domain.Policies;

/// <summary>
/// Defines business rules for email and communication delivery
/// </summary>
public static class EmailPolicy
{
    private const int MaxSubjectLength = 150;
    private const int MinSubjectLength = 1;
    private const int MaxBodyLength = 100000;
    private const int MarketingStartHour = 8;
    private const int MarketingEndHour = 21;

    /// <summary>
    /// Validates an email subject line
    /// </summary>
    public static bool IsValidEmailSubject(string? subject)
    {
        if (string.IsNullOrWhiteSpace(subject))
            return false;

        return subject.Trim().Length >= MinSubjectLength && subject.Trim().Length <= MaxSubjectLength;
    }

    /// <summary>
    /// Validates the length of an email body
    /// </summary>
    public static bool IsValidEmailBodyLength(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return false;

        return body.Length <= MaxBodyLength;
    }

    /// <summary>
    /// Validates a recipient email address format
    /// </summary>
    public static bool IsValidRecipientEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var address = new System.Net.Mail.MailAddress(email);
            return address.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if a notification type represents a transactional email
    /// </summary>
    public static bool IsTransactionalEmail(NotificationType type)
    {
        return type switch
        {
            NotificationType.Order => true,
            NotificationType.Payment => true,
            NotificationType.Shipment => true,
            NotificationType.Account => true,
            _ => false,
        };
    }

    /// <summary>
    /// Checks if marketing consent is required before sending an email
    /// </summary>
    public static bool RequiresMarketingConsent(bool hasConsent, bool isTransactional)
    {
        // Transactional emails never require separate marketing consent
        if (isTransactional)
            return false;

        return !hasConsent;
    }

    /// <summary>
    /// Checks if the current time falls within allowed marketing sending hours
    /// </summary>
    public static bool IsWithinSendingHours(DateTime sendTime)
    {
        var hour = sendTime.Hour;
        return hour >= MarketingStartHour && hour < MarketingEndHour;
    }
}
