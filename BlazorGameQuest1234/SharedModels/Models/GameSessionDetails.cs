namespace SharedModels.Models;

/// <summary>
/// Détails complets d'une session de jeu avec salle actuelle et historique
/// </summary>
public class GameSessionDetails
{
    /// <summary>
    /// Session de jeu actuelle
    /// </summary>
    public GameSession? Session { get; set; }
    
    /// <summary>
    /// Salle actuelle du joueur
    /// </summary>
    public Room? CurrentRoom { get; set; }
    
    /// <summary>
    /// Historique des actions récentes
    /// </summary>
    public List<GameAction> ActionHistory { get; set; } = new();
    
    /// <summary>
    /// Actions disponibles dans la salle actuelle
    /// </summary>
    public string[] AvailableActions { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Résultat de progression vers une nouvelle salle
/// </summary>
public class RoomProgressionResult
{
    /// <summary>
    /// Message de progression (entrée dans nouvelle salle, victoire, etc.)
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Session mise à jour après progression
    /// </summary>
    public GameSession? Session { get; set; }
    
    /// <summary>
    /// Nouvelle salle actuelle (null si jeu terminé)
    /// </summary>
    public Room? CurrentRoom { get; set; }
    
    /// <summary>
    /// Indique si le jeu est maintenant terminé
    /// </summary>
    public bool IsGameCompleted { get; set; }
}