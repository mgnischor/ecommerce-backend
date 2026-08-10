namespace ECommerce.Domain.Policies;

/// <summary>
/// Defines business rules for customer address validation
/// </summary>
public static class AddressPolicy
{
    private const int MinStreetLength = 3;
    private const int MaxStreetLength = 200;
    private const int MinCityLength = 2;
    private const int MaxCityLength = 100;
    private const int MaxCountryCodeLength = 3;
    private const int PostalCodeMinLength = 4;
    private const int PostalCodeMaxLength = 12;

    /// <summary>
    /// Validates a complete address
    /// </summary>
    public static bool IsValidAddress(
        string? street,
        string? city,
        string? postalCode,
        string? country
    )
    {
        if (string.IsNullOrWhiteSpace(street) || street.Trim().Length < MinStreetLength)
            return false;

        if (street.Trim().Length > MaxStreetLength)
            return false;

        if (string.IsNullOrWhiteSpace(city) || city.Trim().Length < MinCityLength)
            return false;

        if (city.Trim().Length > MaxCityLength)
            return false;

        if (!IsValidPostalCode(postalCode))
            return false;

        if (string.IsNullOrWhiteSpace(country) || country.Trim().Length != MaxCountryCodeLength)
            return false;

        return true;
    }

    /// <summary>
    /// Validates a postal code format
    /// </summary>
    public static bool IsValidPostalCode(string? postalCode)
    {
        if (string.IsNullOrWhiteSpace(postalCode))
            return false;

        var trimmed = postalCode.Trim();

        // Postal codes may contain digits, letters, spaces, and dashes
        if (trimmed.Length < PostalCodeMinLength || trimmed.Length > PostalCodeMaxLength)
            return false;

        return trimmed.All(c => char.IsLetterOrDigit(c) || c == '-' || c == ' ');
    }

    /// <summary>
    /// Validates a state code format
    /// </summary>
    public static bool IsValidStateCode(string? state)
    {
        if (string.IsNullOrWhiteSpace(state))
            return true; // State is optional for some countries

        return state.Trim().Length is >= 2 and <= 20;
    }

    /// <summary>
    /// Checks if an address is classified as residential
    /// </summary>
    public static bool IsResidentialAddress(bool isBusinessAddress, string? companyName)
    {
        if (isBusinessAddress)
            return false;

        // Addresses with a company name are treated as business addresses
        return string.IsNullOrWhiteSpace(companyName);
    }

    /// <summary>
    /// Checks if an address can be used as a shipping address
    /// </summary>
    public static bool CanUseAsShippingAddress(bool isVerified, bool isDefault)
    {
        // Default addresses must always be valid, verified addresses are preferred
        if (isDefault)
            return true;

        return isVerified;
    }

    /// <summary>
    /// Checks if all required fields for a shipping address are present
    /// </summary>
    public static bool IsAddressComplete(
        string? street,
        string? city,
        string? postalCode,
        string? country,
        string? recipientName
    )
    {
        return !string.IsNullOrWhiteSpace(recipientName)
            && IsValidAddress(street, city, postalCode, country);
    }
}
