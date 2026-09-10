namespace BrightPathLearningCenter.Api.Domain.Models;

public sealed record Lesson
{
    public const string IdPrefix = "L";
    public const int IdDigits = 3;

    public required string Id { get; init; }
    public required string Student { get; init; }
    public required string TutorId { get; init; }
    public required string RoomId { get; init; }
    public required DateTimeOffset StartsAt { get; init; }
    public required int DurationMinutes { get; init; }
    public LessonStatus Status { get; init; } = LessonStatus.Booked;
    public DateTimeOffset? CancelledAt { get; init; }
    public string? Note { get; init; }
}
