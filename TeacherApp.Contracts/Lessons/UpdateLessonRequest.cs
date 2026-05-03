namespace TeacherApp.Contracts.Lessons;

public sealed record UpdateLessonRequest(
    string Title,
    string? Description,
    int Order,
    Guid? AudioMediaId = null);
