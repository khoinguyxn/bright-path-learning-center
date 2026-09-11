using BrightPathLearningCenter.Api.Domain.Models;
using BrightPathLearningCenter.Api.Mappings;

namespace BrightPathLearningCenter.Api.Tests.Mappings;

public sealed class LessonStatusMappingsTests
{
    [Theory]
    [InlineData(LessonStatus.Booked, "booked")]
    [InlineData(LessonStatus.Cancelled, "cancelled")]
    [InlineData(LessonStatus.NoShow, "no_show")]
    public void ToWireName_ReturnsExpectedName(LessonStatus status, string expected)
    {
        Assert.Equal(expected, status.ToWireName());
    }

    [Fact]
    public void ToWireName_WhenUndefined_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ((LessonStatus)99).ToWireName());
    }
}
