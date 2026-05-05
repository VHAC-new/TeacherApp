using Microsoft.EntityFrameworkCore;
using TeacherApp.Api.Data;
using TeacherApp.Api.Domain;
using TeacherApp.Contracts.Modules;

namespace TeacherApp.Api.Application.Admin.Modules;

public sealed class AdminModuleService(AppDbContext db) : IAdminModuleService
{
    public async Task<IReadOnlyList<ModuleResponse>> ListAsync(CancellationToken cancellationToken)
    {
        return await db.Modules
            .AsNoTracking()
            .OrderBy(x => x.Order)
            .Select(x => new ModuleResponse(x.Id, x.Title, x.Description, x.Order,
                db.Lessons.Count(l => l.ModuleId == x.Id)))
            .ToListAsync(cancellationToken);
    }

    public async Task<ModuleResponse?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        return await db.Modules
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ModuleResponse(x.Id, x.Title, x.Description, x.Order,
                db.Lessons.Count(l => l.ModuleId == x.Id)))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ModuleResponse> CreateAsync(CreateModuleRequest request, CancellationToken cancellationToken)
    {
        Validate(request.Title, request.Description, request.Order);

        var entity = new Module
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Order = request.Order,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        db.Modules.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return new ModuleResponse(entity.Id, entity.Title, entity.Description, entity.Order);
    }

    public async Task<ModuleResponse?> UpdateAsync(Guid id, UpdateModuleRequest request, CancellationToken cancellationToken)
    {
        Validate(request.Title, request.Description, request.Order);

        var entity = await db.Modules.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.Title = request.Title.Trim();
        entity.Description = request.Description?.Trim();
        entity.Order = request.Order;

        await db.SaveChangesAsync(cancellationToken);

        return new ModuleResponse(entity.Id, entity.Title, entity.Description, entity.Order);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await db.Modules.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        db.Modules.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static void Validate(string title, string? description, int order)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required.");
        }

        if (title.Length > 200)
        {
            throw new ArgumentException("Title is too long (max 200).");
        }

        if (description is { Length: > 2000 })
        {
            throw new ArgumentException("Description is too long (max 2000).");
        }

        if (order <= 0)
        {
            throw new ArgumentException("Order must be greater than 0.");
        }
    }
}

