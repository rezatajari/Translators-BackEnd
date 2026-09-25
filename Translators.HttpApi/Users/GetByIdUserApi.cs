using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Translators.Application.Contracts.Users;
using Translators.Application.Users;
using Translators.EntityFrameworkCore.DbContexts;

namespace Translators.HttpApi.Users;

public class GetByIdUserApi : Endpoint<GetByIdUserRequestContract, GetByIdUserResponseContract>
{
    private readonly TranslatorDbContext _context;

    public GetByIdUserApi(TranslatorDbContext context)
    {
        _context = context;
    }

    public override void Configure()
    {
        Get("/api/users/{UserId}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetByIdUserRequestContract req, CancellationToken ct)
    {
        try
        {
            var handler = new GetByIdUserHandler(_context);
            var result = await handler.HandleAsync(req.UserId, ct);
            await SendOkAsync(result, ct);
        }
        catch (InvalidOperationException ex)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            await HttpContext.Response.WriteAsJsonAsync(new { message = ex.Message }, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await HttpContext.Response.WriteAsJsonAsync(new { message = ex.Message }, cancellationToken: ct);
        }
    }
}
