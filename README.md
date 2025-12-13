# 🎮 RomPilot

Application de gestion de ROMs (lecture seule) avec support de multiples formats, identification automatique, et export vers Recalbox et Romm.

## 🚀 Démarrage Rapide

### 1. Ouvrir dans le Dev Container
```bash
VS Code/Cursor > Dev Containers: Reopen in Container
```

### 2. Lancer l'application
```bash
# Avec F5 > "🚀 Lancer l'UI (Sans Debug)"
# Ou :
dotnet run --project src/RomPilot.UI/RomPilot.UI.csproj
```

### 3. Lancer les tests
```bash
# Avec F5 > "🧪 Lancer les Tests"
# Ou :
dotnet test
```

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
├── .vscode/                  # Configurations de lancement et tâches
│   ├── launch.json          # F5 : Lancer l'UI et les tests
│   └── tasks.json           # Tâches de build, test, format
└── .devcontainer/            # Configuration du dev container
    └── devcontainer.json    # Extensions et settings VS Code/Cursor
```

## 🧪 Tests

```bash
# Tous les tests
dotnet test

# Avec couverture
dotnet test --collect:"XPlat Code Coverage"

# Tests filtrés
dotnet test --filter "FullyQualifiedName~ScanService"

# Mode verbeux
dotnet test --logger "console;verbosity=detailed"
```

**Résultats actuels** : ✅ 36 tests / 36 passés

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
