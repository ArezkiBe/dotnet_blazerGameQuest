using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorApp;
using BlazorApp.Pages;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<Weather>();
builder.Services.AddScoped(sp => new HttpClient
{
    // Par exemple celle de notre Gateway
    BaseAddress = new Uri("http://localhost:5000/")
});

builder.Services.AddScoped<AuthenticatedHttpClient>();

builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<ITokenService, LocalStorageTokenService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

await builder.Build().RunAsync();
