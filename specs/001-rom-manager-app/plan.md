# Implementation Plan: ROM Manager Application

**Branch**: `001-rom-manager-app` | **Date**: 2025-01-27 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-rom-manager-app/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Application de bureau multi-plateforme pour la gestion de collections de ROMs avec scan automatique, identification via bases de données (NoIntro, Redump, GoodSet), filtrage intelligent par version, export vers Recalbox et Romm, et synchronisation bidirectionnelle de métadonnées.

**Approche technique** : Application desktop native en C# avec Avalonia UI pour une interface graphique moderne et performante sur Linux, Windows et macOS. Architecture modulaire avec séparation claire entre UI, logique métier, et accès aux données. Base SQLite pour le stockage local. Support des archives ZIP/7Z avec scan récursif. Intégration avec API REST de Romm et fichiers gamelist.xml de Recalbox.

**Recommandation technologique** : C# avec Avalonia UI (recommandé) plutôt que Java/JavaFX ou Python/PyQt pour les raisons suivantes :
- **C# + Avalonia** : Connaissance solide du langage, framework GUI moderne et mature pour desktop, excellent support Linux, syntaxe XAML familière (similaire à WPF), performance native, écosystème .NET riche
- **Java + JavaFX** : Connaissance du langage mais JavaFX moins moderne, support Linux correct mais moins optimal que .NET, JVM plus lourde
- **Python + PyQt** : Connaissance limitée, performance moindre pour traitement de fichiers volumineux, packaging/distribution plus complexe, moins adapté pour application desktop native

## Technical Context

**Language/Version**: C# 12.0 (.NET 8.0)  
**Primary Dependencies**: 
- Avalonia UI 11.x (framework GUI multi-plateforme)
- SQLite (via Microsoft.Data.Sqlite ou Entity Framework Core)
- SharpCompress (gestion archives ZIP/7Z)
- System.Security.Cryptography (calcul checksums MD5, SHA1, SHA256, CRC32)
- System.Text.Json ou Newtonsoft.Json (parsing JSON pour API Romm)
- System.Xml.Linq (parsing XML pour gamelist.xml Recalbox)
- HttpClient (appels API REST Romm)

**Storage**: SQLite (fichier local, base de données embarquée)  
**Testing**: xUnit (framework de tests), Moq (mocking), FluentAssertions (assertions lisibles)  
**Target Platform**: Desktop (Linux prioritaire, Windows, macOS)  
**Project Type**: Single desktop application (application de bureau monolithique modulaire)  
**Performance Goals**: 
- Scan de 1000+ ROMs en <5 minutes
- Export de 500 jeux en <2 minutes
- Interface réactive (pas de blocage UI pendant opérations longues)
- Gestion mémoire efficace pour archives imbriquées (limite profondeur 5 niveaux)

**Constraints**: 
- Multi-plateforme (Linux, Windows, macOS) avec code partagé maximal
- Base SQLite locale (pas de serveur de base de données)
- Support archives ZIP et 7Z avec scan récursif
- Calcul de tous types de checksums requis (MD5, SHA1, SHA256, CRC32)
- Interface graphique moderne et intuitive
- Gestion d'erreurs robuste avec messages utilisateur clairs

**Scale/Scope**: 
- Collections de ROMs de taille variable (quelques centaines à plusieurs milliers)
- Support 35+ consoles rétro
- Bases de données de référence (NoIntro, Redump, GoodSet) avec mises à jour périodiques
- Synchronisation avec 2 plateformes externes (Recalbox, Romm)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Verify compliance with RomPilot Constitution principles:

- **Code Quality**: ✅ Code suivra les conventions C# (.NET coding guidelines), code reviews prévus via PRs, documentation XML pour APIs publiques, README pour modules complexes
- **Testing Standards**: ✅ Stratégie TDD définie : tests unitaires pour logique métier, tests d'intégration pour interactions avec bases de données et APIs externes, tests UI pour workflows critiques. Couverture cible : 80% code critique, 60% reste. CI/CD configuré pour exécuter tous les tests avant merge
- **User Experience Consistency**: ✅ Design system cohérent avec Avalonia UI (composants natifs), navigation logique, messages d'erreur clairs et actionnables, feedback utilisateur pendant opérations longues (progress bars, notifications)
- **Performance Requirements**: ✅ Métriques définies (scan <5min pour 1000 ROMs, export <2min pour 500 jeux), profiling prévu pour identifier goulots d'étranglement, monitoring via logs structurés, limites acceptables documentées

If any principle is violated, document justification in Complexity Tracking section below.

## Project Structure

### Documentation (this feature)

```text
specs/001-rom-manager-app/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/
├── RomPilot.Core/                    # Bibliothèque partagée (logique métier)
│   ├── Models/                       # Entités de domaine (ROM, Game, Console, etc.)
│   ├── Services/                     # Services métier (ScanService, ExportService, SyncService)
│   ├── Repositories/                 # Accès données (SQLite via Entity Framework Core)
│   ├── Database/                     # Configuration EF Core, migrations
│   ├── Checksums/                    # Calcul checksums (MD5, SHA1, SHA256, CRC32)
│   ├── Archives/                     # Gestion archives (ZIP, 7Z) avec SharpCompress
│   ├── DatabaseProviders/            # Parsers bases de données (NoIntro, Redump, GoodSet)
│   ├── PlatformAdapters/             # Adapters pour Recalbox (gamelist.xml) et Romm (API REST)
│   └── Preferences/                  # Gestion préférences utilisateur
│
├── RomPilot.UI/                       # Application Avalonia (interface graphique)
│   ├── Views/                        # Vues XAML (MainWindow, ScanView, ExportView, etc.)
│   ├── ViewModels/                    # ViewModels (MVVM pattern)
│   ├── Controls/                     # Contrôles personnalisés
│   ├── Services/                     # Services UI (dialogs, notifications)
│   ├── Converters/                   # Value converters XAML
│   └── App.axaml.cs                  # Point d'entrée application
│
└── RomPilot.Tests/                    # Tests
    ├── Unit/                         # Tests unitaires (Core)
    ├── Integration/                  # Tests d'intégration (DB, APIs)
    └── UI/                           # Tests UI (workflows critiques)

tests/
├── contract/                         # Tests de contrat (si nécessaire)
├── integration/                      # Tests d'intégration end-to-end
└── unit/                             # Tests unitaires (référence)
```

**Structure Decision**: Architecture modulaire en 3 projets : Core (logique métier sans dépendance UI), UI (application Avalonia), Tests (couverture complète). Cette séparation permet de tester la logique métier indépendamment de l'UI, facilite la maintenance, et permet potentiellement une future CLI ou API si nécessaire.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Architecture modulaire (3 projets) | Séparation claire UI/logique métier, testabilité, maintenabilité | Projet unique plus simple mais mélange UI et logique, tests plus difficiles, moins maintenable |
| Entity Framework Core pour SQLite | ORM facilite gestion schéma, migrations, requêtes complexes | Accès SQLite direct plus simple mais plus de code boilerplate, migrations manuelles, moins type-safe |
| SharpCompress pour archives | Support ZIP et 7Z unifié, scan récursif, gestion mémoire | Bibliothèques natives .NET (ZipFile) plus simples mais pas de support 7Z, moins de contrôle sur scan récursif |

