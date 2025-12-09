# BlazorGameQuest - Projet .NET

**Membres du groupe :** Arezki BEGGAR, Ilyas HADJADJ  
**EFREI Paris - Cours .NET - Semestre 7**

## 📋 Description

BlazorGameQuest est un jeu d'aventure Blazor WebAssembly avec API .NET 9. Le joueur explore des donjons générés aléatoirement avec système RPG complet (HP, XP, niveaux) et authentification Keycloak sécurisée.

## 🏗️ Architecture du Projet

```
BlazorGameQuest1234/
├── BlazorGame.Client/          # Frontend Blazor WebAssembly (port 5003)
├── GameAPI/                    # API REST backend (port 5001)
├── ApiGateway/                 # YARP Gateway (port 5000)
├── SharedModels/               # DTOs et modèles partagés
├── DataAccess/                 # Entity Framework Core + Migrations
└── BlazorGameQuest.Tests/      # 35 tests unitaires (xUnit)
```

## 🚀 Évolution du Projet (5 Versions)

### Version 1 - Frontend Blazor de Base
✅ Pages Blazor avec génération aléatoire de donjons  
✅ Interface utilisateur médiévale responsive  
✅ Navigation entre pages (Home, About, Game)

### Version 2 - API REST et Intégration
✅ API REST ASP.NET Core avec endpoints CRUD  
✅ Modèles de données (Users, Players, Dungeons, Rooms)  
✅ Intégration Frontend-Backend via HttpClient  
✅ Documentation Swagger/OpenAPI

### Version 3 - Base de Données et Persistance
✅ Entity Framework Core avec PostgreSQL/InMemory  
✅ 6 migrations de base de données appliquées  
✅ Persistance des scores et statistiques joueurs  
✅ Système de classement (Leaderboard)

### Version 4 - Tests Unitaires
✅ 35 tests unitaires avec xUnit  
✅ Tests des services (DungeonGenerator, GameSession)  
✅ Tests des contrôleurs (Users, Players, Dungeons, Rooms)  
✅ Couverture complète des fonctionnalités critiques

### Version 5 - Sécurité et Déploiement ⭐ (Version Actuelle)
✅ Authentification Keycloak (OpenID Connect + JWT Bearer)  
✅ Gestion des rôles (`joueur`, `administrateur`)  
✅ YARP API Gateway pour centraliser les appels  
✅ Docker et Docker Compose pour déploiement conteneurisé  
✅ Sécurisation des endpoints admin avec `[Authorize(Roles = "administrateur")]`  
✅ Lien automatique Keycloak ↔ Base de données (`KeycloakUserId`)

## 🛠️ Technologies Utilisées

- **.NET 9** - Framework principal
- **Blazor WebAssembly** - Frontend SPA
- **ASP.NET Core Web API** - Backend REST
- **Entity Framework Core 9** - ORM
- **PostgreSQL** - Base de données production
- **Keycloak 21.1.1** - Authentification/Autorisation
- **YARP** - API Gateway reverse proxy
- **Docker & Docker Compose** - Conteneurisation
- **Swagger/OpenAPI** - Documentation API avec authentification JWT
- **xUnit** - Tests unitaires

## 🎮 Fonctionnalités du Jeu

- **Génération procédurale** : Donjons de 5 salles avec défis variés
- **Système RPG complet** : HP, XP, niveaux, force, défense
- **Actions tactiques** : Combat (⚔️), Fuite (🏃), Fouille (🔍), Repos (💤), Investigation (🔦)
- **Trois difficultés** : Facile, Normal, Difficile
- **Calcul de score** : Points basés sur performance, difficulté, survie
- **Classement global** : Top joueurs accessible à tous les utilisateurs authentifiés
- **Dashboard admin** : Gestion des joueurs, statistiques, exports

## 🐳 Déploiement avec Docker (Recommandé)

### Prérequis
- Docker Desktop installé
- .NET 9 SDK (optionnel, pour développement local)

### Lancement rapide

```powershell
# Se placer dans le dossier du projet
cd BlazorGameQuest1234

# Démarrer tous les services
docker-compose up -d

# Vérifier que les conteneurs tournent
docker ps
```

### Services disponibles

- **Application** : http://localhost:5000
- **Keycloak Admin** : http://localhost:8080 (admin/admin)
- **API Gateway** : http://localhost:5000/api/*
- **GameAPI Swagger** : http://localhost:5001/swagger
- **PostgreSQL** : localhost:5432 (gamequest_db/P@ssw0rd)

### Comptes de test Keycloak

| Utilisateur | Mot de passe | Rôle |
|-------------|--------------|------|
| user1 | user1 | joueur |
| user2 | user2 | joueur |
| admin | admin | administrateur |

### Arrêt des services

```powershell
docker-compose down
```

## 💻 Développement Local (sans Docker)

### Prérequis
- .NET 9 SDK
- PostgreSQL installé localement (ou utiliser InMemory)

### Lancement manuel

```powershell
# Terminal 1 - API GameAPI
cd BlazorGameQuest1234/GameAPI
dotnet run

# Terminal 2 - API Gateway
cd BlazorGameQuest1234/ApiGateway
dotnet run

# Terminal 3 - Client Blazor
cd BlazorGameQuest1234/BlazorGame.Client
dotnet run
```

**Ports en développement :**
- Client : http://localhost:5003
- Gateway : http://localhost:5000
- GameAPI : http://localhost:5001

## 🧪 Tests Unitaires

```powershell
# Exécuter tous les tests (35 tests)
cd BlazorGameQuest1234
dotnet test

# Avec couverture de code
dotnet test --collect:"XPlat Code Coverage"

# Tests avec détails
dotnet test --logger "console;verbosity=detailed"
```

**Couverture :** Services (DungeonGenerator, GameSession), Contrôleurs (Users, Players, Dungeons, Rooms), Intégration

## 🔐 Sécurité et Authentification

### Architecture de sécurité
1. **Keycloak** : Gestion des utilisateurs et génération de JWT
2. **API Gateway** : Validation des tokens JWT entrants
3. **GameAPI** : Autorisation basée sur les rôles via `[Authorize]`
4. **Lien BD** : Champ `KeycloakUserId` dans table Players (auto-rempli depuis JWT `sub` claim)

### Endpoints sécurisés (admin uniquement)
- `POST/PUT/DELETE /api/players` - Gestion joueurs
- `POST/PUT/DELETE /api/users` - Gestion utilisateurs
- `POST/PUT/DELETE /api/dungeons` - Gestion donjons
- `GET /api/users/dashboard-stats` - Statistiques globales

### Test Swagger avec authentification

1. Ouvrir http://localhost:5001/swagger
2. Cliquer sur **"Authorize"** 🔒
3. Obtenir un token JWT depuis Keycloak ou depuis l'application
4. Entrer : `Bearer VOTRE_TOKEN_ICI`
5. Tester les endpoints protégés

## 📚 Documentation Complète

- [KEYCLOAK_SETUP.md](BlazorGameQuest1234/KEYCLOAK_SETUP.md) - Configuration Keycloak détaillée (realm, clients, rôles)
- [DOCKER_DEPLOYMENT.md](BlazorGameQuest1234/DOCKER_DEPLOYMENT.md) - Guide complet Docker Compose
- [SECURITY_IMPROVEMENTS.md](BlazorGameQuest1234/SECURITY_IMPROVEMENTS.md) - 15 endpoints sécurisés + KeycloakUserId
- [CHANGELOG_SECURITY.md](BlazorGameQuest1234/CHANGELOG_SECURITY.md) - Résumé des changements de sécurité

## 📖 Utilisation du Jeu

1. **Accéder à l'application** : http://localhost:5000
2. **Se connecter** : Cliquer sur "Se connecter avec Keycloak" (user1/user1 ou admin/admin)
3. **Démarrer une aventure** : Choisir une difficulté (Facile/Normal/Difficile)
4. **Explorer le donjon** : 5 salles avec défis aléatoires
5. **Prendre des décisions** : Combat, Fuite, Fouille, Repos selon la situation
6. **Consulter le classement** : Voir les meilleurs scores (menu "Classement")
7. **Dashboard admin** : Si rôle administrateur, gérer joueurs et consulter statistiques

## 🎯 Endpoints API Principaux

```http
# Gestion du jeu
POST   /api/game/start-adventure          # Démarrer une nouvelle partie
GET    /api/game/session/{id}             # Récupérer l'état d'une session
POST   /api/game/session/{id}/action      # Exécuter une action (combat, fuite, etc.)
POST   /api/game/session/{id}/next-room   # Passer à la salle suivante

# CRUD Joueurs (admin POST/PUT/DELETE)
GET    /api/players                        # Lister tous les joueurs
POST   /api/players                        # Créer un joueur [Authorize(admin)]
GET    /api/players/{id}                   # Détails d'un joueur
PUT    /api/players/{id}                   # Modifier un joueur [Authorize(admin)]
DELETE /api/players/{id}                   # Supprimer un joueur [Authorize(admin)]

# CRUD Utilisateurs (admin only)
GET    /api/users                          # Lister utilisateurs [Authorize(admin)]
POST   /api/users                          # Créer utilisateur [Authorize(admin)]
GET    /api/users/dashboard-stats          # Statistiques [Authorize(admin)]
```

## 🔧 Configuration

### appsettings.json (GameAPI)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres;Port=5432;Database=gamequest_db;Username=gamequest_user;Password=P@ssw0rd"
  },
  "Keycloak": {
    "Authority": "http://keycloak:8080/realms/blazor-gamequest"
  },
  "ApiSettings": {
    "AllowedOrigins": ["http://localhost:5000", "http://localhost:5003"]
  }
}
```

### docker-compose.yml

Contient la configuration complète pour :
- Keycloak (8080)
- PostgreSQL (5432)
- GameAPI (5001)
- ApiGateway (5000)
- BlazorClient (5003)

## 📦 Structure des Données

### Modèles principaux
- **User** : Comptes utilisateurs (Username, Email, PasswordHash, Role, IsActive)
- **Player** : Profils joueurs (Name, Level, XP, TotalScore, GamesPlayed, **KeycloakUserId**)
- **Dungeon** : Définition des donjons (Name, Description, Difficulty, Rooms)
- **Room** : Salles individuelles (Name, Description, RoomType, MonsterName, Treasure)
- **GameSession** : Sessions de jeu actives (état, statistiques, progression)

## 🐛 Troubleshooting

### Problème : Les conteneurs Docker ne démarrent pas
```powershell
# Vérifier les logs
docker-compose logs

# Supprimer et recréer
docker-compose down -v
docker-compose up --build
```

### Problème : Erreur d'authentification Keycloak
- Vérifier que Keycloak est bien démarré (http://localhost:8080)
- Vérifier les credentials (user1/user1, admin/admin)
- Vider le cache du navigateur et réessayer

### Problème : Migration de base de données échoue
```powershell
# En développement, utiliser InMemory (automatique si pas de ConnectionString)
# En production, vérifier PostgreSQL et appliquer manuellement :
cd BlazorGameQuest1234/DataAccess
dotnet ef database update
```

## 👥 Contributeurs

- **Arezki BEGGAR** - Développement Backend, Docker, Keycloak
- **Ilyas HADJADJ** - Développement Frontend, Tests, Intégration

## 📄 Licence

Projet académique - EFREI Paris 2024

---

**Version actuelle : 5.0** - Authentification Keycloak + Microservices + Docker