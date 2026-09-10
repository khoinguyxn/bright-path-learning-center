namespace BrightPathLearningCenter.Api.Contracts;

public sealed record CreateLessonRequest
{
    public string Student { get; init; } = string.Empty;

    public string TutorId { get; init; } = string.Empty;

    public string RoomId { get; init; } = string.Empty;

    public DateTimeOffset StartsAt { get; init; }

    public int DurationMinutes { get; init; }

    public string? Note { get; init; }
}
