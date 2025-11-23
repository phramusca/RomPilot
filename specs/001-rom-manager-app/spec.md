# Feature Specification: ROM Manager Application

**Feature Branch**: `001-rom-manager-app`  
**Created**: 2025-01-27  
**Status**: Draft  
**Input**: User description: "Il s'agit d'une application pour gérer des roms. Les fonctionnalités:

- importer des répertoires, scanner pour trouver des roms (tous les fichiers, meme dans des zip, 7z ou rar, en parcourant les archives dans les archives aussi récursivement)

- Extraire et grouper les roms par machine et par jeu, en utilisant les bases de données NoIntro, Redump, GoodSet,...

- Filtrer pour ne garder que la bonne version avec ordre de priorité par région/language donné. Ecarter les versions bad dump, ...

- Exporter vers recalbox et romm en respectant leurs conventions respectives (nom dossier machines, zip ou pas, formrats supportés,...)

- Synchroniser les métadatas:

  - Récupérer les infos de scrap depuis recalbox et romm.

  - Synchrniser les données utilisateur (save states, ratings, ...) avec recalbox (via gamelist.xml) et romm (via API)

Il s'agit de refaire https://github.com/phramusca/RomManager \"from scratch\" en changeant des choses: le scan (maintenant automatique, pas besoin de pré-classer par console), ajout des fonctionnalités manquantes, et autres améliorations.

Je ne veux pas nécessairement du java. C'est pour tourner sur PC (linux, windows, mac)."

## Clarifications

### Session 2025-01-27

- Q: Quels types de checksums/hash doivent être calculés lors du scan ? → A: Tous les types de hash utilisés par Romm, Recalbox, Redump, NoIntro, et autres bases de données (MD5, SHA1, SHA256, CRC32, etc.)
- Q: Quel est le rôle des checksums dans le processus d'identification ? → A: Les checksums servent à identifier les jeux (versions) en permettant la correspondance avec les entrées des bases de données de référence
- Q: Comment gérer les jeux qui peuvent être associés à différentes sources de bases de données ? → A: Chaque jeu peut être associé à différentes sources (Redump, GoodSet, NoIntro, etc.) et le système doit sélectionner celle à utiliser selon les critères de préférences utilisateur (région, langue, etc.)
- Q: Quels formats d'archives doivent être supportés lors du scan ? → A: ZIP, 7Z, et RAR (les trois formats les plus courants dans les collections de ROMs)
- Q: Quel niveau de détail doit être affiché pendant le scan pour voir ce qui a été traité ? → A: Feedback détaillé avec liste des fichiers traités/échoués et raisons d'échec pour chaque fichier non traité
- Q: Comment le système doit-il parcourir les dossiers et les archives ? → A: Le système doit parcourir récursivement le dossier choisi (tous les sous-dossiers) et être également récursif dans les archives (archives contenant d'autres archives, sans limite de profondeur jusqu'à la limite technique)

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Scanner et identifier les ROMs (Priority: P1)

Un utilisateur veut importer sa collection de ROMs depuis un ou plusieurs répertoires locaux. L'application doit automatiquement scanner tous les fichiers de manière récursive : (1) parcourir récursivement le système de fichiers (tous les sous-dossiers du répertoire choisi), (2) parcourir récursivement le contenu des archives (ZIP, 7Z, RAR), y compris les archives imbriquées (archives contenant d'autres archives). Pour chaque fichier ROM trouvé, l'application doit identifier la console correspondante et le jeu, en utilisant les bases de données de référence (NoIntro, Redump, GoodSet).

**Why this priority**: Cette fonctionnalité est la fondation de toute l'application. Sans capacité à scanner et identifier les ROMs, aucune autre fonctionnalité n'est possible. C'est le MVP minimum qui apporte de la valeur immédiate en permettant à l'utilisateur de voir sa collection organisée.

**Independent Test**: Peut être testé indépendamment en fournissant un répertoire de test contenant des ROMs dans différents formats (fichiers directs, ZIP, 7Z, RAR, archives imbriquées). L'utilisateur peut vérifier que tous les ROMs sont détectés, que les consoles sont correctement identifiées, et que les jeux sont reconnus via les bases de données. L'utilisateur peut également vérifier que la liste des fichiers traités et échoués est visible avec les raisons d'échec pour chaque fichier non traité.

**Acceptance Scenarios**:

1. **Given** un répertoire contenant des fichiers ROM directement accessibles (y compris dans des sous-dossiers), **When** l'utilisateur lance un scan, **Then** tous les fichiers ROM sont détectés récursivement dans tous les sous-dossiers et identifiés avec leur console et jeu correspondants
2. **Given** un répertoire contenant des archives ZIP avec des ROMs à l'intérieur, **When** l'utilisateur lance un scan, **Then** les ROMs dans les archives ZIP sont détectés et identifiés
3. **Given** un répertoire contenant des archives 7Z avec des ROMs à l'intérieur, **When** l'utilisateur lance un scan, **Then** les ROMs dans les archives 7Z sont détectés et identifiés
4. **Given** un répertoire contenant des archives RAR avec des ROMs à l'intérieur, **When** l'utilisateur lance un scan, **Then** les ROMs dans les archives RAR sont détectés et identifiés
5. **Given** un répertoire contenant des archives (ZIP, 7Z, ou RAR) qui contiennent elles-mêmes des archives avec des ROMs, **When** l'utilisateur lance un scan, **Then** les ROMs dans les archives imbriquées sont détectés récursivement
6. **Given** un répertoire mixte contenant des ROMs de différentes consoles (NES, SNES, Game Boy), **When** l'utilisateur lance un scan, **Then** chaque ROM est correctement associé à sa console d'origine sans pré-classement manuel requis
7. **Given** un ROM qui correspond à une entrée dans une base de données (NoIntro, Redump, ou GoodSet), **When** l'utilisateur lance un scan, **Then** le ROM est identifié avec le nom du jeu et les métadonnées de la base de données en utilisant les checksums calculés pour faire la correspondance
8. **Given** un répertoire contenant des ROMs, **When** l'utilisateur lance un scan, **Then** tous les types de checksums/hash requis (MD5, SHA1, SHA256, CRC32, etc.) sont calculés pour chaque ROM et utilisés pour identifier les jeux et versions via correspondance avec les bases de données
9. **Given** un jeu qui peut être identifié dans plusieurs bases de données (par exemple, présent à la fois dans Redump et GoodSet), **When** le système identifie le jeu, **Then** le jeu est associé à toutes les sources de bases de données pertinentes, et la source à utiliser est sélectionnée selon les préférences utilisateur (région, langue, etc.)
10. **Given** un scan en cours avec des fichiers traités et certains fichiers échoués, **When** l'utilisateur consulte l'interface de scan, **Then** l'utilisateur voit une liste détaillée de tous les fichiers traités (avec statut : succès, console identifiée, jeu identifié) et tous les fichiers échoués avec la raison d'échec explicite (ex: "Console non détectée", "Archive corrompue", "Fichier illisible", "Erreur de checksum")

---

### User Story 2 - Grouper et filtrer les ROMs par version (Priority: P2)

Un utilisateur veut que l'application regroupe automatiquement toutes les versions d'un même jeu trouvées dans sa collection, puis sélectionne automatiquement la meilleure version selon ses préférences (région, langue, qualité). Les versions marquées comme "bad dump" ou autres problèmes de qualité doivent être exclues automatiquement.

**Why this priority**: Une fois les ROMs identifiés, la valeur principale de l'application est de gérer intelligemment les doublons et versions multiples. Cette fonctionnalité permet à l'utilisateur de maintenir une collection propre sans intervention manuelle fastidieuse.

**Independent Test**: Peut être testé indépendamment en fournissant une collection contenant plusieurs versions du même jeu (par exemple, versions USA, EUR, JAP d'un même titre). L'utilisateur configure ses préférences de région/langue, et vérifie que le système groupe correctement les versions et sélectionne celle qui correspond à ses préférences.

**Acceptance Scenarios**:

1. **Given** plusieurs versions du même jeu détectées (USA, EUR, JAP), **When** l'utilisateur configure ses préférences de région (ex: EUR > USA > JAP), **Then** toutes les versions sont regroupées sous un même jeu, la source de base de données appropriée est sélectionnée selon les préférences, et la version EUR est marquée comme sélectionnée
2. **Given** plusieurs versions d'un jeu dont certaines sont marquées "bad dump" dans la base de données, **When** le système applique le filtrage, **Then** les versions "bad dump" sont exclues de la sélection automatique
3. **Given** un jeu avec plusieurs versions de qualité équivalente mais différentes langues, **When** l'utilisateur configure ses préférences de langue (ex: FR > EN > autres), **Then** la version dans la langue préférée est sélectionnée
4. **Given** un jeu avec une seule version disponible, **When** le système applique le filtrage, **Then** cette version unique est automatiquement sélectionnée
5. **Given** un utilisateur qui modifie ses préférences de région/langue après un scan initial, **When** l'utilisateur applique les nouvelles préférences, **Then** la sélection des versions est mise à jour selon les nouvelles préférences

---

### User Story 3 - Exporter vers Recalbox et Romm (Priority: P3)

Un utilisateur veut exporter sa collection de ROMs filtrée vers des plateformes de gestion de ROMs (Recalbox et Romm). L'export doit respecter les conventions spécifiques de chaque plateforme : noms de dossiers de consoles, formats de fichiers acceptés (ZIP ou fichiers décompressés), et autres exigences de structure.

**Why this priority**: L'export est la finalité pratique de la gestion de collection. Une fois les ROMs organisés et filtrés, l'utilisateur veut les utiliser dans ses systèmes de jeu. Cette fonctionnalité complète le cycle de vie de la gestion de ROMs.

**Independent Test**: Peut être testé indépendamment en exportant une petite collection vers des dossiers de destination Recalbox et Romm. L'utilisateur vérifie que les fichiers sont placés dans les bons dossiers (noms de consoles corrects), que les formats respectent les conventions (ZIP pour Recalbox si requis, formats supportés), et que la structure de fichiers est correcte.

**Acceptance Scenarios**:

1. **Given** une collection de ROMs filtrée et organisée, **When** l'utilisateur choisit d'exporter vers Recalbox, **Then** les ROMs sont copiés dans les dossiers de consoles avec les noms de dossiers corrects selon les conventions Recalbox
2. **Given** une collection de ROMs filtrée, **When** l'utilisateur choisit d'exporter vers Romm, **Then** les ROMs sont copiés dans la structure de dossiers attendue par Romm avec les noms de dossiers corrects
3. **Given** Recalbox nécessite des ROMs dans un format spécifique (ZIP ou décompressé), **When** l'utilisateur exporte vers Recalbox, **Then** les ROMs sont convertis/formatés selon les exigences de Recalbox
4. **Given** Romm nécessite des ROMs dans un format spécifique, **When** l'utilisateur exporte vers Romm, **Then** les ROMs sont convertis/formatés selon les exigences de Romm
5. **Given** un export précédent existe déjà dans le dossier de destination, **When** l'utilisateur relance un export, **Then** le système gère les conflits (remplacement, fusion, ou demande de confirmation selon configuration)

---

### User Story 4 - Synchroniser les métadonnées (Priority: P4)

Un utilisateur veut synchroniser les métadonnées de jeux entre l'application et les plateformes Recalbox et Romm. Cela inclut la récupération des données de scrap (descriptions, images, ratings) depuis ces plateformes, ainsi que la synchronisation bidirectionnelle des données utilisateur (favoris, ratings, statistiques de jeu, save states) entre l'application et les plateformes.

**Why this priority**: Les métadonnées enrichissent l'expérience utilisateur mais ne sont pas essentielles pour le fonctionnement de base. Cette fonctionnalité améliore la valeur de l'application en permettant une gestion complète de la collection avec toutes les informations associées.

**Independent Test**: Peut être testé indépendamment en configurant la connexion à Recalbox (via gamelist.xml) et Romm (via API). L'utilisateur vérifie que les métadonnées de scrap sont récupérées, que les favoris/ratings sont synchronisés dans les deux sens, et que les conflits sont gérés correctement.

**Acceptance Scenarios**:

1. **Given** une collection de ROMs exportée vers Recalbox, **When** l'utilisateur lance une synchronisation de métadonnées depuis Recalbox, **Then** les données de scrap (descriptions, images, ratings) sont récupérées depuis les fichiers gamelist.xml de Recalbox
2. **Given** une collection de ROMs exportée vers Romm, **When** l'utilisateur lance une synchronisation de métadonnées depuis Romm, **Then** les données de scrap sont récupérées via l'API REST de Romm
3. **Given** un utilisateur qui marque un jeu comme favori dans l'application, **When** l'utilisateur synchronise vers Recalbox, **Then** le statut favori est écrit dans le fichier gamelist.xml correspondant
4. **Given** un utilisateur qui modifie un rating dans Recalbox, **When** l'utilisateur synchronise depuis Recalbox, **Then** le rating mis à jour est importé dans l'application
5. **Given** des métadonnées différentes pour le même jeu dans l'application et dans Recalbox/Romm, **When** l'utilisateur synchronise, **Then** le système applique une stratégie de résolution de conflits (dernière modification, source la plus récente, ou demande de confirmation)
6. **Given** des statistiques de jeu (temps de jeu, dernière partie) dans Recalbox, **When** l'utilisateur synchronise depuis Recalbox, **Then** ces statistiques sont importées dans l'application

---

### Edge Cases

- Que se passe-t-il si un fichier ROM est corrompu ou illisible pendant le scan ? → Le fichier doit apparaître dans la liste des fichiers échoués avec la raison "Fichier corrompu" ou "Fichier illisible", et le scan continue avec les autres fichiers
- Comment le système gère-t-il les ROMs qui ne correspondent à aucune entrée dans les bases de données (NoIntro, Redump, GoodSet) ?
- Que se passe-t-il si plusieurs bases de données contiennent des informations contradictoires pour le même ROM ?
- Comment le système gère-t-il les archives très volumineuses ou profondément imbriquées (risque de dépassement de mémoire) pour les formats ZIP, 7Z et RAR ?
- Que se passe-t-il si un export échoue partiellement (certains fichiers copiés, d'autres non) ?
- Comment le système gère-t-il les caractères spéciaux dans les noms de fichiers lors de l'export vers différentes plateformes ?
- Que se passe-t-il si les préférences de région/langue ne permettent pas de sélectionner une version unique (toutes les versions ont la même priorité) ?
- Comment le système gère-t-il les ROMs qui appartiennent à plusieurs jeux ou sont des compilations ?
- Que se passe-t-il si la connexion à l'API Romm échoue pendant une synchronisation ?
- Comment le système gère-t-il les fichiers gamelist.xml malformés ou incomplets dans Recalbox ?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST scan directories recursively (all subdirectories) to find ROM files, and MUST recursively process archives (ZIP, 7Z, RAR) including nested archives (archives within archives) at any depth
- **FR-002**: System MUST calculate all checksum/hash types used by Romm, Recalbox, Redump, NoIntro, and other reference databases (MD5, SHA1, SHA256, CRC32, and any other hash types required by these platforms) for each ROM file during scanning
- **FR-003**: System MUST use checksums/hashes to identify games and versions by matching against entries in reference databases
- **FR-004**: System MUST automatically identify the console/platform for each detected ROM file without requiring manual pre-classification
- **FR-005**: System MUST identify games using reference databases (NoIntro, Redump, GoodSet) when available, and MUST support associating each game with multiple database sources
- **FR-006**: System MUST select which database source to use for each game based on user-configured preferences (region, language, etc.) when multiple sources are available
- **FR-007**: System MUST group multiple versions of the same game together
- **FR-008**: System MUST allow users to configure priority preferences for region selection (e.g., EUR > USA > JAP)
- **FR-009**: System MUST allow users to configure priority preferences for language selection
- **FR-010**: System MUST automatically exclude ROM versions marked as "bad dump" or with quality issues from automatic selection
- **FR-011**: System MUST automatically select the best ROM version for each game based on user-configured region and language preferences, including selection of the appropriate database source when multiple sources are available
- **FR-012**: System MUST export ROMs to Recalbox following Recalbox folder naming conventions for consoles
- **FR-013**: System MUST export ROMs to Romm following Romm folder naming conventions for consoles
- **FR-014**: System MUST respect format requirements for each platform (ZIP vs uncompressed, supported file formats)
- **FR-015**: System MUST retrieve scrap metadata (descriptions, images, ratings) from Recalbox via gamelist.xml files
- **FR-016**: System MUST retrieve scrap metadata from Romm via REST API
- **FR-017**: System MUST synchronize user data (favorites, ratings, play statistics) bidirectionally with Recalbox via gamelist.xml
- **FR-018**: System MUST synchronize user data bidirectionally with Romm via REST API
- **FR-019**: System MUST handle conflicts when metadata differs between application and platforms (last modified wins, or user confirmation)
- **FR-020**: System MUST support scanning from multiple source directories
- **FR-021**: System MUST preserve file integrity during export operations
- **FR-022**: System MUST provide detailed progress feedback during scan operations, including a list of all files processed (with status: success, console identified, game identified) and all files that failed processing with explicit failure reasons (e.g., "Console not detected", "Corrupted archive", "Unreadable file", "Checksum error")
- **FR-023**: System MUST handle errors gracefully and provide meaningful error messages to users, including specific reasons why individual files could not be processed during scanning

### Key Entities *(include if feature involves data)*

- **ROM File**: Represents a single ROM file found during scanning. Attributes: file path, file size, archive location (if in ZIP/7Z/RAR archive), detected console/platform, game identification (if matched in database via checksum matching), version information (region, language, quality flags), checksums/hashes (MD5, SHA1, SHA256, CRC32, and any other hash types required by Romm, Recalbox, Redump, NoIntro, and other reference databases - used for game/version identification), processing status (success, failed), failure reason (if processing failed: e.g., "Console not detected", "Corrupted archive", "Unreadable file", "Checksum error", "Archive extraction failed")
- **Game**: Represents a logical game that may have multiple ROM versions. Attributes: game name, console/platform, associated database sources (can be associated with multiple sources: NoIntro, Redump, GoodSet, etc.), selected database source (chosen based on user preferences when multiple sources available), grouped ROM versions, selected version (best match based on preferences), metadata (description, images, ratings)
- **Console/Platform**: Represents a gaming console or platform. Attributes: platform name, folder naming conventions for Recalbox, folder naming conventions for Romm, supported file formats, export requirements
- **User Preferences**: Represents user configuration for filtering and selection. Attributes: region priority order, language priority order, quality filters (exclude bad dumps), export settings per platform
- **Metadata**: Represents game metadata from various sources. Attributes: description, cover images, screenshots, videos, ratings, release date, developer, publisher, genre, user data (favorites, play statistics, save states), source (scraped from Recalbox/Romm or user-entered), last modified timestamp
- **Export Configuration**: Represents settings for exporting to a platform. Attributes: target platform (Recalbox/Romm), destination directory, format requirements (ZIP/uncompressed), conflict resolution strategy
- **Sync Configuration**: Represents settings for metadata synchronization. Attributes: source platform, sync direction (bidirectional/import only/export only), conflict resolution strategy, sync scope (which metadata types to sync)

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can scan a directory containing 1000+ ROM files (including archives) and have all ROMs identified within 5 minutes on a standard desktop computer
- **SC-002**: System correctly identifies console/platform for 95%+ of ROM files without manual intervention
- **SC-003**: System successfully groups versions of the same game with 99%+ accuracy when multiple versions exist in the collection
- **SC-004**: System automatically selects the preferred version (based on user preferences) for 90%+ of games with multiple versions
- **SC-005**: Users can export a collection of 500 games to Recalbox or Romm in under 2 minutes
- **SC-006**: Exported ROMs are correctly placed in platform-specific folder structures with 100% accuracy (all files in correct console folders)
- **SC-007**: Metadata synchronization completes successfully for 95%+ of games when source data is available
- **SC-008**: Users can complete a full workflow (scan → filter → export → sync metadata) for a new collection in under 10 minutes
- **SC-009**: System handles archives nested up to 5 levels deep without performance degradation or errors
- **SC-010**: Application runs on Windows, Linux, and macOS with consistent functionality across all platforms
