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
        finalPrice.Should().Be(100m);
    }

    [Test]
    public void CalculateProductFinalPrice_WithValidDiscount_ReturnsDiscountedPrice()
    {
        // Arrange
        var product = new ProductEntity { Price = 100m, DiscountPrice = 80m };

        // Act
        var finalPrice = DiscountCalculationService.CalculateProductFinalPrice(product);

        // Assert
        finalPrice.Should().Be(80m);
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
        savings.Should().Be(25m);
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
        savings.Should().Be(0m);
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
        subtotal.Should().Be(310m); // (80 * 2) + (50 * 3) = 160 + 150 = 310
        discount.Should().Be(40m); // (100 - 80) * 2 = 40
    }

    [Test]
    public void CalculateCartTotal_WithEmptyCart_ReturnsZero()
    {
        // Arrange
        var cartItems = new List<(ProductEntity, int)>();

        // Act
        var (subtotal, discount) = DiscountCalculationService.CalculateCartTotal(cartItems);

        // Assert
        subtotal.Should().Be(0m);
        discount.Should().Be(0m);
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
        discount.Should().BeGreaterThan(0m);
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
        discount.Should().BeGreaterThan(0m);
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
        discount.Should().BeGreaterThan(0m);
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
        discount.Should().Be(0m);
    }
}
