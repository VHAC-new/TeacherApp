namespace TeacherApp.Api.Domain;

public sealed class ExerciseAttempt
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public Guid? ExerciseId { get; set; }
    public Exercise? Exercise { get; set; }
    public Guid? FinalExerciseId { get; set; }
    public FinalExercise? FinalExercise { get; set; }
    public string SubmittedAnswer { get; set; } = "";
    public bool IsCorrect { get; set; }
    public DateTimeOffset AttemptedAt { get; set; }
}
