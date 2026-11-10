using InventoryHub.Client;
using InventoryHub.Client.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7080/"),
    Timeout = TimeSpan.FromSeconds(20)
});

builder.Services.AddScoped<IProductApiClient, ProductApiClient>();
builder.Services.AddScoped<AppState>();

await builder.Build().RunAsync();