namespace BrightPathLearningCenter.Api.Contracts;

public sealed record LessonConflictResponse
{
    public required string LessonId { get; init; }

    public required IEnumerable<string> Types { get; init; }
}
