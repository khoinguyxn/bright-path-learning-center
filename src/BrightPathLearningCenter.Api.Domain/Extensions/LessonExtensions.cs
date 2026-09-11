using BrightPathLearningCenter.Api.Domain.Models;

namespace BrightPathLearningCenter.Api.Domain.Extensions;

public static class LessonExtensions
{
    extension(Lesson lesson)
    {
        public bool IsActive()
        {
            ArgumentNullException.ThrowIfNull(lesson);

            return lesson.Status != LessonStatus.Cancelled;
        }

        public DateTimeOffset EndsAt()
        {
            return lesson.StartsAt.AddMinutes(lesson.DurationMinutes);
        }

        public TimeSlot Slot()
        {
            ArgumentNullException.ThrowIfNull(lesson);

            return new TimeSlot { Start = lesson.StartsAt, End = lesson.EndsAt() };
        }

        public IReadOnlyList<ConflictType> ConflictTypesWith(Lesson other)
        {
            ArgumentNullException.ThrowIfNull(lesson);
            ArgumentNullException.ThrowIfNull(other);

            if (!lesson.IsActive() || !other.IsActive() || !lesson.Slot().Overlaps(other.Slot()))
            {
                return [];
            }

            List<ConflictType> types = new(3);

            if (string.Equals(lesson.Student, other.Student, StringComparison.Ordinal))
            {
                types.Add(ConflictType.StudentDoubleBooked);
            }

            if (string.Equals(lesson.TutorId, other.TutorId, StringComparison.Ordinal))
            {
                types.Add(ConflictType.TutorDoubleBooked);
            }

            if (string.Equals(lesson.RoomId, other.RoomId, StringComparison.Ordinal))
            {
                types.Add(ConflictType.RoomDoubleBooked);
            }

            return types;
        }

        public bool ClashesWith(Lesson other)
        {
            return lesson.ConflictTypesWith(other).Count > 0;
        }

        public Lesson NormalizeToUtc()
        {
            ArgumentNullException.ThrowIfNull(lesson);

            return lesson with
            {
                StartsAt = lesson.StartsAt.ToUniversalTime(), CancelledAt = lesson.CancelledAt?.ToUniversalTime()
            };
        }
    }
}
