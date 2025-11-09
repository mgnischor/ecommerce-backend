namespace ECommerce.Tests.Domain.Services;

/// <summary>
/// Tests for CartManagementService domain service
/// </summary>
[TestFixture]
public class CartManagementServiceTests : BaseTestFixture
{
    [Test]
    public void ValidateAddToCart_WithInactiveProduct_ReturnsError()
    {
        // Arrange
        var product = new ProductEntity
        {
            Id = Guid.NewGuid(),
            IsActive = false,
            IsDeleted = false,
        };

        // Act
        var (isValid, errorMessage) = CartManagementService.ValidateAddToCart(product, 1);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Be("Product is not available");
    }

    [Test]
    public void ValidateAddToCart_WithDeletedProduct_ReturnsError()
    {
        // Arrange
        var product = new ProductEntity
        {
            Id = Guid.NewGuid(),
            IsActive = true,
            IsDeleted = true,
        };

        // Act
        var (isValid, errorMessage) = CartManagementService.ValidateAddToCart(product, 1);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Be("Product not found");
    }

    [Test]
    public void ValidateAddToCart_WithZeroQuantity_ReturnsError()
    {
        // Arrange
        var product = new ProductEntity
        {
            Id = Guid.NewGuid(),
            IsActive = true,
            IsDeleted = false,
        };

        // Act
        var (isValid, errorMessage) = CartManagementService.ValidateAddToCart(product, 0);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Be("Quantity must be greater than zero");
    }

    [Test]
    public void ValidateAddToCart_WithNegativeQuantity_ReturnsError()
    {
        // Arrange
        var product = new ProductEntity
        {
            Id = Guid.NewGuid(),
            IsActive = true,
            IsDeleted = false,
        };

        // Act
        var (isValid, errorMessage) = CartManagementService.ValidateAddToCart(product, -5);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Be("Quantity must be greater than zero");
    }

    [Test]
    public void ValidateAddToCart_WithExcessiveQuantity_ReturnsError()
    {
        // Arrange
        var product = new ProductEntity
        {
            Id = Guid.NewGuid(),
            IsActive = true,
            IsDeleted = false,
        };

        // Act
        var (isValid, errorMessage) = CartManagementService.ValidateAddToCart(product, 1000);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Contain("Maximum quantity");
    }

    [Test]
    public void ValidateAddToCart_WithInsufficientStock_ReturnsError()
    {
        // Arrange
        var product = new ProductEntity
        {
            Id = Guid.NewGuid(),
            IsActive = true,
            IsDeleted = false,
            StockQuantity = 5,
            MaxOrderQuantity = 100,
        };

        // Act
        var (isValid, errorMessage) = CartManagementService.ValidateAddToCart(product, 10);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Be("Insufficient stock available");
    }

    [Test]
    public void ValidateAddToCart_ExceedsMaxOrderQuantity_ReturnsError()
    {
        // Arrange
        var product = new ProductEntity
        {
            Id = Guid.NewGuid(),
            IsActive = true,
            IsDeleted = false,
            StockQuantity = 100,
            MaxOrderQuantity = 10,
        };

        // Act
        var (isValid, errorMessage) = CartManagementService.ValidateAddToCart(product, 15);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Contain("Maximum order quantity");
    }

    [Test]
    public void ValidateAddToCart_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var product = new ProductEntity
        {
            Id = Guid.NewGuid(),
            IsActive = true,
            IsDeleted = false,
            StockQuantity = 50,
            MaxOrderQuantity = 20,
        };

        // Act
        var (isValid, errorMessage) = CartManagementService.ValidateAddToCart(product, 5);

        // Assert
        isValid.Should().BeTrue();
        errorMessage.Should().BeNull();
    }

    [Test]
    public void ValidateCartForCheckout_WithEmptyCart_ReturnsError()
    {
        // Arrange
        var cartItems = new List<(ProductEntity product, int quantity)>();

        // Act
        var (isValid, errorMessage) = CartManagementService.ValidateCartForCheckout(cartItems);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Be("Cart is empty");
    }

    [Test]
    public void ValidateCartForCheckout_WithNullCart_ReturnsError()
    {
        // Act
        var (isValid, errorMessage) = CartManagementService.ValidateCartForCheckout(null!);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Be("Cart is empty");
    }

    [Test]
    public void ValidateCartForCheckout_WithInvalidItem_ReturnsError()
    {
        // Arrange
        var cartItems = new List<(ProductEntity product, int quantity)>
        {
            (
                new ProductEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Product A",
                    IsActive = false,
                    IsDeleted = false,
                },
                2
            ),
        };

        // Act
        var (isValid, errorMessage) = CartManagementService.ValidateCartForCheckout(cartItems);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Contain("Product A");
    }

    [Test]
    public void ValidateCartForCheckout_WithValidCart_ReturnsSuccess()
    {
        // Arrange
        var cartItems = new List<(ProductEntity product, int quantity)>
        {
            (
                new ProductEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Product A",
                    IsActive = true,
                    IsDeleted = false,
                    StockQuantity = 20,
                    MaxOrderQuantity = 10,
                },
                2
            ),
        };

        // Act
        var (isValid, errorMessage) = CartManagementService.ValidateCartForCheckout(cartItems);

        // Assert
        isValid.Should().BeTrue();
        errorMessage.Should().BeNull();
    }

    [Test]
    public void CalculateCartExpiration_ReturnsCorrectDate()
    {
        // Act
        var expirationDate = CartManagementService.CalculateCartExpiration();

        // Assert
        expirationDate.Should().BeCloseTo(DateTime.UtcNow.AddDays(30), TimeSpan.FromSeconds(5));
    }

    [Test]
    public void IsCartExpired_WithNullDate_ReturnsFalse()
    {
        // Act
        var isExpired = CartManagementService.IsCartExpired(null);

        // Assert
        isExpired.Should().BeFalse();
    }

    [Test]
    public void IsCartExpired_WithFutureDate_ReturnsFalse()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(10);

        // Act
        var isExpired = CartManagementService.IsCartExpired(futureDate);

        // Assert
        isExpired.Should().BeFalse();
    }

    [Test]
    public void IsCartExpired_WithPastDate_ReturnsTrue()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddDays(-10);

        // Act
        var isExpired = CartManagementService.IsCartExpired(pastDate);

        // Assert
        isExpired.Should().BeTrue();
    }

    [Test]
    public void MergeCarts_WithEmptyCarts_ReturnsEmptyList()
    {
        // Arrange
        var anonymousCart = new List<(Guid productId, int quantity)>();
        var userCart = new List<(Guid productId, int quantity)>();

        // Act
        var merged = CartManagementService.MergeCarts(anonymousCart, userCart);

        // Assert
        merged.Should().BeEmpty();
    }

    [Test]
    public void MergeCarts_WithNoOverlap_ReturnsCombinedItems()
    {
        // Arrange
        var product1 = Guid.NewGuid();
        var product2 = Guid.NewGuid();
        var anonymousCart = new List<(Guid productId, int quantity)> { (product1, 2) };
        var userCart = new List<(Guid productId, int quantity)> { (product2, 3) };

        // Act
        var merged = CartManagementService.MergeCarts(anonymousCart, userCart);

        // Assert
        merged.Should().HaveCount(2);
        merged.Should().Contain((product1, 2));
        merged.Should().Contain((product2, 3));
    }

    [Test]
    public void MergeCarts_WithOverlappingProducts_AddsQuantities()
    {
        // Arrange
        var product1 = Guid.NewGuid();
        var anonymousCart = new List<(Guid productId, int quantity)> { (product1, 2) };
        var userCart = new List<(Guid productId, int quantity)> { (product1, 3) };

        // Act
        var merged = CartManagementService.MergeCarts(anonymousCart, userCart);

        // Assert
        merged.Should().HaveCount(1);
        merged.Should().Contain((product1, 5));
    }

    [Test]
    public void CalculateTotalWeight_WithEmptyCart_ReturnsZero()
    {
        // Arrange
        var cartItems = new List<(ProductEntity product, int quantity)>();

        // Act
        var totalWeight = CartManagementService.CalculateTotalWeight(cartItems);

        // Assert
        totalWeight.Should().Be(0);
    }

    [Test]
    public void CalculateTotalWeight_WithMultipleItems_ReturnsCorrectTotal()
    {
        // Arrange
        var cartItems = new List<(ProductEntity product, int quantity)>
        {
            (new ProductEntity { Weight = 1.5m }, 2),
            (new ProductEntity { Weight = 0.5m }, 3),
        };

        // Act
        var totalWeight = CartManagementService.CalculateTotalWeight(cartItems);

        // Assert
        totalWeight.Should().Be(4.5m); // (1.5 * 2) + (0.5 * 3) = 3 + 1.5 = 4.5
    }

    [Test]
    public void QualifiesForFreeShipping_BelowThreshold_ReturnsFalse()
    {
        // Arrange
        var cartSubtotal = 50m;

        // Act
        var qualifies = CartManagementService.QualifiesForFreeShipping(cartSubtotal, 100m);

        // Assert
        qualifies.Should().BeFalse();
    }

    [Test]
    public void QualifiesForFreeShipping_AtThreshold_ReturnsTrue()
    {
        // Arrange
        var cartSubtotal = 100m;

        // Act
        var qualifies = CartManagementService.QualifiesForFreeShipping(cartSubtotal, 100m);

        // Assert
        qualifies.Should().BeTrue();
    }

    [Test]
    public void QualifiesForFreeShipping_AboveThreshold_ReturnsTrue()
    {
        // Arrange
        var cartSubtotal = 150m;

        // Act
        var qualifies = CartManagementService.QualifiesForFreeShipping(cartSubtotal, 100m);

        // Assert
        qualifies.Should().BeTrue();
    }

    [Test]
    public void AmountNeededForFreeShipping_BelowThreshold_ReturnsCorrectAmount()
    {
        // Arrange
        var cartSubtotal = 75m;

        // Act
        var amountNeeded = CartManagementService.AmountNeededForFreeShipping(cartSubtotal, 100m);

        // Assert
        amountNeeded.Should().Be(25m);
    }

    [Test]
    public void AmountNeededForFreeShipping_AboveThreshold_ReturnsZero()
    {
        // Arrange
        var cartSubtotal = 150m;

        // Act
        var amountNeeded = CartManagementService.AmountNeededForFreeShipping(cartSubtotal, 100m);

        // Assert
        amountNeeded.Should().Be(0);
    }

    [Test]
    public void SuggestProductsForFreeShipping_WithNoProducts_ReturnsEmpty()
    {
        // Arrange
        var availableProducts = new List<ProductEntity>();
        var amountNeeded = 25m;

        // Act
        var suggestions = CartManagementService.SuggestProductsForFreeShipping(
            availableProducts,
            amountNeeded
        );

        // Assert
        suggestions.Should().BeEmpty();
    }

    [Test]
    public void SuggestProductsForFreeShipping_FiltersInactiveProducts()
    {
        // Arrange
        var availableProducts = new List<ProductEntity>
        {
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                IsActive = false,
                Price = 20m,
                StockQuantity = 10,
            },
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                IsActive = true,
                IsDeleted = true,
                Price = 20m,
                StockQuantity = 10,
            },
        };
        var amountNeeded = 25m;

        // Act
        var suggestions = CartManagementService.SuggestProductsForFreeShipping(
            availableProducts,
            amountNeeded
        );

        // Assert
        suggestions.Should().BeEmpty();
    }

    [Test]
    public void SuggestProductsForFreeShipping_ReturnsRelevantProducts()
    {
        // Arrange
        var availableProducts = new List<ProductEntity>
        {
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                IsActive = true,
                IsDeleted = false,
                Price = 25m,
                StockQuantity = 10,
            },
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                IsActive = true,
                IsDeleted = false,
                Price = 100m,
                StockQuantity = 5,
            },
        };
        var amountNeeded = 25m;

        // Act
        var suggestions = CartManagementService.SuggestProductsForFreeShipping(
            availableProducts,
            amountNeeded
        );

        // Assert
        suggestions.Should().NotBeEmpty();
        suggestions.Should().Contain(p => p.Price == 25m);
    }
}
