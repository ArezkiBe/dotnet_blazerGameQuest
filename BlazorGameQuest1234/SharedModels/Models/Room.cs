/// <summary>
/// Salle individuelle dans un donjon
/// </summary>
namespace SharedModels.Models;

/// <summary>
/// Salle individuelle dans un donjon
/// </summary>
public class Room
{
    /// <summary>
    /// Identifiant unique de la salle
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Donjon auquel appartient cette salle
    /// </summary>
    public int DungeonId { get; set; }
    
    /// <summary>
    /// Numéro de la salle dans le donjon (1-5)
    /// </summary>
    public int RoomNumber { get; set; }
    
    /// <summary>
    /// Titre de la salle
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Description narrative de la salle
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Type de salle déterminant les actions disponibles
    /// </summary>
    public RoomType Type { get; set; } = RoomType.Monster;
    
    /// <summary>
    /// Type d'ennemi/obstacle rencontré
    /// </summary>
    public string EncounterType { get; set; } = string.Empty;
    
    /// <summary>
    /// Difficulté spécifique de cette salle (1-5)
    /// </summary>
    public int Difficulty { get; set; } = 1;
    
    /// <summary>
    /// Pourcentage de réussite pour le combat (1-100)
    /// </summary>
    public int CombatSuccessRate { get; set; } = 70;
    
    /// <summary>
    /// Points gagnés en cas de victoire au combat
    /// </summary>
    public int CombatReward { get; set; } = 15;
    
    /// <summary>
    /// Points gagnés en fuyant (toujours réussi)
    /// </summary>
    public int FleeReward { get; set; } = 5;
    
    /// <summary>
    /// Pourcentage de chance de trouver un trésor en fouillant (1-100)
    /// </summary>
    public int SearchSuccessRate { get; set; } = 50;
    
    /// <summary>
    /// Points gagnés si la fouille réussit
    /// </summary>
    public int SearchReward { get; set; } = 12;
    
    /// <summary>
    /// Points perdus si la fouille échoue (piège)
    /// </summary>
    public int SearchPenalty { get; set; } = -5;
    
    /// <summary>
    /// Actions effectuées dans cette salle
    /// </summary>
    public virtual ICollection<GameAction> Actions { get; set; } = new List<GameAction>();
    
    /// <summary>
    /// Donjon auquel appartient cette salle
    /// </summary>
    public virtual Dungeon? Dungeon { get; set; }
}