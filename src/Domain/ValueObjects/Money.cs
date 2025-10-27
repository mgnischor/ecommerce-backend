namespace ECommerce.Domain.ValueObjects;

/// <summary>
/// Value object representing money with currency
/// </summary>
public sealed class Money : IEquatable<Money>
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency code is required", nameof(currency));

        Amount = Math.Round(amount, 2);
        Currency = currency.ToUpperInvariant();
    }

    /// <summary>
    /// Creates a new Money instance
    /// </summary>
    public static Money Create(decimal amount, string currency = "USD")
    {
        return new Money(amount, currency);
    }

    /// <summary>
    /// Creates zero money value
    /// </summary>
    public static Money Zero(string currency = "USD")
    {
        return new Money(0, currency);
    }

    /// <summary>
    /// Adds two money values (must be same currency)
    /// </summary>
    public Money Add(Money other)
    {
        ValidateSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    /// <summary>
    /// Subtracts money value (must be same currency)
    /// </summary>
    public Money Subtract(Money other)
    {
        ValidateSameCurrency(other);
        var result = Amount - other.Amount;

        if (result < 0)
            throw new InvalidOperationException("Result cannot be negative");

        return new Money(result, Currency);
    }

    /// <summary>
    /// Multiplies money by a factor
    /// </summary>
    public Money Multiply(decimal factor)
    {
        if (factor < 0)
            throw new ArgumentException("Factor cannot be negative", nameof(factor));

        return new Money(Amount * factor, Currency);
    }

    /// <summary>
    /// Applies a percentage discount
    /// </summary>
    public Money ApplyDiscount(decimal percentageDiscount)
    {
        if (percentageDiscount < 0 || percentageDiscount > 100)
            throw new ArgumentException(
                "Discount must be between 0 and 100",
                nameof(percentageDiscount)
            );

        var discountAmount = Amount * (percentageDiscount / 100);
        return new Money(Amount - discountAmount, Currency);
    }

    /// <summary>
    /// Checks if this amount is greater than another
    /// </summary>
    public bool IsGreaterThan(Money other)
    {
        ValidateSameCurrency(other);
        return Amount > other.Amount;
    }

    /// <summary>
    /// Checks if this amount is less than another
    /// </summary>
    public bool IsLessThan(Money other)
    {
        ValidateSameCurrency(other);
        return Amount < other.Amount;
    }

    private void ValidateSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException(
                $"Cannot perform operation on different currencies: {Currency} and {other.Currency}"
            );
    }

    public bool Equals(Money? other)
    {
        if (other is null)
            return false;
        return Amount == other.Amount && Currency == other.Currency;
    }

    public override bool Equals(object? obj)
    {
        return obj is Money other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Amount, Currency);
    }

    public override string ToString()
    {
        return $"{Amount:F2} {Currency}";
    }

    public static bool operator ==(Money? left, Money? right)
    {
        if (left is null)
            return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(Money? left, Money? right)
    {
        return !(left == right);
    }
}
