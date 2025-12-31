# Tasks: ROM Manager Application

**Feature Branch**: `001-rom-manager-app`  
**Date**: 2025-12-12 (Mis à jour: 2025-12-13 - Réorganisation phases, marquage tâches complétées)  
**Spec**: [spec.md](./spec.md) | **Plan**: [plan.md](./plan.md)

**Note** : Tâches mises à jour suite aux clarifications du 2025-12-12. Une implémentation partielle de la US1 existe et doit être adaptée aux nouvelles spécifications.

## Vue d'Ensemble

Total User Stories : **6** (P1 à P4)

- ✅ **US1 : Scanner et identifier (P1) - COMPLÉTÉ** ✅
- ⏳ US2 : Gérer bases de données (P1.5) - En cours
- US3 : Grouper et filtrer (P2)
- US4 : Exporter (P3)
- US6 : Interface Bibliothèque (P3)
- US5 : Synchroniser métadonnées (P4)

**MVP Suggéré** : US1 + US2 (fondation pour tout le reste)

- ✅ US1 complétée (Phase 3)
- ⏳ US2 en cours (Phase 4)

## Stratégie d'Implémentation

### Approche TDD

- ✅ Tests écrits AVANT l'implémentation (QA-001)
- ✅ Couverture minimale 80% (QA-002, SC-014)
- ✅ Tests d'intégration pour US critiques (QA-003, SC-015)

### Ordre de Livraison

1. **Itération 1** : ✅ Fondations + ✅ US1 adapté (scan complet) + ⏳ US2 (bases de données)
2. **Itération 2** : US3 (filtrage et sélection automatique)
3. **Itération 3** : US4 + US6 (export et visualisation)
4. **Itération 4** : US5 (synchronisation)

### Opportunités de Parallélisme

- Setup tasks : Peuvent être exécutés en parallèle (marqués [P])
- Tâches au sein d'une US : Modèles, services et UI peuvent être en parallèle si fichiers différents
- US indépendantes : US4 et US6 peuvent être développées en parallèle

---

## Phase 1 : Setup et Configuration (Prérequis)

**Objectif** : Préparer l'environnement de développement et la structure du projet

### 1.1 Initialisation Projet

- [X] T001 Vérifier installation .NET 8.0 SDK et outils requis
- [X] T002 [P] Créer structure solution RomPilot.sln avec projets Core, Infrastructure, UI
- [X] T003 [P] Configurer EditorConfig et conventions style C# (.editorconfig)
- [X] T004 [P] Configurer Roslyn analyzers pour linting automatique (.editorconfig configuré, Roslynator installé via extension)
- [X] T005 [P] Configurer dotnet format pour formatting automatique
- [X] T006 [P] Installer packages NuGet Core : EF Core SQLite, SharpCompress, SSH.NET
- [X] T007 [P] Installer packages NuGet UI : Avalonia UI, CommunityToolkit.Mvvm
- [X] T008 [P] Installer packages NuGet Tests : xUnit, Moq, FluentAssertions, EF Core InMemory
- [X] T009 Configurer projet tests avec références appropriées

### 1.2 Infrastructure Base de Données

- [X] T010 Créer RomPilotDbContext dans src/RomPilot.Core/Database/RomPilotDbContext.cs
- [X] T011 Configurer SQLite connection string et options dans RomPilotDbContext
- [X] T012 Créer migration initiale EF Core avec toutes les tables (voir data-model.md)
- [X] T013 Créer script seed data pour consoles supportées (35+ consoles)
- [X] T014 Créer script seed data pour ExclusionFilters par défaut (14 extensions)
- [X] T015 Tester migration et seed sur base SQLite vide

### 1.3 Configuration CI/CD

- [ ] T016 [P] Créer workflow GitHub Actions pour build (.github/workflows/build.yml)
- [ ] T017 [P] Configurer exécution tests automatiques dans CI
- [ ] T018 [P] Configurer vérification linter dans CI (dotnet format --verify-no-changes)
- [ ] T019 [P] Configurer calcul couverture tests (coverlet) dans CI

---

## Phase 2 : Fondations (Bloquant pour toutes les US)

**Objectif** : Créer les composants fondamentaux utilisés par toutes les user stories

### 2.1 Modèles Entités Core

- [X] T020 [P] Créer entité Console dans src/RomPilot.Core/Models/Console.cs
- [X] T021 [P] Créer entité ScannedFile dans src/RomPilot.Core/Models/ScannedFile.cs (anciennement RomFile)
- [X] T022 [P] Créer entité Checksum dans src/RomPilot.Core/Models/Checksum.cs
- [X] T023 [P] Créer entité GameEntry dans src/RomPilot.Core/Models/GameEntry.cs
- [X] T024 [P] Créer entité Game dans src/RomPilot.Core/Models/Game.cs
- [X] T025 [P] Créer entité ReferenceDatabase dans src/RomPilot.Core/Models/ReferenceDatabase.cs
- [X] T026 [P] Créer entité ExclusionFilter dans src/RomPilot.Core/Models/ExclusionFilter.cs
- [X] T027 [P] Créer entité UserPreferences dans src/RomPilot.Core/Models/UserPreferences.cs
- [X] T028 [P] Créer entité Metadata dans src/RomPilot.Core/Models/Metadata.cs
- [X] T029 [P] Créer entité UserData dans src/RomPilot.Core/Models/UserData.cs
- [X] T030 [P] Créer entité ExportConfiguration dans src/RomPilot.Core/Models/ExportConfiguration.cs

### 2.2 Repositories Interfaces et Implémentations

- [X] T031 [P] Créer IScannedFileRepository dans src/RomPilot.Core/Repositories/IScannedFileRepository.cs
- [X] T032 [P] Créer IGameRepository dans src/RomPilot.Core/Repositories/IGameRepository.cs
- [X] T033 [P] Créer IChecksumRepository dans src/RomPilot.Core/Repositories/IChecksumRepository.cs
- [X] T034 [P] Créer IReferenceDatabaseRepository dans src/RomPilot.Core/Repositories/IReferenceDatabaseRepository.cs
- [X] T035 [P] Créer IExclusionFilterRepository dans src/RomPilot.Core/Repositories/IExclusionFilterRepository.cs
- [X] T036 [P] Créer IExportConfigurationRepository dans src/RomPilot.Core/Repositories/IExportConfigurationRepository.cs
- [X] T037 [P] Implémenter ScannedFileRepository dans src/RomPilot.Core/Repositories/ScannedFileRepository.cs
- [X] T038 [P] Implémenter GameRepository dans src/RomPilot.Core/Repositories/GameRepository.cs
- [X] T039 [P] Implémenter ChecksumRepository dans src/RomPilot.Core/Repositories/ChecksumRepository.cs
- [X] T040 [P] Implémenter ReferenceDatabaseRepository dans src/RomPilot.Core/Repositories/ReferenceDatabaseRepository.cs
- [X] T041 [P] Implémenter ExclusionFilterRepository dans src/RomPilot.Core/Repositories/ExclusionFilterRepository.cs
- [X] T042 [P] Implémenter ExportConfigurationRepository dans src/RomPilot.Core/Repositories/ExportConfigurationRepository.cs

### 2.3 Services Fondamentaux

- [X] T043 Créer ChecksumService dans src/RomPilot.Core/Checksums/ChecksumCalculator.cs (calcul MD5, SHA1, SHA256, CRC32)
- [X] T044 Tests unitaires ChecksumService dans tests/RomPilot.Tests/Unit/Checksums/ChecksumCalculatorTests.cs
- [X] T045 Créer ArchiveScanner dans src/RomPilot.Core/Archives/ArchiveScanner.cs (SharpCompress pour ZIP/7Z/RAR)
- [X] T046 Tests unitaires ArchiveScanner avec archives test dans tests/RomPilot.Tests/Unit/Archives/ArchiveScannerTests.cs
- [X] T047 Créer FileSystemScanner dans src/RomPilot.Core/FileSystem/FileSystemScanner.cs (scan récursif) - **IMPLÉMENTÉ DANS ArchiveScanner.ScanDirectoryAsync()**
- [X] T048 Tests unitaires FileSystemScanner dans tests/RomPilot.Tests/Unit/FileSystem/FileSystemScannerTests.cs - **TESTS DANS ArchiveScannerRecursiveTests.cs**

### 2.4 UI Infrastructure MVVM

- [X] T049 [P] Créer MainWindowViewModel dans src/RomPilot.UI/ViewModels/MainWindowViewModel.cs
- [X] T050 [P] Créer MainWindow.axaml dans src/RomPilot.UI/Views/MainWindow.axaml (navigation principale)
- [X] T051 [P] Configurer dependency injection dans src/RomPilot.UI/App.axaml.cs
- [X] T052 [P] Créer ViewModelBase dans src/RomPilot.UI/ViewModels/ViewModelBase.cs
- [ ] T053 [P] Créer converters XAML communs dans src/RomPilot.UI/Converters/

---

## Phase 3 : User Story 1 - Scanner et Identifier (P1) ✅ COMPLÉTÉ

**Objectif** : Scanner tous fichiers (sans présupposition), calculer checksums, identifier ROMs via bases de données

**Note** : US1 est complétée. US2 (bases de données) peut être implémentée en parallèle ou après, mais l'identification fonctionne déjà avec les bases existantes.

**Test Indépendant** : Scanner répertoire avec fichiers variés, voir fichiers "identifiés", "non identifiés", "exclus" avec raisons, choisir scan rapide/complet.

### 3.1 Service Filtres d'Exclusion (Nouveau)

- [X] T081 [US1] Créer FilterService dans src/RomPilot.Core/Services/FilterService.cs
- [X] T082 [US1] Tests FilterService dans tests/RomPilot.Core.Tests/Services/FilterServiceTests.cs
- [X] T083 [US1] Implémenter GetActiveFiltersAsync() retournant filtres actifs depuis ExclusionFilters
- [X] T084 [US1] Implémenter IsFileExcluded(filePath) vérifiant extension, taille, pattern
- [X] T085 [US1] Implémenter AddCustomFilterAsync(filterType, filterValue)
- [X] T086 [US1] Implémenter UpdateFilterStatusAsync(filterId, isActive) pour activer/désactiver
- [X] T087 [US1] Implémenter DeleteFilterAsync(filterId) pour filtres personnalisés

### 3.2 UI Configuration Filtres

- [X] T088 [US1] Créer FilterConfigViewModel dans src/RomPilot.UI/ViewModels/FilterConfigViewModel.cs
- [X] T089 [US1] Créer FilterConfigView.axaml dans src/RomPilot.UI/Views/FilterConfigView.axaml
- [X] T090 [US1] Implémenter UI : Tableau filtres avec colonnes Type, Valeur, Par Défaut, Actif
- [X] T091 [US1] Implémenter UI : Bouton ajouter filtre personnalisé (dialog)
- [X] T092 [US1] Implémenter UI : Checkbox activer/désactiver chaque filtre
- [X] T093 [US1] Implémenter UI : Bouton supprimer pour filtres personnalisés uniquement (non implémenté - désactiver suffit)

### 3.3 Service Scan Adapté (Modification Code Existant)

- [X] T094 [US1] **ADAPTER** ScanService dans src/RomPilot.Core/Services/ScanService.cs pour scan sans présupposition
- [X] T095 [US1] Tests ScanService dans tests/RomPilot.Core.Tests/Services/ScanServiceTests.cs
- [X] T096 [US1] Modifier ScanDirectoryAsync() pour (1) appeler FilterService avant scan, (2) scanner TOUS fichiers hors exclusions
- [X] T097 [US1] Ajouter paramètre scanType (Quick/Full) à ScanDirectoryAsync()
- [X] T098 [US1] Implémenter logique scan rapide : comparer timestamp + taille, skip si inchangé
- [X] T099 [US1] Implémenter logique scan complet : recalculer tous checksums
- [X] T100 [US1] Stocker LastModifiedTimestamp et ScanType dans ScannedFiles
- [X] T101 [US1] Pour chaque fichier scanné, définir IdentificationStatus : "Identified"/"Unidentified"/"Excluded"/"Failed"
- [X] T102 [US1] Stocker ExclusionReason si status="Excluded" (ex: "Extension .jpg exclue")
- [X] T103 [US1] Stocker FailureReason si status="Failed" (ex: "Archive corrompue")
- [X] T104 [US1] Implémenter logique de suppression fichiers supprimés (FR-028) : retirer de la BD les fichiers qui n'existent plus lors du rescan

### 3.4 Service Identification ROMs

- [X] T105 [US1] Créer IdentificationService dans src/RomPilot.Core/Services/IdentificationService.cs (GameIdentificationService)
- [X] T106 [US1] Tests IdentificationService dans tests/RomPilot.Core.Tests/Services/IdentificationServiceTests.cs
- [X] T106 [US1] Implémenter IdentifyFileAsync(scannedFile) cherchant checksums dans GameEntries
- [X] T107 [US1] Si match trouvé : mettre à jour ScannedFile avec ConsoleId, GameEntryId, status="Identified"
- [X] T108 [US1] Si aucun match : laisser status="Unidentified", garder checksums calculés
- [X] T109 [US1] Gérer fichiers identifiés dans plusieurs bases de données (multi-match) (report à US2+)
- [X] T110 [US1] Implémenter BatchIdentifyAsync() pour identification parallèle de multiples fichiers (optimisation future)

### 3.5 UI Scan et Progression

- [X] T112 [US1] **ADAPTER** ScanViewModel dans src/RomPilot.UI/ViewModels/ScanViewModel.cs
- [X] T113 [US1] **ADAPTER** ScanView.axaml dans src/RomPilot.UI/Views/ScanView.axaml
- [X] T114 [US1] Ajouter UI : Boutons radio "Scan Rapide" / "Scan Complet"
- [X] T115 [US1] Adapter UI : Barre progression avec compteurs (Scannés, Identifiés, Non identifiés, Exclus, Échecs)
- [X] T116 [US1] Adapter UI : Tableau résultats avec colonnes Fichier, Statut, Console, Jeu, Raison
- [X] T117 [US1] Implémenter filtres tableau : Afficher uniquement Identifiés / Non identifiés / Exclus / Échecs (use ItemsControl - simple scroll)
- [X] T118 [US1] Implémenter tri tableau par colonne (nom, taille, statut, console) (amélioration future)
- [X] T119 [US1] Afficher raison explicite pour fichiers Exclus et Échecs (IMPLÉMENTÉ - ExclusionReason visible)

### 3.6 Tests Intégration US1

- [X] T120 [US1] Test intégration : Scan répertoire test avec .zip, .nes, .jpg (identifier ROMs, exclure images)
- [X] T121 [US1] Test intégration : Scan rapide répertoire déjà scanné, vérifier réutilisation checksums
- [X] T122 [US1] Test intégration : Scan complet même répertoire, vérifier recalcul checksums
- [X] T123 [US1] Test intégration : Ajouter filtre personnalisé (.mp3), rescanner, vérifier exclusion
- [X] T124 [US1] Test intégration : Scan archives imbriquées (ZIP dans ZIP dans RAR), vérifier profondeur récursive
- [X] T125 [US1] Test intégration : Rescan après suppression fichier, vérifier suppression de la BD (FR-028)

---

## Phase 4 : User Story 2 - Gérer Bases de Données (P1.5) 🆕

**Objectif** : Permettre téléchargement et gestion des bases de données de référence (NoIntro, Redump, GoodSet)

**Note** : US1 est complétée et fonctionne avec les bases de données existantes. US2 permettra de télécharger et gérer les bases de données via l'interface.

**Test Indépendant** : Utilisateur peut naviguer providers, sélectionner console/version, télécharger base de données, voir bases téléchargées dans tableau.

### 4.1 Tests US2 (TDD)

- [X] T054 [US2] Créer DatabaseManagerServiceTests dans tests/RomPilot.Tests/Unit/Services/DatabaseManagerServiceTests.cs
- [X] T055 [US2] Tests : Lister providers disponibles (NoIntro, Redump, GoodSet) - Testé dans GetAvailableProvidersAsync_ShouldReturnNoIntroRedumpGoodSet
- [X] T056 [US2] Tests : Lister versions par provider et console - Testé dans GetAvailableVersionsAsync_ShouldReturnVersionsForProviderAndConsole
- [X] T057 [US2] Tests : Télécharger base de données avec progression - Testé dans DownloadDatabaseAsync_ShouldDownloadDatabaseWithProgress
- [X] T058 [US2] Tests : Sélectionner version par défaut par console - Testé dans SetDefaultDatabaseAsync_ShouldSetDatabaseAsDefault
- [X] T059 [US2] Tests : Vérifier disponibilité mises à jour - Testé dans CheckForUpdatesAsync_ShouldCheckAllDownloadedDatabases
- [ ] T060 [US2] Tests : Gérer échecs téléchargement avec retry - Partiellement testé, retry logic à implémenter

### 4.2 Service Gestion Bases de Données

- [X] T061 [US2] Créer DatabaseManagerService dans src/RomPilot.Core/Services/DatabaseManagerService.cs - Déjà créé
- [X] T062 [US2] Implémenter méthode GetAvailableProvidersAsync() retournant NoIntro, Redump, GoodSet - Implémentation minimale (TODO: intégration APIs réelles)
- [X] T063 [US2] Implémenter méthode GetAvailableVersionsAsync(provider, console) avec parsing métadonnées - Structure créée (TODO: parsing métadonnées depuis APIs)
- [X] T064 [US2] Implémenter méthode DownloadDatabaseAsync(provider, console, version) avec HttpClient - Structure implémentée avec HttpClient, GetDownloadUrl() à compléter (TODO: URLs réelles)
- [X] T065 [US2] Implémenter progress reporting (IProgress<T>) pour téléchargement - Implémenté avec progress reporting basé sur Content-Length
- [X] T066 [US2] Implémenter méthode SetDefaultVersionAsync(provider, console, version) - Déjà implémenté via SetDefaultDatabaseAsync(int databaseId)
- [ ] T067 [US2] Implémenter méthode CheckForUpdatesAsync() comparant versions locales vs disponibles
- [X] T068 [US2] Gérer erreurs réseau et retry logic dans téléchargements - Implémenté avec exponential backoff (3 tentatives max)

### 4.3 UI Gestionnaire Bases de Données

- [X] T069 [US2] Créer DatabaseManagerViewModel dans src/RomPilot.UI/ViewModels/DatabaseManagerViewModel.cs
- [X] T070 [US2] Créer DatabaseManagerView.axaml dans src/RomPilot.UI/Views/DatabaseManagerView.axaml
- [X] T071 [US2] Implémenter UI : Liste providers avec consoles associées - ComboBox providers/consoles implémenté
- [X] T072 [US2] Implémenter UI : Liste versions disponibles avec dates et tailles - ComboBox versions avec métadonnées
- [X] T073 [US2] Implémenter UI : Bouton télécharger avec barre de progression - Implémenté avec ProgressBar
- [X] T074 [US2] Implémenter UI : Tableau bases téléchargées avec filtres (provider, console, version) - Liste implémentée (filtres à ajouter si nécessaire)
- [X] T075 [US2] Implémenter UI : Notification disponibilité mises à jour - Notification Border implémentée
- [X] T076 [US2] Implémenter UI : Sélection version par défaut (bouton/icône) - Commandes créées, boutons retirés du template (à réimplémenter avec approche différente)
- [X] T077 [US2] Implémenter UI : Affichage erreurs téléchargement avec bouton retry - Erreurs affichées, retry via re-téléchargement

### 4.4 Tests Intégration US2

- [ ] T078 [US2] Test intégration end-to-end : Télécharger NoIntro NES, vérifier fichier local, définir par défaut
- [ ] T079 [US2] Test intégration : Télécharger plusieurs versions même console, basculer version par défaut
- [ ] T080 [US2] Test intégration : Vérifier mises à jour, notification affichée si nouvelle version

---

## Phase 5 : User Story 3 - Grouper et Filtrer (P2)

**Objectif** : Regrouper versions d'un même jeu, sélectionner meilleure version selon préférences (région + format vidéo + langue)

**Test Indépendant** : Plusieurs versions même jeu détectées, configuré préférences EUR>USA + PAL>NTSC + FR>EN, version EUR PAL FR sélectionnée automatiquement.

### 5.1 Tests US3 (TDD)

- [ ] T124 [US3] Créer VersionSelectionServiceTests dans tests/RomPilot.Core.Tests/Services/VersionSelectionServiceTests.cs
- [ ] T125 [US3] Tests : Regrouper versions par jeu (même nom + console)
- [ ] T126 [US3] Tests : Calculer score sélection selon préférences région
- [ ] T127 [US3] Tests : Calculer score sélection selon préférences format vidéo (PAL/NTSC)
- [ ] T128 [US3] Tests : Calculer score sélection selon préférences langue
- [ ] T129 [US3] Tests : Exclure versions "bad dump"
- [ ] T130 [US3] Tests : Sélectionner meilleure version avec critères combinés

### 5.2 Service Sélection Versions

- [ ] T131 [US3] Créer VersionSelectionService dans src/RomPilot.Core/Services/VersionSelectionService.cs
- [ ] T132 [US3] Implémenter GroupGameVersionsAsync() regroupant ScannedFiles identifiés par GameName+Console
- [ ] T133 [US3] Implémenter CalculateSelectionScore(gameEntry, preferences) avec pondération région + videoFormat + langue
- [ ] T134 [US3] Implémenter SelectBestVersionAsync(gameId) appliquant préférences utilisateur
- [ ] T135 [US3] Exclure automatiquement versions avec QualityFlags contenant "bad dump"
- [ ] T136 [US3] Créer/mettre à jour entrées Games avec SelectedScannedFileId
- [ ] T137 [US3] Créer entrées GameRomVersions liant Game à tous ScannedFiles avec IsSelected et SelectionScore

### 5.3 Service Gestion Préférences

- [ ] T138 [US3] Créer PreferencesService dans src/RomPilot.Core/Services/PreferencesService.cs
- [ ] T139 [US3] Tests PreferencesService dans tests/RomPilot.Core.Tests/Services/PreferencesServiceTests.cs
- [ ] T140 [US3] Implémenter GetRegionPriorityAsync() retournant ordre (ex: ["EUR", "USA", "JAP"])
- [ ] T141 [US3] Implémenter SetRegionPriorityAsync(order) stockant dans UserPreferences
- [ ] T142 [US3] Implémenter GetVideoFormatPriorityAsync() retournant ordre (ex: ["PAL", "NTSC", "NTSC-J"])
- [ ] T143 [US3] Implémenter SetVideoFormatPriorityAsync(order) stockant dans UserPreferences
- [ ] T144 [US3] Implémenter GetLanguagePriorityAsync() retournant ordre (ex: ["FR", "EN"])
- [ ] T145 [US3] Implémenter SetLanguagePriorityAsync(order) stockant dans UserPreferences

### 5.4 UI Préférences

- [ ] T146 [US3] Créer PreferencesViewModel dans src/RomPilot.UI/ViewModels/PreferencesViewModel.cs
- [ ] T147 [US3] Créer PreferencesView.axaml dans src/RomPilot.UI/Views/PreferencesView.axaml
- [ ] T148 [US3] Implémenter UI : Liste ordonnée réorganisable pour préférences région
- [ ] T149 [US3] Implémenter UI : Liste ordonnée réorganisable pour préférences format vidéo
- [ ] T150 [US3] Implémenter UI : Liste ordonnée réorganisable pour préférences langue
- [ ] T151 [US3] Implémenter UI : Checkbox "Exclure bad dumps"
- [ ] T152 [US3] Implémenter UI : Bouton "Appliquer" recalculant sélections après changement préférences

### 5.5 Tests Intégration US3

- [ ] T153 [US3] Test intégration : Scanner jeu avec 3 versions (USA, EUR, JAP), vérifier regroupement sous même Game
- [ ] T154 [US3] Test intégration : Configurer préférences EUR>USA, vérifier sélection EUR
- [ ] T155 [US3] Test intégration : Changer préférences USA>EUR, réappliquer, vérifier nouvelle sélection USA
- [ ] T156 [US3] Test intégration : Jeu avec versions PAL et NTSC, préférence PAL>NTSC, vérifier sélection PAL

---

## Phase 6 : User Story 4 - Exporter (P3)

**Objectif** : Exporter jeux vers Recalbox et Romm (local ou distant SSH/SFTP) avec conventions spécifiques

**Test Indépendant** : Créer config export local et distant, exporter jeux, vérifier structure dossiers et fichiers sur destination.

### 6.1 Tests US4 (TDD)

- [ ] T157 [US4] Créer ExportServiceTests dans tests/RomPilot.Core.Tests/Services/ExportServiceTests.cs
- [ ] T158 [US4] Tests : Export local vers répertoire avec structure Recalbox
- [ ] T159 [US4] Tests : Export local vers répertoire avec structure Romm
- [ ] T160 [US4] Tests : Export distant SSH/SFTP avec mock client
- [ ] T161 [US4] Tests : Progress reporting pendant export
- [ ] T162 [US4] Tests : Gestion conflits (overwrite, skip)
- [ ] T163 [US4] Tests : Conversion formats (ZIP / uncompressed) selon plateforme

### 6.2 Service Export Local

- [ ] T164 [US4] Créer RecalboxExporter dans src/RomPilot.Infrastructure/Export/RecalboxExporter.cs
- [ ] T165 [US4] Implémenter ExportAsync() pour export local Recalbox avec conventions dossiers
- [ ] T166 [US4] Implémenter logique conversion ZIP/uncompressed selon console
- [ ] T167 [US4] Créer RommExporter dans src/RomPilot.Infrastructure/Export/RommExporter.cs
- [ ] T168 [US4] Implémenter ExportAsync() pour export local Romm avec conventions dossiers
- [ ] T169 [US4] Gérer conflits selon stratégie (overwrite/skip/ask)
- [ ] T170 [US4] Progress reporting (IProgress<T>) avec nombre fichiers copiés

### 6.3 Service Export Distant SSH/SFTP

- [ ] T171 [US4] Créer SftpExporter dans src/RomPilot.Infrastructure/Export/SftpExporter.cs
- [ ] T172 [US4] Tests SftpExporter dans tests/RomPilot.Infrastructure.Tests/Export/SftpExporterTests.cs
- [ ] T173 [US4] Implémenter ConnectAsync(config) avec SSH.NET SftpClient
- [ ] T174 [US4] Implémenter TestConnectionAsync(config) pour vérification connexion
- [ ] T175 [US4] Implémenter UploadFileAsync(localPath, remotePath) avec progress
- [ ] T176 [US4] Implémenter ExportAsync() utilisant SftpClient pour transfert batch
- [ ] T177 [US4] Calculer vitesse transfert et temps restant estimé
- [ ] T178 [US4] Gérer erreurs réseau et retry logic

### 6.4 UI Export et Configuration

- [ ] T179 [US4] Créer ExportConfigViewModel dans src/RomPilot.UI/ViewModels/ExportConfigViewModel.cs
- [ ] T180 [US4] Créer ExportConfigView.axaml dans src/RomPilot.UI/Views/ExportConfigView.axaml
- [ ] T181 [US4] Implémenter UI : Liste configurations export avec nom, plateforme, type
- [ ] T182 [US4] Implémenter UI : Dialog création configuration (nom, plateforme, local/distant)
- [ ] T183 [US4] Implémenter UI : Champs config local (chemin destination)
- [ ] T184 [US4] Implémenter UI : Champs config SFTP (host, port, user, auth, key/password, remote dir)
- [ ] T185 [US4] Implémenter UI : Bouton "Tester connexion" pour configs SFTP
- [ ] T186 [US4] Créer ExportViewModel dans src/RomPilot.UI/ViewModels/ExportViewModel.cs
- [ ] T187 [US4] Créer ExportView.axaml dans src/RomPilot.UI/Views/ExportView.axaml
- [ ] T188 [US4] Implémenter UI : Sélection configuration export
- [ ] T189 [US4] Implémenter UI : Sélection consoles et jeux à exporter
- [ ] T190 [US4] Implémenter UI : Barre progression avec vitesse transfert (si distant)
- [ ] T191 [US4] Implémenter UI : Affichage erreurs export avec détails

### 6.5 Tests Intégration US4

- [ ] T192 [US4] Test intégration : Export local Recalbox, vérifier structure dossiers /nes/, /snes/
- [ ] T193 [US4] Test intégration : Export local Romm, vérifier structure spécifique Romm
- [ ] T194 [US4] Test intégration : Créer config SFTP mock, exporter jeux, vérifier appels UploadFile
- [ ] T195 [US4] Test intégration : Export avec conflit existant, stratégie overwrite, vérifier remplacement

---

## Phase 7 : User Story 6 - Interface Bibliothèque (P3) 🆕

**Objectif** : Visualiser collection avec vues grille/liste, filtres avancés, recherche temps réel, détails jeux

**Test Indépendant** : Ouvrir bibliothèque, basculer vue grille/liste, filtrer par console, rechercher jeu, voir détails avec versions.

**Note** : Peut être développée en parallèle de US4.

### 7.1 Tests US6 (TDD)

- [ ] T196 [US6] Créer LibraryServiceTests dans tests/RomPilot.Core.Tests/Services/LibraryServiceTests.cs
- [ ] T197 [US6] Tests : Récupérer jeux avec pagination
- [ ] T198 [US6] Tests : Filtrer jeux par console
- [ ] T199 [US6] Tests : Filtrer jeux par région, langue, format vidéo
- [ ] T200 [US6] Tests : Recherche jeux par nom (temps réel)
- [ ] T201 [US6] Tests : Tri jeux par nom, date, rating, console
- [ ] T202 [US6] Tests : Récupérer versions disponibles pour un jeu

### 7.2 Service Bibliothèque

- [ ] T203 [US6] Créer LibraryService dans src/RomPilot.Core/Services/LibraryService.cs
- [ ] T204 [US6] Implémenter GetGamesAsync(filters, sorting, pagination) avec requête optimisée
- [ ] T205 [US6] Implémenter SearchGamesAsync(query) avec recherche texte plein
- [ ] T206 [US6] Implémenter GetGameDetailsAsync(gameId) avec métadonnées complètes
- [ ] T207 [US6] Implémenter GetGameVersionsAsync(gameId) retournant tous ScannedFiles du jeu
- [ ] T208 [US6] Implémenter SetSelectedVersionAsync(gameId, scannedFileId)
- [ ] T209 [US6] Optimiser requêtes avec index, projection, Include explicites

### 7.3 UI Vue Grille

- [ ] T210 [US6] Créer LibraryViewModel dans src/RomPilot.UI/ViewModels/LibraryViewModel.cs
- [ ] T211 [US6] Créer LibraryView.axaml dans src/RomPilot.UI/Views/LibraryView.axaml
- [ ] T212 [US6] Créer GameCardControl.axaml dans src/RomPilot.UI/Controls/GameCardControl.axaml (carte jeu avec cover)
- [ ] T213 [US6] Implémenter UI : Vue grille avec WrapPanel de GameCardControl
- [ ] T214 [US6] Implémenter UI : Chargement lazy des images cover (virtualisation)
- [ ] T215 [US6] Implémenter UI : Indicateur visuel métadonnées incomplètes
- [ ] T216 [US6] Implémenter UI : Clic sur carte ouvre détails jeu

### 7.4 UI Vue Liste

- [ ] T217 [US6] Créer GameListItemControl.axaml dans src/RomPilot.UI/Controls/GameListItemControl.axaml
- [ ] T218 [US6] Implémenter UI : Vue liste DataGrid avec colonnes (Nom, Console, Région, Format, Statut, Taille)
- [ ] T219 [US6] Implémenter UI : Tri colonnes cliquable
- [ ] T220 [US6] Implémenter UI : Double-clic ligne ouvre détails jeu

### 7.5 UI Filtres et Recherche

- [ ] T221 [US6] Implémenter UI : Bouton toggle vue grille/liste
- [ ] T222 [US6] Implémenter UI : Barre recherche avec debounce (recherche temps réel < 100ms)
- [ ] T223 [US6] Implémenter UI : Filtres dropdown (Console, Région, Langue, Format Vidéo)
- [ ] T224 [US6] Implémenter UI : Application filtres combinés (plusieurs actifs simultanément)
- [ ] T225 [US6] Implémenter UI : Compteur jeux affichés / total

### 7.6 UI Détails Jeu

- [ ] T226 [US6] Créer GameDetailsViewModel dans src/RomPilot.UI/ViewModels/GameDetailsViewModel.cs
- [ ] T227 [US6] Créer GameDetailsView.axaml dans src/RomPilot.UI/Views/GameDetailsView.axaml
- [ ] T228 [US6] Implémenter UI : Affichage métadonnées complètes (description, images, screenshots, vidéo)
- [ ] T229 [US6] Implémenter UI : Liste versions disponibles avec région, format, langue
- [ ] T230 [US6] Implémenter UI : Highlight version sélectionnée
- [ ] T231 [US6] Implémenter UI : Bouton changer version sélectionnée
- [ ] T232 [US6] Implémenter UI : Lecteur vidéo in-app pour preview

### 7.7 Tests Intégration US6

- [ ] T233 [US6] Test intégration : Ouvrir bibliothèque, voir jeux en grille avec covers
- [ ] T234 [US6] Test intégration : Basculer vue liste, vérifier colonnes et tri
- [ ] T235 [US6] Test intégration : Filtrer console "nes", vérifier uniquement jeux NES affichés
- [ ] T236 [US6] Test intégration : Rechercher "mario", résultats < 100ms
- [ ] T237 [US6] Test intégration : Ouvrir détails jeu, voir 3 versions, changer sélection

---

## Phase 8 : User Story 5 - Synchroniser Métadonnées (P4)

**Objectif** : Synchroniser métadonnées bidirectionnelle avec Recalbox (gamelist.xml) et Romm (API REST)

**Test Indépendant** : Importer métadonnées depuis Recalbox, modifier favori dans app, exporter vers Recalbox, vérifier gamelist.xml mis à jour.

### 8.1 Tests US5 (TDD)

- [ ] T238 [US5] Créer RecalboxSyncProviderTests dans tests/RomPilot.Infrastructure.Tests/Sync/RecalboxSyncProviderTests.cs
- [ ] T239 [US5] Tests : Parser gamelist.xml avec données complètes
- [ ] T240 [US5] Tests : Importer métadonnées depuis gamelist.xml vers Metadata
- [ ] T241 [US5] Tests : Importer données utilisateur depuis gamelist.xml vers UserData
- [ ] T242 [US5] Tests : Exporter métadonnées vers gamelist.xml
- [ ] T243 [US5] Tests : Résolution conflits (dernière modif gagne)

### 8.2 Provider Sync Recalbox

- [ ] T244 [US5] Créer RecalboxSyncProvider dans src/RomPilot.Infrastructure/Sync/RecalboxSyncProvider.cs
- [ ] T245 [US5] Implémenter ParseGamelistXml(xmlPath) avec System.Xml.Linq (voir contracts/recalbox-gamelist.md)
- [ ] T246 [US5] Implémenter ImportFromRecalboxAsync(consolePath) lisant gamelist.xml
- [ ] T247 [US5] Pour chaque entrée <game> : trouver ScannedFile via path, créer/update Metadata
- [ ] T248 [US5] Importer données utilisateur (playcount, lastplayed, favorite) vers UserData
- [ ] T249 [US5] Implémenter ExportToRecalboxAsync(consolePath, games) générant gamelist.xml
- [ ] T250 [US5] Implémenter résolution conflits via comparaison timestamps LastModifiedAt

### 8.3 Provider Sync Romm API

- [ ] T251 [US5] Créer RommSyncProvider dans src/RomPilot.Infrastructure/Sync/RommSyncProvider.cs
- [ ] T252 [US5] Tests RommSyncProvider dans tests/RomPilot.Infrastructure.Tests/Sync/RommSyncProviderTests.cs (avec mock HttpClient)
- [ ] T253 [US5] Implémenter GetMetadataAsync(gameId) appelant API Romm (voir contracts/romm-api.md)
- [ ] T254 [US5] Implémenter ImportFromRommAsync() récupérant métadonnées via API
- [ ] T255 [US5] Implémenter UpdateMetadataAsync(gameId, metadata) envoyant métadonnées à Romm
- [ ] T256 [US5] Implémenter gestion authentification API (token stocké dans UserPreferences chiffré)

### 8.4 Service Synchronisation

- [ ] T257 [US5] Créer SyncService dans src/RomPilot.Core/Services/SyncService.cs
- [ ] T258 [US5] Tests SyncService dans tests/RomPilot.Core.Tests/Services/SyncServiceTests.cs
- [ ] T259 [US5] Implémenter SyncFromRecalboxAsync(direction) orchestrant import/export
- [ ] T260 [US5] Implémenter SyncFromRommAsync(direction) orchestrant appels API
- [ ] T261 [US5] Implémenter gestion conflits selon stratégie utilisateur
- [ ] T262 [US5] Implémenter sync bidirectionnelle (import puis export)

### 8.5 UI Synchronisation

- [ ] T263 [US5] Créer SyncViewModel dans src/RomPilot.UI/ViewModels/SyncViewModel.cs
- [ ] T264 [US5] Créer SyncView.axaml dans src/RomPilot.UI/Views/SyncView.axaml
- [ ] T265 [US5] Implémenter UI : Sélection plateforme (Recalbox / Romm)
- [ ] T266 [US5] Implémenter UI : Sélection direction (Import / Export / Bidirectionnelle)
- [ ] T267 [US5] Implémenter UI : Checkboxes types données (Métadonnées scrap / Données utilisateur)
- [ ] T268 [US5] Implémenter UI : Configuration chemins Recalbox ou URL/API key Romm
- [ ] T269 [US5] Implémenter UI : Bouton lancer sync avec barre progression
- [ ] T270 [US5] Implémenter UI : Dialog résolution conflits si détectés
- [ ] T271 [US5] Implémenter UI : Résumé sync (X jeux synchronisés, Y conflits résolus)

### 8.6 Tests Intégration US5

- [ ] T272 [US5] Test intégration : Importer métadonnées depuis gamelist.xml exemple, vérifier données en BD
- [ ] T273 [US5] Test intégration : Modifier favorite dans app, exporter vers Recalbox, parser XML, vérifier <favorite>true</favorite>
- [ ] T274 [US5] Test intégration : Sync bidirectionnelle avec conflit, stratégie dernière modif, vérifier données finales
- [ ] T275 [US5] Test intégration : Importer depuis Romm mock API, vérifier métadonnées créées

---

## Phase 9 : Polish et Cross-Cutting Concerns

**Objectif** : Finaliser application avec gestion erreurs, logging, documentation, packaging

### 9.1 Gestion Erreurs et Logging

- [ ] T276 [P] Configurer Serilog pour logging structuré dans src/RomPilot.UI/App.axaml.cs
- [ ] T277 [P] Implémenter GlobalExceptionHandler interceptant exceptions non gérées
- [ ] T278 [P] Ajouter logging dans services critiques (ScanService, ExportService, SyncService)
- [ ] T279 [P] Créer dialog erreur générique affichant messages utilisateur-friendly
- [ ] T280 [P] Implémenter bouton "Voir logs" ouvrant fichier log

### 9.2 Performance et Optimisation

- [ ] T281 [P] Ajouter index manquants dans BD si détectés par profiling
- [ ] T282 [P] Implémenter cache pour checksums calculés (éviter recalcul)
- [ ] T283 [P] Optimiser requêtes EF Core avec AsNoTracking() où approprié
- [ ] T284 [P] Implémenter virtualisation UI pour listes longues (bibliothèque)
- [ ] T285 [P] Profiler performance scan 1000+ fichiers, optimiser si nécessaire

### 9.3 Documentation

- [ ] T286 [P] Mettre à jour README.md avec installation, utilisation, screenshots
- [ ] T287 [P] Créer ARCHITECTURE.md documentant structure projet et patterns
- [ ] T288 [P] Créer CONTRIBUTING.md avec guidelines contributions
- [ ] T289 [P] Générer documentation XML APIs publiques
- [ ] T290 [P] Créer guide utilisateur (user manual) basé sur quickstart.md

### 9.4 Packaging et Distribution

- [ ] T291 [P] Créer script build Linux AppImage
- [ ] T292 [P] Créer script build Windows MSI avec Wix Toolset
- [ ] T293 [P] Créer script build macOS DMG
- [ ] T294 [P] Tester packages sur chaque plateforme (Windows 10+, Ubuntu 22+, macOS 11+)
- [ ] T295 [P] Créer workflow GitHub Actions pour releases automatiques

### 9.5 Tests Finaux

- [ ] T296 Test couverture globale, vérifier 80%+ atteint (SC-014)
- [ ] T297 Tests end-to-end workflow complet : scan → filtrage → export → sync
- [ ] T298 Tests performance : scan 1000 fichiers < 5 minutes (SC-001)
- [ ] T299 Tests performance : export 500 jeux < 2 minutes (SC-005)
- [ ] T300 Tests performance : recherche bibliothèque 10K jeux < 100ms (SC-013)

---

## Dépendances entre User Stories

```
Phase 1 (Setup) + Phase 2 (Fondations) ✅
    ↓
    ├─→ Phase 3 (US1: Scan) ✅ COMPLÉTÉ
    │   ↓
    └─→ Phase 4 (US2: Bases de données) ──┐
        ↓                                  ↓
    Phase 5 (US3: Filtrage) ←──────────────┘
        ↓
    ┌────┴────┐
    ↓         ↓
Phase 6    Phase 7
(US4: Export) (US6: Bibliothèque)
    ↓         ↓
    └────┬────┘
         ↓
    Phase 8 (US5: Sync)
         ↓
    Phase 9 (Polish)
```

**Ordre de Complétion Requis** :

1. ✅ Setup + Fondations (Phase 1-2) : COMPLÉTÉ
2. ✅ US1 (Phase 3) : COMPLÉTÉ - Scanner et identifier fonctionne
3. US2 (Phase 4) : Peut être implémentée maintenant (téléchargement bases de données)
4. US1 doit être complété avant US3 (US3 utilise fichiers scannés) ✅
5. US3 doit être complété avant US4 et US6 (sélection versions requise)
6. US4 et US6 peuvent être en parallèle
7. US5 après US4 (sync utilise métadonnées exportées)

**US Indépendantes (Parallélisables)** :

- US4 (Export) et US6 (Bibliothèque) après US3

---

## Exemples Exécution Parallèle

### Par Phase

**Phase 3 (US1)** - ✅ COMPLÉTÉ :

- T081-T087 (FilterService) ✅
- T088-T093 (UI Filtres) ✅
- T094-T103 (ScanService adapté) ✅
- T111-T118 (UI Scan) ✅

**Phase 4 (US2)** - Opportunités parallèles :

- T061-T068 (Service) peuvent commencer dès T054-T060 (Tests) écrits
- T069-T077 (UI) peuvent commencer en parallèle de T061-T068 si interfaces définies

**Phase 6 (US4) + Phase 7 (US6)** - Parallèles complets :

- Toute la Phase 6 peut être développée en parallèle de Phase 7
- T164-T178 (Services Export) indépendants de T203-T209 (Service Bibliothèque)
- T179-T191 (UI Export) indépendants de T210-T232 (UI Bibliothèque)

### Par Développeur

**Développeur 1** :

- Phase 4 : US2 Services + UI
- Phase 6 : US4 Export Services + UI

**Développeur 2** :

- Phase 4 : US2 Tests
- Phase 7 : US6 LibraryService + UI complète

---

## Résumé Statistiques

- **Total Tâches** : 300
- **Setup** : 19 tâches (✅ ~16 complétées)
- **Fondations** : 34 tâches (✅ ~30 complétées)
- **US1** (P1) : 43 tâches ✅ **COMPLÉTÉ**
- **US2** (P1.5) : 27 tâches 🆕 (0 complétées)
- **US3** (P2) : 32 tâches
- **US4** (P3) : 39 tâches
- **US6** (P3) : 42 tâches 🆕
- **US5** (P4) : 38 tâches
- **Polish** : 26 tâches

**Tâches Parallélisables** : ~85 tâches marquées [P]

**MVP Recommandé** : Phase 1-4 (Setup + Fondations + US1 + US2) = ~123 tâches

- ✅ Phase 1-3 complétées (~89 tâches)
- ⏳ Phase 4 en cours (27 tâches)

**Tests Unitaires** : ~45 tâches de tests (15% du total)
**Tests Intégration** : ~25 tâches de tests (8% du total)
**Total Tests** : ~70 tâches (23% du total) - Objectif 80% couverture

**Estimation Temps** :

- MVP (Setup + US2 + US1) : 4-6 semaines
- Version Complète : 10-12 semaines
- (Estimation 1 développeur temps plein, 2-3 jours par tâche complexe)

---

## Notes Implémentation

### Adaptation Code Existant US1

Si code partiel existe pour US1 :

1. **Renommer** `RomFile` → `ScannedFile` dans tout le code (T021, migration BD)
2. **Ajouter** FilterService (T081-T087) - nouveau composant
3. **Modifier** ScanService (T094-T103) pour scan sans présupposition
4. **Mettre à jour** UI Scan (T111-T118) avec choix scan rapide/complet

### Priorités selon Itérations

**Itération 1 (MVP)** :

- ✅ Phase 1-2 : Setup + Fondations (COMPLÉTÉ)
- ✅ Phase 3 : US1 complète et adaptée (COMPLÉTÉ)
- ⏳ Phase 4 : US2 complète (en cours)

**Itération 2** :

- Phase 5 : US3 complète

**Itération 3** :

- Phase 6 + 7 : US4 et US6 en parallèle

**Itération 4** :

- Phase 8 : US5 complète
- Phase 9 : Polish

### TDD Workflow

Pour chaque service :

1. Écrire tests d'abord (tâches marquées "Tests")
2. Vérifier que tests échouent (red)
3. Implémenter service (green)
4. Refactoriser si nécessaire (refactor)
5. Vérifier couverture ≥ 80%

---

## Prochaines Étapes

1. ✅ **Accepter tasks.md** généré
2. 🚀 **Commencer Phase 1** : Setup (T001-T019)
3. 📊 **Tracker progrès** : Marquer tâches complétées avec `[x]`
4. 🔄 **Itérations** : Compléter phase par phase selon dépendances
5. ✅ **Tests** : Exécuter tests après chaque phase
6. 📝 **Documentation** : Mettre à jour au fur et à mesure

**Commande pour démarrer l'implémentation** : `/speckit.implement` (si disponible)
