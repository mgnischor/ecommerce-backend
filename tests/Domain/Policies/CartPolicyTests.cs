using ECommerce.Domain.Policies;

namespace ECommerce.Tests.Domain.Policies;

/// <summary>
/// Tests for CartPolicy
/// </summary>
[TestFixture]
public class CartPolicyTests : BaseTestFixture
{
    [Test]
    public void CanAddItemToCart_BelowLimit_ReturnsTrue()
    {
        // Arrange
        var currentItemCount = 30;

        // Act
        var canAdd = CartPolicy.CanAddItemToCart(currentItemCount);

        // Assert
        canAdd.Should().BeTrue();
    }

    [Test]
    public void CanAddItemToCart_AtLimit_ReturnsFalse()
    {
        // Arrange
        var currentItemCount = 50;

        // Act
        var canAdd = CartPolicy.CanAddItemToCart(currentItemCount);

        // Assert
        canAdd.Should().BeFalse();
    }

    [Test]
    public void CanAddItemToCart_AboveLimit_ReturnsFalse()
    {
        // Arrange
        var currentItemCount = 60;

        // Act
        var canAdd = CartPolicy.CanAddItemToCart(currentItemCount);

        // Assert
        canAdd.Should().BeFalse();
    }

    [Test]
    public void IsValidQuantity_WithPositiveQuantity_ReturnsTrue()
    {
        // Act
        var isValid = CartPolicy.IsValidQuantity(5);

        // Assert
        isValid.Should().BeTrue();
    }

    [Test]
    public void IsValidQuantity_WithZeroQuantity_ReturnsFalse()
    {
        // Act
        var isValid = CartPolicy.IsValidQuantity(0);

        // Assert
        isValid.Should().BeFalse();
    }

    [Test]
    public void IsValidQuantity_WithNegativeQuantity_ReturnsFalse()
    {
        // Act
        var isValid = CartPolicy.IsValidQuantity(-5);

        // Assert
        isValid.Should().BeFalse();
    }

    [Test]
    public void IsValidQuantity_WithExcessiveQuantity_ReturnsFalse()
    {
        // Act
        var isValid = CartPolicy.IsValidQuantity(100);

        // Assert
        isValid.Should().BeFalse();
    }

    [Test]
    public void IsValidQuantity_AtMaximum_ReturnsTrue()
    {
        // Act
        var isValid = CartPolicy.IsValidQuantity(99);

        // Assert
        isValid.Should().BeTrue();
    }

    [Test]
    public void CanUpdateQuantity_WithValidQuantityAndStock_ReturnsTrue()
    {
        // Act
        var canUpdate = CartPolicy.CanUpdateQuantity(5, 10);

        // Assert
        canUpdate.Should().BeTrue();
    }

    [Test]
    public void CanUpdateQuantity_ExceedsStock_ReturnsFalse()
    {
        // Act
        var canUpdate = CartPolicy.CanUpdateQuantity(10, 5);

        // Assert
        canUpdate.Should().BeFalse();
    }

    [Test]
    public void CanUpdateQuantity_WithInvalidQuantity_ReturnsFalse()
    {
        // Act
        var canUpdate = CartPolicy.CanUpdateQuantity(-1, 10);

        // Assert
        canUpdate.Should().BeFalse();
    }

    [Test]
    public void IsCartExpired_GuestCart_WithinExpirationPeriod_ReturnsFalse()
    {
        // Arrange
        var lastUpdatedAt = DateTime.UtcNow.AddDays(-5);

        // Act
        var isExpired = CartPolicy.IsCartExpired(lastUpdatedAt, true);

        // Assert
        isExpired.Should().BeFalse();
    }

    [Test]
    public void IsCartExpired_GuestCart_BeyondExpirationPeriod_ReturnsTrue()
    {
        // Arrange
        var lastUpdatedAt = DateTime.UtcNow.AddDays(-10);

        // Act
        var isExpired = CartPolicy.IsCartExpired(lastUpdatedAt, true);

        // Assert
        isExpired.Should().BeTrue();
    }

    [Test]
    public void IsCartExpired_UserCart_WithinExpirationPeriod_ReturnsFalse()
    {
        // Arrange
        var lastUpdatedAt = DateTime.UtcNow.AddDays(-20);

        // Act
        var isExpired = CartPolicy.IsCartExpired(lastUpdatedAt, false);

        // Assert
        isExpired.Should().BeFalse();
    }

    [Test]
    public void IsCartExpired_UserCart_BeyondExpirationPeriod_ReturnsTrue()
    {
        // Arrange
        var lastUpdatedAt = DateTime.UtcNow.AddDays(-35);

        // Act
        var isExpired = CartPolicy.IsCartExpired(lastUpdatedAt, false);

        // Assert
        isExpired.Should().BeTrue();
    }

    [Test]
    public void IsValidCartValue_WithPositiveValue_ReturnsTrue()
    {
        // Act
        var isValid = CartPolicy.IsValidCartValue(100m);

        // Assert
        isValid.Should().BeTrue();
    }

    [Test]
    public void IsValidCartValue_WithZeroValue_ReturnsFalse()
    {
        // Act
        var isValid = CartPolicy.IsValidCartValue(0m);

        // Assert
        isValid.Should().BeFalse();
    }

    [Test]
    public void IsValidCartValue_ExceedsMaximum_ReturnsFalse()
    {
        // Act
        var isValid = CartPolicy.IsValidCartValue(1000000m);

        // Assert
        isValid.Should().BeFalse();
    }

    [Test]
    public void CanMergeCarts_WithinLimit_ReturnsTrue()
    {
        // Act
        var canMerge = CartPolicy.CanMergeCarts(20, 25);

        // Assert
        canMerge.Should().BeTrue();
    }

    [Test]
    public void CanMergeCarts_ExceedsLimit_ReturnsFalse()
    {
        // Act
        var canMerge = CartPolicy.CanMergeCarts(30, 25);

        // Assert
        canMerge.Should().BeFalse();
    }

    [Test]
    public void ShouldMergeDuplicateItems_SameProductAndVariant_ReturnsTrue()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();

        // Act
        var shouldMerge = CartPolicy.ShouldMergeDuplicateItems(
            productId,
            productId,
            variantId,
            variantId
        );

        // Assert
        shouldMerge.Should().BeTrue();
    }

    [Test]
    public void ShouldMergeDuplicateItems_DifferentProducts_ReturnsFalse()
    {
        // Arrange
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();

        // Act
        var shouldMerge = CartPolicy.ShouldMergeDuplicateItems(
            productId1,
            productId2,
            null,
            null
        );

        // Assert
        shouldMerge.Should().BeFalse();
    }

    [Test]
    public void ShouldMergeDuplicateItems_DifferentVariants_ReturnsFalse()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var variantId1 = Guid.NewGuid();
        var variantId2 = Guid.NewGuid();

        // Act
        var shouldMerge = CartPolicy.ShouldMergeDuplicateItems(
            productId,
            productId,
            variantId1,
            variantId2
        );

        // Assert
        shouldMerge.Should().BeFalse();
    }

    [Test]
    public void CalculateMergedQuantity_WithinLimit_ReturnsSummed()
    {
        // Act
        var merged = CartPolicy.CalculateMergedQuantity(5, 10);

        // Assert
        merged.Should().Be(15);
    }

    [Test]
    public void CalculateMergedQuantity_ExceedsMaximum_ReturnsMaximum()
    {
        // Act
        var merged = CartPolicy.CalculateMergedQuantity(60, 50);

        // Assert
        merged.Should().Be(99);
    }

    [Test]
    public void IsCartEmpty_WithZeroItems_ReturnsTrue()
    {
        // Act
        var isEmpty = CartPolicy.IsCartEmpty(0);

        // Assert
        isEmpty.Should().BeTrue();
    }

    [Test]
    public void IsCartEmpty_WithItems_ReturnsFalse()
    {
        // Act
        var isEmpty = CartPolicy.IsCartEmpty(5);

        // Assert
        isEmpty.Should().BeFalse();
    }

    [Test]
    public void CanConvertToOrder_WithValidCart_ReturnsTrue()
    {
        // Act
        var canConvert = CartPolicy.CanConvertToOrder(5, 100m, false);

        // Assert
        canConvert.Should().BeTrue();
    }

    [Test]
    public void CanConvertToOrder_WithEmptyCart_ReturnsFalse()
    {
        // Act
        var canConvert = CartPolicy.CanConvertToOrder(0, 100m, false);

        // Assert
        canConvert.Should().BeFalse();
    }

    [Test]
    public void CanConvertToOrder_WithOutOfStockItems_ReturnsFalse()
    {
        // Act
        var canConvert = CartPolicy.CanConvertToOrder(5, 100m, true);

        // Assert
        canConvert.Should().BeFalse();
    }

    [Test]
    public void CanConvertToOrder_WithInvalidValue_ReturnsFalse()
    {
        // Act
        var canConvert = CartPolicy.CanConvertToOrder(5, 0m, false);

        // Assert
        canConvert.Should().BeFalse();
    }

    [Test]
    public void RequiresStockRevalidation_RecentValidation_ReturnsFalse()
    {
        // Arrange
        var lastValidatedAt = DateTime.UtcNow.AddMinutes(-10);

        // Act
        var requiresRevalidation = CartPolicy.RequiresStockRevalidation(
            lastValidatedAt
        );

        // Assert
        requiresRevalidation.Should().BeFalse();
    }

    [Test]
    public void RequiresStockRevalidation_OldValidation_ReturnsTrue()
    {
        // Arrange
        var lastValidatedAt = DateTime.UtcNow.AddMinutes(-20);

        // Act
        var requiresRevalidation = CartPolicy.RequiresStockRevalidation(
            lastValidatedAt
        );

        // Assert
        requiresRevalidation.Should().BeTrue();
    }

    [Test]
    public void CanSaveCartForLater_Authenticated_ReturnsTrue()
    {
        // Act
        var canSave = CartPolicy.CanSaveCartForLater(true);

        // Assert
        canSave.Should().BeTrue();
    }

    [Test]
    public void CanSaveCartForLater_NotAuthenticated_ReturnsFalse()
    {
        // Act
        var canSave = CartPolicy.CanSaveCartForLater(false);

        // Assert
        canSave.Should().BeFalse();
    }

    [Test]
    public void ShouldAutoRemoveItem_InactiveProduct_ReturnsTrue()
    {
        // Act
        var shouldRemove = CartPolicy.ShouldAutoRemoveItem(false, false, 10);

        // Assert
        shouldRemove.Should().BeTrue();
    }

    [Test]
    public void ShouldAutoRemoveItem_DeletedProduct_ReturnsTrue()
    {
        // Act
        var shouldRemove = CartPolicy.ShouldAutoRemoveItem(true, true, 10);

        // Assert
        shouldRemove.Should().BeTrue();
    }

    [Test]
    public void ShouldAutoRemoveItem_NoStock_ReturnsTrue()
    {
        // Act
        var shouldRemove = CartPolicy.ShouldAutoRemoveItem(true, false, 0);

        // Assert
        shouldRemove.Should().BeTrue();
    }

    [Test]
    public void ShouldAutoRemoveItem_ValidProduct_ReturnsFalse()
    {
        // Act
        var shouldRemove = CartPolicy.ShouldAutoRemoveItem(true, false, 10);

        // Assert
        shouldRemove.Should().BeFalse();
    }

    [Test]
    public void CalculateSubtotal_WithMultipleItems_ReturnsCorrectTotal()
    {
        // Arrange
        var items = new List<(decimal unitPrice, int quantity)> { (10m, 2), (25m, 3), (50m, 1) };

        // Act
        var subtotal = CartPolicy.CalculateSubtotal(items);

        // Assert
        subtotal.Should().Be(145m); // (10*2) + (25*3) + (50*1) = 20 + 75 + 50
    }

    [Test]
    public void CalculateSubtotal_WithEmptyList_ReturnsZero()
    {
        // Arrange
        var items = new List<(decimal unitPrice, int quantity)>();

        // Act
        var subtotal = CartPolicy.CalculateSubtotal(items);

        // Assert
        subtotal.Should().Be(0m);
    }

    [Test]
    public void AllowsMultipleVariants_WithVariants_ReturnsTrue()
    {
        // Act
        var allows = CartPolicy.AllowsMultipleVariants(true);

        // Assert
        allows.Should().BeTrue();
    }

    [Test]
    public void AllowsMultipleVariants_WithoutVariants_ReturnsFalse()
    {
        // Act
        var allows = CartPolicy.AllowsMultipleVariants(false);

        // Assert
        allows.Should().BeFalse();
    }

    [Test]
    public void MeetsMinimumOrderValue_AboveMinimum_ReturnsTrue()
    {
        // Act
        var meets = CartPolicy.MeetsMinimumOrderValue(100m, 50m);

        // Assert
        meets.Should().BeTrue();
    }

    [Test]
    public void MeetsMinimumOrderValue_BelowMinimum_ReturnsFalse()
    {
        // Act
        var meets = CartPolicy.MeetsMinimumOrderValue(30m, 50m);

        // Assert
        meets.Should().BeFalse();
    }

    [Test]
    public void MeetsMinimumOrderValue_NoMinimum_ReturnsTrue()
    {
        // Act
        var meets = CartPolicy.MeetsMinimumOrderValue(10m, 0m);

        // Assert
        meets.Should().BeTrue();
    }
}

