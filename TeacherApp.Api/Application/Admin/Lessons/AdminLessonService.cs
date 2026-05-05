using Microsoft.EntityFrameworkCore;
using TeacherApp.Api.Data;
using TeacherApp.Api.Domain;
using TeacherApp.Contracts.Lessons;

namespace TeacherApp.Api.Application.Admin.Lessons;

public sealed class AdminLessonService(AppDbContext db) : IAdminLessonService
{
    public async Task<IReadOnlyList<LessonResponse>> ListAsync(Guid? moduleId, CancellationToken cancellationToken)
    {
        var query = db.Lessons.AsNoTracking();

        if (moduleId is not null)
        {
            query = query.Where(x => x.ModuleId == moduleId.Value);
        }

        return await query
            .OrderBy(x => x.ModuleId)
            .ThenBy(x => x.Order)
            .Select(x => new LessonResponse(x.Id, x.ModuleId, x.Title, x.Description, x.Order, x.AudioMediaId,
                x.AudioMediaId != null
                    ? db.MediaFiles.Where(m => m.Id == x.AudioMediaId).Select(m => m.FileName).FirstOrDefault()
                    : null))
            .ToListAsync(cancellationToken);
    }

    public async Task<LessonResponse?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        return await db.Lessons
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new LessonResponse(x.Id, x.ModuleId, x.Title, x.Description, x.Order, x.AudioMediaId,
                x.AudioMediaId != null
                    ? db.MediaFiles.Where(m => m.Id == x.AudioMediaId).Select(m => m.FileName).FirstOrDefault()
                    : null))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<LessonResponse> CreateAsync(CreateLessonRequest request, CancellationToken cancellationToken)
    {
        Validate(request.Title, request.Description, request.Order);
        await ValidateAudioMediaIdAsync(request.AudioMediaId, cancellationToken);

        var moduleExists = await db.Modules.AnyAsync(x => x.Id == request.ModuleId, cancellationToken);
        if (!moduleExists)
        {
            throw new ArgumentException("Module not found.");
        }

        var entity = new Lesson
        {
            Id = Guid.NewGuid(),
            ModuleId = request.ModuleId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Order = request.Order,
            CreatedAt = DateTimeOffset.UtcNow,
            AudioMediaId = request.AudioMediaId,
        };

        try
        {
            db.Lessons.Add(entity);
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Order already exists for this module.");
        }

        return new LessonResponse(entity.Id, entity.ModuleId, entity.Title, entity.Description, entity.Order, entity.AudioMediaId);
    }

    public async Task<LessonResponse?> UpdateAsync(Guid id, UpdateLessonRequest request, CancellationToken cancellationToken)
    {
        Validate(request.Title, request.Description, request.Order);
        await ValidateAudioMediaIdAsync(request.AudioMediaId, cancellationToken);

        var moduleExists = await db.Modules.AnyAsync(x => x.Id == request.ModuleId, cancellationToken);
        if (!moduleExists)
        {
            throw new ArgumentException("Module not found.");
        }

        var entity = await db.Lessons.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.ModuleId = request.ModuleId;
        entity.Title = request.Title.Trim();
        entity.Description = request.Description?.Trim();
        entity.Order = request.Order;
        entity.AudioMediaId = request.AudioMediaId;

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Order already exists for this module.");
        }

        return new LessonResponse(entity.Id, entity.ModuleId, entity.Title, entity.Description, entity.Order, entity.AudioMediaId);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await db.Lessons.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        db.Lessons.Remove(entity);
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

    private async Task ValidateAudioMediaIdAsync(Guid? audioMediaId, CancellationToken cancellationToken)
    {
        if (audioMediaId is null)
        {
            return;
        }

        var media = await db.MediaFiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == audioMediaId.Value, cancellationToken);

        if (media is null)
        {
            throw new ArgumentException("Audio media not found.");
        }

        if (!media.UploadCompleted)
        {
            throw new ArgumentException("Audio media upload is not complete.");
        }

        if (!media.ContentType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Lesson audio must use an audio content type (e.g. audio/mpeg).");
        }
    }
}
