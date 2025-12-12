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

### Session 2025-12-12

- Q: Comment les utilisateurs doivent-ils obtenir les bases de données de référence (NoIntro, Redump, GoodSet) ? → A: L'application doit inclure un gestionnaire intégré de téléchargement des bases de données avec interface dédiée pour sélectionner et télécharger les versions
- Q: Comment le système doit-il gérer les scans incrémentaux sur un répertoire déjà scanné ? → A: L'utilisateur choisit entre scan rapide (timestamp/taille) et scan complet (tous checksums) à chaque fois
- Q: Comment les exports vers Recalbox et Romm doivent-ils être effectués (local vs distant) ? → A: Support pour export local (dossier) et distant via SSH/SFTP avec configuration dans l'interface
- Q: Comment gérer les préférences de région et format vidéo (PAL/NTSC) ? → A: Préférences séparées pour région ET format vidéo (PAL/NTSC/NTSC-J) avec ordre de priorité configurable
- Q: Quel type d'interface pour la Partie Bibliothèque (affichage des jeux avec métadonnées) ? → A: Interface dédiée "Bibliothèque" avec vue grille/liste, filtres avancés, et aperçu des métadonnées

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
11. **Given** un répertoire déjà scanné précédemment, **When** l'utilisateur lance un nouveau scan, **Then** il peut choisir entre un "scan rapide" (détection basée sur timestamp et taille, recalcul des checksums uniquement pour fichiers modifiés) ou un "scan complet" (recalcul de tous les checksums)
12. **Given** un scan rapide sur un répertoire déjà scanné, **When** un fichier a le même timestamp et la même taille, **Then** le système réutilise les checksums et identifications précédentes sans recalcul
13. **Given** un scan rapide sur un répertoire déjà scanné, **When** un fichier a un timestamp ou une taille différente, **Then** le système recalcule les checksums et réidentifie le ROM
14. **Given** un scan sur un répertoire déjà scanné, **When** des fichiers ont été supprimés depuis le dernier scan, **Then** ces fichiers sont retirés de la base de données de l'application
15. **Given** un scan sur un répertoire déjà scanné, **When** de nouveaux fichiers ont été ajoutés depuis le dernier scan, **Then** ces nouveaux fichiers sont scannés et ajoutés à la base de données

---

### User Story 2 - Gérer les bases de données de référence (Priority: P1.5)

Un utilisateur veut télécharger et gérer les bases de données de référence (NoIntro, Redump, GoodSet) nécessaires pour identifier ses ROMs. L'application doit fournir une interface dédiée permettant de naviguer dans les bases de données disponibles, de sélectionner les consoles et versions souhaitées, et de télécharger les fichiers de données. L'utilisateur doit pouvoir gérer plusieurs versions d'une même base de données pour une même console.

**Why this priority**: Cette fonctionnalité est essentielle car sans les bases de données, l'identification des ROMs est impossible. Elle doit être disponible très tôt dans le cycle de vie de l'application, juste après ou en parallèle du scan, pour permettre la correspondance avec les ROMs scannés.

**Independent Test**: Peut être testé indépendamment en vérifiant que l'utilisateur peut accéder à l'interface de gestion des bases de données, voir la liste des providers disponibles (NoIntro, Redump, GoodSet), sélectionner une console, voir les versions disponibles pour cette console, télécharger une version spécifique, et voir les bases de données téléchargées dans un tableau avec filtres.

**Acceptance Scenarios**:

1. **Given** l'utilisateur accède à l'interface de gestion des bases de données, **When** il consulte la liste, **Then** il voit tous les providers disponibles (NoIntro, Redump, GoodSet) avec leurs consoles associées
2. **Given** l'utilisateur sélectionne un provider et une console, **When** il consulte les versions disponibles, **Then** il voit la liste des versions avec leurs dates de publication et tailles
3. **Given** l'utilisateur sélectionne une version spécifique d'une base de données, **When** il lance le téléchargement, **Then** le fichier est téléchargé avec une barre de progression et un statut de téléchargement
4. **Given** plusieurs bases de données ont été téléchargées, **When** l'utilisateur consulte l'interface, **Then** il voit un tableau affichant toutes les bases téléchargées avec filtres par provider, console et version
5. **Given** une nouvelle version d'une base de données est disponible, **When** l'utilisateur consulte l'interface, **Then** il est notifié de la disponibilité d'une mise à jour
6. **Given** l'utilisateur possède plusieurs versions d'une même base de données pour une console, **When** il configure ses préférences, **Then** il peut sélectionner quelle version utiliser par défaut pour l'identification des ROMs
7. **Given** un téléchargement échoue (connexion perdue, fichier indisponible), **When** l'utilisateur consulte le statut, **Then** il voit un message d'erreur clair et peut relancer le téléchargement

---

### User Story 3 - Grouper et filtrer les ROMs par version (Priority: P2)

Un utilisateur veut que l'application regroupe automatiquement toutes les versions d'un même jeu trouvées dans sa collection, puis sélectionne automatiquement la meilleure version selon ses préférences (région, format vidéo PAL/NTSC, langue, qualité). Les versions marquées comme "bad dump" ou autres problèmes de qualité doivent être exclues automatiquement.

**Why this priority**: Une fois les ROMs identifiés, la valeur principale de l'application est de gérer intelligemment les doublons et versions multiples. Cette fonctionnalité permet à l'utilisateur de maintenir une collection propre sans intervention manuelle fastidieuse.

**Independent Test**: Peut être testé indépendamment en fournissant une collection contenant plusieurs versions du même jeu (par exemple, versions USA, EUR, JAP d'un même titre, avec différents formats PAL/NTSC). L'utilisateur configure ses préférences de région/format/langue, et vérifie que le système groupe correctement les versions et sélectionne celle qui correspond à ses préférences.

**Acceptance Scenarios**:

1. **Given** plusieurs versions du même jeu détectées (USA, EUR, JAP), **When** l'utilisateur configure ses préférences de région (ex: EUR > USA > JAP), **Then** toutes les versions sont regroupées sous un même jeu, la source de base de données appropriée est sélectionnée selon les préférences, et la version EUR est marquée comme sélectionnée
2. **Given** plusieurs versions d'un jeu dont certaines sont marquées "bad dump" dans la base de données, **When** le système applique le filtrage, **Then** les versions "bad dump" sont exclues de la sélection automatique
3. **Given** un jeu avec plusieurs versions de qualité équivalente mais différentes langues, **When** l'utilisateur configure ses préférences de langue (ex: FR > EN > autres), **Then** la version dans la langue préférée est sélectionnée
4. **Given** un jeu avec plusieurs versions ayant différents formats vidéo (PAL, NTSC, NTSC-J), **When** l'utilisateur configure ses préférences de format vidéo (ex: PAL > NTSC > NTSC-J), **Then** la version avec le format vidéo préféré est sélectionnée
5. **Given** un jeu avec plusieurs versions ayant des combinaisons région/format différentes, **When** le système applique les préférences combinées (région ET format vidéo), **Then** la version optimale est sélectionnée selon les deux critères de priorité
6. **Given** un jeu avec une seule version disponible, **When** le système applique le filtrage, **Then** cette version unique est automatiquement sélectionnée
7. **Given** un utilisateur qui modifie ses préférences de région/format/langue après un scan initial, **When** l'utilisateur applique les nouvelles préférences, **Then** la sélection des versions est mise à jour selon les nouvelles préférences

---

### User Story 4 - Exporter vers Recalbox et Romm (Priority: P3)

Un utilisateur veut exporter sa collection de ROMs filtrée vers des plateformes de gestion de ROMs (Recalbox et Romm). L'export doit respecter les conventions spécifiques de chaque plateforme : noms de dossiers de consoles, formats de fichiers acceptés (ZIP ou fichiers décompressés), et autres exigences de structure. L'utilisateur doit pouvoir exporter soit vers un dossier local (par exemple un disque externe ou un répertoire réseau monté), soit vers un serveur distant via SSH/SFTP (par exemple un Recalbox ou serveur Romm distant).

**Why this priority**: L'export est la finalité pratique de la gestion de collection. Une fois les ROMs organisés et filtrés, l'utilisateur veut les utiliser dans ses systèmes de jeu. Cette fonctionnalité complète le cycle de vie de la gestion de ROMs. Le support des exports distants est essentiel pour les utilisateurs avec des serveurs dédiés.

**Independent Test**: Peut être testé indépendamment en exportant une petite collection vers des dossiers de destination Recalbox et Romm. L'utilisateur vérifie que les fichiers sont placés dans les bons dossiers (noms de consoles corrects), que les formats respectent les conventions (ZIP pour Recalbox si requis, formats supportés), et que la structure de fichiers est correcte. Test avec export local et export distant via SSH/SFTP.

**Acceptance Scenarios**:

1. **Given** une collection de ROMs filtrée et organisée, **When** l'utilisateur choisit d'exporter vers Recalbox sur un dossier local, **Then** les ROMs sont copiés dans les dossiers de consoles avec les noms de dossiers corrects selon les conventions Recalbox
2. **Given** une collection de ROMs filtrée, **When** l'utilisateur choisit d'exporter vers Romm sur un dossier local, **Then** les ROMs sont copiés dans la structure de dossiers attendue par Romm avec les noms de dossiers corrects
3. **Given** une configuration SSH/SFTP vers un serveur Recalbox distant, **When** l'utilisateur lance un export, **Then** les ROMs sont transférés via SSH/SFTP vers les dossiers corrects du serveur distant
4. **Given** une configuration SSH/SFTP vers un serveur Romm distant, **When** l'utilisateur lance un export, **Then** les ROMs sont transférés via SSH/SFTP vers les dossiers corrects du serveur distant
5. **Given** l'utilisateur configure une connexion SSH/SFTP, **When** il teste la connexion, **Then** le système vérifie la connectivité et les permissions d'accès au répertoire cible
6. **Given** Recalbox nécessite des ROMs dans un format spécifique (ZIP ou décompressé), **When** l'utilisateur exporte vers Recalbox, **Then** les ROMs sont convertis/formatés selon les exigences de Recalbox
7. **Given** Romm nécessite des ROMs dans un format spécifique, **When** l'utilisateur exporte vers Romm, **Then** les ROMs sont convertis/formatés selon les exigences de Romm
8. **Given** un export précédent existe déjà dans le dossier de destination (local ou distant), **When** l'utilisateur relance un export, **Then** le système gère les conflits (remplacement, fusion, ou demande de confirmation selon configuration)
9. **Given** un export distant via SSH/SFTP en cours, **When** l'utilisateur consulte l'interface, **Then** il voit une barre de progression avec le nombre de fichiers transférés et la vitesse de transfert
10. **Given** une erreur de connexion SSH/SFTP pendant un export, **When** le transfert échoue, **Then** le système affiche un message d'erreur clair et propose de réessayer

---

### User Story 5 - Synchroniser les métadonnées (Priority: P4)

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

### User Story 6 - Visualiser la collection dans la Bibliothèque (Priority: P3)

Un utilisateur veut visualiser sa collection de jeux dans une interface dédiée "Bibliothèque" qui offre plusieurs modes d'affichage (vue grille avec vignettes, vue liste détaillée). L'interface doit permettre de filtrer les jeux par console, région, langue, statut, et autres critères. Pour chaque jeu, l'utilisateur doit pouvoir voir les métadonnées enrichies (descriptions, images, vidéos, notes) et gérer les versions multiples disponibles.

**Why this priority**: Cette fonctionnalité transforme l'application d'un outil technique en une vraie interface de gestion de collection attractive. Elle permet à l'utilisateur d'explorer et de gérer sa collection de manière intuitive et visuelle, similaire à RomManager.

**Independent Test**: Peut être testé indépendamment avec une collection déjà scannée et identifiée. L'utilisateur accède à l'interface Bibliothèque, bascule entre vue grille et liste, applique des filtres, sélectionne un jeu et visualise ses métadonnées complètes avec images/vidéos.

**Acceptance Scenarios**:

1. **Given** une collection de jeux identifiés, **When** l'utilisateur accède à la Bibliothèque, **Then** il voit tous ses jeux affichés en vue grille avec vignettes (cover art)
2. **Given** l'utilisateur est en vue grille, **When** il bascule vers la vue liste, **Then** les jeux s'affichent en tableau avec colonnes (nom, console, région, format, statut, taille)
3. **Given** l'utilisateur consulte la Bibliothèque, **When** il applique un filtre par console (ex: "SNES"), **Then** seuls les jeux SNES sont affichés
4. **Given** l'utilisateur consulte la Bibliothèque, **When** il applique des filtres combinés (console + région + statut), **Then** les jeux correspondant à tous les critères sont affichés
5. **Given** l'utilisateur sélectionne un jeu dans la Bibliothèque, **When** il visualise les détails, **Then** il voit toutes les métadonnées disponibles (description, images, captures d'écran, rating, développeur, éditeur, date de sortie)
6. **Given** un jeu avec plusieurs versions disponibles, **When** l'utilisateur consulte les détails, **Then** il voit la liste de toutes les versions avec possibilité de changer la version sélectionnée
7. **Given** un jeu avec une vidéo de preview disponible, **When** l'utilisateur consulte les détails, **Then** il peut lancer la lecture de la vidéo directement dans l'interface
8. **Given** l'utilisateur recherche un jeu spécifique, **When** il utilise la barre de recherche, **Then** les résultats sont filtrés en temps réel selon le texte saisi
9. **Given** l'utilisateur consulte la Bibliothèque, **When** il trie les jeux par nom/date/note/console, **Then** l'affichage est réorganisé selon le critère de tri choisi
10. **Given** des métadonnées manquantes pour certains jeux, **When** l'utilisateur consulte la Bibliothèque, **Then** il voit clairement quels jeux ont des métadonnées incomplètes avec indicateur visuel

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
- **FR-006.1**: System MUST provide an integrated database manager interface for browsing, selecting, and downloading reference databases (NoIntro, Redump, GoodSet)
- **FR-006.2**: System MUST allow users to view available database providers, consoles, and versions in a dedicated interface with filtering capabilities
- **FR-006.3**: System MUST support downloading multiple versions of the same database for a single console
- **FR-006.4**: System MUST display download progress and status for database downloads
- **FR-006.5**: System MUST allow users to select which database version to use as default when multiple versions exist for the same console
- **FR-006.6**: System MUST display all downloaded databases in a table view with filters by provider, console, and version
- **FR-006.7**: System MUST notify users when updates to downloaded databases are available
- **FR-006.8**: System MUST handle download failures gracefully with clear error messages and retry capability
- **FR-007**: System MUST group multiple versions of the same game together
- **FR-008**: System MUST allow users to configure priority preferences for region selection (e.g., EUR > USA > JAP)
- **FR-009**: System MUST allow users to configure priority preferences for language selection
- **FR-009.1**: System MUST allow users to configure priority preferences for video format selection (PAL, NTSC, NTSC-J) as a separate preference list from region
- **FR-009.2**: System MUST apply both region and video format preferences when selecting the best ROM version (both criteria evaluated with configurable priority)
- **FR-010**: System MUST automatically exclude ROM versions marked as "bad dump" or with quality issues from automatic selection
- **FR-011**: System MUST automatically select the best ROM version for each game based on user-configured region, video format, and language preferences, including selection of the appropriate database source when multiple sources are available
- **FR-012**: System MUST export ROMs to Recalbox following Recalbox folder naming conventions for consoles
- **FR-013**: System MUST export ROMs to Romm following Romm folder naming conventions for consoles
- **FR-014**: System MUST respect format requirements for each platform (ZIP vs uncompressed, supported file formats)
- **FR-014.1**: System MUST support export to local directories (local filesystem or mounted network drives)
- **FR-014.2**: System MUST support export to remote servers via SSH/SFTP protocol
- **FR-014.3**: System MUST provide an interface for configuring SSH/SFTP connection parameters (host, port, username, password/key, target directory)
- **FR-014.4**: System MUST allow users to test SSH/SFTP connection before starting export
- **FR-014.5**: System MUST display transfer progress for remote exports (number of files transferred, transfer speed, estimated time remaining)
- **FR-014.6**: System MUST handle SSH/SFTP connection failures gracefully with clear error messages and retry capability
- **FR-015**: System MUST retrieve scrap metadata (descriptions, images, ratings) from Recalbox via gamelist.xml files
- **FR-016**: System MUST retrieve scrap metadata from Romm via REST API
- **FR-017**: System MUST synchronize user data (favorites, ratings, play statistics) bidirectionally with Recalbox via gamelist.xml
- **FR-018**: System MUST synchronize user data bidirectionally with Romm via REST API
- **FR-019**: System MUST handle conflicts when metadata differs between application and platforms (last modified wins, or user confirmation)
- **FR-020**: System MUST support scanning from multiple source directories
- **FR-021**: System MUST preserve file integrity during export operations
- **FR-022**: System MUST provide detailed progress feedback during scan operations, including a list of all files processed (with status: success, console identified, game identified) and all files that failed processing with explicit failure reasons (e.g., "Console not detected", "Corrupted archive", "Unreadable file", "Checksum error")
- **FR-023**: System MUST handle errors gracefully and provide meaningful error messages to users, including specific reasons why individual files could not be processed during scanning
- **FR-024**: System MUST offer users a choice between "quick scan" and "full scan" when rescanning a previously scanned directory
- **FR-025**: In quick scan mode, System MUST detect file changes by comparing timestamp and file size, and MUST recalculate checksums only for files that have changed
- **FR-026**: In quick scan mode, System MUST reuse previously calculated checksums and identifications for files with unchanged timestamp and size
- **FR-027**: In full scan mode, System MUST recalculate all checksums for all files regardless of timestamp or size changes
- **FR-028**: During rescan operations (quick or full), System MUST remove from database any files that no longer exist in the scanned directory
- **FR-029**: During rescan operations (quick or full), System MUST add newly discovered files to the database
- **FR-030**: System MUST provide a dedicated Library interface for visualizing the game collection
- **FR-031**: System MUST support grid view display mode showing games with cover art thumbnails
- **FR-032**: System MUST support list view display mode showing games in a table with sortable columns (name, console, region, format, status, size)
- **FR-033**: System MUST allow users to switch between grid and list view modes
- **FR-034**: System MUST provide filtering capabilities by console, region, language, video format, status, and other game attributes
- **FR-035**: System MUST support combined filters (multiple criteria applied simultaneously)
- **FR-036**: System MUST provide a search bar with real-time filtering as user types
- **FR-037**: System MUST allow sorting games by name, release date, rating, console, and other attributes
- **FR-038**: System MUST display complete metadata for selected games including description, images, screenshots, videos, rating, developer, publisher, release date
- **FR-039**: System MUST allow users to view and manage all available versions of a game when multiple versions exist
- **FR-040**: System MUST support in-app video playback for game preview videos
- **FR-041**: System MUST visually indicate games with incomplete or missing metadata

### Key Entities *(include if feature involves data)*

- **ROM File**: Represents a single ROM file found during scanning. Attributes: file path, file size, last modified timestamp, archive location (if in ZIP/7Z/RAR archive), detected console/platform, game identification (if matched in database via checksum matching), version information (region, video format [PAL/NTSC/NTSC-J], language, quality flags), checksums/hashes (MD5, SHA1, SHA256, CRC32, and any other hash types required by Romm, Recalbox, Redump, NoIntro, and other reference databases - used for game/version identification), processing status (success, failed), failure reason (if processing failed: e.g., "Console not detected", "Corrupted archive", "Unreadable file", "Checksum error", "Archive extraction failed"), last scan date, scan type (quick/full)
- **Game**: Represents a logical game that may have multiple ROM versions. Attributes: game name, console/platform, associated database sources (can be associated with multiple sources: NoIntro, Redump, GoodSet, etc.), selected database source (chosen based on user preferences when multiple sources available), grouped ROM versions, selected version (best match based on preferences), metadata (description, images, ratings)
- **Console/Platform**: Represents a gaming console or platform. Attributes: platform name, folder naming conventions for Recalbox, folder naming conventions for Romm, supported file formats, export requirements
- **Reference Database**: Represents a downloaded reference database file (NoIntro, Redump, GoodSet). Attributes: provider name (NoIntro/Redump/GoodSet), console/platform, version identifier, release date, file size, download status, file path, is_default (boolean indicating if this version is the default for this console), last updated timestamp, available update flag
- **User Preferences**: Represents user configuration for filtering and selection. Attributes: region priority order (e.g., EUR > USA > JAP), video format priority order (e.g., PAL > NTSC > NTSC-J - separate from region preferences), language priority order (e.g., FR > EN > others), quality filters (exclude bad dumps), export settings per platform
- **Metadata**: Represents game metadata from various sources. Attributes: description, cover images, screenshots, videos, ratings, release date, developer, publisher, genre, user data (favorites, play statistics, save states), source (scraped from Recalbox/Romm or user-entered), last modified timestamp
- **Export Configuration**: Represents settings for exporting to a platform. Attributes: target platform (Recalbox/Romm), export type (local/remote), destination directory (for local exports), SSH/SFTP connection settings (host, port, username, authentication method, password/key path, remote directory - for remote exports), format requirements (ZIP/uncompressed), conflict resolution strategy, connection test status
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
- **SC-011**: Library interface can display and filter a collection of 5000+ games without performance degradation
- **SC-012**: Users can switch between grid and list views with less than 500ms transition time
- **SC-013**: Real-time search filtering returns results within 100ms for collections up to 10,000 games
