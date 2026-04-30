namespace TeacherApp.Contracts.Auth;

public sealed record LoginResponse(
    string AccessToken,
    DateTimeOffset ExpiresAtUtc,
    Guid UserId,
    string Email,
    IReadOnlyList<string> Roles
);

