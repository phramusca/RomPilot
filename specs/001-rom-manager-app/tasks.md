---
description: "Task list template for feature implementation"
---

# Tasks: ROM Manager Application

**Input**: Design documents from `/specs/001-rom-manager-app/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Tests are OPTIONAL - following TDD principles from constitution, tests should be written before implementation. Tests are included below but marked as optional.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions

- **Project structure**: `src/RomPilot.Core/`, `src/RomPilot.UI/`, `src/RomPilot.Tests/`
- Paths follow the modular structure from plan.md

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [x] T001 Create solution file RomPilot.sln in repository root
- [x] T002 [P] Create RomPilot.Core class library project in src/RomPilot.Core/RomPilot.Core.csproj
- [x] T003 [P] Create RomPilot.UI Avalonia MVVM project in src/RomPilot.UI/RomPilot.UI.csproj
- [x] T004 [P] Create RomPilot.Tests xUnit project in src/RomPilot.Tests/RomPilot.Tests.csproj
- [x] T005 Add project references: UI references Core, Tests references Core
- [x] T006 [P] Install NuGet packages in RomPilot.Core: Microsoft.EntityFrameworkCore.Sqlite, Microsoft.EntityFrameworkCore.Design, SharpCompress
- [x] T007 [P] Install NuGet packages in RomPilot.UI: CommunityToolkit.Mvvm
- [x] T008 [P] Install NuGet packages in RomPilot.Tests: Moq, FluentAssertions, Microsoft.EntityFrameworkCore.InMemory
- [x] T009 Configure .gitignore for .NET projects (bin/, obj/, *.db, etc.) in .gitignore

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T010 Create database context class in src/RomPilot.Core/Database/RomPilotDbContext.cs
- [x] T011 [P] Create Console entity model in src/RomPilot.Core/Models/Console.cs
- [x] T012 [P] Create RomFile entity model in src/RomPilot.Core/Models/RomFile.cs
- [x] T013 [P] Create Checksum entity model in src/RomPilot.Core/Models/Checksum.cs
- [x] T014 [P] Create DatabaseSource entity model in src/RomPilot.Core/Models/DatabaseSource.cs
- [x] T015 [P] Create GameEntry entity model in src/RomPilot.Core/Models/GameEntry.cs
- [x] T016 [P] Create Game entity model in src/RomPilot.Core/Models/Game.cs
- [x] T017 [P] Create GameRomVersion entity model in src/RomPilot.Core/Models/GameRomVersion.cs
- [x] T018 [P] Create Metadata entity model in src/RomPilot.Core/Models/Metadata.cs
- [x] T019 [P] Create UserData entity model in src/RomPilot.Core/Models/UserData.cs
- [x] T020 [P] Create UserPreferences entity model in src/RomPilot.Core/Models/UserPreferences.cs
- [x] T021 Configure Entity Framework Core DbContext with all entity relationships and indexes in src/RomPilot.Core/Database/RomPilotDbContext.cs
- [x] T022 Create initial EF Core migration InitialCreate in src/RomPilot.Core/Database/Migrations/
- [x] T023 [P] Create repository interface IRomFileRepository in src/RomPilot.Core/Repositories/IRomFileRepository.cs
- [x] T024 [P] Create repository interface IGameRepository in src/RomPilot.Core/Repositories/IGameRepository.cs
- [x] T025 [P] Create repository interface IChecksumRepository in src/RomPilot.Core/Repositories/IChecksumRepository.cs
- [x] T026 [P] Create repository interface IConsoleRepository in src/RomPilot.Core/Repositories/IConsoleRepository.cs
- [x] T027 [P] Create repository interface IDatabaseSourceRepository in src/RomPilot.Core/Repositories/IDatabaseSourceRepository.cs
- [x] T028 [P] Create repository interface IMetadataRepository in src/RomPilot.Core/Repositories/IMetadataRepository.cs
- [x] T029 [P] Create repository implementation RomFileRepository in src/RomPilot.Core/Repositories/RomFileRepository.cs
- [x] T030 [P] Create repository implementation GameRepository in src/RomPilot.Core/Repositories/GameRepository.cs
- [x] T031 [P] Create repository implementation ChecksumRepository in src/RomPilot.Core/Repositories/ChecksumRepository.cs
- [x] T032 [P] Create repository implementation ConsoleRepository in src/RomPilot.Core/Repositories/ConsoleRepository.cs
- [x] T033 [P] Create repository implementation DatabaseSourceRepository in src/RomPilot.Core/Repositories/DatabaseSourceRepository.cs
- [x] T034 [P] Create repository implementation MetadataRepository in src/RomPilot.Core/Repositories/MetadataRepository.cs
- [x] T035 Create checksum calculator interface IChecksumCalculator in src/RomPilot.Core/Checksums/IChecksumCalculator.cs
- [x] T036 Create checksum calculator implementation ChecksumCalculator in src/RomPilot.Core/Checksums/ChecksumCalculator.cs (MD5, SHA1, SHA256, CRC32)
- [x] T037 Create archive scanner interface IArchiveScanner in src/RomPilot.Core/Archives/IArchiveScanner.cs
- [x] T038 Create archive scanner implementation ArchiveScanner in src/RomPilot.Core/Archives/ArchiveScanner.cs (ZIP, 7Z, recursive up to 5 levels)
- [x] T039 Create console detection service interface IConsoleDetectionService in src/RomPilot.Core/Services/IConsoleDetectionService.cs
- [x] T040 Create console detection service implementation ConsoleDetectionService in src/RomPilot.Core/Services/ConsoleDetectionService.cs
- [x] T041 Create database seed data service SeedDataService in src/RomPilot.Core/Database/SeedDataService.cs (35+ consoles, database sources)
- [x] T042 Create user preferences service interface IUserPreferencesService in src/RomPilot.Core/Preferences/IUserPreferencesService.cs
- [x] T043 Create user preferences service implementation UserPreferencesService in src/RomPilot.Core/Preferences/UserPreferencesService.cs
- [x] T044 Configure dependency injection container in src/RomPilot.UI/App.axaml.cs (register all services and repositories)

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Scanner et identifier les ROMs (Priority: P1) 🎯 MVP

**Goal**: Scanner récursivement des répertoires (tous les sous-dossiers) pour trouver des ROMs (y compris dans archives ZIP/7Z/RAR avec archives imbriquées), calculer checksums, identifier console et jeu via bases de données, afficher feedback détaillé avec liste des fichiers traités/échoués et raisons d'échec

**Independent Test**: Fournir un répertoire de test avec ROMs dans différents formats (fichiers directs dans sous-dossiers, ZIP, 7Z, RAR, archives imbriquées). Vérifier que tous les ROMs sont détectés récursivement, consoles identifiées, jeux reconnus via bases de données, checksums calculés, et que la liste des fichiers traités/échoués est visible avec raisons d'échec.

### Tests for User Story 1 (OPTIONAL - TDD approach) ⚠️

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T045 [P] [US1] Unit test for ArchiveScanner recursive directory traversal in src/RomPilot.Tests/Unit/Archives/ArchiveScannerRecursiveTests.cs
- [x] T046 [P] [US1] Unit test for ArchiveScanner RAR support in src/RomPilot.Tests/Unit/Archives/ArchiveScannerRarTests.cs
- [x] T047 [P] [US1] Unit test for ArchiveScanner nested archives (ZIP, 7Z, RAR) in src/RomPilot.Tests/Unit/Archives/ArchiveScannerNestedTests.cs
- [x] T048 [P] [US1] Unit test for ChecksumCalculator in src/RomPilot.Tests/Unit/Checksums/ChecksumCalculatorTests.cs
- [x] T049 [P] [US1] Unit test for ConsoleDetectionService in src/RomPilot.Tests/Unit/Services/ConsoleDetectionServiceTests.cs
- [x] T050 [P] [US1] Unit test for ScanService with detailed progress reporting in src/RomPilot.Tests/Unit/Services/ScanServiceProgressTests.cs
- [x] T051 [US1] Integration test for scan workflow with detailed feedback in src/RomPilot.Tests/Integration/ScanServiceIntegrationTests.cs

### Implementation for User Story 1

- [x] T052 [US1] Create scan service interface IScanService in src/RomPilot.Core/Services/IScanService.cs
- [x] T053 [US1] Create scan service implementation ScanService in src/RomPilot.Core/Services/ScanService.cs (orchestrates scan workflow)
- [x] T054 [P] [US1] Create database provider interface INoIntroProvider in src/RomPilot.Core/DatabaseProviders/INoIntroProvider.cs
- [x] T055 [P] [US1] Create database provider interface IRedumpProvider in src/RomPilot.Core/DatabaseProviders/IRedumpProvider.cs
- [x] T056 [P] [US1] Create database provider interface IGoodSetProvider in src/RomPilot.Core/DatabaseProviders/IGoodSetProvider.cs
- [x] T057 [P] [US1] Create database provider implementation NoIntroProvider in src/RomPilot.Core/DatabaseProviders/NoIntroProvider.cs (parser datfile)
- [x] T058 [P] [US1] Create database provider implementation RedumpProvider in src/RomPilot.Core/DatabaseProviders/RedumpProvider.cs (parser datfile)
- [x] T059 [P] [US1] Create database provider implementation GoodSetProvider in src/RomPilot.Core/DatabaseProviders/GoodSetProvider.cs (parser datfile)
- [x] T060 [US1] Create game identification service interface IGameIdentificationService in src/RomPilot.Core/Services/IGameIdentificationService.cs
- [x] T061 [US1] Create game identification service implementation GameIdentificationService in src/RomPilot.Core/Services/GameIdentificationService.cs (matches checksums with database entries)
- [x] T062 [US1] Create scan progress reporting interface IScanProgressReporter in src/RomPilot.Core/Services/IScanProgressReporter.cs
- [x] T063 [US1] Create scan progress reporting implementation ScanProgressReporter in src/RomPilot.Core/Services/ScanProgressReporter.cs
- [x] T064 [US1] Create main window view model MainWindowViewModel in src/RomPilot.UI/ViewModels/MainWindowViewModel.cs
- [x] T065 [US1] Create main window view MainWindow in src/RomPilot.UI/Views/MainWindow.axaml
- [x] T066 [US1] Create scan view model ScanViewModel in src/RomPilot.UI/ViewModels/ScanViewModel.cs
- [x] T067 [US1] Create scan view ScanView in src/RomPilot.UI/Views/ScanView.axaml (directory selection, scan button, progress bar)
- [x] T068 [US1] Create scan results view model ScanResultsViewModel in src/RomPilot.UI/ViewModels/ScanResultsViewModel.cs
- [x] T069 [US1] Create scan results view ScanResultsView in src/RomPilot.UI/Views/ScanResultsView.axaml (list of detected ROMs with console and game info)
- [x] T070 [US1] Integrate scan service with UI in src/RomPilot.UI/ViewModels/ScanViewModel.cs (async scan, progress updates)
- [x] T071 [US1] Add error handling and user feedback in src/RomPilot.UI/ViewModels/ScanViewModel.cs (error messages, validation)

### NEW: Updates Required for Spec Clarifications

- [ ] T072 [US1] Update ArchiveScanner to support RAR format: add RAR detection in IsArchiveFile() method in src/RomPilot.Core/Archives/ArchiveScanner.cs
- [ ] T073 [US1] Update ArchiveScanner to support RAR format: add RAR archive opening logic in ScanArchiveAsync() method in src/RomPilot.Core/Archives/ArchiveScanner.cs
- [ ] T074 [US1] Update ArchiveScanner to support RAR format: add RAR extraction logic in ExtractFileAsync() method in src/RomPilot.Core/Archives/ArchiveScanner.cs
- [ ] T075 [US1] Update IArchiveScanner interface documentation to mention RAR support in src/RomPilot.Core/Archives/IArchiveScanner.cs
- [ ] T076 [US1] Verify ArchiveScanner recursive directory traversal works correctly (all subdirectories) in src/RomPilot.Core/Archives/ArchiveScanner.cs
- [ ] T077 [US1] Verify ArchiveScanner recursive archive processing works for all formats (ZIP, 7Z, RAR) in src/RomPilot.Core/Archives/ArchiveScanner.cs
- [ ] T078 [US1] Add ProcessingStatus property to RomFile model (success, failed) in src/RomPilot.Core/Models/RomFile.cs
- [ ] T079 [US1] Add FailureReason property to RomFile model (explicit failure reasons) in src/RomPilot.Core/Models/RomFile.cs
- [ ] T080 [US1] Create EF Core migration for RomFile ProcessingStatus and FailureReason fields in src/RomPilot.Core/Migrations/
- [ ] T081 [US1] Enhance IScanProgressReporter interface to report per-file status with failure reasons in src/RomPilot.Core/Services/IScanProgressReporter.cs
- [ ] T082 [US1] Update ScanProgressReporter implementation to support file-level status reporting in src/RomPilot.Core/Services/ScanProgressReporter.cs
- [ ] T083 [US1] Update ScanService to capture and report failure reasons for each file in src/RomPilot.Core/Services/ScanService.cs
- [ ] T084 [US1] Update ScanViewModel to display detailed file list (processed/failed with reasons) in src/RomPilot.UI/ViewModels/ScanViewModel.cs
- [ ] T085 [US1] Update ScanView.axaml to show processed files list with status (success, console identified, game identified) in src/RomPilot.UI/Views/ScanView.axaml
- [ ] T086 [US1] Update ScanView.axaml to show failed files list with explicit failure reasons in src/RomPilot.UI/Views/ScanView.axaml

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently - users can scan directories recursively (including subdirectories and nested archives in ZIP/7Z/RAR), see identified ROMs, and view detailed feedback with processed/failed files and failure reasons

---

## Phase 4: User Story 2 - Grouper et filtrer les ROMs par version (Priority: P2)

**Goal**: Regrouper automatiquement toutes les versions d'un même jeu, puis sélectionner automatiquement la meilleure version selon préférences utilisateur (région, langue, qualité)

**Independent Test**: Fournir une collection contenant plusieurs versions du même jeu (USA, EUR, JAP). Configurer préférences région/langue. Vérifier que versions sont regroupées et version préférée est sélectionnée.

### Tests for User Story 2 (OPTIONAL - TDD approach) ⚠️

- [ ] T087 [P] [US2] Unit test for GameGroupingService in src/RomPilot.Tests/Unit/Services/GameGroupingServiceTests.cs
- [ ] T088 [P] [US2] Unit test for VersionSelectionService in src/RomPilot.Tests/Unit/Services/VersionSelectionServiceTests.cs
- [ ] T089 [US2] Integration test for filtering workflow in src/RomPilot.Tests/Integration/FilteringIntegrationTests.cs

### Implementation for User Story 2

- [ ] T090 [US2] Create game grouping service interface IGameGroupingService in src/RomPilot.Core/Services/IGameGroupingService.cs
- [ ] T091 [US2] Create game grouping service implementation GameGroupingService in src/RomPilot.Core/Services/GameGroupingService.cs (groups ROMs by game)
- [ ] T092 [US2] Create version selection service interface IVersionSelectionService in src/RomPilot.Core/Services/IVersionSelectionService.cs
- [ ] T093 [US2] Create version selection service implementation VersionSelectionService in src/RomPilot.Core/Services/VersionSelectionService.cs (selects best version based on preferences)
- [ ] T094 [US2] Create preference configuration view model PreferencesViewModel in src/RomPilot.UI/ViewModels/PreferencesViewModel.cs
- [ ] T095 [US2] Create preference configuration view PreferencesView in src/RomPilot.UI/Views/PreferencesView.axaml (region priority, language priority, exclude bad dumps)
- [ ] T096 [US2] Create game list view model GameListViewModel in src/RomPilot.UI/ViewModels/GameListViewModel.cs (displays grouped games with selected versions)
- [ ] T097 [US2] Create game list view GameListView in src/RomPilot.UI/Views/GameListView.axaml (list of games with version info)
- [ ] T098 [US2] Integrate grouping and filtering with UI in src/RomPilot.UI/ViewModels/GameListViewModel.cs (apply preferences, update selection)
- [ ] T099 [US2] Add manual version override capability in src/RomPilot.UI/ViewModels/GameListViewModel.cs (user can manually select different version)

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently - users can scan, group games, and filter versions

---

## Phase 5: User Story 3 - Exporter vers Recalbox et Romm (Priority: P3)

**Goal**: Exporter collection de ROMs filtrée vers Recalbox et Romm en respectant conventions de chaque plateforme (noms dossiers, formats fichiers)

**Independent Test**: Exporter une petite collection vers dossiers destination Recalbox et Romm. Vérifier que fichiers sont placés dans bons dossiers, formats respectent conventions, structure correcte.

### Tests for User Story 3 (OPTIONAL - TDD approach) ⚠️

- [ ] T100 [P] [US3] Unit test for RecalboxExportAdapter in src/RomPilot.Tests/Unit/PlatformAdapters/RecalboxExportAdapterTests.cs
- [ ] T101 [P] [US3] Unit test for RommExportAdapter in src/RomPilot.Tests/Unit/PlatformAdapters/RommExportAdapterTests.cs
- [ ] T102 [US3] Integration test for export workflow in src/RomPilot.Tests/Integration/ExportIntegrationTests.cs

### Implementation for User Story 3

- [ ] T103 [US3] Create export service interface IExportService in src/RomPilot.Core/Services/IExportService.cs
- [ ] T104 [US3] Create export service implementation ExportService in src/RomPilot.Core/Services/ExportService.cs (orchestrates export)
- [ ] T105 [P] [US3] Create platform adapter interface IPlatformAdapter in src/RomPilot.Core/PlatformAdapters/IPlatformAdapter.cs
- [ ] T106 [US3] Create Recalbox export adapter RecalboxExportAdapter in src/RomPilot.Core/PlatformAdapters/RecalboxExportAdapter.cs (folder naming, format conversion)
- [ ] T107 [US3] Create Romm export adapter RommExportAdapter in src/RomPilot.Core/PlatformAdapters/RommExportAdapter.cs (folder structure, API integration)
- [ ] T108 [US3] Create console folder name mapping service ConsoleFolderMappingService in src/RomPilot.Core/Services/ConsoleFolderMappingService.cs (Recalbox and Romm conventions)
- [ ] T109 [US3] Create export configuration view model ExportViewModel in src/RomPilot.UI/ViewModels/ExportViewModel.cs
- [ ] T110 [US3] Create export configuration view ExportView in src/RomPilot.UI/Views/ExportView.axaml (platform selection, destination path, options)
- [ ] T111 [US3] Create export progress view model ExportProgressViewModel in src/RomPilot.UI/ViewModels/ExportProgressViewModel.cs
- [ ] T112 [US3] Create export progress view ExportProgressView in src/RomPilot.UI/Views/ExportProgressView.axaml (progress bar, file list)
- [ ] T113 [US3] Integrate export service with UI in src/RomPilot.UI/ViewModels/ExportViewModel.cs (async export, progress updates)
- [ ] T114 [US3] Add conflict resolution handling in src/RomPilot.Core/Services/ExportService.cs (file exists, user confirmation)
- [ ] T115 [US3] Create export history repository interface IExportHistoryRepository in src/RomPilot.Core/Repositories/IExportHistoryRepository.cs
- [ ] T116 [US3] Create export history repository implementation ExportHistoryRepository in src/RomPilot.Core/Repositories/ExportHistoryRepository.cs

**Checkpoint**: At this point, User Stories 1, 2, AND 3 should all work independently - users can scan, filter, and export to platforms

---

## Phase 6: User Story 4 - Synchroniser les métadonnées (Priority: P4)

**Goal**: Synchroniser métadonnées de jeux entre application et plateformes Recalbox et Romm (récupération scrap, synchronisation bidirectionnelle données utilisateur)

**Independent Test**: Configurer connexion Recalbox (gamelist.xml) et Romm (API). Vérifier que métadonnées scrap sont récupérées, favoris/ratings synchronisés bidirectionnellement, conflits gérés.

### Tests for User Story 4 (OPTIONAL - TDD approach) ⚠️

- [ ] T117 [P] [US4] Unit test for RecalboxMetadataAdapter in src/RomPilot.Tests/Unit/PlatformAdapters/RecalboxMetadataAdapterTests.cs
- [ ] T118 [P] [US4] Unit test for RommApiClient in src/RomPilot.Tests/Unit/PlatformAdapters/RommApiClientTests.cs
- [ ] T119 [US4] Integration test for sync workflow in src/RomPilot.Tests/Integration/SyncIntegrationTests.cs

### Implementation for User Story 4

- [ ] T120 [US4] Create sync service interface ISyncService in src/RomPilot.Core/Services/ISyncService.cs
- [ ] T121 [US4] Create sync service implementation SyncService in src/RomPilot.Core/Services/SyncService.cs (orchestrates sync)
- [ ] T122 [US4] Create Recalbox metadata adapter RecalboxMetadataAdapter in src/RomPilot.Core/PlatformAdapters/RecalboxMetadataAdapter.cs (read/write gamelist.xml)
- [ ] T123 [US4] Create Romm API client interface IRommApiClient in src/RomPilot.Core/PlatformAdapters/IRommApiClient.cs
- [ ] T124 [US4] Create Romm API client implementation RommApiClient in src/RomPilot.Core/PlatformAdapters/RommApiClient.cs (HTTP client, endpoints)
- [ ] T125 [US4] Create metadata mapping service MetadataMappingService in src/RomPilot.Core/Services/MetadataMappingService.cs (maps between formats)
- [ ] T126 [US4] Create conflict resolution service ConflictResolutionService in src/RomPilot.Core/Services/ConflictResolutionService.cs (last modified wins, user confirmation)
- [ ] T127 [US4] Create sync configuration view model SyncViewModel in src/RomPilot.UI/ViewModels/SyncViewModel.cs
- [ ] T128 [US4] Create sync configuration view SyncView in src/RomPilot.UI/Views/SyncView.axaml (platform selection, sync direction, options)
- [ ] T129 [US4] Create sync progress view model SyncProgressViewModel in src/RomPilot.UI/ViewModels/SyncProgressViewModel.cs
- [ ] T130 [US4] Create sync progress view SyncProgressView in src/RomPilot.UI/Views/SyncProgressView.axaml (progress bar, conflicts list)
- [ ] T131 [US4] Integrate sync service with UI in src/RomPilot.UI/ViewModels/SyncViewModel.cs (async sync, progress updates)
- [ ] T132 [US4] Add conflict resolution UI ConflictResolutionView in src/RomPilot.UI/Views/ConflictResolutionView.axaml (show conflicts, user choice)
- [ ] T133 [US4] Create sync history repository interface ISyncHistoryRepository in src/RomPilot.Core/Repositories/ISyncHistoryRepository.cs
- [ ] T134 [US4] Create sync history repository implementation SyncHistoryRepository in src/RomPilot.Core/Repositories/SyncHistoryRepository.cs

**Checkpoint**: All user stories should now be independently functional - complete workflow from scan to sync

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T135 [P] Documentation updates in README.md (installation, usage, features)
- [ ] T136 [P] Code cleanup and refactoring in src/ (remove unused code, improve naming)
- [ ] T137 [P] Performance optimization across all stories in src/RomPilot.Core/ (profiling, bottlenecks)
- [ ] T138 [P] Additional unit tests in src/RomPilot.Tests/Unit/ (increase coverage to 80% critical, 60% rest)
- [ ] T139 [P] Security hardening in src/RomPilot.Core/ (validate user inputs, sanitize paths, secure API key storage)
- [ ] T140 [P] Error handling improvements in src/RomPilot.Core/ (comprehensive error messages, logging)
- [ ] T141 [P] UI/UX polish in src/RomPilot.UI/ (icons, tooltips, keyboard shortcuts, accessibility)
- [ ] T142 [P] Localization support preparation in src/RomPilot.UI/ (if needed, prepare for translations)
- [ ] T143 [P] Run quickstart.md validation (verify all setup steps work)
- [ ] T144 [P] Create deployment packages (Linux AppImage/.deb, Windows MSI, macOS .dmg)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3+)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2 → P3 → P4)
- **Polish (Final Phase)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) - Depends on US1 for scanned ROMs data
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) - Depends on US2 for filtered/selected ROMs
- **User Story 4 (P4)**: Can start after Foundational (Phase 2) - Depends on US3 for exported games (for sync)

### Within Each User Story

- Tests (if included) MUST be written and FAIL before implementation
- Models before services
- Services before UI/adapters
- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- Once Foundational phase completes, user stories can start in parallel (if team capacity allows)
- All tests for a user story marked [P] can run in parallel
- Models within a story marked [P] can run in parallel
- Different user stories can be worked on in parallel by different team members (with coordination)

---

## Parallel Example: User Story 1

```bash
# Launch all tests for User Story 1 together (if tests requested):
Task: "Unit test for ArchiveScanner recursive directory traversal in src/RomPilot.Tests/Unit/Archives/ArchiveScannerRecursiveTests.cs"
Task: "Unit test for ArchiveScanner RAR support in src/RomPilot.Tests/Unit/Archives/ArchiveScannerRarTests.cs"
Task: "Unit test for ArchiveScanner nested archives in src/RomPilot.Tests/Unit/Archives/ArchiveScannerNestedTests.cs"

# Launch all database providers together:
Task: "Create database provider interfaces in src/RomPilot.Core/DatabaseProviders/..."
Task: "Create database provider implementations in src/RomPilot.Core/DatabaseProviders/..."

# Launch RAR support updates together:
Task: "Update ArchiveScanner to support RAR format: add RAR detection in IsArchiveFile()"
Task: "Update ArchiveScanner to support RAR format: add RAR archive opening logic"
Task: "Update ArchiveScanner to support RAR format: add RAR extraction logic"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1 (including NEW tasks T072-T086 for spec updates)
4. **STOP and VALIDATE**: Test User Story 1 independently
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → Deploy/Demo (MVP!)
3. Add User Story 2 → Test independently → Deploy/Demo
4. Add User Story 3 → Test independently → Deploy/Demo
5. Add User Story 4 → Test independently → Deploy/Demo
6. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (including RAR support and detailed feedback)
   - Developer B: User Story 2 (after US1 data available)
   - Developer C: User Story 3 (after US2 filtering available)
   - Developer D: User Story 4 (after US3 export available)
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests fail before implementing (TDD)
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
- **NEW tasks T072-T086** address spec clarifications: RAR support, detailed progress feedback, recursive scanning verification
- Total tasks: 144 (Setup: 9, Foundational: 35, US1: 35 [25 existing + 10 new], US2: 13, US3: 17, US4: 18, Polish: 10)
