# Test API avec Swagger - BlazorGameQuest

## Configuration Swagger Corrigée ✅

Le schéma d'authentification a été corrigé dans `GameAPI/Program.cs`:

```csharp
// ✅ Maintenant
Type = SecuritySchemeType.Http,
Scheme = "bearer",
BearerFormat = "JWT"
```

Swagger ajoute automatiquement "Bearer " au token.

## Obtenir un Token JWT

### Méthode 1: Script PowerShell (Recommandé)

```powershell
.\scripts\get-token-simple.ps1 admin   # Pour admin
.\scripts\get-token-simple.ps1 user    # Pour user1
```

Le token est copié automatiquement dans le presse-papiers.

### Méthode 2: PowerShell Manuel (Windows)

```powershell
# Admin
$response = Invoke-RestMethod -Uri "http://localhost:8180/realms/blazor-gamequest/protocol/openid-connect/token" `
  -Method Post -ContentType "application/x-www-form-urlencoded" `
  -Body @{client_id="blazor-client"; username="admin"; password="admin"; grant_type="password"}
$response.access_token | Set-Clipboard

# User1
$response = Invoke-RestMethod -Uri "http://localhost:8180/realms/blazor-gamequest/protocol/openid-connect/token" `
  -Method Post -ContentType "application/x-www-form-urlencoded" `
  -Body @{client_id="blazor-client"; username="user1"; password="1234"; grant_type="password"}
$response.access_token | Set-Clipboard
```

### Méthode 2b: Bash/curl (Mac/Linux/WSL)

```bash
# Admin
TOKEN=$(curl -s -X POST "http://localhost:8180/realms/blazor-gamequest/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=blazor-client" \
  -d "username=admin" \
  -d "password=admin" \
  -d "grant_type=password" | grep -o '"access_token":"[^"]*"' | cut -d'"' -f4)

# Copier dans le presse-papiers
echo $TOKEN | pbcopy          # macOS
echo $TOKEN | xclip -sel clip # Linux (installer: sudo apt install xclip)
echo $TOKEN | clip.exe        # WSL

# User1
TOKEN=$(curl -s -X POST "http://localhost:8180/realms/blazor-gamequest/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=blazor-client" \
  -d "username=user1" \
  -d "password=1234" \
  -d "grant_type=password" | grep -o '"access_token":"[^"]*"' | cut -d'"' -f4)

echo $TOKEN | pbcopy  # ou xclip/clip.exe
```

## Utiliser dans Swagger

1. Ouvrir http://localhost:5000/swagger
2. Cliquer **Authorize** 🔒
3. Coller le token (**sans** "Bearer ")
4. Cliquer "Authorize" → "Close"
5. Tester les endpoints

## Comptes

| Username | Password | Rôle | Accès |
|----------|----------|------|-------|
| user1 | 1234 | joueur | Endpoints publics |
| user2 | 1234 | joueur | Endpoints publics |
| admin | admin | administrateur | Tous endpoints |

## Endpoints Disponibles

### Publics (authentification requise)
```http
GET  /api/players
GET  /api/dungeons
POST /api/game/start-adventure
POST /api/game/session/{id}/action
```

### Admin uniquement
```http
POST   /api/players
PUT    /api/players/{id}
DELETE /api/players/{id}
GET    /api/users
GET    /api/users/dashboard-stats
```

## Notes

- **Expiration:** 5 minutes (300 secondes)
- **Erreur 401:** Token expiré → récupérer nouveau token
- **Accès:** Tout passe par le Gateway (port 5000)
- **Keycloak:** http://localhost:8180

## Dépannage

**401 Unauthorized:** Token expiré ou invalide  
**403 Forbidden:** Rôle insuffisant (endpoint admin)  
**Keycloak inaccessible:** Vérifier `docker-compose ps`
