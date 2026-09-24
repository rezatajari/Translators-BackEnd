namespace Translators.Application.Contracts.Health;

public record HealthResponseContract
{
    public string Status { get; init; } = "ok";
    public bool IsAuthenticated { get; init; }
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}
