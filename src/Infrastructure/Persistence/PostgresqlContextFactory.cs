using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for creating PostgresqlContext instances.
/// This is used by EF Core tools (migrations, scaffolding) at design time.
/// </summary>
public class PostgresqlContextFactory : IDesignTimeDbContextFactory<PostgresqlContext>
{
    public PostgresqlContext CreateDbContext(string[] args)
    {
        // Determine the base path - EF Core tools run from the project root
        var basePath = Directory.GetCurrentDirectory();

        // If running from the src/Infrastructure directory, navigate to project root
        if (basePath.EndsWith("Infrastructure") || basePath.EndsWith("Persistence"))
        {
            basePath = Path.GetFullPath(Path.Combine(basePath, "..", ".."));
        }

        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        // Get connection string from configuration
        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Database=ecommerce;Username=ecommerce;Password=ecommerce;Port=5432";

        // Build DbContextOptions
        var optionsBuilder = new DbContextOptionsBuilder<PostgresqlContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new PostgresqlContext(optionsBuilder.Options);
    }
}
