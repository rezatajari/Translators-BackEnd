using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Translators.Application.Contracts.Users;
using Translators.Application.Users;
using Translators.EntityFrameworkCore.DbContexts;

namespace Translators.HttpApi.Users;

public class LookupUsersApi : Endpoint<LookupUserRequestContract, LookupUsersResponseContract>
{
    private readonly TranslatorDbContext _context;

    public LookupUsersApi(TranslatorDbContext context)
    {
        _context = context;
    }

    public override void Configure()
    {
        Get("/api/users/lookup");
        AllowAnonymous();
    }

    public override async Task HandleAsync(LookupUserRequestContract req, CancellationToken ct)
    {
        try
        {
            var handler = new LookupUserHandler(_context);
            var result = await handler.HandleAsync(req, ct);
            await SendOkAsync(result, ct);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await HttpContext.Response.WriteAsJsonAsync(new { message = ex.Message }, cancellationToken: ct);
        }
    }
}
