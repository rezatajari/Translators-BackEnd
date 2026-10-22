using Translators.Application.Contracts.Authentication.GoogleLogin;
using Translators.Application.Contracts.Security;

namespace Translators.Application.Authentication.GoogleLogin;

public interface IGoogleTokenValidator
{
    Task<GoogleIdentity> ValidateAsync(
        string idToken,
        CancellationToken cancellationToken = default);
}