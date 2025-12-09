# 🎮 BlazorGameQuest

**Arezki BEGGAR, Ilyas HADJADJ** - EFREI Paris S7 .NET - 2024

## Description

Jeu d'aventure RPG en Blazor WebAssembly avec architecture microservices, authentification Keycloak et API Gateway YARP.

**Fonctionnalités:** Donjons procéduraux, combats tour par tour, système RPG (HP/Mana/XP), classement, gestion des rôles.

## Architecture

```
Client (localhost:5000) → ApiGateway (YARP) → GameAPI + BlazorClient
                                ↓
                            Keycloak (8180)
                            PostgreSQL/InMemory
```

**Composants:**
- **ApiGateway** (Port 5000 HTTP) - Point d'entrée unique
- **GameAPI** (Port interne 5001) - API REST + EF Core
- **BlazorGame.Client** (Port interne 80) - Frontend WebAssembly
- **Keycloak** (Port 8180) - Authentification OpenID Connect

## 🚀 Démarrage Rapide

### Prérequis
- Docker Desktop
- Ports disponibles: 5000, 8180

### Installation

```powershell
cd BlazorGameQuest1234
docker-compose up -d
```

### Configuration Keycloak (première utilisation)

1. Accéder à http://localhost:8180
2. Se connecter avec **admin/admin**
3. Créer le realm **blazor-gamequest**
4. Créer le client **blazor-client**
5. Créer les utilisateurs (voir tableau ci-dessous)

**Voir détails:** [KEYCLOAK_SETUP.md](BlazorGameQuest1234/KEYCLOAK_SETUP.md)

### Comptes de test

| Utilisateur | Mot de passe | Rôle | Accès |
|-------------|--------------|------|-------|
| user1 | 1234 | joueur | Jeu uniquement |
| user2 | 1234 | joueur | Jeu uniquement |
| admin | admin | administrateur | Dashboard + Gestion complète |

### Accès

- **Application:** http://localhost:5000 (seul point d'entrée)
- **Keycloak Admin:** http://localhost:8180
- **Swagger API:** http://localhost:5000/swagger

## 🎮 Utilisation

1. Ouvrir http://localhost:5000
2. Se connecter avec un compte (user1/1234 ou admin/admin)
3. "Nouvelle aventure" → Choisir difficulté
4. Explorer le donjon (5 salles)
5. Actions: Attaquer, Fuir, Fouiller, Se reposer

## 🔐 Tester les API

### Obtenir un token JWT

```powershell
# Via script PowerShell
.\scripts\get-token-simple.ps1 admin

# Ou manuellement
$response = Invoke-RestMethod -Uri "http://localhost:8180/realms/blazor-gamequest/protocol/openid-connect/token" `
  -Method Post -ContentType "application/x-www-form-urlencoded" `
  -Body @{client_id="blazor-client"; username="admin"; password="admin"; grant_type="password"}
$response.access_token | Set-Clipboard
```

### Utiliser dans Swagger

1. Ouvrir http://localhost:5000/swagger
2. Cliquer **Authorize** 🔒
3. Coller le token (sans "Bearer ")
4. Tester les endpoints

**Voir détails:** [SWAGGER_AUTHENTICATION_GUIDE.md](BlazorGameQuest1234/SWAGGER_AUTHENTICATION_GUIDE.md)

## 🧪 Tests

```powershell
cd BlazorGameQuest1234
dotnet test
# 49 tests unitaires - Couverture 40%
```

## 📚 Technologies

- **.NET 9** - Framework
- **Blazor WebAssembly** - Frontend
- **ASP.NET Core Web API** - Backend
- **YARP** - API Gateway
- **Keycloak 21.1.1** - Authentification
- **EF Core 9** - ORM
- **PostgreSQL/InMemory** - Base de données
- **xUnit** - Tests
- **Docker** - Conteneurisation

## 🛠️ Commandes Docker

```powershell
# Démarrer
docker-compose up -d

# Arrêter
docker-compose down

# Logs
docker-compose logs -f

# Rebuild
docker-compose up --build -d
```

## 🐛 Dépannage

**Erreur 401 sur Swagger:** Token expiré (5 min) → Récupérer nouveau token  
**Keycloak inaccessible:** Attendre 1-2 min après démarrage  
**Port 5000 occupé:** Vérifier avec `Get-NetTCPConnection -LocalPort 5000`

## 📖 Documentation

- [KEYCLOAK_SETUP.md](BlazorGameQuest1234/KEYCLOAK_SETUP.md) - Configuration Keycloak complète
- [DOCKER_DEPLOYMENT.md](BlazorGameQuest1234/DOCKER_DEPLOYMENT.md) - Déploiement Docker détaillé
- [SWAGGER_AUTHENTICATION_GUIDE.md](BlazorGameQuest1234/SWAGGER_AUTHENTICATION_GUIDE.md) - Test API avec JWT

## ✅ Conformité Projet

- ✅ 2 joueurs (user1/user2) + 1 admin avec bons rôles
- ✅ Gateway port 5000 HTTP uniquement
- ✅ Authentification Keycloak + JWT
- ✅ Lien base de données via KeycloakUserId
- ✅ Tests unitaires + Couverture
- ✅ Docker Compose fonctionnel

---

**Version 5.0** - Microservices + Keycloak + Docker + API Gateway
