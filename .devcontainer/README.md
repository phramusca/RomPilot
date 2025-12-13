# Dev Container pour RomPilot

Ce dossier contient la configuration du dev container pour le projet RomPilot.

## Prérequis

- Docker installé et en cours d'exécution
- Cursor avec l'extension "Dev Containers" installée
- Cursor version 2.2.20 ou plus récente (pour le support du debug)

## Utilisation

1. Ouvrir le projet dans Cursor
2. Appuyer sur `F1` et sélectionner "Dev Containers: Reopen in Container"
3. Choisir "RomPilot"
4. Attendre que le container se construise et démarre
5. Le projet sera automatiquement restauré et compilé

## Contenu du container

- **.NET SDK 8.0 (LTS)** - Framework de développement
- **Entity Framework Core Tools (dotnet-ef 8.0)** - Outils de migration de base de données
- **Dépendances système pour Avalonia UI** (GTK3, X11, etc.) - Pour l'affichage de l'interface graphique
- **Outils de développement** (git, curl, wget) - Utilitaires de base

## Extensions installées automatiquement

- `anysphere.csharp` - Support C# officiel Cursor avec debug (`netcoredbg`)
- `josefpihrt-vscode.roslynator` - Analyseur de code avancé
- `ms-dotnettools.vscode-dotnet-runtime` - Runtime .NET
- `aaron-bond.better-comments` - Commentaires améliorés
- `ms-azuretools.vscode-docker` - Support Docker

## Debug

Le debug avec breakpoints fonctionne dans Cursor grâce à l'extension `anysphere.csharp` qui utilise `netcoredbg`.

### Utilisation

1. Placer des breakpoints dans votre code
2. Appuyer sur `F5` ou ouvrir "Run and Debug"
3. Choisir "🐛 Debug UI" ou "🧪 Debug Tests"
4. Les breakpoints fonctionnent ! ✅

Les configurations de debug sont dans `.vscode/launch.json`.

## Notes importantes

- Le container utilise un utilisateur non-root (`vscode`) pour la sécurité
- Les extensions Cursor sont installées automatiquement au premier lancement
- Le workspace est monté dans `/workspace`
- Les fichiers de l'hôte sont accessibles via `/host/home` et `/host/media` pour accéder aux ROMs
- Le debug avec breakpoints fonctionne grâce à `netcoredbg` (via l'extension `anysphere.csharp`)

## Accès aux fichiers de l'hôte

Le container monte automatiquement certains répertoires de l'hôte pour permettre l'accès aux fichiers ROMs :

- `/host/home` : Accès au répertoire home de l'utilisateur (${localEnv:HOME})
- `/host/media` : Accès au répertoire media de l'utilisateur (/media/${localEnv:USER})

Pour accéder à vos ROMs depuis l'application, utilisez les chemins montés :
- Exemple : `/host/home/roms` au lieu de `~/roms` ou `/home/votre-utilisateur/roms`
- Exemple : `/host/media/roms` pour les médias montés

**Note** : Les mounts sont configurés dans `.devcontainer/devcontainer.json` et utilisent les variables d'environnement `${localEnv:HOME}` et `${localEnv:USER}` de votre machine hôte.

Pour ajouter d'autres mounts, modifiez la section `"mounts"` dans `.devcontainer/devcontainer.json`.

## Dépannage

### L'affichage GUI (Avalonia) ne fonctionne pas

1. Assurez-vous que X11 forwarding est configuré si vous êtes sur Linux :
   ```bash
   xhost +local:docker
   ```

2. Sur Windows/Mac, utilisez WSL2 ou un serveur X11

3. Le container inclut `xvfb` pour les tests headless

### Le debug ne fonctionne pas

1. Vérifiez que vous avez Cursor 2.2.20 ou plus récent
2. Vérifiez que l'extension `anysphere.csharp` est installée (elle devrait l'être automatiquement)
3. Reconstruisez le container si nécessaire : `F1` > "Dev Containers: Rebuild Container"

### Reconstruire le container

Si vous rencontrez des problèmes, vous pouvez reconstruire le container :
```bash
F1 > Dev Containers: Rebuild Container
```
