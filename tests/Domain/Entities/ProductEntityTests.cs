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
        using (Assert.EnterMultipleScope())
        {
            Assert.That(product.Category, Is.EqualTo(ProductCategory.General));
            Assert.That(product.Status, Is.EqualTo(ProductStatus.Active));
            Assert.That(product.IsActive, Is.True);
            Assert.That(product.IsDeleted, Is.False);
            Assert.That(product.IsFeatured, Is.False);
            Assert.That(product.IsOnSale, Is.False);
            Assert.That(product.Tags, Is.Not.Null);
            Assert.That(product.Images, Is.Not.Null);
        }
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
        Assert.That(product.Price, Is.EqualTo(expectedPrice));
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
        Assert.That(product.DiscountPrice, Is.EqualTo(discountPrice));
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
        Assert.That(product.StockQuantity, Is.EqualTo(stockQuantity));
    }

    [Test]
    public void ProductEntity_SetCategory_UpdatesCorrectly()
    {
        // Arrange
        var product = new ProductEntity();

        // Act
        product.Category = ProductCategory.Electronics;

        // Assert
        Assert.That(product.Category, Is.EqualTo(ProductCategory.Electronics));
    }

    [Test]
    public void ProductEntity_SetStatus_UpdatesCorrectly()
    {
        // Arrange
        var product = new ProductEntity();

        // Act
        product.Status = ProductStatus.Discontinued;

        // Assert
        Assert.That(product.Status, Is.EqualTo(ProductStatus.Discontinued));
    }

    [Test]
    public void ProductEntity_CanBeMarkedAsFeatured()
    {
        // Arrange
        var product = new ProductEntity();

        // Act
        product.IsFeatured = true;

        // Assert
        Assert.That(product.IsFeatured, Is.True);
    }

    [Test]
    public void ProductEntity_CanBeMarkedAsOnSale()
    {
        // Arrange
        var product = new ProductEntity();

        // Act
        product.IsOnSale = true;

        // Assert
        Assert.That(product.IsOnSale, Is.True);
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
        using (Assert.EnterMultipleScope())
        {
            Assert.That(product.Tags, Has.Count.EqualTo(2));
            Assert.That(product.Tags, Does.Contain("new"));
            Assert.That(product.Tags, Does.Contain("featured"));
        }
    }

    [Test]
    public void ProductEntity_CanSoftDelete()
    {
        // Arrange
        var product = new ProductEntity { IsDeleted = false };

        // Act
        product.IsDeleted = true;

        // Assert
        Assert.That(product.IsDeleted, Is.True);
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
        using (Assert.EnterMultipleScope())
        {
            Assert.That(
                product.CreatedAt,
                Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(1))
            );
            Assert.That(
                product.UpdatedAt,
                Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(1))
            );
        }
    }
}
