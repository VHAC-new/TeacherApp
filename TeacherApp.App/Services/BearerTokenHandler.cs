using System.Net.Http.Headers;

namespace TeacherApp.App.Services;

public sealed class BearerTokenHandler(TokenStore tokenStore) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (tokenStore.IsAuthenticated)
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenStore.AccessToken);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
