namespace TeacherApp.Contracts.FinalExercises;

public sealed record SubmitFinalExerciseResponse(bool IsCorrect, string? Hint, string? Explanation);
