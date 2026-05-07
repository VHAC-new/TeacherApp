namespace TeacherApp.Contracts.FinalExercises;

public sealed record FinalExerciseStudentResponse(
    Guid Id,
    Guid ModuleId,
    string Prompt,
    string? Hint,
    int Order);
