using SharedModels.Models;
using DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace GameAPI.Services;

/// <summary>
/// Service pour la gestion des sessions de jeu
/// Gère le cycle de vie complet d'une partie : création, actions, progression, fin
/// </summary>
public class GameSessionService : IGameSessionService
{
    private readonly GameDbContext _context;
    private readonly IDungeonGeneratorService _dungeonGenerator;
    private readonly Random _random;

    public GameSessionService(GameDbContext context, IDungeonGeneratorService dungeonGenerator)
    {
        _context = context;
        _dungeonGenerator = dungeonGenerator;
        _random = new Random();
    }

    /// <summary>
    /// Démarre une nouvelle aventure pour un joueur
    /// </summary>
    public async Task<GameSession> StartNewAdventureAsync(int playerId, int difficultyLevel = 2)
    {
        // Vérifier que le joueur existe
        var player = await _context.Players.FindAsync(playerId);
        if (player == null)
        {
            throw new ArgumentException("Joueur non trouvé", nameof(playerId));
        }

        // Terminer toute session active du joueur
        var activeSessions = await _context.GameSessions
            .Where(gs => gs.PlayerId == playerId && gs.Status == GameStatus.Active)
            .ToListAsync();

        foreach (var session in activeSessions)
        {
            session.Status = GameStatus.Failed;
            session.CompletedAt = DateTime.UtcNow;
        }

        // Générer un nouveau donjon
        var dungeon = await _dungeonGenerator.GenerateDungeonAsync(difficultyLevel);

        // Créer la nouvelle session avec stats roguelike par défaut
        var gameSession = new GameSession
        {
            PlayerId = playerId,
            DungeonId = dungeon.Id,
            CurrentRoomNumber = 1,
            CurrentScore = 0,
            Status = GameStatus.Active,
            StartedAt = DateTime.UtcNow,
            
            // Stats roguelike - identiques pour chaque nouvelle aventure
            CurrentHP = 100,
            MaxHP = 100,
            AttackDamage = 20,
            Defense = 5,
            Level = 1,
            ExperiencePoints = 0
        };

        _context.GameSessions.Add(gameSession);
        await _context.SaveChangesAsync();

        return gameSession;
    }

    /// <summary>
    /// Récupère une session de jeu active avec ses détails
    /// </summary>
    public async Task<GameSession?> GetActiveSessionAsync(int sessionId)
    {
        return await _context.GameSessions
            .Include(gs => gs.Player)
            .Include(gs => gs.Dungeon)
            .Include(gs => gs.Actions)
            .FirstOrDefaultAsync(gs => gs.Id == sessionId && gs.Status == GameStatus.Active);
    }

    /// <summary>
    /// Récupère une session de jeu (quel que soit son statut) avec ses détails
    /// </summary>
    public async Task<GameSession?> GetSessionAsync(int sessionId)
    {
        return await _context.GameSessions
            .Include(gs => gs.Player)
            .Include(gs => gs.Dungeon)
            .Include(gs => gs.Actions)
            .FirstOrDefaultAsync(gs => gs.Id == sessionId);
    }

    /// <summary>
    /// Effectue une action dans la salle actuelle
    /// </summary>
    public async Task<GameAction> PerformActionAsync(int sessionId, ActionType actionType)
    {
        var session = await GetActiveSessionAsync(sessionId);
        if (session == null)
        {
            throw new InvalidOperationException("Session de jeu non trouvée ou inactive");
        }

        var currentRoom = await GetCurrentRoomAsync(sessionId);
        if (currentRoom == null)
        {
            throw new InvalidOperationException("Salle actuelle non trouvée");
        }

        // Calculer le résultat de l'action
        var actionResult = CalculateActionResult(actionType, currentRoom);

        // Appliquer les dégâts de HP
        session.CurrentHP -= actionResult.HPDamage;
        if (session.CurrentHP < 0) session.CurrentHP = 0;

        // Gagner de l'XP et vérifier le level up
        var oldLevel = session.Level;
        session.ExperiencePoints += actionResult.XPGained;
        CheckLevelUp(session);
        var leveledUp = session.Level > oldLevel;

        // Créer l'action
        var gameAction = new GameAction
        {
            GameSessionId = sessionId,
            RoomId = currentRoom.Id,
            ActionType = actionType,
            IsSuccessful = actionResult.IsSuccessful,
            PointsEarned = actionResult.PointsEarned,
            ResultDescription = actionResult.Description,
            ItemFound = actionResult.ItemFound,
            ActionTime = DateTime.UtcNow
        };

        // Ajouter feedback de level up
        if (leveledUp)
        {
            gameAction.ResultDescription += $" 🎆 NIVEAU SUPÉRIEUR ! Vous êtes maintenant niveau {session.Level} ! (+10 HP Max, +5 Attaque, +2 Défense)";
        }
        
        // Ajouter feedback XP
        if (actionResult.XPGained > 0)
        {
            gameAction.ResultDescription += $" (+{actionResult.XPGained} XP)";
        }

        // Mettre à jour le score de la session
        session.CurrentScore += actionResult.PointsEarned;

        // Vérifier si le joueur meurt (HP = 0)
        if (session.CurrentHP <= 0)
        {
            session.Status = GameStatus.Dead;
            session.CompletedAt = DateTime.UtcNow;
            gameAction.ResultDescription += " ☠️ Vos points de vie tombent à zéro - vous mourez dans le donjon !";
            
            // Calculer le score final même en cas de mort
            CalculateFinalScore(session);
            
            // Mettre à jour le score du joueur
            var player = await _context.Players.FindAsync(session.PlayerId);
            if (player != null)
            {
                player.Score += session.TotalScore;
            }
        }

        _context.GameActions.Add(gameAction);
        await _context.SaveChangesAsync();

        return gameAction;
    }

    /// <summary>
    /// Passe à la salle suivante
    /// </summary>
    public async Task<bool> MoveToNextRoomAsync(int sessionId)
    {
        var session = await GetActiveSessionAsync(sessionId);
        if (session == null || session.Status != GameStatus.Active)
        {
            return false;
        }

        // Vérifier s'il reste des salles
        if (session.CurrentRoomNumber >= session.Dungeon!.TotalRooms)
        {
            // Fin du donjon - partie terminée avec succès
            session.Status = GameStatus.Completed;
            session.CompletedAt = DateTime.UtcNow;
            
            // Calculer le score final complet
            CalculateFinalScore(session);
            
            // Mettre à jour le score du joueur
            var player = await _context.Players.FindAsync(session.PlayerId);
            if (player != null)
            {
                player.Score += session.TotalScore;
            }
            
            await _context.SaveChangesAsync();
            return true;
        }

        session.CurrentRoomNumber++;
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Termine une session de jeu
    /// </summary>
    public async Task<GameSession> EndSessionAsync(int sessionId, GameStatus status)
    {
        var session = await GetActiveSessionAsync(sessionId);
        if (session == null)
        {
            throw new InvalidOperationException("Session non trouvée");
        }

        session.Status = status;
        session.CompletedAt = DateTime.UtcNow;

        // Calculer le score final pour tous les types de fin
        CalculateFinalScore(session);

        // Mettre à jour le score total du joueur pour toutes les fins de partie
        var player = await _context.Players.FindAsync(session.PlayerId);
        if (player != null)
        {
            player.Score += session.TotalScore;
        }

        await _context.SaveChangesAsync();
        return session;
    }

    /// <summary>
    /// Récupère la salle actuelle
    /// </summary>
    public async Task<Room?> GetCurrentRoomAsync(int sessionId)
    {
        var session = await _context.GameSessions.FindAsync(sessionId);
        if (session == null) return null;

        return await _context.Rooms
            .FirstOrDefaultAsync(r => r.DungeonId == session.DungeonId && 
                                    r.RoomNumber == session.CurrentRoomNumber);
    }

    /// <summary>
    /// Récupère l'historique des actions
    /// </summary>
    public async Task<List<GameAction>> GetSessionActionsAsync(int sessionId)
    {
        return await _context.GameActions
            .Where(ga => ga.GameSessionId == sessionId)
            .OrderBy(ga => ga.ActionTime)
            .ToListAsync();
    }

    /// <summary>
    /// Calcule le résultat d'une action basé sur la salle et le type d'action.
    /// Chaque type d'action a ses propres taux de succès, récompenses et pénalités.
    /// </summary>
    private ActionResult CalculateActionResult(ActionType actionType, Room room)
    {
        return actionType switch
        {
            ActionType.Combat => CalculateCombatResult(room),
            ActionType.Flee => CalculateFleeResult(room),
            ActionType.Search => CalculateSearchResult(room),
            ActionType.OpenChest => CalculateOpenChestResult(room),
            ActionType.Ignore => CalculateIgnoreResult(room),
            ActionType.Rest => CalculateRestResult(room),
            ActionType.Investigate => CalculateInvestigateResult(room),
            ActionType.Bypass => CalculateBypassResult(room),
            _ => new ActionResult(false, 0, 0, 0, "Action inconnue", null)
        };
    }

    private ActionResult CalculateCombatResult(Room room)
    {
        // Jet de réussite basé sur le taux de succès de la salle (variable selon difficulté)
        var success = _random.Next(1, 101) <= room.CombatSuccessRate;
        if (success)
        {
            // Victoire : récompense de base + variabilité aléatoire pour le fun
            var bonusPoints = _random.Next(-2, 5);
            var totalPoints = room.CombatReward + bonusPoints;
            var minorDamage = _random.Next(5, 15); // Même en victoire, on prend quelques dégâts (réalisme)
            var xpGain = 25; // Combat réussi = l'action la plus XP
            return new ActionResult(true, totalPoints, minorDamage, xpGain,
                $"⚔️ Victoire ! Vous vainquez le {room.EncounterType} et gagnez {totalPoints} points ! (-{minorDamage} HP)", 
                _random.Next(1, 5) == 1 ? "Potion de soin" : null);
        }
        else
        {
            var penalty = -_random.Next(3, 8);
            var damage = _random.Next(15, 30); // Dégâts importants en cas de défaite
            var xpGain = 10; // On gagne quand même un peu d'XP de l'expérience
            return new ActionResult(false, penalty, damage, xpGain,
                $"💀 Défaite ! Le {room.EncounterType} vous blesse gravement. Vous perdez {Math.Abs(penalty)} points et {damage} HP.", 
                null);
        }
    }

    private ActionResult CalculateFleeResult(Room room)
    {
        return new ActionResult(true, room.FleeReward, 0, 5,
            $"🏃 Vous fuyez prudemment et gagnez {room.FleeReward} points en sécurité.", 
            null);
    }    private ActionResult CalculateSearchResult(Room room)
    {
        var success = _random.Next(1, 101) <= room.SearchSuccessRate;
        if (success)
        {
            var items = new[] { "Potion magique", "Pièces d'or", "Gemme précieuse", "Parchemin ancien" };
            var item = items[_random.Next(items.Length)];
            return new ActionResult(true, room.SearchReward, 0, 15,
                $"🔍 Bonne trouvaille ! Vous découvrez {item} et gagnez {room.SearchReward} points !", 
                item);
        }
        else
        {
            var damage = _random.Next(10, 20); // Dégâts de piège
            return new ActionResult(false, room.SearchPenalty, damage, 8,
                $"💥 Piège ! Votre fouille déclenche un mécanisme. Vous perdez {Math.Abs(room.SearchPenalty)} points et {damage} HP.", 
                null);
        }
    }

    private ActionResult CalculateOpenChestResult(Room room)
    {
        var success = _random.Next(1, 101) <= 70; // 70% de chance de succès
        if (success)
        {
            var points = _random.Next(10, 25);
            return new ActionResult(true, points, 0, 20,
                $"💰 Trésor ! Le coffre contient des richesses. Vous gagnez {points} points !", 
                "Trésor précieux");
        }
        else
        {
            var penalty = -_random.Next(5, 12);
            var damage = _random.Next(20, 35); // Coffre piégé fait mal !
            return new ActionResult(false, penalty, damage, 12,
                $"🪤 Coffre piégé ! Vous subissez {damage} dégâts et perdez {Math.Abs(penalty)} points.", 
                null);
        }
    }

    private ActionResult CalculateIgnoreResult(Room room)
    {
        return new ActionResult(true, 1, 0, 3,
            "➡️ Vous ignorez les dangers et passez directement. Vous gagnez 1 point de prudence.", 
            null);
    }

    private ActionResult CalculateRestResult(Room room)
    {
        // Se reposer guérit toujours et donne de l'XP
        var healing = _random.Next(20, 40);
        return new ActionResult(true, 5, -healing, 8, // Negative damage = healing
            $"🛌 Vous vous reposez près de la {room.EncounterType}. Vous récupérez {healing} points de santé !",
            _random.Next(1, 3) == 1 ? "Énergie Restaurée" : null);
    }

    private ActionResult CalculateInvestigateResult(Room room)
    {
        // Investigation : mécanique risk/reward - 40% succès mais grosses récompenses, ou dégâts sévères
        var success = _random.Next(1, 101) <= 40;
        if (success)
        {
            var bonusPoints = _random.Next(15, 35);
            return new ActionResult(true, bonusPoints, 0, 30,
                $"🔮 Investigation réussie ! Vous découvrez les secrets de {room.EncounterType} et gagnez {bonusPoints} points !",
                "Savoir Ancien");
        }
        else
        {
            var damage = _random.Next(25, 50); // Très dangereux !
            var penalty = -_random.Next(5, 15);
            return new ActionResult(false, penalty, damage, 12,
                $"⚡ L'investigation tourne mal ! {room.EncounterType} vous inflige {damage} dégâts et vous perdez {Math.Abs(penalty)} points.",
                null);
        }
    }

    private ActionResult CalculateBypassResult(Room room)
    {
        // Contournement sûr mais peu récompensant
        return new ActionResult(true, 8, 0, 10,
            $"🚶 Vous contournez prudemment {room.EncounterType} et gagnez 8 points de prudence.",
            null);
    }
    
    /// <summary>
    /// Calcule le score final avec formule complexe multi-facteurs :
    /// Score = BaseScore + BonusXP + BonusExploration * DifficultyMultiplier + BonusStatus + BonusLevel
    /// </summary>
    private void CalculateFinalScore(GameSession session)
    {
        var baseScore = session.CurrentScore;
        var bonusScore = 0;
        
        // Facteur 1 : XP (1 point par XP gagné)
        var xpBonus = session.ExperiencePoints;
        bonusScore += xpBonus;
        
        // Facteur 2 : Exploration (50 points par salle)
        var roomsExplored = session.Status == GameStatus.Completed 
            ? session.Dungeon?.TotalRooms ?? session.CurrentRoomNumber
            : session.CurrentRoomNumber - 1;
        var explorationBonus = roomsExplored * 50;
        bonusScore += explorationBonus;
        
        // Facteur 3 : Multiplicateur de difficulté (+20% par niveau)
        var difficultyMultiplier = session.Dungeon?.DifficultyLevel ?? 1;
        bonusScore = (int)(bonusScore * (1 + difficultyMultiplier * 0.2));
        
        // Facteur 4 : Bonus de statut final (philosophy: encourager les tentatives, récompenser la victoire)
        switch (session.Status)
        {
            case GameStatus.Completed:
                bonusScore += 500; // Bonus victoire fixe
                var survivalBonus = (int)(session.CurrentHP * 2); // Bonus survie : 2 pts/HP
                bonusScore += survivalBonus;
                break;
                
            case GameStatus.Dead:
                // Design choice : pas de pénalité pour encourager exploration risquée
                break;
                
            case GameStatus.Failed:
                bonusScore = (int)(bonusScore * 0.8); // Pénalité abandon : -20%
                break;
        }
        
        // Bonus de niveau atteint : 100 points par niveau au-dessus de 1
        var levelBonus = (session.Level - 1) * 100;
        bonusScore += levelBonus;
        
        // Mettre à jour le score final
        session.CurrentScore = baseScore + bonusScore;
        session.TotalScore = session.CurrentScore; // Assurer que TotalScore est aussi mis à jour
        
        // S'assurer que le score ne soit jamais négatif
        if (session.CurrentScore < 0)
        {
            session.CurrentScore = 0;
            session.TotalScore = 0;
        }
    }

    /// <summary>
    /// Vérifie et applique le level up avec bonus de stats.
    /// Utilise une boucle while pour gérer les level-ups multiples (si beaucoup d'XP d'un coup).
    /// </summary>
    private void CheckLevelUp(GameSession session)
    {
        var requiredXP = GetRequiredXPForLevel(session.Level + 1);
        
        // Boucle pour gérer plusieurs level-ups consécutifs
        while (session.ExperiencePoints >= requiredXP)
        {
            session.Level++;
            
            // Stats progression : +10 MaxHP, +10 heal, +5 ATK, +2 DEF par niveau
            session.MaxHP += 10;
            session.CurrentHP += 10; // Effet "guérison" lors du level-up
            session.AttackDamage += 5;
            session.Defense += 2;
            
            // Calculer XP requis pour le prochain niveau
            requiredXP = GetRequiredXPForLevel(session.Level + 1);
        }
    }
    
    /// <summary>
    /// Calcule l'XP requis pour atteindre un niveau donné.
    /// Formule progressive : f(n) = (n-1)*50 + (n-2)*25
    /// Exemples : Lvl 1→0 XP, Lvl 2→50 XP, Lvl 3→150 XP, Lvl 4→300 XP (courbe exponentielle)
    /// </summary>
    private int GetRequiredXPForLevel(int level)
    {
        return level <= 1 ? 0 : (level - 1) * 50 + (level - 2) * 25;
    }

    /// <summary>
    /// Résultat d'une action
    /// </summary>
    private record ActionResult(bool IsSuccessful, int PointsEarned, int HPDamage, int XPGained, string Description, string? ItemFound);
}