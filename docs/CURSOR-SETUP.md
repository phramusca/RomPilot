# Configuration pour Cursor

Ce document explique la configuration spécifique pour utiliser RomPilot dans Cursor.

## 🎯 Configuration Fonctionnelle

### Résumé

Cursor fonctionne très bien pour développer RomPilot, avec quelques adaptations :

✅ **Ce qui fonctionne** :
- IntelliSense et autocomplétion C#
- Lancement de l'application avec F5
- Lancement des tests avec F5
- Linting et formatage automatique
- Terminal intégré avec logs
- Édition de code avec toutes les fonctionnalités Cursor (AI, etc.)

❌ **Ce qui ne fonctionne pas** :
- Debugging avec breakpoints (limitation de Cursor, utiliser VS Code si nécessaire)

### Extensions Installées

```json
{
  "extensions": [
    "anysphere.csharp",              // IntelliSense C# pour Cursor
    "ms-dotnettools.vscode-dotnet-runtime",
    "aaron-bond.better-comments",
    "ms-azuretools.vscode-docker"
  ]
}
```

**Note** : Nous n'utilisons **pas** `muhammad-sammy.csharp` ni `josefpihrt-vscode.roslynator` car ils causent des conflits avec le lancement de l'application.

### Configuration launch.json

La clé est d'utiliser le type `node-terminal` au lieu de `coreclr` :

```json
{
  "name": "🚀 Lancer l'UI (Sans Debug)",
  "type": "node-terminal",  // ← Important : pas "coreclr"
  "request": "launch",
  "command": "cd ${workspaceFolder} && dotnet run --project src/RomPilot.UI/RomPilot.UI.csproj",
  "env": {
    "DISPLAY": ":0",
    "GDK_BACKEND": "x11"
  }
}
```

### Pourquoi cette configuration ?

1. **Type `node-terminal`** : Cursor ne supporte pas le debugger `coreclr` utilisé par .NET. Le type `node-terminal` lance simplement une commande dans le terminal, ce qui fonctionne parfaitement pour notre cas d'usage.

2. **Pas de Roslynator** : Cette extension dépend de `muhammad-sammy.csharp` qui entre en conflit avec `anysphere.csharp`. On préfère sacrifier quelques analyseurs pour avoir un lancement qui fonctionne.

3. **Variables d'environnement X11** : Nécessaires pour afficher l'interface graphique Avalonia depuis le dev container.

## 🚀 Utilisation

### Lancer l'application

1. Appuyez sur **F5** (ou Ctrl+Shift+D puis cliquez sur ▶️)
2. Sélectionnez **"🚀 Lancer l'UI (Sans Debug)"**
3. L'application se lance dans une fenêtre séparée

### Lancer les tests

1. Appuyez sur **F5**
2. Sélectionnez **"🧪 Lancer les Tests"**
3. Les résultats s'affichent dans le terminal intégré

### Debugging (sans breakpoints)

Puisque Cursor ne supporte pas les breakpoints, voici les alternatives :

#### 1. Logs détaillés
```bash
# F5 > "🔍 Lancer l'UI avec Logs Détaillés"
# Ou :
./scripts/debug-ui.sh
```

#### 2. Console.WriteLine stratégiques
```csharp
Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [MyClass] Variable value: {myVar}");
```

#### 3. Tests unitaires ciblés
```bash
# Tester une fonctionnalité spécifique
dotnet test --filter "FullyQualifiedName~MyTestClass"
```

#### 4. Mode watch pour les tests
```bash
# Les tests se relancent automatiquement à chaque modification
dotnet watch test --project src/RomPilot.Tests/RomPilot.Tests.csproj
```

## 🔧 Debugging Avancé (VS Code)

Si vous avez vraiment besoin de breakpoints :

1. Ouvrez le projet dans **VS Code** (pas Cursor)
2. Installez l'extension **"C# Dev Kit"** (`ms-dotnettools.csdevkit`)
3. Le debugging avec breakpoints fonctionnera nativement

Vous pouvez alterner entre Cursor (pour le développement général) et VS Code (pour le debugging ponctuel).

## 🐛 Troubleshooting

### L'application ne se lance pas

```bash
# Vérifier que tout compile
dotnet build

# Tester le lancement manuel
dotnet run --project src/RomPilot.UI/RomPilot.UI.csproj
```

### Erreurs OpenGL (libGL error)

Ces erreurs sont **normales** et n'empêchent pas l'application de fonctionner :
```
libGL error: MESA-LOADER: failed to retrieve device information
libGL error: failed to load driver: nvidia-drm
```

Avalonia utilise un fallback software renderer si le pilote GPU n'est pas disponible.

### Fenêtre ne s'affiche pas

```bash
# Vérifier X11
echo $DISPLAY  # Devrait afficher ":0"

# Tester xeyes
xeyes  # Une fenêtre avec des yeux devrait s'afficher
```

Si `xeyes` ne fonctionne pas, le problème vient de la configuration X11 de votre système.

## 📚 Ressources

- [Documentation Cursor](https://cursor.sh/docs)
- [Avalonia UI Documentation](https://docs.avaloniaui.net/)
- [.NET 7.0 Documentation](https://docs.microsoft.com/dotnet/core/whats-new/dotnet-7)


