using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Translators.Application.Authentication.GoogleLogin;
using Translators.Application.Contracts.Authentication.GoogleLogin;
using Translators.Application.Contracts.Users;

namespace Translators.HttpApi.Authentication.GoogleLogin;

public class GoogleLoginApi
    : Endpoint<GoogleLoginRequestContract, LoginUserResponseContract>
{
    private readonly GoogleTokenLoginHandler _handler;

    public GoogleLoginApi(GoogleTokenLoginHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Post("/api/users/google-login");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Log in or register using Google";
            s.Description = "Validates a Google ID token, creates a user if needed, and returns a JWT token.";
        });
    }

    public override async Task HandleAsync(
        GoogleLoginRequestContract req,
        CancellationToken ct)
    {
        try
        {
            var result = await _handler.HandleAsync(req.IdToken, ct);
            await SendOkAsync(result, ct);
        }
        catch (UnauthorizedAccessException)
        {
            await SendUnauthorizedAsync(ct);
        }
        catch (InvalidOperationException ex)
        {
            AddError(ex.Message);
            await SendErrorsAsync(StatusCodes.Status400BadRequest, ct);
        }
    }
}