using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeacherApp.Api.Data;
using TeacherApp.Api.Domain;
using TeacherApp.Api.Infrastructure.Storage;
using TeacherApp.Contracts.Media;

namespace TeacherApp.Api.Application.Admin.Media;

public sealed class AdminMediaService(
    AppDbContext db,
    IConfiguration configuration,
    IS3MediaOperations s3,
    IOptions<MediaS3Options> s3Options) : IAdminMediaService
{
    private static readonly HashSet<string> AllowedContentTypes =
    [
        "audio/mpeg", "audio/wav", "audio/ogg", "audio/mp4",
        "image/jpeg", "image/png", "image/gif", "image/webp",
    ];

    public async Task<IReadOnlyList<MediaResponse>> ListAsync(CancellationToken cancellationToken)
    {
        return await db.MediaFiles
            .AsNoTracking()
            .OrderByDescending(x => x.UploadedAt)
            .Select(x => new MediaResponse(x.Id, x.FileName, x.ContentType, x.SizeBytes, x.UploadedAt, x.UploadCompleted))
            .ToListAsync(cancellationToken);
    }

    public async Task<MediaResponse> UploadAsync(
        string fileName, string contentType, Stream content, CancellationToken cancellationToken)
    {
        if (s3.IsEnabled)
        {
            throw new InvalidOperationException("S3 is enabled: use presigned upload (POST .../media/upload-url) instead of multipart POST.");
        }

        ValidateUpload(fileName, contentType);

        var mediaRoot = GetMediaRoot();
        Directory.CreateDirectory(mediaRoot);

        var id = Guid.NewGuid();
        var ext = Path.GetExtension(fileName);
        var storedName = $"{id}{ext}";
        var fullPath = Path.Combine(mediaRoot, storedName);

        await using (var fs = File.Create(fullPath))
        {
            await content.CopyToAsync(fs, cancellationToken);
        }

        var fileInfo = new FileInfo(fullPath);
        var entity = new MediaFile
        {
            Id = id,
            FileName = fileName.Trim(),
            ContentType = contentType,
            SizeBytes = fileInfo.Length,
            StoragePath = storedName,
            UploadedAt = DateTimeOffset.UtcNow,
            UploadCompleted = true,
        };

        db.MediaFiles.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return ToResponse(entity);
    }

    public async Task<InitMediaUploadResponse> InitPresignedUploadAsync(
        InitMediaUploadRequest request, CancellationToken cancellationToken)
    {
        if (!s3.IsEnabled)
        {
            throw new InvalidOperationException("S3 is not configured. Set Media:S3:Bucket and Media:S3:Region, or use local multipart upload.");
        }

        ValidateUpload(request.FileName, request.ContentType);

        var id = Guid.NewGuid();
        var ext = Path.GetExtension(request.FileName);
        var key = s3.BuildObjectKey(id, ext);

        var entity = new MediaFile
        {
            Id = id,
            FileName = request.FileName.Trim(),
            ContentType = request.ContentType,
            SizeBytes = 0,
            StoragePath = key,
            UploadedAt = DateTimeOffset.UtcNow,
            UploadCompleted = false,
        };

        db.MediaFiles.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        var minutes = s3Options.Value.UploadUrlExpirationMinutes > 0 ? s3Options.Value.UploadUrlExpirationMinutes : 15;
        var expiresUtc = DateTime.UtcNow.AddMinutes(minutes);
        var uploadUrl = s3.CreatePresignedPut(key, request.ContentType, expiresUtc);

        return new InitMediaUploadResponse(id, uploadUrl, key, new DateTimeOffset(expiresUtc, TimeSpan.Zero));
    }

    public async Task<MediaResponse?> CompletePresignedUploadAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!s3.IsEnabled)
        {
            throw new InvalidOperationException("S3 is not configured.");
        }

        var entity = await db.MediaFiles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        if (entity.UploadCompleted)
        {
            return ToResponse(entity);
        }

        S3ObjectHead? head;
        try
        {
            head = await s3.HeadObjectAsync(entity.StoragePath, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to verify S3 object '{entity.StoragePath}': {ex.Message}", ex);
        }

        if (head is null)
        {
            return null;
        }

        entity.SizeBytes = head.ContentLength;
        if (!string.IsNullOrWhiteSpace(head.ContentType))
        {
            entity.ContentType = head.ContentType!;
        }

        entity.UploadCompleted = true;
        await db.SaveChangesAsync(cancellationToken);

        return ToResponse(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await db.MediaFiles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        if (s3.IsEnabled)
        {
            try
            {
                await s3.DeleteObjectAsync(entity.StoragePath, cancellationToken);
            }
            catch
            {
                // Still remove DB row if object already gone
            }
        }
        else
        {
            var mediaRoot = GetMediaRoot();
            var fullPath = Path.Combine(mediaRoot, entity.StoragePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        db.MediaFiles.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> CleanupIncompleteAsync(TimeSpan olderThan, CancellationToken cancellationToken)
    {
        var cutoff = DateTimeOffset.UtcNow - olderThan;
        var stale = await db.MediaFiles
            .Where(x => !x.UploadCompleted && x.UploadedAt < cutoff)
            .ToListAsync(cancellationToken);

        foreach (var entity in stale)
        {
            if (s3.IsEnabled)
            {
                try { await s3.DeleteObjectAsync(entity.StoragePath, cancellationToken); }
                catch { /* Object may already be gone */ }
            }
        }

        db.MediaFiles.RemoveRange(stale);
        await db.SaveChangesAsync(cancellationToken);
        return stale.Count;
    }

    private static void ValidateUpload(string fileName, string contentType)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("FileName is required.");
        }

        if (string.IsNullOrWhiteSpace(contentType) || !AllowedContentTypes.Contains(contentType))
        {
            throw new ArgumentException($"Content type '{contentType}' is not allowed.");
        }
    }

    private static MediaResponse ToResponse(MediaFile entity)
        => new(entity.Id, entity.FileName, entity.ContentType, entity.SizeBytes, entity.UploadedAt, entity.UploadCompleted);

    private string GetMediaRoot()
    {
        return configuration["Media:StoragePath"]
            ?? Path.Combine(AppContext.BaseDirectory, "media-files");
    }
}
