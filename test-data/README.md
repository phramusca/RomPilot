# 🧪 Données de Test US1

Ce répertoire contient des fichiers de test pour valider l'US1 (Scanner et Identifier).

## 📋 Structure

```
test-data/
├── roms/                    # Fichiers ROM de test
│   ├── game1.nes           # ROM NES (4 octets - header NES)
│   ├── game2.nes           # ROM NES différente
│   └── archives/
│       ├── games.zip       # Archive avec ROMs
│       └── nested.7z       # Archive imbriquée
├── excluded/               # Fichiers qui devraient être exclus
│   ├── cover.jpg          # Image (filtre par défaut)
│   ├── readme.txt         # Texte (filtre par défaut)
│   ├── manual.pdf         # Document (filtre par défaut)
│   └── info.nfo           # NFO (filtre par défaut)
└── mixed/                 # Mix de fichiers
    ├── game.nes
    ├── screenshot.png
    └── notes.txt
```

## 🎯 Scénarios de Test

### Test 1 : Scan Basique
1. Scanner le dossier `roms/`
2. **Attendu** :
   - 2 fichiers scannés (game1.nes, game2.nes)
   - Statut "Unidentified" (pas de base de données)
   - Checksums calculés

### Test 2 : Filtres d'Exclusion
1. Scanner le dossier `excluded/`
2. **Attendu** :
   - 4 fichiers exclus (.jpg, .txt, .pdf, .nfo)
   - Compteur "Exclus" = 4
   - Raisons d'exclusion affichées

### Test 3 : Mix de Fichiers
1. Scanner le dossier `mixed/`
2. **Attendu** :
   - 1 scanné (game.nes)
   - 2 exclus (screenshot.png, notes.txt)
   - Compteurs corrects

### Test 4 : Scan Quick vs Full
1. Scanner `roms/` en mode Full
2. Noter le temps de scan
3. Rescanner en mode Quick
4. **Attendu** :
   - Mode Quick beaucoup plus rapide
   - Checksums réutilisés
   - Aucun recalcul

### Test 5 : Filtre Personnalisé
1. Scanner `mixed/`
2. Ajouter filtre personnalisé `.nes`
3. Rescanner
4. **Attendu** :
   - game.nes maintenant exclu
   - Compteur "Exclus" augmenté

## 🚀 Création des Fichiers de Test

```bash
cd /workspace
bash test-data/create-test-files.sh
```

## 🧹 Nettoyage

```bash
cd /workspace
rm -rf test-data/roms test-data/excluded test-data/mixed
```

