using System.Security.Claims;
using Translators.Application.Health;

namespace Translators.Tests.ApiTests.Health;

public class HealthTests
{
    [Fact]
    public void Health_WithAuthenticatedUser_ReturnsOk()
    {
        var userId = Guid.NewGuid();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, "health@example.com"),
            new Claim(ClaimTypes.Name, "Health User")
        }, authenticationType: "Bearer"));

        var result = new HealthHandler().Handle(user);

        Assert.Equal("ok", result.Status);
        Assert.True(result.IsAuthenticated);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("health@example.com", result.Email);
    }

    [Fact]
    public void Health_WithoutAuthentication_ThrowsUnauthorized()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity());

        Assert.Throws<UnauthorizedAccessException>(() => new HealthHandler().Handle(user));
    }
}
