namespace TeacherApp.Contracts.Media;

public sealed record InitMediaUploadResponse(
    Guid MediaId,
    string UploadUrl,
    string ObjectKey,
    DateTimeOffset ExpiresAtUtc);
