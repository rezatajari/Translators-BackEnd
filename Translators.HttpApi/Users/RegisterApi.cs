using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Translators.Application.Contracts.Users;
using Translators.Application.Users;
using Translators.EntityFrameworkCore.DbContexts;

namespace Translators.HttpApi.Users;

public class RegisterApi : Endpoint<RegisterUserRequestContract, RegisterUserResponseContract>
{
    private readonly TranslatorDbContext _context;

    public RegisterApi(TranslatorDbContext context)
    {
        _context = context;
    }

    public override void Configure()
    {
        Post("/api/users/register");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Register a new user";
            s.Description = "Creates a new user account with email/password.";
        });
    }

    public override async Task HandleAsync(RegisterUserRequestContract req, CancellationToken ct)
    {
        try
        {
            var handler = new RegisterUserHandler(_context);
            var result = await handler.HandleAsync(req, ct);
            await SendOkAsync(result, ct);
        }
        catch (InvalidOperationException ex)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsJsonAsync(new { message = ex.Message }, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await HttpContext.Response.WriteAsJsonAsync(new { message = ex.Message }, cancellationToken: ct);
        }
    }
}
