using BrightPathLearningCenter.Api.Domain.Extensions;
using BrightPathLearningCenter.Api.Domain.Models;

namespace BrightPathLearningCenter.Api.Domain.Services;

public static class LessonClashDetector
{
    public static IReadOnlyList<LessonClash> Detect(Lesson candidate, IEnumerable<Lesson> overlappingLessons)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(overlappingLessons);

        return
        [
            .. from existing in overlappingLessons
            where candidate != existing &&
                  candidate.IsActive() &&
                  existing.IsActive() &&
                  candidate.Slot().Overlaps(existing.Slot())
            let types = candidate.ConflictTypesWith(existing)
            where types.Count > 0
            select new LessonClash { Existing = existing, Types = types }
        ];
    }

    public static bool HasClash(Lesson candidate, IEnumerable<Lesson> overlappingLessons)
    {
        return Detect(candidate, overlappingLessons).Count > 0;
    }
}
