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
        using (Assert.EnterMultipleScope())
        {
            Assert.That(order.Status, Is.EqualTo(OrderStatus.Pending));
            Assert.That(order.PaymentMethod, Is.EqualTo(PaymentMethod.NotSpecified));
            Assert.That(order.ShippingMethod, Is.EqualTo(ShippingMethod.NotSpecified));
            Assert.That(order.IsDeleted, Is.False);
        }
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
        Assert.That(order.CustomerId, Is.EqualTo(customerId));
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
        Assert.That(order.OrderNumber, Is.EqualTo(orderNumber));
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
        Assert.That(order.TotalAmount, Is.EqualTo(100m));
    }

    [Test]
    public void OrderEntity_SetStatus_UpdatesCorrectly()
    {
        // Arrange
        var order = new OrderEntity();

        // Act
        order.Status = OrderStatus.Shipped;

        // Assert
        Assert.That(order.Status, Is.EqualTo(OrderStatus.Shipped));
    }

    [Test]
    public void OrderEntity_SetPaymentMethod_UpdatesCorrectly()
    {
        // Arrange
        var order = new OrderEntity();

        // Act
        order.PaymentMethod = PaymentMethod.CreditCard;

        // Assert
        Assert.That(order.PaymentMethod, Is.EqualTo(PaymentMethod.CreditCard));
    }

    [Test]
    public void OrderEntity_SetShippingMethod_UpdatesCorrectly()
    {
        // Arrange
        var order = new OrderEntity();

        // Act
        order.ShippingMethod = ShippingMethod.Express;

        // Assert
        Assert.That(order.ShippingMethod, Is.EqualTo(ShippingMethod.Express));
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
        Assert.That(order.CouponCode, Is.EqualTo(couponCode));
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
        Assert.That(order.TrackingNumber, Is.EqualTo(trackingNumber));
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
        Assert.That(order.ExpectedDeliveryDate, Is.EqualTo(deliveryDate));
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
        using (Assert.EnterMultipleScope())
        {
            Assert.That(order.Status, Is.EqualTo(OrderStatus.Cancelled));
            Assert.That(order.CancelledAt, Is.Not.Null);
            Assert.That(order.CancellationReason, Is.EqualTo(cancellationReason));
        }
    }

    [Test]
    public void OrderEntity_HasTimestamps()
    {
        // Arrange & Act
        var order = new OrderEntity();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(
                order.CreatedAt,
                Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(1))
            );
            Assert.That(
                order.UpdatedAt,
                Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(1))
            );
        }
    }
}
