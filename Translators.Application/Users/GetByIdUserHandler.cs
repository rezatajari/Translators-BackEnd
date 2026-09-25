using Microsoft.EntityFrameworkCore;
using Translators.Application.Contracts.Users;
using Translators.Domain.Users;
using Translators.EntityFrameworkCore.DbContexts;

namespace Translators.Application.Users;

public class GetByIdUserHandler
{
    private readonly TranslatorDbContext _context;

    public GetByIdUserHandler(TranslatorDbContext context)
    {
        _context = context;
    }

    public async Task<GetByIdUserResponseContract> HandleAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException("User not found.");
        }

        return new GetByIdUserResponseContract
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            RoleName = user.Role?.Name ?? RoleNames.User
        };
    }
}
