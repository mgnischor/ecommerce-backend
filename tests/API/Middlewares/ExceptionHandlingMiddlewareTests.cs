using System.IO;
using System.Net;
using System.Text.Json;
using ECommerce.API.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Tests.API.Middlewares;

/// <summary>
/// Tests for ExceptionHandlingMiddleware
/// </summary>
[TestFixture]
public class ExceptionHandlingMiddlewareTests : BaseTestFixture
{
    private Mock<ILogger<ExceptionHandlingMiddleware>> _mockLogger;
    private Mock<RequestDelegate> _mockNext;
    private DefaultHttpContext _httpContext;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        _mockLogger = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        _mockNext = new Mock<RequestDelegate>();
        _httpContext = new DefaultHttpContext();
        _httpContext.Response.Body = new MemoryStream();
    }

    [Test]
    public void Constructor_WithNullNext_ThrowsException()
    {
        // Act
        Action act = () => new ExceptionHandlingMiddleware(null!, _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("next");
    }

    [Test]
    public void Constructor_WithNullLogger_ThrowsException()
    {
        // Act
        Action act = () => new ExceptionHandlingMiddleware(_mockNext.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Test]
    public async Task InvokeAsync_WithNoException_CallsNext()
    {
        // Arrange
        var middleware = new ExceptionHandlingMiddleware(_mockNext.Object, _mockLogger.Object);
        _mockNext.Setup(next => next(_httpContext)).Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _mockNext.Verify(next => next(_httpContext), Times.Once);
    }

    [Test]
    public async Task InvokeAsync_WithArgumentNullException_ReturnsBadRequest()
    {
        // Arrange
        var middleware = new ExceptionHandlingMiddleware(_mockNext.Object, _mockLogger.Object);
        _mockNext
            .Setup(next => next(_httpContext))
            .Throws(new ArgumentNullException("testParam", "Test error"));

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        _httpContext.Response.ContentType.Should().Be("application/json");
    }

    [Test]
    public async Task InvokeAsync_WithArgumentException_ReturnsBadRequest()
    {
        // Arrange
        var middleware = new ExceptionHandlingMiddleware(_mockNext.Object, _mockLogger.Object);
        _mockNext
            .Setup(next => next(_httpContext))
            .Throws(new ArgumentException("Invalid argument"));

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task InvokeAsync_WithInvalidOperationException_ReturnsBadRequest()
    {
        // Arrange
        var middleware = new ExceptionHandlingMiddleware(_mockNext.Object, _mockLogger.Object);
        _mockNext
            .Setup(next => next(_httpContext))
            .Throws(new InvalidOperationException("Invalid operation"));

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task InvokeAsync_WithUnauthorizedAccessException_ReturnsUnauthorized()
    {
        // Arrange
        var middleware = new ExceptionHandlingMiddleware(_mockNext.Object, _mockLogger.Object);
        _mockNext
            .Setup(next => next(_httpContext))
            .Throws(new UnauthorizedAccessException("Unauthorized"));

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task InvokeAsync_WithKeyNotFoundException_ReturnsNotFound()
    {
        // Arrange
        var middleware = new ExceptionHandlingMiddleware(_mockNext.Object, _mockLogger.Object);
        _mockNext
            .Setup(next => next(_httpContext))
            .Throws(new KeyNotFoundException("Item not found"));

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Test]
    public async Task InvokeAsync_WithUnknownException_ReturnsInternalServerError()
    {
        // Arrange
        var middleware = new ExceptionHandlingMiddleware(_mockNext.Object, _mockLogger.Object);
        _mockNext.Setup(next => next(_httpContext)).Throws(new Exception("Unexpected error"));

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
    }

    [Test]
    public async Task InvokeAsync_WithException_LogsError()
    {
        // Arrange
        var middleware = new ExceptionHandlingMiddleware(_mockNext.Object, _mockLogger.Object);
        var exception = new InvalidOperationException("Test error");
        _mockNext.Setup(next => next(_httpContext)).Throws(exception);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _mockLogger.Verify(
            x =>
                x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task InvokeAsync_WithException_WritesJsonResponse()
    {
        // Arrange
        var middleware = new ExceptionHandlingMiddleware(_mockNext.Object, _mockLogger.Object);
        var exceptionMessage = "Test error message";
        _mockNext
            .Setup(next => next(_httpContext))
            .Throws(new InvalidOperationException(exceptionMessage));

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        responseBody.Should().Contain(exceptionMessage);
        responseBody.Should().Contain("\"status\"");
        responseBody.Should().Contain("\"title\"");
        responseBody.Should().Contain("\"detail\"");
    }

    [Test]
    public async Task InvokeAsync_WithException_IncludesRequestPath()
    {
        // Arrange
        var middleware = new ExceptionHandlingMiddleware(_mockNext.Object, _mockLogger.Object);
        _httpContext.Request.Path = "/api/test";
        _mockNext
            .Setup(next => next(_httpContext))
            .Throws(new InvalidOperationException("Test error"));

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        responseBody.Should().Contain("/api/test");
    }

    [Test]
    public async Task InvokeAsync_WithException_SetsProblemDetailsProperties()
    {
        // Arrange
        var middleware = new ExceptionHandlingMiddleware(_mockNext.Object, _mockLogger.Object);
        var exceptionMessage = "Test error";
        _httpContext.Request.Path = "/api/test";
        _mockNext.Setup(next => next(_httpContext)).Throws(new ArgumentException(exceptionMessage));

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(
            responseBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be((int)HttpStatusCode.BadRequest);
        problemDetails.Title.Should().Be("Bad Request");
        problemDetails.Detail.Should().Contain(exceptionMessage);
        problemDetails.Instance.Should().Be("/api/test");
    }
}
