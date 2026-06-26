using System.Net.Http.Json;
using TeacherApp.Contracts.Exercises;
using TeacherApp.Contracts.FinalExercises;
using TeacherApp.Contracts.Lessons;
using TeacherApp.Contracts.Modules;
using TeacherApp.Contracts.Trails;

namespace TeacherApp.App.Core.Services;

public sealed class CatalogService(HttpClient http)
{
    public async Task<List<ModuleResponse>> GetModulesAsync(CancellationToken ct = default) =>
        await http.GetFromJsonAsync<List<ModuleResponse>>("api/v1/modules", ct) ?? [];

    public async Task<List<TrailResponse>> GetTrailsAsync(Guid moduleId, CancellationToken ct = default) =>
        await http.GetFromJsonAsync<List<TrailResponse>>($"api/v1/modules/{moduleId}/trails", ct) ?? [];

    public async Task<List<LessonResponse>> GetLessonsAsync(Guid trailId, CancellationToken ct = default) =>
        await http.GetFromJsonAsync<List<LessonResponse>>($"api/v1/trails/{trailId}/lessons", ct) ?? [];

    public async Task<List<ExerciseStudentResponse>> GetExercisesAsync(Guid trailId, Guid lessonId, CancellationToken ct = default) =>
        await http.GetFromJsonAsync<List<ExerciseStudentResponse>>($"api/v1/trails/{trailId}/lessons/{lessonId}/exercises", ct) ?? [];

    public async Task<List<FinalExerciseStudentResponse>> GetFinalExercisesAsync(Guid trailId, CancellationToken ct = default) =>
        await http.GetFromJsonAsync<List<FinalExerciseStudentResponse>>($"api/v1/trails/{trailId}/final-exercises", ct) ?? [];
}
