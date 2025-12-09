// ApiGateway - Point d'entrée unique pour BlazorGameQuest v5.0
// Reverse proxy basé sur YARP pour router les requêtes vers GameAPI et BlazorClient
var builder = WebApplication.CreateBuilder(args);

// Configuration CORS : autorise les requêtes cross-origin depuis le client Blazor
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("http://localhost:5000", "http://localhost:5003")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

// Configuration YARP Reverse Proxy : charge les routes depuis appsettings.json
// Routes configurées : /api/* → GameAPI, /swagger/* → GameAPI, /* → BlazorClient
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// Middleware 1 : Logging des requêtes entrantes pour debug et monitoring
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation($"Request: {context.Request.Method} {context.Request.Path}");
    
    // Vérifier la présence du header Authorization (important pour API protégées)
    if (context.Request.Headers.ContainsKey("Authorization"))
    {
        logger.LogInformation("Authorization header présent");
    }
    else
    {
        logger.LogWarning("Authorization header manquant");
    }
    
    await next();
});

// Middleware 2 : Content Security Policy pour sécuriser Blazor WebAssembly
app.Use(async (context, next) =>
{
    // CSP optimisée pour Blazor WASM : 'unsafe-eval' et 'wasm-unsafe-eval' requis pour .NET WebAssembly
    // connect-src : autorise connexions à Keycloak pour authentification
    context.Response.Headers.Append("Content-Security-Policy", 
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-eval' 'unsafe-inline' 'wasm-unsafe-eval'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "connect-src 'self' http://localhost:8180 http://keycloak:8080; " +
        "img-src 'self' data:; " +
        "font-src 'self'; " +
        "frame-src 'none'");
    
    // Headers de sécurité supplémentaires pour isolation cross-origin
    context.Response.Headers.Append("Cross-Origin-Embedder-Policy", "require-corp");
    context.Response.Headers.Append("Cross-Origin-Opener-Policy", "same-origin");
    
    await next();
});

// Activer CORS pour permettre les requêtes cross-origin
app.UseCors("AllowBlazorClient");

// Activer le reverse proxy YARP : redirige automatiquement les requêtes selon les routes configurées
app.MapReverseProxy();

app.Run();
