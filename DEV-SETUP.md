# 🛠️ Guide de Configuration - RomPilot

## 🎯 Deux Environnements, Deux Usages

RomPilot utilise **deux configurations de dev container** dans `.devcontainer/` :

### 🖊️ Cursor (`.devcontainer/cursor/`) - Édition et Développement
- **Extensions** : `anysphere.csharp`, `roslynator`
- **Debugger** : ✅ `coreclr` + `netcoredbg` (breakpoints fonctionnels)
- **Usage** : Édition, refactoring, tests rapides, debug
- **Lancement** : `F5` → "🐛 Debug UI" (avec breakpoints) ou "🚀 Lancer l'UI" (sans debug)

### 🐛 VS Code (`.devcontainer/vscode/`) - Debug Complet
- **Extensions** : `ms-dotnettools.csdevkit`, `ms-dotnettools.csharp`
- **Debugger** : ✅ `coreclr` + `vsdbg` (breakpoints fonctionnels)
- **Usage** : Investigation de bugs, debug pas à pas
- **Lancement** : `F5` → "🐛 Debug UI" (avec breakpoints)

## 🚀 Démarrage

### Option 1 : Cursor (Recommandé pour le développement)
```bash
# 1. Ouvrir le dossier dans Cursor
cursor .

# 2. Reopen in Container
Ctrl+Shift+P > Dev Containers: Reopen in Container
# Choisir: "RomPilot - Cursor (Édition)"

# 3. Lancer l'application
F5 > "🐛 Debug UI" (avec breakpoints) ou "🚀 Lancer l'UI" (sans debug)
```

### Option 2 : VS Code (Pour le debug avec breakpoints)
```bash
# 1. Ouvrir le dossier dans VS Code
code .

# 2. Reopen in Container
Ctrl+Shift+P > Dev Containers: Reopen in Container
# Choisir: "RomPilot - VS Code (Debug)"

# 3. Placer des breakpoints et lancer
F5 > "🐛 Debug UI"
```

> 💡 **VS Code/Cursor détecte automatiquement les deux configurations** et vous propose de choisir !

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

### Depuis Cursor ou VS Code
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

### Depuis VS Code avec Test Explorer
1. Ouvrir la vue "Testing" (icône fiole dans la barre latérale)
2. Tous les tests xUnit sont détectés automatiquement
3. Cliquer sur ▶️ pour lancer un test
4. Placer des breakpoints dans les tests pour débugger

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

### L'UI ne se lance pas
```bash
# Vérifier X11
echo $DISPLAY
xhost +local:docker

# Vérifier les logs
dotnet run --project src/RomPilot.UI/RomPilot.UI.csproj
```

### Les breakpoints ne fonctionnent pas dans Cursor
**Avec Cursor 2.2.20+** : Le debug est maintenant supporté grâce à l'extension `anysphere.csharp` qui utilise `netcoredbg`.

**Vérifications** :
1. Assurez-vous d'avoir Cursor 2.2.20 ou plus récent
2. Vérifiez que l'extension `anysphere.csharp` est installée (elle devrait l'être automatiquement via le devcontainer)
3. Utilisez les configurations de debug dans `RomPilot-cursor.code-workspace`

### Extensions C#
Les deux dev containers utilisent **des extensions différentes** :
- **Cursor** : `anysphere.csharp` (support C# officiel Cursor avec debug via `netcoredbg`)
- **VS Code** : `ms-dotnettools.csdevkit` (support C# Microsoft avec debug via `vsdbg`)

Les deux supportent maintenant le debug avec breakpoints. Ils ne se mélangent pas car ils utilisent des containers séparés.

### Tâches ou launch configs en double
Si vous voyez des doublons dans VS Code :
1. Fermez tous les dossiers/workspaces
2. Ouvrez **uniquement** `RomPilot-vscode.code-workspace`
3. Rechargez la fenêtre (`Ctrl+Shift+P` > "Reload Window")

### vsdbg introuvable
Le debugger `vsdbg` est installé automatiquement par `.devcontainer-vscode/`.

Si nécessaire, installation manuelle :
```bash
curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l ~/.vsdbg
```

## 📦 Dépendances

### Runtime
- .NET 7.0 SDK
- SQLite
- X11 (Linux, pour l'UI)

### Extensions VS Code/Cursor
Installées automatiquement par les dev containers :

**Cursor** :
- `anysphere.csharp` - Support C# officiel Cursor avec debug (`netcoredbg`)
- `josefpihrt-vscode.roslynator` - Analyseur de code
- `ms-dotnettools.vscode-dotnet-runtime` - Runtime .NET
- `aaron-bond.better-comments` - Commentaires améliorés
- `ms-azuretools.vscode-docker` - Support Docker

**VS Code** :
- `ms-dotnettools.csdevkit` - C# Dev Kit (debugger inclus)
- `ms-dotnettools.csharp` - Language server C#
- `josefpihrt-vscode.roslynator` - Analyseur de code
- `ms-dotnettools.vscode-dotnet-runtime` - Runtime .NET
- `aaron-bond.better-comments` - Commentaires améliorés
- `ms-azuretools.vscode-docker` - Support Docker

## 🔄 Workflow Recommandé

1. **Développement quotidien** : Cursor
   - Édition de code
   - Refactoring
   - Tests rapides (`F5` > "🧪 Lancer les Tests")
   - Debug avec breakpoints (`F5` > "🐛 Debug UI")
   - Lancement de l'UI pour vérifier visuellement

2. **Investigation de bugs** : Cursor ou VS Code
   - Les deux supportent maintenant le debug avec breakpoints
   - Placer des breakpoints
   - Debug pas à pas
   - Inspecter les variables
   - Analyser la stack trace

3. **Synchronisation** : Git
   - Les deux environnements partagent le même code
   - Commitez depuis n'importe lequel
   - Les configurations sont versionnées

## 📝 Notes Importantes

- **Workspaces spécifiques** : Utilisez `RomPilot-cursor.code-workspace` pour Cursor et `RomPilot-vscode.code-workspace` pour VS Code.
- **Settings communs** : `.vscode/settings.json` contient les settings partagés (versionné).
- **Tâches partagées** : `.vscode/tasks.json` est utilisé par les deux environnements.
- **Launch configs séparées** : 
  - Cursor : configurations avec debug (coreclr + netcoredbg) et sans debug (node-terminal) dans `RomPilot-cursor.code-workspace`
  - VS Code : configurations avec debug (coreclr + vsdbg) dans `RomPilot-vscode.code-workspace`
- **Même Docker Compose** : Les deux dev containers réutilisent `.devcontainer/docker-compose.yml`.

## 🆘 Support

En cas de problème :
1. Vérifier ce guide
2. Consulter les logs du container (`Docker` > `Containers` > clic droit > `View Logs`)
3. Reconstruire le container (`Ctrl+Shift+P` > "Dev Containers: Rebuild Container")
