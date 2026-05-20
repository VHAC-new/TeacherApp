using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TeacherApp.App.Core.Services;
using TeacherApp.App.Features.Exercise.Services;
using TeacherApp.Contracts.FinalExercises;

namespace TeacherApp.App.Features.Exercise.ViewModels;

[QueryProperty(nameof(ModuleId), "moduleId")]
[QueryProperty(nameof(Title), "title")]
public partial class FinalExercisesViewModel(CatalogService catalog, ExerciseService exerciseService) : ObservableObject
{
    [ObservableProperty]
    private string _moduleId = "";

    [ObservableProperty]
    private string _title = "";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _error;

    [ObservableProperty]
    private int _currentIndex;

    [ObservableProperty]
    private string _answer = "";

    [ObservableProperty]
    private bool? _lastResult;

    [ObservableProperty]
    private string? _explanation;

    [ObservableProperty]
    private bool _finished;

    [ObservableProperty]
    private int _correctCount;

    public ObservableCollection<FinalExerciseStudentResponse> Exercises { get; } = [];

    public FinalExerciseStudentResponse? CurrentExercise =>
        CurrentIndex >= 0 && CurrentIndex < Exercises.Count ? Exercises[CurrentIndex] : null;

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (!Guid.TryParse(ModuleId, out var id)) return;

        IsBusy = true;
        Error = null;

        try
        {
            var exercises = await catalog.GetFinalExercisesAsync(id);
            Exercises.Clear();
            foreach (var e in exercises) Exercises.Add(e);
            CurrentIndex = 0;
            CorrectCount = 0;
            Finished = false;
            OnPropertyChanged(nameof(CurrentExercise));
        }
        catch (HttpRequestException)
        {
            Error = "Erro ao carregar exercícios finais.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SubmitAsync()
    {
        if (string.IsNullOrWhiteSpace(Answer) || CurrentExercise is null)
        {
            Error = "Digite uma resposta.";
            return;
        }

        IsBusy = true;
        Error = null;

        try
        {
            var result = await exerciseService.SubmitFinalAsync(CurrentExercise.Id, Answer);
            LastResult = result.IsCorrect;
            Explanation = result.Explanation;
            if (result.IsCorrect) CorrectCount++;
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
    private void Next()
    {
        LastResult = null;
        Explanation = null;
        Answer = "";
        CurrentIndex++;

        if (CurrentIndex >= Exercises.Count)
        {
            Finished = true;
        }

        OnPropertyChanged(nameof(CurrentExercise));
    }
}
