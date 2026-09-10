using BrightPathLearningCenter.Api.Contracts;
using BrightPathLearningCenter.Api.Domain.Abstractions;
using BrightPathLearningCenter.Api.Domain.Models;
using BrightPathLearningCenter.Api.Domain.Services;
using BrightPathLearningCenter.Api.Mappings;

using FluentValidation;
using FluentValidation.Results;

namespace BrightPathLearningCenter.Api.Endpoints;

public static class LessonEndpoints
{
    public static void MapLessonEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapPost("/lessons", CreateLessonAsync)
            .WithName("CreateLesson")
            .Produces<LessonResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> CreateLessonAsync(
        CreateLessonRequest request,
        IValidator<CreateLessonRequest> validator,
        ILessonRepository repository,
        CancellationToken cancellationToken)
    {
        ValidationResult validation = await validator.ValidateAsync(request, cancellationToken);

        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        string lessonId = await repository.NextLessonIdAsync(cancellationToken);
        Lesson lesson = LessonFactory.Create(
            lessonId,
            request.Student.Trim(),
            request.TutorId.Trim(),
            request.RoomId.Trim(),
            request.StartsAt,
            request.DurationMinutes,
            request.Note);

        IReadOnlyList<Lesson> lessons = await repository.GetAllAsync(cancellationToken);
        IReadOnlyList<LessonClash> clashes = LessonClashDetector.Detect(lesson, lessons);

        if (clashes.Count > 0)
        {
            return BuildConflictResult(lesson, clashes);
        }

        await repository.AddAsync(lesson, cancellationToken);

        return Results.Created($"/lessons/{lesson.Id}", lesson.ToResponse());
    }

    private static IResult BuildConflictResult(Lesson lesson, IReadOnlyList<LessonClash> clashes)
    {
        IEnumerable<LessonConflictResponse> conflicts = clashes
            .Select(clash => new LessonConflictResponse
            {
                LessonId = clash.Existing.Id, Types = clash.Types.Select(type => type.ToWireName())
            });

        return Results.Problem(
            title: "Booking clashes with an existing lesson",
            detail:
            $"Lesson {lesson.Id} overlaps {clashes.Count} existing lesson(s) on the same student, tutor or room.",
            statusCode: StatusCodes.Status409Conflict,
            extensions: new Dictionary<string, object?> { ["conflicts"] = conflicts });
    }
}
