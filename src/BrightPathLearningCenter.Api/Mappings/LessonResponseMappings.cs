using BrightPathLearningCenter.Api.Contracts;
using BrightPathLearningCenter.Api.Domain.Common;
using BrightPathLearningCenter.Api.Domain.Extensions;
using BrightPathLearningCenter.Api.Domain.Models;

namespace BrightPathLearningCenter.Api.Mappings;

public static class LessonResponseMappings
{
    extension(Lesson lesson)
    {
        public LessonResponse ToResponse()
        {
            return new LessonResponse
            {
                Id = lesson.Id,
                Student = lesson.Student,
                TutorId = lesson.TutorId,
                RoomId = lesson.RoomId,
                StartsAt = lesson.StartsAt.ToOffset(CentreTimeZone.Offset),
                EndsAt = lesson.EndsAt().ToOffset(CentreTimeZone.Offset),
                DurationMinutes = lesson.DurationMinutes,
                Status = lesson.Status.ToWireName(),
                Note = lesson.Note
            };
        }
    }
}
