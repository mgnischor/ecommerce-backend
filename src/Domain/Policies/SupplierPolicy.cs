using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Policies;

/// <summary>
/// Defines business rules for supplier management
/// </summary>
public static class SupplierPolicy
{
    private const int MinCompanyNameLength = 2;
    private const int MaxCompanyNameLength = 150;
    private const decimal MinimumOnTimeDeliveryRate = 0.90m;
    private const int MinimumOrdersForRating = 5;

    /// <summary>
    /// Validates supplier registration information
    /// </summary>
    public static bool IsValidSupplier(string? companyName, string? contactEmail, string? taxId)
    {
        if (string.IsNullOrWhiteSpace(companyName))
            return false;

        if (companyName.Trim().Length is < MinCompanyNameLength or > MaxCompanyNameLength)
            return false;

        if (string.IsNullOrWhiteSpace(contactEmail))
            return false;

        try
        {
            var address = new System.Net.Mail.MailAddress(contactEmail);
            if (address.Address != contactEmail)
                return false;
        }
        catch
        {
            return false;
        }

        // Tax ID is optional but when provided must be reasonably sized
        if (!string.IsNullOrWhiteSpace(taxId) && taxId.Trim().Length > 30)
            return false;

        return true;
    }

    /// <summary>
    /// Checks if a purchase order can be created for a supplier
    /// </summary>
    public static bool CanCreatePurchaseOrder(VendorStatus status, bool isVerified)
    {
        if (!isVerified)
            return false;

        return status == VendorStatus.Active;
    }

    /// <summary>
    /// Checks if a supplier is eligible for automatic replenishment
    /// </summary>
    public static bool IsEligibleForAutoReplenishment(
        decimal onTimeDeliveryRate,
        int totalOrders
    )
    {
        if (totalOrders < MinimumOrdersForRating)
            return false;

        return onTimeDeliveryRate >= MinimumOnTimeDeliveryRate;
    }

    /// <summary>
    /// Validates supplier payment terms
    /// </summary>
    public static bool IsValidPaymentTerms(string? paymentTerms)
    {
        if (string.IsNullOrWhiteSpace(paymentTerms))
            return true; // Payment terms are optional

        // Common terms: NET 30, NET 60, etc.
        var normalized = paymentTerms.Trim().ToUpperInvariant();
        if (!normalized.StartsWith("NET", StringComparison.Ordinal))
            return false;

        var daysPart = normalized[3..].Trim();
        return int.TryParse(daysPart, out var days) && days is >= 1 and <= 365;
    }

    /// <summary>
    /// Checks if a supplier should be suspended due to complaints
    /// </summary>
    public static bool CanSuspendSupplier(decimal complaintRate, int daysSinceLastComplaint)
    {
        // Suspend if more than 20% of orders generate complaints
        if (complaintRate > 0.20m)
            return true;

        // Suspension must wait for a cooling-off period after the last complaint
        return complaintRate > 0.10m && daysSinceLastComplaint >= 7;
    }
}
