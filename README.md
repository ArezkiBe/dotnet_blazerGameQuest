# BlazorGameQuest Version 1

## Architecture du Projet

### Structure de la Solution
```
BlazorGameQuest1234/
├── BlazorGame.Client/          # Application Blazor WebAssembly (Frontend)
├── AuthenticationServices/     # API Web pour les services d'authentification
├── SharedModels/              # Modèles partagés entre les projets
└── BlazorGameQuest.Tests/     # Tests unitaires
```

### Technologies Utilisées
- **.NET 9** - Framework principal
- **Blazor WebAssembly** - Interface utilisateur interactive
- **ASP.NET Core Web API** - Services backend
- **xUnit** - Framework de tests

## Fonctionnalités (Version 1)

### Pages Principales
- **Accueil** (`/`) - Page d'accueil avec présentation du jeu
- **Aventure** (`/adventure`) - Page de démarrage de l'aventure
- **Jeu** (`/game`) - Interface principale de jeu avec actions
- **À Propos** (`/about`) - Informations sur le projet

### Mécaniques de Jeu
- **Exploration** - Navigation entre différentes salles
- **Combat** - Système de combat basique contre les ennemis
- **Fuite** - Possibilité d'éviter les combats
- **Recherche** - Recherche d'objets et de trésors
- **Interface responsive** - Adaptée aux différentes tailles d'écran

### Services Backend
- **API de Santé** - Endpoint `/health` pour vérifier l'état du service
- **Swagger/OpenAPI** - Documentation interactive de l'API
- **Architecture prête** pour l'authentification (Version 2)

## Installation et Lancement

### Prérequis
- .NET 9 SDK


## Auteurs

Développé dans le cadre du cours .NET - Semestre 7
EFREI Paris - Promotion LSI3

Membres du groupe:
Arezki BEGGAR
Ilyas HADJADJ

---