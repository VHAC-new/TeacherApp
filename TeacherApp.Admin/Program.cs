using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using TeacherApp.Admin.Auth;
using TeacherApp.Admin.Components;
using TeacherApp.Admin.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// AuthorizeRouteView / [Authorize] require IAuthenticationService on the host even when
// Blazor auth state comes from AuthenticationStateProvider + in-memory JWT (TokenStorageService).
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
    });
builder.Services.AddAuthorization();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<TokenStorageService>();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();

var apiBaseUrl = builder.Configuration["Api:BaseUrl"] ?? "http://localhost:5092";

// HttpClientFactory resolves message handlers outside the Blazor circuit scope, so
// BearerTokenHandler would get a different TokenStorageService than Login/components.
// Build the pipeline in the same scoped provider as the rest of the circuit.
builder.Services.AddScoped(sp =>
{
    var tokenStorage = sp.GetRequiredService<TokenStorageService>();
    var inner = new HttpClientHandler();
    var bearer = new BearerTokenHandler(tokenStorage) { InnerHandler = inner };
    var client = new HttpClient(bearer) { BaseAddress = new Uri(apiBaseUrl) };
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    return client;
});

builder.Services.AddMudServices();
builder.Services.AddScoped<AdminThemeState>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
