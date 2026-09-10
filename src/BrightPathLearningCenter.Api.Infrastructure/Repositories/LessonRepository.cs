using System.Globalization;

using BrightPathLearningCenter.Api.Domain.Abstractions;
using BrightPathLearningCenter.Api.Domain.Extensions;
using BrightPathLearningCenter.Api.Domain.Models;
using BrightPathLearningCenter.Api.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace BrightPathLearningCenter.Api.Infrastructure.Repositories;

public sealed class LessonRepository(SchedulingDbContext dbContext) : ILessonRepository
{
    public async Task<IReadOnlyList<Lesson>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Lessons.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<string> NextLessonIdAsync(CancellationToken cancellationToken = default)
    {
        List<string> ids = await dbContext.Lessons
            .AsNoTracking()
            .Select(lesson => lesson.Id)
            .ToListAsync(cancellationToken);

        int highest = 0;

        foreach (string id in ids)
        {
            if (id.Length > Lesson.IdPrefix.Length
                && id.StartsWith(Lesson.IdPrefix, StringComparison.Ordinal)
                && int.TryParse(id.AsSpan(Lesson.IdPrefix.Length),
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out int number
                )
                && number > highest)
            {
                highest = number;
            }
        }

        return (highest + 1).ToLessonId();
    }

    public async Task AddAsync(Lesson lesson, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lesson);

        await dbContext.Lessons.AddAsync(lesson, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
