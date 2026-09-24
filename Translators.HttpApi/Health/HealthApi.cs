using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Translators.Application.Contracts.Health;
using Translators.Application.Health;

namespace Translators.HttpApi.Health;

public class HealthApi : EndpointWithoutRequest<HealthResponseContract>
{
    public override void Configure()
    {
        Get("/api/health");
        AuthSchemes("Bearer");
        Summary(s =>
        {
            s.Summary = "Check API health and JWT authorization";
            s.Description = "Requires a valid Bearer token. Returns the authenticated user when the token is valid.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var handler = new HealthHandler();
            var result = handler.Handle(User);
            await SendOkAsync(result, ct);
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await HttpContext.Response.WriteAsJsonAsync(new { message = ex.Message }, cancellationToken: ct);
        }
    }
}
