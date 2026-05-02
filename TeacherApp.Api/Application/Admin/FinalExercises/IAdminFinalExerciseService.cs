using TeacherApp.Contracts.FinalExercises;

namespace TeacherApp.Api.Application.Admin.FinalExercises;

public interface IAdminFinalExerciseService
{
    Task<IReadOnlyList<FinalExerciseResponse>> ListAsync(Guid? moduleId, CancellationToken cancellationToken);
    Task<FinalExerciseResponse?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<FinalExerciseResponse> CreateAsync(CreateFinalExerciseRequest request, CancellationToken cancellationToken);
    Task<FinalExerciseResponse?> UpdateAsync(Guid id, UpdateFinalExerciseRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
