using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Translators.EntityFrameworkCore.DbContexts;

namespace Translators.Host;

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddAuthorization();
        builder.Services.AddFastEndpoints(options =>
        {
            options.Assemblies = new[]
            {
                typeof(Translators.HttpApi.Users.RegisterApi).Assembly
            };
        });
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Translators API", Version = "v1" });
        });

        if (builder.Environment.IsEnvironment("Testing"))
        {
            builder.Services.AddDbContext<TranslatorDbContext>(options =>
                options.UseInMemoryDatabase($"translators-tests-{Guid.NewGuid()}"));
        }
        else
        {
            builder.Services.AddDbContext<TranslatorDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
        }

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.UseFastEndpoints();

        app.Run();
    }
}
