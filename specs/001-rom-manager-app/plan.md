# Implementation Plan: ROM Manager Application

**Branch**: `001-rom-manager-app` | **Date**: 2025-01-27 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-rom-manager-app/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

RomPilot is a desktop application for managing ROM collections. The application allows users to scan directories recursively (including all subdirectories) to find ROM files, including those contained within ZIP, 7Z, and RAR archives, with recursive processing of nested archives (archives within archives at any depth). For each ROM file found, the application automatically identifies the console/platform and the game using reference databases (NoIntro, Redump, GoodSet) via checksum matching. The application provides detailed progress feedback during scanning, showing all processed files (with status) and all failed files with explicit failure reasons.

**Technical Approach**: 
- **GUI Framework**: Avalonia UI (C#) for cross-platform desktop application
- **Database**: SQLite with Entity Framework Core for local data storage
- **Archive Handling**: SharpCompress library supporting ZIP, 7Z, and RAR formats
- **Architecture**: MVVM pattern with service layer and repository pattern
- **Platforms**: Linux, Windows, macOS (.NET 7.0)

## Technical Context

**Language/Version**: C# / .NET 7.0  
**Primary Dependencies**: 
- Avalonia UI (cross-platform GUI framework)
- Entity Framework Core 7.0.20 (database ORM)
- SharpCompress 0.41.0 (archive handling - ZIP, 7Z, RAR)
- System.Security.Cryptography (checksum calculation - MD5, SHA1, SHA256, CRC32)

**Storage**: SQLite (embedded database file)  
**Testing**: xUnit + Moq (unit tests), integration tests with SQLite in-memory  
**Target Platform**: Desktop (Linux, Windows, macOS)  
**Project Type**: Single desktop application with MVVM architecture  
**Performance Goals**: 
- Scan 1000+ ROM files (including archives) within 5 minutes on standard desktop
- Identify console/platform for 95%+ of ROM files automatically
- Export 500 games to Recalbox/Romm in under 2 minutes

**Constraints**: 
- Handle archives nested up to 5 levels deep without performance degradation
- Memory-efficient processing (streaming for large archives)
- Cross-platform compatibility (Linux priority)

**Scale/Scope**: 
- Support 35+ gaming consoles/platforms
- Handle collections with thousands of ROM files
- Support multiple database sources (NoIntro, Redump, GoodSet)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Verify compliance with RomPilot Constitution principles:

- **Code Quality**: ✅ Code follows C# conventions, XML documentation for public APIs, code reviews planned via PRs
- **Testing Standards**: ✅ TDD approach for new features, xUnit + Moq framework, target 80% coverage for critical code, 60% for rest, CI/CD tests configured
- **User Experience Consistency**: ✅ Avalonia UI design system, consistent navigation patterns, clear error messages with actionable feedback (detailed failure reasons per file)
- **Performance Requirements**: ✅ Performance goals defined (scan time, identification accuracy), benchmarks planned, progress reporting for long operations

**Compliance Status**: ✅ All principles satisfied. No violations requiring justification.

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
├── RomPilot.Core/              # Core business logic
│   ├── Archives/
│   │   ├── ArchiveScanner.cs   # Recursive directory + archive scanning (ZIP, 7Z, RAR)
│   │   └── IArchiveScanner.cs
│   ├── Checksums/
│   │   ├── ChecksumCalculator.cs
│   │   └── IChecksumCalculator.cs
│   ├── Database/
│   │   ├── RomPilotDbContext.cs
│   │   └── Migrations/
│   ├── DatabaseProviders/
│   │   ├── NoIntroProvider.cs
│   │   ├── RedumpProvider.cs
│   │   └── GoodSetProvider.cs
│   ├── Models/                 # Entity models
│   ├── Repositories/           # Data access layer
│   ├── Services/
│   │   ├── ScanService.cs      # Orchestrates scan workflow with detailed progress reporting
│   │   ├── ScanProgressReporter.cs  # Progress feedback with file-level status
│   │   ├── ConsoleDetectionService.cs
│   │   └── GameIdentificationService.cs
│   └── RomPilot.Core.csproj
├── RomPilot.UI/                # Avalonia UI application
│   ├── ViewModels/            # MVVM ViewModels
│   │   ├── MainWindowViewModel.cs
│   │   ├── ScanViewModel.cs    # Scan UI with detailed progress display
│   │   └── ScanResultsViewModel.cs
│   ├── Views/                  # Avalonia XAML views
│   │   ├── MainWindow.axaml
│   │   ├── ScanView.axaml      # Shows processed/failed files with reasons
│   │   └── ScanResultsView.axaml
│   └── RomPilot.UI.csproj
└── RomPilot.Tests/            # Test projects
    ├── Unit/                   # Unit tests
    ├── Integration/            # Integration tests
    └── RomPilot.Tests.csproj
```

**Structure Decision**: Single solution with three projects: Core (business logic), UI (Avalonia desktop app), and Tests. This structure separates concerns while keeping the codebase manageable for a desktop application.

## Key Implementation Updates from Spec Clarifications

### 1. Archive Format Support: RAR Addition
**Requirement**: Support ZIP, 7Z, and RAR formats (FR-001 updated)

**Current State**: ArchiveScanner currently supports only ZIP and 7Z via SharpCompress.

**Action Required**: 
- Update `ArchiveScanner.cs` to detect and process `.rar` files
- Verify SharpCompress RAR support (may require additional configuration or library)
- Update `IsArchiveFile()` method to include `.rar` extension
- Add RAR archive opening logic in `ScanArchiveAsync()` and `ExtractFileAsync()`
- Update interface documentation in `IArchiveScanner.cs`

**Impact**: Medium - requires code changes but SharpCompress should support RAR natively.

### 2. Detailed Progress Feedback
**Requirement**: Show detailed list of processed files (with status) and failed files with explicit reasons (FR-022, FR-023)

**Current State**: `IScanProgressReporter` exists but may not capture file-level details with failure reasons.

**Action Required**:
- Enhance `IScanProgressReporter` to report per-file status:
  - Success with console identified, game identified
  - Failure with explicit reason ("Console not detected", "Corrupted archive", "Unreadable file", "Checksum error", "Archive extraction failed")
- Update `ScanService` to capture and report failure reasons for each file
- Update `ScanViewModel` to display detailed file list in UI
- Update `ScanView.axaml` to show processed/failed files with reasons

**Impact**: Medium - requires UI updates and enhanced progress reporting.

### 3. Recursive Scanning Clarification
**Requirement**: Two-level recursion - (1) recursive directory traversal (all subdirectories), (2) recursive archive processing (nested archives at any depth) (FR-001 updated)

**Current State**: `ArchiveScanner` already implements recursive directory scanning and nested archive processing, but needs verification for RAR support.

**Action Required**:
- Verify recursive directory traversal works correctly (already implemented via `ScanDirectoryRecursiveAsync`)
- Verify recursive archive processing works for all formats (ZIP, 7Z, RAR)
- Update documentation to explicitly state both recursion levels
- Test with deeply nested structures (5+ levels)

**Impact**: Low - mostly verification and documentation, implementation already exists.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| N/A | No violations | All constitution principles satisfied |

## Phase 0: Research Status

**Status**: ✅ Complete (see [research.md](./research.md))

**Key Decisions**:
- Avalonia UI selected for cross-platform GUI
- SQLite for local database
- SharpCompress for archive handling (ZIP, 7Z, RAR)
- Entity Framework Core for ORM
- MVVM pattern for UI architecture

**Open Questions Resolved**:
- Archive library supports RAR (SharpCompress)
- Performance targets defined (5 min for 1000+ ROMs)
- Database schema designed for multi-source game identification

## Phase 1: Design Status

**Status**: ✅ Complete (see [data-model.md](./data-model.md))

**Key Artifacts**:
- Database schema with all entities (Consoles, RomFiles, Checksums, Games, etc.)
- Repository interfaces defined
- Service layer architecture (ScanService, GameIdentificationService, etc.)
- Entity relationships documented

**Updates Needed**:
- Add `ProcessingStatus` and `FailureReason` fields to RomFile model (for detailed feedback)
- Update ArchiveScanner interface documentation for RAR support
- Enhance ScanProgressReporter interface for file-level status reporting

## Next Steps

1. **Update ArchiveScanner for RAR Support**:
   - Add RAR detection in `IsArchiveFile()`
   - Add RAR archive opening in `ScanArchiveAsync()` using SharpCompress RarArchive
   - Update `ExtractFileAsync()` for RAR support
   - Add unit tests for RAR scanning

2. **Enhance Progress Reporting**:
   - Update `IScanProgressReporter` interface to include file-level status reporting
   - Modify `ScanService` to capture failure reasons for each file
   - Update `RomFile` model to include `ProcessingStatus` and `FailureReason` properties
   - Enhance UI to display detailed file list with status and reasons

3. **Verify Recursive Scanning**:
   - Test recursive directory traversal with nested subdirectories
   - Test recursive archive processing with deeply nested archives (ZIP, 7Z, RAR)
   - Verify performance with 5+ levels of nesting

4. **Update Tests**:
   - Add tests for RAR archive scanning
   - Add tests for detailed progress reporting
   - Add tests for failure reason capture

## Dependencies

- **SharpCompress 0.41.0**: Verify RAR support (may need to check documentation or test)
- **Avalonia UI**: For detailed progress UI components
- **Entity Framework Core**: For RomFile model updates

## Risk Assessment

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| SharpCompress RAR support incomplete | Low | Medium | Verify RAR support, consider alternative library if needed |
| Performance degradation with detailed progress reporting | Medium | Low | Use efficient data structures, batch UI updates |
| Deeply nested archives causing memory issues | Low | High | Already mitigated with depth limit (5 levels) and streaming |
