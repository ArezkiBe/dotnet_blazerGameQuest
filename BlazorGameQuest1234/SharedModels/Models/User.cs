/// <summary>
/// Utilisateur de base du système - sera lié à Keycloak plus tard
/// </summary>
namespace SharedModels.Models;

/// <summary>
/// Utilisateur de base du système
/// </summary>
public class User
{
    /// <summary>
    /// Identifiant unique de l'utilisateur
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Email de l'utilisateur (utilisé pour la connexion)
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Nom d'utilisateur affiché
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Rôle de l'utilisateur (Player, Administrator)
    /// </summary>
    public UserRole Role { get; set; } = UserRole.Player;
    
    /// <summary>
    /// Compte actif ou désactivé
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Date de création du compte
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Dernière connexion
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
}

/// <summary>
/// Énumération des rôles utilisateur
/// </summary>
public enum UserRole
{
    Player,
    Administrator
}