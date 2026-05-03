using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

namespace TeacherApp.Api.Infrastructure.Storage;

public sealed class S3MediaOperations(IAmazonS3 s3, IOptions<MediaS3Options> options) : IS3MediaOperations
{
    private readonly MediaS3Options _opts = options.Value;

    public bool IsEnabled => !string.IsNullOrWhiteSpace(_opts.Bucket);

    public string BuildObjectKey(Guid mediaId, string fileExtension)
    {
        var ext = string.IsNullOrEmpty(fileExtension) ? "" : fileExtension.StartsWith('.') ? fileExtension : "." + fileExtension;
        var prefix = (_opts.KeyPrefix ?? "").Trim().TrimEnd('/');
        return string.IsNullOrEmpty(prefix)
            ? $"{mediaId:N}{ext}"
            : $"{prefix}/{mediaId:N}{ext}";
    }

    public string CreatePresignedPut(string objectKey, string contentType, DateTime expiresUtc)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _opts.Bucket,
            Key = objectKey,
            Verb = HttpVerb.PUT,
            Expires = expiresUtc,
            ContentType = contentType,
        };
        return s3.GetPreSignedURL(request);
    }

    public string CreatePresignedGet(string objectKey, DateTime expiresUtc)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _opts.Bucket,
            Key = objectKey,
            Verb = HttpVerb.GET,
            Expires = expiresUtc,
        };
        return s3.GetPreSignedURL(request);
    }

    public async Task<S3ObjectHead?> HeadObjectAsync(string objectKey, CancellationToken cancellationToken)
    {
        try
        {
            var response = await s3.GetObjectMetadataAsync(new GetObjectMetadataRequest
            {
                BucketName = _opts.Bucket,
                Key = objectKey,
            }, cancellationToken);

            return new S3ObjectHead(response.ContentLength, response.Headers.ContentType);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public Task DeleteObjectAsync(string objectKey, CancellationToken cancellationToken)
    {
        return s3.DeleteObjectAsync(new DeleteObjectRequest
        {
            BucketName = _opts.Bucket,
            Key = objectKey,
        }, cancellationToken);
    }
}
