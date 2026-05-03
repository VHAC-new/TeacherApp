namespace TeacherApp.Api.Infrastructure.Storage;

public interface IS3MediaOperations
{
    bool IsEnabled { get; }

    string BuildObjectKey(Guid mediaId, string fileExtension);

    string CreatePresignedPut(string objectKey, string contentType, DateTime expiresUtc);

    string CreatePresignedGet(string objectKey, DateTime expiresUtc);

    Task<S3ObjectHead?> HeadObjectAsync(string objectKey, CancellationToken cancellationToken);

    Task DeleteObjectAsync(string objectKey, CancellationToken cancellationToken);
}

public sealed record S3ObjectHead(long ContentLength, string? ContentType);
