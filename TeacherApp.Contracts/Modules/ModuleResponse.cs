namespace TeacherApp.Contracts.Modules;

public sealed record ModuleResponse(Guid Id, string Title, string? Description, int Order, int LessonCount = 0);

