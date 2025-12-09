# Guide de configuration Keycloak pour BlazorGameQuest

## Vue d'ensemble
Ce guide détaille la configuration complète de Keycloak pour le projet BlazorGameQuest, incluant la création du realm, des clients, des utilisateurs et des rôles.

## Prérequis
- Docker et Docker Compose installés
- Projet BlazorGameQuest cloné
- Keycloak accessible sur `http://localhost:8180`

## Étape 1 : Démarrage de Keycloak

### 1.1 Lancement via Docker Compose
```bash
cd BlazorGameQuest1234
docker-compose up keycloak -d
```

### 1.2 Vérification du démarrage
- Accéder à `http://localhost:8180`
- Interface d'administration Keycloak doit être accessible

## Étape 2 : Configuration initiale de Keycloak

### 2.1 Première connexion admin
1. Accéder à `http://localhost:8180`
2. Cliquer sur "Administration Console"
3. Utiliser les identifiants par défaut :
   - **Username** : `admin`
   - **Password** : `admin`

### 2.2 Création du Realm
1. Dans la console d'administration, survoler "Master" en haut à gauche
2. Cliquer sur "Create Realm"
3. Configurer le nouveau realm :
   - **Realm name** : `blazor-gamequest`
   - **Enabled** : `True`
4. Cliquer sur "Create"

## Étape 3 : Configuration du Client

### 3.1 Création du client Blazor
1. Dans le realm `blazor-gamequest`, aller à "Clients"
2. Cliquer sur "Create client"
3. Configuration général :
   - **Client type** : `OpenID Connect`
   - **Client ID** : `blazor-game-client`
4. Cliquer "Next"

### 3.2 Configuration des capacités
1. **Client authentication** : `Off` (Public client pour Blazor WebAssembly)
2. **Authorization** : `Off`
3. **Authentication flow** :
   - ✅ Standard flow
   - ✅ Implicit flow (pour Blazor WebAssembly)
   - ❌ Direct access grants
   - ❌ Service accounts roles

### 3.3 Configuration des URLs
1. **Valid redirect URIs** :
   ```
   http://localhost:5002/*
   http://blazor-client:5002/*
   ```
2. **Valid post logout redirect URIs** :
   ```
   http://localhost:5002/*
   http://blazor-client:5002/*
   ```
3. **Web origins** :
   ```
   http://localhost:5002
   http://blazor-client:5002
   ```

### 3.4 Configuration avancée
1. Dans l'onglet "Advanced" du client :
   - **Access Token Lifespan** : `30 minutes`
   - **Client Offline Session Idle Timeout** : `1 hour`

## Étape 4 : Configuration des Rôles

### 4.1 Création des rôles realm
1. Aller à "Realm roles"
2. Cliquer "Create role"
3. Créer les rôles suivants :

**Rôle Administrateur :**
- **Role name** : `administrateur`
- **Description** : `Administrateur avec accès complet`

**Rôle Joueur :**
- **Role name** : `joueur`
- **Description** : `Joueur standard avec accès aux fonctionnalités de jeu`

### 4.2 Configuration des rôles par défaut
1. Aller à "Realm settings" > "User registration"
2. Dans "Default roles", ajouter `joueur` comme rôle par défaut

## Étape 5 : Création des Utilisateurs

### 5.1 Utilisateur 1 (Joueur)
1. Aller à "Users" > "Create new user"
2. Configuration :
   - **Username** : `user1`
   - **Email** : `user1@blazorgame.local`
   - **First name** : `Joueur`
   - **Last name** : `Un`
   - **Enabled** : `True`
3. Cliquer "Create"

**Définir le mot de passe :**
1. Aller à l'onglet "Credentials"
2. Cliquer "Set password"
3. **Password** : `1234`
4. **Temporary** : `False`
5. Cliquer "Save"

**Assigner les rôles :**
1. Aller à l'onglet "Role mapping"
2. Cliquer "Assign role"
3. Sélectionner `joueur`
4. Cliquer "Assign"

### 5.2 Utilisateur 2 (Joueur)
1. Créer un nouvel utilisateur :
   - **Username** : `user2`
   - **Email** : `user2@blazorgame.local`
   - **First name** : `Joueur`
   - **Last name** : `Deux`
   - **Enabled** : `True`

2. Définir le mot de passe :
   - **Password** : `1234`
   - **Temporary** : `False`

3. Assigner le rôle `joueur`

### 5.3 Utilisateur Admin
1. Créer un nouvel utilisateur :
   - **Username** : `admin`
   - **Email** : `admin@blazorgame.local`
   - **First name** : `Admin`
   - **Last name** : `Système`
   - **Enabled** : `True`

2. Définir le mot de passe :
   - **Password** : `admin`
   - **Temporary** : `False`

3. Assigner les rôles :
   - `administrateur`
   - `joueur` (pour permettre l'accès au jeu)

## Étape 6 : Configuration des Token Claims

### 6.1 Mapper les rôles dans les tokens
1. Dans le client `blazor-game-client`, aller à "Client scopes"
2. Cliquer sur `blazor-game-client-dedicated`
3. Aller à l'onglet "Mappers"
4. Cliquer "Add mapper" > "By configuration" > "User Realm Role"

Configuration du mapper :
- **Name** : `realm-roles`
- **User Realm Role** : `True`
- **Token Claim Name** : `roles`
- **Claim JSON Type** : `String`
- **Add to ID token** : `True`
- **Add to access token** : `True`
- **Add to userinfo** : `True`

## Étape 7 : Test de la Configuration

### 7.1 Test des utilisateurs
1. Se déconnecter de la console admin
2. Tester chaque utilisateur :
   - `user1` / `1234`
   - `user2` / `1234` 
   - `admin` / `admin`

### 7.2 Vérification des rôles
1. Pour chaque utilisateur connecté, vérifier dans "Account Console" que les rôles sont bien assignés

## Étape 8 : Initialisation de la base de données

### 8.1 Création des joueurs dans la base de données
Après la configuration Keycloak, il faut initialiser la base de données avec les 2 joueurs correspondant aux comptes utilisateurs Keycloak :

**Joueur 1 :**
- **UserID** : Correspond au user1 de Keycloak
- **Username** : `user1`
- **Lié au compte** : user1@blazorgame.local

**Joueur 2 :**
- **UserID** : Correspond au user2 de Keycloak  
- **Username** : `user2`
- **Lié au compte** : user2@blazorgame.local

### 8.2 Synchronisation automatique
L'application se chargera de créer automatiquement les entrées de joueurs dans la base de données lors de la première connexion de chaque utilisateur.

## Étape 9 : Démarrage complet de l'application

### 9.1 Lancement de tous les services
```bash
docker-compose up -d
```

### 9.2 Vérification des services
- **Keycloak** : `http://localhost:8180`
- **API Gateway** : `http://localhost:5000`
- **Game API** : `http://localhost:5001` (via gateway)
- **Blazor Client** : `http://localhost:5002`

### 9.3 Test de connexion dans l'application
1. Accéder à `http://localhost:5002`
2. Cliquer sur "Se connecter"
3. Utiliser l'un des comptes créés
4. Vérifier que l'utilisateur est redirigé vers l'application avec les bonnes permissions

## Configuration réseau Docker

Les services communiquent via le réseau Docker `appnet` :
- **keycloak** : Port 8180
- **gameapi** : Port 5001
- **apigateway** : Port 5000 (HTTP uniquement)
- **blazor-client** : Port 5002

⚠️ **Important** : La Gateway fonctionne uniquement sur le port 5000 et accepte seulement le protocole HTTP comme spécifié dans les exigences du projet.

## Dépannage

### Problème : Keycloak ne démarre pas
- Vérifier que le port 8180 n'est pas utilisé
- Redémarrer Docker : `docker-compose down && docker-compose up -d`

### Problème : Erreur de redirection
- Vérifier les "Valid redirect URIs" du client
- S'assurer que les URLs incluent le protocole http://

### Problème : Token invalide
- Vérifier la configuration des mappers
- S'assurer que les rôles sont bien assignés aux utilisateurs

### Problème : CORS
- Vérifier les "Web origins" du client
- S'assurer que l'ApiGateway est configuré pour les bonnes origines

## Sécurité en Production

⚠️ **Important** : Cette configuration est pour le développement uniquement.

Pour la production :
1. Changer tous les mots de passe par défaut
2. Configurer HTTPS partout
3. Utiliser des secrets externes pour les mots de passe
4. Activer l'audit et les logs de sécurité
5. Configurer la rotation des tokens