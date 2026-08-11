using System.Net;
using System.Text;
using System.Threading.RateLimiting;
using ECommerce.API.Extensions;
using ECommerce.API.Middlewares;
using ECommerce.API.Services;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Persistence;
using ECommerce.Infrastructure.Repositories;
using ECommerce.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using Scalar.AspNetCore;

// CLI mode: generate a strong JWT signing secret and exit.
// Usage: dotnet run -- --generate-jwt-secret
if (args.Any(a => a.Equals("--generate-jwt-secret", StringComparison.OrdinalIgnoreCase)))
{
    Console.WriteLine(JwtSecretGenerator.GenerateSecret());
    return;
}

var builder = WebApplication.CreateBuilder(args);

// Configure OpenTelemetry Observability (Tracing, Metrics, Logging)
builder.AddOpenTelemetryObservability();

// Configure Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Configure logging levels
if (builder.Environment.IsDevelopment())
{
    builder.Logging.SetMinimumLevel(LogLevel.Debug);
}
else
{
    builder.Logging.SetMinimumLevel(LogLevel.Information);
}

// Get connection string from configuration
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' is not configured"
    );

builder.Services.AddDbContext<PostgresqlContext>(options => options.UseNpgsql(connectionString));

// Register specialized repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IWishlistRepository, WishlistRepository>();
builder.Services.AddScoped<ICouponRepository, CouponRepository>();
builder.Services.AddScoped<IChartOfAccountsRepository, ChartOfAccountsRepository>();
builder.Services.AddScoped<IJournalEntryRepository, JournalEntryRepository>();
builder.Services.AddScoped<IAccountingEntryRepository, AccountingEntryRepository>();
builder.Services.AddScoped<IAccountingRuleRepository, AccountingRuleRepository>();
builder.Services.AddScoped<IInventoryTransactionRepository, InventoryTransactionRepository>();
builder.Services.AddScoped<IFinancialTransactionRepository, FinancialTransactionRepository>();

// Register logging service
builder.Services.AddScoped<ILoggingService, LoggingService>();

// Register accounting query services
builder.Services.AddScoped<IAccountingQueryService, AccountingQueryService>();

// Configure HTTPS with optional self-signed certificate support (development / internal use).
// When "Certificates:Enabled" is true, Kestrel listens on both HTTP and HTTPS ports using the
// configured PFX (auto-generated self-signed when it does not exist yet).
var certificatesSection = builder.Configuration.GetSection("Certificates");
if (certificatesSection.GetValue<bool>("Enabled"))
{
    var certPath = certificatesSection["Path"] ?? "certs/ecommerce.pfx";
    var certPassword = certificatesSection["Password"];
    var certHost = certificatesSection["Host"] ?? "localhost";
    var daysValid = certificatesSection.GetValue("DaysValid", 365);
    var httpPort = certificatesSection.GetValue(
        "HttpPort",
        builder.Environment.IsDevelopment() ? 5049 : 80
    );
    var httpsPort = certificatesSection.GetValue("HttpsPort", 8443);

    var certificate = CertificateGenerator.LoadOrCreate(
        certPath,
        certPassword,
        certHost,
        daysValid
    );

    builder.WebHost.ConfigureKestrel(kestrel =>
    {
        kestrel.Listen(IPAddress.Any, httpPort);
        kestrel.Listen(IPAddress.Any, httpsPort, listen => listen.UseHttps(certificate));
    });
}

// Register services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IAccountingService, AccountingService>();
builder.Services.AddScoped<
    ECommerce.Application.Interfaces.IInventoryTransactionService,
    InventoryTransactionService
>();
builder.Services.AddScoped<IFinancialService, FinancialService>();
builder.Services.AddScoped<IPaymentGatewayService, FictitiousPaymentGatewayService>();

// Register security audit service
builder.Services.AddScoped<ISecurityAuditService, SecurityAuditService>();

// Register database backup and disaster recovery service (hosted + on-demand)
builder.Services.AddSingleton<IDatabaseBackupService, DatabaseBackupService>();
builder.Services.AddHostedService(sp =>
    (DatabaseBackupService)sp.GetRequiredService<IDatabaseBackupService>()
);

// Configure JWT Authentication
var jwtSecretKey =
    builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("JWT SecretKey is not configured");
var jwtIssuer =
    builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("JWT Issuer is not configured");
var jwtAudience =
    builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("JWT Audience is not configured");

if (Encoding.UTF8.GetByteCount(jwtSecretKey) < 32)
{
    throw new InvalidOperationException("JWT SecretKey must be at least 256 bits (32 bytes) long.");
}

var jwtSecretValidation = JwtSecretGenerator.ValidateSecret(
    jwtSecretKey,
    builder.Environment.IsDevelopment()
);
if (!jwtSecretValidation.IsValid)
{
    throw new InvalidOperationException(jwtSecretValidation.Error);
}
else if (jwtSecretValidation.Warning is not null)
{
    Console.WriteLine($"[WARNING] {jwtSecretValidation.Warning}");
}

builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });

builder.Services.AddAuthorization();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var configured = builder.Configuration["Cors:AllowedOrigins"];
        var allowedOrigins = string.IsNullOrWhiteSpace(configured)
            ? Array.Empty<string>()
            : configured.Split(
                ',',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            );

        // In production we require an explicit allow-list.
        if (allowedOrigins.Length == 0 && !builder.Environment.IsDevelopment())
        {
            throw new InvalidOperationException(
                "Cors:AllowedOrigins must be configured for non-development environments."
            );
        }

        // Never allow "*" together with credentials.
        if (Array.Exists(allowedOrigins, o => o == "*"))
        {
            throw new InvalidOperationException(
                "Cors:AllowedOrigins cannot contain '*' when credentials are allowed."
            );
        }

        policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader().AllowCredentials();
    });
});

// Register filters that need DI
builder.Services.AddScoped<ECommerce.API.Filters.ApiExceptionFilter>();
builder.Services.AddMemoryCache();

// Configure rate limiting
// - Global baseline: 100 req/min per client IP (sliding window)
// - "auth" policy: 5 req/min per IP to protect against brute-force
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 4,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
            }
        )
    );

    options.AddFixedWindowLimiter(
        "auth",
        o =>
        {
            o.PermitLimit = 5;
            o.Window = TimeSpan.FromMinutes(1);
            o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            o.QueueLimit = 0;
        }
    );
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Configure health checks (liveness / readiness)
builder
    .Services.AddHealthChecks()
    .AddDbContextCheck<PostgresqlContext>("database", tags: ["ready"]);

var app = builder.Build();

// Trust X-Forwarded-For / X-Forwarded-Proto from the immediate upstream proxy.
// This must be the very first middleware so the real client IP and scheme are
// visible to everything downstream (security headers, rate limiter, auth).
// In production, restrict KnownProxies to only your actual reverse-proxy IPs.
app.UseForwardedHeaders(
    new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    }
);

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Global middleware order:
// 1. WAF (request filtering) - after forwarded headers so the real client IP is visible
// 2. Security headers
// 3. Exception handling
app.UseMiddleware<ECommerce.API.Middlewares.WafMiddleware>();
app.UseMiddleware<ECommerce.API.Middlewares.SecurityHeadersMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Apply migrations and seed database (gated by configuration)
var autoMigrate = builder.Configuration.GetValue("Database:AutoMigrate", false);
var seedAdmin = builder.Configuration.GetValue("Admin:SeedEnabled", false);

if (autoMigrate || seedAdmin)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<PostgresqlContext>();
    var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        if (autoMigrate)
        {
            logger.LogInformation("Applying database migrations");
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully");
        }

        if (seedAdmin)
        {
            var adminEmail = builder.Configuration["Admin:Email"];
            var adminPassword = builder.Configuration["Admin:Password"];

            logger.LogInformation("Starting database seeding");
            await DatabaseSeeder.SeedAdminUserAsync(
                context,
                passwordService,
                adminEmail ?? string.Empty,
                adminPassword ?? string.Empty
            );
            await DatabaseSeeder.SeedChartOfAccountsAsync(context);
            logger.LogInformation("Database seeding completed successfully");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while applying migrations or seeding the database");
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(
        "/docs",
        (options) =>
        {
            options.HideSearch = false;
            options.ShowSidebar = true;
            options.Theme = ScalarTheme.DeepSpace;
            options.Title = "E-Commerce API";
            options.WithDirectDocumentDownload();
            options.WithJsonDocumentDownload();
            options.WithOpenApiRoutePattern("/openapi/{documentName}.json");
            options.WithYamlDocumentDownload();
        }
    );
}

app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Health checks:
// - /health/live : liveness probe (process is up)
// - /health/ready: readiness probe (database reachable)
// - /health     : full health report
app.MapHealthChecks(
    "/health/live",
    new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions { Predicate = _ => false }
);
app.MapHealthChecks(
    "/health/ready",
    new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
    }
);
app.MapHealthChecks("/health");

// Prometheus metrics scraping endpoint
app.MapPrometheusScrapingEndpoint("/metrics");

app.Run();
