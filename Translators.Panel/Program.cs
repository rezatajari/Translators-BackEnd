using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Translators.GeneratedServices;
using Translators.Panel.Services;

namespace Translators.Panel;
public class Program
{
    private const int ApiPort = 5000;

    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        var panelUri = new Uri(builder.HostEnvironment.BaseAddress);
        var apiBaseAddress = new UriBuilder(panelUri.Scheme, panelUri.Host, ApiPort).Uri;

        builder.Services.AddScoped<AuthSession>();
        builder.Services.AddScoped<AuthenticationHeaderHandler>();
        builder.Services.AddScoped(sp =>
        {
            var handler = sp.GetRequiredService<AuthenticationHeaderHandler>();
            handler.InnerHandler = new HttpClientHandler();
            var httpClient = new HttpClient(handler) { BaseAddress = apiBaseAddress };
            return new Client(apiBaseAddress.ToString(), httpClient);
        });
        builder.Services.AddMudServices();

        await builder.Build().RunAsync();
    }
}
