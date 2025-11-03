using ECommerce.API.Controllers;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Tests.API.Controllers;

/// <summary>
/// Tests for VendorController
/// </summary>
[TestFixture]
public class VendorControllerTests : DatabaseTestFixture
{
    private Mock<ILogger<VendorController>> _mockLogger;
    private VendorController _controller;
    private PostgresqlContext _context;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        _mockLogger = new Mock<ILogger<VendorController>>();
        _context = CreateInMemoryDbContext();

        _controller = new VendorController(_context, _mockLogger.Object);

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
    public async Task GetAllVendors_WithValidPagination_ReturnsOkWithVendors()
    {
        // Arrange
        var vendor1 = new VendorEntity
        {
            Id = Guid.NewGuid(),
            BusinessName = "Vendor 1",
            Email = "vendor1@example.com",
            Rating = 4.5m,
            IsDeleted = false,
        };
        var vendor2 = new VendorEntity
        {
            Id = Guid.NewGuid(),
            BusinessName = "Vendor 2",
            Email = "vendor2@example.com",
            Rating = 4.8m,
            IsDeleted = false,
        };

        _context.Vendors.Add(vendor1);
        _context.Vendors.Add(vendor2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetAllVendors(1, 10);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var vendors = okResult!.Value as List<VendorEntity>;
        vendors.Should().HaveCount(2);
    }

    [Test]
    public async Task GetAllVendors_WithInvalidPageNumber_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.GetAllVendors(0, 10);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task GetVendorById_WithExistingId_ReturnsOkWithVendor()
    {
        // Arrange
        var vendor = new VendorEntity
        {
            Id = Guid.NewGuid(),
            BusinessName = "Test Vendor",
            Email = "test@example.com",
            IsDeleted = false,
        };

        _context.Vendors.Add(vendor);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetVendorById(vendor.Id);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var returnedVendor = okResult!.Value as VendorEntity;
        returnedVendor.Should().NotBeNull();
        returnedVendor!.Id.Should().Be(vendor.Id);
    }

    [Test]
    public async Task GetVendorById_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _controller.GetVendorById(nonExistentId);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task GetFeaturedVendors_ReturnsFeaturedVendors()
    {
        // Arrange
        var featuredVendor = new VendorEntity
        {
            Id = Guid.NewGuid(),
            BusinessName = "Featured Vendor",
            Email = "featured@example.com",
            IsFeatured = true,
            Status = VendorStatus.Active,
            IsDeleted = false,
        };
        var regularVendor = new VendorEntity
        {
            Id = Guid.NewGuid(),
            BusinessName = "Regular Vendor",
            Email = "regular@example.com",
            IsFeatured = false,
            IsDeleted = false,
        };

        _context.Vendors.Add(featuredVendor);
        _context.Vendors.Add(regularVendor);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetFeaturedVendors();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var vendors = okResult!.Value as List<VendorEntity>;
        vendors.Should().HaveCount(1);
        vendors!.First().IsFeatured.Should().BeTrue();
    }

    [Test]
    public async Task CreateVendor_WithValidVendor_ReturnsCreated()
    {
        // Arrange
        var newVendor = new VendorEntity
        {
            BusinessName = "New Vendor",
            Email = "new@example.com",
            PhoneNumber = "123456789",
            Address = "123 Test St",
        };

        // Act
        var result = await _controller.CreateVendor(newVendor);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result.Result as CreatedAtActionResult;
        var createdVendor = createdResult!.Value as VendorEntity;
        createdVendor.Should().NotBeNull();
        createdVendor!.BusinessName.Should().Be(newVendor.BusinessName);
    }

    [Test]
    public async Task UpdateVendor_WithValidVendor_ReturnsNoContent()
    {
        // Arrange
        var vendor = new VendorEntity
        {
            Id = Guid.NewGuid(),
            BusinessName = "Original Vendor",
            Email = "original@example.com",
            IsDeleted = false,
        };

        _context.Vendors.Add(vendor);
        await _context.SaveChangesAsync();

        var updatedVendor = new VendorEntity
        {
            Id = vendor.Id,
            BusinessName = "Updated Vendor",
            Email = "updated@example.com",
        };

        // Act
        var result = await _controller.UpdateVendor(vendor.Id, updatedVendor);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Test]
    public async Task UpdateVendor_WithIdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var vendorId = Guid.NewGuid();
        var differentId = Guid.NewGuid();

        var updatedVendor = new VendorEntity { Id = differentId, BusinessName = "Updated Vendor" };

        // Act
        var result = await _controller.UpdateVendor(vendorId, updatedVendor);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task DeleteVendor_WithExistingId_ReturnsNoContent()
    {
        // Arrange
        var vendor = new VendorEntity
        {
            Id = Guid.NewGuid(),
            BusinessName = "Vendor to Delete",
            Email = "delete@example.com",
            IsDeleted = false,
        };

        _context.Vendors.Add(vendor);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteVendor(vendor.Id);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Test]
    public async Task DeleteVendor_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _controller.DeleteVendor(nonExistentId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
