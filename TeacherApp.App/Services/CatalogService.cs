using System.Net.Http.Json;
using TeacherApp.Contracts.Exercises;
using TeacherApp.Contracts.FinalExercises;
using TeacherApp.Contracts.Lessons;
using TeacherApp.Contracts.Modules;

namespace TeacherApp.App.Services;

public sealed class CatalogService(HttpClient http)
{
    public async Task<List<ModuleResponse>> GetModulesAsync(CancellationToken ct = default) =>
        await http.GetFromJsonAsync<List<ModuleResponse>>("api/v1/modules", ct) ?? [];

    public async Task<List<LessonResponse>> GetLessonsAsync(Guid moduleId, CancellationToken ct = default) =>
        await http.GetFromJsonAsync<List<LessonResponse>>($"api/v1/modules/{moduleId}/lessons", ct) ?? [];

    public async Task<List<ExerciseStudentResponse>> GetExercisesAsync(Guid moduleId, Guid lessonId, CancellationToken ct = default) =>
        await http.GetFromJsonAsync<List<ExerciseStudentResponse>>($"api/v1/modules/{moduleId}/lessons/{lessonId}/exercises", ct) ?? [];

    public async Task<List<FinalExerciseStudentResponse>> GetFinalExercisesAsync(Guid moduleId, CancellationToken ct = default) =>
        await http.GetFromJsonAsync<List<FinalExerciseStudentResponse>>($"api/v1/modules/{moduleId}/final-exercises", ct) ?? [];
}
