using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TeacherApp.App.Core;
using TeacherApp.App.Core.Services;
using TeacherApp.App.Features.Exercise.Services;
using TeacherApp.Contracts.Exercises;

namespace TeacherApp.App.Features.Exercise.ViewModels;

[QueryProperty(nameof(ModuleId), "moduleId")]
[QueryProperty(nameof(LessonId), "lessonId")]
[QueryProperty(nameof(ModuleTitle), "moduleTitle")]
public partial class ExerciseViewModel(CatalogService catalog, ExerciseService exerciseService) : ObservableObject, ICleanup
{
    private CancellationTokenSource? _cts;
    private string? _loadedKey;

    [ObservableProperty]
    private string _moduleId = "";

    [ObservableProperty]
    private string _lessonId = "";

    [ObservableProperty]
    private string _moduleTitle = "";

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
    private int _correctCount;

    [ObservableProperty]
    private int _totalExercises;

    [ObservableProperty]
    private string _questionLabel = "";

    [ObservableProperty]
    private bool _isHintVisible;

    public string HintToggleLabel => IsHintVisible ? "Hide hint" : "Show hint";

    public string HintChevronGlyph => IsHintVisible ? "\ue5ce" : "\ue5cf";

    public ObservableCollection<ExerciseStudentResponse> Exercises { get; } = [];

    public ExerciseStudentResponse? CurrentExercise =>
        CurrentIndex >= 0 && CurrentIndex < Exercises.Count ? Exercises[CurrentIndex] : null;

    public ObservableCollection<SegmentState> Segments { get; } = [];

    public Func<Task>? BeforeNavigateToResults { get; set; }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (!Guid.TryParse(ModuleId, out var modId) || !Guid.TryParse(LessonId, out var lesId))
            return;

        var key = $"{ModuleId}:{LessonId}";
        if (Exercises.Count > 0 && _loadedKey == key)
            return;

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

        IsBusy = true;
        Error = null;

        try
        {
            var exercises = await catalog.GetExercisesAsync(modId, lesId, ct);
            if (ct.IsCancellationRequested) return;

            Exercises.Clear();
            foreach (var e in exercises.OrderBy(e => e.Order))
                Exercises.Add(e);

            TotalExercises = Exercises.Count;
            CurrentIndex = 0;
            CorrectCount = 0;
            Answer = "";
            LastResult = null;
            Explanation = null;
            IsHintVisible = false;
            _loadedKey = key;

            RebuildSegments();
            UpdateQuestionLabel();
            OnPropertyChanged(nameof(CurrentExercise));
        }
        catch (OperationCanceledException) { }
        catch (HttpRequestException)
        {
            Error = "Erro ao carregar exercícios.";
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
            var result = await exerciseService.SubmitAsync(CurrentExercise.Id, Answer);
            LastResult = result.IsCorrect;
            Explanation = result.Explanation;
            if (result.IsCorrect) CorrectCount++;

            UpdateSegment(CurrentIndex, result.IsCorrect);
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
    private void ToggleHint() => IsHintVisible = !IsHintVisible;

    partial void OnIsHintVisibleChanged(bool value)
    {
        OnPropertyChanged(nameof(HintToggleLabel));
        OnPropertyChanged(nameof(HintChevronGlyph));
    }

    [RelayCommand]
    private async Task NextAsync()
    {
        if (IsBusy)
            return;

        LastResult = null;
        Explanation = null;
        Answer = "";
        IsHintVisible = false;
        CurrentIndex++;

        if (CurrentIndex >= Exercises.Count)
        {
            IsBusy = true;
            try
            {
                if (BeforeNavigateToResults is not null)
                    await BeforeNavigateToResults();

                var moduleTitle = Uri.EscapeDataString(ModuleTitle);
                await Shell.Current.GoToAsync(
                    $"results?moduleId={ModuleId}&lessonId={LessonId}&correct={CorrectCount}&total={TotalExercises}&moduleTitle={moduleTitle}");
            }
            finally
            {
                IsBusy = false;
            }

            return;
        }

        UpdateSegment(CurrentIndex, null);
        UpdateQuestionLabel();
        OnPropertyChanged(nameof(CurrentExercise));
    }

    private void RebuildSegments()
    {
        Segments.Clear();
        for (int i = 0; i < TotalExercises; i++)
            Segments.Add(new SegmentState { Color = i == 0 ? Color.FromArgb("#4F7CFF") : Color.FromArgb("#E0E0E0") });
    }

    private void UpdateSegment(int index, bool? isCorrect)
    {
        if (index >= Segments.Count) return;

        if (isCorrect == true)
            Segments[index] = new SegmentState { Color = Color.FromArgb("#10B981") };
        else if (isCorrect == false)
            Segments[index] = new SegmentState { Color = Color.FromArgb("#F44336") };
        else
            Segments[index] = new SegmentState { Color = Color.FromArgb("#4F7CFF") };
    }

    private void UpdateQuestionLabel()
    {
        QuestionLabel = $"Question {CurrentIndex + 1} of {TotalExercises}";
    }

    public void Cleanup()
    {
        BeforeNavigateToResults = null;
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        IsBusy = false;
    }
}

public class SegmentState : ObservableObject
{
    private Color _color = Color.FromArgb("#E0E0E0");
    public Color Color
    {
        get => _color;
        set => SetProperty(ref _color, value);
    }
}
