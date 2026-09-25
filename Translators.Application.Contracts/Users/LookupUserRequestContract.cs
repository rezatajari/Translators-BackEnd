namespace Translators.Application.Contracts.Users;

public record LookupUserRequestContract
{
    public string? SearchText { get; init; }
    public int Take { get; init; } = 20;
}

public record LookupUserItemContract
{
    public Guid UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string RoleName { get; init; } = string.Empty;
}

public record LookupUsersResponseContract
{
    public List<LookupUserItemContract> Items { get; init; } = new();
}
