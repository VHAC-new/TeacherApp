namespace TeacherApp.Contracts.Progress;

public sealed record OverallProgressResponse(IReadOnlyList<ModuleProgressResponse> Modules);
