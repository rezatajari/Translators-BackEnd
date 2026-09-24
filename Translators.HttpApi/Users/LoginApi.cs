using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Translators.Application.Contracts.Users;
using Translators.Application.Security;
using Translators.Application.Users;
using Translators.EntityFrameworkCore.DbContexts;

namespace Translators.HttpApi.Users;

public class LoginApi : Endpoint<LoginUserRequestContract, LoginUserResponseContract>
{
    private readonly TranslatorDbContext _context;
    private readonly JwtTokenService _jwtTokenService;

    public LoginApi(TranslatorDbContext context, JwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    public override void Configure()
    {
        Post("/api/users/login");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Log in as an existing user";
            s.Description = "Validates the user email and password and returns a JWT token.";
        });
    }

    public override async Task HandleAsync(LoginUserRequestContract req, CancellationToken ct)
    {
        try
        {
            var handler = new LoginHandler(_context, _jwtTokenService);
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
