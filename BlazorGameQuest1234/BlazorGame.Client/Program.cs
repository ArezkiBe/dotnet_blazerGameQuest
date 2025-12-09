/// <summary>
/// Point d'entrée principal de l'application Blazor WebAssembly
/// BlazorGameQuest - Client côté navigateur avec authentification Keycloak
/// </summary>
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorGame.Client;
using BlazorGame.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;

// Création du builder pour l'application WebAssembly
var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Configuration des composants racine
builder.RootComponents.Add<App>("#app");        // Composant principal dans l'élément #app
builder.RootComponents.Add<HeadOutlet>("head::after");  // Gestion des éléments <head>

// Configuration des services DI
// HttpClient configuré pour communiquer avec l'API via la Gateway
// Utilise localhost:5000 comme dans TP7
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri("http://localhost:5000/") // URL de la Gateway
});

// Services de jeu
builder.Services.AddScoped<IGameApiService, GameApiService>();

// Services d'authentification Keycloak (remplace UserContextService)
builder.Services.AddScoped<ITokenService, LocalStorageTokenService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticatedHttpClient>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();

// Ajout de l'autorisation
builder.Services.AddAuthorizationCore();

// Démarrage de l'application Blazor WebAssembly
await builder.Build().RunAsync();
