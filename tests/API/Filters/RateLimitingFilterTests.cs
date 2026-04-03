using ECommerce.API.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ECommerce.Tests.API.Filters;

[TestFixture]
public class RateLimitingFilterTests
{
    [Test]
    public void Constructor_WithNullDistributedCache_ThrowsException()
    {
        var logger = new Mock<ILogger<RateLimitingFilter>>();
        Action act = () => new RateLimitingFilter(logger.Object, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("distributedCache");
    }

    [Test]
    public void OnActionExecuting_WhenLimitExceeded_ReturnsTooManyRequests()
    {
        var logger = new Mock<ILogger<RateLimitingFilter>>();
        var cache = new MemoryDistributedCache(
            Microsoft.Extensions.Options.Options.Create(new MemoryDistributedCacheOptions())
        );
        var filter = new RateLimitingFilter(logger.Object, cache, maxRequests: 1, windowSeconds: 60);

        var context1 = CreateContext();
        filter.OnActionExecuting(context1);
        context1.Result.Should().BeNull();

        var context2 = CreateContext();
        filter.OnActionExecuting(context2);
        context2.Result.Should().BeOfType<ObjectResult>();
        (context2.Result as ObjectResult)!.StatusCode.Should().Be(StatusCodes.Status429TooManyRequests);
    }

    private static ActionExecutingContext CreateContext()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        return new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            new object()
        );
    }
}
