using Microsoft.EntityFrameworkCore;
using TeacherApp.Api.Data;
using TeacherApp.Contracts.Exercises;
using TeacherApp.Contracts.FinalExercises;
using TeacherApp.Contracts.Lessons;
using TeacherApp.Contracts.Modules;

namespace TeacherApp.Api.Application.Catalog;

public sealed class CatalogService(AppDbContext db) : ICatalogService
{
    public async Task<IReadOnlyList<ModuleResponse>> ListModulesAsync(CancellationToken cancellationToken)
    {
        return await db.Modules
            .AsNoTracking()
            .OrderBy(x => x.Order)
            .Select(x => new ModuleResponse(x.Id, x.Title, x.Description, x.Order,
                db.Lessons.Count(l => l.ModuleId == x.Id)))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LessonResponse>> ListLessonsByModuleAsync(Guid moduleId, CancellationToken cancellationToken)
    {
        return await db.Lessons
            .AsNoTracking()
            .Where(x => x.ModuleId == moduleId)
            .OrderBy(x => x.Order)
            .Select(x => new LessonResponse(x.Id, x.ModuleId, x.Title, x.Description, x.Order, x.AudioMediaId, null))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ExerciseStudentResponse>> ListExercisesByLessonAsync(Guid lessonId, CancellationToken cancellationToken)
    {
        return await db.Exercises
            .AsNoTracking()
            .Where(x => x.LessonId == lessonId)
            .OrderBy(x => x.Order)
            .Select(x => new ExerciseStudentResponse(x.Id, x.LessonId, x.Prompt, x.Hint, x.Order))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FinalExerciseStudentResponse>> ListFinalExercisesByModuleAsync(Guid moduleId, CancellationToken cancellationToken)
    {
        return await db.FinalExercises
            .AsNoTracking()
            .Where(x => x.ModuleId == moduleId)
            .OrderBy(x => x.Order)
            .Select(x => new FinalExerciseStudentResponse(x.Id, x.ModuleId, x.Prompt, x.Hint, x.Order))
            .ToListAsync(cancellationToken);
    }
}
