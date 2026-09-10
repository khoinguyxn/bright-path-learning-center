namespace BrightPathLearningCenter.Api.Contracts;

public sealed record LessonResponse
{
    public required string Id { get; init; }

    public required string Student { get; init; }

    public required string TutorId { get; init; }

    public required string RoomId { get; init; }

    public required DateTimeOffset StartsAt { get; init; }

    public required DateTimeOffset EndsAt { get; init; }

    public required int DurationMinutes { get; init; }

    public required string Status { get; init; }

    public string? Note { get; init; }
}
