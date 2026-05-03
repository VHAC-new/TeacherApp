using System.Net;
using System.Net.Http.Json;
using TeacherApp.Contracts.Modules;
using TeacherApp.Tests.Fixtures;

namespace TeacherApp.Tests;

public sealed class CatalogIntegrationTests : IClassFixture<TestWebAppFactory>
{
    private readonly TestWebAppFactory _factory;

    public CatalogIntegrationTests(TestWebAppFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_modules_requires_authentication()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/modules");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_modules_returns_empty_list()
    {
        var client = _factory.CreateAuthenticatedClient(Guid.NewGuid(), "student@test.com", "Student");
        var modules = await client.GetFromJsonAsync<List<ModuleResponse>>("/api/v1/modules");
        Assert.NotNull(modules);
        Assert.Empty(modules);
    }
}
