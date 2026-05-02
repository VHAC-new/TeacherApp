using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeacherApp.Api.Domain;

namespace TeacherApp.Api.Data.Mappings;

public sealed class ExerciseMap : IEntityTypeConfiguration<Exercise>
{
    public void Configure(EntityTypeBuilder<Exercise> builder)
    {
        builder.ToTable("Exercise");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Prompt)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.ExpectedAnswer)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Hint)
            .HasMaxLength(1000);

        builder.Property(x => x.Explanation)
            .HasMaxLength(2000);

        builder.Property(x => x.Order)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => new { x.LessonId, x.Order })
            .IsUnique();

        builder.HasOne(x => x.Lesson)
            .WithMany()
            .HasForeignKey(x => x.LessonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
