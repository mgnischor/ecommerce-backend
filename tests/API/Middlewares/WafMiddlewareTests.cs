using System.IO;
using System.Net;
using Comex.API.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Comex.Tests.API.Middlewares;

/// <summary>
/// Tests for WafMiddleware
/// </summary>
[TestFixture]
public class WafMiddlewareTests
{
    private Mock<ILogger<WafMiddleware>> _mockLogger;
    private Mock<ISecurityAuditService> _mockAuditService;
    private Mock<RequestDelegate> _mockNext;
    private DefaultHttpContext _httpContext;
    private MemoryStream _responseBody;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<WafMiddleware>>();
        _mockAuditService = new Mock<ISecurityAuditService>();
        _mockNext = new Mock<RequestDelegate>();
        _responseBody = new MemoryStream();
        _httpContext = new DefaultHttpContext();
        _httpContext.Response.Body = _responseBody;
    }

    private static WafMiddleware CreateMiddleware(IDictionary<string, string?>? settings = null)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        return new WafMiddleware(
            (ctx) => Task.CompletedTask,
            new Mock<ILogger<WafMiddleware>>().Object,
            config
        );
    }

    private WafMiddleware CreateMiddlewareWithNext(IDictionary<string, string?>? settings = null)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        return new WafMiddleware(_mockNext.Object, _mockLogger.Object, config);
    }

    [Test]
    public void Constructor_WithNullNext_Throws()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();

        // Act
        Action act = () => new WafMiddleware(null!, _mockLogger.Object, config);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("next");
    }

    [Test]
    public async Task InvokeAsync_WhenDisabled_CallsNext()
    {
        // Arrange
        var middleware = CreateMiddlewareWithNext(
            new Dictionary<string, string?> { ["Waf:Enabled"] = "false" }
        );
        _mockNext.Setup(n => n(_httpContext)).Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(_httpContext, _mockAuditService.Object);

        // Assert
        _mockNext.Verify(n => n(_httpContext), Times.Once);
        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Test]
    public async Task InvokeAsync_WithCleanRequest_CallsNext()
    {
        // Arrange
        var middleware = CreateMiddlewareWithNext();
        _httpContext.Request.Path = "/api/v1/products";
        _httpContext.Request.QueryString = new QueryString("?page=1&search=notebook");
        _mockNext.Setup(n => n(_httpContext)).Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(_httpContext, _mockAuditService.Object);

        // Assert
        _mockNext.Verify(n => n(_httpContext), Times.Once);
    }

    [Test]
    public async Task InvokeAsync_WithBlockedIp_ReturnsForbidden()
    {
        // Arrange
        var middleware = CreateMiddlewareWithNext(
            new Dictionary<string, string?> { ["Waf:BlockedIpAddresses"] = "10.0.0.1" }
        );
        _httpContext.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.1");
        _httpContext.Request.Path = "/api/v1/products";

        // Act
        await middleware.InvokeAsync(_httpContext, _mockAuditService.Object);

        // Assert
        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        _mockNext.Verify(n => n(_httpContext), Times.Never);
        _mockAuditService.Verify(
            x =>
                x.Record(
                    "Waf",
                    "Warning",
                    It.IsAny<string>(),
                    "10.0.0.1",
                    It.IsAny<string?>(),
                    It.IsAny<string>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task InvokeAsync_WithScannerUserAgent_ReturnsForbidden()
    {
        // Arrange
        var middleware = CreateMiddlewareWithNext();
        _httpContext.Request.Path = "/api/v1/login";
        _httpContext.Request.Headers.UserAgent = "sqlmap/1.7";

        // Act
        await middleware.InvokeAsync(_httpContext, _mockAuditService.Object);

        // Assert
        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        _mockNext.Verify(n => n(_httpContext), Times.Never);
    }

    [Test]
    public async Task InvokeAsync_WithSqlInjectionQuery_ReturnsForbidden()
    {
        // Arrange
        var middleware = CreateMiddlewareWithNext();
        _httpContext.Request.Path = "/api/v1/products";
        _httpContext.Request.QueryString = new QueryString("?q=' union select 1--");

        // Act
        await middleware.InvokeAsync(_httpContext, _mockAuditService.Object);

        // Assert
        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        _mockNext.Verify(n => n(_httpContext), Times.Never);
    }

    [Test]
    public async Task InvokeAsync_WithPathTraversal_ReturnsForbidden()
    {
        // Arrange
        var middleware = CreateMiddlewareWithNext();
        _httpContext.Request.Path = "/api/v1/files/../../etc/passwd";

        // Act
        await middleware.InvokeAsync(_httpContext, _mockAuditService.Object);

        // Assert
        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        _mockNext.Verify(n => n(_httpContext), Times.Never);
    }

    [Test]
    public async Task InvokeAsync_WithXssPayload_ReturnsForbidden()
    {
        // Arrange
        var middleware = CreateMiddlewareWithNext();
        _httpContext.Request.Path = "/api/v1/products";
        _httpContext.Request.QueryString = new QueryString("?q=<script>alert(1)</script>");

        // Act
        await middleware.InvokeAsync(_httpContext, _mockAuditService.Object);

        // Assert
        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        _mockNext.Verify(n => n(_httpContext), Times.Never);
    }

    [Test]
    public async Task InvokeAsync_WithEncodedXssPayload_ReturnsForbidden()
    {
        // Arrange
        var middleware = CreateMiddlewareWithNext();
        _httpContext.Request.Path = "/api/v1/products";
        _httpContext.Request.QueryString = new QueryString(
            "?q=%3Cscript%3Ealert(1)%3C%2Fscript%3E"
        );

        // Act
        await middleware.InvokeAsync(_httpContext, _mockAuditService.Object);

        // Assert
        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        _mockNext.Verify(n => n(_httpContext), Times.Never);
    }

    [Test]
    public async Task InvokeAsync_WithEncodedSqlInjection_ReturnsForbidden()
    {
        // Arrange - decodes to: ' or '1'='1
        var middleware = CreateMiddlewareWithNext();
        _httpContext.Request.Path = "/api/v1/products";
        _httpContext.Request.QueryString = new QueryString("?q=%27%20or%20%271%27%3D%271");

        // Act
        await middleware.InvokeAsync(_httpContext, _mockAuditService.Object);

        // Assert
        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        _mockNext.Verify(n => n(_httpContext), Times.Never);
    }

    [Test]
    public async Task InvokeAsync_WithOversizedBody_ReturnsPayloadTooLarge()
    {
        // Arrange
        var middleware = CreateMiddlewareWithNext(
            new Dictionary<string, string?> { ["Waf:MaxRequestBodySizeBytes"] = "1024" }
        );
        _httpContext.Request.Path = "/api/v1/products";
        _httpContext.Request.ContentLength = 2048;

        // Act
        await middleware.InvokeAsync(_httpContext, _mockAuditService.Object);

        // Assert
        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status413PayloadTooLarge);
        _mockNext.Verify(n => n(_httpContext), Times.Never);
    }

    [Test]
    public void GetBlockedStats_ReturnsCounts()
    {
        // Arrange
        var middleware = CreateMiddleware(
            new Dictionary<string, string?> { ["Waf:BlockedIpAddresses"] = "10.0.0.5" }
        );
        _httpContext.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.5");
        _httpContext.Request.Path = "/api/v1/products";

        // Act
        var invokeTask = middleware.InvokeAsync(_httpContext, _mockAuditService.Object);
        invokeTask.GetAwaiter().GetResult();

        // Assert
        var stats = WafMiddleware.GetBlockedStats();
        stats.Should().ContainKey("blocked_ip");
        stats["blocked_ip"].Should().BeGreaterThan(0);
    }
}
