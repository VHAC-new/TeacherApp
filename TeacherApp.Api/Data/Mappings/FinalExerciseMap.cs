using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeacherApp.Api.Domain;

namespace TeacherApp.Api.Data.Mappings;

public sealed class FinalExerciseMap : IEntityTypeConfiguration<FinalExercise>
{
    public void Configure(EntityTypeBuilder<FinalExercise> builder)
    {
        builder.ToTable("FinalExercise");

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

        builder.HasIndex(x => new { x.ModuleId, x.Order })
            .IsUnique();

        builder.HasOne(x => x.Module)
            .WithMany()
            .HasForeignKey(x => x.ModuleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
