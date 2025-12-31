# 🎮 RomPilot

Application de gestion de ROMs (lecture seule) avec support de multiples formats, identification automatique, et export vers Recalbox et Romm.

## 🚀 Démarrage Rapide

### 1. Ouvrir dans Cursor

```bash
cursor .
```

### 2. Reopen in Container

```bash
F1 > Dev Containers: Reopen in Container
# Choisir: "RomPilot"
```

### 3. Lancer l'Application avec Debug

```bash
# F5 > "🐛 Debug UI"
# Les breakpoints fonctionnent ! ✅
# Ou sans debug :
dotnet run --project src/RomPilot.UI/RomPilot.UI.csproj
```

### 4. Lancer les tests

```bash
# Avec la tâche VS Code: F1 > "Tasks: Run Task" > "🧪 test"
# Ou avec F5 > "🧪 Lancer les Tests"
# Ou en ligne de commande :
dotnet test
```

**Note** : Les données de test sont générées automatiquement lors de l'exécution des tests si elles sont manquantes. Pour forcer une régénération, supprimez-les d'abord avec la tâche `🗑️ clean-test-data`.

## 📋 Prérequis

- Docker + Cursor avec Dev Containers
- Ou : .NET 8.0 SDK

## 🛠️ Technologies

- **Frontend** : Avalonia UI (C# / .NET 8.0)
- **Base de données** : SQLite + Entity Framework Core
- **Tests** : xUnit + Fluent Assertions + Moq
- **Archives** : SharpCompress (ZIP, 7Z, RAR)
- **Checksums** : MD5, SHA1, SHA256, CRC32

## ✨ Fonctionnalités

### ✅ US1 : Scanner et Identifier (COMPLÉTÉ)

- **Scan sans présupposition** : Scanne TOUS les fichiers sans filtrer par extension
- **Filtres d'exclusion** : 36 filtres par défaut (images, textes, docs, etc.) + ajout de filtres personnalisés
- **Scans incrémentaux** : Mode Quick (réutilise checksums) vs Full (recalcule tout)
- **Compteurs détaillés** : Identifiés / Non identifiés / Exclus / Échecs
- **Interface de gestion** : Vue dédiée pour configurer les filtres
- **Support archives** : ZIP, 7Z, RAR avec récursion complète et formats mixtes (ZIP dans 7Z, RAR dans 7Z, etc.)
- **Calcul checksums** : MD5, SHA1, SHA256, CRC32
- ✅ Détection automatique de console
- ✅ Identification via bases de données de référence

### ⏳ En Cours de Développement

- 🔜 **US2** : Gestionnaire de bases de données (NoIntro, Redump, GoodSet)
- 🔜 **US3** : Export vers Recalbox
- 🔜 **US4** : Export vers Romm
- 🔜 **US5** : Gestion des doublons
- 🔜 **US6** : Statistiques et rapports

## 📂 Structure du Projet

```
RomPilot/
├── src/
│   ├── RomPilot.Core/              # Logique métier, modèles, services
│   ├── RomPilot.UI/                # Interface Avalonia
│   └── RomPilot.Tests/             # Tests unitaires et intégration
├── specs/                          # Documentation des spécifications
│   └── 001-rom-manager-app/
│       ├── spec.md                 # Spécifications fonctionnelles
│       ├── plan.md                 # Plan d'implémentation
│       ├── tasks.md                # Tâches détaillées
│       └── data-model.md           # Modèle de données
├── .vscode/                        # Configurations
│   ├── settings.json              # Settings
│   ├── launch.json               # Configurations de debug
│   └── tasks.json                 # Tâches (build, test, format)
├── .devcontainer/                  # Configuration Dev Container
│   ├── devcontainer.json         # Config Cursor avec extensions
│   ├── docker-compose.yml         # Docker Compose
│   └── Dockerfile                 # Image Docker (.NET 8.0)
```

## 🧪 Tests

### Génération des données de test

Les tests d'intégration nécessitent des fichiers de test générés dans le répertoire `test-data/`.

**Gérer les données de test :**

Les données de test sont générées automatiquement lors de l'exécution des tests si elles sont manquantes. Pour forcer une régénération, supprimez-les simplement :

```bash
# Supprimer les données (elles seront recréées au prochain test)
F1 > Tasks: Run Task > 🗑️ clean-test-data
```

Les données générées incluent trois scénarios :

- `test-data/simple/` : Scénario simple avec quelques ROMs et archives
- `test-data/medium/` : Scénario moyen avec archives imbriquées
- `test-data/load/` : Scénario de charge avec de nombreux fichiers

### Exécution des tests

```bash
# Tous les tests
dotnet test

# Avec la tâche VS Code
F1 > Tasks: Run Task > 🧪 test

# Avec couverture
dotnet test --collect:"XPlat Code Coverage"

# Tests filtrés
dotnet test --filter "FullyQualifiedName~ScanService"

# Mode verbeux
dotnet test --logger "console;verbosity=detailed"
```

**Résultats actuels** : ✅ 54 tests / 54 passés

## 🔧 Commandes Utiles

```bash
# Build
dotnet build

# Restaurer les dépendances
dotnet restore

# Formatage du code
dotnet format

# Nettoyage
dotnet clean

# Migrations EF Core
dotnet ef migrations add NomDeLaMigration --project src/RomPilot.Core
dotnet ef database update --project src/RomPilot.Core
```

## 📖 Documentation

- [Spécifications](specs/001-rom-manager-app/spec.md) - Détails des fonctionnalités
- [Plan d'Implémentation](specs/001-rom-manager-app/plan.md) - Architecture et roadmap
- [Modèle de Données](specs/001-rom-manager-app/data-model.md) - Structure de la base de données
- [Guide de Configuration](DEV-SETUP.md) - Troubleshooting et détails avancés

### 🔧 Environnement de Développement

- **IDE** : Cursor (avec Dev Containers)
- **Extensions** : `anysphere.csharp` (support C# officiel avec debug)
- **Debugger** : `coreclr` + `netcoredbg` (breakpoints fonctionnels)
- **.NET** : 8.0 (LTS)

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
