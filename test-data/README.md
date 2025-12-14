# Fichiers de Test US1

Ce répertoire contient les fichiers de test générés pour tester la fonctionnalité de scan et d'identification (US1).

## Structure

- **simple/** : Scénario simple (~10 fichiers) - équivalent à "mixed" actuel
- **medium/** : Scénario moyen (~50 fichiers) avec archives imbriquées
- **load/** : Scénario de charge (~500 fichiers) pour tests de performance

## Génération

### Méthode 1: Script bash
```bash
./create-test-data.sh
```

### Méthode 2: Depuis les tests
Les tests d'intégration génèrent automatiquement les fichiers s'ils n'existent pas déjà.

### Méthode 3: Manuellement
```bash
cd test-data
dotnet run --project TestDataGenerator.csproj
```

## Utilisation

### Tests automatiques
Les tests d'intégration dans `src/RomPilot.Tests/Integration/` utilisent automatiquement ces fichiers via `TestDataHelper`.

### Tests manuels
1. Lancez l'application UI
2. Scannez les répertoires:
   - `test-data/simple/` pour un test rapide
   - `test-data/medium/` pour tester les archives imbriquées
   - `test-data/load/` pour tester les performances

## Hashs maîtrisés

Les fichiers sont générés avec des contenus maîtrisés pour avoir des hashs prévisibles:
- Les ROMs ont un header NES standard (0x4E 0x45 0x53 0x1A)
- Le contenu est basé sur un seed pour garantir la reproductibilité
- Les hashs MD5 et SHA1 peuvent être vérifiés dans la base de données après scan

## Archives imbriquées

Le générateur crée des archives imbriquées pour tester le chemin complet:
- **Formats simples** : ZIP, 7Z, RAR avec ROMs directement dedans
- **Archives imbriquées 2 niveaux** : 
  - ZIP dans ZIP
  - 7Z dans ZIP
  - RAR dans 7Z
- **Archives imbriquées 3 niveaux** : ZIP dans 7Z dans RAR (deep.rar)
- Le `FilePathInArchive` stocké contient le chemin complet: `level2.7z/level3.zip/game.gba`

## Vérification

Après un scan, vous pouvez vérifier dans la base de données:
- Les fichiers exclus ont `IdentificationStatus = "Excluded"`
- Les fichiers scannés ont des checksums calculés
- Les fichiers dans archives ont `FilePathInArchive` correctement rempli
- Les archives imbriquées ont `ArchiveDepth > 0`
