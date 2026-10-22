using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Translators.Application.Contracts.Security;
using Translators.Application.Contracts.Users;
using Translators.Application.Security;
using Translators.Application.Users;
using Translators.EntityFrameworkCore.DbContexts;

namespace Translators.HttpApi.Users;

public class GoogleLoginApi : Endpoint<GoogleIdentity, LoginUserResponseContract>
{
    private readonly TranslatorDbContext _context;
    private readonly JwtTokenService _jwtTokenService;

    public GoogleLoginApi(
        TranslatorDbContext context,
        JwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    public override void Configure()
    {
        Post("/api/users/google-login");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Log in or register using Google";
            s.Description = "Creates a user if needed and returns a JWT token.";
        });
    }

    public override async Task HandleAsync(
        GoogleIdentity req,
        CancellationToken ct)
    {
        try
        {
            var handler = new GoogleLoginHandler(
                _context,
                _jwtTokenService);

            var result = await handler.HandleAsync(req, ct);
            await SendOkAsync(result, ct);
        }
        catch (InvalidOperationException ex)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsJsonAsync(
                new { message = ex.Message },
                cancellationToken: ct);
        }
    }
}