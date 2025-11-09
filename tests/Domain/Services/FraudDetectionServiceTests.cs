using ECommerce.Domain.Enums;

namespace ECommerce.Tests.Domain.Services;

/// <summary>
/// Tests for FraudDetectionService domain service
/// </summary>
[TestFixture]
public class FraudDetectionServiceTests : BaseTestFixture
{
    [Test]
    public void CalculateFraudRiskScore_WithLowRiskOrder_ReturnsLowScore()
    {
        // Arrange
        var order = new OrderEntity
        {
            TotalAmount = 100m,
            ShippingMethod = ShippingMethod.Standard,
            BillingAddressId = Guid.NewGuid(),
            ShippingAddressId = Guid.NewGuid(),
        };
        var customer = new UserEntity { IsEmailVerified = true };

        // Act
        var score = FraudDetectionService.CalculateFraudRiskScore(order, customer, 1, 0, false);

        // Assert
        score.Should().BeLessThan(30);
    }

    [Test]
    public void CalculateFraudRiskScore_ExpeditedHighValue_IncreasesScore()
    {
        // Arrange
        var order = new OrderEntity
        {
            TotalAmount = 3000m,
            ShippingMethod = ShippingMethod.NextDay,
        };
        var customer = new UserEntity { IsEmailVerified = true };

        // Act
        var score = FraudDetectionService.CalculateFraudRiskScore(order, customer, 1, 0, false);

        // Assert - TotalAmount > 2000 and NextDay shipping adds +10 points
        score.Should().Be(10);
    }

    [Test]
    public void CalculateFraudRiskScore_WithTooManyOrders_IncreasesScore()
    {
        // Arrange
        var order = new OrderEntity
        {
            TotalAmount = 100m,
            ShippingMethod = ShippingMethod.Standard,
        };
        var customer = new UserEntity { IsEmailVerified = true };

        // Act - ordersInLast24Hours must be > 10 to trigger the +30 risk score
        var score = FraudDetectionService.CalculateFraudRiskScore(order, customer, 15, 0, false);

        // Assert
        score.Should().BeGreaterThanOrEqualTo(25);
    }

    [Test]
    public void CalculateFraudRiskScore_NewCustomerHighValue_IncreasesScore()
    {
        // Arrange
        var order = new OrderEntity
        {
            TotalAmount = 2000m,
            ShippingMethod = ShippingMethod.Standard,
        };
        var customer = new UserEntity { IsEmailVerified = true };

        // Act
        var score = FraudDetectionService.CalculateFraudRiskScore(order, customer, 1, 0, true);

        // Assert
        score.Should().BeGreaterThanOrEqualTo(25);
    }

    [Test]
    public void CalculateFraudRiskScore_FrequentAddressChanges_IncreasesScore()
    {
        // Arrange
        var order = new OrderEntity
        {
            TotalAmount = 100m,
            ShippingMethod = ShippingMethod.Standard,
        };
        var customer = new UserEntity { IsEmailVerified = true };

        // Act
        var score = FraudDetectionService.CalculateFraudRiskScore(order, customer, 1, 5, false);

        // Assert
        score.Should().BeGreaterThanOrEqualTo(15);
    }

    [Test]
    public void CalculateFraudRiskScore_DifferentAddresses_IncreasesScore()
    {
        // Arrange
        var order = new OrderEntity
        {
            TotalAmount = 100m,
            ShippingMethod = ShippingMethod.Standard,
            BillingAddressId = Guid.NewGuid(),
            ShippingAddressId = Guid.NewGuid(),
        };
        var customer = new UserEntity { IsEmailVerified = true };

        // Act
        var score = FraudDetectionService.CalculateFraudRiskScore(order, customer, 1, 0, false);

        // Assert - different addresses adds +10 points
        score.Should().Be(10);
    }

    [Test]
    public void CalculateFraudRiskScore_EmailNotVerified_IncreasesScore()
    {
        // Arrange
        var order = new OrderEntity
        {
            TotalAmount = 100m,
            ShippingMethod = ShippingMethod.Standard,
        };
        var customer = new UserEntity { IsEmailVerified = false };

        // Act
        var score = FraudDetectionService.CalculateFraudRiskScore(order, customer, 1, 0, false);

        // Assert
        score.Should().BeGreaterThanOrEqualTo(15);
    }

    [Test]
    public void CalculateFraudRiskScore_MaxScore_DoesNotExceed100()
    {
        // Arrange - All risk factors present
        var order = new OrderEntity
        {
            TotalAmount = 10000m,
            ShippingMethod = ShippingMethod.SameDay,
            BillingAddressId = Guid.NewGuid(),
            ShippingAddressId = Guid.NewGuid(),
        };
        var customer = new UserEntity { IsEmailVerified = false };

        // Act
        var score = FraudDetectionService.CalculateFraudRiskScore(order, customer, 20, 10, true);

        // Assert
        score.Should().BeLessThanOrEqualTo(100);
    }

    [Test]
    public void ShouldFlagForReview_WithHighRisk_ReturnsTrue()
    {
        // Arrange
        var riskScore = 60;

        // Act
        var shouldFlag = FraudDetectionService.ShouldFlagForReview(riskScore);

        // Assert
        shouldFlag.Should().BeTrue();
    }

    [Test]
    public void ShouldFlagForReview_WithLowRisk_ReturnsFalse()
    {
        // Arrange
        var riskScore = 30;

        // Act
        var shouldFlag = FraudDetectionService.ShouldFlagForReview(riskScore);

        // Assert
        shouldFlag.Should().BeFalse();
    }

    [Test]
    public void ShouldFlagForReview_AtThreshold_ReturnsTrue()
    {
        // Arrange
        var riskScore = 50;

        // Act
        var shouldFlag = FraudDetectionService.ShouldFlagForReview(riskScore);

        // Assert
        shouldFlag.Should().BeTrue();
    }

    [Test]
    public void ShouldAutoReject_WithVeryHighRisk_ReturnsTrue()
    {
        // Arrange
        var riskScore = 85;

        // Act
        var shouldReject = FraudDetectionService.ShouldAutoReject(riskScore);

        // Assert
        shouldReject.Should().BeTrue();
    }

    [Test]
    public void ShouldAutoReject_WithMediumRisk_ReturnsFalse()
    {
        // Arrange
        var riskScore = 60;

        // Act
        var shouldReject = FraudDetectionService.ShouldAutoReject(riskScore);

        // Assert
        shouldReject.Should().BeFalse();
    }

    [Test]
    public void ShouldAutoReject_AtThreshold_ReturnsTrue()
    {
        // Arrange
        var riskScore = 80;

        // Act
        var shouldReject = FraudDetectionService.ShouldAutoReject(riskScore);

        // Assert
        shouldReject.Should().BeTrue();
    }

    [Test]
    public void GetRiskLevelDescription_VeryHighRisk_ReturnsCorrectDescription()
    {
        // Act
        var description = FraudDetectionService.GetRiskLevelDescription(85);

        // Assert
        description.Should().Be("Very High Risk");
    }

    [Test]
    public void GetRiskLevelDescription_HighRisk_ReturnsCorrectDescription()
    {
        // Act
        var description = FraudDetectionService.GetRiskLevelDescription(60);

        // Assert
        description.Should().Be("High Risk");
    }

    [Test]
    public void GetRiskLevelDescription_MediumRisk_ReturnsCorrectDescription()
    {
        // Act
        var description = FraudDetectionService.GetRiskLevelDescription(40);

        // Assert
        description.Should().Be("Medium Risk");
    }

    [Test]
    public void GetRiskLevelDescription_LowRisk_ReturnsCorrectDescription()
    {
        // Act
        var description = FraudDetectionService.GetRiskLevelDescription(15);

        // Assert
        description.Should().Be("Low Risk");
    }

    [Test]
    public void GetRiskLevelDescription_MinimalRisk_ReturnsCorrectDescription()
    {
        // Act
        var description = FraudDetectionService.GetRiskLevelDescription(5);

        // Assert
        description.Should().Be("Minimal Risk");
    }

    [Test]
    public void ValidateAddressConsistency_MatchingCountry_ReturnsTrue()
    {
        // Arrange
        var customer = new UserEntity { City = "New York", Country = "USA" };

        // Act
        var isValid = FraudDetectionService.ValidateAddressConsistency(
            customer,
            "Los Angeles",
            "USA"
        );

        // Assert
        isValid.Should().BeTrue();
    }

    [Test]
    public void ValidateAddressConsistency_DifferentCountry_ReturnsFalse()
    {
        // Arrange
        var customer = new UserEntity { City = "New York", Country = "USA" };

        // Act
        var isValid = FraudDetectionService.ValidateAddressConsistency(
            customer,
            "Toronto",
            "Canada"
        );

        // Assert
        isValid.Should().BeFalse();
    }

    [Test]
    public void ValidateAddressConsistency_NoProfileData_ReturnsTrue()
    {
        // Arrange
        var customer = new UserEntity { City = null, Country = null };

        // Act
        var isValid = FraudDetectionService.ValidateAddressConsistency(
            customer,
            "Los Angeles",
            "USA"
        );

        // Assert
        isValid.Should().BeTrue();
    }

    [Test]
    public void ValidateAddressConsistency_CaseInsensitive_ReturnsTrue()
    {
        // Arrange
        var customer = new UserEntity { City = "New York", Country = "usa" };

        // Act
        var isValid = FraudDetectionService.ValidateAddressConsistency(
            customer,
            "Los Angeles",
            "USA"
        );

        // Assert
        isValid.Should().BeTrue();
    }
}
