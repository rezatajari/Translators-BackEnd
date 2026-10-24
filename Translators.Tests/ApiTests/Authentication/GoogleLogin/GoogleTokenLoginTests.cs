using Microsoft.EntityFrameworkCore;
using Translators.Application.Authentication.GoogleLogin;
using Translators.Application.Contracts.Authentication.GoogleLogin;
using Translators.Application.Contracts.Security;
using Translators.Application.Security;
using Translators.Application.Users;
using Translators.EntityFrameworkCore.DbContexts;

namespace Translators.Tests.ApiTests.Authentication.GoogleLogin;

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

        Assert.Equal(0, validator.CallCount);
        Assert.Equal(0, await context.Users.CountAsync());
    }
    
    [Fact]
    public async Task InvalidGoogleToken_ShouldBeRejected()
    {
        var validator = new GoogleTokenValidator("test-client-id");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => validator.ValidateAsync("invalid-token"));
    }
    
    [Fact]
    public async Task ValidGoogleIdentity_ShouldCreateUserAndReturnResponse()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TranslatorDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new TranslatorDbContext(options);

        var identity = new GoogleIdentity
        {
            SubjectId = "google-subject-123",
            Email = "test@example.com",
            Name = "Test User"
        };

        var validator = new FakeGoogleTokenValidator(identity);

        var jwtService = new JwtTokenService(new JwtOptions
        {
            Issuer = "TranslatorsTests",
            Audience = "TranslatorsTests",
            SigningKey = "TestSigningKey_MustBeAtLeast32Chars!",
            ExpirationMinutes = 60
        });

        var loginHandler = new GoogleLoginHandler(context, jwtService);
        var handler = new GoogleTokenLoginHandler(validator, loginHandler);

        // Act
        var response = await handler.HandleAsync("valid-test-token");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(1, validator.CallCount);

        var user = await context.Users
            .SingleOrDefaultAsync(u => u.GoogleSubjectId == identity.SubjectId);

        Assert.NotNull(user);
        Assert.Equal(identity.Email, user.Email);
        Assert.Equal(identity.Name, user.Name);
    }

    private sealed class FakeGoogleTokenValidator : IGoogleTokenValidator
    {
        private readonly GoogleIdentity? _identity;

        public int CallCount { get; private set; }

        public FakeGoogleTokenValidator(GoogleIdentity? identity = null)
        {
            _identity = identity;
        }

        public Task<GoogleIdentity> ValidateAsync(
            string idToken,
            CancellationToken cancellationToken = default)
        {
            CallCount++;

            if (_identity is null)
            {
                throw new UnauthorizedAccessException("Invalid Google token.");
            }

            return Task.FromResult(_identity);
        }
    }
}