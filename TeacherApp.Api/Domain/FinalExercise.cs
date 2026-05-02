namespace TeacherApp.Api.Domain;

public sealed class FinalExercise
{
    public Guid Id { get; set; }
    public Guid ModuleId { get; set; }
    public Module? Module { get; set; }
    public string Prompt { get; set; } = "";
    public string ExpectedAnswer { get; set; } = "";
    public string? Hint { get; set; }
    public string? Explanation { get; set; }
    public bool IgnoreCase { get; set; } = true;
    public bool IgnoreWhitespace { get; set; } = true;
    public int Order { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
