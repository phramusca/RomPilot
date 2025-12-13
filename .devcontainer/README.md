# Dev Container pour RomPilot

Ce dossier contient la configuration du dev container pour le projet RomPilot.

## Prérequis

- Docker installé et en cours d'exécution
- Cursor avec l'extension "Dev Containers" installée

## Utilisation

1. Ouvrir le projet dans Cursor
2. Appuyer sur `Ctrl+Shift+P` et sélectionner "Dev Containers: Reopen in Container"
3. Choisir "RomPilot"
4. Attendre que le container se construise et démarre
5. Le projet sera automatiquement restauré et compilé

## Contenu du container

- .NET SDK 8.0 (LTS)
- Entity Framework Core Tools (dotnet-ef 8.0)
- Dépendances système pour Avalonia UI (GTK3, X11, etc.)
- Outils de développement (git, curl, wget)

## Extensions installées

- `anysphere.csharp` - Support C# officiel Cursor avec debug
- `josefpihrt-vscode.roslynator` - Analyseur de code
- `ms-dotnettools.vscode-dotnet-runtime` - Runtime .NET
- `aaron-bond.better-comments` - Commentaires améliorés
- `ms-azuretools.vscode-docker` - Support Docker

## Notes

- Le container utilise un utilisateur non-root (`vscode`) pour la sécurité
- Les extensions Cursor sont installées automatiquement
- Le workspace est monté dans `/workspace`
- Les fichiers de l'hôte sont accessibles via `/host/home` et `/host/media` pour accéder aux ROMs
- Le debug avec breakpoints fonctionne grâce à `netcoredbg`

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

Si vous rencontrez des problèmes avec l'affichage GUI (Avalonia) :

1. Assurez-vous que X11 forwarding est configuré si vous êtes sur Linux
2. Sur Windows/Mac, utilisez WSL2 ou un serveur X11
3. Le container inclut `xvfb` pour les tests headless

