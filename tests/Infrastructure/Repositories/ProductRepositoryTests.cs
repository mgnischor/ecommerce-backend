using ECommerce.Domain.Enums;

namespace ECommerce.Tests.Infrastructure.Repositories;

/// <summary>
/// Tests for ProductRepository
/// </summary>
[TestFixture]
public class ProductRepositoryTests : DatabaseTestFixture
{
    private ProductRepository _repository;
    private Mock<ILoggingService> _mockLogger;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        _mockLogger = new Mock<ILoggingService>();
        _repository = new ProductRepository(DbContext!, _mockLogger.Object);
    }

    [Test]
    public async Task GetByIdAsync_WithExistingProduct_ReturnsProduct()
    {
        // Arrange
        var product = new ProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Sku = "TEST-001",
            Price = 99.99m,
            StockQuantity = 10,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        await DbContext!.Products.AddAsync(product);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(product.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(product.Id);
        result.Name.Should().Be("Test Product");
    }

    [Test]
    public async Task GetByIdAsync_WithNonExistingProduct_ReturnsNull()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistingId);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task GetBySkuAsync_WithExistingSku_ReturnsProduct()
    {
        // Arrange
        var product = new ProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Sku = "UNIQUE-SKU-001",
            Price = 99.99m,
            StockQuantity = 10,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        await DbContext!.Products.AddAsync(product);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetBySkuAsync("UNIQUE-SKU-001");

        // Assert
        result.Should().NotBeNull();
        result.Sku.Should().Be("UNIQUE-SKU-001");
    }

    [Test]
    public async Task GetAllAsync_ReturnsAllNonDeletedProducts()
    {
        // Arrange
        var products = new[]
        {
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Product 1",
                Sku = "SKU-001",
                Price = 10m,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Product 2",
                Sku = "SKU-002",
                Price = 20m,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Product 3",
                Sku = "SKU-003",
                Price = 30m,
                IsDeleted = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
        };
        await DbContext!.Products.AddRangeAsync(products);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().NotContain(p => p.IsDeleted);
    }

    [Test]
    public async Task GetPagedAsync_ReturnsCorrectPage()
    {
        // Arrange
        var products = Enumerable
            .Range(1, 25)
            .Select(i => new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = $"Product {i}",
                Sku = $"SKU-{i:D3}",
                Price = i * 10m,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-i),
                UpdatedAt = DateTime.UtcNow,
            })
            .ToArray();
        await DbContext!.Products.AddRangeAsync(products);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetPagedAsync(pageNumber: 2, pageSize: 10);

        // Assert
        result.Should().HaveCount(10);
    }

    [Test]
    public async Task GetByCategoryAsync_ReturnsProductsInCategory()
    {
        // Arrange
        var products = new[]
        {
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Electronics Product",
                Sku = "ELEC-001",
                Price = 100m,
                Category = ProductCategory.Electronics,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Clothing Product",
                Sku = "CLOTH-001",
                Price = 50m,
                Category = ProductCategory.Clothing,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
        };
        await DbContext!.Products.AddRangeAsync(products);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByCategoryAsync((int)ProductCategory.Electronics);

        // Assert
        result.Should().HaveCount(1);
        result.First().Category.Should().Be(ProductCategory.Electronics);
    }

    [Test]
    public async Task GetFeaturedAsync_ReturnsOnlyFeaturedProducts()
    {
        // Arrange
        var products = new[]
        {
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Featured Product",
                Sku = "FEAT-001",
                Price = 100m,
                IsFeatured = true,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Regular Product",
                Sku = "REG-001",
                Price = 50m,
                IsFeatured = false,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
        };
        await DbContext!.Products.AddRangeAsync(products);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetFeaturedAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().IsFeatured.Should().BeTrue();
    }

    [Test]
    public async Task GetOnSaleAsync_ReturnsOnlyOnSaleProducts()
    {
        // Arrange
        var products = new[]
        {
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Sale Product",
                Sku = "SALE-001",
                Price = 100m,
                IsOnSale = true,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Regular Product",
                Sku = "REG-002",
                Price = 50m,
                IsOnSale = false,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
        };
        await DbContext!.Products.AddRangeAsync(products);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetOnSaleAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().IsOnSale.Should().BeTrue();
    }

    [Test]
    public async Task SearchByNameAsync_ReturnsMatchingProducts()
    {
        // Arrange
        var products = new[]
        {
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Wireless Mouse",
                Sku = "MOUSE-001",
                Price = 25m,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Wireless Keyboard",
                Sku = "KEY-001",
                Price = 50m,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "USB Cable",
                Sku = "CABLE-001",
                Price = 10m,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
        };
        await DbContext!.Products.AddRangeAsync(products);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.SearchByNameAsync("Wireless");

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(p => p.Name.Should().Contain("Wireless"));
    }

    [Test]
    public async Task GetCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        var products = Enumerable
            .Range(1, 15)
            .Select(i => new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = $"Product {i}",
                Sku = $"SKU-{i:D3}",
                Price = i * 10m,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            })
            .ToArray();
        await DbContext!.Products.AddRangeAsync(products);
        await DbContext.SaveChangesAsync();

        // Act
        var count = await _repository.GetCountAsync();

        // Assert
        count.Should().Be(15);
    }

    [Test]
    public async Task ExistsBySkuAsync_WithExistingSku_ReturnsTrue()
    {
        // Arrange
        var product = new ProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Sku = "EXIST-001",
            Price = 99.99m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        await DbContext!.Products.AddAsync(product);
        await DbContext.SaveChangesAsync();

        // Act
        var exists = await _repository.ExistsBySkuAsync("EXIST-001");

        // Assert
        exists.Should().BeTrue();
    }

    [Test]
    public async Task ExistsBySkuAsync_WithNonExistingSku_ReturnsFalse()
    {
        // Act
        var exists = await _repository.ExistsBySkuAsync("NONEXIST-001");

        // Assert
        exists.Should().BeFalse();
    }

    [Test]
    public async Task AddAsync_WithValidProduct_AddsSuccessfully()
    {
        // Arrange
        var product = new ProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "New Product",
            Sku = "NEW-001",
            Price = 99.99m,
            StockQuantity = 50,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        // Act
        await _repository.AddAsync(product);
        await DbContext!.SaveChangesAsync();

        // Assert
        var savedProduct = await DbContext.Products.FindAsync(product.Id);
        savedProduct.Should().NotBeNull();
        savedProduct.Name.Should().Be("New Product");
    }

    [Test]
    public void AddAsync_WithNullProduct_ThrowsException()
    {
        // Act
        Func<Task> act = async () => await _repository.AddAsync(null!);

        // Assert
        act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public async Task Update_WithValidProduct_UpdatesSuccessfully()
    {
        // Arrange
        var product = new ProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Original Name",
            Sku = "UPD-001",
            Price = 99.99m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        await DbContext!.Products.AddAsync(product);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        // Act
        product.Name = "Updated Name";
        _repository.Update(product);
        await DbContext.SaveChangesAsync();

        // Assert
        var updatedProduct = await DbContext.Products.FindAsync(product.Id);
        updatedProduct!.Name.Should().Be("Updated Name");
    }

    [Test]
    public void Update_WithNullProduct_ThrowsException()
    {
        // Act
        Action act = () => _repository.Update(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public async Task Remove_WithValidProduct_RemovesSuccessfully()
    {
        // Arrange
        var product = new ProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "To Be Removed",
            Sku = "REM-001",
            Price = 99.99m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        await DbContext!.Products.AddAsync(product);
        await DbContext.SaveChangesAsync();

        // Act
        _repository.Remove(product);
        await DbContext.SaveChangesAsync();

        // Assert
        var removedProduct = await DbContext.Products.FindAsync(product.Id);
        removedProduct.Should().BeNull();
    }

    [Test]
    public void Remove_WithNullProduct_ThrowsException()
    {
        // Act
        Action act = () => _repository.Remove(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public async Task SoftDeleteAsync_WithExistingProduct_SetsIsDeletedTrue()
    {
        // Arrange
        var product = new ProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "To Be Soft Deleted",
            Sku = "SOFT-001",
            Price = 99.99m,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        await DbContext!.Products.AddAsync(product);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.SoftDeleteAsync(product.Id);
        await DbContext.SaveChangesAsync();

        // Assert
        result.Should().BeTrue();
        var softDeletedProduct = await DbContext.Products.FindAsync(product.Id);
        softDeletedProduct!.IsDeleted.Should().BeTrue();
    }

    [Test]
    public async Task SoftDeleteAsync_WithNonExistingProduct_ReturnsFalse()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await _repository.SoftDeleteAsync(nonExistingId);

        // Assert
        result.Should().BeFalse();
    }
}
