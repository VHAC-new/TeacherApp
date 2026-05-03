namespace TeacherApp.Api.Domain;

public sealed class Lesson
{
    public Guid Id { get; set; }
    public Guid ModuleId { get; set; }
    public Module? Module { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public int Order { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Optional lesson audio (uploaded media id). Presigned playback uses <c>GET /api/v1/media/{id}/playback-url</c>.</summary>
    public Guid? AudioMediaId { get; set; }
}
