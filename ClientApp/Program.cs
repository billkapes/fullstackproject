using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ClientApp;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress),
});

// register product service as scoped to match HttpClient lifetime in Blazor WASM
// Register a typed HttpClient for ProductService that targets the API server.
// Update the URI if your server runs on a different port or host.
// If AddHttpClient isn't available, register ProductService with a preconfigured HttpClient instance
builder.Services.AddScoped(sp => new ClientApp.Services.ProductService(
    new HttpClient { BaseAddress = new Uri("http://localhost:5015/") }
));

await builder.Build().RunAsync();
