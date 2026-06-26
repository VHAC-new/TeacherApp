using System.Net.Http.Json;
using TeacherApp.Contracts.Progress;

namespace TeacherApp.App.Features.Home.Services;

public sealed class ProgressService(HttpClient http)
{
    public async Task<OverallProgressResponse> GetOverallAsync(CancellationToken ct = default) =>
        await http.GetFromJsonAsync<OverallProgressResponse>("api/v1/progress", ct)
        ?? throw new InvalidOperationException("Resposta inesperada.");

    public async Task<ModuleProgressResponse> GetModuleProgressAsync(Guid moduleId, CancellationToken ct = default) =>
        await http.GetFromJsonAsync<ModuleProgressResponse>($"api/v1/progress/modules/{moduleId}", ct)
        ?? throw new InvalidOperationException("Resposta inesperada.");

    public async Task<List<TrailProgressResponse>> GetTrailProgressAsync(Guid moduleId, CancellationToken ct = default) =>
        await http.GetFromJsonAsync<List<TrailProgressResponse>>($"api/v1/progress/modules/{moduleId}/trails", ct) ?? [];

    public async Task<List<LessonProgressResponse>> GetLessonProgressAsync(Guid trailId, CancellationToken ct = default) =>
        await http.GetFromJsonAsync<List<LessonProgressResponse>>($"api/v1/progress/trails/{trailId}/lessons", ct) ?? [];
}
