using ECommerce.Domain.Policies;

namespace ECommerce.Tests.Domain.Policies;

/// <summary>
/// Tests for PricingPolicy
/// </summary>
[TestFixture]
public class PricingPolicyTests : BaseTestFixture
{
    [Test]
    public void IsValidPrice_WithValidPrice_ReturnsTrue()
    {
        // Act
        var isValid = PricingPolicy.IsValidPrice(100m);

        // Assert
        Assert.That(isValid, Is.True);
    }

    [Test]
    public void IsValidPrice_WithMinimumPrice_ReturnsTrue()
    {
        // Act
        var isValid = PricingPolicy.IsValidPrice(0.01m);

        // Assert
        Assert.That(isValid, Is.True);
    }

    [Test]
    public void IsValidPrice_WithMaximumPrice_ReturnsTrue()
    {
        // Act
        var isValid = PricingPolicy.IsValidPrice(999999.99m);

        // Assert
        Assert.That(isValid, Is.True);
    }

    [Test]
    public void IsValidPrice_WithZeroPrice_ReturnsFalse()
    {
        // Act
        var isValid = PricingPolicy.IsValidPrice(0m);

        // Assert
        Assert.That(isValid, Is.False);
    }

    [Test]
    public void IsValidPrice_WithNegativePrice_ReturnsFalse()
    {
        // Act
        var isValid = PricingPolicy.IsValidPrice(-10m);

        // Assert
        Assert.That(isValid, Is.False);
    }

    [Test]
    public void IsValidPrice_ExceedsMaximum_ReturnsFalse()
    {
        // Act
        var isValid = PricingPolicy.IsValidPrice(1000000m);

        // Assert
        Assert.That(isValid, Is.False);
    }

    [Test]
    public void IsValidDiscountPrice_WithValidDiscount_ReturnsTrue()
    {
        // Act
        var isValid = PricingPolicy.IsValidDiscountPrice(100m, 80m);

        // Assert
        Assert.That(isValid, Is.True);
    }

    [Test]
    public void IsValidDiscountPrice_EqualToOriginal_ReturnsFalse()
    {
        // Act
        var isValid = PricingPolicy.IsValidDiscountPrice(100m, 100m);

        // Assert
        Assert.That(isValid, Is.False);
    }

    [Test]
    public void IsValidDiscountPrice_HigherThanOriginal_ReturnsFalse()
    {
        // Act
        var isValid = PricingPolicy.IsValidDiscountPrice(100m, 120m);

        // Assert
        Assert.That(isValid, Is.False);
    }

    [Test]
    public void IsValidDiscountPrice_BelowMinimum_ReturnsFalse()
    {
        // Act
        var isValid = PricingPolicy.IsValidDiscountPrice(100m, 0m);

        // Assert
        Assert.That(isValid, Is.False);
    }

    [Test]
    public void IsValidDiscountPrice_ExceedsMaximumDiscountPercentage_ReturnsFalse()
    {
        // Act
        var isValid = PricingPolicy.IsValidDiscountPrice(100m, 0.001m);

        // Assert
        Assert.That(isValid, Is.False);
    }

    [Test]
    public void CalculateDiscountPercentage_WithValidPrices_ReturnsCorrectPercentage()
    {
        // Act
        var percentage = PricingPolicy.CalculateDiscountPercentage(100m, 80m);

        // Assert
        Assert.That(percentage, Is.EqualTo(20m));
    }

    [Test]
    public void CalculateDiscountPercentage_WithZeroOriginalPrice_ReturnsZero()
    {
        // Act
        var percentage = PricingPolicy.CalculateDiscountPercentage(0m, 0m);

        // Assert
        Assert.That(percentage, Is.EqualTo(0m));
    }

    [Test]
    public void CalculateDiscountPercentage_WithHalfPrice_ReturnsFiftyPercent()
    {
        // Act
        var percentage = PricingPolicy.CalculateDiscountPercentage(100m, 50m);

        // Assert
        Assert.That(percentage, Is.EqualTo(50m));
    }

    [Test]
    public void CalculateDiscountPercentage_RoundsToTwoDecimals()
    {
        // Act
        var percentage = PricingPolicy.CalculateDiscountPercentage(100m, 66.67m);

        // Assert
        Assert.That(percentage, Is.EqualTo(33.33m));
    }

    [Test]
    public void ApplyDiscountPercentage_WithValidPercentage_ReturnsDiscountedPrice()
    {
        // Act
        var discountedPrice = PricingPolicy.ApplyDiscountPercentage(100m, 20m);

        // Assert
        Assert.That(discountedPrice, Is.EqualTo(80m));
    }

    [Test]
    public void ApplyDiscountPercentage_WithZeroPercentage_ReturnsOriginalPrice()
    {
        // Act
        var discountedPrice = PricingPolicy.ApplyDiscountPercentage(100m, 0m);

        // Assert
        Assert.That(discountedPrice, Is.EqualTo(100m));
    }

    [Test]
    public void ApplyDiscountPercentage_WithMaximumPercentage_ReturnsMinimalPrice()
    {
        // Act
        var discountedPrice = PricingPolicy.ApplyDiscountPercentage(100m, 99m);

        // Assert
        Assert.That(discountedPrice, Is.EqualTo(1m));
    }

    [Test]
    public void ApplyDiscountPercentage_WithNegativePercentage_ThrowsException()
    {
        // Act
        Assert.Throws<ArgumentException>(() => PricingPolicy.ApplyDiscountPercentage(100m, -10m));
    }

    [Test]
    public void ApplyDiscountPercentage_WithExcessivePercentage_ThrowsException()
    {
        // Act
        Assert.Throws<ArgumentException>(() => PricingPolicy.ApplyDiscountPercentage(100m, 100m));
    }

    [Test]
    public void ApplyDiscountPercentage_RoundsToTwoDecimals()
    {
        // Act
        var discountedPrice = PricingPolicy.ApplyDiscountPercentage(99.99m, 33.33m);

        // Assert
        Assert.That(discountedPrice, Is.EqualTo(66.66m));
    }

    [Test]
    public void GetEffectivePrice_WithNoDiscount_ReturnsOriginalPrice()
    {
        // Act
        var effectivePrice = PricingPolicy.GetEffectivePrice(100m, null);

        // Assert
        Assert.That(effectivePrice, Is.EqualTo(100m));
    }

    [Test]
    public void GetEffectivePrice_WithValidDiscount_ReturnsDiscountPrice()
    {
        // Act
        var effectivePrice = PricingPolicy.GetEffectivePrice(100m, 80m);

        // Assert
        Assert.That(effectivePrice, Is.EqualTo(80m));
    }

    [Test]
    public void GetEffectivePrice_WithHigherDiscount_ReturnsOriginalPrice()
    {
        // Act
        var effectivePrice = PricingPolicy.GetEffectivePrice(100m, 120m);

        // Assert
        Assert.That(effectivePrice, Is.EqualTo(100m));
    }

    [Test]
    public void GetEffectivePrice_WithInvalidDiscount_ReturnsOriginalPrice()
    {
        // Act
        var effectivePrice = PricingPolicy.GetEffectivePrice(100m, 0m);

        // Assert
        Assert.That(effectivePrice, Is.EqualTo(100m));
    }

    [Test]
    public void IsValidBulkPrice_WithExactMatch_ReturnsTrue()
    {
        // Act
        var isValid = PricingPolicy.IsValidBulkPrice(10m, 5, 50m);

        // Assert
        Assert.That(isValid, Is.True);
    }

    [Test]
    public void IsValidBulkPrice_WithRoundingTolerance_ReturnsTrue()
    {
        // Act
        var isValid = PricingPolicy.IsValidBulkPrice(10m, 5, 50.005m);

        // Assert
        Assert.That(isValid, Is.True);
    }

    [Test]
    public void IsValidBulkPrice_WithSignificantDifference_ReturnsFalse()
    {
        // Act
        var isValid = PricingPolicy.IsValidBulkPrice(10m, 5, 55m);

        // Assert
        Assert.That(isValid, Is.False);
    }

    [Test]
    public void IsValidBulkPrice_WithDecimalQuantities_CalculatesCorrectly()
    {
        // Act
        var isValid = PricingPolicy.IsValidBulkPrice(9.99m, 10, 99.90m);

        // Assert
        Assert.That(isValid, Is.True);
    }
}
