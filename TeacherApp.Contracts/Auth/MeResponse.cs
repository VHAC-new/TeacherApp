namespace TeacherApp.Contracts.Auth;

public sealed record MeResponse(Guid UserId, string Email, IReadOnlyList<string> Roles);

