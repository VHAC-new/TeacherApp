namespace TeacherApp.Contracts.FinalExercises;

public sealed record UpdateFinalExerciseRequest(
    string Prompt,
    string ExpectedAnswer,
    string? Hint,
    string? Explanation,
    bool IgnoreCase,
    bool IgnoreWhitespace,
    int Order,
    Guid ModuleId);
