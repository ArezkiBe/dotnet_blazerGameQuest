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
    /// Type d'ennemi/obstacle rencontré
    /// </summary>
    public string EncounterType { get; set; } = string.Empty;
    
    /// <summary>
    /// Difficulté spécifique de cette salle (1-5)
    /// </summary>
    public int Difficulty { get; set; } = 1;
}