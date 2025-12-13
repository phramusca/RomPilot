#!/bin/bash
# Script pour lancer l'UI en mode debug (avec logs détaillés)

set -e

echo "🔨 Building project..."
dotnet build /workspace/src/RomPilot.UI/RomPilot.UI.csproj

echo ""
echo "🚀 Launching UI with detailed logging..."
echo "   Press Ctrl+C to stop"
echo ""

export DISPLAY=:0
export GDK_BACKEND=x11
export DOTNET_ENVIRONMENT=Development
export DOTNET_LOGGING__CONSOLE__DISABLECOLORS=false

cd /workspace
dotnet run --project src/RomPilot.UI/RomPilot.UI.csproj


