namespace ECommerce.Domain.Policies;

/// <summary>
/// Defines business rules for tax calculation and tax compliance
/// </summary>
public static class TaxPolicy
{
    private const decimal MinimumTaxRate = 0m;
    private const decimal MaximumTaxRate = 100m;
    private const decimal DefaultTaxRate = 0m;

    /// <summary>
    /// Validates if a tax rate percentage is within acceptable limits
    /// </summary>
    public static bool IsValidTaxRate(decimal taxRate)
    {
        return taxRate >= MinimumTaxRate && taxRate <= MaximumTaxRate;
    }

    /// <summary>
    /// Calculates the tax amount for a given amount and rate
    /// </summary>
    public static decimal CalculateTaxAmount(decimal amount, decimal taxRate)
    {
        if (amount < 0)
            return 0m;

        if (!IsValidTaxRate(taxRate))
            return 0m;

        return Math.Round(amount * (taxRate / 100), 2);
    }

    /// <summary>
    /// Checks if a product is exempt from tax
    /// </summary>
    public static bool IsTaxExemptProduct(bool isTaxExempt, string? category)
    {
        if (isTaxExempt)
            return true;

        // Common tax-exempt categories
        var exemptCategories = new[]
        {
            "Books",
            "Groceries",
            "Medical",
            "Prescription",
            "Education",
        };

        return category != null && exemptCategories.Contains(category);
    }

    /// <summary>
    /// Checks if tax calculation is required for a transaction
    /// </summary>
    public static bool RequiresTaxCalculation(bool isDigitalProduct, string? shipToCountry)
    {
        // Digital products are taxed based on the customer's billing address
        if (isDigitalProduct)
            return true;

        // Physical products require tax unless shipped to a tax-free zone
        if (string.IsNullOrWhiteSpace(shipToCountry))
            return false;

        var taxFreeZones = new[] { "DE", "US-OR", "US-NH", "US-MT", "US-AK" };
        return !taxFreeZones.Contains(shipToCountry);
    }

    /// <summary>
    /// Calculates the tax applied to shipping costs
    /// </summary>
    public static decimal CalculateShippingTax(decimal shippingCost, decimal taxRate)
    {
        if (shippingCost <= 0)
            return 0m;

        if (!IsValidTaxRate(taxRate))
            return 0m;

        return Math.Round(shippingCost * (taxRate / 100), 2);
    }

    /// <summary>
    /// Validates if a tax code format is acceptable
    /// </summary>
    public static bool IsValidTaxCode(string? taxCode)
    {
        if (string.IsNullOrWhiteSpace(taxCode))
            return true; // Tax code is optional

        // Tax codes like "VAT-20" or "TX-8.25"
        return taxCode.Length <= 20 && taxCode.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '.');
    }

    /// <summary>
    /// Extracts the base amount from a gross (tax-inclusive) price
    /// </summary>
    public static decimal CalculateBaseFromGrossAmount(decimal grossAmount, decimal taxRate)
    {
        if (grossAmount <= 0 || !IsValidTaxRate(taxRate) || taxRate == DefaultTaxRate)
            return grossAmount;

        return Math.Round(grossAmount / (1 + (taxRate / 100)), 2);
    }
}
