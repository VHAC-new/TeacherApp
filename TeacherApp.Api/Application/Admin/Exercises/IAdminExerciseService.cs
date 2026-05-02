using TeacherApp.Contracts.Exercises;

namespace TeacherApp.Api.Application.Admin.Exercises;

public interface IAdminExerciseService
{
    Task<IReadOnlyList<ExerciseResponse>> ListAsync(Guid? lessonId, CancellationToken cancellationToken);
    Task<ExerciseResponse?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<ExerciseResponse> CreateAsync(CreateExerciseRequest request, CancellationToken cancellationToken);
    Task<ExerciseResponse?> UpdateAsync(Guid id, UpdateExerciseRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
