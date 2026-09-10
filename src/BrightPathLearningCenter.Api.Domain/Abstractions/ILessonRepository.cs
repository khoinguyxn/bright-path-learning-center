using BrightPathLearningCenter.Api.Domain.Models;

namespace BrightPathLearningCenter.Api.Domain.Abstractions;

public interface ILessonRepository
{
    Task<IReadOnlyList<Lesson>> FindOverlappingActiveLessonsAsync(
        Lesson candidate,
        CancellationToken cancellationToken = default);

    Task<string> NextLessonIdAsync(CancellationToken cancellationToken = default);

    Task AddAsync(Lesson lesson, CancellationToken cancellationToken = default);
}
