using BrightPathLearningCenter.Api.Domain.Models;
using BrightPathLearningCenter.Api.Infrastructure.Seed;

namespace BrightPathLearningCenter.Api.Infrastructure.Tests.Seed;

public sealed class LessonCsvParserTests
{
    [Fact]
    public void Parse_MapsBookedRow()
    {
        Lesson lesson = LessonCsvParser.Parse("L001,2026-03-03,09:00,60,Le Minh Chau,T1,R1,booked,,");

        Assert.Equal("L001", lesson.Id);
        Assert.Equal("Le Minh Chau", lesson.Student);
        Assert.Equal("T1", lesson.TutorId);
        Assert.Equal("R1", lesson.RoomId);
        Assert.Equal(60, lesson.DurationMinutes);
        Assert.Equal(LessonStatus.Booked, lesson.Status);
        Assert.Null(lesson.CancelledAt);
        Assert.Null(lesson.Note);
        Assert.Equal(new DateTime(2026, 3, 3, 2, 0, 0, DateTimeKind.Utc), lesson.StartsAt.UtcDateTime);
    }

    [Fact]
    public void Parse_MapsCancelledRowWithCancelledAtAndNote()
    {
        Lesson lesson = LessonCsvParser.Parse(
            "L005,2026-03-03,14:00,60,Vu Ha My,T2,R2,cancelled,2026-03-03T08:15:00+07:00,family cancelled");

        Assert.Equal(LessonStatus.Cancelled, lesson.Status);
        Assert.NotNull(lesson.CancelledAt);
        Assert.Equal(TimeSpan.Zero, lesson.CancelledAt!.Value.Offset);
        Assert.Equal("family cancelled", lesson.Note);
    }

    [Fact]
    public void Parse_MapsNoShowRow()
    {
        Lesson lesson = LessonCsvParser.Parse("L015,2026-03-05,13:00,60,Tran Bao Long,T2,R2,no_show,,");

        Assert.Equal(LessonStatus.NoShow, lesson.Status);
    }

    [Fact]
    public void Parse_WhenFieldsMissing_Throws()
    {
        Assert.Throws<FormatException>(() => LessonCsvParser.Parse("L001,2026-03-03,09:00"));
    }

    [Fact]
    public void Parse_WhenStatusUnknown_Throws()
    {
        Assert.Throws<FormatException>(() =>
            LessonCsvParser.Parse("L001,2026-03-03,09:00,60,A,T1,R1,unknown,,"));
    }
}
