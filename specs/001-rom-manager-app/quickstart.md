# Quick Start Guide: ROM Manager Application

**Date**: 2025-01-27 (Mis à jour: 2025-12-12)  
**Feature**: [spec.md](./spec.md)

**Note** : Guide mis à jour suite aux clarifications du 2025-12-12 (gestionnaire de bases de données intégré, filtres d'exclusion configurables, scans incrémentaux, export SSH/SFTP).

## Prerequisites

### Development Environment

1. **.NET 8.0 SDK** (ou supérieur)
   - Téléchargement : https://dotnet.microsoft.com/download
   - Vérification : `dotnet --version`

2. **IDE** (au choix)
   - Visual Studio 2022 (Windows/Mac)
   - JetBrains Rider (multi-plateforme)
   - Visual Studio Code avec extensions C# et Avalonia

3. **Git** (pour cloner le repository)

### Platform-Specific Requirements

#### Linux
- Runtime .NET 8.0
- Bibliothèques système : `libgtk-3-dev`, `libwebkit2gtk-4.0-dev` (pour Avalonia)

#### Windows
- Runtime .NET 8.0
- Windows 10/11

#### macOS
- Runtime .NET 8.0
- macOS 10.15+

## Project Setup

### 1. Clone Repository

```bash
git clone <repository-url>
cd RomPilot
git checkout 001-rom-manager-app
```

### 2. Create Project Structure

```bash
# Créer solution
dotnet new sln -n RomPilot

# Créer projets
dotnet new classlib -n RomPilot.Core -o src/RomPilot.Core
dotnet new avalonia.mvvm -n RomPilot.UI -o src/RomPilot.UI
dotnet new xunit -n RomPilot.Tests -o src/RomPilot.Tests

# Ajouter projets à solution
dotnet sln add src/RomPilot.Core/RomPilot.Core.csproj
dotnet sln add src/RomPilot.UI/RomPilot.UI.csproj
dotnet sln add src/RomPilot.Tests/RomPilot.Tests.csproj

# Ajouter références
dotnet add src/RomPilot.UI reference src/RomPilot.Core
dotnet add src/RomPilot.Tests reference src/RomPilot.Core
```

### 3. Install NuGet Packages

#### RomPilot.Core

```bash
cd src/RomPilot.Core
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package SharpCompress
dotnet add package SSH.NET  # Pour export SSH/SFTP
```

#### RomPilot.UI

```bash
cd src/RomPilot.UI
# Avalonia packages inclus dans template
dotnet add package CommunityToolkit.Mvvm  # Pour MVVM helpers
```

#### RomPilot.Tests

```bash
cd src/RomPilot.Tests
dotnet add package Moq
dotnet add package FluentAssertions
dotnet add package Microsoft.EntityFrameworkCore.InMemory
```

### 4. Configure Database

```bash
cd src/RomPilot.Core
# Créer migration initiale
dotnet ef migrations add InitialCreate --startup-project ../RomPilot.UI
```

## Running the Application

### Development Mode

```bash
cd src/RomPilot.UI
dotnet run
```

### Build Release

```bash
# Build pour plateforme actuelle
dotnet build -c Release

# Publish pour Linux
dotnet publish -c Release -r linux-x64 --self-contained

# Publish pour Windows
dotnet publish -c Release -r win-x64 --self-contained

# Publish pour macOS
dotnet publish -c Release -r osx-x64 --self-contained
```

## First Steps

### 1. Télécharger Bases de Données de Référence

**NOUVEAU** : L'application inclut un gestionnaire intégré de bases de données.

1. Lancer l'application
2. Accéder à l'onglet "Bases de Données"
3. Parcourir les providers disponibles (NoIntro, Redump, GoodSet)
4. Sélectionner une console (ex: "nes", "snes")
5. Voir les versions disponibles avec dates et tailles
6. Télécharger la dernière version
7. Répéter pour chaque console de votre collection

**Exemple** : Télécharger NoIntro pour NES, SNES, Game Boy

### 2. Configurer Filtres d'Exclusion (Optionnel)

**NOUVEAU** : Configuration des filtres pour le scan.

1. Accéder à Paramètres → Filtres d'Exclusion
2. Voir la liste des extensions exclues par défaut :
   - Images : `.jpg`, `.png`, `.gif`, `.bmp`
   - Textes : `.txt`, `.nfo`, `.diz`
   - Exécutables : `.exe`, `.dll`, `.so`
   - Documents : `.doc`, `.pdf`, `.html`, `.xml`
3. Ajouter filtres personnalisés si nécessaire (ex: `.mp3`, `.avi`)
4. Activer/désactiver filtres individuellement
5. Sauvegarder

### 3. Ajouter Répertoires Sources et Scanner

1. Accéder à l'onglet "Scan"
2. Cliquer "Ajouter répertoire source"
3. Sélectionner répertoire contenant vos ROMs
4. **Choisir type de scan** :
   - **Scan complet** : Calcul de tous les checksums (premier scan)
   - **Scan rapide** : Détection changements via timestamp/taille (rescans)
5. Lancer le scan
6. Observer progression détaillée :
   - Fichiers scannés
   - Checksums calculés
   - ROMs identifiées vs non identifiées
   - Fichiers exclus (filtres)
   - Erreurs avec raisons explicites

**Résultat attendu** :
- Fichiers identifiés comme ROMs avec console et jeu
- Fichiers non identifiés listés avec checksums
- Fichiers exclus (selon filtres)

### 4. Configurer Préférences de Sélection

**NOUVEAU** : Préférences étendues avec format vidéo.

1. Accéder à Paramètres → Préférences
2. Configurer priorités **Région** : `EUR > USA > JAP`
3. Configurer priorités **Format Vidéo** : `PAL > NTSC > NTSC-J` (nouveau)
4. Configurer priorités **Langue** : `FR > EN > autres`
5. Activer "Exclure bad dumps" : ✓
6. Sauvegarder

**Effet** : Le système sélectionnera automatiquement la meilleure version pour chaque jeu selon ces critères.

### 5. Vérifier Regroupement et Sélection

1. Accéder à l'onglet "Bibliothèque" (nouveau)
2. Basculer en **vue grille** (vignettes) ou **vue liste** (tableau)
3. Utiliser filtres pour afficher jeux d'une console
4. Sélectionner un jeu avec plusieurs versions
5. Voir détails du jeu :
   - Toutes les versions détectées
   - Version sélectionnée automatiquement (highlight)
   - Métadonnées (si disponibles)
6. Changer manuellement la version sélectionnée si souhaité

### 6. Configurer Export (Local ou Distant)

**NOUVEAU** : Support export local ET SSH/SFTP.

#### Export Local

1. Accéder à Paramètres → Export → Nouvelle Configuration
2. Nom : "Recalbox Local"
3. Plateforme : Recalbox
4. Type : Local
5. Chemin : `/mnt/recalbox/share/roms/` (ou chemin réseau monté)
6. Sauvegarder

#### Export Distant SSH/SFTP (nouveau)

1. Accéder à Paramètres → Export → Nouvelle Configuration
2. Nom : "Recalbox Distant"
3. Plateforme : Recalbox
4. Type : Distant
5. Configuration SSH/SFTP :
   - Hôte : `192.168.1.100`
   - Port : `22`
   - Utilisateur : `root`
   - Méthode Auth : Clé SSH
   - Chemin clé : `/home/user/.ssh/id_rsa`
   - Répertoire distant : `/recalbox/share/roms/`
6. Tester connexion
7. Sauvegarder

### 7. Premier Export

1. Accéder à l'onglet "Export"
2. Sélectionner configuration créée
3. Sélectionner consoles à exporter
4. Sélectionner jeux (ou "Tous")
5. Lancer export
6. Observer progression :
   - Fichiers copiés / transférés
   - Vitesse de transfert (si distant)
   - Temps restant estimé
7. Vérifier structure créée sur destination

**Exemple Local** :
```
/mnt/recalbox/share/roms/
├── nes/
│   ├── Super Mario Bros. (Europe).zip
│   └── ...
├── snes/
│   ├── Super Metroid (Europe).zip
│   └── ...
```

### 8. Synchroniser Métadonnées (Optionnel)

1. Accéder à l'onglet "Synchronisation"
2. Sélectionner plateforme : Recalbox ou Romm
3. Direction : Import / Export / Bidirectionnel
4. Sélectionner types de données :
   - ✓ Métadonnées scrap (descriptions, images)
   - ✓ Données utilisateur (favoris, play count, ratings)
5. Lancer synchronisation
6. Gérer conflits si détectés (dernière modif gagne / demander)
7. Voir résultat : X jeux synchronisés

## Testing

### Run Unit Tests

```bash
cd src/RomPilot.Tests
dotnet test
```

### Run with Coverage

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## Troubleshooting

### Common Issues

1. **Avalonia ne démarre pas sur Linux**
   - Vérifier bibliothèques système installées
   - Vérifier variables d'environnement DISPLAY

2. **Erreur migration EF Core**
   - Vérifier Design package installé
   - Vérifier startup project configuré

3. **SharpCompress erreur 7Z**
   - Vérifier dépendances natives installées

## Next Steps

- Voir [plan.md](./plan.md) pour architecture détaillée
- Voir [data-model.md](./data-model.md) pour schéma base de données
- Voir [tasks.md](./tasks.md) pour liste tâches implémentation

