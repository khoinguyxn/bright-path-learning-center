using System.Diagnostics.CodeAnalysis;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BrightPathLearningCenter.Api.Infrastructure.Persistence;

[ExcludeFromCodeCoverage]
public sealed class SchedulingDbContextFactory : IDesignTimeDbContextFactory<SchedulingDbContext>
{
    private const string DesignTimeConnectionString = "Data Source=brightpath.db";

    public SchedulingDbContext CreateDbContext(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        DbContextOptions<SchedulingDbContext> options = new DbContextOptionsBuilder<SchedulingDbContext>()
            .UseSqlite(DesignTimeConnectionString)
            .Options;

        return new SchedulingDbContext(options);
    }
}
