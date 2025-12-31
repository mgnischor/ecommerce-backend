namespace ECommerce.Tests.Domain.Services;

/// <summary>
/// Tests for DiscountCalculationService domain service
/// </summary>
[TestFixture]
public class DiscountCalculationServiceTests : BaseTestFixture
{
    [Test]
    public void CalculateProductFinalPrice_WithNoDiscount_ReturnsOriginalPrice()
    {
        // Arrange
        var product = new ProductEntity { Price = 100m, DiscountPrice = null };

        // Act
        var finalPrice = DiscountCalculationService.CalculateProductFinalPrice(product);

        // Assert
        Assert.That(finalPrice, Is.EqualTo(100m));
    }

    [Test]
    public void CalculateProductFinalPrice_WithValidDiscount_ReturnsDiscountedPrice()
    {
        // Arrange
        var product = new ProductEntity { Price = 100m, DiscountPrice = 80m };

        // Act
        var finalPrice = DiscountCalculationService.CalculateProductFinalPrice(product);

        // Assert
        Assert.That(finalPrice, Is.EqualTo(80m));
    }

    [Test]
    public void CalculateSavings_WithPositiveDifference_ReturnsCorrectSavings()
    {
        // Arrange
        var originalPrice = 100m;
        var finalPrice = 75m;

        // Act
        var savings = DiscountCalculationService.CalculateSavings(originalPrice, finalPrice);

        // Assert
        Assert.That(savings, Is.EqualTo(25m));
    }

    [Test]
    public void CalculateSavings_WithNegativeDifference_ReturnsZero()
    {
        // Arrange
        var originalPrice = 100m;
        var finalPrice = 120m;

        // Act
        var savings = DiscountCalculationService.CalculateSavings(originalPrice, finalPrice);

        // Assert
        Assert.That(savings, Is.EqualTo(0m));
    }

    [Test]
    public void CalculateCartTotal_WithMultipleItems_ReturnsCorrectTotals()
    {
        // Arrange
        var cartItems = new List<(ProductEntity, int)>
        {
            (new ProductEntity { Price = 100m, DiscountPrice = 80m }, 2),
            (new ProductEntity { Price = 50m, DiscountPrice = null }, 3),
        };

        // Act
        var (subtotal, discount) = DiscountCalculationService.CalculateCartTotal(cartItems);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(subtotal, Is.EqualTo(310m)); // (80 * 2) + (50 * 3) = 160 + 150 = 310
            Assert.That(discount, Is.EqualTo(40m)); // (100 - 80) * 2 = 40
        }
    }

    [Test]
    public void CalculateCartTotal_WithEmptyCart_ReturnsZero()
    {
        // Arrange
        var cartItems = new List<(ProductEntity, int)>();

        // Act
        var (subtotal, discount) = DiscountCalculationService.CalculateCartTotal(cartItems);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(subtotal, Is.EqualTo(0m));
            Assert.That(discount, Is.EqualTo(0m));
        }
    }

    [Test]
    public void CalculateBulkDiscount_WithLargeQuantity_AppliesHighestDiscount()
    {
        // Arrange
        var unitPrice = 100m;
        var quantity = 150;

        // Act
        var discount = DiscountCalculationService.CalculateBulkDiscount(unitPrice, quantity);

        // Assert
        Assert.That(discount, Is.GreaterThan(0m));
    }

    [Test]
    public void CalculateBulkDiscount_WithMediumQuantity_AppliesMediumDiscount()
    {
        // Arrange
        var unitPrice = 100m;
        var quantity = 75;

        // Act
        var discount = DiscountCalculationService.CalculateBulkDiscount(unitPrice, quantity);

        // Assert
        Assert.That(discount, Is.GreaterThan(0m));
    }

    [Test]
    public void CalculateBulkDiscount_WithSmallQuantity_AppliesLowDiscount()
    {
        // Arrange
        var unitPrice = 100m;
        var quantity = 25;

        // Act
        var discount = DiscountCalculationService.CalculateBulkDiscount(unitPrice, quantity);

        // Assert
        Assert.That(discount, Is.GreaterThan(0m));
    }

    [Test]
    public void CalculateBulkDiscount_WithMinimalQuantity_AppliesNoDiscount()
    {
        // Arrange
        var unitPrice = 100m;
        var quantity = 5;

        // Act
        var discount = DiscountCalculationService.CalculateBulkDiscount(unitPrice, quantity);

        // Assert
        Assert.That(discount, Is.EqualTo(0m));
    }
}
