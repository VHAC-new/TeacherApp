namespace TeacherApp.Contracts.Media;

public sealed record MediaResponse(
    Guid Id,
    string FileName,
    string ContentType,
    long SizeBytes,
    DateTimeOffset UploadedAt,
    bool UploadCompleted);
