# 🌐 ApiGateway - Reverse Proxy YARP

## 📋 Description

ApiGateway est le **point d'entrée unique** de l'architecture microservices BlazorGameQuest v5.0. Il utilise **YARP (Yet Another Reverse Proxy)** de Microsoft pour router intelligemment les requêtes vers les services appropriés.

## 🎯 Rôle et Responsabilités

### Fonctionnalités principales :
1. **Reverse Proxy** : Redirige les requêtes vers GameAPI ou BlazorClient
2. **CORS Management** : Gère les politiques cross-origin pour le client web
3. **Security Headers** : Applique les CSP (Content Security Policy) pour Blazor WASM
4. **Request Logging** : Trace toutes les requêtes entrantes pour monitoring
5. **Header Forwarding** : Propage les headers d'authentification vers les services backend

## 🔧 Architecture de Routage

```
┌─────────────────┐
│   Client Web    │
│  (Navigateur)   │
└────────┬────────┘
         │ http://localhost:5003
         ▼
┌─────────────────────────┐
│     ApiGateway          │
│   (Port 5003/8080)      │
│   ┌─────────────────┐   │
│   │  YARP Proxy     │   │
│   │  ┌───────────┐  │   │
│   │  │  Routes   │  │   │
│   │  └─────┬─────┘  │   │
│   └────────┼────────┘   │
└────────────┼────────────┘
             │
        ┌────┴────┐
        ▼         ▼
   ┌────────┐  ┌─────────┐
   │GameAPI │  │ Blazor  │
   │ :5001  │  │Client:80│
   └────────┘  └─────────┘
```

## 📍 Configuration des Routes

### Routes définies dans `appsettings.json` :

| Pattern | Destination | Ordre | Description |
|---------|-------------|-------|-------------|
| `/api/{**catch-all}` | GameAPI:5001 | 1 | Toutes les API REST |
| `/swagger/{**catch-all}` | GameAPI:5001 | 2 | Documentation Swagger |
| `/openapi/{**catch-all}` | GameAPI:5001 | 3 | Schéma OpenAPI |
| `/{**catch-all}` | BlazorClient:80 | 1000 | Application Blazor (fallback) |

### Logique de routage :
- Les routes sont évaluées par **ordre croissant**
- La route avec le **catch-all** (`/{**catch-all}`) a l'ordre le plus élevé (1000) pour servir de **fallback**
- Les routes API sont prioritaires pour éviter les conflits

## 🔐 Sécurité

### Content Security Policy (CSP)
```csharp
default-src 'self';                    // Ressources par défaut : même origine
script-src 'self' 'unsafe-eval'        // Scripts : requis pour Blazor WASM
           'unsafe-inline' 
           'wasm-unsafe-eval';
style-src 'self' 'unsafe-inline';      // Styles inline autorisés
connect-src 'self'                     // Connexions externes : Keycloak
            http://localhost:8180 
            http://keycloak:8080;
img-src 'self' data:;                  // Images : même origine + data URIs
font-src 'self';                       // Polices : même origine
frame-src 'none';                      // Pas d'iframes
```

### Headers de sécurité supplémentaires :
- **Cross-Origin-Embedder-Policy** : `require-corp` (isolation des ressources)
- **Cross-Origin-Opener-Policy** : `same-origin` (isolation du contexte de navigation)

## 🚀 Utilisation

### Développement local :
```bash
cd ApiGateway
dotnet run
```

L'API Gateway sera accessible sur : **http://localhost:5003**

### Docker :
```bash
docker-compose up apigateway
```

## 📊 Monitoring et Logging

Le middleware de logging trace automatiquement :
- Méthode HTTP et chemin de chaque requête
- Présence du header `Authorization`
- Niveau de log : `Information` (succès) ou `Warning` (header manquant)

### Exemple de logs :
```
info: Program[0]
      Request: GET /api/players
info: Program[0]
      Authorization header présent

info: Program[0]
      Request: POST /api/game/start-adventure
warn: Program[0]
      Authorization header manquant
```

## 🧪 Tests

### Tester le routage :

```powershell
# Requête vers GameAPI (via gateway)
Invoke-RestMethod -Uri "http://localhost:5003/api/players"

# Requête vers Swagger (via gateway)
Start-Process "http://localhost:5003/swagger"

# Requête vers BlazorClient (via gateway)
Start-Process "http://localhost:5003"
```

### Vérifier les headers de sécurité :

```powershell
$response = Invoke-WebRequest -Uri "http://localhost:5003" -Method Get
$response.Headers
```

## 📦 Dépendances

- **Yarp.ReverseProxy** : Reverse proxy de Microsoft
- **.NET 9.0** : Framework de base
- **Microsoft.Extensions.Logging** : Logging intégré

## 🔗 Liens Utiles

- [Documentation YARP](https://microsoft.github.io/reverse-proxy/)
- [Blazor WASM Security](https://learn.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/)
- [Content Security Policy](https://developer.mozilla.org/en-US/docs/Web/HTTP/CSP)

## 📝 Notes de Design

### Pourquoi YARP ?
- **Performant** : Optimisé pour .NET (meilleur que Nginx pour .NET)
- **Type-safe** : Configuration en C# avec IntelliSense
- **Flexible** : Facile à étendre avec middlewares personnalisés
- **Native .NET** : Intégration parfaite avec ASP.NET Core

### Pourquoi pas Ocelot ?
- YARP est plus récent et mieux maintenu par Microsoft
- Meilleure performance (jusqu'à 2x plus rapide)
- Support natif de .NET 9.0
- Documentation officielle Microsoft

## ✅ Checklist de Conformité

- ✅ **Nommage limpide** : Variables et méthodes claires
- ✅ **Code court** : Program.cs simple et lisible (60 lignes)
- ✅ **Algorithmes simples** : Routing déclaratif via configuration
- ✅ **Bien commenté** : Chaque middleware expliqué
- ✅ **Pas de fichiers inutiles** : Structure minimaliste (Program.cs + appsettings + Dockerfile)
- ✅ **Design accessible** : Configuration JSON facile à comprendre et modifier
