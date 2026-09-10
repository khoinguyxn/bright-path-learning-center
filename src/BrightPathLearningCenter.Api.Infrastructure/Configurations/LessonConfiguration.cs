using BrightPathLearningCenter.Api.Domain.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BrightPathLearningCenter.Api.Infrastructure.Configurations;

public sealed class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    private static readonly ValueConverter<DateTimeOffset, DateTime> UtcConverter = new(
        offset => offset.UtcDateTime,
        value => new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc)));

    public void Configure(EntityTypeBuilder<Lesson> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Lessons");

        builder.HasKey(lesson => lesson.Id);

        builder.Property(lesson => lesson.Id).HasMaxLength(16);
        builder.Property(lesson => lesson.Student).HasMaxLength(200).IsRequired();
        builder.Property(lesson => lesson.TutorId).HasMaxLength(16).IsRequired();
        builder.Property(lesson => lesson.RoomId).HasMaxLength(16).IsRequired();
        builder.Property(lesson => lesson.StartsAt).HasConversion(UtcConverter).IsRequired();
        builder.Property(lesson => lesson.DurationMinutes).IsRequired();
        builder.Property(lesson => lesson.Status).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(lesson => lesson.CancelledAt).HasConversion(UtcConverter);
        builder.Property(lesson => lesson.Note).HasMaxLength(500);

        builder.HasIndex(lesson => new { lesson.TutorId, lesson.StartsAt });
        builder.HasIndex(lesson => new { lesson.RoomId, lesson.StartsAt });
        builder.HasIndex(lesson => new { lesson.Student, lesson.StartsAt });
    }
}
