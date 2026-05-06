using Microsoft.EntityFrameworkCore;
using TeacherApp.Api.Data;
using TeacherApp.Api.Domain;
using TeacherApp.Contracts.Exercises;

namespace TeacherApp.Api.Application.Admin.Exercises;

public sealed class AdminExerciseService(AppDbContext db) : IAdminExerciseService
{
    public async Task<IReadOnlyList<ExerciseResponse>> ListAsync(Guid? lessonId, CancellationToken cancellationToken)
    {
        var query = db.Exercises.AsNoTracking();

        if (lessonId is not null)
        {
            query = query.Where(x => x.LessonId == lessonId.Value);
        }

        return await query
            .OrderBy(x => x.LessonId)
            .ThenBy(x => x.Order)
            .Select(x => new ExerciseResponse(x.Id, x.LessonId, x.Prompt, x.ExpectedAnswer, x.Hint, x.Explanation, x.IgnoreCase, x.IgnoreWhitespace, x.Order))
            .ToListAsync(cancellationToken);
    }

    public async Task<ExerciseResponse?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        return await db.Exercises
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ExerciseResponse(x.Id, x.LessonId, x.Prompt, x.ExpectedAnswer, x.Hint, x.Explanation, x.IgnoreCase, x.IgnoreWhitespace, x.Order))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ExerciseResponse> CreateAsync(CreateExerciseRequest request, CancellationToken cancellationToken)
    {
        Validate(request.Prompt, request.ExpectedAnswer, request.Hint, request.Explanation, request.Order);

        var lessonExists = await db.Lessons.AnyAsync(x => x.Id == request.LessonId, cancellationToken);
        if (!lessonExists)
        {
            throw new ArgumentException("Lesson not found.");
        }

        var entity = new Exercise
        {
            Id = Guid.NewGuid(),
            LessonId = request.LessonId,
            Prompt = request.Prompt.Trim(),
            ExpectedAnswer = request.ExpectedAnswer.Trim(),
            Hint = request.Hint?.Trim(),
            Explanation = request.Explanation?.Trim(),
            IgnoreCase = request.IgnoreCase,
            IgnoreWhitespace = request.IgnoreWhitespace,
            Order = request.Order,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        try
        {
            db.Exercises.Add(entity);
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Order already exists for this lesson.");
        }

        return new ExerciseResponse(entity.Id, entity.LessonId, entity.Prompt, entity.ExpectedAnswer, entity.Hint, entity.Explanation, entity.IgnoreCase, entity.IgnoreWhitespace, entity.Order);
    }

    public async Task<ExerciseResponse?> UpdateAsync(Guid id, UpdateExerciseRequest request, CancellationToken cancellationToken)
    {
        Validate(request.Prompt, request.ExpectedAnswer, request.Hint, request.Explanation, request.Order);

        var lessonExists = await db.Lessons.AnyAsync(x => x.Id == request.LessonId, cancellationToken);
        if (!lessonExists)
        {
            throw new ArgumentException("Lesson not found.");
        }

        var entity = await db.Exercises.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.LessonId = request.LessonId;
        entity.Prompt = request.Prompt.Trim();
        entity.ExpectedAnswer = request.ExpectedAnswer.Trim();
        entity.Hint = request.Hint?.Trim();
        entity.Explanation = request.Explanation?.Trim();
        entity.IgnoreCase = request.IgnoreCase;
        entity.IgnoreWhitespace = request.IgnoreWhitespace;
        entity.Order = request.Order;

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Order already exists for this lesson.");
        }

        return new ExerciseResponse(entity.Id, entity.LessonId, entity.Prompt, entity.ExpectedAnswer, entity.Hint, entity.Explanation, entity.IgnoreCase, entity.IgnoreWhitespace, entity.Order);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await db.Exercises.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        db.Exercises.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static void Validate(string prompt, string expectedAnswer, string? hint, string? explanation, int order)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException("Prompt is required.");
        if (prompt.Length > 2000)
            throw new ArgumentException("Prompt is too long (max 2000).");
        if (string.IsNullOrWhiteSpace(expectedAnswer))
            throw new ArgumentException("ExpectedAnswer is required.");
        if (expectedAnswer.Length > 500)
            throw new ArgumentException("ExpectedAnswer is too long (max 500).");
        if (hint is { Length: > 1000 })
            throw new ArgumentException("Hint is too long (max 1000).");
        if (explanation is { Length: > 2000 })
            throw new ArgumentException("Explanation is too long (max 2000).");
        if (order <= 0)
            throw new ArgumentException("Order must be greater than 0.");
    }
}
