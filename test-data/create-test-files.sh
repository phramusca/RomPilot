#!/bin/bash
# Script de création de fichiers de test pour US1

set -e

echo "🧪 Création des fichiers de test US1..."

# Créer la structure
mkdir -p test-data/roms/archives
mkdir -p test-data/excluded
mkdir -p test-data/mixed

# ROMs NES (header NES = 0x4E 0x45 0x53 0x1A)
echo "📦 Création des ROMs de test..."
printf '\x4E\x45\x53\x1A' > test-data/roms/game1.nes
printf '\x4E\x45\x53\x1A\x00\x01' > test-data/roms/game2.nes

# Fichiers à exclure
echo "🚫 Création des fichiers à exclure..."
echo "Fake JPEG image" > test-data/excluded/cover.jpg
echo "This is a readme file" > test-data/excluded/readme.txt
echo "Fake PDF document" > test-data/excluded/manual.pdf
echo "NFO file content" > test-data/excluded/info.nfo

# Mix de fichiers
echo "🔀 Création du mix de fichiers..."
printf '\x4E\x45\x53\x1A' > test-data/mixed/game.nes
echo "Fake PNG screenshot" > test-data/mixed/screenshot.png
echo "Notes about the game" > test-data/mixed/notes.txt

# Archives (si zip disponible)
if command -v zip &> /dev/null; then
    echo "📦 Création des archives..."
    cd test-data/roms/archives
    printf '\x4E\x45\x53\x1A' > game3.nes
    printf '\x4E\x45\x53\x1A' > game4.nes
    zip -q games.zip game3.nes game4.nes
    rm game3.nes game4.nes
    cd ../../..
else
    echo "⚠️  zip non disponible, archives non créées"
fi

echo "✅ Fichiers de test créés dans test-data/"
echo ""
echo "📊 Résumé :"
echo "  - ROMs : $(find test-data/roms -name '*.nes' | wc -l) fichiers .nes"
echo "  - Exclus : $(find test-data/excluded -type f | wc -l) fichiers"
echo "  - Mix : $(find test-data/mixed -type f | wc -l) fichiers"
echo ""
echo "🚀 Lancez l'application et scannez les dossiers :"
echo "  - test-data/roms"
echo "  - test-data/excluded"
echo "  - test-data/mixed"

