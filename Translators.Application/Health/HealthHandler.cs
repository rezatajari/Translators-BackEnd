using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Translators.Application.Contracts.Health;

namespace Translators.Application.Health;

public class HealthHandler
{
    public HealthResponseContract Handle(ClaimsPrincipal user)
    {
        if (user?.Identity is not { IsAuthenticated: true })
        {
            throw new UnauthorizedAccessException("A valid JWT token is required.");
        }

        var userIdValue = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            throw new UnauthorizedAccessException("The token does not contain a valid user id.");
        }

        return new HealthResponseContract
        {
            Status = "ok",
            IsAuthenticated = true,
            UserId = userId,
            Email = user.FindFirst(ClaimTypes.Email)?.Value
                ?? user.FindFirst(JwtRegisteredClaimNames.Email)?.Value
                ?? string.Empty,
            Name = user.FindFirst(ClaimTypes.Name)?.Value
                ?? user.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value
                ?? string.Empty
        };
    }
}
