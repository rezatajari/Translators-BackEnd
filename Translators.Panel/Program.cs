using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Translators.Panel.Services;

namespace Translators.Panel;
public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        var panelUri = new Uri(builder.HostEnvironment.BaseAddress);
        builder.Services.AddScoped(sp => new HttpClient
        {
            BaseAddress = new UriBuilder(panelUri.Scheme, panelUri.Host, 5000).Uri
        });
        builder.Services.AddScoped<AuthSession>();
        builder.Services.AddScoped<TranslatorsApiClient>();
        builder.Services.AddMudServices();

        await builder.Build().RunAsync();
    }
}
