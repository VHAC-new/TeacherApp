namespace TeacherApp.Api.Domain;

public sealed class MediaFile
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = "";
    public string ContentType { get; set; } = "";
    public long SizeBytes { get; set; }
    public string StoragePath { get; set; } = "";
    public DateTimeOffset UploadedAt { get; set; }
    public bool UploadCompleted { get; set; } = true;
}
