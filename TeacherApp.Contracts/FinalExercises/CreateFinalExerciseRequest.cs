namespace TeacherApp.Contracts.FinalExercises;

public sealed record CreateFinalExerciseRequest(
    Guid ModuleId,
    string Prompt,
    string ExpectedAnswer,
    string? Hint,
    string? Explanation,
    bool IgnoreCase,
    bool IgnoreWhitespace,
    int Order);
