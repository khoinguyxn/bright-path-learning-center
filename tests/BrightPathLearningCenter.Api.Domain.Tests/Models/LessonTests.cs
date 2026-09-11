using BrightPathLearningCenter.Api.Domain.Common;
using BrightPathLearningCenter.Api.Domain.Extensions;
using BrightPathLearningCenter.Api.Domain.Models;
using BrightPathLearningCenter.Api.Domain.Services;

namespace BrightPathLearningCenter.Api.Domain.Tests.Models;

public sealed class LessonTests
{
    private static readonly DateTimeOffset Wednesday9Am =
        new(2026, 3, 4, 9, 0, 0, CentreTimeZone.Offset);

    [Fact]
    public void Create_NormalizesInstantsToUtcAndDerivesEnd()
    {
        Lesson lesson = LessonFactory.Create("L001", "Student", "T1", "R1", Wednesday9Am, 60);

        Assert.Equal(TimeSpan.Zero, lesson.StartsAt.Offset);
        Assert.Equal(new DateTime(2026, 3, 4, 2, 0, 0, DateTimeKind.Utc), lesson.StartsAt.UtcDateTime);
        Assert.Equal(new DateTime(2026, 3, 4, 3, 0, 0, DateTimeKind.Utc), lesson.EndsAt().UtcDateTime);
        Assert.Equal(LessonStatus.Booked, lesson.Status);
        Assert.True(lesson.IsActive());
    }

    [Fact]
    public void Restore_NormalizesCancelledAtToUtcAndKeepsStatus()
    {
        Lesson lesson = LessonFactory.Restore(
            "L002", "B", "T1", "R1", Wednesday9Am, 60, LessonStatus.Cancelled, Wednesday9Am.AddHours(-3), "note");

        Assert.Equal(LessonStatus.Cancelled, lesson.Status);
        Assert.False(lesson.IsActive());
        Assert.NotNull(lesson.CancelledAt);
        Assert.Equal(TimeSpan.Zero, lesson.CancelledAt!.Value.Offset);
        Assert.Equal("note", lesson.Note);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-30)]
    public void Create_WhenDurationNotPositive_Throws(int durationMinutes)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            LessonFactory.Create("L001", "Student", "T1", "R1", Wednesday9Am, durationMinutes));
    }

    [Fact]
    public void Create_WhenRequiredFieldMissing_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            LessonFactory.Create("L001", "  ", "T1", "R1", Wednesday9Am, 60));
    }

    [Fact]
    public void ToLessonId_PadsToThreeDigits()
    {
        Assert.Equal("L007", 7.ToLessonId());
        Assert.Equal("L035", 35.ToLessonId());
    }

    [Fact]
    public void Slot_ProjectsStartAndEnd()
    {
        Lesson lesson = LessonFactory.Create("L001", "Student", "T1", "R1", Wednesday9Am, 90);

        Assert.Equal(lesson.StartsAt, lesson.Slot().Start);
        Assert.Equal(lesson.EndsAt(), lesson.Slot().End);
    }

    [Fact]
    public void ClashesWith_WhenSharedResourceAndOverlap_ReturnsTrue()
    {
        Lesson first = LessonFactory.Create("L001", "A", "T1", "R1", Wednesday9Am, 60);
        Lesson second = LessonFactory.Create("L002", "B", "T1", "R2", Wednesday9Am, 60);

        Assert.True(first.ClashesWith(second));
    }

    [Fact]
    public void ClashesWith_WhenNoSharedResource_ReturnsFalse()
    {
        Lesson first = LessonFactory.Create("L001", "A", "T1", "R1", Wednesday9Am, 60);
        Lesson second = LessonFactory.Create("L002", "B", "T2", "R2", Wednesday9Am, 60);

        Assert.False(first.ClashesWith(second));
    }

    [Fact]
    public void ClashesWith_WhenSharedResourceButNoOverlap_ReturnsFalse()
    {
        Lesson first = LessonFactory.Create("L001", "A", "T1", "R1", Wednesday9Am, 60);
        Lesson second = LessonFactory.Create("L002", "B", "T1", "R2", Wednesday9Am.AddHours(2), 60);

        Assert.False(first.ClashesWith(second));
    }

    [Fact]
    public void ConflictTypesWith_WhenSharedResourceButNoOverlap_ReturnsEmpty()
    {
        Lesson first = LessonFactory.Create("L001", "A", "T1", "R1", Wednesday9Am, 60);
        Lesson second = LessonFactory.Create("L002", "A", "T1", "R1", Wednesday9Am.AddHours(2), 60);

        Assert.Empty(first.ConflictTypesWith(second));
    }

    [Fact]
    public void ConflictTypesWith_WhenEitherLessonCancelled_ReturnsEmpty()
    {
        Lesson active = LessonFactory.Create("L001", "A", "T1", "R1", Wednesday9Am, 60);
        Lesson cancelled = active with { Id = "L002", Status = LessonStatus.Cancelled };

        Assert.Empty(active.ConflictTypesWith(cancelled));
        Assert.Empty(cancelled.ConflictTypesWith(active));
    }

    [Fact]
    public void ConflictTypesWith_WhenOverlappingSharedResources_ReturnsEveryConflictType()
    {
        Lesson first = LessonFactory.Create("L001", "A", "T1", "R1", Wednesday9Am, 60);
        Lesson second = LessonFactory.Create("L002", "A", "T1", "R1", Wednesday9Am, 60);

        Assert.Equal(
            [ConflictType.StudentDoubleBooked, ConflictType.TutorDoubleBooked, ConflictType.RoomDoubleBooked],
            first.ConflictTypesWith(second));
    }

    [Fact]
    public void NormalizeToUtc_ConvertsOffsetsToZero()
    {
        Lesson lesson = new()
        {
            Id = "L001",
            Student = "A",
            TutorId = "T1",
            RoomId = "R1",
            StartsAt = Wednesday9Am,
            DurationMinutes = 60,
            CancelledAt = Wednesday9Am.AddHours(-3)
        };

        Lesson normalized = lesson.NormalizeToUtc();

        Assert.Equal(TimeSpan.Zero, normalized.StartsAt.Offset);
        Assert.NotNull(normalized.CancelledAt);
        Assert.Equal(TimeSpan.Zero, normalized.CancelledAt!.Value.Offset);
    }
}
