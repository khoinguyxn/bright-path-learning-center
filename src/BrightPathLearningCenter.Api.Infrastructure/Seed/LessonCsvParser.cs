using System.Globalization;

using BrightPathLearningCenter.Api.Domain.Common;
using BrightPathLearningCenter.Api.Domain.Models;
using BrightPathLearningCenter.Api.Domain.Services;

namespace BrightPathLearningCenter.Api.Infrastructure.Seed;

public static class LessonCsvParser
{
    private const string DateFormat = "yyyy-MM-dd";
    private const string TimeFormat = "HH:mm";

    public static Lesson Parse(string line)
    {
        ArgumentNullException.ThrowIfNull(line);

        string[] fields = line.Split(',');
        if (fields.Length < 8)
        {
            throw new FormatException($"Lesson row has {fields.Length} fields, expected at least 8: '{line}'.");
        }

        string id = fields[0];
        DateOnly date = DateOnly.ParseExact(fields[1], DateFormat, CultureInfo.InvariantCulture);
        TimeOnly time = TimeOnly.ParseExact(fields[2], TimeFormat, CultureInfo.InvariantCulture);
        int durationMinutes = int.Parse(fields[3], CultureInfo.InvariantCulture);
        string student = fields[4];
        string tutorId = fields[5];
        string roomId = fields[6];
        LessonStatus status = ParseStatus(fields[7]);
        DateTimeOffset? cancelledAt = ParseCancelledAt(fields.Length > 8 ? fields[8] : null);
        string? note = fields.Length > 9 && !string.IsNullOrWhiteSpace(fields[9]) ? fields[9] : null;

        DateTimeOffset startsAt = new(date.ToDateTime(time), CentreTimeZone.Offset);
        return LessonFactory.Restore(id, student, tutorId, roomId, startsAt, durationMinutes, status, cancelledAt,
            note);
    }

    private static LessonStatus ParseStatus(string value)
    {
        return value switch
        {
            "booked" => LessonStatus.Booked,
            "cancelled" => LessonStatus.Cancelled,
            "no_show" => LessonStatus.NoShow,
            _ => throw new FormatException($"Unknown lesson status '{value}'.")
        };
    }

    private static DateTimeOffset? ParseCancelledAt(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
    }
}
