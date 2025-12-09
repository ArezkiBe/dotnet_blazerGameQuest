# Guide de Déploiement Docker - BlazorGameQuest

## Vue d'ensemble
Ce guide détaille le déploiement complet de l'application BlazorGameQuest avec Docker, incluant tous les services nécessaires.

## Architecture de déploiement

### Services déployés
- **Keycloak** : Serveur d'authentification (Port 8180)
- **GameAPI** : API REST backend (Port 5001)
- **ApiGateway** : Passerelle YARP (Port 5000) 
- **BlazorClient** : Interface utilisateur WebAssembly (Port 5002)

### Réseau Docker
Tous les services communiquent via le réseau `appnet` pour une isolation et sécurité optimales.

## Prérequis

### Système
- **Docker Desktop** : Version 20.10 ou supérieure
- **Docker Compose** : Version 2.0 ou supérieure
- **Ports disponibles** : 5000, 5001, 5002, 8180

### Vérification des prérequis
```bash
docker --version
docker-compose --version
```

## Étape 1 : Préparation de l'environnement

### 1.1 Clone et navigation
```bash
git clone <your-repo-url>
cd BlazorGameQuest1234
```

### 1.2 Vérification de la structure
```bash
ls -la
# Vérifier la présence de :
# - docker-compose.yml
# - Dockerfiles dans chaque projet
# - appsettings.json correctement configurés
```

## Étape 2 : Construction des images

### 2.1 Construction de toutes les images
```bash
docker-compose build
```

### 2.2 Vérification des images créées
```bash
docker images | grep blazorgame
```

Vous devriez voir :
- `blazorgamequestcompose-gameapi`
- `blazorgamequestcompose-apigateway`  
- `blazorgamequestcompose-blazor-client`

## Étape 3 : Déploiement des services

### 3.1 Démarrage de Keycloak en premier
```bash
docker-compose up keycloak -d
```

### 3.2 Attendre l'initialisation de Keycloak
```bash
# Vérifier que Keycloak est prêt
docker-compose logs keycloak

# Attendre le message : "Keycloak ... started"
```

### 3.3 Configuration de Keycloak
Suivre le guide `KEYCLOAK_SETUP.md` pour configurer :
- Le realm `blazor-gamequest`
- Le client `blazor-game-client`  
- Les utilisateurs et rôles

### 3.4 Démarrage de tous les services
```bash
docker-compose up -d
```

## Étape 4 : Vérification du déploiement

### 4.1 État des conteneurs
```bash
docker-compose ps
```

Tous les services doivent être dans l'état `running`.

### 4.2 Vérification des logs
```bash
# Logs de tous les services
docker-compose logs

# Logs d'un service spécifique
docker-compose logs gameapi
docker-compose logs apigateway
docker-compose logs blazor-client
```

### 4.3 Test des endpoints

**Keycloak :**
```bash
curl http://localhost:8180/realms/blazor-gamequest/.well-known/openid_configuration
```

**API Gateway :**
```bash
curl http://localhost:5000/health
```

**Game API (via Gateway) :**
```bash
curl http://localhost:5000/api/health
```

**Blazor Client :**
```bash
curl -I http://localhost:5002
```

## Étape 5 : Test de l'application complète

### 5.1 Accès à l'interface
1. Ouvrir `http://localhost:5002`
2. Vérifier que l'interface Blazor se charge
3. Tester le bouton de connexion

### 5.2 Test d'authentification
1. Se connecter avec `user1` / `password123`
2. Vérifier la redirection après connexion
3. Tester l'accès aux fonctionnalités du jeu

### 5.3 Test des API
1. Une fois connecté, ouvrir les outils développeur
2. Vérifier que les appels API fonctionnent
3. Tester la création d'une nouvelle partie

## Configuration réseau

### Communication inter-services
Les services utilisent les noms Docker pour communiquer :

**Dans ApiGateway (appsettings.json) :**
```json
{
  "ReverseProxy": {
    "Clusters": {
      "GameAPI": {
        "Destinations": {
          "gameapi": {
            "Address": "http://gameapi:5001/"
          }
        }
      },
      "BlazorClient": {
        "Destinations": {
          "blazor-client": {
            "Address": "http://blazor-client:5002/"
          }
        }
      }
    }
  }
}
```

**Dans BlazorClient (Program.cs) :**
```csharp
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri("http://apigateway:5000/")
});
```

## Commandes utiles

### Gestion des services
```bash
# Arrêt de tous les services
docker-compose down

# Redémarrage d'un service spécifique
docker-compose restart gameapi

# Reconstruction et redémarrage
docker-compose up --build -d

# Suppression complète (attention : perd les données)
docker-compose down -v --remove-orphans
```

### Débogage
```bash
# Inspection d'un conteneur
docker inspect blazorgamequestcompose-gameapi-1

# Accès shell dans un conteneur
docker-compose exec gameapi /bin/bash

# Surveillance des logs en temps réel
docker-compose logs -f gameapi
```

### Nettoyage
```bash
# Suppression des images non utilisées
docker image prune

# Suppression complète des ressources du projet
docker-compose down -v --remove-orphans --rmi all
```

## Dépannage

### Problème : Port déjà utilisé
```bash
# Vérifier les ports en cours d'utilisation
netstat -tulpn | grep :5000

# Arrêter le processus occupant le port
kill -9 <PID>
```

### Problème : Service ne démarre pas
1. Vérifier les logs : `docker-compose logs <service>`
2. Vérifier la configuration réseau
3. Redémarrer le service : `docker-compose restart <service>`

### Problème : Erreur de connexion entre services
1. Vérifier que tous les services sont sur le même réseau
2. Utiliser les noms de services Docker (pas localhost)
3. Vérifier les ports internes (pas les ports exposés)

### Problème : Keycloak non accessible
1. Attendre l'initialisation complète (peut prendre 1-2 minutes)
2. Vérifier les logs : `docker-compose logs keycloak`
3. Redémarrer si nécessaire : `docker-compose restart keycloak`

## Surveillance et maintenance

### Monitoring des ressources
```bash
# Utilisation CPU/Mémoire par conteneur
docker stats

# Espace disque utilisé par Docker
docker system df
```

### Backup des données
```bash
# Export des volumes (si utilisation de base de données persistante)
docker-compose exec gameapi tar -czf /tmp/backup.tar.gz /app/data
docker cp container_id:/tmp/backup.tar.gz ./backup.tar.gz
```

## Production

⚠️ **Cette configuration est pour le développement uniquement.**

Pour la production :
1. Utiliser HTTPS partout
2. Configurer des secrets externes
3. Utiliser des bases de données persistantes
4. Configurer la surveillance et les logs
5. Mettre en place des health checks
6. Configurer les limites de ressources
7. Utiliser des registres d'images privés