using System.Net;
using System.Net.Http.Json;
using TeacherApp.Contracts.Exercises;
using TeacherApp.Contracts.Lessons;
using TeacherApp.Contracts.Modules;
using TeacherApp.Tests.Fixtures;

namespace TeacherApp.Tests;

public sealed class ExerciseSubmitIntegrationTests : IClassFixture<TestWebAppFactory>
{
    private readonly TestWebAppFactory _factory;

    public ExerciseSubmitIntegrationTests(TestWebAppFactory factory) => _factory = factory;

    [Fact]
    public async Task Submit_exercise_returns_correct_result()
    {
        var adminClient = _factory.CreateAuthenticatedClient(Guid.NewGuid(), "admin@test.com", "Admin");

        var modResponse = await adminClient.PostAsJsonAsync("/api/v1/admin/modules",
            new CreateModuleRequest("Test Module", null, 1));
        var module = await modResponse.Content.ReadFromJsonAsync<ModuleResponse>();

        var lessonResponse = await adminClient.PostAsJsonAsync("/api/v1/admin/lessons",
            new TeacherApp.Contracts.Lessons.CreateLessonRequest(module!.Id, "Test Lesson", null, 1));
        var lesson = await lessonResponse.Content.ReadFromJsonAsync<LessonResponse>();

        var exResponse = await adminClient.PostAsJsonAsync("/api/v1/admin/exercises",
            new CreateExerciseRequest(lesson!.Id, "What is 2+2?", "4", "It's simple math", "2+2=4", true, true, 1));
        var exercise = await exResponse.Content.ReadFromJsonAsync<ExerciseResponse>();

        var studentClient = _factory.CreateAuthenticatedClient(Guid.NewGuid(), "student@test.com", "Student");

        var submitResponse = await studentClient.PostAsJsonAsync($"/api/v1/exercises/{exercise!.Id}/submit",
            new SubmitExerciseRequest("4"));
        Assert.True(submitResponse.IsSuccessStatusCode);
        var result = await submitResponse.Content.ReadFromJsonAsync<SubmitExerciseResponse>();
        Assert.NotNull(result);
        Assert.True(result.IsCorrect);

        var wrongResponse = await studentClient.PostAsJsonAsync($"/api/v1/exercises/{exercise.Id}/submit",
            new SubmitExerciseRequest("5"));
        var wrongResult = await wrongResponse.Content.ReadFromJsonAsync<SubmitExerciseResponse>();
        Assert.NotNull(wrongResult);
        Assert.False(wrongResult.IsCorrect);
    }

    [Fact]
    public async Task Submit_to_nonexistent_exercise_returns_not_found()
    {
        var client = _factory.CreateAuthenticatedClient(Guid.NewGuid(), "student@test.com", "Student");
        var response = await client.PostAsJsonAsync($"/api/v1/exercises/{Guid.NewGuid()}/submit",
            new SubmitExerciseRequest("answer"));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
