using TeacherApp.Contracts.Admin;

namespace TeacherApp.Api.Application.Admin.Dashboard;

public interface IDashboardAdminService
{
    Task<DashboardAdminStatsResponse> GetStatsAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<AdminStudentResponse>> ListStudentsAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<CompletedLessonAdminRowResponse>> ListCompletedLessonsAsync(
        Guid? studentId,
        int take,
        CancellationToken cancellationToken);
}
