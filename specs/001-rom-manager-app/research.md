# Research: ROM Manager Application

**Date**: 2025-01-27  
**Feature**: [spec.md](./spec.md)

## Technology Stack Research

### GUI Framework Selection

**Options évaluées** :
1. **Avalonia UI (C#)** - ✅ **Sélectionné**
   - Framework moderne et mature pour applications desktop multi-plateformes
   - Syntaxe XAML similaire à WPF (familiarité si expérience WPF)
   - Excellent support Linux (priorité utilisateur)
   - Performance native, pas de runtime lourd
   - Communauté active, documentation complète
   - Référence : https://avaloniaui.net/

2. **JavaFX (Java)**
   - Connaissance du langage Java
   - Support multi-plateforme mais moins optimal sur Linux
   - JVM plus lourde, démarrage plus lent
   - Moins moderne que Avalonia

3. **PyQt/PySide (Python)**
   - Connaissance limitée du langage
   - Performance moindre pour traitement fichiers volumineux
   - Packaging/distribution plus complexe (PyInstaller, cx_Freeze)
   - Moins adapté pour application desktop native

**Décision** : Avalonia UI avec C# offre le meilleur compromis entre familiarité du langage, performance, support Linux, et modernité du framework.

### Database Selection

**SQLite** - ✅ **Sélectionné**
- Base de données embarquée, pas de serveur requis
- Parfait pour application desktop locale
- Support complet via Entity Framework Core ou Microsoft.Data.Sqlite
- Performances excellentes pour collections de ROMs (milliers d'entrées)
- Pas de configuration nécessaire, fichier unique

### Archive Handling

**SharpCompress** - ✅ **Sélectionné**
- Bibliothèque .NET native pour gestion archives
- Support ZIP et 7Z unifié
- Permet scan récursif d'archives imbriquées
- Gestion mémoire efficace (streaming)
- Référence : https://github.com/adamhathcock/sharpcompress

**Alternatives rejetées** :
- `System.IO.Compression.ZipFile` : Support ZIP uniquement, pas de 7Z
- `SevenZipSharp` : Bibliothèque séparée, nécessite 7z.dll native

### Checksum Calculation

**System.Security.Cryptography** - ✅ **Sélectionné**
- Bibliothèque native .NET
- Support MD5, SHA1, SHA256, CRC32
- Performance optimisée
- Pas de dépendance externe

### API Integration Research

#### Romm API
- **Documentation** : https://docs.romm.app/latest/API-and-Development/API-Reference/
- **OpenAPI Spec** : http://rpi5.local/openapi.json (version live)
- **Type** : REST API avec authentification
- **Format** : JSON
- **Bibliothèque** : `HttpClient` (natif .NET) + `System.Text.Json` pour parsing

#### Recalbox Integration
- **Format** : Fichiers `gamelist.xml` (format EmulationStation)
- **Structure** : XML avec métadonnées par console
- **Bibliothèque** : `System.Xml.Linq` (LINQ to XML)
- **Référence** : Analyser implémentation RomManager existante pour format exact
- **GitLab** : https://gitlab.com/recalbox/recalbox

### ROM Database Sources

**Sources identifiées** :
1. **NoIntro** - Datfiles pour ROMs vérifiés
2. **Redump** - Datfiles pour disques optiques (CD/DVD)
3. **GoodSet** - Datfiles pour collections complètes

**Format** : Datfiles (format XML ou texte structuré)
- Nécessite parser pour chaque format
- Téléchargement périodique des mises à jour
- Référence : https://wiki.recalbox.com/fr/tutorials/games/generalities/isos-and-roms/differents-groups

### Platform Folder Conventions

**Recalbox** :
- Structure : `/recalbox/share/roms/{console_folder}/`
- Noms de dossiers : Voir documentation Recalbox (ex: `nes`, `snes`, `gb`, etc.)
- Formats : ZIP ou décompressé selon console
- Référence : Documentation Recalbox + RomManager existant

**Romm** :
- Structure : Définie par Romm (à vérifier via API)
- Formats : Selon configuration Romm
- Référence : Documentation API Romm

## Architecture Patterns

### MVVM (Model-View-ViewModel)
- Pattern standard pour applications Avalonia/WPF
- Séparation UI / logique métier
- Binding bidirectionnel pour données
- Command pattern pour actions utilisateur

### Repository Pattern
- Abstraction accès données
- Facilite tests (mocking)
- Permet changement de stockage si nécessaire

### Service Layer
- Services métier isolés (ScanService, ExportService, SyncService)
- Logique réutilisable
- Testabilité améliorée

## Performance Considerations

### Scan Performance
- Calcul checksums : Parallélisation possible (Task.Parallel.ForEach)
- Scan archives : Streaming pour éviter chargement complet en mémoire
- Base de données : Index sur checksums pour recherche rapide

### UI Responsiveness
- Opérations longues : Exécution en arrière-plan (Task/async)
- Progress reporting : IProgress<T> pour feedback utilisateur
- Cancellation : CancellationToken pour annulation utilisateur

### Memory Management
- Archives imbriquées : Limite profondeur (5 niveaux)
- Streaming : Lecture fichiers par chunks, pas chargement complet
- Disposal : Utilisation `using` statements pour ressources

## Testing Strategy

### Unit Tests
- Logique métier isolée (Services, Repositories)
- Mocks pour dépendances externes (DB, APIs, fichiers)
- Framework : xUnit + Moq

### Integration Tests
- Tests avec SQLite en mémoire
- Tests avec fichiers réels (archives, ROMs de test)
- Tests API Romm avec mock server ou test instance

### UI Tests
- Tests workflows critiques (scan, export, sync)
- Framework : Avalonia.Headless (si disponible) ou tests manuels guidés

## Security Considerations

### File System Access
- Validation chemins utilisateur (éviter path traversal)
- Permissions fichiers : Lecture seule pour scan, écriture pour export

### API Security
- Authentification Romm : Stockage sécurisé credentials (pas en clair)
- Validation données API : Sanitization inputs, validation réponses

### Data Privacy
- Base SQLite locale : Pas de transmission données externes sauf synchronisation explicite
- Métadonnées utilisateur : Stockage local uniquement

## Deployment Considerations

### Multi-Platform Build
- .NET 8.0 : Support natif Linux, Windows, macOS
- Packaging : 
  - Linux : AppImage ou .deb (via dotnet publish)
  - Windows : MSI ou exe (via Wix Toolset ou Squirrel)
  - macOS : .dmg (via create-dmg)

### Dependencies
- .NET Runtime : Inclus dans package ou prérequis
- Bibliothèques natives : SharpCompress peut nécessiter dépendances système (à vérifier)

## Open Questions / TODOs

- [ ] Analyser format exact des datfiles NoIntro/Redump/GoodSet
- [ ] Vérifier structure dossiers Romm via API
- [ ] Tester performance scan avec collections réelles (1000+ ROMs)
- [ ] Définir stratégie mise à jour bases de données (téléchargement automatique ?)
- [ ] Évaluer nécessité cache pour checksums (éviter recalcul si fichier inchangé)

