using ECommerce.Domain.Policies;

namespace ECommerce.Tests.Domain.Policies;

/// <summary>
/// Tests for StockManagementPolicy
/// </summary>
[TestFixture]
public class StockManagementPolicyTests : BaseTestFixture
{
    [Test]
    public void IsOutOfStock_WithZeroStock_ReturnsTrue()
    {
        // Act
        var isOutOfStock = StockManagementPolicy.IsOutOfStock(0);

        // Assert
        isOutOfStock.Should().BeTrue();
    }

    [Test]
    public void IsOutOfStock_WithNegativeStock_ReturnsTrue()
    {
        // Act
        var isOutOfStock = StockManagementPolicy.IsOutOfStock(-5);

        // Assert
        isOutOfStock.Should().BeTrue();
    }

    [Test]
    public void IsOutOfStock_WithPositiveStock_ReturnsFalse()
    {
        // Act
        var isOutOfStock = StockManagementPolicy.IsOutOfStock(10);

        // Assert
        isOutOfStock.Should().BeFalse();
    }

    [Test]
    public void IsLowStock_WithStockBelowThreshold_ReturnsTrue()
    {
        // Act
        var isLowStock = StockManagementPolicy.IsLowStock(5, 10);

        // Assert
        isLowStock.Should().BeTrue();
    }

    [Test]
    public void IsLowStock_WithStockAtThreshold_ReturnsTrue()
    {
        // Act
        var isLowStock = StockManagementPolicy.IsLowStock(10, 10);

        // Assert
        isLowStock.Should().BeTrue();
    }

    [Test]
    public void IsLowStock_WithStockAboveThreshold_ReturnsFalse()
    {
        // Act
        var isLowStock = StockManagementPolicy.IsLowStock(15, 10);

        // Assert
        isLowStock.Should().BeFalse();
    }

    [Test]
    public void IsLowStock_WithZeroStock_ReturnsFalse()
    {
        // Act
        var isLowStock = StockManagementPolicy.IsLowStock(0, 10);

        // Assert
        isLowStock.Should().BeFalse();
    }

    [Test]
    public void IsLowStock_WithDefaultThreshold_UsesDefault()
    {
        // Act
        var isLowStock = StockManagementPolicy.IsLowStock(5, 0);

        // Assert
        isLowStock.Should().BeTrue();
    }

    [Test]
    public void CanReserveStock_WithSufficientStock_ReturnsTrue()
    {
        // Act
        var canReserve = StockManagementPolicy.CanReserveStock(10, 5);

        // Assert
        canReserve.Should().BeTrue();
    }

    [Test]
    public void CanReserveStock_WithExactStock_ReturnsTrue()
    {
        // Act
        var canReserve = StockManagementPolicy.CanReserveStock(10, 10);

        // Assert
        canReserve.Should().BeTrue();
    }

    [Test]
    public void CanReserveStock_WithInsufficientStock_ReturnsFalse()
    {
        // Act
        var canReserve = StockManagementPolicy.CanReserveStock(5, 10);

        // Assert
        canReserve.Should().BeFalse();
    }

    [Test]
    public void CanReserveStock_WithZeroQuantity_ReturnsFalse()
    {
        // Act
        var canReserve = StockManagementPolicy.CanReserveStock(10, 0);

        // Assert
        canReserve.Should().BeFalse();
    }

    [Test]
    public void CanReserveStock_WithNegativeQuantity_ReturnsFalse()
    {
        // Act
        var canReserve = StockManagementPolicy.CanReserveStock(10, -5);

        // Assert
        canReserve.Should().BeFalse();
    }

    [Test]
    public void IsQuantityWithinOrderLimits_BelowLimit_ReturnsTrue()
    {
        // Act
        var isWithinLimit = StockManagementPolicy.IsQuantityWithinOrderLimits(
            5,
            10
        );

        // Assert
        isWithinLimit.Should().BeTrue();
    }

    [Test]
    public void IsQuantityWithinOrderLimits_AtLimit_ReturnsTrue()
    {
        // Act
        var isWithinLimit = StockManagementPolicy.IsQuantityWithinOrderLimits(
            10,
            10
        );

        // Assert
        isWithinLimit.Should().BeTrue();
    }

    [Test]
    public void IsQuantityWithinOrderLimits_ExceedsLimit_ReturnsFalse()
    {
        // Act
        var isWithinLimit = StockManagementPolicy.IsQuantityWithinOrderLimits(
            15,
            10
        );

        // Assert
        isWithinLimit.Should().BeFalse();
    }

    [Test]
    public void IsQuantityWithinOrderLimits_WithNoLimit_ReturnsTrue()
    {
        // Act
        var isWithinLimit = StockManagementPolicy.IsQuantityWithinOrderLimits(
            100,
            0
        );

        // Assert
        isWithinLimit.Should().BeTrue();
    }

    [Test]
    public void IsQuantityWithinOrderLimits_WithZeroQuantity_ReturnsFalse()
    {
        // Act
        var isWithinLimit = StockManagementPolicy.IsQuantityWithinOrderLimits(
            0,
            10
        );

        // Assert
        isWithinLimit.Should().BeFalse();
    }

    [Test]
    public void IsQuantityWithinOrderLimits_WithNegativeQuantity_ReturnsFalse()
    {
        // Act
        var isWithinLimit = StockManagementPolicy.IsQuantityWithinOrderLimits(
            -5,
            10
        );

        // Assert
        isWithinLimit.Should().BeFalse();
    }

    [Test]
    public void CalculateReorderQuantity_WithNormalSales_ReturnsThirtyDaysSupply()
    {
        // Arrange
        var currentStock = 10;
        var minStockLevel = 20;
        var averageDailySales = 5;

        // Act
        var reorderQuantity = StockManagementPolicy.CalculateReorderQuantity(
            currentStock,
            minStockLevel,
            averageDailySales
        );

        // Assert
        // (5 * 30) + 20 - 10 = 150 + 20 - 10 = 160
        reorderQuantity.Should().Be(160);
    }

    [Test]
    public void CalculateReorderQuantity_WithZeroSales_ReturnsDefaultQuantity()
    {
        // Arrange
        var currentStock = 10;
        var minStockLevel = 20;
        var averageDailySales = 0;

        // Act
        var reorderQuantity = StockManagementPolicy.CalculateReorderQuantity(
            currentStock,
            minStockLevel,
            averageDailySales
        );

        // Assert
        reorderQuantity.Should().Be(50); // Default reorder quantity
    }

    [Test]
    public void CalculateReorderQuantity_WithNegativeSales_ReturnsDefaultQuantity()
    {
        // Arrange
        var currentStock = 10;
        var minStockLevel = 20;
        var averageDailySales = -5;

        // Act
        var reorderQuantity = StockManagementPolicy.CalculateReorderQuantity(
            currentStock,
            minStockLevel,
            averageDailySales
        );

        // Assert
        reorderQuantity.Should().Be(50); // Default reorder quantity
    }

    [Test]
    public void CalculateReorderQuantity_WithHighStock_ReturnsMinimumQuantity()
    {
        // Arrange
        var currentStock = 200;
        var minStockLevel = 20;
        var averageDailySales = 2;

        // Act
        var reorderQuantity = StockManagementPolicy.CalculateReorderQuantity(
            currentStock,
            minStockLevel,
            averageDailySales
        );

        // Assert
        reorderQuantity.Should().BeGreaterThanOrEqualTo(50); // At least default quantity
    }

    [Test]
    public void IsValidStockAdjustment_WithPositiveAdjustment_ReturnsTrue()
    {
        // Act
        var isValid = StockManagementPolicy.IsValidStockAdjustment(10, 5);

        // Assert
        isValid.Should().BeTrue();
    }

    [Test]
    public void IsValidStockAdjustment_WithNegativeAdjustmentAboveMinimum_ReturnsTrue()
    {
        // Act
        var isValid = StockManagementPolicy.IsValidStockAdjustment(10, -5);

        // Assert
        isValid.Should().BeTrue();
    }

    [Test]
    public void IsValidStockAdjustment_ResultingInZeroStock_ReturnsTrue()
    {
        // Act
        var isValid = StockManagementPolicy.IsValidStockAdjustment(10, -10);

        // Assert
        isValid.Should().BeTrue();
    }

    [Test]
    public void IsValidStockAdjustment_ResultingInNegativeStock_ReturnsFalse()
    {
        // Act
        var isValid = StockManagementPolicy.IsValidStockAdjustment(10, -15);

        // Assert
        isValid.Should().BeFalse();
    }

    [Test]
    public void IsValidStockAdjustment_WithZeroAdjustment_ReturnsTrue()
    {
        // Act
        var isValid = StockManagementPolicy.IsValidStockAdjustment(10, 0);

        // Assert
        isValid.Should().BeTrue();
    }

    [Test]
    public void ShouldTriggerReorder_BelowReorderPoint_ReturnsTrue()
    {
        // Act
        var shouldReorder = StockManagementPolicy.ShouldTriggerReorder(5, 10, 8);

        // Assert
        shouldReorder.Should().BeTrue();
    }

    [Test]
    public void ShouldTriggerReorder_AtReorderPoint_ReturnsTrue()
    {
        // Act
        var shouldReorder = StockManagementPolicy.ShouldTriggerReorder(8, 10, 8);

        // Assert
        shouldReorder.Should().BeTrue();
    }

    [Test]
    public void ShouldTriggerReorder_AboveReorderPoint_ReturnsFalse()
    {
        // Act
        var shouldReorder = StockManagementPolicy.ShouldTriggerReorder(10, 10, 8);

        // Assert
        shouldReorder.Should().BeFalse();
    }

    [Test]
    public void ShouldTriggerReorder_WithNoReorderPoint_UsesLowStockLogic()
    {
        // Act
        var shouldReorder = StockManagementPolicy.ShouldTriggerReorder(5, 10, 0);

        // Assert
        shouldReorder.Should().BeTrue();
    }

    [Test]
    public void ShouldTriggerReorder_HighStock_WithNoReorderPoint_ReturnsFalse()
    {
        // Act
        var shouldReorder = StockManagementPolicy.ShouldTriggerReorder(15, 10, 0);

        // Assert
        shouldReorder.Should().BeFalse();
    }
}

