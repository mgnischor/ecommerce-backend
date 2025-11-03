using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Tests.TestFixtures;

/// <summary>
/// Test fixture for tests requiring database context
/// </summary>
public abstract class DatabaseTestFixture : BaseTestFixture
{
    protected PostgresqlContext? DbContext { get; private set; }

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        DbContext = CreateInMemoryDbContext();
    }

    [TearDown]
    public override void TearDown()
    {
        DbContext?.Dispose();
        DbContext = null;
        base.TearDown();
    }

    /// <summary>
    /// Seeds the database with test data
    /// </summary>
    protected async Task SeedDatabaseAsync()
    {
        if (DbContext == null)
            return;

        // Add sample data here
        await DbContext.SaveChangesAsync();
    }
}
