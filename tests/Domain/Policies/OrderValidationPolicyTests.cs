using ECommerce.Domain.Enums;
using ECommerce.Domain.Policies;

namespace ECommerce.Tests.Domain.Policies;

/// <summary>
/// Tests for OrderValidationPolicy
/// </summary>
[TestFixture]
public class OrderValidationPolicyTests : BaseTestFixture
{
    [Test]
    public void IsValidOrderAmount_WithValidAmount_ReturnsTrue()
    {
        // Act
        var isValid = OrderValidationPolicy.IsValidOrderAmount(100m);

        // Assert
        isValid.Should().BeTrue();
    }

    [Test]
    public void IsValidOrderAmount_WithMinimumAmount_ReturnsTrue()
    {
        // Act
        var isValid = OrderValidationPolicy.IsValidOrderAmount(0.01m);

        // Assert
        isValid.Should().BeTrue();
    }

    [Test]
    public void IsValidOrderAmount_WithMaximumAmount_ReturnsTrue()
    {
        // Act
        var isValid = OrderValidationPolicy.IsValidOrderAmount(999999.99m);

        // Assert
        isValid.Should().BeTrue();
    }

    [Test]
    public void IsValidOrderAmount_WithZeroAmount_ReturnsFalse()
    {
        // Act
        var isValid = OrderValidationPolicy.IsValidOrderAmount(0m);

        // Assert
        isValid.Should().BeFalse();
    }

    [Test]
    public void IsValidOrderAmount_WithNegativeAmount_ReturnsFalse()
    {
        // Act
        var isValid = OrderValidationPolicy.IsValidOrderAmount(-10m);

        // Assert
        isValid.Should().BeFalse();
    }

    [Test]
    public void IsValidOrderAmount_ExceedsMaximum_ReturnsFalse()
    {
        // Act
        var isValid = OrderValidationPolicy.IsValidOrderAmount(1000000m);

        // Assert
        isValid.Should().BeFalse();
    }

    [Test]
    public void IsValidItemCount_WithValidCount_ReturnsTrue()
    {
        // Act
        var isValid = OrderValidationPolicy.IsValidItemCount(50);

        // Assert
        isValid.Should().BeTrue();
    }

    [Test]
    public void IsValidItemCount_WithOneItem_ReturnsTrue()
    {
        // Act
        var isValid = OrderValidationPolicy.IsValidItemCount(1);

        // Assert
        isValid.Should().BeTrue();
    }

    [Test]
    public void IsValidItemCount_AtMaximum_ReturnsTrue()
    {
        // Act
        var isValid = OrderValidationPolicy.IsValidItemCount(100);

        // Assert
        isValid.Should().BeTrue();
    }

    [Test]
    public void IsValidItemCount_WithZeroItems_ReturnsFalse()
    {
        // Act
        var isValid = OrderValidationPolicy.IsValidItemCount(0);

        // Assert
        isValid.Should().BeFalse();
    }

    [Test]
    public void IsValidItemCount_ExceedsMaximum_ReturnsFalse()
    {
        // Act
        var isValid = OrderValidationPolicy.IsValidItemCount(150);

        // Assert
        isValid.Should().BeFalse();
    }

    [Test]
    public void CanCancelOrder_WithPendingStatus_ReturnsTrue()
    {
        // Act
        var canCancel = OrderValidationPolicy.CanCancelOrder(OrderStatus.Pending);

        // Assert
        canCancel.Should().BeTrue();
    }

    [Test]
    public void CanCancelOrder_WithProcessingStatus_ReturnsTrue()
    {
        // Act
        var canCancel = OrderValidationPolicy.CanCancelOrder(OrderStatus.Processing);

        // Assert
        canCancel.Should().BeTrue();
    }

    [Test]
    public void CanCancelOrder_WithConfirmedStatus_ReturnsTrue()
    {
        // Act
        var canCancel = OrderValidationPolicy.CanCancelOrder(OrderStatus.Confirmed);

        // Assert
        canCancel.Should().BeTrue();
    }

    [Test]
    public void CanCancelOrder_WithShippedStatus_ReturnsFalse()
    {
        // Act
        var canCancel = OrderValidationPolicy.CanCancelOrder(OrderStatus.Shipped);

        // Assert
        canCancel.Should().BeFalse();
    }

    [Test]
    public void CanCancelOrder_WithDeliveredStatus_ReturnsFalse()
    {
        // Act
        var canCancel = OrderValidationPolicy.CanCancelOrder(OrderStatus.Delivered);

        // Assert
        canCancel.Should().BeFalse();
    }

    [Test]
    public void CanCancelOrder_WithCancelledStatus_ReturnsFalse()
    {
        // Act
        var canCancel = OrderValidationPolicy.CanCancelOrder(OrderStatus.Cancelled);

        // Assert
        canCancel.Should().BeFalse();
    }

    [Test]
    public void CanCancelOrder_WithRefundedStatus_ReturnsFalse()
    {
        // Act
        var canCancel = OrderValidationPolicy.CanCancelOrder(OrderStatus.Refunded);

        // Assert
        canCancel.Should().BeFalse();
    }

    [Test]
    public void IsWithinCancellationWindow_WithinWindow_ReturnsTrue()
    {
        // Arrange
        var orderCreatedAt = DateTime.UtcNow.AddHours(-10);

        // Act
        var isWithinWindow = OrderValidationPolicy.IsWithinCancellationWindow(orderCreatedAt);

        // Assert
        isWithinWindow.Should().BeTrue();
    }

    [Test]
    public void IsWithinCancellationWindow_AtWindowEdge_ReturnsTrue()
    {
        // Arrange - use slightly under 24 hours to avoid timing precision issues
        var orderCreatedAt = DateTime.UtcNow.AddHours(-23.5);

        // Act
        var isWithinWindow = OrderValidationPolicy.IsWithinCancellationWindow(orderCreatedAt);

        // Assert
        isWithinWindow.Should().BeTrue();
    }

    [Test]
    public void IsWithinCancellationWindow_BeyondWindow_ReturnsFalse()
    {
        // Arrange
        var orderCreatedAt = DateTime.UtcNow.AddHours(-30);

        // Act
        var isWithinWindow = OrderValidationPolicy.IsWithinCancellationWindow(orderCreatedAt);

        // Assert
        isWithinWindow.Should().BeFalse();
    }

    [Test]
    public void CanModifyOrder_WithPendingStatus_ReturnsTrue()
    {
        // Act
        var canModify = OrderValidationPolicy.CanModifyOrder(OrderStatus.Pending);

        // Assert
        canModify.Should().BeTrue();
    }

    [Test]
    public void CanModifyOrder_WithProcessingStatus_ReturnsFalse()
    {
        // Act
        var canModify = OrderValidationPolicy.CanModifyOrder(OrderStatus.Processing);

        // Assert
        canModify.Should().BeFalse();
    }

    [Test]
    public void CanModifyOrder_WithShippedStatus_ReturnsFalse()
    {
        // Act
        var canModify = OrderValidationPolicy.CanModifyOrder(OrderStatus.Shipped);

        // Assert
        canModify.Should().BeFalse();
    }

    [Test]
    public void CanRefundOrder_WithDeliveredStatusAndRecentDelivery_ReturnsTrue()
    {
        // Arrange
        var deliveredAt = DateTime.UtcNow.AddDays(-10);

        // Act
        var canRefund = OrderValidationPolicy.CanRefundOrder(OrderStatus.Delivered, deliveredAt);

        // Assert
        canRefund.Should().BeTrue();
    }

    [Test]
    public void CanRefundOrder_WithDeliveredStatusAndOldDelivery_ReturnsFalse()
    {
        // Arrange
        var deliveredAt = DateTime.UtcNow.AddDays(-40);

        // Act
        var canRefund = OrderValidationPolicy.CanRefundOrder(OrderStatus.Delivered, deliveredAt);

        // Assert
        canRefund.Should().BeFalse();
    }

    [Test]
    public void CanRefundOrder_WithPendingStatus_ReturnsFalse()
    {
        // Arrange
        var deliveredAt = DateTime.UtcNow.AddDays(-10);

        // Act
        var canRefund = OrderValidationPolicy.CanRefundOrder(OrderStatus.Pending, deliveredAt);

        // Assert
        canRefund.Should().BeFalse();
    }

    [Test]
    public void CanRefundOrder_WithNullDeliveryDate_ReturnsFalse()
    {
        // Act
        var canRefund = OrderValidationPolicy.CanRefundOrder(OrderStatus.Delivered, null);

        // Assert
        canRefund.Should().BeFalse();
    }

    [Test]
    public void IsValidOrderTotal_WithCorrectCalculation_ReturnsTrue()
    {
        // Act
        var isValid = OrderValidationPolicy.IsValidOrderTotal(
            subtotal: 100m,
            taxAmount: 10m,
            shippingCost: 5m,
            discountAmount: 15m,
            totalAmount: 100m // 100 + 10 + 5 - 15 = 100
        );

        // Assert
        isValid.Should().BeTrue();
    }

    [Test]
    public void IsValidOrderTotal_WithRoundingTolerance_ReturnsTrue()
    {
        // Act
        var isValid = OrderValidationPolicy.IsValidOrderTotal(
            subtotal: 100m,
            taxAmount: 10m,
            shippingCost: 5m,
            discountAmount: 15m,
            totalAmount: 100.005m // Within tolerance
        );

        // Assert
        isValid.Should().BeTrue();
    }

    [Test]
    public void IsValidOrderTotal_WithIncorrectCalculation_ReturnsFalse()
    {
        // Act
        var isValid = OrderValidationPolicy.IsValidOrderTotal(
            subtotal: 100m,
            taxAmount: 10m,
            shippingCost: 5m,
            discountAmount: 15m,
            totalAmount: 110m // Incorrect
        );

        // Assert
        isValid.Should().BeFalse();
    }

    [Test]
    public void IsValidOrderTotal_WithNegativeSubtotal_ReturnsFalse()
    {
        // Act
        var isValid = OrderValidationPolicy.IsValidOrderTotal(
            subtotal: -100m,
            taxAmount: 10m,
            shippingCost: 5m,
            discountAmount: 15m,
            totalAmount: -100m
        );

        // Assert
        isValid.Should().BeFalse();
    }

    [Test]
    public void IsValidOrderTotal_WithNegativeTax_ReturnsFalse()
    {
        // Act
        var isValid = OrderValidationPolicy.IsValidOrderTotal(
            subtotal: 100m,
            taxAmount: -10m,
            shippingCost: 5m,
            discountAmount: 15m,
            totalAmount: 80m
        );

        // Assert
        isValid.Should().BeFalse();
    }

    [Test]
    public void IsValidStatusTransition_PendingToProcessing_ReturnsTrue()
    {
        // Act
        var isValid = OrderValidationPolicy.IsValidStatusTransition(
            OrderStatus.Pending,
            OrderStatus.Processing
        );

        // Assert
        isValid.Should().BeTrue();
    }

    [Test]
    public void IsValidStatusTransition_ProcessingToConfirmed_ReturnsTrue()
    {
        // Act
        var isValid = OrderValidationPolicy.IsValidStatusTransition(
            OrderStatus.Processing,
            OrderStatus.Confirmed
        );

        // Assert
        isValid.Should().BeTrue();
    }

    [Test]
    public void IsValidStatusTransition_ConfirmedToShipped_ReturnsTrue()
    {
        // Act
        var isValid = OrderValidationPolicy.IsValidStatusTransition(
            OrderStatus.Confirmed,
            OrderStatus.Shipped
        );

        // Assert
        isValid.Should().BeTrue();
    }

    [Test]
    public void IsValidStatusTransition_ShippedToDelivered_ReturnsTrue()
    {
        // Act
        var isValid = OrderValidationPolicy.IsValidStatusTransition(
            OrderStatus.Shipped,
            OrderStatus.Delivered
        );

        // Assert
        isValid.Should().BeTrue();
    }

    [Test]
    public void IsValidStatusTransition_DeliveredToRefunded_ReturnsTrue()
    {
        // Act
        var isValid = OrderValidationPolicy.IsValidStatusTransition(
            OrderStatus.Delivered,
            OrderStatus.Refunded
        );

        // Assert
        isValid.Should().BeTrue();
    }

    [Test]
    public void IsValidStatusTransition_PendingToCancelled_ReturnsTrue()
    {
        // Act
        var isValid = OrderValidationPolicy.IsValidStatusTransition(
            OrderStatus.Pending,
            OrderStatus.Cancelled
        );

        // Assert
        isValid.Should().BeTrue();
    }

    [Test]
    public void IsValidStatusTransition_ShippedToCancelled_ReturnsFalse()
    {
        // Act
        var isValid = OrderValidationPolicy.IsValidStatusTransition(
            OrderStatus.Shipped,
            OrderStatus.Cancelled
        );

        // Assert
        isValid.Should().BeFalse();
    }

    [Test]
    public void IsValidStatusTransition_DeliveredToPending_ReturnsFalse()
    {
        // Act
        var isValid = OrderValidationPolicy.IsValidStatusTransition(
            OrderStatus.Delivered,
            OrderStatus.Pending
        );

        // Assert
        isValid.Should().BeFalse();
    }

    [Test]
    public void RequiresShippingAddress_WithStandardShipping_ReturnsTrue()
    {
        // Act
        var requires = OrderValidationPolicy.RequiresShippingAddress(ShippingMethod.Standard);

        // Assert
        requires.Should().BeTrue();
    }

    [Test]
    public void RequiresShippingAddress_WithExpressShipping_ReturnsTrue()
    {
        // Act
        var requires = OrderValidationPolicy.RequiresShippingAddress(ShippingMethod.Express);

        // Assert
        requires.Should().BeTrue();
    }

    [Test]
    public void RequiresShippingAddress_WithStorePickup_ReturnsFalse()
    {
        // Act
        var requires = OrderValidationPolicy.RequiresShippingAddress(ShippingMethod.StorePickup);

        // Assert
        requires.Should().BeFalse();
    }

    [Test]
    public void RequiresShippingAddress_WithNotSpecified_ReturnsFalse()
    {
        // Act
        var requires = OrderValidationPolicy.RequiresShippingAddress(ShippingMethod.NotSpecified);

        // Assert
        requires.Should().BeFalse();
    }
}
