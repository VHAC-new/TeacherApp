namespace TeacherApp.Api.Domain;

public sealed class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
}

