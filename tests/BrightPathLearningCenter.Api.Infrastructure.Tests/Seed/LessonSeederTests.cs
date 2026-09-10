using BrightPathLearningCenter.Api.Domain.Models;
using BrightPathLearningCenter.Api.Infrastructure.Persistence;
using BrightPathLearningCenter.Api.Infrastructure.Repositories;
using BrightPathLearningCenter.Api.Infrastructure.Seed;
using BrightPathLearningCenter.Api.Infrastructure.Tests.Persistence;

using Microsoft.EntityFrameworkCore;

namespace BrightPathLearningCenter.Api.Infrastructure.Tests.Seed;

public sealed class LessonSeederTests : IDisposable
{
    private readonly SchedulingDbContext context;
    private readonly TestDatabase database = new();
    private readonly LessonSeeder seeder;

    public LessonSeederTests()
    {
        context = database.CreateContext();
        seeder = new LessonSeeder(context);
    }

    public void Dispose()
    {
        context.Dispose();
        database.Dispose();
    }

    [Fact]
    public async Task SeedAsync_LoadsEveryRow()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        await seeder.SeedAsync(cancellationToken);

        Assert.Equal(34, await context.Lessons.CountAsync(cancellationToken));
    }

    [Fact]
    public async Task SeedAsync_StoresAllInstantsInUtc()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        await seeder.SeedAsync(cancellationToken);

        List<Lesson> lessons = await context.Lessons.ToListAsync(cancellationToken);
        Assert.All(lessons, lesson => Assert.Equal(TimeSpan.Zero, lesson.StartsAt.Offset));

        Lesson first = await context.Lessons.SingleAsync(lesson => lesson.Id == "L001", cancellationToken);
        Assert.Equal(new DateTime(2026, 3, 3, 2, 0, 0, DateTimeKind.Utc), first.StartsAt.UtcDateTime);
    }

    [Fact]
    public async Task SeedAsync_PreservesPreExistingViolations()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        await seeder.SeedAsync(cancellationToken);

        List<Lesson> tutorPair = await context.Lessons
            .Where(lesson => lesson.Id == "L009" || lesson.Id == "L010")
            .ToListAsync(cancellationToken);
        Assert.Equal(2, tutorPair.Count);
        Assert.All(tutorPair, lesson => Assert.Equal("T1", lesson.TutorId));
        Assert.Single(tutorPair.Select(lesson => lesson.StartsAt).Distinct());

        List<Lesson> studentPair = await context.Lessons
            .Where(lesson => lesson.Id == "L007" || lesson.Id == "L008")
            .ToListAsync(cancellationToken);
        Assert.Equal(2, studentPair.Count);
        Assert.Single(studentPair.Select(lesson => lesson.Student).Distinct());

        Lesson noShow = await context.Lessons.SingleAsync(lesson => lesson.Id == "L015", cancellationToken);
        Assert.Equal(LessonStatus.NoShow, noShow.Status);

        Lesson cancelled = await context.Lessons.SingleAsync(lesson => lesson.Id == "L005", cancellationToken);
        Assert.Equal(LessonStatus.Cancelled, cancelled.Status);
    }

    [Fact]
    public async Task SeedAsync_WhenRunTwice_DoesNotDuplicate()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        await seeder.SeedAsync(cancellationToken);
        await seeder.SeedAsync(cancellationToken);

        Assert.Equal(34, await context.Lessons.CountAsync(cancellationToken));
    }

    [Fact]
    public async Task SeedAsync_ThenNextLessonId_ReturnsL035()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        await seeder.SeedAsync(cancellationToken);
        LessonRepository repository = new(context);

        Assert.Equal("L035", await repository.NextLessonIdAsync(cancellationToken));
    }
}
