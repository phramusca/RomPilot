using System;
using System.IO;
using System.Threading.Tasks;
using RomPilot.Tests.Helpers;

namespace RomPilot.Tests.Helpers;

/// <summary>
/// Helper pour utiliser les fichiers de test générés dans les tests d'intégration
/// </summary>
public static class TestDataHelper
{
    private static string? _testDataRoot;
    private static bool _dataGenerated = false;
    private static readonly object _lockObject = new object();
    private static Task? _generationTask = null;

    /// <summary>
    /// Obtient le répertoire racine des fichiers de test
    /// </summary>
    public static string GetTestDataRoot()
    {
        if (_testDataRoot != null)
            return _testDataRoot;

        // Chercher le répertoire test-data depuis le répertoire du projet de tests
        var testProjectDir = Path.GetDirectoryName(typeof(TestDataHelper).Assembly.Location);
        var solutionRoot = FindSolutionRoot(testProjectDir!);
        _testDataRoot = Path.Combine(solutionRoot, "test-data");

        return _testDataRoot;
    }

    /// <summary>
    /// Génère les fichiers de test si nécessaire
    /// </summary>
    public static async Task EnsureTestDataGeneratedAsync()
    {
        // Double-check locking pattern pour thread-safety
        if (_dataGenerated)
            return;

        Task? taskToWait = null;
        lock (_lockObject)
        {
            if (_dataGenerated)
                return;

            // Si une génération est déjà en cours, attendre qu'elle se termine
            if (_generationTask != null)
            {
                taskToWait = _generationTask;
            }
            else
            {
                // Créer la tâche de génération
                _generationTask = EnsureTestDataGeneratedAsyncInternal();
                taskToWait = _generationTask;
            }
        }

        if (taskToWait != null)
        {
            await taskToWait;
        }

        // Marquer comme généré après l'attente
        lock (_lockObject)
        {
            _dataGenerated = true;
        }
    }

    private static async Task EnsureTestDataGeneratedAsyncInternal()
    {
        var root = GetTestDataRoot();
        var generator = new TestDataGenerator(root);

        // Vérifier si les scénarios existent déjà ET contiennent les fichiers attendus
        var simpleExists = Directory.Exists(Path.Combine(root, "simple"));
        var mediumExists = Directory.Exists(Path.Combine(root, "medium"));
        var loadExists = Directory.Exists(Path.Combine(root, "load"));

        // Vérifier si les fichiers 7Z et RAR sont présents (ils peuvent manquer si générés avec l'ancien générateur)
        var simpleHas7z = File.Exists(Path.Combine(root, "simple", "archives", "games.7z"));
        var simpleHasRar = File.Exists(Path.Combine(root, "simple", "archives", "games.rar"));
        var mediumHas7z = File.Exists(Path.Combine(root, "medium", "archive7z.7z"));
        var mediumHasRar = File.Exists(Path.Combine(root, "medium", "archiveRar.rar"));
        var mediumHasNested7z = File.Exists(Path.Combine(root, "medium", "nested7z.zip"));
        var mediumHasNestedRar = File.Exists(Path.Combine(root, "medium", "nestedRar.7z"));
        var mediumHasDeep = File.Exists(Path.Combine(root, "medium", "deep.rar"));

        // Régénérer si les répertoires n'existent pas OU si les fichiers 7Z/RAR manquent
        if (!simpleExists || !mediumExists || !loadExists ||
            !simpleHas7z || !simpleHasRar ||
            !mediumHas7z || !mediumHasRar || !mediumHasNested7z || !mediumHasNestedRar || !mediumHasDeep)
        {
            Console.WriteLine("📦 Génération des fichiers de test...");
            // Supprimer les répertoires existants pour forcer la régénération complète
            if (simpleExists) Directory.Delete(Path.Combine(root, "simple"), recursive: true);
            if (mediumExists) Directory.Delete(Path.Combine(root, "medium"), recursive: true);
            if (loadExists) Directory.Delete(Path.Combine(root, "load"), recursive: true);

            await generator.GenerateSimpleScenarioAsync();
            await generator.GenerateMediumScenarioAsync();
            await generator.GenerateLoadScenarioAsync();
        }
    }

    /// <summary>
    /// Obtient le chemin d'un scénario de test
    /// </summary>
    public static string GetScenarioPath(string scenarioName)
    {
        return Path.Combine(GetTestDataRoot(), scenarioName);
    }

    /// <summary>
    /// Trouve la racine de la solution en remontant depuis un répertoire
    /// </summary>
    private static string FindSolutionRoot(string startDir)
    {
        var dir = new DirectoryInfo(startDir);
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "RomPilot.sln")) ||
                Directory.Exists(Path.Combine(dir.FullName, "test-data")))
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }
        throw new DirectoryNotFoundException("Impossible de trouver la racine de la solution");
    }
}
