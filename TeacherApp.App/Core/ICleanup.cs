namespace TeacherApp.App.Core;

/// <summary>
/// Cancela trabalho assíncrono em andamento ao sair da tela.
/// Não deve limpar coleções ou estado de UI — a página pode permanecer na pilha ao navegar adiante.
/// </summary>
public interface ICleanup
{
    void Cleanup();
}
