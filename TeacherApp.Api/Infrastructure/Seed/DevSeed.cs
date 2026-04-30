using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TeacherApp.Api.Common;
using TeacherApp.Api.Data;
using TeacherApp.Api.Domain;

namespace TeacherApp.Api.Infrastructure.Seed;

public static class DevSeed
{
    public static async Task TrySeedAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DevSeed");

        try
        {
            var db = services.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync(cancellationToken);

            var adminPassword = Environment.GetEnvironmentVariable("TEACHERAPP_ADMIN_PASSWORD");
            if (string.IsNullOrWhiteSpace(adminPassword))
            {
                logger.LogWarning("TEACHERAPP_ADMIN_PASSWORD not set. Skipping dev admin seed.");
                return;
            }

            var adminEmail = "admin@appdiego.local";
            var normalizedAdminEmail = adminEmail.ToLowerInvariant();

            var exists = await db.Users.AnyAsync(x => x.Email.ToLower() == normalizedAdminEmail, cancellationToken);
            if (exists)
            {
                return;
            }

            var hasher = new PasswordHasher<User>();
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = adminEmail,
                Role = Roles.Admin,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
            };

            user.PasswordHash = hasher.HashPassword(user, adminPassword);

            db.Users.Add(user);
            await db.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Seeded dev admin user {Email} with role {Role}.", adminEmail, Roles.Admin);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Dev seed skipped due to an error (db may be unavailable).");
        }
    }
}

