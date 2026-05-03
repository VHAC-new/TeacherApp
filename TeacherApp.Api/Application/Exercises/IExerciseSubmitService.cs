using TeacherApp.Contracts.Exercises;
using TeacherApp.Contracts.FinalExercises;

namespace TeacherApp.Api.Application.Exercises;

public interface IExerciseSubmitService
{
    Task<SubmitExerciseResponse?> SubmitAsync(Guid exerciseId, Guid userId, SubmitExerciseRequest request, CancellationToken cancellationToken);
    Task<SubmitFinalExerciseResponse?> SubmitFinalAsync(Guid finalExerciseId, Guid userId, SubmitFinalExerciseRequest request, CancellationToken cancellationToken);
}
