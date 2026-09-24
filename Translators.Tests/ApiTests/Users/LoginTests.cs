using Microsoft.EntityFrameworkCore;
using Translators.Application.Contracts.Users;
using Translators.Application.Users;
using Translators.EntityFrameworkCore.DbContexts;

namespace Translators.Tests.ApiTests.Users;

public class LoginTests
{
    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOk()
    {
        await using var context = CreateContext();
        var registerHandler = new RegisterUserHandler(context);
        var loginHandler = new LoginHandler(context);

        await registerHandler.HandleAsync(new RegisterUserRequestContract
        {
            Name = "Login User",
            Email = "login@example.com",
            Password = "P@ssw0rd123"
        });

        var result = await loginHandler.HandleAsync(new LoginUserRequestContract
        {
            Email = "login@example.com",
            Password = "P@ssw0rd123"
        });

        Assert.Equal("login@example.com", result.Email);
        Assert.NotEqual(Guid.Empty, result.UserId);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ThrowsUnauthorized()
    {
        await using var context = CreateContext();
        var registerHandler = new RegisterUserHandler(context);
        var loginHandler = new LoginHandler(context);

        await registerHandler.HandleAsync(new RegisterUserRequestContract
        {
            Name = "Wrong Password User",
            Email = "wrongpass@example.com",
            Password = "P@ssw0rd123"
        });

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => loginHandler.HandleAsync(new LoginUserRequestContract
        {
            Email = "wrongpass@example.com",
            Password = "WrongPassword!"
        }));
    }

    private static TranslatorDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TranslatorDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TranslatorDbContext(options);
    }
}
