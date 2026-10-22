using Translators.Application.Contracts.Users;
using Translators.Application.Security;

namespace Translators.Application.Authentication.GoogleLogin;

public class GoogleTokenLoginHandler
{
    private readonly IGoogleTokenValidator _tokenValidator;
    private readonly GoogleLoginHandler _googleLoginHandler;

    public GoogleTokenLoginHandler(
        IGoogleTokenValidator tokenValidator,
        GoogleLoginHandler googleLoginHandler)
    {
        _tokenValidator = tokenValidator;
        _googleLoginHandler = googleLoginHandler;
    }

    public async Task<LoginUserResponseContract> HandleAsync(
        string idToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idToken))
        {
            throw new UnauthorizedAccessException(
                "Google token is required.");
        }

        var identity = await _tokenValidator.ValidateAsync(
            idToken,
            cancellationToken);

        return await _googleLoginHandler.HandleAsync(
            identity,
            cancellationToken);
    }
}