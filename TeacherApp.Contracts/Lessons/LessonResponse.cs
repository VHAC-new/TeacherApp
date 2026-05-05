namespace TeacherApp.Contracts.Lessons;

public sealed record LessonResponse(
    Guid Id,
    Guid ModuleId,
    string Title,
    string? Description,
    int Order,
    Guid? AudioMediaId,
    string? AudioFileName = null);
