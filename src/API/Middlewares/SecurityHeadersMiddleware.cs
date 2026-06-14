namespace ECommerce.API.Middlewares;

/// <summary>
/// Adds standard security-related HTTP response headers to every response.
/// </summary>
public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        // Prevent MIME-sniffing
        headers["X-Content-Type-Options"] = "nosniff";
        // Disallow framing (clickjacking protection)
        headers["X-Frame-Options"] = "DENY";
        // Legacy XSS protection (mostly superseded by CSP)
        headers["X-XSS-Protection"] = "0";
        // Referrer policy
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        // HSTS - only on HTTPS connections
        if (context.Request.IsHttps)
        {
            headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
        }
        // Remove server fingerprinting
        headers.Remove("Server");
        headers.Remove("X-Powered-By");

        await _next(context);
    }
}
