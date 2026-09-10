using BrightPathLearningCenter.Api.Domain.Models;

namespace BrightPathLearningCenter.Api.Mappings;

public static class ConflictTypeMappings
{
    extension(ConflictType type)
    {
        public string ToWireName()
        {
            return type switch
            {
                ConflictType.StudentDoubleBooked => "student",
                ConflictType.TutorDoubleBooked => "tutor",
                ConflictType.RoomDoubleBooked => "room",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown conflict type.")
            };
        }
    }
}
