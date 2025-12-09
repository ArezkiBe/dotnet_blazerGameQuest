using System.Security.Claims;

namespace BlazorGame.Client.Services;

/// <summary>
/// Service d'autorisation centralisé pour gérer les rôles et permissions
/// Utilisé pour vérifier les droits d'accès aux pages et fonctionnalités
/// </summary>
public interface IAuthorizationService
{
    /// <summary>
    /// Vérifie si l'utilisateur a le rôle Admin
    /// </summary>
    bool IsAdmin(ClaimsPrincipal? user);
    
    /// <summary>
    /// Vérifie si l'utilisateur a le rôle Player (connecté mais pas admin)
    /// </summary>
    bool IsPlayer(ClaimsPrincipal? user);
    
    /// <summary>
    /// Vérifie si l'utilisateur est authentifié
    /// </summary>
    bool IsAuthenticated(ClaimsPrincipal? user);
}

public class AuthorizationService : IAuthorizationService
{
    /// <summary>
    /// Vérifie si l'utilisateur a le rôle Admin
    /// Vérifie le username "admin" ou les rôles "admin"/"administrator"
    /// </summary>
    public bool IsAdmin(ClaimsPrincipal? user)
    {
        if (user?.Identity?.IsAuthenticated != true) return false;
        
        // Vérification du nom d'utilisateur admin
        var username = user.FindFirst("preferred_username")?.Value ?? "";
        if (username.ToLower() == "admin") return true;
        
        // Vérification des rôles
        return user.IsInRole("admin") || user.IsInRole("administrator");
    }

    /// <summary>
    /// Vérifie si l'utilisateur a le rôle Player
    /// Un joueur est quelqu'un de connecté qui n'est pas admin
    /// </summary>
    public bool IsPlayer(ClaimsPrincipal? user)
    {
        if (user?.Identity?.IsAuthenticated != true) return false;
        
        // Un joueur est quelqu'un de connecté qui n'est pas admin
        return !IsAdmin(user);
    }

    /// <summary>
    /// Vérifie si l'utilisateur est authentifié (connecté)
    /// </summary>
    public bool IsAuthenticated(ClaimsPrincipal? user)
    {
        return user?.Identity?.IsAuthenticated == true;
    }
}