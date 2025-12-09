using System.Security.Claims;

namespace BlazorGame.Client.Services;

/// <summary>
/// Service d'autorisation centralisé pour gérer les rôles et permissions
/// </summary>
public interface IAuthorizationService
{
    bool IsAdmin(ClaimsPrincipal? user);
    bool IsPlayer(ClaimsPrincipal? user);
    bool IsAuthenticated(ClaimsPrincipal? user);
}

public class AuthorizationService : IAuthorizationService
{
    public bool IsAdmin(ClaimsPrincipal? user)
    {
        if (user?.Identity?.IsAuthenticated != true) return false;
        
        // Vérification du nom d'utilisateur admin
        var username = user.FindFirst("preferred_username")?.Value ?? "";
        if (username.ToLower() == "admin") return true;
        
        // Vérification des rôles
        return user.IsInRole("admin") || user.IsInRole("administrator");
    }

    public bool IsPlayer(ClaimsPrincipal? user)
    {
        if (user?.Identity?.IsAuthenticated != true) return false;
        
        // Un joueur est quelqu'un de connecté qui n'est pas admin
        return !IsAdmin(user);
    }

    public bool IsAuthenticated(ClaimsPrincipal? user)
    {
        return user?.Identity?.IsAuthenticated == true;
    }
}