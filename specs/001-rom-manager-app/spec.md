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
- Q: Comment le scan doit-il déterminer quels fichiers traiter sans présupposer ce qui est une ROM ? → A: Scanner tous les fichiers avec exclusions par défaut de types évidents (images, textes, exécutables), et permettre à l'utilisateur de configurer des filtres d'exclusion personnalisés (extensions, tailles)
- Q: Quelle liste d'extensions doit être exclue par défaut lors du scan ? → A: .jpg .jpeg .png .gif .bmp .txt .nfo .diz .exe .dll .so .doc .pdf .html .xml (liste configurable par l'utilisateur)
- Q: Comment persister les données scannées (checksums, identifications, métadonnées, préférences) ? → A: Base de données SQLite locale (fichier .db unique avec requêtes SQL)
- Q: Quelle exigence de couverture de tests pour ce projet (TDD mentionné dans specs.md) ? → A: 80%+ couverture tests unitaires + tests d'intégration pour user stories critiques
- Q: Quelle approche de revue de code et standards pour garantir les bonnes pratiques ? → A: Linter/formatter automatique uniquement (sans revue humaine obligatoire)

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Scanner et identifier les ROMs (Priority: P1)

Un utilisateur veut importer sa collection de ROMs depuis un ou plusieurs répertoires locaux. L'application doit automatiquement scanner tous les fichiers de manière récursive : (1) parcourir récursivement le système de fichiers (tous les sous-dossiers du répertoire choisi), (2) parcourir récursivement le contenu des archives (ZIP, 7Z, RAR), y compris les archives imbriquées (archives contenant d'autres archives). Le scan ne doit pas présupposer quels fichiers sont des ROMs (pas de filtrage par extension) : tous les fichiers sont candidats, sauf ceux exclus par des filtres (exclusions par défaut de types évidents comme images .jpg/.png, textes .txt/.nfo, exécutables .exe/.dll, et filtres personnalisables par l'utilisateur). Pour chaque fichier scanné, l'application calcule les checksums et tente d'identifier la console et le jeu correspondants en les comparant avec les bases de données de référence (NoIntro, Redump, GoodSet). Les fichiers qui correspondent sont identifiés comme ROMs, les autres restent non identifiés.

**Why this priority**: Cette fonctionnalité est la fondation de toute l'application. Sans capacité à scanner et identifier les ROMs, aucune autre fonctionnalité n'est possible. C'est le MVP minimum qui apporte de la valeur immédiate en permettant à l'utilisateur de voir sa collection organisée.

**Independent Test**: Peut être testé indépendamment en fournissant un répertoire de test contenant des fichiers variés (ROMs dans différents formats, fichiers directs, ZIP, 7Z, RAR, archives imbriquées, et aussi des fichiers non-ROM). L'utilisateur peut vérifier que tous les fichiers candidats sont scannés (hors exclusions), que les checksums sont calculés, que les ROMs correspondant aux bases de données sont correctement identifiés avec leur console et jeu, et que les fichiers non identifiés restent listés séparément. L'utilisateur peut également vérifier que la liste des fichiers traités et échoués est visible avec les raisons d'échec pour chaque fichier non traité.

**Acceptance Scenarios**:

1. **Given** un répertoire contenant divers fichiers (y compris dans des sous-dossiers), **When** l'utilisateur lance un scan, **Then** tous les fichiers sont scannés récursivement (hors exclusions par défaut), les checksums sont calculés, et les fichiers correspondant aux bases de données sont identifiés comme ROMs avec leur console et jeu correspondants
2. **Given** un répertoire contenant des fichiers d'exclusion par défaut (.jpg, .jpeg, .png, .gif, .bmp, .txt, .nfo, .diz, .exe, .dll, .so, .doc, .pdf, .html, .xml), **When** l'utilisateur lance un scan, **Then** ces fichiers sont automatiquement exclus du scan et n'apparaissent pas dans les résultats
3. **Given** un utilisateur consulte la liste d'exclusions par défaut, **When** il accède aux paramètres de scan, **Then** il voit la liste complète des extensions exclues par défaut et peut ajouter ou retirer des extensions de cette liste
4. **Given** un utilisateur configure des filtres d'exclusion personnalisés (ex: exclure .mp3, .avi, ou fichiers > 2GB), **When** il lance un scan, **Then** les fichiers correspondant aux filtres personnalisés sont exclus en plus des exclusions par défaut (ou de la liste modifiée)
5. **Given** un répertoire contenant des archives ZIP avec des fichiers à l'intérieur, **When** l'utilisateur lance un scan, **Then** le contenu des archives ZIP est extrait et scanné, et les fichiers correspondant aux bases de données sont identifiés comme ROMs
6. **Given** un répertoire contenant des archives 7Z avec des fichiers à l'intérieur, **When** l'utilisateur lance un scan, **Then** le contenu des archives 7Z est extrait et scanné, et les fichiers correspondant aux bases de données sont identifiés comme ROMs
7. **Given** un répertoire contenant des archives RAR avec des fichiers à l'intérieur, **When** l'utilisateur lance un scan, **Then** le contenu des archives RAR est extrait et scanné, et les fichiers correspondant aux bases de données sont identifiés comme ROMs
8. **Given** un répertoire contenant des archives (ZIP, 7Z, ou RAR) qui contiennent elles-mêmes des archives avec des fichiers, **When** l'utilisateur lance un scan, **Then** les fichiers dans les archives imbriquées sont extraits et scannés récursivement, et ceux correspondant aux bases de données sont identifiés comme ROMs
9. **Given** un répertoire mixte contenant des fichiers de différentes consoles potentielles (sans pré-classement), **When** l'utilisateur lance un scan, **Then** chaque fichier scanné a ses checksums calculés et est automatiquement identifié avec sa console d'origine si correspondance trouvée dans les bases de données
10. **Given** un fichier qui correspond à une entrée dans une base de données (NoIntro, Redump, ou GoodSet), **When** l'utilisateur lance un scan, **Then** ce fichier est identifié comme ROM avec le nom du jeu et les métadonnées de la base de données en utilisant les checksums calculés pour faire la correspondance
11. **Given** un fichier qui ne correspond à aucune entrée dans les bases de données, **When** l'utilisateur lance un scan, **Then** ce fichier apparaît dans la liste des fichiers scannés comme "non identifié" avec ses checksums calculés mais sans association à un jeu
12. **Given** un répertoire contenant des fichiers, **When** l'utilisateur lance un scan, **Then** tous les types de checksums/hash requis (MD5, SHA1, SHA256, CRC32, etc.) sont calculés pour chaque fichier scanné (hors exclusions) et utilisés pour tenter d'identifier les jeux et versions via correspondance avec les bases de données
13. **Given** un fichier qui peut être identifié dans plusieurs bases de données (par exemple, présent à la fois dans Redump et GoodSet), **When** le système identifie le fichier, **Then** le fichier est associé à toutes les sources de bases de données pertinentes, et la source à utiliser est sélectionnée selon les préférences utilisateur (région, langue, etc.)
14. **Given** un scan en cours avec des fichiers traités et certains fichiers échoués, **When** l'utilisateur consulte l'interface de scan, **Then** l'utilisateur voit une liste détaillée de tous les fichiers traités avec leur statut : "Identifié comme ROM" (console et jeu), "Non identifié" (checksum calculé mais pas de match), "Exclu" (filtres), ou "Échec" avec raison explicite (ex: "Archive corrompue", "Fichier illisible", "Erreur de checksum")
15. **Given** un répertoire déjà scanné précédemment, **When** l'utilisateur lance un nouveau scan, **Then** il peut choisir entre un "scan rapide" (détection basée sur timestamp et taille, recalcul des checksums uniquement pour fichiers modifiés) ou un "scan complet" (recalcul de tous les checksums)
16. **Given** un scan rapide sur un répertoire déjà scanné, **When** un fichier a le même timestamp et la même taille, **Then** le système réutilise les checksums et identifications précédentes sans recalcul
17. **Given** un scan rapide sur un répertoire déjà scanné, **When** un fichier a un timestamp ou une taille différente, **Then** le système recalcule les checksums et réidentifie le fichier
18. **Given** un scan sur un répertoire déjà scanné, **When** des fichiers ont été supprimés depuis le dernier scan, **Then** ces fichiers sont retirés de la base de données de l'application
19. **Given** un scan sur un répertoire déjà scanné, **When** de nouveaux fichiers ont été ajoutés depuis le dernier scan, **Then** ces nouveaux fichiers sont scannés et ajoutés à la base de données

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

- Que se passe-t-il si un fichier est corrompu ou illisible pendant le scan ? → Le fichier doit apparaître dans la liste des fichiers échoués avec la raison "Fichier corrompu" ou "Fichier illisible", et le scan continue avec les autres fichiers
- Comment le système gère-t-il les fichiers qui ne correspondent à aucune entrée dans les bases de données (NoIntro, Redump, GoodSet) ? → Ces fichiers apparaissent dans la liste comme "non identifiés" avec leurs checksums calculés, permettant à l'utilisateur de les consulter (ils pourraient être des ROMs de systèmes non couverts par les bases de données, ou des fichiers non-ROM qui ont passé les filtres)
- Que se passe-t-il si un utilisateur veut scanner un fichier qui serait normalement exclu par les filtres par défaut ? → L'utilisateur peut modifier les filtres d'exclusion avant le scan ou désactiver temporairement certains filtres pour inclure ces fichiers
- Que se passe-t-il si plusieurs bases de données contiennent des informations contradictoires pour le même fichier identifié ?
- Comment le système gère-t-il les archives très volumineuses ou profondément imbriquées (risque de dépassement de mémoire) pour les formats ZIP, 7Z et RAR ?
- Que se passe-t-il si un export échoue partiellement (certains fichiers copiés, d'autres non) ?
- Comment le système gère-t-il les caractères spéciaux dans les noms de fichiers lors de l'export vers différentes plateformes ?
- Que se passe-t-il si les préférences de région/langue ne permettent pas de sélectionner une version unique (toutes les versions ont la même priorité) ?
- Comment le système gère-t-il les ROMs qui appartiennent à plusieurs jeux ou sont des compilations ?
- Que se passe-t-il si la connexion à l'API Romm échoue pendant une synchronisation ?
- Comment le système gère-t-il les fichiers gamelist.xml malformés ou incomplets dans Recalbox ?

## Exigences *(obligatoire)*

### Exigences Fonctionnelles

- **FR-001**: Le système DOIT scanner les répertoires de manière récursive (tous les sous-répertoires) pour trouver tous les fichiers (sans présupposer lesquels sont des ROMs), et DOIT traiter récursivement les archives (ZIP, 7Z, RAR) incluant les archives imbriquées (archives dans d'autres archives) à n'importe quelle profondeur
- **FR-001.1**: Le système DOIT exclure par défaut les fichiers avec les extensions suivantes : .jpg, .jpeg, .png, .gif, .bmp (images), .txt, .nfo, .diz (fichiers texte), .exe, .dll, .so (exécutables), .doc, .pdf, .html, .xml (documents)
- **FR-001.2**: Le système DOIT permettre aux utilisateurs de configurer des filtres d'exclusion personnalisés par extension de fichier, plage de taille ou motifs de nom de fichier avant le scan
- **FR-001.3**: Le système DOIT permettre aux utilisateurs de consulter et modifier la liste d'exclusion par défaut (ajouter ou retirer des extensions)
- **FR-002**: Le système DOIT calculer tous les types de checksums/hash utilisés par Romm, Recalbox, Redump, NoIntro et autres bases de données de référence (MD5, SHA1, SHA256, CRC32, et tout autre type de hash requis par ces plateformes) pour chaque fichier scanné (excluant les fichiers filtrés) pendant le scan
- **FR-003**: Le système DOIT utiliser les checksums/hash pour identifier quels fichiers scannés sont des ROMs en les comparant aux entrées des bases de données de référence, et identifier leurs jeux et versions
- **FR-003.1**: Le système DOIT lister les fichiers qui ne correspondent à aucune entrée de base de données comme "fichiers non identifiés" avec leurs checksums calculés, sans présupposer qu'ils ne sont pas des ROMs
- **FR-004**: Le système DOIT identifier automatiquement la console/plateforme pour chaque fichier qui correspond à une entrée de base de données, sans nécessiter de pré-classification manuelle
- **FR-005**: Le système DOIT identifier les jeux en utilisant les bases de données de référence (NoIntro, Redump, GoodSet) lorsque disponibles, et DOIT supporter l'association de chaque jeu avec plusieurs sources de bases de données
- **FR-006**: Le système DOIT sélectionner quelle source de base de données utiliser pour chaque jeu en fonction des préférences configurées par l'utilisateur (région, langue, etc.) lorsque plusieurs sources sont disponibles
- **FR-006.1**: Le système DOIT fournir une interface de gestion de bases de données intégrée pour parcourir, sélectionner et télécharger les bases de données de référence (NoIntro, Redump, GoodSet)
- **FR-006.2**: Le système DOIT permettre aux utilisateurs de voir les providers de bases de données disponibles, les consoles et les versions dans une interface dédiée avec capacités de filtrage
- **FR-006.3**: Le système DOIT supporter le téléchargement de plusieurs versions d'une même base de données pour une seule console
- **FR-006.4**: Le système DOIT afficher la progression et le statut du téléchargement pour les bases de données
- **FR-006.5**: Le système DOIT permettre aux utilisateurs de sélectionner quelle version de base de données utiliser par défaut lorsque plusieurs versions existent pour la même console
- **FR-006.6**: Le système DOIT afficher toutes les bases de données téléchargées dans une vue tableau avec filtres par provider, console et version
- **FR-006.7**: Le système DOIT notifier les utilisateurs lorsque des mises à jour des bases de données téléchargées sont disponibles
- **FR-006.8**: Le système DOIT gérer les échecs de téléchargement avec élégance avec des messages d'erreur clairs et une capacité de réessai
- **FR-007**: Le système DOIT regrouper ensemble plusieurs versions d'un même jeu
- **FR-008**: Le système DOIT permettre aux utilisateurs de configurer les préférences de priorité pour la sélection de région (ex : EUR > USA > JAP)
- **FR-009**: Le système DOIT permettre aux utilisateurs de configurer les préférences de priorité pour la sélection de langue
- **FR-009.1**: Le système DOIT permettre aux utilisateurs de configurer les préférences de priorité pour la sélection de format vidéo (PAL, NTSC, NTSC-J) comme une liste de préférence séparée de la région
- **FR-009.2**: Le système DOIT appliquer à la fois les préférences de région et de format vidéo lors de la sélection de la meilleure version de ROM (les deux critères évalués avec priorité configurable)
- **FR-010**: Le système DOIT automatiquement exclure les versions de ROM marquées comme "bad dump" ou avec des problèmes de qualité de la sélection automatique
- **FR-011**: Le système DOIT automatiquement sélectionner la meilleure version de ROM pour chaque jeu basée sur les préférences configurées de région, format vidéo et langue, incluant la sélection de la source de base de données appropriée lorsque plusieurs sources sont disponibles
- **FR-012**: Le système DOIT exporter les ROMs vers Recalbox en suivant les conventions de nommage de dossiers Recalbox pour les consoles
- **FR-013**: Le système DOIT exporter les ROMs vers Romm en suivant les conventions de nommage de dossiers Romm pour les consoles
- **FR-014**: Le système DOIT respecter les exigences de format pour chaque plateforme (ZIP vs décompressé, formats de fichiers supportés)
- **FR-014.1**: Le système DOIT supporter l'export vers des répertoires locaux (système de fichiers local ou lecteurs réseau montés)
- **FR-014.2**: Le système DOIT supporter l'export vers des serveurs distants via protocole SSH/SFTP
- **FR-014.3**: Le système DOIT fournir une interface pour configurer les paramètres de connexion SSH/SFTP (hôte, port, nom d'utilisateur, mot de passe/clé, répertoire cible)
- **FR-014.4**: Le système DOIT permettre aux utilisateurs de tester la connexion SSH/SFTP avant de démarrer l'export
- **FR-014.5**: Le système DOIT afficher la progression du transfert pour les exports distants (nombre de fichiers transférés, vitesse de transfert, temps restant estimé)
- **FR-014.6**: Le système DOIT gérer les échecs de connexion SSH/SFTP avec élégance avec des messages d'erreur clairs et une capacité de réessai
- **FR-015**: Le système DOIT récupérer les métadonnées de scrap (descriptions, images, notes) depuis Recalbox via les fichiers gamelist.xml
- **FR-016**: Le système DOIT récupérer les métadonnées de scrap depuis Romm via l'API REST
- **FR-017**: Le système DOIT synchroniser les données utilisateur (favoris, notes, statistiques de jeu) de manière bidirectionnelle avec Recalbox via gamelist.xml
- **FR-018**: Le système DOIT synchroniser les données utilisateur de manière bidirectionnelle avec Romm via l'API REST
- **FR-019**: Le système DOIT gérer les conflits lorsque les métadonnées diffèrent entre l'application et les plateformes (dernière modification gagne, ou confirmation utilisateur)
- **FR-020**: Le système DOIT supporter le scan depuis plusieurs répertoires sources
- **FR-021**: Le système DOIT préserver l'intégrité des fichiers pendant les opérations d'export
- **FR-022**: Le système DOIT fournir un retour détaillé de progression pendant les opérations de scan, incluant une liste de tous les fichiers traités (avec statut : succès, console identifiée, jeu identifié) et tous les fichiers en échec avec les raisons explicites d'échec (ex : "Console non détectée", "Archive corrompue", "Fichier illisible", "Erreur de checksum")
- **FR-023**: Le système DOIT gérer les erreurs avec élégance et fournir des messages d'erreur significatifs aux utilisateurs, incluant les raisons spécifiques pour lesquelles des fichiers individuels n'ont pas pu être traités pendant le scan
- **FR-024**: Le système DOIT offrir aux utilisateurs un choix entre "scan rapide" et "scan complet" lors du rescan d'un répertoire précédemment scanné
- **FR-025**: En mode scan rapide, le système DOIT détecter les changements de fichiers en comparant timestamp et taille de fichier, et DOIT recalculer les checksums uniquement pour les fichiers qui ont changé
- **FR-026**: En mode scan rapide, le système DOIT réutiliser les checksums et identifications précédemment calculés pour les fichiers avec timestamp et taille inchangés
- **FR-027**: En mode scan complet, le système DOIT recalculer tous les checksums pour tous les fichiers indépendamment des changements de timestamp ou taille
- **FR-028**: Pendant les opérations de rescan (rapide ou complet), le système DOIT retirer de la base de données tous les fichiers qui n'existent plus dans le répertoire scanné
- **FR-029**: Pendant les opérations de rescan (rapide ou complet), le système DOIT ajouter les fichiers nouvellement découverts à la base de données
- **FR-030**: Le système DOIT fournir une interface Bibliothèque dédiée pour visualiser la collection de jeux
- **FR-031**: Le système DOIT supporter le mode d'affichage grille montrant les jeux avec vignettes de cover art
- **FR-032**: Le système DOIT supporter le mode d'affichage liste montrant les jeux dans un tableau avec colonnes triables (nom, console, région, format, statut, taille)
- **FR-033**: Le système DOIT permettre aux utilisateurs de basculer entre les modes d'affichage grille et liste
- **FR-034**: Le système DOIT fournir des capacités de filtrage par console, région, langue, format vidéo, statut et autres attributs de jeu
- **FR-035**: Le système DOIT supporter les filtres combinés (plusieurs critères appliqués simultanément)
- **FR-036**: Le système DOIT fournir une barre de recherche avec filtrage en temps réel pendant que l'utilisateur tape
- **FR-037**: Le système DOIT permettre le tri des jeux par nom, date de sortie, note, console et autres attributs
- **FR-038**: Le système DOIT afficher les métadonnées complètes pour les jeux sélectionnés incluant description, images, captures d'écran, vidéos, note, développeur, éditeur, date de sortie
- **FR-039**: Le système DOIT permettre aux utilisateurs de voir et gérer toutes les versions disponibles d'un jeu lorsque plusieurs versions existent
- **FR-040**: Le système DOIT supporter la lecture vidéo in-app pour les vidéos de prévisualisation de jeux
- **FR-041**: Le système DOIT indiquer visuellement les jeux avec métadonnées incomplètes ou manquantes
- **FR-042**: Le système DOIT persister toutes les données de fichiers scannés, checksums, identifications, métadonnées et préférences utilisateur dans une base de données SQLite locale
- **FR-043**: Le système DOIT utiliser des requêtes SQL pour filtrer, rechercher et récupérer les données de la collection
- **FR-044**: Le système DOIT maintenir l'intégrité de la base de données et gérer l'accès concurrent de manière appropriée
- **FR-045**: Le système DOIT fournir une fonctionnalité de sauvegarde et restauration pour la base de données SQLite

### Entités Clés *(inclure si la fonctionnalité implique des données)*

**Note** : Toutes les entités ci-dessous sont persistées dans une base de données SQLite locale pour des requêtes, filtrages et gestion de données efficaces.

- **Fichier Scanné** : Représente un fichier unique trouvé pendant le scan (peut être ou ne pas être une ROM jusqu'à identification). Attributs : chemin du fichier, taille du fichier, timestamp de dernière modification, emplacement dans archive (si dans archive ZIP/7Z/RAR), statut d'identification (identifié comme ROM / non identifié / exclu / échec), console/plateforme détectée (seulement si identifié comme ROM via correspondance base de données), identification du jeu (seulement si correspondance dans base de données via checksums), informations de version (région, format vidéo [PAL/NTSC/NTSC-J], langue, drapeaux de qualité - seulement si identifié), checksums/hashs (MD5, SHA1, SHA256, CRC32, et tout autre type de hash requis par Romm, Recalbox, Redump, NoIntro et autres bases de données de référence - calculés pour tous les fichiers scannés pour tenter l'identification), statut de traitement (succès, échec), raison d'échec (si traitement échoué : ex. "Archive corrompue", "Fichier illisible", "Erreur de calcul de checksum", "Échec d'extraction d'archive"), raison d'exclusion (si exclu : ex. "Filtre d'exclusion par défaut", "Filtre d'exclusion défini par l'utilisateur"), date du dernier scan, type de scan (rapide/complet)
- **Jeu** : Représente un jeu logique qui peut avoir plusieurs versions de ROM (fichiers identifiés comme ROMs). Attributs : nom du jeu, console/plateforme, sources de bases de données associées (peut être associé à plusieurs sources : NoIntro, Redump, GoodSet, etc.), source de base de données sélectionnée (choisie en fonction des préférences utilisateur lorsque plusieurs sources disponibles), fichiers ROM identifiés regroupés, version sélectionnée (meilleure correspondance basée sur les préférences), métadonnées (description, images, notes)
- **Console/Plateforme** : Représente une console de jeu ou plateforme. Attributs : nom de la plateforme, conventions de nommage des dossiers pour Recalbox, conventions de nommage des dossiers pour Romm, formats de fichiers supportés, exigences d'export
- **Base de Données de Référence** : Représente un fichier de base de données de référence téléchargé (NoIntro, Redump, GoodSet). Attributs : nom du provider (NoIntro/Redump/GoodSet), console/plateforme, identifiant de version, date de sortie, taille du fichier, statut de téléchargement, chemin du fichier, is_default (booléen indiquant si cette version est celle par défaut pour cette console), timestamp de dernière mise à jour, drapeau de mise à jour disponible
- **Préférences Utilisateur** : Représente la configuration utilisateur pour le filtrage et la sélection. Attributs : ordre de priorité de région (ex : EUR > USA > JAP), ordre de priorité de format vidéo (ex : PAL > NTSC > NTSC-J - séparé des préférences de région), ordre de priorité de langue (ex : FR > EN > autres), filtres de qualité (exclure les bad dumps), paramètres d'export par plateforme
- **Métadonnées** : Représente les métadonnées de jeu depuis diverses sources. Attributs : description, images de couverture, captures d'écran, vidéos, notes, date de sortie, développeur, éditeur, genre, données utilisateur (favoris, statistiques de jeu, save states), source (scrappées depuis Recalbox/Romm ou saisies par l'utilisateur), timestamp de dernière modification
- **Configuration d'Export** : Représente les paramètres pour exporter vers une plateforme. Attributs : plateforme cible (Recalbox/Romm), type d'export (local/distant), répertoire de destination (pour exports locaux), paramètres de connexion SSH/SFTP (hôte, port, nom d'utilisateur, méthode d'authentification, chemin mot de passe/clé, répertoire distant - pour exports distants), exigences de format (ZIP/décompressé), stratégie de résolution de conflits, statut de test de connexion
- **Configuration de Synchronisation** : Représente les paramètres pour la synchronisation des métadonnées. Attributs : plateforme source, direction de synchronisation (bidirectionnelle/import seulement/export seulement), stratégie de résolution de conflits, portée de synchronisation (quels types de métadonnées synchroniser)

## Critères de Succès *(obligatoire)*

### Résultats Mesurables

- **SC-001** : Les utilisateurs peuvent scanner un répertoire contenant 1000+ fichiers (incluant les archives) et avoir les checksums calculés et l'identification tentée pour tous les fichiers (excluant les filtres) en moins de 5 minutes sur un ordinateur de bureau standard
- **SC-002** : Le système identifie correctement la console/plateforme pour 95%+ des fichiers qui correspondent aux entrées de base de données (identifiés comme ROMs) sans intervention manuelle
- **SC-003** : Le système regroupe avec succès les versions d'un même jeu avec 99%+ de précision lorsque plusieurs versions existent dans la collection
- **SC-004** : Le système sélectionne automatiquement la version préférée (basée sur les préférences utilisateur) pour 90%+ des jeux avec plusieurs versions
- **SC-005** : Les utilisateurs peuvent exporter une collection de 500 jeux vers Recalbox ou Romm en moins de 2 minutes
- **SC-006** : Les ROMs exportées sont correctement placées dans les structures de dossiers spécifiques aux plateformes avec 100% de précision (tous les fichiers dans les bons dossiers de consoles)
- **SC-007** : La synchronisation des métadonnées se termine avec succès pour 95%+ des jeux lorsque les données sources sont disponibles
- **SC-008** : Les utilisateurs peuvent compléter un workflow complet (scan → filtrer → exporter → synchroniser métadonnées) pour une nouvelle collection en moins de 10 minutes
- **SC-009** : Le système gère les archives imbriquées jusqu'à 5 niveaux de profondeur sans dégradation de performance ou erreurs
- **SC-010** : L'application fonctionne sur Windows, Linux et macOS avec une fonctionnalité cohérente sur toutes les plateformes
- **SC-011** : L'interface Bibliothèque peut afficher et filtrer une collection de 5000+ jeux sans dégradation de performance
- **SC-012** : Les utilisateurs peuvent basculer entre les vues grille et liste avec moins de 500ms de temps de transition
- **SC-013** : Le filtrage de recherche en temps réel retourne les résultats en moins de 100ms pour des collections jusqu'à 10 000 jeux
- **SC-014** : La couverture de tests doit atteindre un minimum de 80% pour les tests unitaires sur l'ensemble du code
- **SC-015** : Toutes les user stories critiques (Priorité P1, P1.5) ont des tests d'intégration complets validant les scénarios de bout en bout

## Attributs de Qualité *(obligatoire)*

### Tests et Assurance Qualité

- **QA-001** : Le développement DOIT suivre les pratiques de Test-Driven Development (TDD) où les tests sont écrits avant l'implémentation
- **QA-002** : Le code DOIT maintenir une couverture minimale de 80% de tests unitaires mesurée par couverture de lignes/branches
- **QA-003** : Toutes les user stories critiques (scan, gestion de base de données, regroupement/filtrage, export, sync, bibliothèque) DOIVENT avoir des tests d'intégration validant les workflows complets
- **QA-004** : Les tests unitaires DOIVENT couvrir toute la logique métier, les transformations de données et les scénarios de gestion d'erreurs
- **QA-005** : Les tests d'intégration DOIVENT valider l'interaction entre les composants et les systèmes externes (système de fichiers, bases de données, SSH/SFTP)
- **QA-006** : La suite de tests DOIT s'exécuter automatiquement à chaque changement de code (intégration continue)
- **QA-007** : Tous les tests DOIVENT être maintenables, lisibles et suivre des conventions de nommage cohérentes
- **QA-008** : Les tests de performance DOIVENT valider les métriques des critères de succès (SC-001 à SC-013)

### Qualité du Code et Standards

- **QA-009** : Le code DOIT utiliser un linter automatisé configuré pour appliquer les standards de codage et bonnes pratiques pour le langage/framework choisi
- **QA-010** : Le code DOIT utiliser un formateur de code automatisé pour assurer un style de code cohérent sur tout le projet
- **QA-011** : Le linter et le formateur DOIVENT s'exécuter automatiquement avant chaque commit (pre-commit hooks) ou dans le pipeline CI/CD
- **QA-012** : Le code DOIT passer toutes les vérifications du linter sans avertissements ni erreurs avant d'être fusionné
- **QA-013** : La configuration du linter DOIT appliquer les bonnes pratiques spécifiques au langage (ex : ESLint pour JavaScript/TypeScript, Pylint/Flake8 pour Python, RuboCop pour Ruby, Clippy pour Rust)
- **QA-014** : Le code DOIT suivre des conventions de nommage cohérentes pour les variables, fonctions, classes et fichiers tel qu'imposé par le linter
- **QA-015** : La logique complexe DOIT être documentée avec des commentaires inline expliquant le "pourquoi" et pas seulement le "quoi"
