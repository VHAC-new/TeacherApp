using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using TeacherApp.Admin.Services;

namespace TeacherApp.Admin.Auth;

public sealed class JwtAuthenticationStateProvider(TokenStorageService tokenStorage) : AuthenticationStateProvider
{
    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (!tokenStorage.IsAuthenticated)
            return Task.FromResult(Anonymous);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, tokenStorage.UserId.ToString()!),
            new(ClaimTypes.Email, tokenStorage.Email!),
            new(ClaimTypes.Name, tokenStorage.Email!),
        };

        foreach (var role in tokenStorage.Roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var identity = new ClaimsIdentity(claims, "jwt");
        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }

    public void NotifyAuthStateChanged() =>
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
}
