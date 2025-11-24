/// <summary>
/// Types de salles dans le donjon
/// Chaque type offre des actions et des rencontres spécifiques
/// </summary>
namespace SharedModels.Models;

/// <summary>
/// Énumération des différents types de salles
/// </summary>
public enum RoomType
{
    /// <summary>
    /// Salle avec des monstres à combattre
    /// Actions: Combat, Fuite
    /// </summary>
    Monster = 0,
    
    /// <summary>
    /// Salle au trésor avec des coffres
    /// Actions: Ouvrir Coffre, Fouiller, Ignorer
    /// </summary>
    Treasure = 1,
    
    /// <summary>
    /// Salle avec des pièges et des énigmes
    /// Actions: Fouiller (risqué), Contourner (sûr)
    /// </summary>
    Trap = 2,
    
    /// <summary>
    /// Salle vide avec objets éparpillés
    /// Actions: Fouiller, Ignorer
    /// </summary>
    Empty = 3,
    
    /// <summary>
    /// Salle de repos avec une fontaine magique
    /// Actions: Se reposer (guérit), Ignorer
    /// </summary>
    Rest = 4,
    
    /// <summary>
    /// Salle mystérieuse avec événements aléatoires
    /// Actions: Investiguer (très risqué), Fuir
    /// </summary>
    Mystery = 5
}