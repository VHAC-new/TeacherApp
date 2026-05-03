namespace TeacherApp.Api.Infrastructure.Storage;

public sealed class MediaS3Options
{
    public const string SectionName = "Media:S3";

    public string? Bucket { get; set; }
    public string? Region { get; set; }
    public string? KeyPrefix { get; set; }
    public int UploadUrlExpirationMinutes { get; set; } = 15;
    public int GetUrlExpirationMinutes { get; set; } = 60;
}
