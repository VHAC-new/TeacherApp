using System.Net.Http.Json;
using TeacherApp.Contracts.Exercises;
using TeacherApp.Contracts.FinalExercises;

namespace TeacherApp.App.Services;

public sealed class ExerciseService(HttpClient http)
{
    public async Task<SubmitExerciseResponse> SubmitAsync(Guid exerciseId, string answer, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync($"api/v1/exercises/{exerciseId}/submit",
            new SubmitExerciseRequest(answer), ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SubmitExerciseResponse>(ct)
            ?? throw new InvalidOperationException("Resposta inesperada.");
    }

    public async Task<SubmitFinalExerciseResponse> SubmitFinalAsync(Guid exerciseId, string answer, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync($"api/v1/final-exercises/{exerciseId}/submit",
            new SubmitFinalExerciseRequest(answer), ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SubmitFinalExerciseResponse>(ct)
            ?? throw new InvalidOperationException("Resposta inesperada.");
    }
}
