using System.Net;
using System.Net.Http.Json;
using TeacherApp.Contracts.Modules;
using TeacherApp.Tests.Fixtures;

namespace TeacherApp.Tests;

public sealed class AdminModulesIntegrationTests : IClassFixture<TestWebAppFactory>
{
    private readonly TestWebAppFactory _factory;

    public AdminModulesIntegrationTests(TestWebAppFactory factory) => _factory = factory;

    [Fact]
    public async Task Admin_crud_modules_full_lifecycle()
    {
        var client = _factory.CreateAuthenticatedClient(Guid.NewGuid(), "admin@test.com", "Admin");

        var createResponse = await client.PostAsJsonAsync("/api/v1/admin/modules",
            new CreateModuleRequest("Module 1", "First module", 1));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ModuleResponse>();
        Assert.NotNull(created);
        Assert.Equal("Module 1", created.Title);
        Assert.Equal(0, created.LessonCount);

        var modules = await client.GetFromJsonAsync<List<ModuleResponse>>("/api/v1/admin/modules");
        Assert.NotNull(modules);
        Assert.Single(modules);
        Assert.Equal(0, modules[0].LessonCount);

        var updateResponse = await client.PutAsJsonAsync($"/api/v1/admin/modules/{created.Id}",
            new UpdateModuleRequest("Module 1 Updated", "Updated desc", 1));
        Assert.True(updateResponse.IsSuccessStatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<ModuleResponse>();
        Assert.NotNull(updated);
        Assert.Equal("Module 1 Updated", updated.Title);

        var deleteResponse = await client.DeleteAsync($"/api/v1/admin/modules/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        modules = await client.GetFromJsonAsync<List<ModuleResponse>>("/api/v1/admin/modules");
        Assert.NotNull(modules);
        Assert.Empty(modules);
    }

    [Fact]
    public async Task Non_admin_cannot_access_admin_modules()
    {
        var client = _factory.CreateAuthenticatedClient(Guid.NewGuid(), "student@test.com", "Student");
        var response = await client.GetAsync("/api/v1/admin/modules");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
