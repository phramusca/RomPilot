# Contrat: Recalbox gamelist.xml Format

**Date**: 2025-12-12  
**Feature**: [spec.md](../spec.md)  
**Purpose**: Format XML utilisé par Recalbox (EmulationStation) pour stocker les métadonnées et données utilisateur des jeux

## Vue d'Ensemble

Recalbox utilise le format `gamelist.xml` hérité d'EmulationStation pour stocker :
- Métadonnées de jeux (nom, description, développeur, éditeur, etc.)
- Chemins vers médias (images, vidéos)
- Données utilisateur (favoris, compteur de jeu, dernière partie, notes)

Il existe un fichier `gamelist.xml` par console dans le répertoire des ROMs :
```
/recalbox/share/roms/{console}/gamelist.xml
```

## Structure XML

### Schéma Global

```xml
<?xml version="1.0"?>
<gameList>
    <folder>
        <!-- Dossiers optionnels -->
    </folder>
    <game>
        <!-- Entrée jeu (une par ROM) -->
    </game>
    <!-- ... autres jeux ... -->
</gameList>
```

### Élément `<game>` Complet

```xml
<game>
    <!-- Identification (REQUIS) -->
    <path>./rom-file.zip</path>              <!-- Chemin relatif au fichier ROM -->
    
    <!-- Métadonnées de base -->
    <name>Game Name</name>                   <!-- Nom affiché -->
    <desc>Game description here...</desc>     <!-- Description complète -->
    
    <!-- Métadonnées détaillées -->
    <developer>Developer Name</developer>
    <publisher>Publisher Name</publisher>
    <genre>Action</genre>
    <players>1-2</players>                    <!-- Nombre de joueurs -->
    <rating>0.85</rating>                     <!-- Note 0.0 à 1.0 -->
    <releasedate>19900101T000000</releasedate> <!-- Format: YYYYMMDDThhmmss -->
    
    <!-- Médias (chemins relatifs au dossier console) -->
    <image>./images/game-image.png</image>
    <thumbnail>./images/game-thumb.png</thumbnail>
    <marquee>./images/game-marquee.png</marquee>
    <video>./videos/game-video.mp4</video>
    
    <!-- Données utilisateur -->
    <playcount>5</playcount>                  <!-- Nombre de fois joué -->
    <lastplayed>20240115T120000</lastplayed>  <!-- Dernière partie (format date) -->
    <favorite>true</favorite>                 <!-- Favori (true/false) -->
    <kidgame>false</kidgame>                  <!-- Jeu enfant (true/false) -->
    <hidden>false</hidden>                    <!-- Caché (true/false) -->
    
    <!-- Métadonnées additionnelles (optionnelles) -->
    <region>Europe</region>
    <lang>en</lang>
    <hash>md5hash...</hash>                   <!-- Hash MD5 du fichier -->
</game>
```

### Élément `<folder>` (Optionnel)

```xml
<folder>
    <path>./subdirectory</path>
    <name>Folder Name</name>
    <desc>Folder description</desc>
    <image>./images/folder-image.png</image>
    <thumbnail>./images/folder-thumb.png</thumbnail>
</folder>
```

## Spécification des Champs

### Champs Obligatoires

| Champ | Type | Description |
|-------|------|-------------|
| `path` | String | Chemin relatif au fichier ROM (commence par `./`) |

### Champs Métadonnées

| Champ | Type | Format | Description |
|-------|------|--------|-------------|
| `name` | String | - | Nom du jeu affiché dans l'interface |
| `desc` | String | - | Description complète du jeu |
| `developer` | String | - | Nom du développeur |
| `publisher` | String | - | Nom de l'éditeur |
| `genre` | String | - | Genre du jeu |
| `players` | String | "1", "1-2", "1-4", etc. | Nombre de joueurs supportés |
| `rating` | Float | 0.0 - 1.0 | Note du jeu (0 = mauvais, 1 = excellent) |
| `releasedate` | DateTime | YYYYMMDDThhmmss | Date de sortie du jeu |
| `region` | String | - | Région du jeu (Europe, USA, Japan, etc.) |
| `lang` | String | Code ISO 639 | Langue du jeu (en, fr, ja, etc.) |
| `hash` | String | Hex | Hash MD5 du fichier ROM |

### Champs Médias

| Champ | Type | Format | Description |
|-------|------|--------|-------------|
| `image` | String | Chemin relatif | Image principale (boxart, screenshot) |
| `thumbnail` | String | Chemin relatif | Miniature (version réduite de l'image) |
| `marquee` | String | Chemin relatif | Image marquee/logo du jeu |
| `video` | String | Chemin relatif | Vidéo de prévisualisation |

**Note** : Tous les chemins de médias sont relatifs au répertoire de la console.

### Champs Données Utilisateur

| Champ | Type | Format | Description |
|-------|------|--------|-------------|
| `playcount` | Integer | ≥ 0 | Nombre de fois que le jeu a été lancé |
| `lastplayed` | DateTime | YYYYMMDDThhmmss | Date/heure de dernière partie |
| `favorite` | Boolean | "true"/"false" | Jeu marqué comme favori |
| `kidgame` | Boolean | "true"/"false" | Jeu adapté aux enfants |
| `hidden` | Boolean | "true"/"false" | Jeu caché de la liste |

## Exemples

### Exemple Complet

```xml
<?xml version="1.0"?>
<gameList>
    <game>
        <path>./Super Mario Bros. (Europe).zip</path>
        <name>Super Mario Bros.</name>
        <desc>A classic platformer where Mario must rescue Princess Peach from Bowser.</desc>
        <developer>Nintendo</developer>
        <publisher>Nintendo</publisher>
        <genre>Platform</genre>
        <players>1-2</players>
        <rating>0.95</rating>
        <releasedate>19850913T000000</releasedate>
        <region>Europe</region>
        <lang>en</lang>
        <image>./images/Super Mario Bros. (Europe)-image.png</image>
        <thumbnail>./images/Super Mario Bros. (Europe)-thumb.png</thumbnail>
        <video>./videos/Super Mario Bros. (Europe)-video.mp4</video>
        <playcount>42</playcount>
        <lastplayed>20250112T183000</lastplayed>
        <favorite>true</favorite>
        <kidgame>true</kidgame>
        <hidden>false</hidden>
        <hash>8e3630186e35d477231bf8fd50e54cdd</hash>
    </game>
</gameList>
```

### Exemple Minimal

```xml
<?xml version="1.0"?>
<gameList>
    <game>
        <path>./game.zip</path>
        <name>Game Name</name>
    </game>
</gameList>
```

## Lecture (Import/Synchronisation)

### Algorithme de Lecture

1. Parser le fichier XML avec `System.Xml.Linq`
2. Itérer sur chaque élément `<game>`
3. Extraire `path` pour identifier le fichier ROM correspondant
4. Lire tous les champs disponibles
5. Importer les métadonnées dans la base SQLite
6. Associer avec le `ScannedFile` correspondant via chemin

### Code C# Exemple

```csharp
using System.Xml.Linq;

public class RecalboxGamelistReader
{
    public IEnumerable<GamelistEntry> ReadGamelist(string xmlFilePath)
    {
        var doc = XDocument.Load(xmlFilePath);
        var games = doc.Root.Elements("game");
        
        foreach (var game in games)
        {
            yield return new GamelistEntry
            {
                Path = game.Element("path")?.Value,
                Name = game.Element("name")?.Value,
                Description = game.Element("desc")?.Value,
                Developer = game.Element("developer")?.Value,
                Publisher = game.Element("publisher")?.Value,
                Genre = game.Element("genre")?.Value,
                Rating = ParseRating(game.Element("rating")?.Value),
                ReleaseDate = ParseDate(game.Element("releasedate")?.Value),
                ImagePath = game.Element("image")?.Value,
                ThumbnailPath = game.Element("thumbnail")?.Value,
                VideoPath = game.Element("video")?.Value,
                PlayCount = ParseInt(game.Element("playcount")?.Value),
                LastPlayed = ParseDate(game.Element("lastplayed")?.Value),
                IsFavorite = ParseBool(game.Element("favorite")?.Value),
                IsHidden = ParseBool(game.Element("hidden")?.Value)
            };
        }
    }
    
    private float? ParseRating(string value) 
        => float.TryParse(value, out var result) ? result : null;
    
    private DateTime? ParseDate(string value)
        => DateTime.TryParseExact(value, "yyyyMMddTHHmmss", null, 
            DateTimeStyles.None, out var result) ? result : null;
    
    private int ParseInt(string value)
        => int.TryParse(value, out var result) ? result : 0;
    
    private bool ParseBool(string value)
        => value?.ToLower() == "true";
}
```

## Écriture (Export/Synchronisation)

### Algorithme d'Écriture

1. Récupérer tous les jeux pour la console depuis la base SQLite
2. Pour chaque jeu, récupérer métadonnées et données utilisateur
3. Créer structure XML avec `System.Xml.Linq`
4. Formater les chemins relatifs correctement
5. Formater les dates au format YYYYMMDDThhmmss
6. Écrire le fichier XML

### Code C# Exemple

```csharp
public class RecalboxGamelistWriter
{
    public void WriteGamelist(string xmlFilePath, IEnumerable<GamelistEntry> games)
    {
        var doc = new XDocument(
            new XDeclaration("1.0", null, null),
            new XElement("gameList",
                games.Select(game => new XElement("game",
                    new XElement("path", game.Path),
                    game.Name != null ? new XElement("name", game.Name) : null,
                    game.Description != null ? new XElement("desc", game.Description) : null,
                    game.Developer != null ? new XElement("developer", game.Developer) : null,
                    game.Publisher != null ? new XElement("publisher", game.Publisher) : null,
                    game.Genre != null ? new XElement("genre", game.Genre) : null,
                    game.Rating.HasValue ? new XElement("rating", game.Rating.Value.ToString("0.00")) : null,
                    game.ReleaseDate.HasValue ? new XElement("releasedate", 
                        game.ReleaseDate.Value.ToString("yyyyMMddTHHmmss")) : null,
                    game.ImagePath != null ? new XElement("image", game.ImagePath) : null,
                    game.ThumbnailPath != null ? new XElement("thumbnail", game.ThumbnailPath) : null,
                    game.VideoPath != null ? new XElement("video", game.VideoPath) : null,
                    game.PlayCount > 0 ? new XElement("playcount", game.PlayCount) : null,
                    game.LastPlayed.HasValue ? new XElement("lastplayed",
                        game.LastPlayed.Value.ToString("yyyyMMddTHHmmss")) : null,
                    new XElement("favorite", game.IsFavorite ? "true" : "false"),
                    new XElement("hidden", game.IsHidden ? "true" : "false")
                ))
            )
        );
        
        doc.Save(xmlFilePath);
    }
}
```

## Synchronisation Bidirectionnelle

### Import depuis Recalbox vers RomPilot

1. Lire gamelist.xml pour chaque console
2. Pour chaque entrée :
   - Trouver le `ScannedFile` correspondant via `path`
   - Créer/mettre à jour `Metadata` avec source "Recalbox"
   - Créer/mettre à jour `UserData` (favoris, playcount, etc.)
   - Conserver timestamp `LastModifiedAt`

### Export depuis RomPilot vers Recalbox

1. Pour chaque console, récupérer jeux exportés
2. Générer entrées `<game>` avec métadonnées RomPilot
3. Fusionner avec gamelist.xml existant si présent
4. Écrire nouveau gamelist.xml

### Résolution de Conflits

**Stratégies** (configurable par utilisateur) :
- **Dernière modification gagne** : Comparer timestamps, garder données les plus récentes
- **Recalbox prioritaire** : Toujours importer depuis Recalbox, écraser données RomPilot
- **RomPilot prioritaire** : Toujours exporter vers Recalbox, écraser gamelist.xml
- **Demander à l'utilisateur** : Afficher dialogue de résolution de conflits

## Références

- Documentation Recalbox : https://wiki.recalbox.com/
- Format EmulationStation : https://github.com/RetroPie/EmulationStation/blob/master/GAMELISTS.md
- RomManager existant : https://github.com/phramusca/RomManager (implémentation de référence)

## Notes d'Implémentation

### Gestion des Chemins

- Tous les chemins dans gamelist.xml sont **relatifs** au répertoire de la console
- Préfixe obligatoire : `./`
- Exemple : Si ROM dans `/recalbox/share/roms/nes/game.zip`, path = `./game.zip`

### Encodage

- Fichiers XML en UTF-8
- Échapper caractères spéciaux XML (&, <, >, ", ')
- Utiliser `XmlWriter` avec paramètres appropriés

### Performance

- Parser gamelist.xml peut être coûteux pour consoles avec milliers de jeux
- Considérer cache en mémoire
- Synchronisation incrémentale : comparer timestamps avant import/export complet

### Compatibilité

- Tester avec différentes versions de Recalbox
- Certains champs peuvent varier selon version EmulationStation
- Supporter à la fois lecture de formats anciens et écriture de format actuel

