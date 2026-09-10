namespace BrightPathLearningCenter.Api.Domain.Models;

public readonly record struct TimeSlot
{
    public required DateTimeOffset Start { get; init; }
    public required DateTimeOffset End { get; init; }
}
