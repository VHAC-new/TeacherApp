using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TeacherApp.App.Features.Exercise.Services;

namespace TeacherApp.App.Features.Exercise.ViewModels;

[QueryProperty(nameof(ExerciseId), "exerciseId")]
[QueryProperty(nameof(Prompt), "prompt")]
[QueryProperty(nameof(Hint), "hint")]
public partial class ExerciseViewModel(ExerciseService exerciseService) : ObservableObject
{
    [ObservableProperty]
    private string _exerciseId = "";

    [ObservableProperty]
    private string _prompt = "";

    [ObservableProperty]
    private string _hint = "";

    [ObservableProperty]
    private string _answer = "";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool? _isCorrect;

    [ObservableProperty]
    private string? _explanation;

    [ObservableProperty]
    private string? _error;

    [ObservableProperty]
    private bool _submitted;

    [RelayCommand]
    private async Task SubmitAsync()
    {
        if (string.IsNullOrWhiteSpace(Answer))
        {
            Error = "Digite uma resposta.";
            return;
        }

        if (!Guid.TryParse(ExerciseId, out var id)) return;

        IsBusy = true;
        Error = null;

        try
        {
            var result = await exerciseService.SubmitAsync(id, Answer);
            IsCorrect = result.IsCorrect;
            Explanation = result.Explanation;
            Submitted = true;

            if (!result.IsCorrect && !string.IsNullOrEmpty(result.Hint))
            {
                Hint = result.Hint;
            }
        }
        catch (HttpRequestException)
        {
            Error = "Erro ao enviar resposta.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
