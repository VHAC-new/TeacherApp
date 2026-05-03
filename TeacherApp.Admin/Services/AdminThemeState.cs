using Microsoft.JSInterop;

namespace TeacherApp.Admin.Services;

/// <summary>
/// Holds dark/light preference for <see cref="MudBlazor.MudThemeProvider"/> and persists to localStorage.
/// </summary>
public sealed class AdminThemeState
{
    private readonly IJSRuntime _js;
    private bool _initialized;
    private bool _isDarkMode;

    public AdminThemeState(IJSRuntime js) => _js = js;

    public bool IsDarkMode => _isDarkMode;

    public event Func<Task>? ChangedAsync;

    public async Task InitializeAsync()
    {
        if (_initialized) return;
        _initialized = true;
        try
        {
            var stored = await _js.InvokeAsync<string?>("teacherAppAdminTheme.get");
            if (bool.TryParse(stored, out var parsed))
                _isDarkMode = parsed;
            else
                _isDarkMode = await _js.InvokeAsync<bool>("teacherAppAdminTheme.prefersDark");
        }
        catch
        {
            _isDarkMode = false;
        }

        await RaiseChangedAsync();
    }

    public async Task SetDarkModeAsync(bool value)
    {
        if (_isDarkMode == value) return;
        _isDarkMode = value;
        try
        {
            await _js.InvokeVoidAsync("teacherAppAdminTheme.set", value ? "true" : "false");
        }
        catch
        {
            // ignore storage failures (private mode, etc.)
        }

        await RaiseChangedAsync();
    }

    public Task ToggleAsync() => SetDarkModeAsync(!_isDarkMode);

    private async Task RaiseChangedAsync()
    {
        if (ChangedAsync is null) return;
        await ChangedAsync.Invoke();
    }
}
