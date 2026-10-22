namespace Translators.Application.Contracts.Authentication.GoogleLogin;

public record GoogleIdentity
{
    public string SubjectId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}