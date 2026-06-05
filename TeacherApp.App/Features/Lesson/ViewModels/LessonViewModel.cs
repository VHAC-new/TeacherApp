using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TeacherApp.App.Core;
using TeacherApp.App.Features.Lesson.Services;

namespace TeacherApp.App.Features.Lesson.ViewModels;

[QueryProperty(nameof(ModuleId), "moduleId")]
[QueryProperty(nameof(LessonId), "lessonId")]
[QueryProperty(nameof(Title), "title")]
[QueryProperty(nameof(Description), "description")]
[QueryProperty(nameof(AudioMediaId), "audioMediaId")]
public partial class LessonViewModel(MediaPlaybackService mediaPlayback) : ObservableObject, ICleanup
{
    private CancellationTokenSource? _cts;
    private bool _audioLoaded;

    [ObservableProperty]
    private string _moduleId = "";

    [ObservableProperty]
    private string _lessonId = "";

    [ObservableProperty]
    private string _title = "";

    [ObservableProperty]
    private string _description = "";

    [ObservableProperty]
    private string _audioMediaId = "";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _lessonAudioUri;

    [ObservableProperty]
    private bool _hasLessonAudio;

    [ObservableProperty]
    private bool _isAudioLoading;

    [ObservableProperty]
    private string? _audioLoadError;

    [ObservableProperty]
    private bool _hasDescription;

    partial void OnDescriptionChanged(string value)
    {
        HasDescription = !string.IsNullOrWhiteSpace(value);
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (_audioLoaded) return;

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

        IsBusy = true;

        try
        {
            if (Guid.TryParse(AudioMediaId, out var audioId))
            {
                HasLessonAudio = true;
                IsAudioLoading = true;
                try
                {
                    LessonAudioUri = await mediaPlayback.ResolveLessonAudioUriAsync(audioId, ct);
                    if (ct.IsCancellationRequested) return;

                    if (LessonAudioUri is null)
                        AudioLoadError = "Não foi possível carregar o áudio desta lição.";
                }
                catch (OperationCanceledException) { }
                catch (HttpRequestException)
                {
                    AudioLoadError = "Erro de rede ao obter o áudio.";
                }
                catch (Exception)
                {
                    AudioLoadError = "Erro ao preparar o áudio.";
                }
                finally
                {
                    if (!ct.IsCancellationRequested)
                        IsAudioLoading = false;
                }
            }

            _audioLoaded = true;
        }
        finally
        {
            if (!ct.IsCancellationRequested)
                IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ContinueToExercises()
    {
        await Shell.Current.GoToAsync(
            $"exercise?moduleId={ModuleId}&lessonId={LessonId}");
    }

    public void Cleanup()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        _audioLoaded = false;
        IsAudioLoading = false;
        IsBusy = false;
    }
}
