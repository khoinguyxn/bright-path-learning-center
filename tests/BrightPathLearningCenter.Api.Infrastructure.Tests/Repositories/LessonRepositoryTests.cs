using BrightPathLearningCenter.Api.Domain.Common;
using BrightPathLearningCenter.Api.Domain.Models;
using BrightPathLearningCenter.Api.Domain.Services;
using BrightPathLearningCenter.Api.Infrastructure.Persistence;
using BrightPathLearningCenter.Api.Infrastructure.Repositories;
using BrightPathLearningCenter.Api.Infrastructure.Tests.Persistence;

using Microsoft.EntityFrameworkCore;

namespace BrightPathLearningCenter.Api.Infrastructure.Tests.Repositories;

public sealed class LessonRepositoryTests : IDisposable
{
    private static readonly DateTimeOffset Wednesday9Am =
        new(2026, 3, 4, 9, 0, 0, CentreTimeZone.Offset);

    private readonly SchedulingDbContext _context;

    private readonly TestDatabase _database = new();
    private readonly LessonRepository _repository;

    public LessonRepositoryTests()
    {
        _context = _database.CreateContext();
        _repository = new LessonRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _database.Dispose();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEveryLessonIncludingCancelled()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        _context.Lessons.Add(Existing());
        _context.Lessons.Add(Existing("L002", LessonStatus.Cancelled));
        await _context.SaveChangesAsync(cancellationToken);

        IReadOnlyList<Lesson> lessons = await _repository.GetAllAsync(cancellationToken);

        Assert.Equal(2, lessons.Count);
    }

    [Fact]
    public async Task NextLessonIdAsync_WhenEmpty_ReturnsL001()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        Assert.Equal("L001", await _repository.NextLessonIdAsync(cancellationToken));
    }

    [Fact]
    public async Task NextLessonIdAsync_ReturnsHighestPlusOne()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        _context.Lessons.Add(Existing());
        _context.Lessons.Add(LessonFactory.Create("L034", "B", "T2", "R2", Wednesday9Am, 60));
        await _context.SaveChangesAsync(cancellationToken);

        Assert.Equal("L035", await _repository.NextLessonIdAsync(cancellationToken));
    }

    [Fact]
    public async Task NextLessonIdAsync_IgnoresIdsThatDoNotMatchThePattern()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        _context.Lessons.Add(Existing("X999"));
        _context.Lessons.Add(Existing("LAB"));
        _context.Lessons.Add(Existing("L"));
        _context.Lessons.Add(Existing("L007"));
        await _context.SaveChangesAsync(cancellationToken);

        Assert.Equal("L008", await _repository.NextLessonIdAsync(cancellationToken));
    }

    [Fact]
    public async Task AddAsync_PersistsLessonInUtc()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        await _repository.AddAsync(LessonFactory.Create("L001", "A", "T1", "R1", Wednesday9Am, 60), cancellationToken);

        await using SchedulingDbContext verificationContext = _database.CreateContext();
        Lesson stored = await verificationContext.Lessons.SingleAsync(lesson => lesson.Id == "L001", cancellationToken);
        Assert.Equal(TimeSpan.Zero, stored.StartsAt.Offset);
    }

    private static Lesson Existing(string id = "L001", LessonStatus status = LessonStatus.Booked)
    {
        return status == LessonStatus.Booked
            ? LessonFactory.Create(id, "A", "T1", "R1", Wednesday9Am, 60)
            : LessonFactory.Restore(id, "A", "T1", "R1", Wednesday9Am, 60, status, Wednesday9Am.AddHours(-3), null);
    }
}
