/// <summary>
/// Point d'entrée principal de l'application Blazor WebAssembly
/// BlazorGameQuest - Client côté navigateur
/// </summary>
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorGame.Client;
using BlazorGame.Client.Services;

// Création du builder pour l'application WebAssembly
var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Configuration des composants racine
builder.RootComponents.Add<App>("#app");        // Composant principal dans l'élément #app
builder.RootComponents.Add<HeadOutlet>("head::after");  // Gestion des éléments <head>

// Configuration des services DI
// HttpClient configuré pour communiquer avec l'API GameAPI
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri("http://localhost:5215/") // URL de l'API
});

// Services de jeu
builder.Services.AddScoped<IGameApiService, GameApiService>();

// Démarrage de l'application Blazor WebAssembly
await builder.Build().RunAsync();
