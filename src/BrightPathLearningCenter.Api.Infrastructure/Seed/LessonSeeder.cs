using System.Globalization;
using System.Reflection;

using BrightPathLearningCenter.Api.Domain.Common;
using BrightPathLearningCenter.Api.Domain.Models;
using BrightPathLearningCenter.Api.Domain.Services;
using BrightPathLearningCenter.Api.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace BrightPathLearningCenter.Api.Infrastructure.Seed;

public sealed class LessonSeeder(SchedulingDbContext dbContext)
{
    private const string LessonsResourceSuffix = "lessons_export.csv";
    private const string DateFormat = "yyyy-MM-dd";
    private const string TimeFormat = "HH:mm";

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await dbContext.Lessons.AnyAsync(cancellationToken))
        {
            return;
        }

        foreach (string line in ReadResourceLines(LessonsResourceSuffix).Skip(1))
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                dbContext.Lessons.Add(ParseLesson(line));
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static Lesson ParseLesson(string line)
    {
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

    private static IEnumerable<string> ReadResourceLines(string resourceSuffix)
    {
        Assembly assembly = typeof(LessonSeeder).Assembly;
        string resourceName = assembly
                                  .GetManifestResourceNames()
                                  .SingleOrDefault(name => name.EndsWith(resourceSuffix, StringComparison.Ordinal))
                              ?? throw new InvalidOperationException(
                                  $"Embedded resource ending with '{resourceSuffix}' was not found.");

        using Stream stream = assembly.GetManifestResourceStream(resourceName)
                              ?? throw new InvalidOperationException(
                                  $"Embedded resource '{resourceName}' could not be opened.");
        using StreamReader reader = new(stream);

        return ReadAllLines(reader);
    }

    private static List<string> ReadAllLines(TextReader reader)
    {
        List<string> lines = new();
        while (reader.ReadLine() is { } line)
        {
            lines.Add(line);
        }

        return lines;
    }
}
