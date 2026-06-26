using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TeacherApp.App.Core;
using TeacherApp.App.Core.Services;
using TeacherApp.App.Features.Exercise.Services;
using TeacherApp.Contracts.FinalExercises;

namespace TeacherApp.App.Features.Exercise.ViewModels;

[QueryProperty(nameof(TrailId), "trailId")]
[QueryProperty(nameof(Title), "title")]
public partial class FinalExercisesViewModel(CatalogService catalog, ExerciseService exerciseService) : ObservableObject, ICleanup
{
    private CancellationTokenSource? _cts;
    private string? _loadedTrailId;

    [ObservableProperty]
    private string _trailId = "";

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
        if (!Guid.TryParse(TrailId, out var id))
            return;

        if (Exercises.Count > 0 && _loadedTrailId == TrailId)
            return;

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

        IsBusy = true;
        Error = null;

        try
        {
            var exercises = await catalog.GetFinalExercisesAsync(id, ct);
            if (ct.IsCancellationRequested)
                return;

            Exercises.Clear();
            foreach (var e in exercises)
                Exercises.Add(e);
            CurrentIndex = 0;
            CorrectCount = 0;
            Finished = false;
            _loadedTrailId = TrailId;
            OnPropertyChanged(nameof(CurrentExercise));
        }
        catch (OperationCanceledException)
        {
            // Tela saiu ou carregamento substituído.
        }
        catch (HttpRequestException)
        {
            Error = "Erro ao carregar exercícios finais.";
        }
        finally
        {
            if (!ct.IsCancellationRequested)
                IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SubmitAsync()
    {
        if (IsBusy || LastResult is not null)
            return;

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
    private async Task GoBackAsync() => await Shell.Current.GoToAsync("..");

    [RelayCommand]
    private void Next()
    {
        LastResult = null;
        Explanation = null;
        Answer = "";
        CurrentIndex++;

        if (CurrentIndex >= Exercises.Count)
            Finished = true;

        OnPropertyChanged(nameof(CurrentExercise));
    }

    public void Cleanup()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        IsBusy = false;
    }
}
