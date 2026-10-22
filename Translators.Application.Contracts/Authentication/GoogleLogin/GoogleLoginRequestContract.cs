namespace Translators.Application.Contracts.Users;

public record GoogleLoginRequestContract
{
    public string IdToken { get; init; } = string.Empty;
}