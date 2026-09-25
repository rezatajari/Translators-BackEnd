namespace Translators.Application.Contracts.Users;

public record PaginatedUsersRequestContract
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchText { get; init; }
}

public record PaginatedUserItemContract
{
    public Guid UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string RoleName { get; init; } = string.Empty;
}

public record PaginatedUsersResponseContract
{
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public List<PaginatedUserItemContract> Items { get; init; } = new();
}
