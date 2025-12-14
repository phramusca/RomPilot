using System;
using System.Threading.Tasks;
using RomPilot.TestData;

namespace RomPilot.TestData;

class Program
{
    static async Task Main(string[] args)
    {
        var baseDir = args.Length > 0 ? args[0] : AppContext.BaseDirectory;
        var generator = new TestDataGenerator(baseDir);

        Console.WriteLine("🧪 Génération des fichiers de test US1...\n");

        // Générer les 3 scénarios
        await generator.GenerateSimpleScenarioAsync();
        await generator.GenerateMediumScenarioAsync();
        await generator.GenerateLoadScenarioAsync();

        Console.WriteLine("\n✅ Tous les scénarios ont été générés avec succès!");
        Console.WriteLine("\n📊 Pour voir les hashs des fichiers, utilisez:");
        Console.WriteLine("   dotnet run --project test-data/TestDataGenerator.csproj -- --print-hashes");
    }
}

