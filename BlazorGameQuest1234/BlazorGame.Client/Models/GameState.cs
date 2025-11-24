using SharedModels.Models;
using BlazorGame.Client.Services;

namespace BlazorGame.Client.Models;

/// <summary>
/// État du jeu côté client
/// Gère l'état local de l'interface utilisateur
/// </summary>
public class GameState
{
    /// <summary>
    /// Session de jeu actuelle
    /// </summary>
    public GameSession? CurrentSession { get; set; }
    
    /// <summary>
    /// Salle actuelle
    /// </summary>
    public Room? CurrentRoom { get; set; }
    
    /// <summary>
    /// Dernière action effectuée
    /// </summary>
    public GameAction? LastAction { get; set; }
    
    /// <summary>
    /// Joueur sélectionné
    /// </summary>
    public Player? SelectedPlayer { get; set; }
    
    /// <summary>
    /// Actions disponibles dans la salle actuelle
    /// </summary>
    public string[] AvailableActions { get; set; } = Array.Empty<string>();
    
    /// <summary>
    /// État de chargement
    /// </summary>
    public bool IsLoading { get; set; } = false;
    
    /// <summary>
    /// Message d'erreur éventuel
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Indique si une partie est en cours
    /// </summary>
    public bool IsGameActive => CurrentSession?.Status == GameStatus.Active;
    
    /// <summary>
    /// Indique si une action a été effectuée et attend la progression
    /// </summary>
    public bool IsWaitingForProgression { get; set; } = false;
    
    /// <summary>
    /// Message de résultat à afficher
    /// </summary>
    public string? ResultMessage { get; set; }
    
    /// <summary>
    /// Historique des actions récentes
    /// </summary>
    public List<GameAction> RecentActions { get; set; } = new();

    /// <summary>
    /// Remet à zéro l'état pour une nouvelle partie
    /// </summary>
    public void Reset()
    {
        CurrentSession = null;
        CurrentRoom = null;
        LastAction = null;
        AvailableActions = Array.Empty<string>();
        IsWaitingForProgression = false;
        ResultMessage = null;
        RecentActions.Clear();
        ErrorMessage = null;
    }
    
    /// <summary>
    /// Met à jour l'état avec les données d'une session
    /// </summary>
    public void UpdateFromSessionDetails(GameSessionDetails details)
    {
        CurrentSession = details.Session;
        CurrentRoom = details.CurrentRoom;
        AvailableActions = details.AvailableActions;
        RecentActions = details.ActionHistory.TakeLast(5).ToList();
        ErrorMessage = null;
    }
    
    /// <summary>
    /// Met à jour l'état après une action
    /// </summary>
    public void UpdateFromAction(GameAction action)
    {
        LastAction = action;
        ResultMessage = action.ResultDescription;
        IsWaitingForProgression = true;
        
        // Ajouter à l'historique
        RecentActions.Add(action);
        if (RecentActions.Count > 5)
        {
            RecentActions.RemoveAt(0);
        }
    }
}