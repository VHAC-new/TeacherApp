namespace TeacherApp.Api.Domain;

public sealed class Module
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public int Order { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

