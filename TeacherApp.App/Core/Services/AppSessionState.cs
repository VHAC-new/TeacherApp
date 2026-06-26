namespace TeacherApp.App.Core.Services;

/// <summary>
/// Estado de sessão do app (singleton). Sinaliza eventos como "app retomado do background"
/// para que telas revalidem dados uma única vez (one-shot), sem rebater na API a cada navegação.
/// </summary>
public sealed class AppSessionState
{
    private bool _resyncRequested;

    /// <summary>Solicita uma revalidação na próxima oportunidade (ex.: chamado no resume do app).</summary>
    public void RequestResync() => _resyncRequested = true;

    /// <summary>Consome (one-shot) a solicitação de revalidação; retorna true se havia uma pendente.</summary>
    public bool ConsumeResyncRequest()
    {
        if (!_resyncRequested)
            return false;
        _resyncRequested = false;
        return true;
    }
}
