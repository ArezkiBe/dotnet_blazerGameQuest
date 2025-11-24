namespace SharedModels.Models;

/// <summary>
/// Action effectuée par le joueur dans une salle
/// </summary>
public class GameAction
{
    /// <summary>
    /// Identifiant unique de l'action
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// ID de la session de jeu
    /// </summary>
    public int GameSessionId { get; set; }
    
    /// <summary>
    /// ID de la salle où l'action a été effectuée
    /// </summary>
    public int RoomId { get; set; }
    
    /// <summary>
    /// Type d'action effectuée (Combat, Flee, Search, etc.)
    /// </summary>
    public ActionType ActionType { get; set; }
    
    /// <summary>
    /// Résultat de l'action (succès/échec)
    /// </summary>
    public bool IsSuccessful { get; set; }
    
    /// <summary>
    /// Points gagnés ou perdus suite à cette action
    /// </summary>
    public int PointsEarned { get; set; }
    
    /// <summary>
    /// Description du résultat de l'action
    /// </summary>
    public string ResultDescription { get; set; } = string.Empty;
    
    /// <summary>
    /// Objet trouvé (si applicable)
    /// </summary>
    public string? ItemFound { get; set; }
    
    /// <summary>
    /// Timestamp de l'action
    /// </summary>
    public DateTime ActionTime { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Navigation property vers la session
    /// </summary>
    public virtual GameSession? GameSession { get; set; }
    
    /// <summary>
    /// Navigation property vers la salle
    /// </summary>
    public virtual Room? Room { get; set; }
}

/// <summary>
/// Types d'actions possibles dans une salle
/// </summary>
public enum ActionType
{
    /// <summary>
    /// Combattre l'ennemi - Risqué mais plus de points
    /// </summary>
    Combat,
    
    /// <summary>
    /// Fuir la salle - Sûr mais moins de points
    /// </summary>
    Flee,
    
    /// <summary>
    /// Fouiller la salle - Chance de trésor ou piège
    /// </summary>
    Search,
    
    /// <summary>
    /// Ouvrir un coffre mystérieux
    /// </summary>
    OpenChest,
    
    /// <summary>
    /// Ignorer et passer à la salle suivante
    /// </summary>
    Ignore,
    
    /// <summary>
    /// Se reposer et récupérer de la santé (salles de repos)
    /// </summary>
    Rest,
    
    /// <summary>
    /// Investiguer des phénomènes mystérieux (très risqué)
    /// </summary>
    Investigate,
    
    /// <summary>
    /// Contourner prudemment les pièges (salle piège)
    /// </summary>
    Bypass
}