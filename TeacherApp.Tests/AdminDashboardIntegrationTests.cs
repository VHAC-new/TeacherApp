using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TeacherApp.Api.Common;
using TeacherApp.Api.Data;
using TeacherApp.Api.Domain;
using TeacherApp.Contracts.Admin;
using TeacherApp.Contracts.Exercises;
using TeacherApp.Contracts.Lessons;
using TeacherApp.Contracts.Modules;
using TeacherApp.Tests.Fixtures;

namespace TeacherApp.Tests;

public sealed class AdminDashboardIntegrationTests : IClassFixture<TestWebAppFactory>
{
    private readonly TestWebAppFactory _factory;

    public AdminDashboardIntegrationTests(TestWebAppFactory factory) => _factory = factory;

    [Fact]
    public async Task Dashboard_stats_and_completed_lessons_reflect_student_progress()
    {
        var studentId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = new PasswordHasher<User>();
            var user = new User
            {
                Id = studentId,
                Email = "dash.student@test.local",
                Role = Roles.Student,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
            };
            user.PasswordHash = hasher.HashPassword(user, "password123!");
            db.Users.Add(user);
            await db.SaveChangesAsync();
        }

        var adminClient = _factory.CreateAuthenticatedClient(Guid.NewGuid(), "admin@test.com", "Admin");

        var modResponse = await adminClient.PostAsJsonAsync("/api/v1/admin/modules",
            new CreateModuleRequest("Dash Module", null, 1));
        var module = await modResponse.Content.ReadFromJsonAsync<ModuleResponse>();
        Assert.NotNull(module);

        var lessonResponse = await adminClient.PostAsJsonAsync("/api/v1/admin/lessons",
            new CreateLessonRequest(module!.Id, "Dash Lesson", null, 1));
        var lesson = await lessonResponse.Content.ReadFromJsonAsync<LessonResponse>();
        Assert.NotNull(lesson);

        var ex1Response = await adminClient.PostAsJsonAsync("/api/v1/admin/exercises",
            new CreateExerciseRequest(lesson!.Id, "Q1?", "a", null, null, true, true, 1));
        var ex1 = await ex1Response.Content.ReadFromJsonAsync<ExerciseResponse>();

        var ex2Response = await adminClient.PostAsJsonAsync("/api/v1/admin/exercises",
            new CreateExerciseRequest(lesson.Id, "Q2?", "b", null, null, true, true, 2));
        var ex2 = await ex2Response.Content.ReadFromJsonAsync<ExerciseResponse>();

        Assert.NotNull(ex1);
        Assert.NotNull(ex2);

        var studentClient = _factory.CreateAuthenticatedClient(studentId, "dash.student@test.local", "Student");

        var submit1 = await studentClient.PostAsJsonAsync($"/api/v1/exercises/{ex1!.Id}/submit",
            new SubmitExerciseRequest("a"));
        Assert.True(submit1.IsSuccessStatusCode);

        var submit2 = await studentClient.PostAsJsonAsync($"/api/v1/exercises/{ex2!.Id}/submit",
            new SubmitExerciseRequest("b"));
        Assert.True(submit2.IsSuccessStatusCode);

        var statsResponse = await adminClient.GetAsync("/api/v1/admin/dashboard/stats");
        Assert.True(statsResponse.IsSuccessStatusCode);
        var stats = await statsResponse.Content.ReadFromJsonAsync<DashboardAdminStatsResponse>();
        Assert.NotNull(stats);
        Assert.Equal(1, stats.TotalStudents);
        Assert.Equal(1, stats.TotalModules);
        Assert.Equal(1, stats.TotalLessons);

        var studentsResponse = await adminClient.GetFromJsonAsync<List<AdminStudentResponse>>("/api/v1/admin/users/students");
        Assert.NotNull(studentsResponse);
        Assert.Contains(studentsResponse, s => s.Id == studentId);

        var completedResponse = await adminClient.GetAsync("/api/v1/admin/dashboard/completed-lessons?take=50");
        Assert.True(completedResponse.IsSuccessStatusCode);
        var completed = await completedResponse.Content.ReadFromJsonAsync<List<CompletedLessonAdminRowResponse>>();
        Assert.NotNull(completed);
        Assert.Single(completed);
        Assert.Equal(studentId, completed[0].StudentId);
        Assert.Equal("dash.student@test.local", completed[0].StudentEmail);
        Assert.Equal(lesson.Id, completed[0].LessonId);
        Assert.Equal("Dash Lesson", completed[0].LessonTitle);
        Assert.Equal(module.Id, completed[0].ModuleId);
        Assert.Equal("Dash Module", completed[0].ModuleTitle);

        var filteredResponse = await adminClient.GetAsync(
            $"/api/v1/admin/dashboard/completed-lessons?take=50&studentId={studentId:D}");
        Assert.True(filteredResponse.IsSuccessStatusCode);
        var filtered = await filteredResponse.Content.ReadFromJsonAsync<List<CompletedLessonAdminRowResponse>>();
        Assert.NotNull(filtered);
        Assert.Single(filtered);

        var otherStudentClient = _factory.CreateAuthenticatedClient(Guid.NewGuid(), "other@test.com", "Student");
        var forbiddenStudents = await otherStudentClient.GetAsync("/api/v1/admin/dashboard/stats");
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenStudents.StatusCode);
    }
}
