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

## Dépannage

Si vous rencontrez des problèmes avec l'affichage GUI (Avalonia) :

1. Assurez-vous que X11 forwarding est configuré si vous êtes sur Linux
2. Sur Windows/Mac, utilisez WSL2 ou un serveur X11
3. Le container inclut `xvfb` pour les tests headless



