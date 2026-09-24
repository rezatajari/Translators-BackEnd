using Microsoft.EntityFrameworkCore;
using Translators.Application.Contracts.Users;
using Translators.EntityFrameworkCore.DbContexts;

namespace Translators.Application.Users;

public class LoginHandler
{
    private readonly TranslatorDbContext _context;

    public LoginHandler(TranslatorDbContext context)
    {
        _context = context;
    }

    public async Task<LoginUserResponseContract> HandleAsync(LoginUserRequestContract request, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == request.Email.Trim(), cancellationToken);
        if (user is null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var isValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isValid)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        return new LoginUserResponseContract
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email
        };
    }
}
