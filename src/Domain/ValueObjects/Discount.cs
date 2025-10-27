namespace ECommerce.Domain.ValueObjects;

/// <summary>
/// Value object representing a discount
/// </summary>
public sealed class Discount : IEquatable<Discount>
{
    private const string TypePercentage = "percentage";
    private const string TypeFixed = "fixed";
    private const string TypeNone = "none";

    public string Type { get; }
    public decimal Value { get; }
    public string? Description { get; }

    private Discount(string type, decimal value, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Discount type is required", nameof(type));

        if (value < 0)
            throw new ArgumentException("Discount value cannot be negative", nameof(value));

        Type = type.ToLowerInvariant();
        Value = value;
        Description = description;

        ValidateDiscountValue();
    }

    /// <summary>
    /// Creates a percentage discount
    /// </summary>
    public static Discount Percentage(decimal percentage, string? description = null)
    {
        if (percentage < 0 || percentage > 100)
            throw new ArgumentException("Percentage must be between 0 and 100", nameof(percentage));

        return new Discount(TypePercentage, percentage, description);
    }

    /// <summary>
    /// Creates a fixed amount discount
    /// </summary>
    public static Discount Fixed(decimal amount, string? description = null)
    {
        if (amount < 0)
            throw new ArgumentException("Fixed discount amount cannot be negative", nameof(amount));

        return new Discount(TypeFixed, amount, description);
    }

    /// <summary>
    /// Creates a no discount instance
    /// </summary>
    public static Discount None()
    {
        return new Discount(TypeNone, 0, "No discount");
    }

    /// <summary>
    /// Calculates the discount amount based on the original price
    /// </summary>
    public decimal Calculate(decimal originalPrice)
    {
        if (originalPrice < 0)
            throw new ArgumentException("Original price cannot be negative", nameof(originalPrice));

        return Type switch
        {
            TypePercentage => Math.Round((originalPrice * Value) / 100, 2),
            TypeFixed => Math.Min(Value, originalPrice), // Cannot discount more than price
            TypeNone => 0,
            _ => 0,
        };
    }

    /// <summary>
    /// Applies the discount to a price and returns the final price
    /// </summary>
    public decimal ApplyTo(decimal originalPrice)
    {
        var discountAmount = Calculate(originalPrice);
        return Math.Max(originalPrice - discountAmount, 0);
    }

    /// <summary>
    /// Checks if this is a valid discount
    /// </summary>
    public bool IsValid()
    {
        return Type switch
        {
            TypePercentage => Value >= 0 && Value <= 100,
            TypeFixed => Value >= 0,
            TypeNone => Value == 0,
            _ => false,
        };
    }

    private void ValidateDiscountValue()
    {
        if (!IsValid())
            throw new ArgumentException($"Invalid discount: {Type} with value {Value}");
    }

    public bool Equals(Discount? other)
    {
        if (other is null)
            return false;
        return Type == other.Type && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is Discount other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Type, Value);
    }

    public override string ToString()
    {
        return Type switch
        {
            TypePercentage => $"{Value}% off",
            TypeFixed => $"${Value:F2} off",
            TypeNone => "No discount",
            _ => "Unknown discount",
        };
    }

    public static bool operator ==(Discount? left, Discount? right)
    {
        if (left is null)
            return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(Discount? left, Discount? right)
    {
        return !(left == right);
    }
}
