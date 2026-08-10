namespace ECommerce.Domain.Policies;

/// <summary>
/// Defines business rules for gift card validation and redemption
/// </summary>
public static class GiftCardPolicy
{
    private const int MinCardNumberLength = 13;
    private const int MaxCardNumberLength = 19;
    private const decimal MinimumBalance = 0.01m;
    private const decimal MaximumBalance = 5000m;
    private const int MaxRedeemPercentPerTransaction = 100;
    private const int GiftCardValidityMonths = 24;

    /// <summary>
    /// Validates if a gift card number format is acceptable
    /// </summary>
    public static bool IsValidCardNumber(string? cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
            return false;

        if (cardNumber.Length < MinCardNumberLength || cardNumber.Length > MaxCardNumberLength)
            return false;

        // Card number should contain only digits and optional dashes
        var digitsOnly = new string(cardNumber.Where(char.IsDigit).ToArray());
        var hasValidFormat = cardNumber.All(c => char.IsDigit(c) || c == '-');

        return hasValidFormat && digitsOnly.Length >= MinCardNumberLength;
    }

    /// <summary>
    /// Validates if a gift card balance is within acceptable limits
    /// </summary>
    public static bool IsValidBalance(decimal balance)
    {
        return balance >= MinimumBalance && balance <= MaximumBalance;
    }

    /// <summary>
    /// Checks if a gift card is active and can be used
    /// </summary>
    public static bool IsCardActive(bool isActive, bool isRevoked)
    {
        return isActive && !isRevoked;
    }

    /// <summary>
    /// Checks if a gift card is within its validity period
    /// </summary>
    public static bool IsWithinValidityPeriod(DateTime? issuedAt, DateTime? expiresAt)
    {
        if (!issuedAt.HasValue)
            return false;

        var effectiveExpiration = expiresAt ?? issuedAt.Value.AddMonths(GiftCardValidityMonths);
        var now = DateTime.UtcNow;

        return now >= issuedAt.Value && now <= effectiveExpiration;
    }

    /// <summary>
    /// Checks if a redeem amount can be covered by the available balance
    /// </summary>
    public static bool CanRedeemAmount(decimal balance, decimal redeemAmount)
    {
        if (redeemAmount <= 0)
            return false;

        if (!IsValidBalance(balance))
            return false;

        return redeemAmount <= balance;
    }

    /// <summary>
    /// Checks if a gift card has expired
    /// </summary>
    public static bool IsCardExpired(DateTime? expiresAt)
    {
        if (!expiresAt.HasValue)
            return false;

        return DateTime.UtcNow > expiresAt.Value;
    }

    /// <summary>
    /// Calculates the remaining balance after a redemption
    /// </summary>
    public static decimal CalculateRemainingBalance(decimal currentBalance, decimal redeemedAmount)
    {
        return Math.Max(currentBalance - redeemedAmount, 0);
    }

    /// <summary>
    /// Checks if a gift card can be reloaded
    /// </summary>
    public static bool CanReloadCard(bool allowsReload, decimal currentBalance)
    {
        if (!allowsReload)
            return false;

        return currentBalance < MaximumBalance;
    }
}
