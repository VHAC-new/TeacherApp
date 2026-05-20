using System.Net.Http.Json;
using TeacherApp.App.Core.Services;
using TeacherApp.Contracts.Auth;

namespace TeacherApp.App.Features.Login.Services;

public sealed class AuthService(HttpClient http, TokenStore tokenStore)
{
    public async Task<LoginResponse> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync("api/v1/auth/login", new LoginRequest(email, password), ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>(ct)
            ?? throw new InvalidOperationException("Resposta inesperada do servidor.");
        tokenStore.Store(result.AccessToken, result.Email, result.Roles, result.UserId);
        return result;
    }

    public async Task<MeResponse> GetMeAsync(CancellationToken ct = default) =>
        await http.GetFromJsonAsync<MeResponse>("api/v1/auth/me", ct)
        ?? throw new InvalidOperationException("Resposta inesperada do servidor.");

    public void Logout() => tokenStore.Clear();
    public bool IsAuthenticated => tokenStore.IsAuthenticated;
}
