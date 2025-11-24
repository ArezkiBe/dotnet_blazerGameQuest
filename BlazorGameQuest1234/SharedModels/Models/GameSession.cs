namespace SharedModels.Models;

/// <summary>
/// Session de jeu active - trace une partie en cours d'un joueur
/// </summary>
public class GameSession
{
    /// <summary>
    /// Identifiant unique de la session de jeu
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// ID du joueur participant à cette session
    /// </summary>
    public int PlayerId { get; set; }
    
    /// <summary>
    /// ID du donjon généré pour cette session
    /// </summary>
    public int DungeonId { get; set; }
    
    /// <summary>
    /// Numéro de la salle actuelle (1-5)
    /// </summary>
    public int CurrentRoomNumber { get; set; } = 1;
    
    /// <summary>
    /// Score actuel de la session
    /// </summary>
    public int CurrentScore { get; set; } = 0;
    
    /// <summary>
    /// Statut de la partie (Active, Completed, Failed)
    /// </summary>
    public GameStatus Status { get; set; } = GameStatus.Active;
    
    /// <summary>
    /// Date de début de la session
    /// </summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Date de fin de la session (nullable tant que la partie n'est pas terminée)
    /// </summary>
    public DateTime? CompletedAt { get; set; }
    
    // === ROGUELIKE STATS (Reset each run) ===
    
    /// <summary>
    /// Points de vie actuels pour cette aventure
    /// </summary>
    public int CurrentHP { get; set; } = 100;
    
    /// <summary>
    /// Points de vie maximum pour cette aventure
    /// </summary>
    public int MaxHP { get; set; } = 100;
    
    /// <summary>
    /// Dégâts d'attaque pour cette aventure
    /// </summary>
    public int AttackDamage { get; set; } = 20;
    
    /// <summary>
    /// Défense pour cette aventure
    /// </summary>
    public int Defense { get; set; } = 5;
    
    /// <summary>
    /// Niveau pour cette aventure
    /// </summary>
    public int Level { get; set; } = 1;
    
    /// <summary>
    /// Points d'expérience pour cette aventure
    /// </summary>
    public int ExperiencePoints { get; set; } = 0;
    
    /// <summary>
    /// Score total final de cette session (calculé à la fin)
    /// </summary>
    public int TotalScore { get; set; } = 0;
    
    /// <summary>
    /// Navigation property vers le joueur
    /// </summary>
    public virtual Player? Player { get; set; }
    
    /// <summary>
    /// Navigation property vers le donjon
    /// </summary>
    public virtual Dungeon? Dungeon { get; set; }
    
    /// <summary>
    /// Actions effectuées pendant cette session
    /// </summary>
    public virtual ICollection<GameAction> Actions { get; set; } = new List<GameAction>();
}

/// <summary>
/// Statut d'une partie
/// </summary>
public enum GameStatus
{
    /// <summary>
    /// Partie en cours
    /// </summary>
    Active,
    
    /// <summary>
    /// Partie terminée avec succès (toutes les salles parcourues)
    /// </summary>
    Completed,
    
    /// <summary>
    /// Partie échouée (abandon volontaire)
    /// </summary>
    Failed,
    
    /// <summary>
    /// Partie terminée par mort du joueur (HP = 0)
    /// </summary>
    Dead
}