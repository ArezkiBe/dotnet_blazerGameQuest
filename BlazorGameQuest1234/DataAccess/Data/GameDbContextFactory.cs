/// <summary>
/// Factory de conception pour la création du DbContext lors des migrations
/// Permet à EF Core Tools de créer le contexte au moment du design
/// </summary>
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DataAccess.Data;

/// <summary>
/// Factory pour créer le GameDbContext au moment du design
/// </summary>
public class GameDbContextFactory : IDesignTimeDbContextFactory<GameDbContext>
{
    /// <summary>
    /// Crée une instance du GameDbContext pour les outils EF Core
    /// </summary>
    /// <param name="args">Arguments de ligne de commande</param>
    /// <returns>Instance configurée du GameDbContext</returns>
    public GameDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<GameDbContext>();
        
        // Configuration PostgreSQL pour les migrations
        // Utilise une chaîne de connexion par défaut pour le design-time
        optionsBuilder.UseNpgsql("Host=localhost;Database=blazergamequest_design;Username=postgres;Password=password");
        
        return new GameDbContext(optionsBuilder.Options);
    }
}