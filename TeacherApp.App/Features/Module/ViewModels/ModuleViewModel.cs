using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TeacherApp.App.Core.Services;
using TeacherApp.Contracts.Lessons;

namespace TeacherApp.App.Features.Module.ViewModels;

[QueryProperty(nameof(ModuleId), "moduleId")]
[QueryProperty(nameof(Title), "title")]
public partial class ModuleViewModel(CatalogService catalog) : ObservableObject
{
    [ObservableProperty]
    private string _moduleId = "";

    [ObservableProperty]
    private string _title = "";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _error;

    public ObservableCollection<LessonResponse> Lessons { get; } = [];

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (!Guid.TryParse(ModuleId, out var id)) return;

        IsBusy = true;
        Error = null;

        try
        {
            var lessons = await catalog.GetLessonsAsync(id);
            Lessons.Clear();
            foreach (var l in lessons) Lessons.Add(l);
        }
        catch (HttpRequestException)
        {
            Error = "Erro ao carregar lições.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToLesson(LessonResponse lesson)
    {
        var audio = lesson.AudioMediaId is { } aid ? $"&audioMediaId={aid}" : "";
        await Shell.Current.GoToAsync(
            $"lesson?moduleId={ModuleId}&lessonId={lesson.Id}&title={Uri.EscapeDataString(lesson.Title)}{audio}");
    }

    [RelayCommand]
    private async Task NavigateToFinalExercises()
    {
        await Shell.Current.GoToAsync(
            $"final-exercises?moduleId={ModuleId}&title={Uri.EscapeDataString(Title)}");
    }
}
