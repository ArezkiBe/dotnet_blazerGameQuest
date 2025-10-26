/// <summary>
/// Modèles de jeu de base pour BlazorGameQuest Version 2
/// Contient les énumérations et classes utilitaires de base
/// </summary>
namespace SharedModels;

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