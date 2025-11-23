# Quick Start Guide: ROM Manager Application

**Date**: 2025-01-27  
**Feature**: [spec.md](./spec.md)

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

### 1. Configure Database Sources

- Télécharger datfiles NoIntro/Redump/GoodSet
- Configurer chemins dans préférences application

### 2. Add Source Directories

- Ajouter répertoires contenant ROMs
- Lancer premier scan

### 3. Configure Preferences

- Définir priorités région/langue
- Configurer chemins export Recalbox/Romm
- Configurer API Romm (URL, clé API)

### 4. First Scan

- Sélectionner répertoires sources
- Lancer scan
- Vérifier identification ROMs

### 5. Export Test

- Sélectionner quelques jeux
- Exporter vers Recalbox ou Romm
- Vérifier structure fichiers créée

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

