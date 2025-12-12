# Rom Pilot

## Composition de l'appli

### Partie Scan

- Scan récursif d'un répertoire et lecture des différents checksum (sha1, md5, ...) des fichiers, ainsi que lastmodified, taille et autres infos qui pourraient être utiles pour la suite.
- Tous les fichiers, y compris les archives, y compris dans les archives (7z, zip, rar) doivent être listés
- On ne peut pas faire de correspondances avec des jeux à ce stade. 
  - Il nous faudra les data de nointro, redump, ... pour la partie 3
  - on ne peut se baser sur l'extension, car pls consoles
- Affichage sous forme de tableau dans la UI
- Un nouveau scan sur le même répertoire doit:
  - mettre à jour seulement si le timetamp et la taille d'un fichier a changé
  - remettre à zéro les correspondances avec les data nointro, ..., si un checksum a changé
  - retirer de la base les fichiers manquants
  - ajouter les nouveaux éléments dans la base

### Partie Data

Infos: https://wiki.recalbox.com/fr/tutorials/games/generalities/isos-and-roms/differents-groups

- Lecture des data de nointro, redump, ...
  - Nointro: https://datomatic.no-intro.org/index.php?page=download&op=select&s=64
  - Redump: http://redump.org/downloads/
  - Goodset: voir RomManager
- On doit pouvoir naviguer et télécharger aisement les différentes data, et plusieurs versions pour chaque provider/console
- affichage sous forme de tableau, avec des filtres dans la UI

### Partie Correspondance

- Correspondance entre les fichiers scannés en partie 2 avec les data en partie 3, selon les règles générales


### Partie Options

- Faire une page pour les options.
- En autre, définir les préférences de l'utilisateur pour l'export et le choix des versions à choisir ou non pour chaque jeu. Ex:
  - FR > EU > US > Japan
  - PAL > NTSC
  - autre ?
- et les chemins 
- options ssh pour monter dossiers de config de recalbox et rommm

### Partie Bibliothèque

- Affichage des jeux comme dans RomManager avec
  - filtres 
  - liste des versions 
  - vidéos, thumbnails, ...
  - export vers Romm et Recalbox (en gérant les folders)
  - synchro des métadonnées avec romm et recalbox
  - ... ?

## Règles générales

### Code

- Faire des tests unitaires à chaque étape, en mode TDD. 
- Faire des tests fonctionnels à chaque étape, pour valider celle-ci
- Suivre les bonnes pratiques de codage
- RomManger: https://github.com/phramusca/RomManager

### Fonctionnel

- Chaque console/machine est indépendante en terme de liste de jeux
- Un jeu est entendu par console/machine
  - Meme si un jeu peut sortir sur plusieurs machines, on considèrera que ce sont des jeux diférents.
  - Il peut etre interessant de garder les infos de lien pour les jeux multi-consoles cela dit, si on les as, mais d'une autre manière, et qu'on exploitera peut etre dans une future mise à jour.
- Un jeu peut être lié à plusieurs roms, une version par region par ex, ou des version bad, des versions fix, ...
- Un jeu peut apparaitre chez pusieurs providers
- Chaque jeu aura donc une liste de versions. 
- Une version correpond donc à une version pour un provider donné
- Chaque provider a pour chaque console/machine plusieurs versions de ces fichiers.
  - La dernière est l'idéal à atteindre
  - Si on a une rom d'une version précédente, c'est déjà bien
  - Il faut pouvoir distinguer le niveau de "qualité" d'une rom:
    - nointro & recalbox > GoodSets
    - nointro et recalbox gèrents des médias différents, mais peut-etre que certaines consoles existent dans les 2
    - 