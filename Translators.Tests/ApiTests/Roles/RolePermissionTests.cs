using Microsoft.EntityFrameworkCore;
using Translators.Application.Contracts.Users;
using Translators.Application.Users;
using Translators.Domain.Users;
using Translators.EntityFrameworkCore.DbContexts;

namespace Translators.Tests.ApiTests.Roles;

public class RolePermissionTests
{
    [Fact]
    public void AdminRole_HasAccessToAllApiActions()
    {
        var role = new RoleEntity
        {
            Id = Guid.NewGuid(),
            Name = RoleNames.Admin,
            Description = "System administrator",
            Permissions = "*"
        };

        Assert.True(role.HasAccess("GetByIdUserHandler"));
        Assert.True(role.HasAccess("PaginatedUserHandler"));
        Assert.True(role.HasAccess("LookupUserHandler"));
        Assert.True(role.HasAccess("CreateUserHandler"));
    }

    [Fact]
    public void ReadOnlyUserRole_OnlyAllowsReadActions()
    {
        var role = new RoleEntity
        {
            Id = Guid.NewGuid(),
            Name = RoleNames.User,
            Description = "Registered user",
            Permissions = "GetByIdUserHandler,PaginatedUserHandler,LookupUserHandler"
        };

        Assert.True(role.HasAccess("GetByIdUserHandler"));
        Assert.True(role.HasAccess("PaginatedUserHandler"));
        Assert.True(role.HasAccess("LookupUserHandler"));
        Assert.False(role.HasAccess("CreateUserHandler"));
    }

    [Fact]
    public async Task RegisterUser_AssignsDefaultUserRole()
    {
        await using var context = CreateContext();
        var handler = new RegisterUserHandler(context);

        var result = await handler.HandleAsync(new RegisterUserRequestContract
        {
            Name = "Role User",
            Email = "role-user@example.com",
            Password = "P@ssw0rd123"
        });

        var user = await context.Users
            .Include(x => x.Role)
            .SingleAsync(x => x.Id == result.UserId);

        Assert.Equal(RoleNames.User, user.Role!.Name);
        Assert.True(user.Role.HasAccess("GetByIdUserHandler"));
    }

    private static TranslatorDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TranslatorDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TranslatorDbContext(options);
    }
}
