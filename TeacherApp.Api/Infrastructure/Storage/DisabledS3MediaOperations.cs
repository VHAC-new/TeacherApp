namespace TeacherApp.Api.Infrastructure.Storage;

public sealed class DisabledS3MediaOperations : IS3MediaOperations
{
    public bool IsEnabled => false;

    public string BuildObjectKey(Guid mediaId, string fileExtension)
        => throw new InvalidOperationException("S3 is not configured (Media:S3:Bucket).");

    public string CreatePresignedPut(string objectKey, string contentType, DateTime expiresUtc)
        => throw new InvalidOperationException("S3 is not configured (Media:S3:Bucket).");

    public string CreatePresignedGet(string objectKey, DateTime expiresUtc)
        => throw new InvalidOperationException("S3 is not configured (Media:S3:Bucket).");

    public Task<S3ObjectHead?> HeadObjectAsync(string objectKey, CancellationToken cancellationToken)
        => throw new InvalidOperationException("S3 is not configured (Media:S3:Bucket).");

    public Task DeleteObjectAsync(string objectKey, CancellationToken cancellationToken)
        => Task.CompletedTask;
}
