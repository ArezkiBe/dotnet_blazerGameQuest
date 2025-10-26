/// <summary>
/// Classe représentant un administrateur
/// </summary>
namespace SharedModels.Models;

/// <summary>
/// Classe représentant un administrateur
/// </summary>
public class Administrator
{
    /// <summary>
    /// Identifiant unique de l'administrateur
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Référence vers l'utilisateur associé
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Peut gérer les utilisateurs
    /// </summary>
    public bool CanManageUsers { get; set; } = true;
    
    /// <summary>
    /// Peut gérer les parties
    /// </summary>
    public bool CanManageGames { get; set; } = true;
    
    /// <summary>
    /// Peut voir les statistiques globales
    /// </summary>
    public bool CanViewAnalytics { get; set; } = true;
    
    /// <summary>
    /// Date de création du compte admin
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}