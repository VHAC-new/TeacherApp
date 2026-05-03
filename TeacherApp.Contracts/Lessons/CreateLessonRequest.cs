namespace TeacherApp.Contracts.Lessons;

public sealed record CreateLessonRequest(
    Guid ModuleId,
    string Title,
    string? Description,
    int Order,
    Guid? AudioMediaId = null);
