using Microsoft.EntityFrameworkCore;
using Translators.Application.Contracts.Authentication.GoogleLogin;
using Translators.Application.Contracts.Security;
using Translators.Application.Contracts.Users;
using Translators.Application.Security;
using Translators.Domain.Users;
using Translators.EntityFrameworkCore.DbContexts;

namespace Translators.Application.Authentication.GoogleLogin;

public class GoogleLoginHandler
{
    private readonly TranslatorDbContext _context;
    private readonly JwtTokenService _jwtTokenService;

    public GoogleLoginHandler(
        TranslatorDbContext context,
        JwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginUserResponseContract> HandleAsync(
        GoogleIdentity identity,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(identity.SubjectId) ||
            string.IsNullOrWhiteSpace(identity.Email) ||
            string.IsNullOrWhiteSpace(identity.Name))
        {
            throw new InvalidOperationException(
                "Google identity must contain subject ID, email, and name.");
        }

        var subjectId = identity.SubjectId.Trim();
        var email = identity.Email.Trim();

        var user = await _context.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(
                x => x.GoogleSubjectId == subjectId,
                cancellationToken);

        if (user is null)
        {
            var existingEmailUser = await _context.Users
                .AnyAsync(x => x.Email == email, cancellationToken);

            if (existingEmailUser)
            {
                throw new InvalidOperationException(
                    "A user with this email already exists.");
            }

            var userRole = await _context.Roles
                .FirstOrDefaultAsync(
                    x => x.Name == RoleNames.User,
                    cancellationToken);

            if (userRole is null)
            {
                userRole = new RoleEntity
                {
                    Id = Guid.NewGuid(),
                    Name = RoleNames.User,
                    Description = "Default registered user role.",
                    Permissions =
                        "GetByIdUserHandler,PaginatedUserHandler,LookupUserHandler",
                    IsSystemRole = true
                };

                _context.Roles.Add(userRole);
            }

            user = new UserEntity
            {
                Id = Guid.NewGuid(),
                Name = identity.Name.Trim(),
                Email = email,
                GoogleSubjectId = subjectId,
                PasswordHash = string.Empty,
                RoleId = userRole.Id,
                Role = userRole
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return new LoginUserResponseContract
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            Token = _jwtTokenService.CreateToken(user)
        };
    }
}
