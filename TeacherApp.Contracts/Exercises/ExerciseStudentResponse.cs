namespace TeacherApp.Contracts.Exercises;

public sealed record ExerciseStudentResponse(
    Guid Id,
    Guid LessonId,
    string Prompt,
    string? Hint,
    int Order);
