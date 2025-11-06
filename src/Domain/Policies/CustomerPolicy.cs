using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Policies;

/// <summary>
/// Defines business rules for customer account management and loyalty
/// </summary>
public static class CustomerPolicy
{
    private const int MinimumPasswordLength = 8;
    private const int MaximumPasswordLength = 128;
    private const int MinimumUsernameLength = 3;
    private const int MaximumUsernameLength = 50;
    private const int AccountInactivityDays = 365;
    private const int MaximumAddressesPerCustomer = 10;
    private const int MaximumFailedLoginAttempts = 5;
    private const int AccountLockoutMinutes = 30;

    /// <summary>
    /// Validates username format and length
    /// </summary>
    public static bool IsValidUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return false;

        if (username.Length < MinimumUsernameLength || username.Length > MaximumUsernameLength)
            return false;

        // Username should start with a letter and contain only alphanumeric characters, dots, or underscores
        return char.IsLetter(username[0])
            && username.All(c => char.IsLetterOrDigit(c) || c == '.' || c == '_');
    }

    /// <summary>
    /// Validates email format
    /// </summary>
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates password strength
    /// </summary>
    public static bool IsValidPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        if (password.Length < MinimumPasswordLength || password.Length > MaximumPasswordLength)
            return false;

        // Password must contain at least one uppercase, one lowercase, one digit, and one special character
        var hasUpperCase = password.Any(char.IsUpper);
        var hasLowerCase = password.Any(char.IsLower);
        var hasDigit = password.Any(char.IsDigit);
        var hasSpecialChar = password.Any(c => !char.IsLetterOrDigit(c));

        return hasUpperCase && hasLowerCase && hasDigit && hasSpecialChar;
    }

    /// <summary>
    /// Checks if a customer account is active
    /// </summary>
    public static bool IsAccountActive(bool isActive, bool isDeleted, DateTime? lastLoginAt)
    {
        if (!isActive || isDeleted)
            return false;

        // Consider account inactive if not logged in for specified days
        if (lastLoginAt.HasValue)
        {
            var daysSinceLogin = (DateTime.UtcNow - lastLoginAt.Value).TotalDays;
            if (daysSinceLogin > AccountInactivityDays)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Validates if customer can add more addresses
    /// </summary>
    public static bool CanAddMoreAddresses(int currentAddressCount)
    {
        return currentAddressCount < MaximumAddressesPerCustomer;
    }

    /// <summary>
    /// Checks if account should be locked due to failed login attempts
    /// </summary>
    public static bool ShouldLockAccount(int failedLoginAttempts)
    {
        return failedLoginAttempts >= MaximumFailedLoginAttempts;
    }

    /// <summary>
    /// Checks if account lockout period has expired
    /// </summary>
    public static bool IsAccountLockoutExpired(DateTime? lockedOutAt)
    {
        if (!lockedOutAt.HasValue)
            return true;

        var lockoutDuration = DateTime.UtcNow - lockedOutAt.Value;
        return lockoutDuration.TotalMinutes >= AccountLockoutMinutes;
    }

    /// <summary>
    /// Determines customer tier based on total spending
    /// </summary>
    public static string GetCustomerTier(decimal totalSpending)
    {
        return totalSpending switch
        {
            >= 10000m => "Platinum",
            >= 5000m => "Gold",
            >= 1000m => "Silver",
            _ => "Bronze",
        };
    }

    /// <summary>
    /// Calculates loyalty discount percentage based on customer tier
    /// </summary>
    public static decimal GetLoyaltyDiscountPercentage(string tier)
    {
        return tier switch
        {
            "Platinum" => 15m,
            "Gold" => 10m,
            "Silver" => 5m,
            "Bronze" => 2m,
            _ => 0m,
        };
    }

    /// <summary>
    /// Checks if customer is eligible for free shipping
    /// </summary>
    public static bool IsEligibleForFreeShipping(
        decimal totalSpending,
        decimal freeShippingThreshold
    )
    {
        return totalSpending >= freeShippingThreshold;
    }

    /// <summary>
    /// Validates if customer can delete their account
    /// </summary>
    public static bool CanDeleteAccount(bool hasPendingOrders, decimal accountBalance)
    {
        // Cannot delete if there are pending orders or outstanding balance
        return !hasPendingOrders && accountBalance == 0;
    }

    /// <summary>
    /// Checks if customer needs to verify their email
    /// </summary>
    public static bool RequiresEmailVerification(bool isEmailVerified, DateTime createdAt)
    {
        if (isEmailVerified)
            return false;

        // Require verification if account is older than 7 days
        var accountAge = (DateTime.UtcNow - createdAt).TotalDays;
        return accountAge > 7;
    }

    /// <summary>
    /// Validates phone number format (basic validation)
    /// </summary>
    public static bool IsValidPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return true; // Phone number is optional

        // Remove common separators
        var digitsOnly = new string(phoneNumber.Where(char.IsDigit).ToArray());

        // Phone number should have between 10 and 15 digits
        return digitsOnly.Length >= 10 && digitsOnly.Length <= 15;
    }

    /// <summary>
    /// Determines if customer qualifies as VIP
    /// </summary>
    public static bool IsVIPCustomer(decimal totalSpending, int totalOrders)
    {
        return totalSpending >= 10000m || totalOrders >= 50;
    }
}
