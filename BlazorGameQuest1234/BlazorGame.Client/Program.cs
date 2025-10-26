/// <summary>
/// Point d'entrée principal de l'application Blazor WebAssembly
/// BlazorGameQuest - Client côté navigateur
/// </summary>
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorGame.Client;

// Création du builder pour l'application WebAssembly
var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Configuration des composants racine
builder.RootComponents.Add<App>("#app");        // Composant principal dans l'élément #app
builder.RootComponents.Add<HeadOutlet>("head::after");  // Gestion des éléments <head>

// Configuration des services DI
// HttpClient configuré avec l'adresse de base de l'application
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Démarrage de l'application Blazor WebAssembly
await builder.Build().RunAsync();
