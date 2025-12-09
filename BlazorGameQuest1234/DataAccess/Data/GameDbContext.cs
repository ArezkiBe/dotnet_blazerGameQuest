/// <summary>
/// DbContext pour la base de données BlazorGameQuest Version 2
/// Configure les entités et leurs relations pour PostgreSQL
/// </summary>
using Microsoft.EntityFrameworkCore;
using SharedModels.Models;

namespace DataAccess.Data;

/// <summary>
/// Contexte de base de données principal pour BlazorGameQuest
/// </summary>
public class GameDbContext : DbContext
{
    /// <summary>
    /// Constructeur avec options de configuration
    /// </summary>
    /// <param name="options">Options de configuration du DbContext</param>
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Table des utilisateurs
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Table des joueurs
    /// </summary>
    public DbSet<Player> Players { get; set; }

    /// <summary>
    /// Table des administrateurs
    /// </summary>
    public DbSet<Administrator> Administrators { get; set; }

    /// <summary>
    /// Table des donjons
    /// </summary>
    public DbSet<Dungeon> Dungeons { get; set; }

    /// <summary>
    /// Table des salles
    /// </summary>
    public DbSet<Room> Rooms { get; set; }
    
    /// <summary>
    /// Table des sessions de jeu
    /// </summary>
    public DbSet<GameSession> GameSessions { get; set; }
    
    /// <summary>
    /// Table des actions de jeu
    /// </summary>
    public DbSet<GameAction> GameActions { get; set; }

    /// <summary>
    /// Configuration des modèles et relations
    /// </summary>
    /// <param name="modelBuilder">Builder pour la configuration des modèles</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuration de la table Users : gestion des utilisateurs avec authentification Keycloak
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email)
                  .IsRequired()
                  .HasMaxLength(100);
            entity.Property(e => e.Username)
                  .IsRequired()
                  .HasMaxLength(50);
            // Conversion de l'enum Role en string pour stockage en base
            entity.Property(e => e.Role)
                  .HasConversion<string>();
            // Index uniques pour garantir l'unicité des emails et usernames
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique();
        });

        // Configuration de la table Players : profils de joueurs liés aux utilisateurs
        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username)
                  .IsRequired()
                  .HasMaxLength(50);
            // Relation 1-1 avec User : chaque Player est lié à un User unique
            // DeleteBehavior.Cascade : la suppression d'un User supprime aussi le Player
            entity.HasOne(p => p.User)
                  .WithOne(u => u.Player)
                  .HasForeignKey<Player>(p => p.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuration de la table Administrators
        modelBuilder.Entity<Administrator>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(a => a.User)
                  .WithOne(u => u.Administrator)
                  .HasForeignKey<Administrator>(a => a.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuration de la table Dungeons : donjons générés procéduralement
        modelBuilder.Entity<Dungeon>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(100);
            entity.Property(e => e.Description)
                  .HasMaxLength(500);
        });

        // Configuration de la table Rooms : salles individuelles dans les donjons
        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title)
                  .IsRequired()
                  .HasMaxLength(100);
            entity.Property(e => e.Description)
                  .HasMaxLength(1000);
            entity.Property(e => e.EncounterType)
                  .HasMaxLength(50);
            // Relation 1-N : un Dungeon contient plusieurs Rooms
            entity.HasOne(r => r.Dungeon)
                  .WithMany(d => d.Rooms)
                  .HasForeignKey(r => r.DungeonId)
                  .OnDelete(DeleteBehavior.Cascade);
            // Index composite unique : garantit qu'un RoomNumber est unique par Dungeon
            entity.HasIndex(e => new { e.DungeonId, e.RoomNumber }).IsUnique();
        });
        
        // Configuration de la table GameSessions : sessions de jeu actives ou terminées
        modelBuilder.Entity<GameSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            // Conversion enum GameStatus → string pour lisibilité en base
            entity.Property(e => e.Status)
                  .HasConversion<string>();
            // Relation N-1 : un Player peut avoir plusieurs GameSessions
            entity.HasOne(gs => gs.Player)
                  .WithMany(p => p.GameSessions)
                  .HasForeignKey(gs => gs.PlayerId)
                  .OnDelete(DeleteBehavior.Cascade);
            // Relation N-1 : un Dungeon peut être joué dans plusieurs GameSessions
            entity.HasOne(gs => gs.Dungeon)
                  .WithMany(d => d.GameSessions)
                  .HasForeignKey(gs => gs.DungeonId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Configuration de la table GameActions : historique des actions des joueurs
        modelBuilder.Entity<GameAction>(entity =>
        {
            entity.HasKey(e => e.Id);
            // Conversion enum ActionType → string (Combat, Search, Flee, etc.)
            entity.Property(e => e.ActionType)
                  .HasConversion<string>();
            entity.Property(e => e.ResultDescription)
                  .HasMaxLength(500);
            entity.Property(e => e.ItemFound)
                  .HasMaxLength(100);
            // Relation N-1 : une GameSession contient plusieurs Actions
            entity.HasOne(ga => ga.GameSession)
                  .WithMany(gs => gs.Actions)
                  .HasForeignKey(ga => ga.GameSessionId)
                  .OnDelete(DeleteBehavior.Cascade);
            // Relation N-1 : une Room peut avoir plusieurs Actions effectuées
            entity.HasOne(ga => ga.Room)
                  .WithMany(r => r.Actions)
                  .HasForeignKey(ga => ga.RoomId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}