
using Microsoft.EntityFrameworkCore;
using Translators.Application.Contracts.Security;
using Translators.Application.Security;
using Translators.Application.Users;
using Translators.Domain.Users;
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
    
    
    [Fact]
    public async Task GoogleLogin_WithExistingGoogleUser_ReturnsSameUserWithoutCreatingDuplicate()
    {
        // Arrange
        await using var context = CreateContext();

        var role = new RoleEntity
        {
            Id = Guid.NewGuid(),
            Name = RoleNames.User,
            Description = "Default registered user role.",
            Permissions = "GetByIdUserHandler,PaginatedUserHandler,LookupUserHandler",
            IsSystemRole = true
        };

        var existingUser = new UserEntity
        {
            Id = Guid.NewGuid(),
            Name = "Google User",
            Email = "google@example.com",
            GoogleSubjectId = "google-subject-123",
            PasswordHash = string.Empty,
            RoleId = role.Id,
            Role = role
        };

        context.Roles.Add(role);
        context.Users.Add(existingUser);
        await context.SaveChangesAsync();

        var handler = new GoogleLoginHandler(
            context,
            CreateJwtTokenService());

        var googleIdentity = new GoogleIdentity
        {
            SubjectId = "google-subject-123",
            Email = "google@example.com",
            Name = "Google User"
        };

        // Act
        var result = await handler.HandleAsync(googleIdentity);

        // Assert
        var users = await context.Users.ToListAsync();

        Assert.Single(users);
        Assert.Equal(existingUser.Id, result.UserId);
        Assert.Equal(existingUser.Email, result.Email);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
    }
    
    
    [Fact]
    public async Task GoogleLogin_WithEmailAlreadyUsedByNonGoogleUser_ThrowsAndDoesNotCreateDuplicate()
    {
        await using var context = CreateContext();

        var role = new RoleEntity
        {
            Id = Guid.NewGuid(),
            Name = RoleNames.User,
            Description = "Default registered user role.",
            Permissions = "GetByIdUserHandler,PaginatedUserHandler,LookupUserHandler",
            IsSystemRole = true
        };

        var existingUser = new UserEntity
        {
            Id = Guid.NewGuid(),
            Name = "Existing User",
            Email = "existing@example.com",
            PasswordHash = "existing-password-hash",
            RoleId = role.Id,
            Role = role
        };

        context.Roles.Add(role);
        context.Users.Add(existingUser);
        await context.SaveChangesAsync();

        var handler = new GoogleLoginHandler(
            context,
            CreateJwtTokenService());

        var googleIdentity = new GoogleIdentity
        {
            SubjectId = "google-subject-456",
            Email = "existing@example.com",
            Name = "Google User"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.HandleAsync(googleIdentity));

        var users = await context.Users.ToListAsync();

        Assert.Single(users);
        Assert.Equal(existingUser.Id, users[0].Id);
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