namespace TeacherApp.Contracts.FinalExercises;

public sealed record FinalExerciseResponse(
    Guid Id,
    Guid ModuleId,
    string Prompt,
    string ExpectedAnswer,
    string? Hint,
    string? Explanation,
    bool IgnoreCase,
    bool IgnoreWhitespace,
    int Order);
