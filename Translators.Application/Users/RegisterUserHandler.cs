using Microsoft.EntityFrameworkCore;
using Translators.Application.Contracts.Users;
using Translators.Domain.Users;
using Translators.EntityFrameworkCore.DbContexts;

namespace Translators.Application.Users;

public class RegisterUserHandler
{
    private readonly TranslatorDbContext _context;

    public RegisterUserHandler(TranslatorDbContext context)
    {
        _context = context;
    }

    public async Task<RegisterUserResponseContract> HandleAsync(RegisterUserRequestContract request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            throw new InvalidOperationException("Name, email, and password are required.");
        }

        var normalizedEmail = request.Email.Trim();

        var userExists = await _context.Users.AnyAsync(x => x.Email == normalizedEmail, cancellationToken);
        if (userExists)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var entity = new UserEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        _context.Users.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return new RegisterUserResponseContract
        {
            UserId = entity.Id,
            Name = entity.Name,
            Email = entity.Email
        };
    }
}
