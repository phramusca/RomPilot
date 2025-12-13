# 🎮 RomPilot

Application de gestion de ROMs (lecture seule) avec support de multiples formats, identification automatique, et export vers Recalbox et Romm.

## 🚀 Démarrage Rapide

### Pour les Développeurs

1. **Ouvrir dans le Dev Container**
   ```bash
   VS Code/Cursor > Dev Containers: Reopen in Container
   ```

2. **Consulter les guides**
   - 📖 [Guide de Configuration de l'Environnement de Développement](DEV-SETUP.md)
   - 🎯 [Configuration Spécifique pour Cursor](docs/CURSOR-SETUP.md)

3. **Lancer l'application** 🚀
   ```bash
   # Dans Cursor/VS Code : Appuyez sur F5
   # Sélectionnez "🚀 Lancer l'UI (Sans Debug)"
   
   # Ou en ligne de commande :
   ./scripts/debug-ui.sh
   ```

4. **Lancer les tests** 🧪
   ```bash
   # Avec F5 > "🧪 Lancer les Tests"
   # Ou en ligne de commande :
   ./scripts/run-tests.sh
   dotnet test
   ```

💡 **Note** : Pour le debugging avec breakpoints, utilisez VS Code avec l'extension "C# Dev Kit" (Cursor ne supporte pas les breakpoints C#).

## 📋 Prérequis

- Docker + VS Code/Cursor avec Dev Containers
- Ou : .NET 7.0 SDK

## 🛠️ Technologies

- **Frontend** : Avalonia UI (C# / .NET 7.0)
- **Base de données** : SQLite + Entity Framework Core
- **Tests** : xUnit + Fluent Assertions + Moq
- **Archives** : SharpCompress (ZIP, 7Z, RAR)
- **Checksums** : MD5, SHA1, SHA256, CRC32

## ✨ Fonctionnalités

- ✅ Scan de répertoires (récursif)
- ✅ Support des archives (ZIP, 7Z, RAR, imbriquées)
- ✅ Calcul de checksums
- ✅ Détection automatique de console
- ✅ Identification via bases de données de référence
- 🚧 Gestionnaire de bases de données (NoIntro, Redump, GoodSet)
- 🚧 Filtrage et groupement des ROMs
- 🚧 Export vers Recalbox et Romm
- 🚧 Synchronisation des métadonnées
- 🚧 Interface Bibliothèque

## 📂 Structure du Projet

```
RomPilot/
├── src/
│   ├── RomPilot.Core/       # Logique métier, modèles, services
│   ├── RomPilot.UI/          # Interface Avalonia
│   └── RomPilot.Tests/       # Tests unitaires et intégration
├── specs/                    # Documentation des spécifications
│   └── 001-rom-manager-app/
│       ├── spec.md           # Spécifications fonctionnelles
│       ├── plan.md           # Plan d'implémentation
│       ├── tasks.md          # Tâches détaillées
│       └── data-model.md     # Modèle de données
├── scripts/                  # Scripts utilitaires
│   └── run-tests.sh         # Script de lancement des tests
├── .vscode/                  # Configuration VS Code
│   ├── launch.json          # Configurations de debug
│   ├── tasks.json           # Tâches automatisées
│   └── settings.json        # Paramètres du projet
├── .devcontainer/           # Configuration du dev container
└── .editorconfig            # Standards de code

```

## 🧪 Tests

```bash
# Tous les tests
./scripts/run-tests.sh

# Avec couverture
./scripts/run-tests.sh --coverage

# Tests filtrés
./scripts/run-tests.sh --filter ScanService

# Mode verbeux
./scripts/run-tests.sh --verbose
```

**Résultats actuels** : ✅ 36 tests / 36 passés

## 📖 Documentation

- [Guide de Configuration de l'Environnement](DEV-SETUP.md) - Setup complet pour développeurs
- [Spécifications](specs/001-rom-manager-app/spec.md) - Détails des fonctionnalités
- [Plan d'Implémentation](specs/001-rom-manager-app/plan.md) - Architecture et roadmap
- [Modèle de Données](specs/001-rom-manager-app/data-model.md) - Structure de la base de données

## 🎯 État d'Avancement

### ✅ Itération 1 : Fondations
- [x] Modèle de données (ScannedFile, ExclusionFilter, ReferenceDatabase, ExportConfiguration)
- [x] Repositories et services de base
- [x] Scan de fichiers et archives
- [x] Calcul de checksums
- [x] Identification de console
- [x] Persistence SQLite
- [x] Tests unitaires et intégration
- [x] Configuration environnement de développement

### 🚧 Itération 2 : Scanner Amélioré (US1)
- [ ] Scan sans présupposition (tous les fichiers)
- [ ] Application des filtres d'exclusion
- [ ] Scans incrémentaux (Quick vs Full)
- [ ] Interface pour gérer les filtres

### 🚧 Itération 3 : Gestionnaire de Bases de Données (US2)
- [x] Service DatabaseManager
- [ ] Interface UI pour le gestionnaire
- [ ] Téléchargement réel des bases
- [ ] Parsing des fichiers DAT

### 📋 Itération 4 : Filtrage et Export (US3, US4)
- [ ] Regroupement par version
- [ ] Sélection selon préférences (région, langue, format vidéo)
- [ ] Export local et SSH/SFTP

### 📋 Itération 5 : Visualisation et Synchronisation (US5, US6)
- [ ] Interface Bibliothèque
- [ ] Synchronisation métadonnées bidirectionnelle

## 🤝 Contribution

1. Fork le projet
2. Créer une branche feature (`git checkout -b feature/AmazingFeature`)
3. Commit les changements (`git commit -m 'Add AmazingFeature'`)
4. Push vers la branche (`git push origin feature/AmazingFeature`)
5. Ouvrir une Pull Request

### Standards de Code

- Formatage automatique à la sauvegarde (`.editorconfig`)
- Tests obligatoires pour les nouvelles fonctionnalités
- Coverage cible : 80%+

## 📜 Licence

Voir le fichier [LICENSE](LICENSE)

## 🙏 Remerciements

- Avalonia UI Team
- SharpCompress
- NoIntro, Redump, GoodSet pour les bases de données de référence
