using Microsoft.EntityFrameworkCore;
using Translators.Application.Contracts.Users;
using Translators.Application.Users;
using Translators.EntityFrameworkCore.DbContexts;

namespace Translators.Tests.ApiTests.Users;

public class RegisterTests
{
    [Fact]
    public async Task Register_WithValidRequest_ReturnsCreatedUser()
    {
        await using var context = CreateContext();
        var handler = new RegisterUserHandler(context);

        var request = new RegisterUserRequestContract
        {
            Name = "Ali Reza",
            Email = "ali@example.com",
            Password = "P@ssw0rd123"
        };

        var result = await handler.HandleAsync(request);

        Assert.Equal("ali@example.com", result.Email);
        Assert.Equal("Ali Reza", result.Name);
        Assert.NotEqual(Guid.Empty, result.UserId);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ThrowsException()
    {
        await using var context = CreateContext();
        var handler = new RegisterUserHandler(context);

        await handler.HandleAsync(new RegisterUserRequestContract
        {
            Name = "Ali Reza",
            Email = "duplicate@example.com",
            Password = "P@ssw0rd123"
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(new RegisterUserRequestContract
        {
            Name = "Ali Reza 2",
            Email = "duplicate@example.com",
            Password = "AnotherPass456!"
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
