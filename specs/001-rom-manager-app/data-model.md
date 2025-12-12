# Data Model: ROM Manager Application

**Date**: 2025-01-27 (Mis à jour: 2025-12-12)  
**Feature**: [spec.md](./spec.md)

**Note** : Modèle mis à jour suite aux clarifications du 2025-12-12 (scan sans présupposition, filtres configurables, scans incrémentaux, préférences format vidéo, export SSH/SFTP).

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

#### Scanned File
Représente un fichier scanné (peut être identifié comme ROM ou non).

**CHANGEMENT MAJEUR** : Renommé de `RomFiles` à `ScannedFiles` pour refléter que tous les fichiers sont scannés sans présupposition.

```sql
CREATE TABLE ScannedFiles (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FilePath TEXT NOT NULL,                       -- Chemin complet fichier
    FileName TEXT NOT NULL,                       -- Nom fichier
    FileSize BIGINT NOT NULL,                     -- Taille en octets
    LastModifiedTimestamp INTEGER NOT NULL,       -- Timestamp modification (pour scans incrémentaux)
    ArchivePath TEXT,                             -- Chemin archive si dans ZIP/7Z/RAR
    ArchiveDepth INTEGER DEFAULT 0,               -- Profondeur dans archives (0 = direct)
    IdentificationStatus TEXT NOT NULL,           -- "Identified", "Unidentified", "Excluded", "Failed"
    ExclusionReason TEXT,                         -- Raison exclusion si Status="Excluded"
    FailureReason TEXT,                           -- Raison échec si Status="Failed"
    ConsoleId INTEGER,                            -- FK vers Consoles (NULL si non identifié)
    GameEntryId INTEGER,                          -- FK vers GameEntries (NULL si non identifié)
    DetectedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    LastScannedAt DATETIME,                        -- Dernier scan
    ScanType TEXT,                                -- "Quick" ou "Full"
    FOREIGN KEY (ConsoleId) REFERENCES Consoles(Id),
    FOREIGN KEY (GameEntryId) REFERENCES GameEntries(Id)
);

-- Index pour recherche rapide
CREATE INDEX IX_ScannedFiles_ConsoleId ON ScannedFiles(ConsoleId);
CREATE INDEX IX_ScannedFiles_IdentificationStatus ON ScannedFiles(IdentificationStatus);
CREATE INDEX IX_ScannedFiles_FilePath ON ScannedFiles(FilePath);
```

#### Checksums
Stocke les checksums calculés pour chaque fichier scanné.

```sql
CREATE TABLE Checksums (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ScannedFileId INTEGER NOT NULL,               -- FK vers ScannedFiles (changé)
    HashType TEXT NOT NULL,                       -- "MD5", "SHA1", "SHA256", "CRC32"
    HashValue TEXT NOT NULL,                      -- Valeur hash (hex)
    CalculatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ScannedFileId) REFERENCES ScannedFiles(Id),
    UNIQUE(ScannedFileId, HashType)
);

-- Index pour recherche par hash (identification jeux)
CREATE INDEX IX_Checksums_HashType_HashValue ON Checksums(HashType, HashValue);
CREATE INDEX IX_Checksums_ScannedFileId ON Checksums(ScannedFileId);
```

#### Database Source
Représente un type de source de base de données (NoIntro, Redump, GoodSet).

```sql
CREATE TABLE DatabaseSources (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL UNIQUE,                    -- "NoIntro", "Redump", "GoodSet"
    Description TEXT,
    IsActive BOOLEAN NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);
```

#### Reference Database
**NOUVELLE TABLE** : Représente une base de données de référence téléchargée (version spécifique pour une console).

```sql
CREATE TABLE ReferenceDatabases (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Provider TEXT NOT NULL,                       -- "NoIntro", "Redump", "GoodSet"
    Console TEXT NOT NULL,                        -- Nom console (ex: "nes", "snes")
    Version TEXT NOT NULL,                        -- Version datfile (ex: "2024-01-15")
    ReleaseDate TEXT,                             -- Date publication version
    FileSize BIGINT,                              -- Taille fichier
    DownloadStatus TEXT NOT NULL,                 -- "Available", "Downloading", "Downloaded", "Error"
    FilePath TEXT,                                -- Chemin fichier local (si téléchargé)
    IsDefault BOOLEAN NOT NULL DEFAULT 0,         -- Version par défaut pour cette console
    DownloadedAt DATETIME,                        -- Date téléchargement
    LastCheckedAt DATETIME,                       -- Dernière vérification mise à jour
    ErrorMessage TEXT,                            -- Message erreur si DownloadStatus="Error"
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(Provider, Console, Version)
);

-- Index pour recherche par provider et console
CREATE INDEX IX_ReferenceDatabases_Provider_Console ON ReferenceDatabases(Provider, Console);
CREATE INDEX IX_ReferenceDatabases_IsDefault ON ReferenceDatabases(IsDefault);
```

#### Exclusion Filters
**NOUVELLE TABLE** : Gère les filtres d'exclusion pour le scan (par défaut et personnalisés).

```sql
CREATE TABLE ExclusionFilters (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FilterType TEXT NOT NULL,                     -- "Extension", "SizeRange", "Pattern"
    FilterValue TEXT NOT NULL,                    -- Ex: ".jpg", "0-1KB", "*.tmp"
    IsDefault BOOLEAN NOT NULL DEFAULT 0,         -- Filtre par défaut (non modifiable facilement)
    IsActive BOOLEAN NOT NULL DEFAULT 1,          -- Filtre actif
    Description TEXT,                             -- Description filtre
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Index pour recherche filtres actifs
CREATE INDEX IX_ExclusionFilters_IsActive ON ExclusionFilters(IsActive);
CREATE INDEX IX_ExclusionFilters_FilterType ON ExclusionFilters(FilterType);

-- Données initiales (extensions par défaut)
INSERT INTO ExclusionFilters (FilterType, FilterValue, IsDefault, IsActive, Description)
VALUES 
    ('Extension', '.jpg', 1, 1, 'Images JPEG'),
    ('Extension', '.jpeg', 1, 1, 'Images JPEG'),
    ('Extension', '.png', 1, 1, 'Images PNG'),
    ('Extension', '.gif', 1, 1, 'Images GIF'),
    ('Extension', '.bmp', 1, 1, 'Images BMP'),
    ('Extension', '.txt', 1, 1, 'Fichiers texte'),
    ('Extension', '.nfo', 1, 1, 'Fichiers NFO'),
    ('Extension', '.diz', 1, 1, 'Fichiers DIZ'),
    ('Extension', '.exe', 1, 1, 'Exécutables Windows'),
    ('Extension', '.dll', 1, 1, 'Bibliothèques Windows'),
    ('Extension', '.so', 1, 1, 'Bibliothèques Linux'),
    ('Extension', '.doc', 1, 1, 'Documents Word'),
    ('Extension', '.pdf', 1, 1, 'Documents PDF'),
    ('Extension', '.html', 1, 1, 'Pages HTML'),
    ('Extension', '.xml', 1, 1, 'Fichiers XML');
```

#### Game Entry (from Database)
Entrée de jeu dans une base de données de référence.

```sql
CREATE TABLE GameEntries (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ReferenceDatabaseId INTEGER NOT NULL,         -- FK vers ReferenceDatabases (changé)
    GameName TEXT NOT NULL,                       -- Nom du jeu
    ConsoleId INTEGER NOT NULL,                   -- FK vers Consoles
    HashType TEXT NOT NULL,                       -- Type hash utilisé pour identification
    HashValue TEXT NOT NULL,                      -- Hash du ROM
    Region TEXT,                                  -- Région (EUR, USA, JAP, etc.)
    VideoFormat TEXT,                             -- NOUVEAU: Format vidéo (PAL, NTSC, NTSC-J)
    Language TEXT,                                -- Langue
    QualityFlags TEXT,                            -- Flags qualité (JSON: ["bad dump", ...])
    Version TEXT,                                 -- Version du ROM
    SerialNumber TEXT,                            -- Numéro série (si applicable)
    Metadata TEXT,                                -- Métadonnées additionnelles (JSON)
    FOREIGN KEY (ReferenceDatabaseId) REFERENCES ReferenceDatabases(Id),
    FOREIGN KEY (ConsoleId) REFERENCES Consoles(Id)
);

-- Index pour recherche par hash (identification)
CREATE INDEX IX_GameEntries_HashType_HashValue ON GameEntries(HashType, HashValue);
CREATE INDEX IX_GameEntries_ReferenceDatabaseId ON GameEntries(ReferenceDatabaseId);
CREATE INDEX IX_GameEntries_ConsoleId ON GameEntries(ConsoleId);
```

#### Game
Représente un jeu logique (peut avoir plusieurs versions ROM).

```sql
CREATE TABLE Games (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,                           -- Nom du jeu
    ConsoleId INTEGER NOT NULL,                   -- FK vers Consoles
    SelectedReferenceDatabaseId INTEGER,          -- Source BD sélectionnée (si multiple)
    SelectedScannedFileId INTEGER,                -- Fichier version sélectionnée (changé)
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ConsoleId) REFERENCES Consoles(Id),
    FOREIGN KEY (SelectedReferenceDatabaseId) REFERENCES ReferenceDatabases(Id),
    FOREIGN KEY (SelectedScannedFileId) REFERENCES ScannedFiles(Id)
);

-- Index pour recherche par console
CREATE INDEX IX_Games_ConsoleId ON Games(ConsoleId);
```

#### Game ROM Versions
Association entre un jeu et ses versions (fichiers identifiés comme ROM).

```sql
CREATE TABLE GameRomVersions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    GameId INTEGER NOT NULL,                      -- FK vers Games
    ScannedFileId INTEGER NOT NULL,               -- FK vers ScannedFiles (changé)
    GameEntryId INTEGER,                          -- FK vers GameEntries (si identifié)
    IsSelected BOOLEAN NOT NULL DEFAULT 0,        -- Version sélectionnée
    SelectionScore INTEGER,                        -- Score de sélection (pour tri)
    FOREIGN KEY (GameId) REFERENCES Games(Id),
    FOREIGN KEY (ScannedFileId) REFERENCES ScannedFiles(Id),
    FOREIGN KEY (GameEntryId) REFERENCES GameEntries(Id),
    UNIQUE(GameId, ScannedFileId)
);

-- Index pour recherche versions d'un jeu
CREATE INDEX IX_GameRomVersions_GameId ON GameRomVersions(GameId);
CREATE INDEX IX_GameRomVersions_ScannedFileId ON GameRomVersions(ScannedFileId);
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
-- "video_format_priority" : JSON array ["PAL", "NTSC", "NTSC-J"]  -- NOUVEAU
-- "language_priority" : JSON array ["FR", "EN", ...]
-- "exclude_bad_dumps" : "true"/"false"
-- "scan_mode_default" : "Quick" ou "Full"                         -- NOUVEAU
-- "recalbox_export_path" : "/path/to/recalbox/roms"
-- "romm_api_url" : "http://..."
-- "romm_api_key" : "..." (chiffré)
-- "sftp_enabled" : "true"/"false"                                 -- NOUVEAU
```

#### Export Configuration
**NOUVELLE TABLE** : Configuration des exports vers plateformes (local ou distant).

```sql
CREATE TABLE ExportConfigurations (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,                           -- Nom configuration (ex: "Recalbox Production")
    Platform TEXT NOT NULL,                       -- "Recalbox" ou "Romm"
    ExportType TEXT NOT NULL,                     -- "Local" ou "Remote"
    
    -- Configuration export local
    LocalPath TEXT,                               -- Chemin local (si ExportType="Local")
    
    -- Configuration export distant SSH/SFTP
    SftpHost TEXT,                                -- Hôte SSH/SFTP
    SftpPort INTEGER DEFAULT 22,                  -- Port SSH/SFTP
    SftpUsername TEXT,                            -- Nom utilisateur
    SftpAuthMethod TEXT,                          -- "Password" ou "Key"
    SftpPasswordEncrypted TEXT,                   -- Mot de passe chiffré
    SftpKeyPath TEXT,                             -- Chemin clé privée
    RemoteDirectory TEXT,                         -- Répertoire distant
    
    -- Options export
    FormatRequirement TEXT,                       -- "ZIP" ou "Uncompressed"
    ConflictResolution TEXT,                      -- "Overwrite", "Skip", "Ask"
    
    -- Status
    IsActive BOOLEAN NOT NULL DEFAULT 1,
    LastConnectionTest DATETIME,
    ConnectionTestStatus TEXT,                    -- "Success", "Failed", NULL
    ConnectionTestError TEXT,
    
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IX_ExportConfigurations_Platform ON ExportConfigurations(Platform);
CREATE INDEX IX_ExportConfigurations_IsActive ON ExportConfigurations(IsActive);
```

#### Export History
Historique des exports.

```sql
CREATE TABLE ExportHistory (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ExportConfigurationId INTEGER NOT NULL,       -- FK vers ExportConfigurations
    GamesExported INTEGER NOT NULL,
    FilesExported INTEGER NOT NULL,
    BytesTransferred BIGINT NOT NULL DEFAULT 0,   -- Octets transférés
    StartedAt DATETIME NOT NULL,
    CompletedAt DATETIME,
    Status TEXT NOT NULL,                         -- "Success", "Failed", "Partial"
    ErrorMessage TEXT,
    FOREIGN KEY (ExportConfigurationId) REFERENCES ExportConfigurations(Id)
);

CREATE INDEX IX_ExportHistory_ExportConfigurationId ON ExportHistory(ExportConfigurationId);
CREATE INDEX IX_ExportHistory_StartedAt ON ExportHistory(StartedAt);
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
Consoles (1) ──< (N) ScannedFiles          -- Changé: RomFiles → ScannedFiles
Consoles (1) ──< (N) Games
Consoles (1) ──< (N) GameEntries

ScannedFiles (1) ──< (N) Checksums         -- Changé: RomFiles → ScannedFiles
ScannedFiles (1) ──< (N) GameRomVersions   -- Changé: RomFiles → ScannedFiles

ReferenceDatabases (1) ──< (N) GameEntries -- Changé: DatabaseSources → ReferenceDatabases

Games (1) ──< (N) GameRomVersions
Games (1) ──< (N) Metadata
Games (1) ──< (1) UserData

GameEntries (1) ──< (N) GameRomVersions

ExportConfigurations (1) ──< (N) ExportHistory  -- Nouveau

-- Nouvelles tables indépendantes:
-- ExclusionFilters (table de configuration)
-- UserPreferences (clé-valeur)
-- SyncHistory (historique)
```

## Data Access Patterns

### Repository Interfaces

```csharp
// Exemples d'interfaces Repository (mis à jour)

public interface IScannedFileRepository  // Changé: IRomFileRepository → IScannedFileRepository
{
    Task<ScannedFile> GetByIdAsync(int id);
    Task<IEnumerable<ScannedFile>> GetByConsoleIdAsync(int consoleId);
    Task<IEnumerable<ScannedFile>> GetByIdentificationStatusAsync(string status);
    Task<ScannedFile> GetByChecksumAsync(string hashType, string hashValue);
    Task<ScannedFile> GetByFilePathAsync(string filePath);
    Task<ScannedFile> AddAsync(ScannedFile scannedFile);
    Task UpdateAsync(ScannedFile scannedFile);
    Task DeleteAsync(int id);
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
    Task<IEnumerable<Checksum>> GetByScannedFileIdAsync(int scannedFileId);  // Changé
    Task<Checksum> GetByHashAsync(string hashType, string hashValue);
    Task<Checksum> AddAsync(Checksum checksum);
}

public interface IReferenceDatabaseRepository  // Nouveau
{
    Task<ReferenceDatabase> GetByIdAsync(int id);
    Task<IEnumerable<ReferenceDatabase>> GetByProviderAsync(string provider);
    Task<IEnumerable<ReferenceDatabase>> GetByProviderAndConsoleAsync(string provider, string console);
    Task<ReferenceDatabase> GetDefaultByConsoleAsync(string console);
    Task<ReferenceDatabase> AddAsync(ReferenceDatabase database);
    Task UpdateAsync(ReferenceDatabase database);
}

public interface IExclusionFilterRepository  // Nouveau
{
    Task<IEnumerable<ExclusionFilter>> GetActiveFiltersAsync();
    Task<IEnumerable<ExclusionFilter>> GetByFilterTypeAsync(string filterType);
    Task<ExclusionFilter> AddAsync(ExclusionFilter filter);
    Task UpdateAsync(ExclusionFilter filter);
    Task DeleteAsync(int id);
}

public interface IExportConfigurationRepository  // Nouveau
{
    Task<ExportConfiguration> GetByIdAsync(int id);
    Task<IEnumerable<ExportConfiguration>> GetByPlatformAsync(string platform);
    Task<IEnumerable<ExportConfiguration>> GetActiveConfigurationsAsync();
    Task<ExportConfiguration> AddAsync(ExportConfiguration config);
    Task UpdateAsync(ExportConfiguration config);
    Task DeleteAsync(int id);
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

