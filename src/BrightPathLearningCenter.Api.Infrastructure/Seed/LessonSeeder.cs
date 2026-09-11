using System.Reflection;

using BrightPathLearningCenter.Api.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace BrightPathLearningCenter.Api.Infrastructure.Seed;

public sealed class LessonSeeder(SchedulingDbContext dbContext)
{
    private const string LessonsResourceSuffix = "lessons_export.csv";

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
                dbContext.Lessons.Add(LessonCsvParser.Parse(line));
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
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
