using TeacherApp.Contracts.Modules;

namespace TeacherApp.Api.Application.Admin.Modules;

public interface IAdminModuleService
{
    Task<IReadOnlyList<ModuleResponse>> ListAsync(CancellationToken cancellationToken);
    Task<ModuleResponse?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<ModuleResponse> CreateAsync(CreateModuleRequest request, CancellationToken cancellationToken);
    Task<ModuleResponse?> UpdateAsync(Guid id, UpdateModuleRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}

