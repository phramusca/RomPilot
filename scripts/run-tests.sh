#!/bin/bash
# Script pour lancer les tests avec différentes options

set -e

# Couleurs pour l'output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}🧪 Lancement des tests RomPilot${NC}"
echo ""

# Parse arguments
COVERAGE=false
FILTER=""
VERBOSE=false

while [[ $# -gt 0 ]]; do
    case $1 in
        --coverage|-c)
            COVERAGE=true
            shift
            ;;
        --filter|-f)
            FILTER="$2"
            shift 2
            ;;
        --verbose|-v)
            VERBOSE=true
            shift
            ;;
        --help|-h)
            echo "Usage: $0 [OPTIONS]"
            echo ""
            echo "Options:"
            echo "  -c, --coverage    Générer le rapport de couverture de code"
            echo "  -f, --filter      Filtrer les tests (ex: --filter ScanService)"
            echo "  -v, --verbose     Mode verbeux"
            echo "  -h, --help        Afficher cette aide"
            exit 0
            ;;
        *)
            echo -e "${RED}Option inconnue: $1${NC}"
            exit 1
            ;;
    esac
done

# Construction des arguments
ARGS=("test" "src/RomPilot.Tests/RomPilot.Tests.csproj")

if [ "$VERBOSE" = true ]; then
    ARGS+=("--logger" "console;verbosity=detailed")
else
    ARGS+=("--logger" "console;verbosity=normal")
fi

if [ -n "$FILTER" ]; then
    ARGS+=("--filter" "$FILTER")
    echo -e "${YELLOW}Filtre appliqué: $FILTER${NC}"
fi

if [ "$COVERAGE" = true ]; then
    ARGS+=("--collect:XPlat Code Coverage")
    echo -e "${YELLOW}Couverture de code activée${NC}"
fi

echo ""

# Lancement des tests
dotnet "${ARGS[@]}"

EXIT_CODE=$?

echo ""
if [ $EXIT_CODE -eq 0 ]; then
    echo -e "${GREEN}✅ Tous les tests ont réussi !${NC}"
else
    echo -e "${RED}❌ Certains tests ont échoué${NC}"
fi

# Si couverture activée, afficher où trouver le rapport
if [ "$COVERAGE" = true ] && [ $EXIT_CODE -eq 0 ]; then
    echo ""
    echo -e "${GREEN}📊 Rapport de couverture généré dans:${NC}"
    find src/RomPilot.Tests/TestResults -name "coverage.cobertura.xml" -type f 2>/dev/null | head -1
fi

exit $EXIT_CODE


