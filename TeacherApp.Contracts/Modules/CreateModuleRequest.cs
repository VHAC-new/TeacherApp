namespace TeacherApp.Contracts.Modules;

public sealed record CreateModuleRequest(string Title, string? Description, int Order);

