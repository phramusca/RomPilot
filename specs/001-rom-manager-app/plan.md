# Plan d'Implémentation: ROM Manager Application

**Branche**: `001-rom-manager-app` | **Date**: 2025-12-12 | **Spec**: [spec.md](./spec.md)
**Input**: Spécification de fonctionnalité depuis `/specs/001-rom-manager-app/spec.md`

**Note** : Ce plan a été mis à jour suite aux clarifications du 2025-12-12. Une implémentation partielle de la US1 existe mais doit être adaptée aux nouvelles spécifications (notamment : pas de présupposition sur ce qui est une ROM, filtres d'exclusion configurables, scans incrémentaux).

## Résumé

Application desktop multi-plateforme (Windows, Linux, macOS) pour gérer des collections de ROMs de jeux rétro. L'application scanne récursivement des répertoires et archives (ZIP, 7Z, RAR) sans présupposer quels fichiers sont des ROMs, calcule les checksums, identifie les jeux via bases de données de référence (NoIntro, Redump, GoodSet), groupe les versions, sélectionne automatiquement la meilleure version selon préférences utilisateur, et exporte vers Recalbox et Romm avec synchronisation des métadonnées.

## Contexte Technique

**Langage/Version**: C# 12 / .NET 8.0  
**Framework GUI**: Avalonia UI 11.x (framework desktop multi-plateforme)  
**Dépendances Principales**: 
- Entity Framework Core 8.0 (accès données)
- Microsoft.Data.Sqlite (provider SQLite)
- SharpCompress (gestion archives ZIP/7Z/RAR)
- SSH.NET (export SFTP)
- System.Text.Json (parsing JSON/API)
- System.Xml.Linq (parsing XML gamelist.xml)

**Stockage**: SQLite (base de données locale embarquée)  
**Tests**: xUnit + Moq + FluentAssertions  
**Target Platform**: Desktop (Windows 10+, Linux, macOS 11+)  
**Type de Projet**: Application desktop avec architecture MVVM  
**Objectifs de Performance**:
- Scan 1000+ fichiers en < 5 minutes (SC-001)
- Identification 95%+ ROMs sans intervention (SC-002)
- Export 500 jeux en < 2 minutes (SC-005)
- Recherche temps réel < 100ms pour 10K jeux (SC-013)

**Contraintes**:
- Sans présupposition : Ne pas filtrer fichiers par extension avant scan
- Exclusions configurables : Liste par défaut + personnalisations utilisateur
- Multi-plateforme : Support Windows, Linux, macOS avec fonctionnalité identique
- Offline-capable : Fonctionnement sans connexion sauf téléchargement bases de données et sync

**Échelle/Portée**:
- Collections 10 000+ fichiers ROMs
- 35+ consoles supportées
- 3 providers bases de données (NoIntro, Redump, GoodSet)
- 6 User Stories (scan, bases données, filtrage, export, sync, bibliothèque)

## Vérification Constitution

*GATE : Doit passer avant Phase 0 recherche. Revérifier après Phase 1 design.*

Vérification conformité avec les principes de la Constitution RomPilot :

- ✅ **Qualité du Code** : 
  - Code suivra conventions C#/.NET (StyleCop, EditorConfig)
  - Linter automatique (Roslyn analyzers) + formatter (dotnet format)
  - Documentation XML pour APIs publiques
  - Pas de code review humaine obligatoire (specs : linter/formatter uniquement)

- ✅ **Standards de Tests** :
  - TDD pour nouvelles fonctionnalités (specs : QA-001)
  - Couverture 80%+ tests unitaires (specs : QA-002, SC-014)
  - Tests d'intégration pour US critiques (specs : QA-003, SC-015)
  - CI/CD avec tests automatisés (specs : QA-006)
  - Types de tests : unitaires (xUnit), intégration (SQLite in-memory), contrats (API mocks)

- ✅ **Cohérence Expérience Utilisateur** :
  - Design system Avalonia (Material Design ou Fluent)
  - Navigation logique entre vues (Scan → Bases données → Bibliothèque → Export → Sync)
  - Messages d'erreur clairs et actionnables (specs : FR-023)
  - Feedback progression détaillé (specs : FR-022)
  - Responsive sur tous environnements desktop

- ✅ **Exigences de Performance** :
  - Objectifs mesurables définis (SC-001 à SC-013)
  - Benchmarks dans specs (5 min scan, 2 min export, 100ms recherche)
  - Monitoring via logging structuré (Serilog)
  - Optimisations : parallélisation scan, streaming archives, index BD

**Aucune violation** : Toutes les exigences de la Constitution sont respectées.

## Structure Projet

### Documentation (cette fonctionnalité)

```text
specs/001-rom-manager-app/
├── plan.md              # Ce fichier (mis à jour)
├── research.md          # Recherche technologique (existant)
├── data-model.md        # Modèle de données (à mettre à jour)
├── quickstart.md        # Guide démarrage rapide (à mettre à jour)
├── contracts/           # Contrats API
│   └── romm-api.md      # API Romm (existant)
└── tasks.md             # Tâches détaillées (Phase 2 - pas encore créé)
```

### Code Source (racine repository)

```text
src/
├── RomPilot.Core/                    # Logique métier
│   ├── Models/                       # Entités de domaine
│   │   ├── ScannedFile.cs           # Fichier scanné (ROM ou non)
│   │   ├── Game.cs                   # Jeu logique
│   │   ├── Console.cs                # Console/Plateforme
│   │   ├── Checksum.cs               # Checksums calculés
│   │   ├── GameEntry.cs              # Entrée base de données
│   │   ├── ReferenceDatabase.cs      # Base données de référence
│   │   └── ...
│   ├── Services/                     # Services métier
│   │   ├── ScanService.cs            # Service scan fichiers
│   │   ├── ChecksumService.cs        # Calcul checksums
│   │   ├── IdentificationService.cs  # Identification ROMs
│   │   ├── DatabaseManagerService.cs # Gestion bases données
│   │   ├── FilterService.cs          # Gestion filtres exclusion
│   │   ├── VersionSelectionService.cs # Sélection meilleures versions
│   │   ├── ExportService.cs          # Export Recalbox/Romm
│   │   ├── SyncService.cs            # Synchronisation métadonnées
│   │   └── ...
│   ├── Repositories/                 # Accès données
│   │   ├── IScannedFileRepository.cs
│   │   ├── IGameRepository.cs
│   │   ├── IChecksumRepository.cs
│   │   └── ...
│   └── Interfaces/                   # Interfaces services
│       └── ...
│
├── RomPilot.Infrastructure/          # Implémentation infrastructure
│   ├── Data/                         # Accès données
│   │   ├── RomPilotDbContext.cs      # Context Entity Framework
│   │   ├── Migrations/               # Migrations EF Core
│   │   └── Repositories/             # Implémentations repositories
│   ├── Archives/                     # Gestion archives
│   │   └── ArchiveScanner.cs         # Scanner archives (SharpCompress)
│   ├── FileSystem/                   # Gestion système fichiers
│   │   └── FileSystemScanner.cs
│   ├── Crypto/                       # Checksums
│   │   └── ChecksumCalculator.cs
│   ├── Export/                       # Export vers plateformes
│   │   ├── RecalboxExporter.cs
│   │   ├── RommExporter.cs
│   │   └── SftpExporter.cs           # Export SSH/SFTP
│   └── Sync/                         # Synchronisation
│       ├── RecalboxSyncProvider.cs
│       └── RommSyncProvider.cs
│
├── RomPilot.UI/                      # Interface utilisateur Avalonia
│   ├── ViewModels/                   # ViewModels MVVM
│   │   ├── MainWindowViewModel.cs
│   │   ├── ScanViewModel.cs          # US1 : Scan et identification
│   │   ├── DatabaseManagerViewModel.cs # US2 : Gestion bases données
│   │   ├── FilterConfigViewModel.cs  # Configuration filtres exclusion
│   │   ├── LibraryViewModel.cs       # US6 : Interface bibliothèque
│   │   ├── GameDetailsViewModel.cs   # Détails jeu + versions
│   │   ├── ExportViewModel.cs        # US4 : Export
│   │   ├── SyncViewModel.cs          # US5 : Synchronisation
│   │   ├── PreferencesViewModel.cs   # Préférences utilisateur
│   │   └── ...
│   ├── Views/                        # Vues XAML
│   │   ├── MainWindow.axaml
│   │   ├── ScanView.axaml
│   │   ├── DatabaseManagerView.axaml
│   │   ├── LibraryView.axaml
│   │   ├── GameDetailsView.axaml
│   │   ├── ExportView.axaml
│   │   └── ...
│   ├── Controls/                     # Contrôles réutilisables
│   │   ├── GameCardControl.axaml     # Carte jeu (vue grille)
│   │   ├── GameListItemControl.axaml # Item jeu (vue liste)
│   │   └── ProgressIndicator.axaml
│   ├── Converters/                   # Convertisseurs XAML
│   └── Resources/                    # Ressources (styles, images)
│
└── RomPilot.CLI/                     # Interface ligne de commande (optionnelle)
    └── Program.cs

tests/
├── RomPilot.Core.Tests/              # Tests unitaires Core
│   ├── Services/
│   ├── Models/
│   └── ...
├── RomPilot.Infrastructure.Tests/    # Tests unitaires Infrastructure
│   ├── Data/
│   ├── Archives/
│   └── ...
├── RomPilot.Integration.Tests/       # Tests d'intégration
│   ├── ScanWorkflowTests.cs          # US1 end-to-end
│   ├── DatabaseManagerTests.cs       # US2 end-to-end
│   ├── ExportWorkflowTests.cs        # US4 end-to-end
│   └── ...
└── RomPilot.UI.Tests/                # Tests UI (si faisable)
    └── ViewModelTests/

docs/
├── README.md                         # Documentation principale
├── ARCHITECTURE.md                   # Architecture application
└── CONTRIBUTING.md                   # Guide contribution
```

**Décision de Structure** : Architecture en couches (Core, Infrastructure, UI) suivant principes Clean Architecture. Séparation claire logique métier (Core) et détails implémentation (Infrastructure, UI). Permet testabilité et changement technologie si nécessaire.

## Suivi Complexité

> **À remplir UNIQUEMENT si Vérification Constitution a des violations à justifier**

Aucune violation constatée - pas de complexité injustifiée.

---

## Changements par rapport à l'Implémentation Partielle US1

### Modifications Nécessaires dans le Code Existant

1. **Renommage Entité "RomFile" → "ScannedFile"**
   - Tous les fichiers scannés, pas seulement ceux identifiés comme ROMs
   - Ajout champ `IdentificationStatus` (enum: Identified/Unidentified/Excluded/Failed)
   - Ajout champ `ExclusionReason` (si exclu : filtre par défaut ou personnalisé)
   - Ajout champs `LastModifiedTimestamp`, `ScanType` (quick/full)

2. **Nouveau Service : FilterService**
   - Gestion liste exclusions par défaut (.jpg, .png, .txt, .exe, etc.)
   - Gestion filtres personnalisés utilisateur (extensions, tailles, patterns)
   - Configuration modifiable depuis UI

3. **Scans Incrémentaux**
   - Ajout choix "scan rapide" vs "scan complet" dans UI
   - Stockage timestamp + taille fichier pour détection changements
   - Réutilisation checksums si fichier inchangé (scan rapide)

4. **Table ReferenceDatabases**
   - Nouvelle table pour stocker bases de données téléchargées
   - Champs : provider, console, version, release_date, file_path, is_default

5. **Préférences Format Vidéo**
   - Ajout champ `VideoFormat` dans GameEntries (PAL/NTSC/NTSC-J)
   - Séparé des préférences de région
   - Ordre de priorité configurable

6. **Export Configuration SSH/SFTP**
   - Ajout configuration connexion SSH/SFTP dans table ExportConfiguration
   - Champs : host, port, username, auth_method, key_path, remote_directory
   - Implémentation SftpExporter avec SSH.NET

### Nouvelles Fonctionnalités à Implémenter

1. **US2 : Gestionnaire de Bases de Données** (nouveau)
   - Interface dédiée téléchargement/gestion bases de données
   - Vue tableau bases téléchargées avec filtres
   - Sélection version par défaut par console

2. **US6 : Interface Bibliothèque** (nouveau)
   - Vue grille avec cover art
   - Vue liste avec colonnes triables
   - Filtres avancés combinables
   - Recherche temps réel
   - Détails jeu avec toutes versions

3. **Linter/Formatter Automatique**
   - Configuration Roslyn analyzers
   - Configuration dotnet format
   - Pre-commit hooks ou CI/CD

---

## Phase 0 : Recherche et Architecture ✅ TERMINÉ

**Output** : `research.md` - Décisions technologiques documentées

### Décisions Technologiques Confirmées

- ✅ **GUI Framework** : Avalonia UI 11.x (C#/.NET 8.0)
- ✅ **Base de données** : SQLite avec Entity Framework Core
- ✅ **Gestion archives** : SharpCompress (ZIP, 7Z, RAR)
- ✅ **Checksums** : System.Security.Cryptography (MD5, SHA1, SHA256, CRC32)
- ✅ **Architecture** : MVVM + Clean Architecture (Core/Infrastructure/UI)

### Nouvelles Recherches Nécessaires (Mises à Jour Specs)

- ✅ **Export SSH/SFTP** : SSH.NET (bibliothèque .NET pour SFTP)
- ✅ **Filtres Configurables** : Implémentation via table configuration + UI settings
- ✅ **Scans Incrémentaux** : Comparaison timestamp + file size (File.GetLastWriteTimeUtc, FileInfo.Length)

---

## Phase 1 : Design et Contrats

**Prérequis** : `research.md` complet

### 1.1 Mise à Jour Modèle de Données ⚠️ À METTRE À JOUR

**Output** : `data-model.md` (mise à jour)

#### Changements Requis dans `data-model.md`

1. **Renommer table `RomFiles` → `ScannedFiles`**
   ```sql
   CREATE TABLE ScannedFiles (
       Id INTEGER PRIMARY KEY AUTOINCREMENT,
       FilePath TEXT NOT NULL,
       FileName TEXT NOT NULL,
       FileSize BIGINT NOT NULL,
       LastModifiedTimestamp INTEGER NOT NULL,  -- Nouveau : Unix timestamp
       ArchivePath TEXT,
       ArchiveDepth INTEGER DEFAULT 0,
       IdentificationStatus TEXT NOT NULL,      -- Nouveau : "Identified", "Unidentified", "Excluded", "Failed"
       ExclusionReason TEXT,                     -- Nouveau : raison exclusion si applicable
       ConsoleId INTEGER,                        -- Nullable (NULL si non identifié)
       GameEntryId INTEGER,                      -- Nullable (NULL si non identifié)
       LastScannedAt DATETIME,
       ScanType TEXT,                            -- Nouveau : "Quick" ou "Full"
       FailureReason TEXT,                       -- Raison échec si Status = Failed
       FOREIGN KEY (ConsoleId) REFERENCES Consoles(Id),
       FOREIGN KEY (GameEntryId) REFERENCES GameEntries(Id)
   );
   ```

2. **Nouvelle table `ExclusionFilters`**
   ```sql
   CREATE TABLE ExclusionFilters (
       Id INTEGER PRIMARY KEY AUTOINCREMENT,
       FilterType TEXT NOT NULL,  -- "Extension", "SizeRange", "Pattern"
       FilterValue TEXT NOT NULL, -- Ex: ".jpg", "0-1KB", "*.tmp"
       IsDefault BOOLEAN NOT NULL DEFAULT 0,
       IsActive BOOLEAN NOT NULL DEFAULT 1,
       CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
   );
   
   -- Données par défaut (extensions)
   INSERT INTO ExclusionFilters (FilterType, FilterValue, IsDefault, IsActive)
   VALUES 
       ('Extension', '.jpg', 1, 1),
       ('Extension', '.jpeg', 1, 1),
       ('Extension', '.png', 1, 1),
       -- ... (toutes les extensions par défaut)
   ```

3. **Nouvelle table `ReferenceDatabases`** (distincte de `DatabaseSources`)
   ```sql
   CREATE TABLE ReferenceDatabases (
       Id INTEGER PRIMARY KEY AUTOINCREMENT,
       Provider TEXT NOT NULL,         -- "NoIntro", "Redump", "GoodSet"
       Console TEXT NOT NULL,
       Version TEXT NOT NULL,
       ReleaseDate TEXT,
       FileSize BIGINT,
       DownloadStatus TEXT NOT NULL,   -- "Available", "Downloading", "Downloaded", "Error"
       FilePath TEXT,
       IsDefault BOOLEAN NOT NULL DEFAULT 0,  -- Version par défaut pour cette console
       DownloadedAt DATETIME,
       LastCheckedAt DATETIME,
       UNIQUE(Provider, Console, Version)
   );
   
   CREATE INDEX IX_ReferenceDatabases_Provider_Console ON ReferenceDatabases(Provider, Console);
   ```

4. **Ajout champ `VideoFormat` dans `GameEntries`**
   ```sql
   ALTER TABLE GameEntries ADD COLUMN VideoFormat TEXT; -- "PAL", "NTSC", "NTSC-J"
   ```

5. **Mise à jour `UserPreferences` avec nouvelles préférences**
   ```sql
   -- Nouvelles clés préférences :
   -- "video_format_priority" : JSON array ["PAL", "NTSC", "NTSC-J"]
   -- "scan_mode_default" : "Quick" ou "Full"
   -- "sftp_host" : "192.168.1.100"
   -- "sftp_port" : "22"
   -- "sftp_username" : "user"
   -- "sftp_key_path" : "/path/to/key"
   ```

6. **Mise à jour `ExportConfiguration`**
   ```sql
   ALTER TABLE ExportConfiguration ADD COLUMN ExportType TEXT NOT NULL DEFAULT 'Local';  -- "Local" ou "Remote"
   ALTER TABLE ExportConfiguration ADD COLUMN SftpHost TEXT;
   ALTER TABLE ExportConfiguration ADD COLUMN SftpPort INTEGER;
   ALTER TABLE ExportConfiguration ADD COLUMN SftpUsername TEXT;
   ALTER TABLE ExportConfiguration ADD COLUMN SftpAuthMethod TEXT;  -- "Password" ou "Key"
   ALTER TABLE ExportConfiguration ADD COLUMN SftpKeyPath TEXT;
   ALTER TABLE ExportConfiguration ADD COLUMN RemoteDirectory TEXT;
   ```

### 1.2 Contrats API ✅ EXISTANT (Romm)

**Output** : `/contracts/romm-api.md` (existant - à vérifier)

#### Contrat À Créer : Recalbox gamelist.xml

**Nouveau fichier** : `/contracts/recalbox-gamelist.md`

```xml
<!-- Format gamelist.xml Recalbox -->
<?xml version="1.0"?>
<gameList>
    <game>
        <path>./game.zip</path>
        <name>Game Name</name>
        <desc>Game description</desc>
        <rating>0.85</rating>
        <releasedate>19900101T000000</releasedate>
        <developer>Developer</developer>
        <publisher>Publisher</publisher>
        <genre>Genre</genre>
        <players>1-2</players>
        <image>./images/game-image.png</image>
        <thumbnail>./images/game-thumb.png</thumbnail>
        <video>./videos/game-video.mp4</video>
        <playcount>5</playcount>
        <lastplayed>20240115T120000</lastplayed>
        <favorite>true</favorite>
    </game>
</gameList>
```

### 1.3 Guide Démarrage Rapide ⚠️ À METTRE À JOUR

**Output** : `quickstart.md` (mise à jour)

**Contenu** :
1. Prérequis : .NET 8.0 SDK
2. Clone repository
3. Build : `dotnet build`
4. Run : `dotnet run --project src/RomPilot.UI`
5. Tests : `dotnet test`
6. Premier scan : créer dossier test avec quelques fichiers
7. Configuration filtres exclusion
8. Téléchargement bases de données
9. Scan et identification
10. Export vers Recalbox/Romm

### 1.4 Mise à Jour Contexte Agent

**Action** : Exécuter `.specify/scripts/bash/update-agent-context.sh cursor-agent`

Technologies à ajouter au contexte :
- Avalonia UI 11.x (MVVM, XAML)
- Entity Framework Core 8.0 (SQLite)
- SharpCompress (archives)
- SSH.NET (SFTP)
- xUnit + Moq (tests)

---

## Phase 2 : Tâches Détaillées

**Note** : Phase 2 (`/speckit.tasks`) sera exécutée séparément après ce plan.

**Output prévu** : `tasks.md` avec décomposition complète des User Stories en tâches implémentables.

### Priorisation des User Stories

1. **P1 : US1 - Scanner et identifier** (partiellement implémenté - à adapter)
2. **P1.5 : US2 - Gérer bases de données** (nouveau - requis pour US1)
3. **P2 : US3 - Grouper et filtrer** (dépend de US1)
4. **P3 : US4 - Exporter** (dépend de US3)
5. **P3 : US6 - Interface Bibliothèque** (parallèle à US4)
6. **P4 : US5 - Synchroniser métadonnées** (final)

### Ordre Implémentation Recommandé

#### Itération 1 : Fondations (US2 + US1 mise à jour)
1. Mettre à jour modèle données (ScannedFiles, ExclusionFilters, ReferenceDatabases)
2. Implémenter US2 : Gestionnaire bases de données
3. Adapter US1 : Scan sans présupposition + filtres configurables + scans incrémentaux

#### Itération 2 : Filtrage et Sélection (US3)
4. Implémenter US3 : Regroupement versions + sélection automatique (région + format vidéo)

#### Itération 3 : Export et Visualisation (US4 + US6)
5. Implémenter US4 : Export local et SSH/SFTP vers Recalbox/Romm
6. Implémenter US6 : Interface Bibliothèque (vue grille/liste, filtres)

#### Itération 4 : Synchronisation (US5)
7. Implémenter US5 : Synchronisation métadonnées bidirectionnelle

---

## Revérification Constitution Post-Design

### Code Quality ✅
- Architecture Clean définie
- Linter Roslyn configuré
- Documentation XML pour APIs publiques

### Testing Standards ✅
- Strategy TDD définie
- 80% couverture cible
- Tests unitaires + intégration + contrats
- xUnit + Moq configurés

### User Experience Consistency ✅
- Design MVVM cohérent
- Navigation logique définie
- Feedback utilisateur détaillé (progression, erreurs)
- Messages actionnables

### Performance Requirements ✅
- Objectifs mesurables (SC-001 à SC-013)
- Optimisations identifiées (parallélisation, streaming, index)
- Monitoring via logging

**Résultat** : ✅ Tous les principes de la Constitution respectés.

---

## Artefacts Générés

- ✅ `plan.md` - Ce fichier (mis à jour)
- ✅ `research.md` - Recherche technologique (existant)
- ⚠️ `data-model.md` - **À METTRE À JOUR** avec nouvelles tables et champs
- ⚠️ `quickstart.md` - **À METTRE À JOUR** avec nouveaux workflows
- 📝 `/contracts/recalbox-gamelist.md` - **À CRÉER**
- 📝 `tasks.md` - À créer via `/speckit.tasks`

## Prochaine Étape

Exécuter `/speckit.tasks` pour décomposer les User Stories en tâches d'implémentation détaillées.
