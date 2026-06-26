using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TeacherApp.App.Features.Exercise.ViewModels;

[QueryProperty(nameof(ModuleId), "moduleId")]
[QueryProperty(nameof(LessonId), "lessonId")]
[QueryProperty(nameof(Correct), "correct")]
[QueryProperty(nameof(Total), "total")]
[QueryProperty(nameof(ModuleTitle), "moduleTitle")]
public partial class ResultsViewModel : ObservableObject
{
    [ObservableProperty]
    private string _moduleId = "";

    [ObservableProperty]
    private string _lessonId = "";

    [ObservableProperty]
    private string _moduleTitle = "";

    [ObservableProperty]
    private string _correct = "0";

    [ObservableProperty]
    private string _total = "0";

    [ObservableProperty]
    private int _correctCount;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private int _incorrectCount;

    [ObservableProperty]
    private int _scorePercent;

    [ObservableProperty]
    private string _headline = "";

    [ObservableProperty]
    private string _trophy = "\uD83C\uDFC6";

    [ObservableProperty]
    private Color _headlineColor = Color.FromArgb("#7C5DF7");

    partial void OnCorrectChanged(string value) => RecalculateScore();
    partial void OnTotalChanged(string value) => RecalculateScore();

    private void RecalculateScore()
    {
        _ = int.TryParse(Correct, out var c);
        _ = int.TryParse(Total, out var t);
        CorrectCount = c;
        TotalCount = t;
        IncorrectCount = t - c;
        ScorePercent = t > 0 ? (int)Math.Round(100.0 * c / t) : 0;

        if (ScorePercent >= 80)
        {
            Headline = "Excelente trabalho!";
            Trophy = "\uD83C\uDFC6";
            HeadlineColor = Color.FromArgb("#34D399");
        }
        else if (ScorePercent >= 50)
        {
            Headline = "Bom trabalho!";
            Trophy = "\u2B50";
            HeadlineColor = Color.FromArgb("#FBBF24");
        }
        else
        {
            Headline = "Continue praticando!";
            Trophy = "\uD83D\uDCAA";
            HeadlineColor = Color.FromArgb("#F87171");
        }
    }

    [RelayCommand]
    private async Task ContinueLearning()
    {
        // Volta para a jornada (Módulo › Trilhas › Aulas) na aba de aulas.
        await Shell.Current.GoToAsync("//lessons");
    }

    [RelayCommand]
    private async Task ReviewLesson()
    {
        await Shell.Current.GoToAsync("../..");
    }
}
