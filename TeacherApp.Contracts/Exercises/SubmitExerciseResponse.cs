namespace TeacherApp.Contracts.Exercises;

public sealed record SubmitExerciseResponse(bool IsCorrect, string? Hint, string? Explanation);
