namespace ECommerce.Domain.ValueObjects;

/// <summary>
/// Value object representing a product SKU (Stock Keeping Unit)
/// </summary>
public sealed class Sku : IEquatable<Sku>
{
    public string Value { get; }

    private const int MinLength = 3;
    private const int MaxLength = 50;

    private Sku(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("SKU cannot be empty", nameof(value));

        if (!IsValidSku(value))
            throw new ArgumentException($"Invalid SKU format: {value}", nameof(value));

        Value = value.ToUpperInvariant().Trim();
    }

    /// <summary>
    /// Creates a new SKU instance
    /// </summary>
    public static Sku Create(string value)
    {
        return new Sku(value);
    }

    /// <summary>
    /// Validates SKU format
    /// </summary>
    private static bool IsValidSku(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return false;

        var trimmed = sku.Trim();

        if (trimmed.Length < MinLength || trimmed.Length > MaxLength)
            return false;

        // SKU should contain only alphanumeric characters and hyphens
        return trimmed.All(c => char.IsLetterOrDigit(c) || c == '-');
    }

    /// <summary>
    /// Generates a SKU from category and product name
    /// </summary>
    public static Sku Generate(string category, string productName)
    {
        if (string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Category and product name are required");

        var categoryPrefix = new string(category.Take(3).ToArray()).ToUpperInvariant();
        var productPrefix = new string(
            productName.Where(char.IsLetterOrDigit).Take(6).ToArray()
        ).ToUpperInvariant();
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");

        var sku = $"{categoryPrefix}-{productPrefix}-{timestamp}";

        return new Sku(sku);
    }

    /// <summary>
    /// Gets the category prefix from the SKU
    /// </summary>
    public string GetCategoryPrefix()
    {
        var parts = Value.Split('-');
        return parts.Length > 0 ? parts[0] : string.Empty;
    }

    public bool Equals(Sku? other)
    {
        if (other is null)
            return false;
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is Sku other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }

    public static bool operator ==(Sku? left, Sku? right)
    {
        if (left is null)
            return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(Sku? left, Sku? right)
    {
        return !(left == right);
    }

    public static implicit operator string(Sku sku)
    {
        return sku.Value;
    }
}
