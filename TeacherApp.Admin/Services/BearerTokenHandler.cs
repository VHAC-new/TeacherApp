using System.Net.Http.Headers;

namespace TeacherApp.Admin.Services;

public sealed class BearerTokenHandler(TokenStorageService tokenStorage) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (tokenStorage.IsAuthenticated)
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenStorage.AccessToken);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
