using Microsoft.EntityFrameworkCore;
using Translators.Application.Contracts.Security;
using Translators.Application.Security;
using Translators.Application.Users;
using Translators.EntityFrameworkCore.DbContexts;
using Translators.Tests;
using Xunit;

namespace Translators.Tests.ApiTests.Users;

public class GoogleTokenLoginTests
{
    [Fact]
    public async Task InvalidGoogleToken_ShouldNotCreateUser()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TranslatorDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new TranslatorDbContext(options);

        var validator = new FakeGoogleTokenValidator();

        var jwtService = new JwtTokenService(new JwtOptions
        {
            Issuer = "TranslatorsTests",
            Audience = "TranslatorsTests",
            SigningKey = "TestSigningKey_MustBeAtLeast32Chars!",
            ExpirationMinutes = 60
        });

        var loginHandler = new GoogleLoginHandler(context, jwtService);

        var handler = new GoogleTokenLoginHandler(
            validator,
            loginHandler);

        // Act
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.HandleAsync("invalid-token"));

        // Assert
        Assert.Equal(0, await context.Users.CountAsync());
    }
    
    [Fact]
    public async Task EmptyGoogleToken_ShouldNotCreateUser()
    {
        var options = new DbContextOptionsBuilder<TranslatorDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new TranslatorDbContext(options);

        var validator = new FakeGoogleTokenValidator();

        var jwtService = new JwtTokenService(new JwtOptions
        {
            Issuer = "TranslatorsTests",
            Audience = "TranslatorsTests",
            SigningKey = "TestSigningKey_MustBeAtLeast32Chars!",
            ExpirationMinutes = 60
        });

        var loginHandler = new GoogleLoginHandler(context, jwtService);
        var handler = new GoogleTokenLoginHandler(validator, loginHandler);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.HandleAsync(string.Empty));

        Assert.Equal(0, await context.Users.CountAsync());
    }

    private sealed class FakeGoogleTokenValidator : IGoogleTokenValidator
    {
        public Task<GoogleIdentity> ValidateAsync(
            string idToken,
            CancellationToken cancellationToken = default)
        {
            throw new UnauthorizedAccessException("Invalid Google token.");
        }
    }
}