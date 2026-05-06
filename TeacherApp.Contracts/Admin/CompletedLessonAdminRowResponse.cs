namespace TeacherApp.Contracts.Admin;

public sealed record CompletedLessonAdminRowResponse(
    Guid StudentId,
    string StudentEmail,
    Guid LessonId,
    string LessonTitle,
    Guid ModuleId,
    string ModuleTitle,
    DateTimeOffset CompletedAt);
