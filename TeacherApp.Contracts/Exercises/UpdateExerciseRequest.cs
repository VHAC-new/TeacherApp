namespace TeacherApp.Contracts.Exercises;

public sealed record UpdateExerciseRequest(
    string Prompt,
    string ExpectedAnswer,
    string? Hint,
    string? Explanation,
    bool IgnoreCase,
    bool IgnoreWhitespace,
    int Order,
    Guid LessonId);
