using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Policies;

/// <summary>
/// Defines security rules for authentication, authorization, and data access
/// </summary>
public static class SecurityPolicy
{
    private const int SessionTimeoutMinutes = 30;
    private const int RefreshTokenValidityDays = 7;
    private const int AccessTokenValidityMinutes = 15;
    private const int MaxConcurrentSessions = 5;
    private const int PasswordResetTokenValidityHours = 24;
    private const int MaxApiRequestsPerMinute = 100;

    /// <summary>
    /// Validates if a user has permission to perform an action
    /// </summary>
    public static bool HasPermission(UserAccessLevel userLevel, UserAccessLevel requiredLevel)
    {
        return userLevel >= requiredLevel;
    }

    /// <summary>
    /// Checks if a user can access administrative features
    /// </summary>
    public static bool CanAccessAdminFeatures(UserAccessLevel userLevel)
    {
        return userLevel switch
        {
            UserAccessLevel.Admin => true,
            UserAccessLevel.Manager => true,
            UserAccessLevel.Developer => true,
            _ => false,
        };
    }

    /// <summary>
    /// Checks if a user can manage other users
    /// </summary>
    public static bool CanManageUsers(UserAccessLevel userLevel, UserAccessLevel targetUserLevel)
    {
        if (userLevel != UserAccessLevel.Admin && userLevel != UserAccessLevel.Developer)
            return false;

        // Admin can manage all users except Developers
        // Developers can manage all users
        if (userLevel == UserAccessLevel.Admin)
            return targetUserLevel != UserAccessLevel.Developer;

        return true;
    }

    /// <summary>
    /// Validates if session has expired
    /// </summary>
    public static bool IsSessionExpired(DateTime lastActivityAt)
    {
        var inactivityDuration = DateTime.UtcNow - lastActivityAt;
        return inactivityDuration.TotalMinutes >= SessionTimeoutMinutes;
    }

    /// <summary>
    /// Validates if access token is still valid
    /// </summary>
    public static bool IsAccessTokenValid(DateTime issuedAt, DateTime? expiresAt)
    {
        if (expiresAt.HasValue)
            return DateTime.UtcNow < expiresAt.Value;

        // Fallback to default validity period if not specified
        var tokenAge = DateTime.UtcNow - issuedAt;
        return tokenAge.TotalMinutes < AccessTokenValidityMinutes;
    }

    /// <summary>
    /// Validates if refresh token is still valid
    /// </summary>
    public static bool IsRefreshTokenValid(DateTime issuedAt, bool isRevoked)
    {
        if (isRevoked)
            return false;

        var tokenAge = DateTime.UtcNow - issuedAt;
        return tokenAge.TotalDays < RefreshTokenValidityDays;
    }

    /// <summary>
    /// Checks if user has exceeded concurrent session limit
    /// </summary>
    public static bool HasExceededSessionLimit(int activeSessions)
    {
        return activeSessions >= MaxConcurrentSessions;
    }

    /// <summary>
    /// Validates password reset token
    /// </summary>
    public static bool IsPasswordResetTokenValid(DateTime issuedAt, bool isUsed)
    {
        if (isUsed)
            return false;

        var tokenAge = DateTime.UtcNow - issuedAt;
        return tokenAge.TotalHours < PasswordResetTokenValidityHours;
    }

    /// <summary>
    /// Checks if user can access sensitive data
    /// </summary>
    public static bool CanAccessSensitiveData(
        UserAccessLevel userLevel,
        Guid dataOwnerId,
        Guid requestingUserId
    )
    {
        // Admin and Developers can access all data
        if (userLevel == UserAccessLevel.Admin || userLevel == UserAccessLevel.Developer)
            return true;

        // Users can only access their own sensitive data
        return dataOwnerId == requestingUserId;
    }

    /// <summary>
    /// Validates if user can modify order
    /// </summary>
    public static bool CanModifyOrder(
        UserAccessLevel userLevel,
        Guid orderOwnerId,
        Guid requestingUserId,
        OrderStatus orderStatus
    )
    {
        // Admin and Managers can modify any order
        if (CanAccessAdminFeatures(userLevel))
            return true;

        // Customers can only modify their own pending orders
        if (orderOwnerId != requestingUserId)
            return false;

        return orderStatus == OrderStatus.Pending;
    }

    /// <summary>
    /// Checks if API rate limit has been exceeded
    /// </summary>
    public static bool HasExceededRateLimit(int requestCount, int timeWindowMinutes)
    {
        var maxRequestsForWindow = MaxApiRequestsPerMinute * timeWindowMinutes;
        return requestCount >= maxRequestsForWindow;
    }

    /// <summary>
    /// Validates if user can delete resource
    /// </summary>
    public static bool CanDeleteResource(
        UserAccessLevel userLevel,
        Guid resourceOwnerId,
        Guid requestingUserId
    )
    {
        // Admin and Developers can delete any resource
        if (userLevel == UserAccessLevel.Admin || userLevel == UserAccessLevel.Developer)
            return true;

        // Users can only delete their own resources
        return resourceOwnerId == requestingUserId;
    }

    /// <summary>
    /// Checks if two-factor authentication is required
    /// </summary>
    public static bool RequiresTwoFactorAuth(
        UserAccessLevel userLevel,
        bool isSensitiveOperation,
        decimal? transactionAmount
    )
    {
        // Always require for admin operations
        if (CanAccessAdminFeatures(userLevel))
            return true;

        // Require for sensitive operations
        if (isSensitiveOperation)
            return true;

        // Require for high-value transactions
        if (transactionAmount.HasValue && transactionAmount.Value >= 1000m)
            return true;

        return false;
    }

    /// <summary>
    /// Validates if user can export data
    /// </summary>
    public static bool CanExportData(UserAccessLevel userLevel)
    {
        return userLevel switch
        {
            UserAccessLevel.Admin => true,
            UserAccessLevel.Developer => true,
            UserAccessLevel.Manager => true,
            _ => false,
        };
    }

    /// <summary>
    /// Checks if IP address should be blocked
    /// </summary>
    public static bool ShouldBlockIPAddress(int failedAttempts, int suspiciousActivityCount)
    {
        // Block after too many failed attempts or suspicious activities
        return failedAttempts >= 10 || suspiciousActivityCount >= 5;
    }

    /// <summary>
    /// Validates if guest checkout is allowed
    /// </summary>
    public static bool AllowsGuestCheckout(bool isGuestCheckoutEnabled, decimal orderTotal)
    {
        if (!isGuestCheckoutEnabled)
            return false;

        // May restrict guest checkout for high-value orders
        return orderTotal < 5000m;
    }
}
