using Comex.Application.Interfaces;
using Comex.Application.Services;

namespace Comex.Tests.Application.Services;

/// <summary>
/// Tests for SecurityAuditService
/// </summary>
[TestFixture]
public class SecurityAuditServiceTests : BaseTestFixture
{
    private Mock<ILoggingService> _mockLogger;
    private SecurityAuditService _service;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        _mockLogger = new Mock<ILoggingService>();
        _service = new SecurityAuditService(_mockLogger.Object);
    }

    [Test]
    public void Constructor_WithNullLogger_Throws()
    {
        // Act
        Action act = () => new SecurityAuditService(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Test]
    public void Record_AddsEventToBuffer()
    {
        // Act
        _service.Record("Authentication", "Warning", "Login failed");

        // Assert
        var events = _service.GetRecent(10);
        events.Should().HaveCount(1);
        events[0].Category.Should().Be("Authentication");
        events[0].Severity.Should().Be("Warning");
        events[0].Message.Should().Be("Login failed");
    }

    [Test]
    public void Record_LogsWarningForWarningSeverity()
    {
        // Act
        _service.Record("Waf", "Warning", "Request blocked");

        // Assert
        _mockLogger.Verify(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Test]
    public void Record_LogsInformationForDefaultSeverity()
    {
        // Act
        _service.Record("Backup", "Info", "Backup completed");

        // Assert
        _mockLogger.Verify(
            x => x.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()),
            Times.Once
        );
    }

    [Test]
    public void GetRecent_ReturnsNewestFirst()
    {
        // Arrange
        _service.Record("Category", "Info", "First");
        _service.Record("Category", "Info", "Second");

        // Act
        var events = _service.GetRecent(10);

        // Assert
        events.Should().HaveCount(2);
        events[0].Message.Should().Be("Second");
        events[1].Message.Should().Be("First");
    }

    [Test]
    public void GetRecent_WithCountLimitsResult()
    {
        // Arrange
        for (var i = 0; i < 5; i++)
        {
            _service.Record("Category", "Info", $"Event {i}");
        }

        // Act
        var events = _service.GetRecent(2);

        // Assert
        events.Should().HaveCount(2);
        events[0].Message.Should().Be("Event 4");
    }

    [Test]
    public void GetRecent_WithNonPositiveCount_ReturnsEmpty()
    {
        // Arrange
        _service.Record("Category", "Info", "Event");

        // Act
        var events = _service.GetRecent(0);

        // Assert
        events.Should().BeEmpty();
    }

    [Test]
    public void Record_PreservesIpAndUserAgent()
    {
        // Act
        _service.Record(
            "Category",
            "Warning",
            "Blocked",
            ipAddress: "10.0.0.1",
            userAgent: "sqlmap",
            path: "/api/v1/login"
        );

        // Assert
        var events = _service.GetRecent(1);
        events[0].IpAddress.Should().Be("10.0.0.1");
        events[0].UserAgent.Should().Be("sqlmap");
        events[0].Path.Should().Be("/api/v1/login");
    }
}
