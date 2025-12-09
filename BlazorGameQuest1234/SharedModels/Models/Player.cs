namespace SharedModels.Models;

/// <summary>
/// Profil joueur avec statistiques et progression
/// </summary>
public class Player
{
    public int Id { get; set; }
    
    /// <summary>
    /// Utilisateur associé à ce profil joueur
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Identifiant Keycloak du joueur (sub claim du JWT)
    /// Permet de lier le joueur à son compte Keycloak
    /// </summary>
    public string KeycloakUserId { get; set; } = string.Empty;
    
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Score total cumulé de toutes les parties
    /// </summary>
    public int Score { get; set; } = 0;
    
    public int CurrentRoom { get; set; } = 1;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Relations
    public virtual ICollection<GameSession> GameSessions { get; set; } = new List<GameSession>();
    public virtual User? User { get; set; }
}