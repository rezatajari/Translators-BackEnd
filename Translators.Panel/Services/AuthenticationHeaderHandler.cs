using System.Net;
using System.Net.Http.Headers;

namespace Translators.Panel.Services;

/// <summary>
/// Adds the bearer token of the current session to every call of the generated client.
/// </summary>
public sealed class AuthenticationHeaderHandler : DelegatingHandler
{
    private readonly AuthSession _session;

    public AuthenticationHeaderHandler(AuthSession session)
    {
        _session = session;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_session.Token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _session.Token);
        }

        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode is HttpStatusCode.Unauthorized)
        {
            _session.SignOut();
        }

        return response;
    }
}
