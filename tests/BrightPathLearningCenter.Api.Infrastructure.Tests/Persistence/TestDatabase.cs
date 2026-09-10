using BrightPathLearningCenter.Api.Infrastructure.Persistence;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BrightPathLearningCenter.Api.Infrastructure.Tests.Persistence;

internal sealed class TestDatabase : IDisposable
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");

    public TestDatabase()
    {
        connection.Open();
    }

    public void Dispose()
    {
        connection.Dispose();
    }

    public SchedulingDbContext CreateContext()
    {
        DbContextOptions<SchedulingDbContext> options = new DbContextOptionsBuilder<SchedulingDbContext>()
            .UseSqlite(connection)
            .Options;

        SchedulingDbContext context = new(options);
        context.Database.EnsureCreated();
        return context;
    }
}
