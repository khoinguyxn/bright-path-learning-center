using System.Globalization;

using BrightPathLearningCenter.Api.Domain.Models;

namespace BrightPathLearningCenter.Api.Domain.Extensions;

public static class LessonIdExtensions
{
    public static string ToLessonId(this int number)
    {
        return $"{Lesson.IdPrefix}{number.ToString($"D{Lesson.IdDigits}", CultureInfo.InvariantCulture)}";
    }
}
