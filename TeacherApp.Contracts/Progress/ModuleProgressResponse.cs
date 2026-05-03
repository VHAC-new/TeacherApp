namespace TeacherApp.Contracts.Progress;

public sealed record ModuleProgressResponse(
    Guid ModuleId,
    string ModuleTitle,
    int TotalLessons,
    int CompletedLessons,
    int TotalExercises,
    int CompletedExercises,
    int TotalFinalExercises,
    int CompletedFinalExercises);
