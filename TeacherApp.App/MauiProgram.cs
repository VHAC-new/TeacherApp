using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using TeacherApp.App.Services;
using TeacherApp.App.ViewModels;
using TeacherApp.App.Views;

namespace TeacherApp.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseMauiCommunityToolkitMediaElement()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        var apiBaseUrl =
#if ANDROID
            "http://10.0.2.2:5092";
#else
            "http://localhost:5092";
#endif

        builder.Services.AddSingleton<TokenStore>();
        builder.Services.AddTransient<BearerTokenHandler>();

        builder.Services.AddHttpClient("Api", client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        }).AddHttpMessageHandler<BearerTokenHandler>();

        builder.Services.AddSingleton(sp =>
            sp.GetRequiredService<IHttpClientFactory>().CreateClient("Api"));

        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<CatalogService>();
        builder.Services.AddSingleton<ExerciseService>();
        builder.Services.AddSingleton<ProgressService>();
        builder.Services.AddSingleton<MediaPlaybackService>();

        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<ModuleViewModel>();
        builder.Services.AddTransient<LessonViewModel>();
        builder.Services.AddTransient<ExerciseViewModel>();
        builder.Services.AddTransient<FinalExercisesViewModel>();

        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<ModulePage>();
        builder.Services.AddTransient<LessonPage>();
        builder.Services.AddTransient<ExercisePage>();
        builder.Services.AddTransient<FinalExercisesPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
