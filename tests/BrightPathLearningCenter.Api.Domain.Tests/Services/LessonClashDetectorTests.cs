using BrightPathLearningCenter.Api.Domain.Common;
using BrightPathLearningCenter.Api.Domain.Models;
using BrightPathLearningCenter.Api.Domain.Services;

namespace BrightPathLearningCenter.Api.Domain.Tests.Services;

public sealed class LessonClashDetectorTests
{
    private static readonly DateTimeOffset Wednesday9Am =
        new(2026, 3, 4, 9, 0, 0, CentreTimeZone.Offset);

    private static readonly Lesson Candidate =
        LessonFactory.Create("L001", "A", "T1", "R1", Wednesday9Am, 60);

    [Theory]
    [InlineData("L002", "B", "T1", "R2", ConflictType.TutorDoubleBooked)]
    [InlineData("L002", "A", "T2", "R2", ConflictType.StudentDoubleBooked)]
    [InlineData("L002", "B", "T2", "R1", ConflictType.RoomDoubleBooked)]
    public void Detect_WhenSingleResourceOverlaps_ReturnsMatchingConflictType(
        string id,
        string student,
        string tutorId,
        string roomId,
        ConflictType expected)
    {
        Lesson existing = Candidate with { Id = id, Student = student, TutorId = tutorId, RoomId = roomId };

        LessonClash clash = Assert.Single(LessonClashDetector.Detect(Candidate, [existing]));

        Assert.Equal(id, clash.Existing.Id);
        Assert.Equal([expected], clash.Types);
    }

    [Fact]
    public void Detect_WhenAllResourcesOverlap_ReturnsEveryConflictType()
    {
        Lesson existing = Candidate with { Id = "L002" };

        LessonClash clash = Assert.Single(LessonClashDetector.Detect(Candidate, [existing]));

        Assert.Equal(
            [ConflictType.StudentDoubleBooked, ConflictType.TutorDoubleBooked, ConflictType.RoomDoubleBooked],
            clash.Types);
    }

    [Theory]
    [InlineData("L002", "B", "T2", "R2", 0, LessonStatus.Booked)]
    [InlineData("L002", "A", "T1", "R1", 1, LessonStatus.Booked)]
    [InlineData("L002", "A", "T1", "R1", 0, LessonStatus.Cancelled)]
    [InlineData("L001", "A", "T1", "R1", 0, LessonStatus.Booked)]
    public void Detect_WhenNotAClash_ReturnsEmpty(
        string id,
        string student,
        string tutorId,
        string roomId,
        int hourOffset,
        LessonStatus status)
    {
        Lesson existing = Candidate with
        {
            Id = id,
            Student = student,
            TutorId = tutorId,
            RoomId = roomId,
            StartsAt = Wednesday9Am.AddHours(hourOffset),
            Status = status
        };

        Assert.Empty(LessonClashDetector.Detect(Candidate, [existing]));
    }

    [Fact]
    public void Detect_WhenExistingIsNoShow_ReturnsClash()
    {
        Lesson existing = Candidate with { Id = "L002", Status = LessonStatus.NoShow };

        Assert.Single(LessonClashDetector.Detect(Candidate, [existing]));
    }

    [Fact]
    public void Detect_WhenCandidateIsCancelled_ReturnsEmpty()
    {
        Lesson cancelled = Candidate with { Status = LessonStatus.Cancelled };
        Lesson existing = Candidate with { Id = "L002" };

        Assert.Empty(LessonClashDetector.Detect(cancelled, [existing]));
    }

    [Fact]
    public void Detect_WhenResourcesSharedButSlotsDoNotOverlap_ReturnsEmpty()
    {
        Lesson existing = Candidate with { Id = "L002", StartsAt = Wednesday9Am.AddHours(2) };

        Assert.Empty(LessonClashDetector.Detect(Candidate, [existing]));
    }

    [Fact]
    public void Detect_AmongMultipleLessons_ReturnsOnlyOverlappingClashes()
    {
        Lesson nonOverlapping = Candidate with { Id = "L002", TutorId = "T1", StartsAt = Wednesday9Am.AddHours(2) };
        Lesson overlappingWithoutSharedResource =
            Candidate with { Id = "L003", Student = "B", TutorId = "T2", RoomId = "R2" };
        Lesson clashing = Candidate with { Id = "L004", Student = "B", TutorId = "T1", RoomId = "R3" };

        LessonClash clash = Assert.Single(
            LessonClashDetector.Detect(Candidate, [nonOverlapping, overlappingWithoutSharedResource, clashing]));

        Assert.Equal("L004", clash.Existing.Id);
    }

    [Theory]
    [InlineData("T1", true)]
    [InlineData("T2", false)]
    public void HasClash_ReflectsWhetherAnyClashExists(string tutorId, bool expected)
    {
        Lesson existing = Candidate with { Id = "L002", Student = "B", TutorId = tutorId, RoomId = "R2" };

        Assert.Equal(expected, LessonClashDetector.HasClash(Candidate, [existing]));
    }
}
