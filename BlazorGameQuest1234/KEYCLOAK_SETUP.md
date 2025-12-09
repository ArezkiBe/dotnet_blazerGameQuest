# Configuration Keycloak - BlazorGameQuest

## Démarrage

```bash
cd BlazorGameQuest1234
docker-compose up keycloak -d
```

Accéder à http://localhost:8180 - Identifiants: **admin/admin**

## 1. Créer le Realm

1. Survoler "Master" → "Create Realm"
2. **Realm name:** `blazor-gamequest`
3. **Enabled:** True
4. Create

## 2. Créer le Client

1. Clients → "Create client"
2. **Client ID:** `blazor-client`
3. **Client authentication:** Off (public client)
4. **Authentication flow:** Standard flow + Implicit flow + Direct access grants
5. **Valid redirect URIs:** `http://localhost:5000/*`
6. **Valid post logout redirect URIs:** `http://localhost:5000/*`
7. **Web origins:** `http://localhost:5000`
8. Save

## 3. Créer les Rôles

Realm roles → Create role:

- **administrateur** - Accès complet
- **joueur** - Accès au jeu

## 4. Créer les Utilisateurs

### User1 (Joueur)
1. Users → Create new user
2. **Username:** `user1`, **Email:** `user1@blazorgame.local`
3. Create
4. Credentials → Set password: `1234` (Temporary: False)
5. Role mapping → Assign role: `joueur`

### User2 (Joueur)
1. **Username:** `user2`, **Email:** `user2@blazorgame.local`
2. Password: `1234` (Temporary: False)
3. Role: `joueur`

### Admin
1. **Username:** `admin`, **Email:** `admin@blazorgame.local`
2. Password: `admin` (Temporary: False)
3. Roles: `administrateur` + `joueur`

## 5. Configuration Token Claims

1. Client `blazor-client` → Client scopes → `blazor-client-dedicated`
2. Mappers → Add mapper → User Realm Role
3. **Name:** `realm-roles`
4. **Token Claim Name:** `roles`
5. **Add to ID token:** True
6. **Add to access token:** True
7. Save

## 6. Démarrer l'Application

```bash
docker-compose up -d
```

**Vérifications:**
- Keycloak: http://localhost:8180
- Application: http://localhost:5000

## Architecture Réseau

| Service | Port | Accès |
|---------|------|-------|
| keycloak | 8180 | Public |
| apigateway | 5000 | Public (HTTP uniquement) |
| gameapi | 5001 | Interne uniquement |
| blazor-client | 80 | Interne uniquement |

⚠️ **Important:** L'accès se fait via le Gateway (port 5000), pas directement sur les services internes.

## Dépannage

**Keycloak ne démarre pas:** Vérifier port 8180 libre  
**Erreur de redirection:** Vérifier les redirect URIs  
**Token invalide:** Vérifier les rôles assignés et les mappers
