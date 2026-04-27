using Microsoft.EntityFrameworkCore;

namespace TeacherApp.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Mappings will be added in Data/Mappings as entities are introduced.
    }
}

