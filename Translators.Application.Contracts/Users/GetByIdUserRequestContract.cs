namespace Translators.Application.Contracts.Users;

public record GetByIdUserRequestContract
{
    public Guid Guid { get; init; }
}

public record GetByIdUserResponseContract
{
    public Guid UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string RoleName { get; init; } = "User";
}
