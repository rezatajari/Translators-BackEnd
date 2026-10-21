using Microsoft.EntityFrameworkCore;
using Translators.Application.Contracts.Security;
using Translators.Application.Security;
using Translators.Application.Users;
using Translators.EntityFrameworkCore.DbContexts;

namespace Translators.Tests.ApiTests.Users;

public class GoogleLoginTests
{
    [Fact]
    public async Task GoogleLogin_WithValidIdentityAndNewUser_CreatesUserAndReturnsJwt()
    {
        // Arrange
        await using var context = CreateContext();

        var googleIdentity = new GoogleIdentity
        {
            SubjectId = "google-subject-123",
            Email = "google@example.com",
            Name = "Google User"
        };

        var handler = new GoogleLoginHandler(
            context,
            CreateJwtTokenService());

        // Act
        var result = await handler.HandleAsync(googleIdentity);

        // Assert
        var user = await context.Users
            .SingleOrDefaultAsync(x => x.Email == googleIdentity.Email);

        Assert.NotNull(user);
        Assert.Equal(googleIdentity.SubjectId, user.GoogleSubjectId);
        Assert.Equal(googleIdentity.Email, user.Email);
        Assert.Equal(googleIdentity.Name, user.Name);

        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(user.Email, result.Email);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
    }

    private static TranslatorDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TranslatorDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TranslatorDbContext(options);
    }

    private static JwtTokenService CreateJwtTokenService()
    {
        return new JwtTokenService(new JwtOptions
        {
            Issuer = "TranslatorsTests",
            Audience = "TranslatorsTests",
            SigningKey = "TestSigningKey_MustBeAtLeast32Chars!",
            ExpirationMinutes = 60
        });
    }
}