# 🧪 Tests de Vérification Rapide

Ce document vous permet de vérifier que tout fonctionne correctement.

## ✅ Checklist de Vérification (5 minutes)

### 1. Compilation ✓
```bash
dotnet build
```
**Attendu** : `Build succeeded` avec 0 erreurs

### 2. Tests Unitaires ✓
```bash
dotnet test
```
**Attendu** : Tous les tests passent (25+)

### 3. Lancement de l'UI (F5) ✓
1. Appuyez sur **F5**
2. Sélectionnez **"🚀 Lancer l'UI (Sans Debug)"**

**Attendu** :
- L'application se lance
- Une fenêtre apparaît avec l'interface RomPilot
- Logs dans le terminal : `[ScanViewModel] Loaded...`, `[LibraryViewModel] Loaded...`

### 4. Lancement des Tests (F5) ✓
1. Appuyez sur **F5**
2. Sélectionnez **"🧪 Lancer les Tests"**

**Attendu** :
- Les tests s'exécutent
- Résultats détaillés dans le terminal
- Tous les tests passent

### 5. IntelliSense ✓
1. Ouvrez `src/RomPilot.Core/Services/ScanService.cs`
2. Tapez `Console.` et attendez

**Attendu** : Liste d'autocomplétion apparaît

## 🐛 Si Quelque Chose Ne Fonctionne Pas

### L'application ne se lance pas (F5)
```bash
# Test manuel
dotnet run --project src/RomPilot.UI/RomPilot.UI.csproj
```

### Les tests échouent
```bash
# Voir les détails
dotnet test --logger "console;verbosity=detailed"
```

### IntelliSense ne fonctionne pas
```bash
# Recharger l'extension C#
# Dans Cursor: Ctrl+Shift+P > "Developer: Reload Window"
```

### Erreur X11 / Display
```bash
export DISPLAY=:0
xeyes  # Devrait afficher une fenêtre
```

## 📊 Résultats Attendus

### Build
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### Tests
```
Passed!  - Failed:     0, Passed:    25+, Skipped:     0, Total:    25+
```

### Lancement UI
```
[ScanViewModel] Loaded last scanned directory: ...
[LibraryViewModel] Loaded 226 scanned files from database
[LibraryViewModel] Sample file: Console=Sony PlayStation, Checksums=4
```

## ✅ Tout Fonctionne ?

Si tous les tests passent, vous êtes prêt pour :
1. **Continuer l'implémentation de la User Story 1**
2. **Implémenter la User Story 2**
3. **Ou toute autre fonctionnalité !**

Consultez [SETUP-COMPLETE.md](SETUP-COMPLETE.md) pour les prochaines étapes.
