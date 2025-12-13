# 🛠️ Configuration de l'Environnement de Développement

Ce document explique comment utiliser l'environnement de développement RomPilot configuré dans le dev container.

## ✅ Configuration Fonctionnelle

Cette configuration est **testée et fonctionnelle** dans Cursor et VS Code :

- **Extensions** : `anysphere.csharp` (IntelliSense C#)
- **Lancement** : Type `node-terminal` dans `launch.json` (compatible Cursor)
- **Tests** : Lancez avec F5 > "🧪 Lancer les Tests"
- **UI** : Lancez avec F5 > "🚀 Lancer l'UI (Sans Debug)"
- **Limitation** : Pas de breakpoints dans Cursor (utiliser VS Code si nécessaire)

📖 **Pour plus de détails sur Cursor** : Consultez [Configuration Spécifique pour Cursor](docs/CURSOR-SETUP.md)

## 📋 Prérequis

- Docker installé et fonctionnel
- VS Code ou Cursor avec l'extension "Dev Containers"
- .NET 7.0 (déjà installé dans le dev container)
- Serveur X11 pour l'affichage graphique (automatique sur Linux)

## 🚀 Démarrage Rapide

1. **Ouvrir le projet dans le dev container** :
   - Ouvrir VS Code/Cursor
   - Commande : `Dev Containers: Reopen in Container`
   - Attendre que le container se construise et démarre (~2-3 minutes la première fois)

2. **Vérifier l'installation** :
   ```bash
   dotnet --version  # Devrait afficher 7.0.x
   dotnet build      # Compile le projet
   ```

3. **Lancer l'application** :
   - Appuyez sur **F5**
   - Sélectionnez **"🚀 Lancer l'UI (Sans Debug)"**
   - L'application s'affiche dans une fenêtre séparée ✅

## 🐛 Lancer et Débugger l'Application

### 🎯 Lancer l'Application dans Cursor

L'application se lance facilement avec F5 dans Cursor ! Trois configurations sont disponibles :

1. **🚀 Lancer l'UI (Sans Debug)** ✅ **Recommandé**
   - Appuyez sur F5 et sélectionnez cette option
   - Lance l'application Avalonia normalement
   - Affiche les logs dans le terminal intégré

2. **🧪 Lancer les Tests**
   - Lance tous les tests unitaires avec sortie détaillée
   - Idéal pour vérifier que tout fonctionne

3. **🔍 Lancer l'UI avec Logs Détaillés**
   - Lance l'application en mode Development
   - Affiche des logs plus verbeux pour le debugging

**Note** : Ces configurations utilisent le type `node-terminal` car Cursor ne supporte pas le debugging C# avec breakpoints (type `coreclr`).

### 🔍 Techniques de Debugging dans Cursor

Puisque Cursor ne supporte pas les breakpoints C#, voici les meilleures pratiques :

#### 1. Logging Structuré
```csharp
// Utilisez des logs informatifs
Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [ScanService] Scanning directory: {path}");
Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [ScanService] Found {files.Count} files");
```

#### 2. Tests Unitaires (TDD)
```bash
# Lancer les tests en mode watch (relance automatique)
dotnet watch test --project src/RomPilot.Tests/RomPilot.Tests.csproj
```

#### 3. Script de Debugging
```bash
# Lance l'UI avec logs détaillés
./scripts/debug-ui.sh
```

### 🔧 Debugging Avancé avec Breakpoints (VS Code uniquement)

Si vous avez besoin de breakpoints, utilisez VS Code :

1. Ouvrir le projet dans VS Code (pas Cursor)
2. Installer l'extension "C# Dev Kit" (`ms-dotnettools.csdevkit`)
3. Le debugging avec breakpoints fonctionnera avec F5

**Note X11** : L'application utilise Avalonia UI qui nécessite un serveur X11. Dans le dev container :
- Si vous êtes sur **Linux** : Le X11 forwarding est automatique via `DISPLAY=:0`
- Si vous êtes sur **Windows/macOS** : Vous devrez peut-être installer un serveur X11 (VcXsrv, XQuartz)
- L'application s'affichera dans une fenêtre séparée sur votre système hôte

### Lancer les Tests

#### Méthode 1 : Avec F5 dans Cursor
1. Appuyez sur F5
2. Sélectionnez **"🧪 Lancer les Tests"**
3. Les résultats s'affichent dans le terminal intégré

#### Méthode 2 : En ligne de commande

Le Test Explorer de VS Code/Cursor détecte automatiquement les tests. Vous pouvez :
- Voir tous les tests dans le panneau Testing (icône éprouvette)
- Lancer un test individuel (clic sur ▶️)
- Débugger un test individuel (clic droit > Debug Test)
- Voir les résultats en temps réel

## 🧪 Lancer les Tests

### Via le Terminal

```bash
# Tous les tests
dotnet test src/RomPilot.Tests/RomPilot.Tests.csproj

# Avec verbosité
dotnet test src/RomPilot.Tests/RomPilot.Tests.csproj --logger "console;verbosity=detailed"

# Filtrer les tests
dotnet test --filter "FullyQualifiedName~ScanService"

# Avec couverture de code
dotnet test --collect:"XPlat Code Coverage"
```

### Via le Script Utilitaire

```bash
# Tests basiques
./scripts/run-tests.sh

# Avec couverture
./scripts/run-tests.sh --coverage

# Filtrer les tests
./scripts/run-tests.sh --filter ScanService

# Mode verbeux
./scripts/run-tests.sh --verbose

# Aide
./scripts/run-tests.sh --help
```

### Via VS Code Tasks

1. Ouvrir la palette de commandes (Ctrl+Shift+P)
2. Taper "Tasks: Run Task"
3. Sélectionner :
   - **test** : Lance tous les tests
   - **test-with-coverage** : Lance les tests avec couverture
   - **build-tests** : Compile seulement les tests

## 🎨 Linter et Formatter

### Formatage Automatique

Le formatage automatique est configuré :
- **À la sauvegarde** : Le code est formaté automatiquement
- **Imports organisés** : Les using sont triés à la sauvegarde

### Formater Manuellement

```bash
# Formater tout le projet
dotnet format RomPilot.sln

# Vérifier le formatage sans modifier
dotnet format RomPilot.sln --verify-no-changes

# Via VS Code Task
# Palette de commandes > Tasks: Run Task > format
```

### Standards de Code

Le fichier `.editorconfig` définit les standards :
- Indentation : 4 espaces pour C#, 2 pour XML/JSON
- Conventions de nommage :
  - Champs privés : `_camelCase` avec underscore
  - Propriétés/méthodes publiques : `PascalCase`
  - Constantes : `PascalCase`
- Accolades : toujours sur nouvelle ligne
- Using : hors du namespace

## 📊 Couverture de Code

### Générer un Rapport

```bash
# Lancer les tests avec couverture
dotnet test --collect:"XPlat Code Coverage"

# Le rapport est généré dans:
# src/RomPilot.Tests/TestResults/{guid}/coverage.cobertura.xml
```

### Visualiser la Couverture

Installer l'extension "Coverage Gutters" pour voir la couverture directement dans l'éditeur :
1. Installer l'extension
2. Lancer les tests avec couverture
3. Commande : `Coverage Gutters: Display Coverage`

## 🔧 Tâches VS Code Disponibles

Toutes les tâches sont accessibles via : `Ctrl+Shift+P` > `Tasks: Run Task`

- **build** : Compile l'UI
- **build-tests** : Compile les tests
- **test** : Lance tous les tests
- **test-with-coverage** : Tests avec couverture
- **run** : Lance l'application sans debugger
- **watch** : Mode watch (recompile à chaque changement)
- **format** : Formate le code
- **format-verify** : Vérifie le formatage
- **clean** : Nettoie les binaires
- **restore** : Restaure les dépendances
- **publish** : Publie l'application

## 🛠️ Résolution de Problèmes

### Le debugger ne démarre pas

1. Vérifier que le projet compile : `dotnet build`
2. Si l'UI ne s'affiche pas mais le debug fonctionne, vérifier X11 :
   ```bash
   echo $DISPLAY  # Doit afficher :0 ou :99
   ```
3. Tester avec la tâche "run" : Si elle fonctionne, le debug devrait aussi fonctionner
4. Supprimer bin/obj et recompiler : `dotnet clean && dotnet build`

### Les tests ne sont pas détectés

1. Recompiler les tests : `dotnet build src/RomPilot.Tests/`
2. Recharger la fenêtre VS Code : `Ctrl+Shift+P` > `Reload Window`
3. Vérifier le Test Explorer : panneau Testing (icône éprouvette)

### Problèmes de formatage

```bash
# Réinitialiser le formatage
dotnet format RomPilot.sln

# Vérifier .editorconfig
cat .editorconfig
```

### Base de données verrouillée

```bash
# Supprimer la base de données de dev
rm rompilot.db rompilot.db-shm rompilot.db-wal

# Recréer avec les migrations
dotnet ef database update --project src/RomPilot.Core --startup-project src/RomPilot.UI
```

### L'application ne se lance pas (F5)

**Symptôme** : Erreur au lancement ou l'application ne démarre pas.

**Solutions** :

1. **Vérifier que le projet compile** :
```bash
dotnet build src/RomPilot.UI/RomPilot.UI.csproj
```

2. **Vérifier X11 (pour l'interface graphique)** :
```bash
echo $DISPLAY  # Devrait afficher ":0" ou similaire
```
Si vide, exportez la variable :
```bash
export DISPLAY=:0
```

3. **Tester le lancement manuel** :
```bash
dotnet run --project src/RomPilot.UI/RomPilot.UI.csproj
```

4. **Vérifier les extensions installées** :
```bash
code --list-extensions | grep csharp
```
Vous devriez voir `anysphere.csharp`.

Si vous voyez des erreurs OpenGL (libGL error), c'est normal - elles n'empêchent pas l'application de fonctionner.

## 📚 Ressources

- [Documentation .NET](https://docs.microsoft.com/dotnet/)
- [Avalonia UI Documentation](https://docs.avaloniaui.net/)
- [xUnit Documentation](https://xunit.net/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)

## 🎯 Bonnes Pratiques

### Avant de Commit

```bash
# 1. Formater le code
dotnet format

# 2. Vérifier que tout compile
dotnet build

# 3. Lancer les tests
./scripts/run-tests.sh

# 4. Vérifier la couverture si nécessaire
./scripts/run-tests.sh --coverage
```

### Pendant le Développement

- ✅ Utiliser le debugger plutôt que des `Console.WriteLine()`
- ✅ Écrire des tests pour les nouvelles fonctionnalités
- ✅ Respecter les conventions de nommage
- ✅ Commiter régulièrement avec des messages clairs
- ✅ Utiliser le formatage automatique

### TDD (Test-Driven Development)

1. Écrire un test qui échoue
2. Implémenter le minimum de code pour passer le test
3. Refactorer si nécessaire
4. Répéter

## 💡 Astuces

### Raccourcis Clavier Utiles

- **F5** : Démarrer le debug
- **Shift+F5** : Arrêter le debug
- **F9** : Toggle breakpoint
- **F10** : Step over
- **F11** : Step into
- **Ctrl+Shift+B** : Build
- **Ctrl+Shift+T** : Rouvrir un test fermé

### Snippets C#

Taper puis Tab :
- `ctor` : Constructeur
- `prop` : Propriété
- `propfull` : Propriété complète avec backing field
- `testm` : Méthode de test xUnit

### Extensions Recommandées

Déjà installées dans le dev container :
- ✅ C# (muhammad-sammy.csharp)
- ✅ .NET Runtime (ms-dotnettools.vscode-dotnet-runtime)
- ✅ Better Comments (aaron-bond.better-comments)
- ✅ Roslynator (josefpihrt-vscode.roslynator)
- ✅ Docker (ms-azuretools.vscode-docker)

Optionnelles mais recommandées :
- Coverage Gutters (pour visualiser la couverture)
- GitLens (pour Git avancé)


