using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeacherApp.Api.Domain;

namespace TeacherApp.Api.Data.Mappings;

public sealed class ExerciseAttemptMap : IEntityTypeConfiguration<ExerciseAttempt>
{
    public void Configure(EntityTypeBuilder<ExerciseAttempt> builder)
    {
        builder.ToTable("ExerciseAttempt");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SubmittedAnswer)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.AttemptedAt)
            .IsRequired();

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Exercise)
            .WithMany()
            .HasForeignKey(x => x.ExerciseId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.FinalExercise)
            .WithMany()
            .HasForeignKey(x => x.FinalExerciseId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.UserId, x.ExerciseId });
        builder.HasIndex(x => new { x.UserId, x.FinalExerciseId });
    }
}
