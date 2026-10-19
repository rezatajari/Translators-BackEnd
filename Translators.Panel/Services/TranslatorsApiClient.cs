using System.Net.Http.Headers;
using System.Net.Http.Json;
using Translators.Application.Contracts.Users;

namespace Translators.Panel.Services;

public sealed class TranslatorsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly AuthSession _session;

    public TranslatorsApiClient(HttpClient httpClient, AuthSession session)
    {
        _httpClient = httpClient;
        _session = session;
    }

    public async Task<LoginUserResponseContract?> LoginAsync(LoginUserRequestContract request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/users/login", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var result = await response.Content.ReadFromJsonAsync<LoginUserResponseContract>(cancellationToken);
        if (result is not null)
        {
            _session.SignIn(result);
        }

        return result;
    }

    public async Task<RegisterUserResponseContract?> RegisterAsync(RegisterUserRequestContract request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/users/register", request, cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<RegisterUserResponseContract>(cancellationToken)
            : null;
    }

    public async Task<PaginatedUsersResponseContract> GetUsersAsync(int pageNumber, int pageSize, string? searchText, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/users/Paginated")
        {
            Content = JsonContent.Create(new PaginatedUsersRequestContract
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchText = searchText
            })
        };
        AddBearerToken(request);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PaginatedUsersResponseContract>(cancellationToken)
            ?? new PaginatedUsersResponseContract { PageNumber = pageNumber, PageSize = pageSize };
    }

    private void AddBearerToken(HttpRequestMessage request)
    {
        if (_session.CurrentUser is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _session.CurrentUser.Token);
        }
    }
}