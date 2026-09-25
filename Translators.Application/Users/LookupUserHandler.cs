using Microsoft.EntityFrameworkCore;
using Translators.Application.Contracts.Users;
using Translators.Domain.Users;
using Translators.EntityFrameworkCore.DbContexts;

namespace Translators.Application.Users;

public class LookupUserHandler
{
    private readonly TranslatorDbContext _context;

    public LookupUserHandler(TranslatorDbContext context)
    {
        _context = context;
    }

    public async Task<LookupUsersResponseContract> HandleAsync(LookupUserRequestContract request, CancellationToken cancellationToken = default)
    {
        var take = request.Take > 0 ? Math.Min(request.Take, 100) : 20;
        var query = _context.Users
            .Include(x => x.Role)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var searchText = request.SearchText.Trim();
            query = query.Where(x => x.Name.Contains(searchText) || x.Email.Contains(searchText));
        }

        var items = await query
            .OrderBy(x => x.Name)
            .Take(take)
            .Select(x => new LookupUserItemContract
            {
                UserId = x.Id,
                Name = x.Name,
                Email = x.Email,
                RoleName = x.Role != null ? x.Role.Name : RoleNames.User
            })
            .ToListAsync(cancellationToken);

        return new LookupUsersResponseContract
        {
            Items = items
        };
    }
}
