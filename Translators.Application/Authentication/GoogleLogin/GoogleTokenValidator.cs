using Google.Apis.Auth;
using Translators.Application.Contracts.Security;
using Translators.Application.Security;

namespace Translators.Application.Authentication.GoogleLogin;

public class GoogleTokenValidator : IGoogleTokenValidator
{
    private readonly string _clientId;

    public GoogleTokenValidator(string clientId)
    {
        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new ArgumentException(
                "Google Client ID is required.",
                nameof(clientId));
        }

        _clientId = clientId;
    }

    public async Task<GoogleIdentity> ValidateAsync(
        string idToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idToken))
        {
            throw new UnauthorizedAccessException(
                "Google token is required.");
        }

        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(
                idToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _clientId }
                });

            if (string.IsNullOrWhiteSpace(payload.Subject) ||
                string.IsNullOrWhiteSpace(payload.Email) ||
                string.IsNullOrWhiteSpace(payload.Name) ||
                !payload.EmailVerified)
            {
                throw new UnauthorizedAccessException(
                    "Google identity is incomplete or email is not verified.");
            }

            return new GoogleIdentity
            {
                SubjectId = payload.Subject,
                Email = payload.Email,
                Name = payload.Name
            };
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex) when (
            ex is not UnauthorizedAccessException)
        {
            throw new UnauthorizedAccessException(
                "Invalid Google token.",
                ex);
        }
    }
}