using System.Net.Http.Json;
using TeacherApp.Contracts.Progress;

namespace TeacherApp.App.Services;

public sealed class ProgressService(HttpClient http)
{
    public async Task<OverallProgressResponse> GetOverallAsync(CancellationToken ct = default) =>
        await http.GetFromJsonAsync<OverallProgressResponse>("api/v1/progress", ct)
        ?? throw new InvalidOperationException("Resposta inesperada.");

    public async Task<ModuleProgressResponse> GetModuleProgressAsync(Guid moduleId, CancellationToken ct = default) =>
        await http.GetFromJsonAsync<ModuleProgressResponse>($"api/v1/progress/modules/{moduleId}", ct)
        ?? throw new InvalidOperationException("Resposta inesperada.");
}
