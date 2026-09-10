using BrightPathLearningCenter.Api.Domain.Models;

namespace BrightPathLearningCenter.Api.Domain.Services;

public static class LessonFactory
{
    public static Lesson Create(
        string id,
        string student,
        string tutorId,
        string roomId,
        DateTimeOffset startsAt,
        int durationMinutes,
        string? note = null
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(student);
        ArgumentException.ThrowIfNullOrWhiteSpace(tutorId);
        ArgumentException.ThrowIfNullOrWhiteSpace(roomId);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(durationMinutes, 0);

        DateTimeOffset start = startsAt.ToUniversalTime();

        return new Lesson
        {
            Id = id,
            Student = student,
            TutorId = tutorId,
            RoomId = roomId,
            StartsAt = start,
            DurationMinutes = durationMinutes,
            Status = LessonStatus.Booked,
            Note = note
        };
    }

    public static Lesson Restore(
        string id,
        string student,
        string tutorId,
        string roomId,
        DateTimeOffset startsAt,
        int durationMinutes,
        LessonStatus status,
        DateTimeOffset? cancelledAt,
        string? note
    )
    {
        DateTimeOffset start = startsAt.ToUniversalTime();

        return new Lesson
        {
            Id = id,
            Student = student,
            TutorId = tutorId,
            RoomId = roomId,
            StartsAt = start,
            DurationMinutes = durationMinutes,
            Status = status,
            CancelledAt = cancelledAt?.ToUniversalTime(),
            Note = note
        };
    }
}
