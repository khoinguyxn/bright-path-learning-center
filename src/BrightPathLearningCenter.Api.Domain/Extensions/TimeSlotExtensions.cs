using BrightPathLearningCenter.Api.Domain.Models;

namespace BrightPathLearningCenter.Api.Domain.Extensions;

public static class TimeSlotExtensions
{
    public static bool Overlaps(this TimeSlot slot, TimeSlot other)
    {
        return slot.Start < other.End && other.Start < slot.End;
    }
}
