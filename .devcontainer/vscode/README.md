# Dev Container VS Code (Debug)

Ce dev container est configuré pour **VS Code** avec les capacités de debug complètes.

## Extensions Installées

- **C# Dev Kit** (`ms-dotnettools.csdevkit`) : Support complet C# + debugger
- **C#** (`ms-dotnettools.csharp`) : Language server
- **Roslynator** : Analyseur de code avancé
- **.NET Runtime** : Runtime support
- **Better Comments** : Commentaires améliorés
- **Docker** : Support Docker

## Configurations de Debug

- 🐛 **Debug UI** : Lance l'application avec breakpoints
- 🧪 **Debug Tests** : Lance tous les tests avec breakpoints
- 🧪 **Debug Test (Current File)** : Lance le fichier de test courant

## Avantages

- ✅ **Breakpoints fonctionnels** : Debugger `coreclr` + `vsdbg`
- ✅ **Debug pas à pas** : Inspection des variables, stack trace
- ✅ **Test Explorer** : Interface graphique pour les tests

## Installation Automatique

Le debugger `vsdbg` est installé automatiquement au premier lancement du container.

## Pour Éditer sans Debug

Utilisez le dev container **cursor** :
```bash
Ctrl+Shift+P > Dev Containers: Reopen in Container
Choisir: "RomPilot - Cursor (Édition)"
```

