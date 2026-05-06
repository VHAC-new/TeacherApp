using Microsoft.EntityFrameworkCore;
using TeacherApp.Api.Data;
using TeacherApp.Api.Domain;
using TeacherApp.Contracts.FinalExercises;

namespace TeacherApp.Api.Application.Admin.FinalExercises;

public sealed class AdminFinalExerciseService(AppDbContext db) : IAdminFinalExerciseService
{
    public async Task<IReadOnlyList<FinalExerciseResponse>> ListAsync(Guid? moduleId, CancellationToken cancellationToken)
    {
        var query = db.FinalExercises.AsNoTracking();

        if (moduleId is not null)
        {
            query = query.Where(x => x.ModuleId == moduleId.Value);
        }

        return await query
            .OrderBy(x => x.ModuleId)
            .ThenBy(x => x.Order)
            .Select(x => new FinalExerciseResponse(x.Id, x.ModuleId, x.Prompt, x.ExpectedAnswer, x.Hint, x.Explanation, x.IgnoreCase, x.IgnoreWhitespace, x.Order))
            .ToListAsync(cancellationToken);
    }

    public async Task<FinalExerciseResponse?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        return await db.FinalExercises
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new FinalExerciseResponse(x.Id, x.ModuleId, x.Prompt, x.ExpectedAnswer, x.Hint, x.Explanation, x.IgnoreCase, x.IgnoreWhitespace, x.Order))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<FinalExerciseResponse> CreateAsync(CreateFinalExerciseRequest request, CancellationToken cancellationToken)
    {
        Validate(request.Prompt, request.ExpectedAnswer, request.Hint, request.Explanation, request.Order);

        var moduleExists = await db.Modules.AnyAsync(x => x.Id == request.ModuleId, cancellationToken);
        if (!moduleExists)
        {
            throw new ArgumentException("Module not found.");
        }

        var entity = new FinalExercise
        {
            Id = Guid.NewGuid(),
            ModuleId = request.ModuleId,
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
            db.FinalExercises.Add(entity);
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Order already exists for this module.");
        }

        return new FinalExerciseResponse(entity.Id, entity.ModuleId, entity.Prompt, entity.ExpectedAnswer, entity.Hint, entity.Explanation, entity.IgnoreCase, entity.IgnoreWhitespace, entity.Order);
    }

    public async Task<FinalExerciseResponse?> UpdateAsync(Guid id, UpdateFinalExerciseRequest request, CancellationToken cancellationToken)
    {
        Validate(request.Prompt, request.ExpectedAnswer, request.Hint, request.Explanation, request.Order);

        var moduleExists = await db.Modules.AnyAsync(x => x.Id == request.ModuleId, cancellationToken);
        if (!moduleExists)
        {
            throw new ArgumentException("Module not found.");
        }

        var entity = await db.FinalExercises.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.ModuleId = request.ModuleId;
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
            throw new InvalidOperationException("Order already exists for this module.");
        }

        return new FinalExerciseResponse(entity.Id, entity.ModuleId, entity.Prompt, entity.ExpectedAnswer, entity.Hint, entity.Explanation, entity.IgnoreCase, entity.IgnoreWhitespace, entity.Order);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await db.FinalExercises.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        db.FinalExercises.Remove(entity);
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
