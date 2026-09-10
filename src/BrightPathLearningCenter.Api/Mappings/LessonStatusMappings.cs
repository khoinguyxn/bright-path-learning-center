using BrightPathLearningCenter.Api.Domain.Models;

namespace BrightPathLearningCenter.Api.Mappings;

public static class LessonStatusMappings
{
    extension(LessonStatus status)
    {
        public string ToWireName()
        {
            return status switch
            {
                LessonStatus.Booked => "booked",
                LessonStatus.Cancelled => "cancelled",
                LessonStatus.NoShow => "no_show",
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown lesson status.")
            };
        }
    }
}
