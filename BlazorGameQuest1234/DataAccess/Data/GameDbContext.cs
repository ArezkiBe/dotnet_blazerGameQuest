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
    /// Configuration des modèles et relations
    /// </summary>
    /// <param name="modelBuilder">Builder pour la configuration des modèles</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuration de la table Users
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email)
                  .IsRequired()
                  .HasMaxLength(100);
            entity.Property(e => e.Username)
                  .IsRequired()
                  .HasMaxLength(50);
            entity.Property(e => e.Role)
                  .HasConversion<string>();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique();
        });

        // Configuration de la table Players
        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username)
                  .IsRequired()
                  .HasMaxLength(50);
            entity.HasOne<User>()
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuration de la table Administrators
        modelBuilder.Entity<Administrator>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne<User>()
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuration de la table Dungeons
        modelBuilder.Entity<Dungeon>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(100);
            entity.Property(e => e.Description)
                  .HasMaxLength(500);
        });

        // Configuration de la table Rooms
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
            entity.HasOne<Dungeon>()
                  .WithMany()
                  .HasForeignKey(e => e.DungeonId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.DungeonId, e.RoomNumber }).IsUnique();
        });
    }
}