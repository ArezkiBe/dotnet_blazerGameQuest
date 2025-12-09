# 💾 DataAccess - Couche d'accès aux données

## 📋 Description

**DataAccess** est la couche de persistance de BlazorGameQuest v5.0. Elle utilise **Entity Framework Core 9.0** avec **PostgreSQL** pour gérer toutes les interactions avec la base de données.

## 🎯 Rôle et Responsabilités

### Fonctionnalités principales :
1. **DbContext** : Contexte de base de données principal (`GameDbContext`)
2. **Migrations** : Gestion du schéma de base de données avec EF Core Migrations
3. **Data Seeding** : Initialisation automatique avec données de test (`DbInitializer`)
4. **Design-Time Support** : Factory pour les outils EF Core CLI (`GameDbContextFactory`)

## 🗄️ Schéma de Base de Données

### Tables et Relations

```
┌─────────────┐
│    Users    │ ◄─── Authentification Keycloak
└──────┬──────┘      (synchronisation locale)
       │ 1
       │
       │ 1
┌──────▼──────┐      ┌──────────────┐
│   Players   │──────│ GameSessions │
└─────────────┘  N   └──────┬───────┘
                             │ N
                             │
                        ┌────▼────┐
                        │  Rooms  │
                        └────┬────┘
                             │ N
                        ┌────▼────┐
                        │Dungeons │
                        └─────────┘

┌──────────────┐
│ GameActions  │ ──► Historique des actions
└──────────────┘     (Combat, Search, etc.)
```

### Description des Entités

| Entité | Description | Clés/Index |
|--------|-------------|-----------|
| **Users** | Utilisateurs synchronisés avec Keycloak | PK: Id, UQ: Email, Username |
| **Players** | Profils de joueurs avec scores | PK: Id, FK: UserId (1-1) |
| **Dungeons** | Donjons générés procéduralement | PK: Id |
| **Rooms** | Salles individuelles dans les donjons | PK: Id, UQ: (DungeonId, RoomNumber) |
| **GameSessions** | Sessions de jeu actives/terminées | PK: Id, FK: PlayerId, DungeonId |
| **GameActions** | Actions effectuées par les joueurs | PK: Id, FK: GameSessionId, RoomId |

## 🏗️ Architecture de Code

### Structure du Projet

```
DataAccess/
├── Data/
│   ├── GameDbContext.cs         # DbContext principal
│   ├── DbInitializer.cs         # Seeding des données
│   └── GameDbContextFactory.cs  # Factory pour EF CLI
├── Migrations/
│   ├── 20251019_InitialCreate.cs
│   ├── 20251123_AddGameSessions.cs
│   ├── 20251124_UpdateRelationships.cs
│   └── GameDbContextModelSnapshot.cs
├── DataAccess.csproj
└── README.md
```

### Fichiers Clés

#### **GameDbContext.cs** (170 lignes)
- Configuration de 6 DbSet (Users, Players, Dungeons, Rooms, GameSessions, GameActions)
- Configuration Fluent API pour toutes les relations
- Contraintes d'intégrité et index uniques
- Conversions d'enums (Role, GameStatus, ActionType)

#### **DbInitializer.cs** (120 lignes)
- Pattern idempotent : vérifie si déjà initialisé
- Stratégie différente pour PostgreSQL vs InMemory
- Crée un donjon de démonstration avec 5 salles
- Note : Les utilisateurs sont gérés par Keycloak (pas de seeding)

#### **GameDbContextFactory.cs** (30 lignes)
- Implémente `IDesignTimeDbContextFactory<GameDbContext>`
- Permet l'utilisation des commandes EF Core CLI
- Utilise une connexion PostgreSQL par défaut pour le design-time

## 🔧 Configuration Fluent API

### Relations Configurées

#### **User → Player** (1-1)
```csharp
entity.HasOne(p => p.User)
      .WithOne(u => u.Player)
      .HasForeignKey<Player>(p => p.UserId)
      .OnDelete(DeleteBehavior.Cascade);
```

#### **Dungeon → Rooms** (1-N)
```csharp
entity.HasOne(r => r.Dungeon)
      .WithMany(d => d.Rooms)
      .HasForeignKey(r => r.DungeonId)
      .OnDelete(DeleteBehavior.Cascade);
```

#### **GameSession → Player/Dungeon** (N-1)
```csharp
// Un joueur peut avoir plusieurs sessions
entity.HasOne(gs => gs.Player)
      .WithMany(p => p.GameSessions)
      .HasForeignKey(gs => gs.PlayerId)
      .OnDelete(DeleteBehavior.Cascade);

// Un donjon peut être joué plusieurs fois
entity.HasOne(gs => gs.Dungeon)
      .WithMany(d => d.GameSessions)
      .HasForeignKey(gs => gs.DungeonId)
      .OnDelete(DeleteBehavior.Cascade);
```

#### **GameAction → GameSession/Room** (N-1)
```csharp
// Historique des actions par session
entity.HasOne(ga => ga.GameSession)
      .WithMany(gs => gs.Actions)
      .HasForeignKey(ga => ga.GameSessionId)
      .OnDelete(DeleteBehavior.Cascade);
```

### Contraintes et Index

| Table | Contrainte | Type | Raison |
|-------|------------|------|--------|
| Users | Email | UNIQUE | Éviter les doublons |
| Users | Username | UNIQUE | Identifiant unique |
| Rooms | (DungeonId, RoomNumber) | UNIQUE COMPOSITE | Une salle par numéro par donjon |

### Conversions d'Enums

```csharp
// Role → "administrateur" ou "joueur"
entity.Property(e => e.Role)
      .HasConversion<string>();

// GameStatus → "Active", "Completed", "Dead", "Failed"
entity.Property(e => e.Status)
      .HasConversion<string>();

// ActionType → "Combat", "Search", "Flee", etc.
entity.Property(e => e.ActionType)
      .HasConversion<string>();
```

**Avantage** : Lisibilité en base de données (strings au lieu d'entiers)

## 🗃️ Migrations Entity Framework

### Historique des Migrations

| Date | Migration | Description |
|------|-----------|-------------|
| 2024-10-19 | InitialCreate | Création initiale : Users, Players, Dungeons, Rooms |
| 2024-11-23 | AddGameSessions | Ajout GameSessions et GameActions |
| 2024-11-23 | AddRoguelikeStats | Stats roguelike : HP, XP, Level |
| 2024-11-23 | AddRoomType | Ajout enum RoomType (Monster, Treasure, Trap) |
| 2024-11-24 | UpdateRelationships | Correction relations Dungeon-Room |
| 2024-12-09 | AddKeycloakUserId | Ajout KeycloakUserId pour Players |

### Commandes EF Core

```bash
# Créer une nouvelle migration
dotnet ef migrations add NomDeLaMigration --project DataAccess

# Appliquer les migrations
dotnet ef database update --project DataAccess

# Générer un script SQL
dotnet ef migrations script --project DataAccess

# Supprimer la dernière migration (non appliquée)
dotnet ef migrations remove --project DataAccess

# Afficher l'historique
dotnet ef migrations list --project DataAccess
```

## 🚀 Utilisation

### Configuration dans Program.cs (GameAPI)

```csharp
builder.Services.AddDbContext<GameDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    if (!string.IsNullOrEmpty(connectionString))
    {
        // Production : PostgreSQL
        options.UseNpgsql(connectionString);
    }
    else
    {
        // Tests : InMemory
        options.UseInMemoryDatabase("BlazorGameQuestDB");
    }
});

// Initialiser la base avec données de test
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<GameDbContext>();
    await DbInitializer.InitializeAsync(dbContext);
}
```

### Chaîne de Connexion PostgreSQL

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=blazergamequest;Username=postgres;Password=yourpassword"
  }
}
```

### Injection dans les Contrôleurs

```csharp
public class PlayersController : ControllerBase
{
    private readonly GameDbContext _context;

    public PlayersController(GameDbContext context)
    {
        _context = context;
    }

    public async Task<ActionResult<Player>> GetPlayer(int id)
    {
        var player = await _context.Players
            .Include(p => p.User)
            .Include(p => p.GameSessions)
            .FirstOrDefaultAsync(p => p.Id == id);
        
        return player == null ? NotFound() : Ok(player);
    }
}
```

## 🧪 Tests

### Utilisation en Tests Unitaires

```csharp
// Dans les tests : utiliser InMemory
var options = new DbContextOptionsBuilder<GameDbContext>()
    .UseInMemoryDatabase(databaseName: "TestDatabase")
    .Options;

var context = new GameDbContext(options);
await DbInitializer.InitializeAsync(context);

// Tester avec des données
var players = await context.Players.ToListAsync();
Assert.NotEmpty(players);
```

## 📦 Dépendances

- **Npgsql.EntityFrameworkCore.PostgreSQL** : Provider PostgreSQL pour EF Core
- **Microsoft.EntityFrameworkCore.Design** : Outils CLI pour migrations
- **Microsoft.EntityFrameworkCore.InMemory** : Base en mémoire pour tests
- **SharedModels** : Référence au projet contenant les entités

## 🔐 Sécurité et Bonnes Pratiques

### DeleteBehavior.Cascade
- Utilisé sur toutes les relations pour garantir l'intégrité référentielle
- Suppression d'un User → supprime automatiquement le Player associé
- Suppression d'un Dungeon → supprime toutes ses Rooms et GameSessions

### Validation des Contraintes
- **IsRequired()** : Champs non-null au niveau base de données
- **HasMaxLength()** : Limite de taille pour strings (évite l'overflow)
- **HasIndex().IsUnique()** : Garantit l'unicité (Email, Username)

### Pattern Idempotent
```csharp
// DbInitializer vérifie si déjà initialisé
if (await context.Users.AnyAsync())
{
    return; // Ne rien faire si déjà des données
}
```

## ✅ Checklist de Conformité

- ✅ **Nommage limpide** : Entités claires (User, Player, GameSession)
- ✅ **Code court** : Chaque fichier < 200 lignes
- ✅ **Algorithmes simples** : Configuration déclarative Fluent API
- ✅ **Bien commenté** : Chaque configuration expliquée
- ✅ **Design accessible** : Architecture EF Core standard
- ✅ **Pas de fichiers inutiles** : Seulement Data/ et Migrations/
- ✅ **Tests compatibles** : Support InMemory Database

## 📊 Métriques

- **Nombre d'entités** : 6 (Users, Players, Administrators, Dungeons, Rooms, GameSessions, GameActions)
- **Nombre de migrations** : 6
- **Lignes de code** : ~320 lignes (sans migrations)
- **Relations configurées** : 8 relations 1-1 et 1-N
- **Index uniques** : 3 (Email, Username, DungeonId+RoomNumber)

## 🔗 Liens Utiles

- [EF Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [Fluent API Configuration](https://learn.microsoft.com/en-us/ef/core/modeling/)
- [Npgsql Provider](https://www.npgsql.org/efcore/)
- [Migrations Overview](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)

---

**DataAccess v5.0 - Couche de persistance robuste et bien documentée** 🚀
