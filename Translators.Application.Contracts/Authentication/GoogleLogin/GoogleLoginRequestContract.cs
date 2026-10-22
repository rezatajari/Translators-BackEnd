namespace Translators.Application.Contracts.Authentication.GoogleLogin;

public record GoogleLoginRequestContract
{
    public string IdToken { get; init; } = string.Empty;
}