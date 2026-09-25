using Microsoft.EntityFrameworkCore;
using Translators.Application.Contracts.Users;
using Translators.Domain.Users;
using Translators.EntityFrameworkCore.DbContexts;

namespace Translators.Application.Users;

public class PaginatedUserHandler
{
    private readonly TranslatorDbContext _context;

    public PaginatedUserHandler(TranslatorDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedUsersResponseContract> HandleAsync(PaginatedUsersRequestContract request, CancellationToken cancellationToken = default)
    {
        var pageNumber = request.PageNumber > 0 ? request.PageNumber : 1;
        var pageSize = request.PageSize > 0 ? Math.Min(request.PageSize, 100) : 10;

        var query = _context.Users
            .Include(x => x.Role)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var searchText = request.SearchText.Trim();
            query = query.Where(x => x.Name.Contains(searchText) || x.Email.Contains(searchText));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new PaginatedUserItemContract
            {
                UserId = x.Id,
                Name = x.Name,
                Email = x.Email,
                RoleName = x.Role != null ? x.Role.Name : RoleNames.User
            })
            .ToListAsync(cancellationToken);

        return new PaginatedUsersResponseContract
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = items
        };
    }
}
