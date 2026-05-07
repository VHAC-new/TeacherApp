using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeacherApp.Api.Domain;

namespace TeacherApp.Api.Data.Mappings;

public sealed class ModuleMap : IEntityTypeConfiguration<Module>
{
    public void Configure(EntityTypeBuilder<Module> builder)
    {
        builder.ToTable("Module");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.Property(x => x.Order)
            .IsRequired();

        builder.HasIndex(x => x.Order)
            .IsUnique();

        builder.Property(x => x.CreatedAt)
            .IsRequired();
    }
}

