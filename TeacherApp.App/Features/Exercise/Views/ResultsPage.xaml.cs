using TeacherApp.App.Features.Exercise.ViewModels;

namespace TeacherApp.App.Features.Exercise.Views;

public partial class ResultsPage : ContentPage
{
    private bool _entrancePlayed;

    public ResultsPage(ResultsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_entrancePlayed)
            return;

        _entrancePlayed = true;
        await PlayEntranceAnimationAsync();
    }

    private async Task PlayEntranceAnimationAsync()
    {
        ResultsContent.AbortAnimation("ResultsEntrance");
        TrophyLabel.AbortAnimation("TrophyPop");
        HeadlineLabel.AbortAnimation("HeadlineIn");
        SubtitleLabel.AbortAnimation("SubtitleIn");
        ScoreCard.AbortAnimation("ScoreCardIn");
        ActionsPanel.AbortAnimation("ActionsIn");

        await Task.WhenAll(
            ResultsContent.FadeTo(1, 400, Easing.CubicOut),
            ResultsContent.ScaleTo(1, 450, Easing.SpringOut),
            ResultsContent.TranslateTo(0, 0, 450, Easing.SpringOut));

        await TrophyLabel.ScaleTo(1, 500, Easing.SpringOut);

        await Task.WhenAll(
            HeadlineLabel.FadeTo(1, 280, Easing.CubicOut),
            HeadlineLabel.TranslateTo(0, 0, 280, Easing.CubicOut),
            SubtitleLabel.FadeTo(1, 280, Easing.CubicOut),
            SubtitleLabel.TranslateTo(0, 0, 280, Easing.CubicOut));

        ScoreCard.Opacity = 0;
        ScoreCard.Scale = 0.9;
        await Task.WhenAll(
            ScoreCard.FadeTo(1, 350, Easing.CubicOut),
            ScoreCard.ScaleTo(1, 400, Easing.SpringOut));

        await Task.WhenAll(
            ActionsPanel.FadeTo(1, 300, Easing.CubicOut),
            ActionsPanel.TranslateTo(0, 0, 300, Easing.CubicOut));
    }
}
