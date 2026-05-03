using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TeacherApp.App.Services;
using TeacherApp.Contracts.Exercises;

namespace TeacherApp.App.ViewModels;

[QueryProperty(nameof(ModuleId), "moduleId")]
[QueryProperty(nameof(LessonId), "lessonId")]
[QueryProperty(nameof(Title), "title")]
[QueryProperty(nameof(AudioMediaId), "audioMediaId")]
public partial class LessonViewModel(CatalogService catalog, MediaPlaybackService mediaPlayback) : ObservableObject
{
    [ObservableProperty]
    private string _moduleId = "";

    [ObservableProperty]
    private string _lessonId = "";

    [ObservableProperty]
    private string _title = "";

    [ObservableProperty]
    private string _audioMediaId = "";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _error;

    [ObservableProperty]
    private string? _lessonAudioUri;

    [ObservableProperty]
    private bool _hasLessonAudio;

    [ObservableProperty]
    private bool _isAudioLoading;

    [ObservableProperty]
    private string? _audioLoadError;

    public ObservableCollection<ExerciseStudentResponse> Exercises { get; } = [];

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (!Guid.TryParse(ModuleId, out var modId) || !Guid.TryParse(LessonId, out var lesId))
        {
            return;
        }

        IsBusy = true;
        Error = null;
        LessonAudioUri = null;
        AudioLoadError = null;
        HasLessonAudio = false;
        IsAudioLoading = false;

        try
        {
            var exercises = await catalog.GetExercisesAsync(modId, lesId);
            Exercises.Clear();
            foreach (var e in exercises)
            {
                Exercises.Add(e);
            }

            if (Guid.TryParse(AudioMediaId, out var audioId))
            {
                HasLessonAudio = true;
                IsAudioLoading = true;
                try
                {
                    LessonAudioUri = await mediaPlayback.ResolveLessonAudioUriAsync(audioId);
                    if (LessonAudioUri is null)
                    {
                        AudioLoadError = "Não foi possível carregar o áudio desta lição.";
                    }
                }
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
                    IsAudioLoading = false;
                }
            }
        }
        catch (HttpRequestException)
        {
            Error = "Erro ao carregar exercícios.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToExercise(ExerciseStudentResponse exercise)
    {
        await Shell.Current.GoToAsync(
            $"exercise?exerciseId={exercise.Id}&prompt={Uri.EscapeDataString(exercise.Prompt)}&hint={Uri.EscapeDataString(exercise.Hint ?? "")}");
    }
}
