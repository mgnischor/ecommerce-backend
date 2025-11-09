using ECommerce.API.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;

namespace ECommerce.Tests.API.Filters;

/// <summary>
/// Tests for ValidateModelStateFilter
/// </summary>
[TestFixture]
public class ValidateModelStateFilterTests : BaseTestFixture
{
    private Mock<ILogger<ValidateModelStateFilter>> _mockLogger;
    private ValidateModelStateFilter _filter;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        _mockLogger = new Mock<ILogger<ValidateModelStateFilter>>();
        _filter = new ValidateModelStateFilter(_mockLogger.Object);
    }

    [Test]
    public void Constructor_WithNullLogger_ThrowsException()
    {
        // Act
        Action act = () => new ValidateModelStateFilter(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Test]
    public void OnActionExecuting_WithValidModelState_DoesNotSetResult()
    {
        // Arrange
        var actionContext = CreateActionContext();
        var context = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            new object()
        );

        // Act
        _filter.OnActionExecuting(context);

        // Assert
        context.Result.Should().BeNull();
    }

    [Test]
    public void OnActionExecuting_WithInvalidModelState_SetsResult()
    {
        // Arrange
        var actionContext = CreateActionContext();
        actionContext.ModelState.AddModelError("Email", "Email is required");
        var context = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            new object()
        );

        // Act
        _filter.OnActionExecuting(context);

        // Assert
        context.Result.Should().NotBeNull();
        context.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public void OnActionExecuting_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var actionContext = CreateActionContext();
        actionContext.ModelState.AddModelError("Email", "Email is required");
        var context = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            new object()
        );

        // Act
        _filter.OnActionExecuting(context);

        // Assert
        var result = context.Result as BadRequestObjectResult;
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(400);
    }

    [Test]
    public void OnActionExecuting_WithInvalidModelState_ReturnsErrorDetails()
    {
        // Arrange
        var actionContext = CreateActionContext();
        actionContext.ModelState.AddModelError("Email", "Email is required");
        actionContext.ModelState.AddModelError("Password", "Password is required");
        var context = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            new object()
        );

        // Act
        _filter.OnActionExecuting(context);

        // Assert
        var result = context.Result as BadRequestObjectResult;
        result.Should().NotBeNull();
        var value = result!.Value;
        value.Should().NotBeNull();

        var valueType = value!.GetType();
        var messageProperty = valueType.GetProperty("Message");
        var errorsProperty = valueType.GetProperty("Errors");

        messageProperty.Should().NotBeNull();
        errorsProperty.Should().NotBeNull();
    }

    [Test]
    public void OnActionExecuting_WithMultipleErrors_IncludesAllErrors()
    {
        // Arrange
        var actionContext = CreateActionContext();
        actionContext.ModelState.AddModelError("Email", "Email is required");
        actionContext.ModelState.AddModelError("Email", "Email format is invalid");
        actionContext.ModelState.AddModelError("Password", "Password is required");
        var context = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            new object()
        );

        // Act
        _filter.OnActionExecuting(context);

        // Assert
        var result = context.Result as BadRequestObjectResult;
        result.Should().NotBeNull();

        var value = result!.Value;
        var errorsProperty = value!.GetType().GetProperty("Errors");
        var errors = errorsProperty!.GetValue(value) as Dictionary<string, string[]>;

        errors.Should().NotBeNull();
        errors!.Should().ContainKey("Email");
        errors.Should().ContainKey("Password");
        errors["Email"].Should().HaveCount(2);
        errors["Password"].Should().HaveCount(1);
    }

    [Test]
    public void OnActionExecuting_WithInvalidModelState_LogsWarning()
    {
        // Arrange
        var actionContext = CreateActionContext();
        actionContext.ModelState.AddModelError("Email", "Email is required");
        var context = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            new object()
        );

        // Act
        _filter.OnActionExecuting(context);

        // Assert
        _mockLogger.Verify(
            x =>
                x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (v, t) => v.ToString()!.Contains("Model validation failed")
                    ),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Test]
    public void OnActionExecuting_WithValidModelState_DoesNotLog()
    {
        // Arrange
        var actionContext = CreateActionContext();
        var context = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            new object()
        );

        // Act
        _filter.OnActionExecuting(context);

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
    public void OnActionExecuted_DoesNothing()
    {
        // Arrange
        var actionContext = CreateActionContext();
        var context = new ActionExecutedContext(
            actionContext,
            new List<IFilterMetadata>(),
            new object()
        );

        // Act
        _filter.OnActionExecuted(context);

        // Assert - No exception thrown, method completes successfully
        Assert.Pass();
    }

    [Test]
    public void OnActionExecuting_WithEmptyErrorMessage_HandlesGracefully()
    {
        // Arrange
        var actionContext = CreateActionContext();
        actionContext.ModelState.AddModelError("Field", "");
        var context = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            new object()
        );

        // Act
        _filter.OnActionExecuting(context);

        // Assert
        var result = context.Result as BadRequestObjectResult;
        result.Should().NotBeNull();
    }

    [Test]
    public void OnActionExecuting_WithNestedPropertyErrors_IncludesFullPath()
    {
        // Arrange
        var actionContext = CreateActionContext();
        actionContext.ModelState.AddModelError("User.Email", "Email is required");
        actionContext.ModelState.AddModelError("User.Address.City", "City is required");
        var context = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            new object()
        );

        // Act
        _filter.OnActionExecuting(context);

        // Assert
        var result = context.Result as BadRequestObjectResult;
        result.Should().NotBeNull();

        var value = result!.Value;
        var errorsProperty = value!.GetType().GetProperty("Errors");
        var errors = errorsProperty!.GetValue(value) as Dictionary<string, string[]>;

        errors.Should().NotBeNull();
        errors!.Should().ContainKey("User.Email");
        errors.Should().ContainKey("User.Address.City");
    }

    private ActionContext CreateActionContext()
    {
        var httpContext = new DefaultHttpContext();
        var routeData = new RouteData();
        var actionDescriptor = new ActionDescriptor { DisplayName = "TestAction" };

        return new ActionContext(httpContext, routeData, actionDescriptor);
    }
}
