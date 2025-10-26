# BlazorGameQuest Version 2

Membres du groupe:
Arezki BEGGAR
Ilyas HADJADJ

## Architecture du Projet

### Structure de la Solution
```
BlazorGameQuest1234/
├── BlazorGame.Client/          # Application Blazor WebAssembly (Frontend)
├── AuthenticationServices/     # API Web avec CRUD complet
├── SharedModels/              # Modèles de domaine (User, Player, Dungeon, Room)
├── DataAccess/                # Couche d'accès aux données (EF Core)
└── BlazorGameQuest.Tests/     # Tests unitaires (24 tests)
```

### Technologies Utilisées
- **.NET 9** - Framework principal
- **Entity Framework Core 9** - ORM avec PostgreSQL/InMemory
- **ASP.NET Core Web API** - Services REST complets
- **Swagger/OpenAPI** - Documentation API interactive
- **xUnit** - Tests unitaires avec couverture de code

## Nouveautés Version 2

### Base de Données et Modèles
- **Utilisateurs** - Gestion complète des comptes (Player/Administrator)
- **Joueurs** - Profils avec scores et progression
- **Donjons** - Donjons générés avec description et métadonnées
- **Salles** - Salles individuelles liées aux donjons
- **Entity Framework Core** - Migrations et initialisation automatique

### API REST Complète
- **Users API** - CRUD complet avec statistiques
- **Players API** - Gestion des profils et leaderboard
- **Dungeons API** - CRUD des donjons
- **Rooms API** - CRUD des salles par donjon
- **Swagger UI** - Interface de test à `/swagger`

### Endpoints Principaux
```
GET/POST/PUT/DELETE /api/users
GET/POST/PUT/DELETE /api/players
GET/POST/PUT/DELETE /api/dungeons  
GET/POST/PUT/DELETE /api/rooms
GET /api/players/leaderboard
GET /health
GET /database-info
```

## Installation et Lancement

### Prérequis
- .NET 9 SDK

### Démarrage Rapide
```bash
cd BlazorGameQuest1234
dotnet run --project AuthenticationServices
```

### Tests
```bash
dotnet test BlazorGameQuest.Tests
```

### Documentation API
Accédez à `/swagger` une fois l'API lancée


## Auteurs

Développé dans le cadre du cours .NET - Semestre 7
EFREI Paris - Promotion LSI2

---