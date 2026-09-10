using BrightPathLearningCenter.Api.Domain.Common;
using BrightPathLearningCenter.Api.Domain.Extensions;
using BrightPathLearningCenter.Api.Domain.Models;

namespace BrightPathLearningCenter.Api.Domain.Tests.Models;

public sealed class TimeSlotTests
{
    private static TimeSlot Slot(int hour, int durationMinutes = 60)
    {
        DateTimeOffset start = new(2026, 3, 4, hour, 0, 0, CentreTimeZone.Offset);
        return new TimeSlot { Start = start, End = start.AddMinutes(durationMinutes) };
    }

    [Fact]
    public void Overlaps_WhenFullyDisjoint_ReturnsFalse()
    {
        Assert.False(Slot(9).Overlaps(Slot(11)));
    }

    [Fact]
    public void Overlaps_WhenTouchingAtEnd_IsBackToBack_ReturnsFalse()
    {
        Assert.False(Slot(9).Overlaps(Slot(10)));
    }

    [Fact]
    public void Overlaps_WhenTouchingAtStart_IsBackToBack_ReturnsFalse()
    {
        Assert.False(Slot(10).Overlaps(Slot(9)));
    }

    [Fact]
    public void Overlaps_WhenIdentical_ReturnsTrue()
    {
        Assert.True(Slot(9).Overlaps(Slot(9)));
    }

    [Fact]
    public void Overlaps_WhenContainedWithinOther_ReturnsTrue()
    {
        Assert.True(Slot(9, 30).Overlaps(Slot(9)));
    }

    [Fact]
    public void Overlaps_WhenContainingOther_ReturnsTrue()
    {
        Assert.True(Slot(9).Overlaps(Slot(9, 30)));
    }
}
