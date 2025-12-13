# Dev Container Cursor (Édition)

Ce dev container est configuré pour **Cursor** avec un focus sur l'édition et le développement rapide.

## Extensions Installées

- **C# (Cursor)** (`anysphere.csharp`) : Support C# optimisé pour Cursor
- **Roslynator** : Analyseur de code avancé
- **.NET Runtime** : Runtime support
- **Better Comments** : Commentaires améliorés
- **Docker** : Support Docker

## Configurations de Lancement

- 🚀 **Lancer l'UI** : Lance l'application (sans breakpoints)
- 🧪 **Lancer les Tests** : Lance tous les tests
- 🔍 **Lancer l'UI avec Logs Détaillés** : Lance avec logs verbeux

## Limitations

- ❌ **Pas de breakpoints** : Utilise `node-terminal` (pas de debugger coreclr)
- ✅ **Rapide** : Lancement instantané sans overhead de debug
- ✅ **Édition fluide** : Optimisé pour le développement quotidien

## Pour Débugger avec Breakpoints

Utilisez le dev container **vscode** :
```bash
Ctrl+Shift+P > Dev Containers: Reopen in Container
Choisir: "RomPilot - VS Code (Debug)"
```

