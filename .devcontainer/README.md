# Dev Container pour RomPilot

Ce dossier contient la configuration du dev container pour le projet RomPilot.

## Prérequis

- Docker installé et en cours d'exécution
- Visual Studio Code avec l'extension "Dev Containers" installée

## Utilisation

1. Ouvrir le projet dans VS Code
2. Appuyer sur `F1` et sélectionner "Dev Containers: Reopen in Container"
3. Attendre que le container se construise et démarre
4. Le projet sera automatiquement restauré et compilé

## Contenu du container

- .NET SDK 7.0
- Entity Framework Core Tools (dotnet-ef)
- Dépendances système pour Avalonia UI (GTK3, X11, etc.)
- Outils de développement (git, curl, wget)

## Notes

- Le container utilise un utilisateur non-root (`vscode`) pour la sécurité
- Les extensions VS Code recommandées sont installées automatiquement
- Le workspace est monté dans `/workspace`
- Les fichiers de l'hôte sont accessibles via `/host/home` et `/host/media` pour accéder aux ROMs

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


