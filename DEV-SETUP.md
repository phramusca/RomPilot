# 🛠️ Guide de Configuration de l'Environnement

Guide de troubleshooting et configuration avancée pour RomPilot.

## 🚀 Installation

### Dev Container (Recommandé)

```bash
# 1. Ouvrir le projet dans VS Code/Cursor
# 2. Dev Containers: Reopen in Container
# 3. Attendre la construction (~2-3 minutes)
```

Le dev container configure automatiquement :
- .NET 7.0 SDK
- Extensions C# (Roslynator)
- SQLite
- X11 forwarding pour l'UI

### Installation Locale

```bash
# 1. Installer .NET 7.0 SDK
wget https://dot.net/v1/dotnet-install.sh
bash dotnet-install.sh --channel 7.0

# 2. Restaurer les dépendances
dotnet restore

# 3. Build
dotnet build

# 4. Créer la base de données
dotnet ef database update --project src/RomPilot.Core
```

## 🐛 Troubleshooting

### L'application ne se lance pas (F5)

**Vérifier que tout compile** :
```bash
dotnet build
```

**Vérifier X11 (pour l'interface graphique)** :
```bash
echo $DISPLAY  # Devrait afficher ":0"
xeyes          # Fenêtre de test
```

**Lancer manuellement** :
```bash
dotnet run --project src/RomPilot.UI/RomPilot.UI.csproj
```

### Erreurs OpenGL (libGL error)

Ces erreurs sont **normales** et n'empêchent pas l'application de fonctionner :
```
libGL error: failed to load driver: nvidia-drm
```
Avalonia utilise un fallback software renderer.

### Tests échouent

```bash
# Voir les détails
dotnet test --logger "console;verbosity=detailed"

# Nettoyer et rebuild
dotnet clean
dotnet build
dotnet test
```

### Problèmes de base de données

```bash
# Supprimer et recréer
rm -f src/RomPilot.UI/rompilot.db
dotnet ef database update --project src/RomPilot.Core

# Créer une nouvelle migration
dotnet ef migrations add NomDeLaMigration --project src/RomPilot.Core
```

### IntelliSense ne fonctionne pas

```bash
# Recharger la fenêtre
# Ctrl+Shift+P > "Developer: Reload Window"

# Ou reconstruire
dotnet clean
dotnet restore
dotnet build
```

## 🧪 Tests

### Lancer les Tests

```bash
# Tous
dotnet test

# Avec logs détaillés
dotnet test --logger "console;verbosity=detailed"

# Filtrer par nom
dotnet test --filter "FullyQualifiedName~ScanService"

# Avec couverture
dotnet test --collect:"XPlat Code Coverage"
```

### Mode Watch (TDD)

```bash
# Les tests se relancent automatiquement à chaque modification
dotnet watch test --project src/RomPilot.Tests/RomPilot.Tests.csproj
```

## 🔧 Configuration

### Fichiers de Configuration

**`.devcontainer/devcontainer.json`** :
- Extensions C# (Roslynator)
- Settings VS Code/Cursor (formatage, IntelliSense, etc.)
- Montage des répertoires hôte

**`.vscode/launch.json`** :
- 🚀 Lancer l'UI (Sans Debug)
- 🧪 Lancer les Tests
- 🔍 Lancer l'UI avec Logs Détaillés

**`.vscode/tasks.json`** :
- Tâches de build, test, format, clean

**Note** : Les settings sont dans `devcontainer.json`, pas dans `.vscode/settings.json` (qui n'existe pas). Pour le debugging avec breakpoints, utilisez VS Code avec l'extension "C# Dev Kit".

## 📦 Dépendances

```bash
# Ajouter un package
dotnet add src/RomPilot.Core package NomDuPackage

# Restaurer après changement
dotnet restore
```

## 🗃️ Base de Données

### Migrations EF Core

```bash
# Créer une migration
dotnet ef migrations add NomDeLaMigration --project src/RomPilot.Core

# Appliquer les migrations
dotnet ef database update --project src/RomPilot.Core

# Supprimer la dernière migration
dotnet ef migrations remove --project src/RomPilot.Core

# Voir les migrations
dotnet ef migrations list --project src/RomPilot.Core
```

### Localisation de la BD

```
src/RomPilot.UI/rompilot.db
```

## 🎨 Formatage et Linting

```bash
# Formater automatiquement
dotnet format

# Vérifier sans modifier
dotnet format --verify-no-changes

# Formater uniquement les fichiers modifiés
dotnet format --include <fichier.cs>
```

Le formatage suit les règles définies dans `.editorconfig`.

## 🔍 Debugging

### Avec Logs

```csharp
// Ajouter des logs informatifs
Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [MyClass] Debug: {value}");
```

### Avec Tests

```csharp
[Fact]
public void Should_Handle_Edge_Case()
{
    // Arrange
    var service = new MyService();
    
    // Act
    var result = service.DoSomething();
    
    // Assert
    result.Should().Be(expected);
}
```

### Avec Breakpoints (VS Code seulement)

1. Ouvrir dans VS Code (pas Cursor)
2. Installer "C# Dev Kit" (`ms-dotnettools.csdevkit`)
3. F5 fonctionne avec breakpoints

## 🌐 X11 et Avalonia UI

L'application nécessite un serveur X11 pour l'affichage graphique.

### Linux
X11 forwarding automatique via `DISPLAY=:0`

### Windows (WSL2)
Installer VcXsrv ou X410

### macOS
Installer XQuartz

## 📚 Ressources

- [Avalonia UI Docs](https://docs.avaloniaui.net/)
- [.NET 7.0 Docs](https://docs.microsoft.com/dotnet/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [xUnit Documentation](https://xunit.net/)
