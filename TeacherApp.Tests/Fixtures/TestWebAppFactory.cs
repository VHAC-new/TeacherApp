using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TeacherApp.Api.Data;
using TeacherApp.Api.Infrastructure.Storage;

namespace TeacherApp.Tests.Fixtures;

public sealed class TestWebAppFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"TestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("TEACHERAPP_JWT_SECRET", JwtTestHelper.Secret);

        builder.UseSetting("ConnectionStrings:DefaultConnection",
            "Host=not-used;Database=not-used");

        builder.UseSetting("Media:S3:Bucket", "");

        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var toRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<AppDbContext>)
                    || d.ServiceType == typeof(AppDbContext)
                    || d.ServiceType.IsGenericType &&
                       d.ServiceType.GetGenericTypeDefinition().FullName?.Contains("DbContextOptions") == true
                    || d.ServiceType.FullName?.Contains("EntityFrameworkCore") == true
                    || d.ImplementationType?.FullName?.Contains("Npgsql") == true)
                .ToList();
            foreach (var d in toRemove) services.Remove(d);

            services.AddDbContext<AppDbContext>(opts =>
                opts.UseInMemoryDatabase(_dbName));

            services.AddSingleton<IS3MediaOperations, DisabledS3MediaOperations>();
        });
    }

    public HttpClient CreateAuthenticatedClient(Guid userId, string email, params string[] roles)
    {
        var client = CreateClient();
        var token = JwtTestHelper.CreateToken(userId, email, roles);
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
