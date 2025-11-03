using ECommerce.Domain.Enums;

namespace ECommerce.Tests.Domain.Entities;

/// <summary>
/// Tests for ProductEntity
/// </summary>
[TestFixture]
public class ProductEntityTests : BaseTestFixture
{
    [Test]
    public void ProductEntity_Creation_SetsDefaultValues()
    {
        // Act
        var product = new ProductEntity();

        // Assert
        product.Category.Should().Be(ProductCategory.General);
        product.Status.Should().Be(ProductStatus.Active);
        product.IsActive.Should().BeTrue();
        product.IsDeleted.Should().BeFalse();
        product.IsFeatured.Should().BeFalse();
        product.IsOnSale.Should().BeFalse();
        product.Tags.Should().NotBeNull();
        product.Images.Should().NotBeNull();
    }

    [Test]
    public void ProductEntity_SetPrice_UpdatesCorrectly()
    {
        // Arrange
        var product = new ProductEntity();
        var expectedPrice = 99.99m;

        // Act
        product.Price = expectedPrice;

        // Assert
        product.Price.Should().Be(expectedPrice);
    }

    [Test]
    public void ProductEntity_SetDiscountPrice_UpdatesCorrectly()
    {
        // Arrange
        var product = new ProductEntity { Price = 100m };
        var discountPrice = 80m;

        // Act
        product.DiscountPrice = discountPrice;

        // Assert
        product.DiscountPrice.Should().Be(discountPrice);
    }

    [Test]
    public void ProductEntity_SetStockQuantity_UpdatesCorrectly()
    {
        // Arrange
        var product = new ProductEntity();
        var stockQuantity = 100;

        // Act
        product.StockQuantity = stockQuantity;

        // Assert
        product.StockQuantity.Should().Be(stockQuantity);
    }

    [Test]
    public void ProductEntity_SetCategory_UpdatesCorrectly()
    {
        // Arrange
        var product = new ProductEntity();

        // Act
        product.Category = ProductCategory.Electronics;

        // Assert
        product.Category.Should().Be(ProductCategory.Electronics);
    }

    [Test]
    public void ProductEntity_SetStatus_UpdatesCorrectly()
    {
        // Arrange
        var product = new ProductEntity();

        // Act
        product.Status = ProductStatus.Discontinued;

        // Assert
        product.Status.Should().Be(ProductStatus.Discontinued);
    }

    [Test]
    public void ProductEntity_CanBeMarkedAsFeatured()
    {
        // Arrange
        var product = new ProductEntity();

        // Act
        product.IsFeatured = true;

        // Assert
        product.IsFeatured.Should().BeTrue();
    }

    [Test]
    public void ProductEntity_CanBeMarkedAsOnSale()
    {
        // Arrange
        var product = new ProductEntity();

        // Act
        product.IsOnSale = true;

        // Assert
        product.IsOnSale.Should().BeTrue();
    }

    [Test]
    public void ProductEntity_CanAddTags()
    {
        // Arrange
        var product = new ProductEntity();

        // Act
        product.Tags.Add("new");
        product.Tags.Add("featured");

        // Assert
        product.Tags.Should().HaveCount(2);
        product.Tags.Should().Contain(new[] { "new", "featured" });
    }

    [Test]
    public void ProductEntity_CanSoftDelete()
    {
        // Arrange
        var product = new ProductEntity { IsDeleted = false };

        // Act
        product.IsDeleted = true;

        // Assert
        product.IsDeleted.Should().BeTrue();
    }

    [Test]
    public void ProductEntity_HasTimestamps()
    {
        // Arrange & Act
        var product = new ProductEntity
        {
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        // Assert
        product.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        product.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}
