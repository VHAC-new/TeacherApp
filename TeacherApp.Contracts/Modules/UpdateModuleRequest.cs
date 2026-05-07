namespace TeacherApp.Contracts.Modules;

public sealed record UpdateModuleRequest(string Title, string? Description, int Order);

