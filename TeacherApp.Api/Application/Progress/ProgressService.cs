using Microsoft.EntityFrameworkCore;
using TeacherApp.Api.Data;
using TeacherApp.Contracts.Progress;

namespace TeacherApp.Api.Application.Progress;

public sealed class ProgressService(AppDbContext db) : IProgressService
{
    public async Task<OverallProgressResponse> GetOverallAsync(Guid userId, CancellationToken cancellationToken)
    {
        var modules = await db.Modules
            .AsNoTracking()
            .OrderBy(m => m.Order)
            .ToListAsync(cancellationToken);

        var results = new List<ModuleProgressResponse>();
        foreach (var mod in modules)
        {
            results.Add(await BuildModuleProgress(userId, mod.Id, mod.Title, cancellationToken));
        }

        return new OverallProgressResponse(results);
    }

    public async Task<ModuleProgressResponse?> GetModuleAsync(Guid userId, Guid moduleId, CancellationToken cancellationToken)
    {
        var mod = await db.Modules
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == moduleId, cancellationToken);

        if (mod is null)
            return null;

        return await BuildModuleProgress(userId, mod.Id, mod.Title, cancellationToken);
    }

    private async Task<ModuleProgressResponse> BuildModuleProgress(
        Guid userId, Guid moduleId, string moduleTitle, CancellationToken cancellationToken)
    {
        var lessonIds = await db.Lessons
            .AsNoTracking()
            .Where(l => l.ModuleId == moduleId)
            .Select(l => l.Id)
            .ToListAsync(cancellationToken);

        var totalLessons = lessonIds.Count;

        var exerciseIds = await db.Exercises
            .AsNoTracking()
            .Where(e => lessonIds.Contains(e.LessonId))
            .Select(e => e.Id)
            .ToListAsync(cancellationToken);

        var totalExercises = exerciseIds.Count;

        var completedExerciseIds = await db.ExerciseAttempts
            .AsNoTracking()
            .Where(a => a.UserId == userId && a.ExerciseId != null && a.IsCorrect)
            .Select(a => a.ExerciseId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        var completedExercises = exerciseIds.Count(id => completedExerciseIds.Contains(id));

        // A lesson is "completed" when all its exercises have at least one correct attempt
        var completedLessons = 0;
        foreach (var lessonId in lessonIds)
        {
            var lessonExerciseIds = await db.Exercises
                .AsNoTracking()
                .Where(e => e.LessonId == lessonId)
                .Select(e => e.Id)
                .ToListAsync(cancellationToken);

            if (lessonExerciseIds.Count > 0 && lessonExerciseIds.All(id => completedExerciseIds.Contains(id)))
                completedLessons++;
        }

        var finalExerciseIds = await db.FinalExercises
            .AsNoTracking()
            .Where(f => f.ModuleId == moduleId)
            .Select(f => f.Id)
            .ToListAsync(cancellationToken);

        var totalFinalExercises = finalExerciseIds.Count;

        var completedFinalExerciseIds = await db.ExerciseAttempts
            .AsNoTracking()
            .Where(a => a.UserId == userId && a.FinalExerciseId != null && a.IsCorrect)
            .Select(a => a.FinalExerciseId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        var completedFinalExercises = finalExerciseIds.Count(id => completedFinalExerciseIds.Contains(id));

        return new ModuleProgressResponse(
            moduleId,
            moduleTitle,
            totalLessons,
            completedLessons,
            totalExercises,
            completedExercises,
            totalFinalExercises,
            completedFinalExercises);
    }
}
