using System.Text;
using ECommerce.API.Middlewares;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Persistence;
using ECommerce.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddDbContext<PostgresqlContext>(options =>
    options.UseNpgsql("Host=localhost;Database=ecommerce;Username=ecommerce;Password=ecommerce")
);

builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();

// Register generic repositories for accounting entities
builder.Services.AddScoped<IRepository<ChartOfAccountsEntity>, Repository<ChartOfAccountsEntity>>();
builder.Services.AddScoped<IRepository<JournalEntryEntity>, Repository<JournalEntryEntity>>();
builder.Services.AddScoped<IRepository<AccountingEntryEntity>, Repository<AccountingEntryEntity>>();
builder.Services.AddScoped<
    IRepository<InventoryTransactionEntity>,
    Repository<InventoryTransactionEntity>
>();

// Register accounting services
builder.Services.AddScoped<IAccountingService, AccountingService>();
builder.Services.AddScoped<IInventoryTransactionService, InventoryTransactionService>();

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
            ClockSkew = TimeSpan.Zero,
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Use global exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Seed database with admin user
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PostgresqlContext>();
    var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Starting database seeding");
        await DatabaseSeeder.SeedAdminUserAsync(context, passwordService);
        await DatabaseSeeder.SeedChartOfAccountsAsync(context);
        logger.LogInformation("Database seeding completed successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding the database");
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

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
