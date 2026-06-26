namespace TeacherApp.App.Core.Messages;

/// <summary>
/// Publicada quando uma aula é concluída (todos os exercícios respondidos), para que a trilha
/// faça um update otimista + sincronização imediata do módulo afetado.
/// <paramref name="LessonAllCorrect"/> indica se a sessão acertou todos os exercícios — regra de
/// conclusão do backend (aula concluída quando todos os exercícios têm ≥1 acerto).
/// </summary>
public sealed record ProgressChangedMessage(Guid ModuleId, Guid TrailId, Guid LessonId, bool LessonAllCorrect);
