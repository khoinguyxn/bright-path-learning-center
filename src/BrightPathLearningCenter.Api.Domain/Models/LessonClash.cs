namespace BrightPathLearningCenter.Api.Domain.Models;

public sealed record LessonClash
{
    public required Lesson Existing { get; init; }
    public required IReadOnlyList<ConflictType> Types { get; init; }
}
