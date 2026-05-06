using Microsoft.EntityFrameworkCore;
using TeacherApp.Api.Common;
using TeacherApp.Api.Data;
using TeacherApp.Contracts.Admin;

namespace TeacherApp.Api.Application.Admin.Dashboard;

public sealed class DashboardAdminService(AppDbContext db) : IDashboardAdminService
{
    public async Task<DashboardAdminStatsResponse> GetStatsAsync(CancellationToken cancellationToken)
    {
        var totalStudents = await db.Users
            .CountAsync(u => u.Role == Roles.Student && u.IsActive, cancellationToken);
        var totalModules = await db.Modules.CountAsync(cancellationToken);
        var totalLessons = await db.Lessons.CountAsync(cancellationToken);
        return new DashboardAdminStatsResponse(totalStudents, totalModules, totalLessons);
    }

    public async Task<IReadOnlyList<AdminStudentResponse>> ListStudentsAsync(CancellationToken cancellationToken)
    {
        return await db.Users.AsNoTracking()
            .Where(u => u.Role == Roles.Student)
            .OrderBy(u => u.Email)
            .Select(u => new AdminStudentResponse(u.Id, u.Email, u.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CompletedLessonAdminRowResponse>> ListCompletedLessonsAsync(
        Guid? studentId,
        int take,
        CancellationToken cancellationToken)
    {
        take = Math.Clamp(take, 1, 200);

        var lessons = await (
            from l in db.Lessons.AsNoTracking()
            join m in db.Modules.AsNoTracking() on l.ModuleId equals m.Id
            select new { l.Id, l.Title, ModuleId = m.Id, ModuleTitle = m.Title }
        ).ToListAsync(cancellationToken);

        var exerciseRows = await db.Exercises.AsNoTracking()
            .Select(e => new { e.Id, e.LessonId })
            .ToListAsync(cancellationToken);

        var exercisesByLesson = exerciseRows
            .GroupBy(e => e.LessonId)
            .ToDictionary(g => g.Key, g => g.Select(e => e.Id).ToList());

        var attempts = await db.ExerciseAttempts.AsNoTracking()
            .Where(a => a.ExerciseId != null && a.IsCorrect)
            .Select(a => new { a.UserId, ExerciseId = a.ExerciseId!.Value, a.AttemptedAt })
            .ToListAsync(cancellationToken);

        var firstCorrectByUserExercise = attempts
            .GroupBy(a => (a.UserId, a.ExerciseId))
            .ToDictionary(g => g.Key, g => g.Min(a => a.AttemptedAt));

        var students = await db.Users.AsNoTracking()
            .Where(u => u.Role == Roles.Student)
            .Select(u => new { u.Id, u.Email })
            .ToListAsync(cancellationToken);

        var rows = new List<CompletedLessonAdminRowResponse>();

        foreach (var student in students)
        {
            if (studentId.HasValue && student.Id != studentId.Value)
                continue;

            foreach (var lesson in lessons)
            {
                if (!exercisesByLesson.TryGetValue(lesson.Id, out var exerciseIds) || exerciseIds.Count == 0)
                    continue;

                DateTimeOffset? completedAt = null;
                foreach (var exId in exerciseIds)
                {
                    if (!firstCorrectByUserExercise.TryGetValue((student.Id, exId), out var firstAt))
                    {
                        completedAt = null;
                        break;
                    }

                    completedAt = completedAt.HasValue
                        ? (firstAt > completedAt.Value ? firstAt : completedAt.Value)
                        : firstAt;
                }

                if (completedAt.HasValue)
                {
                    rows.Add(new CompletedLessonAdminRowResponse(
                        student.Id,
                        student.Email,
                        lesson.Id,
                        lesson.Title,
                        lesson.ModuleId,
                        lesson.ModuleTitle,
                        completedAt.Value));
                }
            }
        }

        return rows
            .OrderByDescending(r => r.CompletedAt)
            .Take(take)
            .ToList();
    }
}
