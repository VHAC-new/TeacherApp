using Microsoft.EntityFrameworkCore;
using TeacherApp.Api.Common;
using TeacherApp.Api.Data;
using TeacherApp.Api.Domain;
using TeacherApp.Contracts.Exercises;
using TeacherApp.Contracts.FinalExercises;

namespace TeacherApp.Api.Application.Exercises;

public sealed class ExerciseSubmitService(AppDbContext db) : IExerciseSubmitService
{
    public async Task<SubmitExerciseResponse?> SubmitAsync(
        Guid exerciseId, Guid userId, SubmitExerciseRequest request, CancellationToken cancellationToken)
    {
        var exercise = await db.Exercises
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == exerciseId, cancellationToken);

        if (exercise is null)
            return null;

        var normalizedExpected = AnswerNormalizer.Normalize(exercise.ExpectedAnswer, exercise.IgnoreCase, exercise.IgnoreWhitespace);
        var normalizedSubmitted = AnswerNormalizer.Normalize(request.Answer, exercise.IgnoreCase, exercise.IgnoreWhitespace);
        var isCorrect = normalizedExpected == normalizedSubmitted;

        db.ExerciseAttempts.Add(new ExerciseAttempt
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ExerciseId = exerciseId,
            SubmittedAnswer = request.Answer.Trim(),
            IsCorrect = isCorrect,
            AttemptedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync(cancellationToken);

        return new SubmitExerciseResponse(
            isCorrect,
            isCorrect ? null : exercise.Hint,
            isCorrect ? null : exercise.Explanation);
    }

    public async Task<SubmitFinalExerciseResponse?> SubmitFinalAsync(
        Guid finalExerciseId, Guid userId, SubmitFinalExerciseRequest request, CancellationToken cancellationToken)
    {
        var exercise = await db.FinalExercises
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == finalExerciseId, cancellationToken);

        if (exercise is null)
            return null;

        var normalizedExpected = AnswerNormalizer.Normalize(exercise.ExpectedAnswer, exercise.IgnoreCase, exercise.IgnoreWhitespace);
        var normalizedSubmitted = AnswerNormalizer.Normalize(request.Answer, exercise.IgnoreCase, exercise.IgnoreWhitespace);
        var isCorrect = normalizedExpected == normalizedSubmitted;

        db.ExerciseAttempts.Add(new ExerciseAttempt
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FinalExerciseId = finalExerciseId,
            SubmittedAnswer = request.Answer.Trim(),
            IsCorrect = isCorrect,
            AttemptedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync(cancellationToken);

        return new SubmitFinalExerciseResponse(
            isCorrect,
            isCorrect ? null : exercise.Hint,
            isCorrect ? null : exercise.Explanation);
    }
}
