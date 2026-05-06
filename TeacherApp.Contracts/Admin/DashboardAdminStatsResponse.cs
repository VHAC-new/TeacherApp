namespace TeacherApp.Contracts.Admin;

public sealed record DashboardAdminStatsResponse(
    int TotalStudents,
    int TotalModules,
    int TotalLessons);
