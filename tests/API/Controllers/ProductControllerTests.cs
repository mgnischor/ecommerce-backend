using ECommerce.API.Controllers;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Tests.API.Controllers;

/// <summary>
/// Tests for ProductController
/// </summary>
[TestFixture]
public class ProductControllerTests : DatabaseTestFixture
{
    private Mock<IProductRepository> _mockProductRepository;
    private Mock<ILogger<ProductController>> _mockLogger;
    private ProductController _controller;
    private PostgresqlContext _context;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        _mockProductRepository = new Mock<IProductRepository>();
        _mockLogger = new Mock<ILogger<ProductController>>();
        _context = CreateInMemoryDbContext();

        _controller = new ProductController(
            _mockProductRepository.Object,
            _context,
            _mockLogger.Object
        );

        // Set up HttpContext for Response.Headers access
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext(),
        };
    }

    [TearDown]
    public override void TearDown()
    {
        _context?.Dispose();
        base.TearDown();
    }

    [Test]
    public async Task GetAllProducts_WithValidPagination_ReturnsOkWithProducts()
    {
        // Arrange
        int pageNumber = 1;
        int pageSize = 10;

        var products = new List<ProductEntity>
        {
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Product 1",
                Sku = "SKU001",
                Description = "Test product 1",
                Price = 99.99m,
                Category = ProductCategory.Electronics,
                StockQuantity = 10,
                IsActive = true,
            },
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Product 2",
                Sku = "SKU002",
                Description = "Test product 2",
                Price = 149.99m,
                Category = ProductCategory.Electronics,
                StockQuantity = 5,
                IsActive = true,
            },
        };

        _mockProductRepository
            .Setup(x => x.GetPagedAsync(pageNumber, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        _mockProductRepository
            .Setup(x => x.GetCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products.Count);

        // Act
        var result = await _controller.GetAllProducts(pageNumber, pageSize);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var returnedProducts = okResult!.Value as List<ProductEntity>;
        returnedProducts.Should().HaveCount(2);
    }

    [Test]
    public async Task GetAllProducts_WithInvalidPageNumber_ReturnsBadRequest()
    {
        // Arrange
        int pageNumber = 0;
        int pageSize = 10;

        // Act
        var result = await _controller.GetAllProducts(pageNumber, pageSize);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task GetAllProducts_WithInvalidPageSize_ReturnsBadRequest()
    {
        // Arrange
        int pageNumber = 1;
        int pageSize = 0;

        // Act
        var result = await _controller.GetAllProducts(pageNumber, pageSize);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task GetProductById_WithExistingId_ReturnsOkWithProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new ProductEntity
        {
            Id = productId,
            Name = "Test Product",
            Sku = "SKU001",
            Description = "Test product",
            Price = 99.99m,
            Category = ProductCategory.Electronics,
            StockQuantity = 10,
            IsActive = true,
        };

        _mockProductRepository
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _controller.GetProductById(productId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var returnedProduct = okResult!.Value as ProductEntity;
        returnedProduct.Should().NotBeNull();
        returnedProduct!.Id.Should().Be(productId);
        returnedProduct.Name.Should().Be("Test Product");
    }

    [Test]
    public async Task GetProductById_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        var productId = Guid.NewGuid();

        _mockProductRepository
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductEntity?)null);

        // Act
        var result = await _controller.GetProductById(productId);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task GetProductBySku_WithExistingSku_ReturnsOkWithProduct()
    {
        // Arrange
        var sku = "SKU001";
        var product = new ProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Sku = sku,
            Description = "Test product",
            Price = 99.99m,
            Category = ProductCategory.Electronics,
            StockQuantity = 10,
            IsActive = true,
        };

        _mockProductRepository
            .Setup(x => x.GetBySkuAsync(sku, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _controller.GetProductBySku(sku);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var returnedProduct = okResult!.Value as ProductEntity;
        returnedProduct.Should().NotBeNull();
        returnedProduct!.Sku.Should().Be(sku);
    }

    [Test]
    public async Task GetProductBySku_WithNonExistingSku_ReturnsNotFound()
    {
        // Arrange
        var sku = "NONEXISTENT";

        _mockProductRepository
            .Setup(x => x.GetBySkuAsync(sku, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductEntity?)null);

        // Act
        var result = await _controller.GetProductBySku(sku);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task GetProductsByCategory_WithValidCategory_ReturnsOkWithProducts()
    {
        // Arrange
        var category = (int)ProductCategory.Electronics;
        var products = new List<ProductEntity>
        {
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Product 1",
                Sku = "SKU001",
                Category = ProductCategory.Electronics,
                Price = 99.99m,
                IsActive = true,
            },
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Product 2",
                Sku = "SKU002",
                Category = ProductCategory.Electronics,
                Price = 149.99m,
                IsActive = true,
            },
        };

        _mockProductRepository
            .Setup(x => x.GetByCategoryAsync(category, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var result = await _controller.GetProductsByCategory(category);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var returnedProducts = okResult!.Value as List<ProductEntity>;
        returnedProducts.Should().HaveCount(2);
        returnedProducts
            .Should()
            .AllSatisfy(p => p.Category.Should().Be(ProductCategory.Electronics));
    }

    [Test]
    public async Task GetFeaturedProducts_ReturnsFeaturedProducts()
    {
        // Arrange
        var products = new List<ProductEntity>
        {
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Featured Product 1",
                Sku = "SKU001",
                Price = 99.99m,
                IsFeatured = true,
                IsActive = true,
            },
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Featured Product 2",
                Sku = "SKU002",
                Price = 149.99m,
                IsFeatured = true,
                IsActive = true,
            },
        };

        _mockProductRepository
            .Setup(x => x.GetFeaturedAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var result = await _controller.GetFeaturedProducts();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var returnedProducts = okResult!.Value as List<ProductEntity>;
        returnedProducts.Should().HaveCount(2);
        returnedProducts.Should().AllSatisfy(p => p.IsFeatured.Should().BeTrue());
    }

    [Test]
    public async Task GetOnSaleProducts_ReturnsOnSaleProducts()
    {
        // Arrange
        var products = new List<ProductEntity>
        {
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Sale Product 1",
                Sku = "SKU001",
                Price = 99.99m,
                IsOnSale = true,
                IsActive = true,
            },
        };

        _mockProductRepository
            .Setup(x => x.GetOnSaleAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var result = await _controller.GetOnSaleProducts();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var returnedProducts = okResult!.Value as List<ProductEntity>;
        returnedProducts.Should().HaveCount(1);
        returnedProducts.Should().AllSatisfy(p => p.IsOnSale.Should().BeTrue());
    }

    [Test]
    public async Task SearchProducts_WithValidSearchTerm_ReturnsMatchingProducts()
    {
        // Arrange
        var searchTerm = "laptop";
        var products = new List<ProductEntity>
        {
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Gaming Laptop",
                Sku = "SKU001",
                Price = 999.99m,
                IsActive = true,
            },
        };

        _mockProductRepository
            .Setup(x => x.SearchByNameAsync(searchTerm, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var result = await _controller.SearchProducts(searchTerm);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var returnedProducts = okResult!.Value as List<ProductEntity>;
        returnedProducts.Should().HaveCount(1);
    }

    [Test]
    public async Task SearchProducts_WithEmptySearchTerm_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.SearchProducts(string.Empty);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task CreateProduct_WithValidProduct_ReturnsCreated()
    {
        // Arrange
        var newProduct = new ProductEntity
        {
            Name = "New Product",
            Sku = "SKU999",
            Description = "New product description",
            Price = 199.99m,
            Category = ProductCategory.Electronics,
            StockQuantity = 100,
            IsActive = true,
        };

        _mockProductRepository
            .Setup(x => x.ExistsBySkuAsync(newProduct.Sku, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockProductRepository
            .Setup(x => x.AddAsync(It.IsAny<ProductEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CreateProduct(newProduct);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result.Result as CreatedAtActionResult;
        var createdProduct = createdResult!.Value as ProductEntity;
        createdProduct.Should().NotBeNull();
        createdProduct!.Name.Should().Be(newProduct.Name);
    }

    [Test]
    public async Task CreateProduct_WithDuplicateSku_ReturnsConflict()
    {
        // Arrange
        var newProduct = new ProductEntity
        {
            Name = "New Product",
            Sku = "SKU001",
            Price = 199.99m,
        };

        _mockProductRepository
            .Setup(x => x.ExistsBySkuAsync(newProduct.Sku, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.CreateProduct(newProduct);

        // Assert
        result.Result.Should().BeOfType<ConflictObjectResult>();
    }

    [Test]
    public async Task UpdateProduct_WithValidProduct_ReturnsNoContent()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var existingProduct = new ProductEntity
        {
            Id = productId,
            Name = "Existing Product",
            Sku = "SKU001",
            Price = 99.99m,
        };

        var updatedProduct = new ProductEntity
        {
            Id = productId,
            Name = "Updated Product",
            Sku = "SKU001",
            Price = 149.99m,
        };

        _mockProductRepository
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        // Act
        var result = await _controller.UpdateProduct(productId, updatedProduct);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Test]
    public async Task UpdateProduct_WithIdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var differentId = Guid.NewGuid();

        var updatedProduct = new ProductEntity { Id = differentId, Name = "Updated Product" };

        // Act
        var result = await _controller.UpdateProduct(productId, updatedProduct);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task UpdateProduct_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var updatedProduct = new ProductEntity { Id = productId, Name = "Updated Product" };

        _mockProductRepository
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductEntity?)null);

        // Act
        var result = await _controller.UpdateProduct(productId, updatedProduct);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task DeleteProduct_WithExistingId_ReturnsNoContent()
    {
        // Arrange
        var productId = Guid.NewGuid();

        _mockProductRepository
            .Setup(x => x.RemoveByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteProduct(productId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Test]
    public async Task DeleteProduct_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        var productId = Guid.NewGuid();

        _mockProductRepository
            .Setup(x => x.RemoveByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteProduct(productId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task SoftDeleteProduct_WithExistingId_ReturnsNoContent()
    {
        // Arrange
        var productId = Guid.NewGuid();

        _mockProductRepository
            .Setup(x => x.SoftDeleteAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.SoftDeleteProduct(productId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Test]
    public async Task SoftDeleteProduct_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        var productId = Guid.NewGuid();

        _mockProductRepository
            .Setup(x => x.SoftDeleteAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.SoftDeleteProduct(productId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    // Note: GetEndpoints and GetOptions tests are integration tests
    // These are metadata endpoints that don't require extensive unit testing
}
