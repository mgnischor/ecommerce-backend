namespace ECommerce.Tests.Domain.Services;

/// <summary>
/// Tests for ProductPricingService domain service
/// </summary>
[TestFixture]
public class ProductPricingServiceTests : BaseTestFixture
{
    [Test]
    public void ValidateProductPricing_WithValidPrice_ReturnsTrue()
    {
        // Arrange
        var product = new ProductEntity { Price = 99.99m, DiscountPrice = null };

        // Act
        var (isValid, errorMessage) = ProductPricingService.ValidateProductPricing(product);

        // Assert
        isValid.Should().BeTrue();
        errorMessage.Should().BeNull();
    }

    [Test]
    public void ValidateProductPricing_WithInvalidPrice_ReturnsFalse()
    {
        // Arrange
        var product = new ProductEntity { Price = -10m };

        // Act
        var (isValid, errorMessage) = ProductPricingService.ValidateProductPricing(product);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().NotBeNullOrEmpty();
    }

    [Test]
    public void ValidateProductPricing_WithValidDiscountPrice_ReturnsTrue()
    {
        // Arrange
        var product = new ProductEntity { Price = 100m, DiscountPrice = 80m };

        // Act
        var (isValid, errorMessage) = ProductPricingService.ValidateProductPricing(product);

        // Assert
        isValid.Should().BeTrue();
        errorMessage.Should().BeNull();
    }

    [Test]
    public void ValidateProductPricing_WithInvalidDiscountPrice_ReturnsFalse()
    {
        // Arrange
        var product = new ProductEntity { Price = 100m, DiscountPrice = 150m };

        // Act
        var (isValid, errorMessage) = ProductPricingService.ValidateProductPricing(product);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Be("Invalid discount price");
    }

    [Test]
    public void CalculateRetailPrice_WithValidInputs_ReturnsCorrectPrice()
    {
        // Arrange
        var cost = 50m;
        var margin = 40m; // 40% margin

        // Act
        var retailPrice = ProductPricingService.CalculateRetailPrice(cost, margin);

        // Assert
        retailPrice.Should().Be(83.33m);
    }

    [Test]
    public void CalculateRetailPrice_WithZeroCost_ThrowsException()
    {
        // Arrange
        var cost = 0m;
        var margin = 40m;

        // Act
        Action act = () => ProductPricingService.CalculateRetailPrice(cost, margin);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*Cost must be greater than zero*");
    }

    [Test]
    public void CalculateRetailPrice_WithInvalidMargin_ThrowsException()
    {
        // Arrange
        var cost = 50m;
        var margin = 150m;

        // Act
        Action act = () => ProductPricingService.CalculateRetailPrice(cost, margin);

        // Assert
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("*Margin percentage must be between 0 and 100*");
    }

    [Test]
    public void CalculateProfitMargin_WithValidInputs_ReturnsCorrectMargin()
    {
        // Arrange
        var cost = 60m;
        var sellingPrice = 100m;

        // Act
        var margin = ProductPricingService.CalculateProfitMargin(cost, sellingPrice);

        // Assert
        margin.Should().Be(40m);
    }

    [Test]
    public void CalculateProfitMargin_WithZeroSellingPrice_ReturnsZero()
    {
        // Arrange
        var cost = 60m;
        var sellingPrice = 0m;

        // Act
        var margin = ProductPricingService.CalculateProfitMargin(cost, sellingPrice);

        // Assert
        margin.Should().Be(0m);
    }

    [Test]
    public void SuggestDiscountPrice_WithLowStock_ReturnsMinimalDiscount()
    {
        // Arrange
        var currentPrice = 100m;
        var stockQuantity = 5;
        var minStockLevel = 10;
        var averageDailySales = 2;

        // Act
        var discountPrice = ProductPricingService.SuggestDiscountPrice(
            currentPrice,
            stockQuantity,
            minStockLevel,
            averageDailySales
        );

        // Assert
        discountPrice.Should().BeLessThan(currentPrice);
        discountPrice.Should().BeGreaterThan(currentPrice * 0.9m);
    }

    [Test]
    public void SuggestDiscountPrice_WithHighStock_ReturnsAggressiveDiscount()
    {
        // Arrange
        var currentPrice = 100m;
        var stockQuantity = 1000;
        var minStockLevel = 10;
        var averageDailySales = 10;

        // Act
        var discountPrice = ProductPricingService.SuggestDiscountPrice(
            currentPrice,
            stockQuantity,
            minStockLevel,
            averageDailySales
        );

        // Assert
        discountPrice.Should().BeLessThan(currentPrice * 0.8m);
    }

    [Test]
    public void CalculateDynamicPrice_WithHighDemand_IncreasesPrice()
    {
        // Arrange
        var basePrice = 100m;
        var stockQuantity = 100;
        var recentSales = 60;
        var isFeatured = false;

        // Act
        var dynamicPrice = ProductPricingService.CalculateDynamicPrice(
            basePrice,
            stockQuantity,
            recentSales,
            isFeatured
        );

        // Assert
        dynamicPrice.Should().BeGreaterThan(basePrice);
    }

    [Test]
    public void CalculateDynamicPrice_WithLowDemand_DecreasesPrice()
    {
        // Arrange
        var basePrice = 100m;
        var stockQuantity = 100;
        var recentSales = 5;
        var isFeatured = false;

        // Act
        var dynamicPrice = ProductPricingService.CalculateDynamicPrice(
            basePrice,
            stockQuantity,
            recentSales,
            isFeatured
        );

        // Assert
        dynamicPrice.Should().BeLessThan(basePrice);
    }

    [Test]
    public void CalculateDynamicPrice_WithFeaturedProduct_AddsPremium()
    {
        // Arrange
        var basePrice = 100m;
        var stockQuantity = 100;
        var recentSales = 20;
        var isFeatured = true;

        // Act
        var dynamicPrice = ProductPricingService.CalculateDynamicPrice(
            basePrice,
            stockQuantity,
            recentSales,
            isFeatured
        );

        // Assert
        dynamicPrice.Should().BeGreaterThanOrEqualTo(basePrice);
    }

    [Test]
    public void GetPriceCompetitiveness_WithNoCompetitors_ReturnsNoData()
    {
        // Arrange
        var ourPrice = 100m;
        var competitorPrices = new List<decimal>();

        // Act
        var competitiveness = ProductPricingService.GetPriceCompetitiveness(
            ourPrice,
            competitorPrices
        );

        // Assert
        competitiveness.Should().Be("No competition data");
    }

    [Test]
    public void GetPriceCompetitiveness_WithLowestPrice_ReturnsHighlyCompetitive()
    {
        // Arrange
        var ourPrice = 80m;
        var competitorPrices = new List<decimal> { 100m, 110m, 120m };

        // Act
        var competitiveness = ProductPricingService.GetPriceCompetitiveness(
            ourPrice,
            competitorPrices
        );

        // Assert
        competitiveness.Should().Be("Highly competitive");
    }

    [Test]
    public void GetPriceCompetitiveness_WithAveragePrice_ReturnsCompetitive()
    {
        // Arrange
        var ourPrice = 105m;
        var competitorPrices = new List<decimal> { 100m, 110m, 120m };

        // Act
        var competitiveness = ProductPricingService.GetPriceCompetitiveness(
            ourPrice,
            competitorPrices
        );

        // Assert
        competitiveness.Should().Be("Competitive");
    }

    [Test]
    public void GetPriceCompetitiveness_WithHighestPrice_ReturnsAboveMarket()
    {
        // Arrange
        var ourPrice = 150m;
        var competitorPrices = new List<decimal> { 100m, 110m, 120m };

        // Act
        var competitiveness = ProductPricingService.GetPriceCompetitiveness(
            ourPrice,
            competitorPrices
        );

        // Assert
        competitiveness.Should().Be("Above market");
    }
}
