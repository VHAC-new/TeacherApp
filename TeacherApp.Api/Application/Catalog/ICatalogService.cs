using TeacherApp.Contracts.Exercises;
using TeacherApp.Contracts.FinalExercises;
using TeacherApp.Contracts.Lessons;
using TeacherApp.Contracts.Modules;

namespace TeacherApp.Api.Application.Catalog;

public interface ICatalogService
{
    Task<IReadOnlyList<ModuleResponse>> ListModulesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<LessonResponse>> ListLessonsByModuleAsync(Guid moduleId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ExerciseStudentResponse>> ListExercisesByLessonAsync(Guid lessonId, CancellationToken cancellationToken);
    Task<IReadOnlyList<FinalExerciseStudentResponse>> ListFinalExercisesByModuleAsync(Guid moduleId, CancellationToken cancellationToken);
}
