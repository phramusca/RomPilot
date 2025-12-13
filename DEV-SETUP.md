# 🛠️ Guide de Configuration - RomPilot

## 🎯 Environnement de Développement

RomPilot utilise **Cursor** avec un dev container configuré pour le développement et le debug.

### ✨ Fonctionnalités

- **Extensions** : `anysphere.csharp` (support C# officiel Cursor avec debug)
- **Debugger** : ✅ `coreclr` + `netcoredbg` (breakpoints fonctionnels)
- **Usage** : Édition, refactoring, tests, debug pas à pas
- **Lancement** : `F5` → "🐛 Debug UI" (avec breakpoints)

## 🚀 Démarrage

### 1. Ouvrir dans Cursor
```bash
cursor .
```

### 2. Reopen in Container
```bash
F1 > Dev Containers: Reopen in Container
# Choisir: "RomPilot"
```

### 3. Lancer l'application avec debug
```bash
# Placer des breakpoints dans votre code
# F5 > "🐛 Debug UI"
# Les breakpoints fonctionnent ! ✅
```

> 💡 **Cursor détecte automatiquement la configuration** dans `.devcontainer/` !

## 🔧 Configuration X11 (Linux uniquement)

Pour afficher l'UI Avalonia depuis le container :

```bash
# Autoriser les connexions X11 depuis le container
xhost +local:docker

# Vérifier que DISPLAY est défini
echo $DISPLAY  # Devrait afficher ":0" ou ":1"
```

**Ajout permanent** (dans `~/.bashrc` ou `~/.zshrc`) :
```bash
# Autoriser X11 pour Docker au démarrage
xhost +local:docker > /dev/null 2>&1
```

## 🧪 Tests

### Depuis Cursor
```bash
# Tous les tests
dotnet test

# Tests avec couverture
dotnet test --collect:"XPlat Code Coverage"

# Tests filtrés
dotnet test --filter "FullyQualifiedName~ScanService"

# Mode verbeux
dotnet test --logger "console;verbosity=detailed"
```

### Debug des tests
1. Placer des breakpoints dans vos tests
2. `F5` > "🧪 Debug Tests"
3. Les breakpoints fonctionnent dans les tests aussi !

## 🎨 Formatage et Linting

Le formatage est **automatique à la sauvegarde** grâce à `.editorconfig` et Roslynator.

### Formatage manuel
```bash
# Formater tout le projet
dotnet format

# Vérifier sans modifier
dotnet format --verify-no-changes
```

### Règles de style
- **Indentation** : 4 espaces (C#), 2 espaces (XML/AXAML)
- **Longueur de ligne** : 120 caractères
- **Imports** : Organisés automatiquement à la sauvegarde
- **Analyseurs** : Roslynator + règles .editorconfig

## 🐛 Troubleshooting

### Extensions téléchargent .NET 9 alors qu'on utilise .NET 8

**Problème** : Au démarrage du container, certaines extensions (notamment `ms-dotnettools.csharp`, dépendance de Roslynator) téléchargent automatiquement .NET 9.0 (~150 MB) alors que le projet utilise .NET 8.0.

**Solution** : Les paramètres suivants dans `devcontainer.json` forcent l'utilisation du .NET 8.0 du container :
```json
"dotnetAcquisitionExtension.sharedExistingDotnetPath": "/usr/bin/dotnet",
"dotnetAcquisitionExtension.existingDotnetPath": "/usr/bin/dotnet",
"dotnetAcquisitionExtension.enableTelemetry": false,
"dotnet.dotnetPath": "/usr/bin/dotnet"
```

Ces 4 paramètres sont **tous nécessaires** pour bloquer complètement l'acquisition automatique.

### L'UI ne se lance pas
```bash
# Vérifier X11
echo $DISPLAY
xhost +local:docker

# Vérifier les logs
dotnet run --project src/RomPilot.UI/RomPilot.UI.csproj
```

### Les breakpoints ne fonctionnent pas
**Vérifications** :
1. Assurez-vous d'avoir Cursor 2.2.20 ou plus récent
2. Vérifiez que l'extension `anysphere.csharp` est installée (elle devrait l'être automatiquement via le devcontainer)
3. Utilisez les configurations de debug dans `.vscode/launch.json`

### Tâches ou launch configs en double
Si vous voyez des doublons :
1. Fermez tous les dossiers/workspaces
2. Ouvrez uniquement le dossier racine
3. Rechargez la fenêtre (`F1` > "Reload Window")

## 📦 Dépendances

### Runtime
- .NET 8.0 SDK (LTS)
- SQLite
- X11 (Linux, pour l'UI)

### Extensions Cursor
Installées automatiquement par le dev container :
- `anysphere.csharp` - Support C# officiel Cursor avec debug et IntelliSense
- `josefpihrt-vscode.roslynator` - Analyseur de code avancé (linting + refactorings)
- `aaron-bond.better-comments` - Commentaires améliorés
- `ms-azuretools.vscode-docker` - Support Docker

## 🔄 Workflow Recommandé

1. **Développement quotidien** : Cursor
   - Édition de code
   - Refactoring
   - Tests rapides (`F5` > "🧪 Debug Tests")
   - Debug avec breakpoints (`F5` > "🐛 Debug UI")
   - Lancement de l'UI pour vérifier visuellement

2. **Synchronisation** : Git
   - Les configurations sont versionnées
   - Commitez depuis Cursor

## 📝 Notes Importantes

- **Settings** : `.vscode/settings.json` contient les settings partagés (versionné).
- **Tâches** : `.vscode/tasks.json` contient les tâches communes.
- **Launch configs** : `.vscode/launch.json` contient les configurations de debug.
- **Dev Container** : Configuration unique dans `.devcontainer/devcontainer.json`.

## 🆘 Support

En cas de problème :
1. Vérifier ce guide
2. Consulter les logs du container (`Docker` > `Containers` > clic droit > `View Logs`)
3. Reconstruire le container (`F1` > "Dev Containers: Rebuild Container")
