/// <summary>
/// Classe représentant un joueur du jeu
/// </summary>
namespace SharedModels.Models;

/// <summary>
/// Classe représentant un joueur du jeu
/// </summary>
public class Player
{
    /// <summary>
    /// Identifiant unique du joueur
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Référence vers l'utilisateur associé
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Nom d'utilisateur choisi par le joueur
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Score total accumulé par le joueur
    /// </summary>
    public int Score { get; set; } = 0;
    
    /// <summary>
    /// Numéro de la salle actuelle (1-5)
    /// </summary>
    public int CurrentRoom { get; set; } = 1;
    
    /// <summary>
    /// Date de création du compte joueur
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}