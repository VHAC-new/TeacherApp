using TeacherApp.Contracts.Lessons;
using TeacherApp.Contracts.Progress;

namespace TeacherApp.App.Features.Module.Models;

public sealed class LessonWithProgress
{
    public Guid Id { get; }
    public Guid TrailId { get; }
    public string Title { get; }
    public string? Description { get; }
    public int Order { get; }
    public Guid? AudioMediaId { get; }
    public bool IsCompleted { get; }
    public bool IsLocked { get; }
    public int TotalExercises { get; }
    public int CompletedExercises { get; }
    public string StatusText { get; }
    public string StatusIcon { get; }
    public Color StatusColor { get; }

    public LessonWithProgress(LessonResponse lesson, LessonProgressResponse? progress, bool isLocked)
    {
        Id = lesson.Id;
        TrailId = lesson.TrailId;
        Title = lesson.Title;
        Description = lesson.Description;
        Order = lesson.Order;
        AudioMediaId = lesson.AudioMediaId;
        TotalExercises = progress?.TotalExercises ?? 0;
        CompletedExercises = progress?.CompletedExercises ?? 0;
        IsCompleted = progress?.IsCompleted ?? false;
        IsLocked = isLocked;

        if (IsCompleted)
        {
            StatusText = "Completed";
            StatusIcon = "\u2714";
            StatusColor = Color.FromArgb("#10B981");
        }
        else if (isLocked)
        {
            StatusText = "Locked";
            StatusIcon = "\uD83D\uDD12";
            StatusColor = Color.FromArgb("#9E9E9E");
        }
        else
        {
            StatusText = "Ready to start";
            StatusIcon = "\u25B6";
            StatusColor = Color.FromArgb("#4F7CFF");
        }
    }
}
