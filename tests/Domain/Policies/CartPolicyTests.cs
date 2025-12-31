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
        Assert.That(canAdd, Is.True);
    }

    [Test]
    public void CanAddItemToCart_AtLimit_ReturnsFalse()
    {
        // Arrange
        var currentItemCount = 50;

        // Act
        var canAdd = CartPolicy.CanAddItemToCart(currentItemCount);

        // Assert
        Assert.That(canAdd, Is.False);
    }

    [Test]
    public void CanAddItemToCart_AboveLimit_ReturnsFalse()
    {
        // Arrange
        var currentItemCount = 60;

        // Act
        var canAdd = CartPolicy.CanAddItemToCart(currentItemCount);

        // Assert
        Assert.That(canAdd, Is.False);
    }

    [Test]
    public void IsValidQuantity_WithPositiveQuantity_ReturnsTrue()
    {
        // Act
        var isValid = CartPolicy.IsValidQuantity(5);

        // Assert
        Assert.That(isValid, Is.True);
    }

    [Test]
    public void IsValidQuantity_WithZeroQuantity_ReturnsFalse()
    {
        // Act
        var isValid = CartPolicy.IsValidQuantity(0);

        // Assert
        Assert.That(isValid, Is.False);
    }

    [Test]
    public void IsValidQuantity_WithNegativeQuantity_ReturnsFalse()
    {
        // Act
        var isValid = CartPolicy.IsValidQuantity(-5);

        // Assert
        Assert.That(isValid, Is.False);
    }

    [Test]
    public void IsValidQuantity_WithExcessiveQuantity_ReturnsFalse()
    {
        // Act
        var isValid = CartPolicy.IsValidQuantity(100);

        // Assert
        Assert.That(isValid, Is.False);
    }

    [Test]
    public void IsValidQuantity_AtMaximum_ReturnsTrue()
    {
        // Act
        var isValid = CartPolicy.IsValidQuantity(99);

        // Assert
        Assert.That(isValid, Is.True);
    }

    [Test]
    public void CanUpdateQuantity_WithValidQuantityAndStock_ReturnsTrue()
    {
        // Act
        var canUpdate = CartPolicy.CanUpdateQuantity(5, 10);

        // Assert
        Assert.That(canUpdate, Is.True);
    }

    [Test]
    public void CanUpdateQuantity_ExceedsStock_ReturnsFalse()
    {
        // Act
        var canUpdate = CartPolicy.CanUpdateQuantity(10, 5);

        // Assert
        Assert.That(canUpdate, Is.False);
    }

    [Test]
    public void CanUpdateQuantity_WithInvalidQuantity_ReturnsFalse()
    {
        // Act
        var canUpdate = CartPolicy.CanUpdateQuantity(-1, 10);

        // Assert
        Assert.That(canUpdate, Is.False);
    }

    [Test]
    public void IsCartExpired_GuestCart_WithinExpirationPeriod_ReturnsFalse()
    {
        // Arrange
        var lastUpdatedAt = DateTime.UtcNow.AddDays(-5);

        // Act
        var isExpired = CartPolicy.IsCartExpired(lastUpdatedAt, true);

        // Assert
        Assert.That(isExpired, Is.False);
    }

    [Test]
    public void IsCartExpired_GuestCart_BeyondExpirationPeriod_ReturnsTrue()
    {
        // Arrange
        var lastUpdatedAt = DateTime.UtcNow.AddDays(-10);

        // Act
        var isExpired = CartPolicy.IsCartExpired(lastUpdatedAt, true);

        // Assert
        Assert.That(isExpired, Is.True);
    }

    [Test]
    public void IsCartExpired_UserCart_WithinExpirationPeriod_ReturnsFalse()
    {
        // Arrange
        var lastUpdatedAt = DateTime.UtcNow.AddDays(-20);

        // Act
        var isExpired = CartPolicy.IsCartExpired(lastUpdatedAt, false);

        // Assert
        Assert.That(isExpired, Is.False);
    }

    [Test]
    public void IsCartExpired_UserCart_BeyondExpirationPeriod_ReturnsTrue()
    {
        // Arrange
        var lastUpdatedAt = DateTime.UtcNow.AddDays(-35);

        // Act
        var isExpired = CartPolicy.IsCartExpired(lastUpdatedAt, false);

        // Assert
        Assert.That(isExpired, Is.True);
    }

    [Test]
    public void IsValidCartValue_WithPositiveValue_ReturnsTrue()
    {
        // Act
        var isValid = CartPolicy.IsValidCartValue(100m);

        // Assert
        Assert.That(isValid, Is.True);
    }

    [Test]
    public void IsValidCartValue_WithZeroValue_ReturnsFalse()
    {
        // Act
        var isValid = CartPolicy.IsValidCartValue(0m);

        // Assert
        Assert.That(isValid, Is.False);
    }

    [Test]
    public void IsValidCartValue_ExceedsMaximum_ReturnsFalse()
    {
        // Act
        var isValid = CartPolicy.IsValidCartValue(1000000m);

        // Assert
        Assert.That(isValid, Is.False);
    }

    [Test]
    public void CanMergeCarts_WithinLimit_ReturnsTrue()
    {
        // Act
        var canMerge = CartPolicy.CanMergeCarts(20, 25);

        // Assert
        Assert.That(canMerge, Is.True);
    }

    [Test]
    public void CanMergeCarts_ExceedsLimit_ReturnsFalse()
    {
        // Act
        var canMerge = CartPolicy.CanMergeCarts(30, 25);

        // Assert
        Assert.That(canMerge, Is.False);
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
        Assert.That(shouldMerge, Is.True);
    }

    [Test]
    public void ShouldMergeDuplicateItems_DifferentProducts_ReturnsFalse()
    {
        // Arrange
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();

        // Act
        var shouldMerge = CartPolicy.ShouldMergeDuplicateItems(productId1, productId2, null, null);

        // Assert
        Assert.That(shouldMerge, Is.False);
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
        Assert.That(shouldMerge, Is.False);
    }

    [Test]
    public void CalculateMergedQuantity_WithinLimit_ReturnsSummed()
    {
        // Act
        var merged = CartPolicy.CalculateMergedQuantity(5, 10);

        // Assert
        Assert.That(merged, Is.EqualTo(15));
    }

    [Test]
    public void CalculateMergedQuantity_ExceedsMaximum_ReturnsMaximum()
    {
        // Act
        var merged = CartPolicy.CalculateMergedQuantity(60, 50);

        // Assert
        Assert.That(merged, Is.EqualTo(99));
    }

    [Test]
    public void IsCartEmpty_WithZeroItems_ReturnsTrue()
    {
        // Act
        var isEmpty = CartPolicy.IsCartEmpty(0);

        // Assert
        Assert.That(isEmpty, Is.True);
    }

    [Test]
    public void IsCartEmpty_WithItems_ReturnsFalse()
    {
        // Act
        var isEmpty = CartPolicy.IsCartEmpty(5);

        // Assert
        Assert.That(isEmpty, Is.False);
    }

    [Test]
    public void CanConvertToOrder_WithValidCart_ReturnsTrue()
    {
        // Act
        var canConvert = CartPolicy.CanConvertToOrder(5, 100m, false);

        // Assert
        Assert.That(canConvert, Is.True);
    }

    [Test]
    public void CanConvertToOrder_WithEmptyCart_ReturnsFalse()
    {
        // Act
        var canConvert = CartPolicy.CanConvertToOrder(0, 100m, false);

        // Assert
        Assert.That(canConvert, Is.False);
    }

    [Test]
    public void CanConvertToOrder_WithOutOfStockItems_ReturnsFalse()
    {
        // Act
        var canConvert = CartPolicy.CanConvertToOrder(5, 100m, true);

        // Assert
        Assert.That(canConvert, Is.False);
    }

    [Test]
    public void CanConvertToOrder_WithInvalidValue_ReturnsFalse()
    {
        // Act
        var canConvert = CartPolicy.CanConvertToOrder(5, 0m, false);

        // Assert
        Assert.That(canConvert, Is.False);
    }

    [Test]
    public void RequiresStockRevalidation_RecentValidation_ReturnsFalse()
    {
        // Arrange
        var lastValidatedAt = DateTime.UtcNow.AddMinutes(-10);

        // Act
        var requiresRevalidation = CartPolicy.RequiresStockRevalidation(lastValidatedAt);

        // Assert
        Assert.That(requiresRevalidation, Is.False);
    }

    [Test]
    public void RequiresStockRevalidation_OldValidation_ReturnsTrue()
    {
        // Arrange
        var lastValidatedAt = DateTime.UtcNow.AddMinutes(-20);

        // Act
        var requiresRevalidation = CartPolicy.RequiresStockRevalidation(lastValidatedAt);

        // Assert
        Assert.That(requiresRevalidation, Is.True);
    }

    [Test]
    public void CanSaveCartForLater_Authenticated_ReturnsTrue()
    {
        // Act
        var canSave = CartPolicy.CanSaveCartForLater(true);

        // Assert
        Assert.That(canSave, Is.True);
    }

    [Test]
    public void CanSaveCartForLater_NotAuthenticated_ReturnsFalse()
    {
        // Act
        var canSave = CartPolicy.CanSaveCartForLater(false);

        // Assert
        Assert.That(canSave, Is.False);
    }

    [Test]
    public void ShouldAutoRemoveItem_InactiveProduct_ReturnsTrue()
    {
        // Act
        var shouldRemove = CartPolicy.ShouldAutoRemoveItem(false, false, 10);

        // Assert
        Assert.That(shouldRemove, Is.True);
    }

    [Test]
    public void ShouldAutoRemoveItem_DeletedProduct_ReturnsTrue()
    {
        // Act
        var shouldRemove = CartPolicy.ShouldAutoRemoveItem(true, true, 10);

        // Assert
        Assert.That(shouldRemove, Is.True);
    }

    [Test]
    public void ShouldAutoRemoveItem_NoStock_ReturnsTrue()
    {
        // Act
        var shouldRemove = CartPolicy.ShouldAutoRemoveItem(true, false, 0);

        // Assert
        Assert.That(shouldRemove, Is.True);
    }

    [Test]
    public void ShouldAutoRemoveItem_ValidProduct_ReturnsFalse()
    {
        // Act
        var shouldRemove = CartPolicy.ShouldAutoRemoveItem(true, false, 10);

        // Assert
        Assert.That(shouldRemove, Is.False);
    }

    [Test]
    public void CalculateSubtotal_WithMultipleItems_ReturnsCorrectTotal()
    {
        // Arrange
        var items = new List<(decimal unitPrice, int quantity)> { (10m, 2), (25m, 3), (50m, 1) };

        // Act
        var subtotal = CartPolicy.CalculateSubtotal(items);

        // Assert
        Assert.That(subtotal, Is.EqualTo(145m)); // (10*2) + (25*3) + (50*1) = 20 + 75 + 50
    }

    [Test]
    public void CalculateSubtotal_WithEmptyList_ReturnsZero()
    {
        // Arrange
        var items = new List<(decimal unitPrice, int quantity)>();

        // Act
        var subtotal = CartPolicy.CalculateSubtotal(items);

        // Assert
        Assert.That(subtotal, Is.EqualTo(0m));
    }

    [Test]
    public void AllowsMultipleVariants_WithVariants_ReturnsTrue()
    {
        // Act
        var allows = CartPolicy.AllowsMultipleVariants(true);

        // Assert
        Assert.That(allows, Is.True);
    }

    [Test]
    public void AllowsMultipleVariants_WithoutVariants_ReturnsFalse()
    {
        // Act
        var allows = CartPolicy.AllowsMultipleVariants(false);

        // Assert
        Assert.That(allows, Is.False);
    }

    [Test]
    public void MeetsMinimumOrderValue_AboveMinimum_ReturnsTrue()
    {
        // Act
        var meets = CartPolicy.MeetsMinimumOrderValue(100m, 50m);

        // Assert
        Assert.That(meets, Is.True);
    }

    [Test]
    public void MeetsMinimumOrderValue_BelowMinimum_ReturnsFalse()
    {
        // Act
        var meets = CartPolicy.MeetsMinimumOrderValue(30m, 50m);

        // Assert
        Assert.That(meets, Is.False);
    }

    [Test]
    public void MeetsMinimumOrderValue_NoMinimum_ReturnsTrue()
    {
        // Act
        var meets = CartPolicy.MeetsMinimumOrderValue(10m, 0m);

        // Assert
        Assert.That(meets, Is.True);
    }
}
