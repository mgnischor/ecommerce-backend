using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Tests.TestFixtures;

/// <summary>
/// Base test fixture providing common setup and teardown for tests
/// </summary>
public abstract class BaseTestFixture
{
    protected Mock<ILogger>? MockLogger { get; private set; }

    [SetUp]
    public virtual void SetUp()
    {
        // Common setup for all tests
        MockLogger = new Mock<ILogger>();
    }

    [TearDown]
    public virtual void TearDown()
    {
        // Common cleanup for all tests
        MockLogger = null;
    }

    /// <summary>
    /// Creates an in-memory database context for testing
    /// </summary>
    protected static PostgresqlContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<PostgresqlContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new PostgresqlContext(options);
    }
}
