namespace TeacherApp.Api.Domain;

public sealed class Exercise
{
    public Guid Id { get; set; }
    public Guid LessonId { get; set; }
    public Lesson? Lesson { get; set; }
    public string Prompt { get; set; } = "";
    public string ExpectedAnswer { get; set; } = "";
    public string? Hint { get; set; }
    public string? Explanation { get; set; }
    public bool IgnoreCase { get; set; } = true;
    public bool IgnoreWhitespace { get; set; } = true;
    public int Order { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
