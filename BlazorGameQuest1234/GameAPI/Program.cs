using Microsoft.EntityFrameworkCore;
using DataAccess.Data;
using GameAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

// Point d'entrée principal de l'API GameAPI - BlazorGameQuest v5.0
// Service backend REST pour la gestion des parties avec authentification Keycloak
var builder = WebApplication.CreateBuilder(args);

// Configuration de la base de données
builder.Services.AddDbContext<GameDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    // Utilise PostgreSQL si la chaîne de connexion est fournie, sinon InMemory pour les tests
    if (!string.IsNullOrEmpty(connectionString))
    {
        options.UseNpgsql(connectionString);
    }
    else
    {
        options.UseInMemoryDatabase("BlazorGameQuestDB");
    }
});

// Ajout des services au conteneur DI
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configuration pour éviter les références circulaires
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// Configuration CORS pour permettre l'accès depuis le client Blazor
builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration.GetSection("ApiSettings:AllowedOrigins").Get<string[]>() 
                        ?? new[] { "http://localhost:5215", "https://localhost:5216" };
    
    options.AddPolicy("AllowBlazorClient", builder =>
    {
        builder.WithOrigins(allowedOrigins)
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

// Configuration de l'authentification JWT avec Keycloak
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var authority = builder.Configuration["Keycloak:Authority"];
        options.Authority = authority;
        options.RequireHttpsMetadata = false; // Pour le développement
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = authority,
            ValidateAudience = false, // Keycloak gère l'audience
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Services métier pour la génération de donjons et la gestion des sessions
builder.Services.AddScoped<IDungeonGeneratorService, DungeonGeneratorService>();
builder.Services.AddScoped<IGameSessionService, GameSessionService>();

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "BlazorGameQuest API",
        Version = "v5.0",
        Description = "API REST pour le jeu BlazorGameQuest - Version 5 avec authentification Keycloak, microservices et déploiement Docker",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "BlazorGameQuest Team",
            Email = "admin@blazergamequest.com"
        }
    });
    
    // Configuration de l'authentification JWT Bearer pour Swagger
    // IMPORTANT : Utiliser SecuritySchemeType.Http avec Scheme = "bearer" pour que Swagger
    // envoie correctement le token dans le header "Authorization: Bearer <token>"
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header en utilisant le Bearer scheme. Entrez SEULEMENT le token (sans 'Bearer '). Exemple: 'eyJhbGciOi...'",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    
    // Inclure la documentation XML si elle existe
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Configuration CORS - doit être avant UseAuthentication/UseAuthorization
app.UseCors("AllowBlazorClient");

// Middleware d'authentification et autorisation
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    // Configuration OpenAPI pour l'environnement de développement
    app.MapOpenApi();
    
    // Interface Swagger pour tester l'API
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BlazorGameQuest API v3.0");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "BlazorGameQuest API Documentation";
        c.DisplayRequestDuration();
        c.EnableTryItOutByDefault();
    });
}

app.MapGet("/health", () => new { 
    Status = "Healthy", 
    Service = "GameAPI",
    Timestamp = DateTime.UtcNow 
})
.WithName("HealthCheck")
.WithTags("Health");

app.MapGet("/database-info", async (GameDbContext dbContext) =>
{
    try 
    {
        var userCount = await dbContext.Users.CountAsync();
        var playerCount = await dbContext.Players.CountAsync();
        var dungeonCount = await dbContext.Dungeons.CountAsync();
        var roomCount = await dbContext.Rooms.CountAsync();
        
        return Results.Ok(new {
            Status = "Database Connected",
            DatabaseProvider = dbContext.Database.ProviderName,
            Statistics = new {
                Users = userCount,
                Players = playerCount,
                Dungeons = dungeonCount,
                Rooms = roomCount
            },
            Timestamp = DateTime.UtcNow
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Database connection failed: {ex.Message}");
    }
})
.WithName("DatabaseInfo")
.WithTags("Health")
.WithSummary("Test de la connexion à la base de données")
.WithDescription("Vérifie la connexion à la base de données et retourne les statistiques des entités");

// Mapping des contrôleurs
app.MapControllers();

// Initialisation de la base de données avec des données de test
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<GameDbContext>();
    await DbInitializer.InitializeAsync(dbContext);
}

app.Run();
