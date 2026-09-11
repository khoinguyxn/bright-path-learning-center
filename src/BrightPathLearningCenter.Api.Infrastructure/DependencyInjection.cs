using System.Diagnostics.CodeAnalysis;

using BrightPathLearningCenter.Api.Domain.Abstractions;
using BrightPathLearningCenter.Api.Infrastructure.Persistence;
using BrightPathLearningCenter.Api.Infrastructure.Repositories;
using BrightPathLearningCenter.Api.Infrastructure.Seed;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BrightPathLearningCenter.Api.Infrastructure;

[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<SchedulingDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<ILessonRepository, LessonRepository>();
        services.AddScoped<LessonSeeder>();

        return services;
    }
}
