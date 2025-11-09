namespace ECommerce.Tests.Application.Services;

/// <summary>
/// Tests for LoggingService
/// </summary>
[TestFixture]
public class LoggingServiceTests : BaseTestFixture
{
    private Mock<ILogger<LoggingServiceTests>> _mockLogger;
    private LoggingService<LoggingServiceTests> _loggingService;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        _mockLogger = new Mock<ILogger<LoggingServiceTests>>();
        _loggingService = new LoggingService<LoggingServiceTests>(_mockLogger.Object);
    }

    [Test]
    public void Constructor_WithNullLogger_ThrowsException()
    {
        // Act
        Action act = () => new LoggingService<LoggingServiceTests>(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Test]
    public void LogInformation_WithValidMessage_CallsLogger()
    {
        // Arrange
        var message = "Test information message";

        // Act
        _loggingService.LogInformation(message);

        // Assert
        _mockLogger.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Test]
    public void LogInformation_WithNullMessage_DoesNotCallLogger()
    {
        // Act
        _loggingService.LogInformation(null!);

        // Assert
        _mockLogger.Verify(
            x =>
                x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Never
        );
    }

    [Test]
    public void LogInformation_WithEmptyMessage_DoesNotCallLogger()
    {
        // Act
        _loggingService.LogInformation("");

        // Assert
        _mockLogger.Verify(
            x =>
                x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Never
        );
    }

    [Test]
    public void LogInformation_WithWhitespaceMessage_DoesNotCallLogger()
    {
        // Act
        _loggingService.LogInformation("   ");

        // Assert
        _mockLogger.Verify(
            x =>
                x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Never
        );
    }

    [Test]
    public void LogWarning_WithValidMessage_CallsLogger()
    {
        // Arrange
        var message = "Test warning message";

        // Act
        _loggingService.LogWarning(message);

        // Assert
        _mockLogger.Verify(
            x =>
                x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Test]
    public void LogWarning_WithException_CallsLogger()
    {
        // Arrange
        var message = "Test warning with exception";
        var exception = new InvalidOperationException("Test exception");

        // Act
        _loggingService.LogWarning(exception, message);

        // Assert
        _mockLogger.Verify(
            x =>
                x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Test]
    public void LogWarning_WithNullException_DoesNotCallLogger()
    {
        // Act
        _loggingService.LogWarning(null!, "Test message");

        // Assert
        _mockLogger.Verify(
            x =>
                x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Never
        );
    }

    [Test]
    public void LogError_WithValidMessage_CallsLogger()
    {
        // Arrange
        var message = "Test error message";

        // Act
        _loggingService.LogError(message);

        // Assert
        _mockLogger.Verify(
            x =>
                x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Test]
    public void LogError_WithException_CallsLogger()
    {
        // Arrange
        var message = "Test error with exception";
        var exception = new InvalidOperationException("Test exception");

        // Act
        _loggingService.LogError(exception, message);

        // Assert
        _mockLogger.Verify(
            x =>
                x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Test]
    public void LogError_WithNullException_DoesNotCallLogger()
    {
        // Act
        _loggingService.LogError(null!, "Test message");

        // Assert
        _mockLogger.Verify(
            x =>
                x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Never
        );
    }

    [Test]
    public void LogDebug_WithValidMessage_CallsLogger()
    {
        // Arrange
        var message = "Test debug message";

        // Act
        _loggingService.LogDebug(message);

        // Assert
        _mockLogger.Verify(
            x =>
                x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Test]
    public void LogCritical_WithException_CallsLogger()
    {
        // Arrange
        var message = "Test critical message";
        var exception = new InvalidOperationException("Critical exception");

        // Act
        _loggingService.LogCritical(exception, message);

        // Assert
        _mockLogger.Verify(
            x =>
                x.Log(
                    LogLevel.Critical,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Test]
    public void LogCritical_WithNullException_DoesNotCallLogger()
    {
        // Act
        _loggingService.LogCritical(null!, "Test message");

        // Assert
        _mockLogger.Verify(
            x =>
                x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Never
        );
    }

    [Test]
    public void LogInformation_WithParameters_CallsLoggerWithParameters()
    {
        // Arrange
        var message = "Test message with {Parameter1} and {Parameter2}";
        var param1 = "value1";
        var param2 = "value2";

        // Act
        _loggingService.LogInformation(message, param1, param2);

        // Assert
        _mockLogger.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }
}
