namespace TeacherApp.App.Core.Services;

/// <summary>
/// Estado leve compartilhado do ciclo de vida do app. Hoje sinaliza, de forma "one-shot",
/// que algo (ex.: o app voltar do background) pede uma revalidação dos dados na próxima
/// vez que uma tela aparecer — independentemente da janela de TTL.
/// </summary>
public sealed class AppSessionState
{
    private int _resyncRequested;

    /// <summary>Pede uma revalidação. Idempotente até ser consumida.</summary>
    public void RequestResync() => Interlocked.Exchange(ref _resyncRequested, 1);

    /// <summary>Retorna <c>true</c> uma única vez se havia um pedido pendente, e o zera.</summary>
    public bool ConsumeResyncRequest() => Interlocked.Exchange(ref _resyncRequested, 0) == 1;
}
