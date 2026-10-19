using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Translators.Application.Contracts.Users;
using Translators.Application.Users;
using Translators.EntityFrameworkCore.DbContexts;

namespace Translators.HttpApi.Users;

public class PaginatedUsersApi : Endpoint<PaginatedUsersRequestContract, PaginatedUsersResponseContract>
{
    private readonly TranslatorDbContext _context;

    public PaginatedUsersApi(TranslatorDbContext context)
    {
        _context = context;
    }

    public override void Configure()
    {
        Post("/api/users/Paginated");
        AllowAnonymous();
    }

    public override async Task HandleAsync(PaginatedUsersRequestContract req, CancellationToken ct)
    {
        try
        {
            var handler = new PaginatedUserHandler(_context);
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
