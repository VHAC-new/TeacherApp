using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TeacherApp.App.Features.Exercise.ViewModels;

[QueryProperty(nameof(ModuleId), "moduleId")]
[QueryProperty(nameof(LessonId), "lessonId")]
[QueryProperty(nameof(Correct), "correct")]
[QueryProperty(nameof(Total), "total")]
public partial class ResultsViewModel : ObservableObject
{
    [ObservableProperty]
    private string _moduleId = "";

    [ObservableProperty]
    private string _lessonId = "";

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
    private Color _headlineColor = Color.FromArgb("#512BD4");

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
            Headline = "Excellent Work!";
            Trophy = "\uD83C\uDFC6";
            HeadlineColor = Color.FromArgb("#4CAF50");
        }
        else if (ScorePercent >= 50)
        {
            Headline = "Good Job!";
            Trophy = "\u2B50";
            HeadlineColor = Color.FromArgb("#FF9800");
        }
        else
        {
            Headline = "Keep Practicing!";
            Trophy = "\uD83D\uDCAA";
            HeadlineColor = Color.FromArgb("#F44336");
        }
    }

    [RelayCommand]
    private async Task ContinueLearning()
    {
        await Shell.Current.GoToAsync("//main/home");
    }

    [RelayCommand]
    private async Task ReviewLesson()
    {
        await Shell.Current.GoToAsync("../..");
    }
}
