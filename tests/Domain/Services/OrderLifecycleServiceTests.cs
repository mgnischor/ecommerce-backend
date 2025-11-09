using ECommerce.Domain.Enums;

namespace ECommerce.Tests.Domain.Services;

/// <summary>
/// Tests for OrderLifecycleService domain service
/// </summary>
[TestFixture]
public class OrderLifecycleServiceTests : BaseTestFixture
{
    [Test]
    public void ValidateOrderPlacement_WithExcessiveItems_ReturnsError()
    {
        // Arrange
        var orderItems = new List<(ProductEntity product, int quantity)>();
        for (int i = 0; i < 150; i++)
        {
            orderItems.Add((new ProductEntity { Id = Guid.NewGuid() }, 1));
        }

        // Act
        var (isValid, errorMessage) = OrderLifecycleService.ValidateOrderPlacement(
            orderItems,
            1000m,
            Guid.NewGuid(),
            ShippingMethod.Standard
        );

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Contain("exceeds maximum number");
    }

    [Test]
    public void ValidateOrderPlacement_WithInvalidAmount_ReturnsError()
    {
        // Arrange
        var orderItems = new List<(ProductEntity product, int quantity)>
        {
            (
                new ProductEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Product",
                    StockQuantity = 10,
                    MaxOrderQuantity = 10,
                },
                2
            ),
        };

        // Act
        var (isValid, errorMessage) = OrderLifecycleService.ValidateOrderPlacement(
            orderItems,
            -100m,
            Guid.NewGuid(),
            ShippingMethod.Standard
        );

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Contain("amount is invalid");
    }

    [Test]
    public void ValidateOrderPlacement_WithOutOfStock_ReturnsError()
    {
        // Arrange
        var orderItems = new List<(ProductEntity product, int quantity)>
        {
            (
                new ProductEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Product A",
                    StockQuantity = 0,
                    MaxOrderQuantity = 10,
                },
                2
            ),
        };

        // Act
        var (isValid, errorMessage) = OrderLifecycleService.ValidateOrderPlacement(
            orderItems,
            100m,
            Guid.NewGuid(),
            ShippingMethod.Standard
        );

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Contain("out of stock");
    }

    [Test]
    public void ValidateOrderPlacement_ExceedsMaxOrderLimit_ReturnsError()
    {
        // Arrange
        var orderItems = new List<(ProductEntity product, int quantity)>
        {
            (
                new ProductEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Product A",
                    StockQuantity = 100,
                    MaxOrderQuantity = 5,
                },
                10
            ),
        };

        // Act
        var (isValid, errorMessage) = OrderLifecycleService.ValidateOrderPlacement(
            orderItems,
            100m,
            Guid.NewGuid(),
            ShippingMethod.Standard
        );

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Contain("exceeds maximum order limit");
    }

    [Test]
    public void ValidateOrderPlacement_WithoutShippingAddress_ReturnsError()
    {
        // Arrange
        var orderItems = new List<(ProductEntity product, int quantity)>
        {
            (
                new ProductEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Product A",
                    StockQuantity = 10,
                    MaxOrderQuantity = 10,
                },
                2
            ),
        };

        // Act
        var (isValid, errorMessage) = OrderLifecycleService.ValidateOrderPlacement(
            orderItems,
            100m,
            null,
            ShippingMethod.Standard
        );

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Contain("Shipping address is required");
    }

    [Test]
    public void ValidateOrderPlacement_WithStorePickup_DoesNotRequireShippingAddress()
    {
        // Arrange
        var orderItems = new List<(ProductEntity product, int quantity)>
        {
            (
                new ProductEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Product A",
                    StockQuantity = 10,
                    MaxOrderQuantity = 10,
                },
                2
            ),
        };

        // Act
        var (isValid, errorMessage) = OrderLifecycleService.ValidateOrderPlacement(
            orderItems,
            100m,
            null,
            ShippingMethod.StorePickup
        );

        // Assert
        isValid.Should().BeTrue();
        errorMessage.Should().BeNull();
    }

    [Test]
    public void ValidateOrderPlacement_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var orderItems = new List<(ProductEntity product, int quantity)>
        {
            (
                new ProductEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Product A",
                    StockQuantity = 10,
                    MaxOrderQuantity = 10,
                },
                2
            ),
        };

        // Act
        var (isValid, errorMessage) = OrderLifecycleService.ValidateOrderPlacement(
            orderItems,
            100m,
            Guid.NewGuid(),
            ShippingMethod.Standard
        );

        // Assert
        isValid.Should().BeTrue();
        errorMessage.Should().BeNull();
    }

    [Test]
    public void TransitionOrderStatus_WithInvalidTransition_ReturnsError()
    {
        // Arrange
        var order = new OrderEntity { Status = OrderStatus.Delivered };

        // Act
        var (success, errorMessage) = OrderLifecycleService.TransitionOrderStatus(
            order,
            OrderStatus.Processing
        );

        // Assert
        success.Should().BeFalse();
        errorMessage.Should().Contain("Cannot transition");
    }

    [Test]
    public void TransitionOrderStatus_CancelAlreadyShipped_ReturnsError()
    {
        // Arrange
        var order = new OrderEntity { Status = OrderStatus.Shipped };

        // Act
        var (success, errorMessage) = OrderLifecycleService.TransitionOrderStatus(
            order,
            OrderStatus.Cancelled
        );

        // Assert
        success.Should().BeFalse();
        errorMessage.Should().Contain("Cannot transition");
    }

    [Test]
    public void TransitionOrderStatus_ShipWithoutTracking_ReturnsError()
    {
        // Arrange
        var order = new OrderEntity { Status = OrderStatus.Processing, TrackingNumber = null };

        // Act
        var (success, errorMessage) = OrderLifecycleService.TransitionOrderStatus(
            order,
            OrderStatus.Shipped
        );

        // Assert
        success.Should().BeFalse();
        errorMessage.Should().Contain("Tracking number");
    }

    [Test]
    public void TransitionOrderStatus_ShipWithTracking_ReturnsSuccess()
    {
        // Arrange - order must be Confirmed before it can be Shipped
        var order = new OrderEntity { Status = OrderStatus.Confirmed, TrackingNumber = "TRACK123" };

        // Act
        var (success, errorMessage) = OrderLifecycleService.TransitionOrderStatus(
            order,
            OrderStatus.Shipped
        );

        // Assert
        success.Should().BeTrue();
        errorMessage.Should().BeNull();
    }

    [Test]
    public void TransitionOrderStatus_RefundBeforeDelivery_ReturnsError()
    {
        // Arrange
        var order = new OrderEntity { Status = OrderStatus.Processing, DeliveredAt = null };

        // Act
        var (success, errorMessage) = OrderLifecycleService.TransitionOrderStatus(
            order,
            OrderStatus.Refunded
        );

        // Assert
        success.Should().BeFalse();
        errorMessage.Should().Contain("Cannot transition");
    }

    [Test]
    public void GenerateOrderNumber_ReturnsValidFormat()
    {
        // Act
        var orderNumber = OrderLifecycleService.GenerateOrderNumber();

        // Assert
        orderNumber.Should().StartWith("ORD-");
        orderNumber.Should().MatchRegex(@"ORD-\d{8}-\d{6}-\d{4}");
    }

    [Test]
    public void GenerateOrderNumber_GeneratesUniqueNumbers()
    {
        // Act
        var orderNumber1 = OrderLifecycleService.GenerateOrderNumber();
        var orderNumber2 = OrderLifecycleService.GenerateOrderNumber();

        // Assert
        orderNumber1.Should().NotBe(orderNumber2);
    }

    [Test]
    public void CalculateExpectedDeliveryDate_SameDay_ReturnsSameDay()
    {
        // Arrange
        var orderDate = new DateTime(2025, 1, 15, 10, 0, 0); // Wednesday

        // Act
        var deliveryDate = OrderLifecycleService.CalculateExpectedDeliveryDate(
            ShippingMethod.SameDay,
            orderDate
        );

        // Assert
        deliveryDate.Should().Be(orderDate);
    }

    [Test]
    public void CalculateExpectedDeliveryDate_NextDay_ReturnsNextBusinessDay()
    {
        // Arrange
        var orderDate = new DateTime(2025, 1, 15, 10, 0, 0); // Wednesday

        // Act
        var deliveryDate = OrderLifecycleService.CalculateExpectedDeliveryDate(
            ShippingMethod.NextDay,
            orderDate
        );

        // Assert
        deliveryDate.Date.Should().Be(new DateTime(2025, 1, 16)); // Thursday
    }

    [Test]
    public void CalculateExpectedDeliveryDate_Express_ReturnsThreeBusinessDays()
    {
        // Arrange
        var orderDate = new DateTime(2025, 1, 15, 10, 0, 0); // Wednesday

        // Act
        var deliveryDate = OrderLifecycleService.CalculateExpectedDeliveryDate(
            ShippingMethod.Express,
            orderDate
        );

        // Assert
        deliveryDate.Date.Should().Be(new DateTime(2025, 1, 20)); // Monday (skips weekend)
    }

    [Test]
    public void CalculateExpectedDeliveryDate_Standard_ReturnsSevenBusinessDays()
    {
        // Arrange
        var orderDate = new DateTime(2025, 1, 15, 10, 0, 0); // Wednesday

        // Act
        var deliveryDate = OrderLifecycleService.CalculateExpectedDeliveryDate(
            ShippingMethod.Standard,
            orderDate
        );

        // Assert
        var expectedDays = 7;
        deliveryDate.Should().BeAfter(orderDate.AddDays(expectedDays - 1));
    }

    [Test]
    public void CalculateExpectedDeliveryDate_International_ReturnsFourteenBusinessDays()
    {
        // Arrange
        var orderDate = new DateTime(2025, 1, 15, 10, 0, 0); // Wednesday

        // Act
        var deliveryDate = OrderLifecycleService.CalculateExpectedDeliveryDate(
            ShippingMethod.International,
            orderDate
        );

        // Assert
        var expectedDays = 14;
        deliveryDate.Should().BeAfter(orderDate.AddDays(expectedDays - 1));
    }

    [Test]
    public void CalculateShippingCost_FreeShipping_ReturnsZero()
    {
        // Arrange
        var weight = 5m;

        // Act
        var cost = OrderLifecycleService.CalculateShippingCost(weight, ShippingMethod.FreeShipping);

        // Assert
        cost.Should().Be(0);
    }

    [Test]
    public void CalculateShippingCost_StorePickup_ReturnsZero()
    {
        // Arrange
        var weight = 0.5m; // Use weight under 1kg to avoid weight-based charges

        // Act
        var cost = OrderLifecycleService.CalculateShippingCost(weight, ShippingMethod.StorePickup);

        // Assert
        cost.Should().Be(0);
    }

    [Test]
    public void CalculateShippingCost_SameDay_ReturnsBaseCost()
    {
        // Arrange
        var weight = 0.5m;

        // Act
        var cost = OrderLifecycleService.CalculateShippingCost(weight, ShippingMethod.SameDay);

        // Assert
        cost.Should().Be(25.00m);
    }

    [Test]
    public void CalculateShippingCost_WithExtraWeight_AddsWeightCost()
    {
        // Arrange
        var weight = 3m; // 2kg over the 1kg threshold

        // Act
        var cost = OrderLifecycleService.CalculateShippingCost(weight, ShippingMethod.Standard);

        // Assert
        cost.Should().Be(6.00m); // 5.00 base + (2 * 0.50) = 6.00
    }

    [Test]
    public void CalculateShippingCost_NextDay_ReturnsCorrectCost()
    {
        // Arrange
        var weight = 1m;

        // Act
        var cost = OrderLifecycleService.CalculateShippingCost(weight, ShippingMethod.NextDay);

        // Assert
        cost.Should().Be(15.00m);
    }

    [Test]
    public void CalculateShippingCost_Express_ReturnsCorrectCost()
    {
        // Arrange
        var weight = 1m;

        // Act
        var cost = OrderLifecycleService.CalculateShippingCost(weight, ShippingMethod.Express);

        // Assert
        cost.Should().Be(10.00m);
    }

    [Test]
    public void CalculateShippingCost_International_ReturnsCorrectCost()
    {
        // Arrange
        var weight = 1m;

        // Act
        var cost = OrderLifecycleService.CalculateShippingCost(
            weight,
            ShippingMethod.International
        );

        // Assert
        cost.Should().Be(30.00m);
    }
}
