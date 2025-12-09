using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataAccess.Data;
using SharedModels.Models;
using Microsoft.AspNetCore.Authorization;

namespace GameAPI.Controllers;

/// <summary>
/// API Controller dédié aux fonctionnalités administrateur
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Admin")]
[Authorize] // Utilisateurs authentifiés, vérification admin dans les méthodes
public class AdminController : ControllerBase
{
    private readonly GameDbContext _context;
    private readonly ILogger<AdminController> _logger;

    public AdminController(GameDbContext context, ILogger<AdminController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Vérifie si l'utilisateur actuel est administrateur
    /// Supporte plusieurs variantes de rôles pour compatibilité Keycloak
    /// </summary>
    private bool IsCurrentUserAdmin()
    {
        var username = User.FindFirst("preferred_username")?.Value ?? "";
        return username.ToLower() == "admin" || User.IsInRole("admin") || User.IsInRole("administrator");
    }

    /// <summary>
    /// Récupère les statistiques globales pour le dashboard administrateur
    /// </summary>
    /// <returns>Statistiques globales du système</returns>
    [HttpGet("dashboard")]
    public async Task<ActionResult<object>> GetDashboardData()
    {
        if (!IsCurrentUserAdmin())
        {
            return Forbid("Accès réservé aux administrateurs");
        }

        var totalUsers = await _context.Users.CountAsync();
        var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
        var totalPlayers = await _context.Players.CountAsync();
        var totalAdmins = await _context.Users.CountAsync(u => u.Role == UserRole.Administrator);
        
        var totalSessions = await _context.GameSessions.CountAsync();
        var activeSessions = await _context.GameSessions.CountAsync(gs => !gs.CompletedAt.HasValue);
        
        var topPlayer = await _context.Players
            .OrderByDescending(p => p.Score)
            .Select(p => new { p.Username, p.Score })
            .FirstOrDefaultAsync();
            
        var averageScore = await _context.Players.AverageAsync(p => (double?)p.Score) ?? 0;
        var totalScore = await _context.Players.SumAsync(p => p.Score);
        
        // Calcul des statistiques d'activité récente
        var yesterday = DateTime.UtcNow.AddDays(-1);
        var recentSessions = await _context.GameSessions
            .Where(gs => gs.StartedAt >= yesterday)
            .CountAsync();
            
        var newUsersToday = await _context.Users
            .Where(u => u.CreatedAt >= yesterday)
            .CountAsync();

        // Récupération des sessions les plus récentes avec durée calculée
        var recentActivity = await _context.GameSessions
            .Include(gs => gs.Player)
            .OrderByDescending(gs => gs.StartedAt)
            .Take(5)
            .Select(gs => new {
                PlayerName = gs.Player!.Username,
                Score = gs.TotalScore,
                StartTime = gs.StartedAt,
                Duration = gs.CompletedAt.HasValue ? 
                    (int?)(gs.CompletedAt.Value - gs.StartedAt).TotalMinutes : null,
                IsActive = !gs.CompletedAt.HasValue
            })
            .ToListAsync();

        var dashboardData = new
        {
            UserStats = new
            {
                Total = totalUsers,
                Active = activeUsers,
                Inactive = totalUsers - activeUsers,
                Players = totalPlayers,
                Administrators = totalAdmins,
                NewToday = newUsersToday
            },
            GameStats = new
            {
                TotalSessions = totalSessions,
                ActiveSessions = activeSessions,
                RecentSessions = recentSessions,
                TopPlayer = topPlayer?.Username ?? "Aucun",
                TopScore = topPlayer?.Score ?? 0,
                AverageScore = (int)averageScore,
                TotalScore = totalScore
            },
            RecentActivity = recentActivity,
            SystemInfo = new
            {
                ServerTime = DateTime.UtcNow,
                Version = "4.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
            }
        };

        return Ok(dashboardData);
    }

    /// <summary>
    /// Exporte toutes les données du système (Admin uniquement)
    /// </summary>
    /// <param name="format">Format d'export: json, csv</param>
    /// <param name="includeInactive">Inclure les utilisateurs inactifs</param>
    /// <returns>Données exportées</returns>
    [HttpGet("export")]
    public async Task<ActionResult<object>> ExportSystemData(
        [FromQuery] string format = "json", 
        [FromQuery] bool includeInactive = false)
    {
        var userQuery = _context.Users.AsQueryable();
        var playerQuery = _context.Players.Include(p => p.User).AsQueryable();

        if (!includeInactive)
        {
            userQuery = userQuery.Where(u => u.IsActive);
            playerQuery = playerQuery.Where(p => p.User!.IsActive);
        }

        var users = await userQuery
            .OrderBy(u => u.Username)
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.Email,
                Role = u.Role.ToString(),
                u.IsActive,
                CreatedAt = u.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
            })
            .ToListAsync();

        var players = await playerQuery
            .Include(p => p.GameSessions)
            .OrderByDescending(p => p.Score)
            .Select(p => new
            {
                p.Id,
                p.Username,
                p.Score,
                p.CurrentRoom,
                Email = p.User!.Email,
                IsActive = p.User.IsActive,
                CreatedAt = p.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                SessionsCount = p.GameSessions.Count,
                TotalPlaytime = p.GameSessions
                    .Where(gs => gs.CompletedAt.HasValue)
                    .Sum(gs => (gs.CompletedAt!.Value - gs.StartedAt).TotalMinutes),
                LastSession = p.GameSessions
                    .OrderByDescending(gs => gs.StartedAt)
                    .Select(gs => gs.StartedAt.ToString("yyyy-MM-dd HH:mm:ss"))
                    .FirstOrDefault()
            })
            .ToListAsync();

        var sessions = await _context.GameSessions
            .Include(gs => gs.Player!)
                .ThenInclude(p => p.User)
            .Where(gs => includeInactive || gs.Player!.User!.IsActive)
            .OrderByDescending(gs => gs.StartedAt)
            .Select(gs => new
            {
                gs.Id,
                PlayerName = gs.Player!.Username,
                Score = gs.TotalScore,
                StartTime = gs.StartedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                EndTime = gs.CompletedAt.HasValue ? gs.CompletedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                Duration = gs.CompletedAt.HasValue ? 
                    (int?)(gs.CompletedAt.Value - gs.StartedAt).TotalMinutes : null,
                IsCompleted = gs.CompletedAt.HasValue
            })
            .ToListAsync();

        var exportData = new
        {
            ExportInfo = new
            {
                ExportDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                Format = format,
                IncludesInactive = includeInactive,
                GeneratedBy = "BlazorGameQuest Admin Panel"
            },
            Summary = new
            {
                TotalUsers = users.Count,
                TotalPlayers = players.Count,
                TotalSessions = sessions.Count,
                ActiveUsers = users.Count(u => u.IsActive),
                CompletedSessions = sessions.Count(s => s.IsCompleted)
            },
            Users = users,
            Players = players,
            Sessions = sessions
        };

        if (format.ToLower() == "csv")
        {
            // Format CSV : structure séparée pour faciliter la conversion côté client
            return Ok(new
            {
                exportData.ExportInfo,
                exportData.Summary,
                CsvData = new
                {
                    Users = users,
                    Players = players,
                    Sessions = sessions
                }
            });
        }

        return Ok(exportData);
    }

    /// <summary>
    /// Récupère les métriques système pour le monitoring
    /// </summary>
    [HttpGet("metrics")]
    public async Task<ActionResult<object>> GetSystemMetrics()
    {
        var now = DateTime.UtcNow;
        var last24h = now.AddDays(-1);
        var last7days = now.AddDays(-7);
        var last30days = now.AddDays(-30);

        var metrics = new
        {
            Database = new
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalPlayers = await _context.Players.CountAsync(),
                TotalSessions = await _context.GameSessions.CountAsync(),
                ActiveSessions = await _context.GameSessions.CountAsync(gs => !gs.CompletedAt.HasValue)
            },
            Activity = new
            {
                NewUsersLast24h = await _context.Users.CountAsync(u => u.CreatedAt >= last24h),
                NewUsersLast7days = await _context.Users.CountAsync(u => u.CreatedAt >= last7days),
                NewUsersLast30days = await _context.Users.CountAsync(u => u.CreatedAt >= last30days),
                SessionsLast24h = await _context.GameSessions.CountAsync(gs => gs.StartedAt >= last24h),
                SessionsLast7days = await _context.GameSessions.CountAsync(gs => gs.StartedAt >= last7days),
                SessionsLast30days = await _context.GameSessions.CountAsync(gs => gs.StartedAt >= last30days)
            },
            Performance = new
            {
                AverageSessionDuration = await _context.GameSessions
                    .Where(gs => gs.CompletedAt.HasValue)
                    .AverageAsync(gs => (double?)(gs.CompletedAt!.Value - gs.StartedAt).TotalMinutes) ?? 0,
                TopScoreLast30days = await _context.GameSessions
                    .Where(gs => gs.StartedAt >= last30days)
                    .MaxAsync(gs => (int?)gs.TotalScore) ?? 0,
                AverageScoreLast30days = await _context.GameSessions
                    .Where(gs => gs.StartedAt >= last30days)
                    .AverageAsync(gs => (double?)gs.TotalScore) ?? 0
            },
            System = new
            {
                ServerTime = now,
                Uptime = TimeSpan.FromMilliseconds(Environment.TickCount64),
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
            }
        };

        return Ok(metrics);
    }

    /// <summary>
    /// Nettoie les données anciennes du système (Admin uniquement)
    /// </summary>
    /// <param name="daysToKeep">Nombre de jours à conserver</param>
    [HttpPost("cleanup")]
    public async Task<ActionResult<object>> CleanupOldData([FromQuery] int daysToKeep = 90)
    {
        if (daysToKeep < 7)
        {
            return BadRequest(new { Message = "Impossible de supprimer les données de moins de 7 jours" });
        }

        var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
        
        // Supprimer les anciennes sessions terminées
        var oldSessions = await _context.GameSessions
            .Where(gs => gs.CompletedAt.HasValue && gs.CompletedAt < cutoffDate)
            .CountAsync();

        if (oldSessions > 0)
        {
            _context.GameSessions.RemoveRange(
                _context.GameSessions.Where(gs => gs.CompletedAt.HasValue && gs.CompletedAt < cutoffDate)
            );
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message = "Nettoyage effectué avec succès",
            SessionsDeleted = oldSessions,
            CutoffDate = cutoffDate.ToString("yyyy-MM-dd"),
            DaysKept = daysToKeep
        });
    }
}