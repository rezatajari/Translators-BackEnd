using Translators.Application.Contracts.Security;

namespace Translators.Application.Security;

public interface IGoogleTokenValidator
{
    Task<GoogleIdentity> ValidateAsync(
        string idToken,
        CancellationToken cancellationToken = default);
}