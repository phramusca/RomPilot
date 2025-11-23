# Data Model: ROM Manager Application

**Date**: 2025-01-27  
**Feature**: [spec.md](./spec.md)

## Database Schema (SQLite)

### Core Entities

#### Console/Platform
Représente une console de jeu rétro.

```sql
CREATE TABLE Consoles (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL UNIQUE,                    -- Ex: "Nintendo Entertainment System"
    ShortName TEXT NOT NULL UNIQUE,              -- Ex: "nes"
    RecalboxFolderName TEXT NOT NULL,            -- Nom dossier Recalbox
    RommPlatformId TEXT,                          -- ID plateforme Romm (si applicable)
    SupportedFormats TEXT,                        -- Formats supportés (JSON array)
    ExportFormat TEXT,                            -- Format export (ZIP/uncompressed)
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);
```

#### ROM File
Représente un fichier ROM détecté lors du scan.

```sql
CREATE TABLE RomFiles (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FilePath TEXT NOT NULL,                       -- Chemin complet fichier
    FileName TEXT NOT NULL,                       -- Nom fichier
    FileSize BIGINT NOT NULL,                     -- Taille en octets
    ArchivePath TEXT,                             -- Chemin archive si dans ZIP/7Z
    ArchiveDepth INTEGER DEFAULT 0,               -- Profondeur dans archives (0 = direct)
    ConsoleId INTEGER NOT NULL,                   -- FK vers Consoles
    DetectedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    LastScannedAt DATETIME,                        -- Dernier scan
    FOREIGN KEY (ConsoleId) REFERENCES Consoles(Id)
);

-- Index pour recherche rapide par console
CREATE INDEX IX_RomFiles_ConsoleId ON RomFiles(ConsoleId);
```

#### Checksums
Stocke les checksums calculés pour chaque ROM.

```sql
CREATE TABLE Checksums (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    RomFileId INTEGER NOT NULL,                   -- FK vers RomFiles
    HashType TEXT NOT NULL,                       -- "MD5", "SHA1", "SHA256", "CRC32"
    HashValue TEXT NOT NULL,                      -- Valeur hash (hex)
    CalculatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (RomFileId) REFERENCES RomFiles(Id),
    UNIQUE(RomFileId, HashType)
);

-- Index pour recherche par hash (identification jeux)
CREATE INDEX IX_Checksums_HashType_HashValue ON Checksums(HashType, HashValue);
CREATE INDEX IX_Checksums_RomFileId ON Checksums(RomFileId);
```

#### Database Source
Représente une source de base de données (NoIntro, Redump, GoodSet).

```sql
CREATE TABLE DatabaseSources (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL UNIQUE,                    -- "NoIntro", "Redump", "GoodSet"
    Description TEXT,
    DatfilePath TEXT,                             -- Chemin fichier datfile local
    LastUpdatedAt DATETIME,                       -- Dernière mise à jour datfile
    IsActive BOOLEAN NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);
```

#### Game Entry (from Database)
Entrée de jeu dans une base de données de référence.

```sql
CREATE TABLE GameEntries (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    DatabaseSourceId INTEGER NOT NULL,            -- FK vers DatabaseSources
    GameName TEXT NOT NULL,                       -- Nom du jeu
    ConsoleId INTEGER NOT NULL,                   -- FK vers Consoles
    HashType TEXT NOT NULL,                       -- Type hash utilisé pour identification
    HashValue TEXT NOT NULL,                      -- Hash du ROM
    Region TEXT,                                  -- Région (EUR, USA, JAP, etc.)
    Language TEXT,                                -- Langue
    QualityFlags TEXT,                            -- Flags qualité (JSON: ["bad dump", ...])
    Version TEXT,                                 -- Version du ROM
    SerialNumber TEXT,                            -- Numéro série (si applicable)
    Metadata TEXT,                                -- Métadonnées additionnelles (JSON)
    FOREIGN KEY (DatabaseSourceId) REFERENCES DatabaseSources(Id),
    FOREIGN KEY (ConsoleId) REFERENCES Consoles(Id)
);

-- Index pour recherche par hash (identification)
CREATE INDEX IX_GameEntries_HashType_HashValue ON GameEntries(HashType, HashValue);
CREATE INDEX IX_GameEntries_DatabaseSourceId ON GameEntries(DatabaseSourceId);
CREATE INDEX IX_GameEntries_ConsoleId ON GameEntries(ConsoleId);
```

#### Game
Représente un jeu logique (peut avoir plusieurs versions ROM).

```sql
CREATE TABLE Games (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,                           -- Nom du jeu
    ConsoleId INTEGER NOT NULL,                   -- FK vers Consoles
    SelectedDatabaseSourceId INTEGER,             -- Source BD sélectionnée (si multiple)
    SelectedRomFileId INTEGER,                    -- ROM version sélectionnée
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ConsoleId) REFERENCES Consoles(Id),
    FOREIGN KEY (SelectedDatabaseSourceId) REFERENCES DatabaseSources(Id),
    FOREIGN KEY (SelectedRomFileId) REFERENCES RomFiles(Id)
);

-- Index pour recherche par console
CREATE INDEX IX_Games_ConsoleId ON Games(ConsoleId);
```

#### Game ROM Versions
Association entre un jeu et ses versions ROM.

```sql
CREATE TABLE GameRomVersions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    GameId INTEGER NOT NULL,                      -- FK vers Games
    RomFileId INTEGER NOT NULL,                   -- FK vers RomFiles
    GameEntryId INTEGER,                          -- FK vers GameEntries (si identifié)
    IsSelected BOOLEAN NOT NULL DEFAULT 0,        -- Version sélectionnée
    SelectionScore INTEGER,                        -- Score de sélection (pour tri)
    FOREIGN KEY (GameId) REFERENCES Games(Id),
    FOREIGN KEY (RomFileId) REFERENCES RomFiles(Id),
    FOREIGN KEY (GameEntryId) REFERENCES GameEntries(Id),
    UNIQUE(GameId, RomFileId)
);

-- Index pour recherche versions d'un jeu
CREATE INDEX IX_GameRomVersions_GameId ON GameRomVersions(GameId);
CREATE INDEX IX_GameRomVersions_RomFileId ON GameRomVersions(RomFileId);
```

#### Metadata
Métadonnées de jeu (scraped ou utilisateur).

```sql
CREATE TABLE Metadata (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    GameId INTEGER NOT NULL,                      -- FK vers Games
    Source TEXT NOT NULL,                         -- "Recalbox", "Romm", "User", "Scraped"
    Description TEXT,
    Rating REAL,                                  -- Rating (0.0 - 10.0)
    ReleaseDate TEXT,                             -- Date sortie
    Developer TEXT,
    Publisher TEXT,
    Genre TEXT,
    CoverImagePath TEXT,
    ThumbnailPath TEXT,
    ScreenshotPaths TEXT,                         -- JSON array
    VideoPath TEXT,
    LastModifiedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (GameId) REFERENCES Games(Id)
);

-- Index pour recherche par jeu
CREATE INDEX IX_Metadata_GameId ON Metadata(GameId);
```

#### User Data
Données utilisateur (favoris, statistiques, etc.).

```sql
CREATE TABLE UserData (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    GameId INTEGER NOT NULL,                      -- FK vers Games
    IsFavorite BOOLEAN NOT NULL DEFAULT 0,
    IsHidden BOOLEAN NOT NULL DEFAULT 0,
    UserRating REAL,                              -- Rating utilisateur
    PlayCount INTEGER DEFAULT 0,
    LastPlayedAt DATETIME,
    TimePlayed INTEGER DEFAULT 0,                 -- Temps jeu en secondes
    Notes TEXT,                                   -- Notes utilisateur
    LastModifiedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (GameId) REFERENCES Games(Id),
    UNIQUE(GameId)
);

-- Index pour recherche favoris, etc.
CREATE INDEX IX_UserData_GameId ON UserData(GameId);
CREATE INDEX IX_UserData_IsFavorite ON UserData(IsFavorite);
```

#### User Preferences
Préférences utilisateur pour filtrage et sélection.

```sql
CREATE TABLE UserPreferences (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Key TEXT NOT NULL UNIQUE,                     -- Clé préférence
    Value TEXT NOT NULL,                          -- Valeur (JSON si complexe)
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Préférences stockées :
-- "region_priority" : JSON array ["EUR", "USA", "JAP"]
-- "language_priority" : JSON array ["FR", "EN", ...]
-- "exclude_bad_dumps" : "true"/"false"
-- "recalbox_export_path" : "/path/to/recalbox/roms"
-- "romm_api_url" : "http://..."
-- "romm_api_key" : "..." (chiffré)
```

#### Export History
Historique des exports.

```sql
CREATE TABLE ExportHistory (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Platform TEXT NOT NULL,                       -- "Recalbox" ou "Romm"
    DestinationPath TEXT NOT NULL,
    GamesExported INTEGER NOT NULL,
    FilesExported INTEGER NOT NULL,
    StartedAt DATETIME NOT NULL,
    CompletedAt DATETIME,
    Status TEXT NOT NULL,                         -- "Success", "Failed", "Partial"
    ErrorMessage TEXT,
    FOREIGN KEY (GameId) REFERENCES Games(Id)
);
```

#### Sync History
Historique des synchronisations.

```sql
CREATE TABLE SyncHistory (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Platform TEXT NOT NULL,                       -- "Recalbox" ou "Romm"
    Direction TEXT NOT NULL,                      -- "Import", "Export", "Bidirectional"
    GamesSynced INTEGER NOT NULL,
    StartedAt DATETIME NOT NULL,
    CompletedAt DATETIME,
    Status TEXT NOT NULL,                         -- "Success", "Failed", "Partial"
    ErrorMessage TEXT
);
```

## Entity Relationships

```
Consoles (1) ──< (N) RomFiles
Consoles (1) ──< (N) Games
Consoles (1) ──< (N) GameEntries

RomFiles (1) ──< (N) Checksums
RomFiles (1) ──< (N) GameRomVersions

DatabaseSources (1) ──< (N) GameEntries

Games (1) ──< (N) GameRomVersions
Games (1) ──< (N) Metadata
Games (1) ──< (1) UserData

GameEntries (1) ──< (N) GameRomVersions
```

## Data Access Patterns

### Repository Interfaces

```csharp
// Exemples d'interfaces Repository
public interface IRomFileRepository
{
    Task<RomFile> GetByIdAsync(int id);
    Task<IEnumerable<RomFile>> GetByConsoleIdAsync(int consoleId);
    Task<RomFile> GetByChecksumAsync(string hashType, string hashValue);
    Task<RomFile> AddAsync(RomFile romFile);
    Task UpdateAsync(RomFile romFile);
}

public interface IGameRepository
{
    Task<Game> GetByIdAsync(int id);
    Task<IEnumerable<Game>> GetByConsoleIdAsync(int consoleId);
    Task<Game> GetByNameAndConsoleAsync(string name, int consoleId);
    Task<Game> AddAsync(Game game);
    Task UpdateAsync(Game game);
}

public interface IChecksumRepository
{
    Task<IEnumerable<Checksum>> GetByRomFileIdAsync(int romFileId);
    Task<Checksum> GetByHashAsync(string hashType, string hashValue);
    Task<Checksum> AddAsync(Checksum checksum);
}
```

## Migration Strategy

### Entity Framework Core Migrations

- Utiliser EF Core Migrations pour gestion schéma
- Migrations incrémentielles lors ajout fonctionnalités
- Scripts de migration pour mises à jour utilisateurs

### Initial Data

- Seed data pour consoles supportées (35+)
- Seed data pour sources bases de données (NoIntro, Redump, GoodSet)
- Configuration par défaut préférences utilisateur

## Performance Optimizations

### Indexes
- Index sur checksums pour recherche rapide identification
- Index sur consoleId pour filtrage
- Index composite sur (HashType, HashValue) pour GameEntries

### Query Optimization
- Utilisation projections (Select) pour éviter chargement entités complètes
- Pagination pour listes importantes
- Lazy loading désactivé, chargement explicite (Include)

### Caching Strategy
- Cache checksums calculés (éviter recalcul si fichier inchangé)
- Cache métadonnées scraped (TTL configurable)
- Cache résultats identification (mémoire, durée limitée)

