using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plugin.Maui.Audio;
using TeacherApp.App.Core;
using TeacherApp.App.Features.Lesson.Services;

namespace TeacherApp.App.Features.Lesson.ViewModels;

[QueryProperty(nameof(ModuleId), "moduleId")]
[QueryProperty(nameof(LessonId), "lessonId")]
[QueryProperty(nameof(Title), "title")]
[QueryProperty(nameof(Description), "description")]
[QueryProperty(nameof(AudioMediaId), "audioMediaId")]
[QueryProperty(nameof(ModuleTitle), "moduleTitle")]
public partial class LessonViewModel(MediaPlaybackService mediaPlayback) : ObservableObject, ICleanup
{
    private CancellationTokenSource? _cts;
    private bool _audioLoaded;
    private IAudioPlayer? _audioPlayer;
    private IDispatcherTimer? _progressTimer;

    // Play glyph = &#xe037; (play_arrow), Pause glyph = &#xe034; (pause)
    private const string PlayGlyph = "\ue037";
    private const string PauseGlyph = "\ue034";

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
    private string _moduleTitle = "";

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

    [ObservableProperty]
    private bool _isPlaying;

    [ObservableProperty]
    private double _audioProgress;

    [ObservableProperty]
    private string _audioCurrentTime = "0:00";

    [ObservableProperty]
    private string _audioDuration = "0:00";

    [ObservableProperty]
    private string _playButtonGlyph = PlayGlyph;

    public string AudioTimeDisplay => $"{AudioCurrentTime} / {AudioDuration}";

    partial void OnDescriptionChanged(string value)
    {
        HasDescription = !string.IsNullOrWhiteSpace(value);
    }

    partial void OnLessonAudioUriChanged(string? value)
    {
        if (!string.IsNullOrEmpty(value))
            InitAudioPlayer(value);
    }

    private void InitAudioPlayer(string uri)
    {
        try
        {
            ReleaseAudioPlayer();

            var audioManager = AudioManager.Current;

            string filePath;
            if (uri.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
                filePath = new Uri(uri).LocalPath;
            else if (uri.StartsWith("/", StringComparison.Ordinal) || uri.Contains(":\\"))
                filePath = uri;
            else
            {
                AudioLoadError = "Formato de áudio não suportado.";
                return;
            }

            var stream = File.OpenRead(filePath);
            _audioPlayer = audioManager.CreatePlayer(stream);

            _audioPlayer.PlaybackEnded += OnPlaybackEnded;

            AudioDuration = FormatTime(_audioPlayer.Duration);
            OnPropertyChanged(nameof(AudioTimeDisplay));
        }
        catch
        {
            AudioLoadError = "Erro ao preparar o player de áudio.";
        }
    }

    private void OnPlaybackEnded(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            IsPlaying = false;
            PlayButtonGlyph = PlayGlyph;
            AudioProgress = 0;
            AudioCurrentTime = "0:00";
            StopProgressTimer();
            OnPropertyChanged(nameof(AudioTimeDisplay));
        });
    }

    [RelayCommand]
    private void TogglePlay()
    {
        if (_audioPlayer is null) return;

        if (IsPlaying)
        {
            _audioPlayer.Pause();
            IsPlaying = false;
            PlayButtonGlyph = PlayGlyph;
            StopProgressTimer();
        }
        else
        {
            _audioPlayer.Play();
            IsPlaying = true;
            PlayButtonGlyph = PauseGlyph;
            StartProgressTimer();
        }
    }

    private void StartProgressTimer()
    {
        StopProgressTimer();

        _progressTimer = Application.Current?.Dispatcher.CreateTimer();
        if (_progressTimer is null) return;

        _progressTimer.Interval = TimeSpan.FromMilliseconds(250);
        _progressTimer.Tick += OnProgressTimerTick;
        _progressTimer.Start();
    }

    private void StopProgressTimer()
    {
        if (_progressTimer is null) return;
        _progressTimer.Stop();
        _progressTimer.Tick -= OnProgressTimerTick;
        _progressTimer = null;
    }

    private void OnProgressTimerTick(object? sender, EventArgs e)
    {
        if (_audioPlayer is null) return;

        var duration = _audioPlayer.Duration;
        var current = _audioPlayer.CurrentPosition;

        AudioProgress = duration > 0 ? current / duration : 0;
        AudioCurrentTime = FormatTime(current);
        AudioDuration = FormatTime(duration);
        OnPropertyChanged(nameof(AudioTimeDisplay));
    }

    private static string FormatTime(double seconds)
    {
        var ts = TimeSpan.FromSeconds(Math.Max(0, seconds));
        return ts.Hours > 0
            ? ts.ToString(@"h\:mm\:ss")
            : ts.ToString(@"m\:ss");
    }

    public void StopAudio()
    {
        StopProgressTimer();

        if (_audioPlayer is not null)
        {
            try { _audioPlayer.Stop(); } catch { }
            IsPlaying = false;
            PlayButtonGlyph = PlayGlyph;
        }
    }

    private void ReleaseAudioPlayer()
    {
        if (_audioPlayer is null)
            return;

        _audioPlayer.PlaybackEnded -= OnPlaybackEnded;
        try { _audioPlayer.Stop(); } catch { }
        _audioPlayer.Dispose();
        _audioPlayer = null;
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
        var moduleTitle = Uri.EscapeDataString(ModuleTitle);
        await Shell.Current.GoToAsync(
            $"exercise?moduleId={ModuleId}&lessonId={LessonId}&moduleTitle={moduleTitle}");
    }

    public void Cleanup()
    {
        StopProgressTimer();
        ReleaseAudioPlayer();
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        _audioLoaded = false;
        IsAudioLoading = false;
        IsBusy = false;
    }
}
