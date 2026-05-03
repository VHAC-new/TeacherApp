namespace TeacherApp.Admin.Services;

public sealed class TokenStorageService
{
    public string? AccessToken { get; private set; }
    public string? Email { get; private set; }
    public IReadOnlyList<string> Roles { get; private set; } = [];
    public Guid? UserId { get; private set; }

    public void Store(string accessToken, string email, IReadOnlyList<string> roles, Guid userId)
    {
        AccessToken = accessToken;
        Email = email;
        Roles = roles;
        UserId = userId;
    }

    public void Clear()
    {
        AccessToken = null;
        Email = null;
        Roles = [];
        UserId = null;
    }

    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(AccessToken);
}
