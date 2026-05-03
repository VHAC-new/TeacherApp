using TeacherApp.Contracts.Progress;

namespace TeacherApp.Api.Application.Progress;

public interface IProgressService
{
    Task<OverallProgressResponse> GetOverallAsync(Guid userId, CancellationToken cancellationToken);
    Task<ModuleProgressResponse?> GetModuleAsync(Guid userId, Guid moduleId, CancellationToken cancellationToken);
}
