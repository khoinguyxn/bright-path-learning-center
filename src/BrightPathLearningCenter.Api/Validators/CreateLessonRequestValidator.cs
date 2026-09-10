using BrightPathLearningCenter.Api.Contracts;
using BrightPathLearningCenter.Api.Domain.Models;

using FluentValidation;

namespace BrightPathLearningCenter.Api.Validators;

public sealed class CreateLessonRequestValidator : AbstractValidator<CreateLessonRequest>
{
    public CreateLessonRequestValidator()
    {
        RuleFor(request => request.Student).NotEmpty();
        RuleFor(request => request.TutorId).NotEmpty();
        RuleFor(request => request.RoomId).NotEmpty();
        RuleFor(request => request.StartsAt).NotEqual(default(DateTimeOffset));
        RuleFor(request => request.DurationMinutes)
            .GreaterThan(0)
            .LessThanOrEqualTo(Lesson.MaxDurationMinutes);
    }
}
