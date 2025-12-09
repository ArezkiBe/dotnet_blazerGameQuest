# Déploiement Docker - BlazorGameQuest

## Architecture

| Service | Port | Description |
|---------|------|-------------|
| Keycloak | 8180 | Authentification (public) |
| ApiGateway | 5000 | Point d'entrée HTTP unique (public) |
| GameAPI | 5001 | API REST (interne) |
| BlazorClient | 80 | Frontend (interne) |

Tous les services communiquent via le réseau Docker `appnet`.

## Démarrage Rapide

```bash
cd BlazorGameQuest1234
docker-compose up -d
```

## Étapes Détaillées

### 1. Démarrer Keycloak

```bash
docker-compose up keycloak -d
docker-compose logs -f keycloak  # Attendre "Keycloak ... started"
```

### 2. Configurer Keycloak

Suivre [KEYCLOAK_SETUP.md](KEYCLOAK_SETUP.md) pour:
- Créer realm `blazor-gamequest`
- Créer client `blazor-client`
- Créer utilisateurs (user1/1234, user2/1234, admin/admin)

### 3. Démarrer tous les services

```bash
docker-compose up -d
```

### 4. Vérifier

```bash
# État des conteneurs
docker-compose ps

# Logs
docker-compose logs -f

# Test Keycloak
curl http://localhost:8180/realms/blazor-gamequest/.well-known/openid-configuration

# Test Gateway
curl -I http://localhost:5000
```

## Test de l'Application

1. Ouvrir http://localhost:5000
2. Se connecter avec:
   - **user1/1234** (joueur)
   - **user2/1234** (joueur)
   - **admin/admin** (administrateur)
3. Tester les fonctionnalités

## Configuration Réseau

**Communication interne (docker-compose.yml):**
```yaml
Gateway → GameAPI:     http://gameapi:5001
Gateway → BlazorClient: http://blazor-client:80
Services → Keycloak:    http://keycloak:8080
```

**Accès externe:**
- Client → Gateway: `http://localhost:5000` (HTTP uniquement)
- Client → Keycloak: `http://localhost:8180`

## Commandes Utiles

```bash
# Arrêter
docker-compose down

# Reconstruire
docker-compose up --build -d

# Logs service spécifique
docker-compose logs -f gameapi

# Nettoyer complètement
docker-compose down -v --remove-orphans

# Ressources
docker stats
```

## Dépannage

**Conteneurs ne démarrent pas:**
```bash
docker-compose logs
docker-compose down -v
docker-compose up --build -d
```

**Port 5000 occupé:**
```powershell
Get-Process -Id (Get-NetTCPConnection -LocalPort 5000).OwningProcess
```

**Keycloak non accessible:** Attendre 1-2 minutes après démarrage

**Gateway renvoie 404:** Vérifier que GameAPI et BlazorClient sont démarrés

## Production

⚠️ Configuration pour développement uniquement.

Pour production:
- Utiliser HTTPS
- Secrets externes
- Base de données persistante
- Health checks
- Limites de ressources
