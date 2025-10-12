/// <summary>
/// Point d'entrée principal de l'API AuthenticationServices pour BlazorGameQuest
/// Gère l'authentification et les services utilisateur
/// </summary>
var builder = WebApplication.CreateBuilder(args);

// Ajout des services au conteneur DI
// Documentation OpenAPI disponible sur : https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Ajout des services Swagger pour la documentation API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

/// <summary>
/// Configuration du pipeline de requêtes HTTP
/// </summary>
if (app.Environment.IsDevelopment())
{
    // Configuration OpenAPI pour l'environnement de développement
    app.MapOpenApi();
    
    // Interface Swagger pour tester l'API
    app.UseSwagger();
    app.UseSwaggerUI();
}

/// <summary>
/// Endpoint de vérification de santé de l'API
/// Retourne le statut du service avec un timestamp
/// </summary>
app.MapGet("/health", () => new { 
    Status = "Healthy", 
    Service = "AuthenticationServices",
    Timestamp = DateTime.UtcNow 
})
.WithName("HealthCheck")
.WithTags("Health");

// TODO Version 2 : Les endpoints d'authentification seront ajoutés ici
// - Inscription utilisateur
// - Connexion/Déconnexion
// - Gestion des sessions
// - Validation des tokens

/// <summary>
/// Démarrage de l'application web
/// </summary>
app.Run();
