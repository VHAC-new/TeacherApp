namespace TeacherApp.Contracts.Exercises;

public sealed record CreateExerciseRequest(
    Guid LessonId,
    string Prompt,
    string ExpectedAnswer,
    string? Hint,
    string? Explanation,
    bool IgnoreCase,
    bool IgnoreWhitespace,
    int Order);
