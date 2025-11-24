# BlazorGameQuest

**Membres du groupe :** Arezki BEGGAR, Ilyas HADJADJ

## Description

Jeu d'aventure Blazor WebAssembly avec API .NET 9. Le joueur explore des donjons générés aléatoirement avec système de score et progression RPG.

## Architecture

```
BlazorGameQuest1234/
├── BlazorGame.Client/          # Frontend Blazor WebAssembly
├── GameAPI/                   # API REST avec CRUD
├── SharedModels/              # Modèles de données partagés
├── DataAccess/                # Entity Framework Core
└── BlazorGameQuest.Tests/     # Tests unitaires (35 tests)
```

## Technologies

- .NET 9, Entity Framework Core, ASP.NET Web API
- Blazor WebAssembly, Swagger/OpenAPI
- xUnit, PostgreSQL/InMemory

## Fonctionnalités

- **Génération procédurale** : Donjons de 5 salles avec défis variés
- **Système RPG** : HP, XP, niveaux, statistiques de combat
- **Actions multiples** : Combat, fuite, fouille, repos, investigation
- **Calcul de score** : Points basés sur performance et difficulté
- **Persistance** : Sauvegarde automatique des scores

## API Endpoints

```
# Jeu
POST /api/game/start-adventure
GET /api/game/session/{id}
POST /api/game/session/{id}/action
POST /api/game/session/{id}/next-room

# CRUD
GET/POST/PUT/DELETE /api/users
GET/POST/PUT/DELETE /api/players
GET/POST/PUT/DELETE /api/dungeons
```

## Installation et Lancement

**Prérequis :** .NET 9 SDK

**Ports :**
- Frontend : http://localhost:5000
- API : http://localhost:5215
- Documentation : http://localhost:5215/swagger

```bash
# Terminal 1 - API
cd BlazorGameQuest1234/GameAPI
dotnet run

# Terminal 2 - Client
cd BlazorGameQuest1234/BlazorGame.Client
dotnet run
```

## Tests

```bash
# Exécuter les tests
dotnet test

# Avec couverture
dotnet test --collect:"XPlat Code Coverage"
```

**Résultats :** 35 tests passent - Services (DungeonGenerator, GameSession) et Contrôleurs (Users, Players, Dungeons, Rooms)

## Utilisation

1. Démarrer API et client
2. Ouvrir http://localhost:5000
3. Sélectionner joueur et difficulté
4. Explorer 5 salles générées aléatoirement
5. Choisir actions : Combat, Fuite, Fouille, Repos
6. Consulter score final sauvegardé

---

**EFREI Paris - Cours .NET - Semestre 7**