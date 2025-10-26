/// <summary>
/// Modèle représentant un joueur dans le jeu BlazorGameQuest
/// Contient les informations de base du joueur et ses statistiques
/// </summary>
namespace SharedModels;

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

/// <summary>
/// Énumération des actions possibles dans une salle de donjon
/// </summary>
public enum GameAction
{
    /// <summary>
    /// Action de combat - Risqué mais plus de points
    /// </summary>
    Combat,
    
    /// <summary>
    /// Action de fuite - Sûr mais moins de points
    /// </summary>
    Flee,
    
    /// <summary>
    /// Action de fouille - Chance de trésor ou piège
    /// </summary>
    Search
}

/// <summary>
/// Résultat d'une action de jeu
/// </summary>
public class ActionResult
{
    /// <summary>
    /// Action effectuée par le joueur
    /// </summary>
    public GameAction Action { get; set; }
    
    /// <summary>
    /// Points gagnés ou perdus
    /// </summary>
    public int Points { get; set; }
    
    /// <summary>
    /// Message descriptif du résultat
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Indique si l'action a réussi
    /// </summary>
    public bool Success { get; set; }
}