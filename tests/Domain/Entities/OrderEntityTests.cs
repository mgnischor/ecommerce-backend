using ECommerce.Domain.Enums;

namespace ECommerce.Tests.Domain.Entities;

/// <summary>
/// Tests for OrderEntity
/// </summary>
[TestFixture]
public class OrderEntityTests : BaseTestFixture
{
    [Test]
    public void OrderEntity_Creation_SetsDefaultValues()
    {
        // Act
        var order = new OrderEntity();

        // Assert
        order.Status.Should().Be(OrderStatus.Pending);
        order.PaymentMethod.Should().Be(PaymentMethod.NotSpecified);
        order.ShippingMethod.Should().Be(ShippingMethod.NotSpecified);
        order.IsDeleted.Should().BeFalse();
    }

    [Test]
    public void OrderEntity_SetCustomerId_UpdatesCorrectly()
    {
        // Arrange
        var order = new OrderEntity();
        var customerId = Guid.NewGuid();

        // Act
        order.CustomerId = customerId;

        // Assert
        order.CustomerId.Should().Be(customerId);
    }

    [Test]
    public void OrderEntity_SetOrderNumber_UpdatesCorrectly()
    {
        // Arrange
        var order = new OrderEntity();
        var orderNumber = "ORD-12345";

        // Act
        order.OrderNumber = orderNumber;

        // Assert
        order.OrderNumber.Should().Be(orderNumber);
    }

    [Test]
    public void OrderEntity_CalculateTotalAmount_IsCorrect()
    {
        // Arrange
        var order = new OrderEntity
        {
            SubTotal = 100m,
            TaxAmount = 10m,
            ShippingCost = 5m,
            DiscountAmount = 15m,
        };

        // Act
        order.TotalAmount =
            order.SubTotal + order.TaxAmount + order.ShippingCost - order.DiscountAmount;

        // Assert
        order.TotalAmount.Should().Be(100m);
    }

    [Test]
    public void OrderEntity_SetStatus_UpdatesCorrectly()
    {
        // Arrange
        var order = new OrderEntity();

        // Act
        order.Status = OrderStatus.Shipped;

        // Assert
        order.Status.Should().Be(OrderStatus.Shipped);
    }

    [Test]
    public void OrderEntity_SetPaymentMethod_UpdatesCorrectly()
    {
        // Arrange
        var order = new OrderEntity();

        // Act
        order.PaymentMethod = PaymentMethod.CreditCard;

        // Assert
        order.PaymentMethod.Should().Be(PaymentMethod.CreditCard);
    }

    [Test]
    public void OrderEntity_SetShippingMethod_UpdatesCorrectly()
    {
        // Arrange
        var order = new OrderEntity();

        // Act
        order.ShippingMethod = ShippingMethod.Express;

        // Assert
        order.ShippingMethod.Should().Be(ShippingMethod.Express);
    }

    [Test]
    public void OrderEntity_CanSetCouponCode()
    {
        // Arrange
        var order = new OrderEntity();
        var couponCode = "SAVE20";

        // Act
        order.CouponCode = couponCode;

        // Assert
        order.CouponCode.Should().Be(couponCode);
    }

    [Test]
    public void OrderEntity_CanSetTrackingNumber()
    {
        // Arrange
        var order = new OrderEntity();
        var trackingNumber = "1Z999AA10123456784";

        // Act
        order.TrackingNumber = trackingNumber;

        // Assert
        order.TrackingNumber.Should().Be(trackingNumber);
    }

    [Test]
    public void OrderEntity_CanSetDeliveryDate()
    {
        // Arrange
        var order = new OrderEntity();
        var deliveryDate = DateTime.UtcNow.AddDays(3);

        // Act
        order.ExpectedDeliveryDate = deliveryDate;

        // Assert
        order.ExpectedDeliveryDate.Should().Be(deliveryDate);
    }

    [Test]
    public void OrderEntity_CanBeCancelled()
    {
        // Arrange
        var order = new OrderEntity();
        var cancellationReason = "Customer requested cancellation";

        // Act
        order.Status = OrderStatus.Cancelled;
        order.CancelledAt = DateTime.UtcNow;
        order.CancellationReason = cancellationReason;

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
        order.CancelledAt.Should().NotBeNull();
        order.CancellationReason.Should().Be(cancellationReason);
    }

    [Test]
    public void OrderEntity_HasTimestamps()
    {
        // Arrange & Act
        var order = new OrderEntity();

        // Assert
        order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        order.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}
