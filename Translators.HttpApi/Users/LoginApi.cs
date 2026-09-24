using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Translators.Application.Contracts.Users;
using Translators.Application.Users;
using Translators.EntityFrameworkCore.DbContexts;

namespace Translators.HttpApi.Users;

public class LoginApi : Endpoint<LoginUserRequestContract, LoginUserResponseContract>
{
    private readonly TranslatorDbContext _context;

    public LoginApi(TranslatorDbContext context)
    {
        _context = context;
    }

    public override void Configure()
    {
        Post("/api/users/login");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Log in as an existing user";
            s.Description = "Validates the user email and password.";
        });
    }

    public override async Task HandleAsync(LoginUserRequestContract req, CancellationToken ct)
    {
        try
        {
            var handler = new LoginHandler(_context);
            var result = await handler.HandleAsync(req, ct);
            await SendOkAsync(result, ct);
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await HttpContext.Response.WriteAsJsonAsync(new { message = ex.Message }, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await HttpContext.Response.WriteAsJsonAsync(new { message = ex.Message }, cancellationToken: ct);
        }
    }
}
