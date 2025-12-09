# 🔐 Guide d'Authentification Swagger

## Comment tester les API avec Swagger

### Méthode 1 : Via Keycloak directement (Recommandé pour les tests)

1. **Obtenir un token via l'API de Keycloak**

```bash
# Pour un utilisateur joueur (user1)
curl -X POST "http://localhost:8080/realms/blazor-gamequest/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=blazor-client" \
  -d "username=user1" \
  -d "password=user1" \
  -d "grant_type=password"

# Pour un administrateur (admin)
curl -X POST "http://localhost:8180/realms/blazor-gamequest/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=blazor-client" \
  -d "username=admin" \
  -d "password=admin" \
  -d "grant_type=password"
```

2. **Copier l'access_token** du JSON retourné

```json
{
  "access_token": "eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICI...",
  "expires_in": 300,
  "refresh_expires_in": 1800,
  "refresh_token": "eyJhbGciOiJIUzI1NiIsInR5cCIgOiAiSldU...",
  "token_type": "Bearer"
}
```

3. **Dans Swagger UI (http://localhost:5001/swagger)**
   - Cliquer sur le bouton **"Authorize"** 🔒 (en haut à droite)
   - Dans le champ "Value", entrer : `Bearer VOTRE_ACCESS_TOKEN_ICI`
   - Cliquer sur "Authorize"
   - Cliquer sur "Close"

4. **Tester les endpoints**
   - Les requêtes incluront automatiquement l'en-tête `Authorization: Bearer ...`
   - Tester les endpoints protégés comme `POST /api/players`

### Méthode 2 : Via PowerShell (Windows)

```powershell
# Utilisateur joueur
$response = Invoke-RestMethod -Uri "http://localhost:8080/realms/blazor-gamequest/protocol/openid-connect/token" `
  -Method Post `
  -ContentType "application/x-www-form-urlencoded" `
  -Body @{
    client_id="blazor-client"
    username="user1"
    password="user1"
    grant_type="password"
  }

# Afficher le token
$response.access_token

# Copier dans le presse-papiers
$response.access_token | Set-Clipboard
Write-Host "Token copié dans le presse-papiers!"
```

```powershell
# Administrateur
$response = Invoke-RestMethod -Uri "http://localhost:8080/realms/blazor-gamequest/protocol/openid-connect/token" `
  -Method Post `
  -ContentType "application/x-www-form-urlencoded" `
  -Body @{
    client_id="blazor-client"
    username="admin"
    password="admin"
    grant_type="password"
  }

$response.access_token | Set-Clipboard
Write-Host "Token admin copié!"
```

### Méthode 3 : Via l'application Blazor (Méthode visuelle)

1. **Se connecter à l'application** : http://localhost:5000
2. **Ouvrir les DevTools du navigateur** (F12)
3. **Aller dans l'onglet "Application" ou "Storage"**
4. **Rechercher dans "Local Storage" → `http://localhost:5000`**
5. **Copier la valeur de la clé `authToken`**
6. **Utiliser ce token dans Swagger** (ajouter `Bearer ` devant)

### Méthode 4 : Via Postman (Alternative à Swagger)

1. Créer une nouvelle requête POST
2. URL : `http://localhost:8080/realms/blazor-gamequest/protocol/openid-connect/token`
3. Body → x-www-form-urlencoded :
   - `client_id` = `blazor-client`
   - `username` = `admin` (ou `user1`)
   - `password` = `admin` (ou `user1`)
   - `grant_type` = `password`
4. Envoyer → Copier `access_token`

## 🧪 Test Rapide des Endpoints

### Endpoints publics (pas besoin de token)
```http
GET http://localhost:5001/api/players
GET http://localhost:5001/api/dungeons
```

### Endpoints admin (nécessitent token administrateur)
```http
POST http://localhost:5001/api/players
PUT http://localhost:5001/api/players/1
DELETE http://localhost:5001/api/players/1
GET http://localhost:5001/api/users
```

## 📋 Comptes de Test

| Username | Password | Rôle | Description |
|----------|----------|------|-------------|
| `user1` | `user1` | joueur | Compte joueur standard |
| `user2` | `user2` | joueur | Compte joueur standard |
| `admin` | `admin` | administrateur | Accès complet |

## 🔍 Vérifier le contenu d'un Token JWT

Pour décoder et voir les claims du token (utile pour vérifier les rôles) :

1. Aller sur https://jwt.io
2. Coller le token dans la zone "Encoded"
3. Vérifier les claims dans "Payload" :
   ```json
   {
     "sub": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
     "preferred_username": "admin",
     "realm_access": {
       "roles": ["administrateur"]
     }
   }
   ```

## 🚀 Script Complet PowerShell (Pour le Prof)

Créer un fichier `get-token.ps1` :

```powershell
param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("user1", "user2", "admin")]
    [string]$User = "admin"
)

Write-Host "🔐 Obtention du token JWT pour : $User" -ForegroundColor Cyan

$body = @{
    client_id = "blazor-client"
    username = $User
    password = $User
    grant_type = "password"
}

try {
    $response = Invoke-RestMethod -Uri "http://localhost:8080/realms/blazor-gamequest/protocol/openid-connect/token" `
        -Method Post `
        -ContentType "application/x-www-form-urlencoded" `
        -Body $body

    $token = $response.access_token
    $token | Set-Clipboard
    
    Write-Host "✅ Token obtenu avec succès!" -ForegroundColor Green
    Write-Host "📋 Token copié dans le presse-papiers" -ForegroundColor Green
    Write-Host ""
    Write-Host "🔗 Pour Swagger UI:" -ForegroundColor Yellow
    Write-Host "   1. Ouvrir http://localhost:5001/swagger" -ForegroundColor White
    Write-Host "   2. Cliquer sur 'Authorize' 🔒" -ForegroundColor White
    Write-Host "   3. Entrer: Bearer <Ctrl+V pour coller>" -ForegroundColor White
    Write-Host ""
    Write-Host "Token (30 premiers caractères):" -ForegroundColor Gray
    Write-Host $token.Substring(0, 30)... -ForegroundColor DarkGray
    
} catch {
    Write-Host "❌ Erreur lors de l'obtention du token" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}
```

**Utilisation :**
```powershell
# Pour admin
.\get-token.ps1 -User admin

# Pour user1
.\get-token.ps1 -User user1
```

## 📝 Notes Importantes

- Les tokens expirent après **5 minutes** (300 secondes)
- Si vous obtenez une erreur 401, redemander un nouveau token
- Le token admin contient le rôle `administrateur`
- Le token user contient le rôle `joueur`
- Keycloak doit être démarré (http://localhost:8080)
