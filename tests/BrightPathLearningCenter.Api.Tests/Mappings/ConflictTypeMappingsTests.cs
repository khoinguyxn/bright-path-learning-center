using BrightPathLearningCenter.Api.Domain.Models;
using BrightPathLearningCenter.Api.Mappings;

namespace BrightPathLearningCenter.Api.Tests.Mappings;

public sealed class ConflictTypeMappingsTests
{
    [Theory]
    [InlineData(ConflictType.StudentDoubleBooked, "student")]
    [InlineData(ConflictType.TutorDoubleBooked, "tutor")]
    [InlineData(ConflictType.RoomDoubleBooked, "room")]
    public void ToWireName_ReturnsExpectedName(ConflictType type, string expected)
    {
        Assert.Equal(expected, type.ToWireName());
    }

    [Fact]
    public void ToWireName_WhenUndefined_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ((ConflictType)99).ToWireName());
    }
}
