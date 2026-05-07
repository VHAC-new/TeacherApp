namespace TeacherApp.Contracts.Media;

public sealed record MediaPlaybackUrlResponse(string Url, DateTimeOffset ExpiresAtUtc);
