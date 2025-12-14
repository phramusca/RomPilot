#!/bin/bash
# Script amélioré de création de fichiers de test pour US1
# Génère 3 scénarios: simple, medium, load

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TEST_DATA_DIR="$SCRIPT_DIR"

echo "🧪 Création des fichiers de test US1..."
echo ""

# Vérifier que dotnet est disponible
if ! command -v dotnet &> /dev/null; then
    echo "❌ dotnet n'est pas disponible. Veuillez installer .NET SDK."
    exit 1
fi

# Compiler et exécuter le générateur
echo "📦 Compilation du générateur de test..."
cd "$SCRIPT_DIR"
dotnet run --project TestDataGenerator.csproj 2>/dev/null || {
    # Si le projet n'existe pas, créer un projet temporaire
    echo "Création d'un projet temporaire pour le générateur..."
    mkdir -p .temp-generator
    cd .temp-generator
    dotnet new console -n TestDataGenerator
    cp ../TestDataGenerator.cs Program.cs
    dotnet run
    cd ..
    rm -rf .temp-generator
}

echo ""
echo "✅ Fichiers de test créés dans $TEST_DATA_DIR"
echo ""
echo "📊 Scénarios disponibles:"
echo "  - simple/  : ~10 fichiers (scénario basique)"
echo "  - medium/  : ~50 fichiers avec archives imbriquées"
echo "  - load/    : ~500 fichiers pour tests de charge"
echo ""
echo "🚀 Utilisation:"
echo "  1. Tests automatiques: Les tests d'intégration utilisent ces fichiers"
echo "  2. Tests manuels: Scannez les dossiers dans l'application UI"
echo ""

