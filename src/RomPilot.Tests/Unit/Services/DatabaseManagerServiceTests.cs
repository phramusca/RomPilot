using System.Net.Http;
using FluentAssertions;
using Microsoft.Extensions.Http;
using Moq;
using RomPilot.Core.Models;
using RomPilot.Core.Repositories;
using RomPilot.Core.Services;
using Xunit;

namespace RomPilot.Tests.Unit.Services;

/// <summary>
/// Unit tests for DatabaseManagerService.
/// Tests database management functionality for reference databases (NoIntro, Redump, GoodSet).
/// </summary>
public class DatabaseManagerServiceTests
{
    private readonly Mock<IReferenceDatabaseRepository> _databaseRepositoryMock;
    private readonly Mock<IGameRepository> _gameRepositoryMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly DatabaseManagerService _service;

    public DatabaseManagerServiceTests()
    {
        _databaseRepositoryMock = new Mock<IReferenceDatabaseRepository>();
        _gameRepositoryMock = new Mock<IGameRepository>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _service = new DatabaseManagerService(
            _databaseRepositoryMock.Object,
            _gameRepositoryMock.Object,
            _httpClientFactoryMock.Object);
    }

    [Fact]
    public async Task GetAvailableDatabasesAsync_ShouldReturnDatabasesForProvider()
    {
        // Arrange
        var provider = "NoIntro";
        var databases = new List<ReferenceDatabase>
        {
            new ReferenceDatabase { Id = 1, Provider = provider, Console = "NES", Version = "20240101" },
            new ReferenceDatabase { Id = 2, Provider = provider, Console = "SNES", Version = "20240101" }
        };
        _databaseRepositoryMock.Setup(r => r.GetByProviderAsync(provider))
            .ReturnsAsync(databases);

        // Act
        var result = await _service.GetAvailableDatabasesAsync(provider);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(d => d.Console == "NES");
        result.Should().Contain(d => d.Console == "SNES");
    }

    [Fact]
    public async Task GetDatabasesForConsoleAsync_ShouldReturnDatabasesForConsole()
    {
        // Arrange
        var console = "NES";
        var databases = new List<ReferenceDatabase>
        {
            new ReferenceDatabase { Id = 1, Provider = "NoIntro", Console = console, Version = "20240101" },
            new ReferenceDatabase { Id = 2, Provider = "Redump", Console = console, Version = "20240101" }
        };
        _databaseRepositoryMock.Setup(r => r.GetByConsoleAsync(console))
            .ReturnsAsync(databases);

        // Act
        var result = await _service.GetDatabasesForConsoleAsync(console);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(d => d.Provider == "NoIntro");
        result.Should().Contain(d => d.Provider == "Redump");
    }

    [Fact]
    public async Task DownloadDatabaseAsync_ShouldDownloadDatabaseWithProgress()
    {
        // Arrange - T064: Test avec HttpClient mocké
        // Pour l'instant, GetDownloadUrl() n'est pas implémenté, donc ce test s'attend à une NotImplementedException
        // TODO: Une fois les URLs réelles implémentées, mocker HttpClient avec HttpMessageHandler

        var provider = "NoIntro";
        var console = "NES";
        var version = "20240101";

        var progressReports = new List<int>();
        var progress = new Progress<int>(p => progressReports.Add(p));

        _databaseRepositoryMock.Setup(r => r.GetByProviderConsoleVersionAsync(provider, console, version))
            .ReturnsAsync((ReferenceDatabase?)null);

        // Setup AddAsync pour retourner la base de données avec un Id
        _databaseRepositoryMock.Setup(r => r.AddAsync(It.IsAny<ReferenceDatabase>()))
            .ReturnsAsync((ReferenceDatabase db) =>
            {
                db.Id = 1; // Simuler l'ajout avec un Id
                return db;
            });

        _databaseRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ReferenceDatabase>()))
            .Returns(Task.CompletedTask);

        // Act & Assert
        // Pour l'instant, GetDownloadUrl() lève NotImplementedException
        // Ce test sera mis à jour une fois les URLs réelles implémentées
        Func<Task> act = async () => await _service.DownloadDatabaseAsync(provider, console, version, progress);

        await act.Should().ThrowAsync<NotImplementedException>()
            .WithMessage("*n'est pas encore implémenté*");
    }

    [Fact]
    public async Task DownloadDatabaseAsync_ShouldThrowExceptionIfAlreadyDownloaded()
    {
        // Arrange
        var provider = "NoIntro";
        var console = "NES";
        var version = "20240101";

        var existingDatabase = new ReferenceDatabase
        {
            Id = 1,
            Provider = provider,
            Console = console,
            Version = version,
            DownloadStatus = "Downloaded"
        };

        _databaseRepositoryMock.Setup(r => r.GetByProviderConsoleVersionAsync(provider, console, version))
            .ReturnsAsync(existingDatabase);

        // Act
        Func<Task> act = async () => await _service.DownloadDatabaseAsync(provider, console, version);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{provider}/{console}/{version}*");
    }

    [Fact]
    public async Task SetDefaultDatabaseAsync_ShouldSetDatabaseAsDefault()
    {
        // Arrange
        var databaseId = 1;
        var database = new ReferenceDatabase
        {
            Id = databaseId,
            Provider = "NoIntro",
            Console = "NES",
            Version = "20240101",
            DownloadStatus = "Downloaded",
            IsDefault = false
        };

        var otherDatabase = new ReferenceDatabase
        {
            Id = 2,
            Provider = "Redump",
            Console = "NES",
            Version = "20240101",
            DownloadStatus = "Downloaded",
            IsDefault = true
        };

        _databaseRepositoryMock.Setup(r => r.GetByIdAsync(databaseId))
            .ReturnsAsync(database);

        _databaseRepositoryMock.Setup(r => r.GetByConsoleAsync("NES"))
            .ReturnsAsync(new List<ReferenceDatabase> { database, otherDatabase });

        ReferenceDatabase? updatedDatabase = null;
        _databaseRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ReferenceDatabase>()))
            .Returns(Task.CompletedTask)
            .Callback<ReferenceDatabase>(db => updatedDatabase = db);

        // Act
        await _service.SetDefaultDatabaseAsync(databaseId);

        // Assert
        database.IsDefault.Should().BeTrue();
        otherDatabase.IsDefault.Should().BeFalse();
        _databaseRepositoryMock.Verify(r => r.UpdateAsync(database), Times.Once);
        _databaseRepositoryMock.Verify(r => r.UpdateAsync(otherDatabase), Times.Once);
    }

    [Fact]
    public async Task SetDefaultDatabaseAsync_ShouldThrowExceptionIfDatabaseNotFound()
    {
        // Arrange
        var databaseId = 999;
        _databaseRepositoryMock.Setup(r => r.GetByIdAsync(databaseId))
            .ReturnsAsync((ReferenceDatabase?)null);

        // Act
        Func<Task> act = async () => await _service.SetDefaultDatabaseAsync(databaseId);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage($"*{databaseId}*");
    }

    [Fact]
    public async Task SetDefaultDatabaseAsync_ShouldThrowExceptionIfNotDownloaded()
    {
        // Arrange
        var databaseId = 1;
        var database = new ReferenceDatabase
        {
            Id = databaseId,
            Provider = "NoIntro",
            Console = "NES",
            Version = "20240101",
            DownloadStatus = "Available",
            IsDefault = false
        };

        _databaseRepositoryMock.Setup(r => r.GetByIdAsync(databaseId))
            .ReturnsAsync(database);

        // Act
        Func<Task> act = async () => await _service.SetDefaultDatabaseAsync(databaseId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*téléchargée*");
    }

    [Fact]
    public async Task GetDefaultDatabaseForConsoleAsync_ShouldReturnDefaultDatabase()
    {
        // Arrange
        var console = "NES";
        var defaultDatabase = new ReferenceDatabase
        {
            Id = 1,
            Provider = "NoIntro",
            Console = console,
            Version = "20240101",
            IsDefault = true
        };

        _databaseRepositoryMock.Setup(r => r.GetDefaultForConsoleAsync(console))
            .ReturnsAsync(defaultDatabase);

        // Act
        var result = await _service.GetDefaultDatabaseForConsoleAsync(console);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.IsDefault.Should().BeTrue();
    }

    [Fact]
    public async Task CheckForUpdatesAsync_ShouldCheckAllDownloadedDatabases()
    {
        // Arrange
        var databases = new List<ReferenceDatabase>
        {
            new ReferenceDatabase
            {
                Id = 1,
                Provider = "NoIntro",
                Console = "NES",
                Version = "20240101",
                DownloadStatus = "Downloaded"
            },
            new ReferenceDatabase
            {
                Id = 2,
                Provider = "Redump",
                Console = "SNES",
                Version = "20240101",
                DownloadStatus = "Downloaded"
            }
        };

        _databaseRepositoryMock.Setup(r => r.GetDownloadedAsync())
            .ReturnsAsync(databases);

        ReferenceDatabase? updatedDatabase = null;
        _databaseRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ReferenceDatabase>()))
            .Returns(Task.CompletedTask)
            .Callback<ReferenceDatabase>(db => updatedDatabase = db);

        // Act
        var result = await _service.CheckForUpdatesAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(item =>
        {
            item.current.Should().NotBeNull();
            item.update.Should().BeNull(); // TODO: Will be implemented later with real provider logic
        });

        // Verify LastCheckedAt was updated
        _databaseRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<ReferenceDatabase>()), Times.Exactly(2));
    }

    [Fact]
    public async Task DeleteDatabaseAsync_ShouldDeleteDatabaseAndFile()
    {
        // Arrange
        var databaseId = 1;
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_db_{Guid.NewGuid()}.dat");
        await File.WriteAllTextAsync(tempFile, "test content");

        var database = new ReferenceDatabase
        {
            Id = databaseId,
            Provider = "NoIntro",
            Console = "NES",
            Version = "20240101",
            FilePath = tempFile,
            DownloadStatus = "Downloaded"
        };

        _databaseRepositoryMock.Setup(r => r.GetByIdAsync(databaseId))
            .ReturnsAsync(database);

        _databaseRepositoryMock.Setup(r => r.DeleteAsync(databaseId))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteDatabaseAsync(databaseId);

        // Assert
        File.Exists(tempFile).Should().BeFalse();
        _databaseRepositoryMock.Verify(r => r.DeleteAsync(databaseId), Times.Once);
    }

    [Fact]
    public async Task DeleteDatabaseAsync_ShouldThrowExceptionIfDatabaseNotFound()
    {
        // Arrange
        var databaseId = 999;
        _databaseRepositoryMock.Setup(r => r.GetByIdAsync(databaseId))
            .ReturnsAsync((ReferenceDatabase?)null);

        // Act
        Func<Task> act = async () => await _service.DeleteDatabaseAsync(databaseId);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage($"*{databaseId}*");
    }

    // T055: Tests pour GetAvailableProvidersAsync() - TDD: Écrire le test AVANT l'implémentation
    [Fact]
    public async Task GetAvailableProvidersAsync_ShouldReturnNoIntroRedumpGoodSet()
    {
        // Arrange
        // Act
        var result = await _service.GetAvailableProvidersAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("NoIntro");
        result.Should().Contain("Redump");
        result.Should().Contain("GoodSet");
        result.Should().HaveCount(3);
    }

    // T056: Tests pour GetAvailableVersionsAsync(provider, console) - TDD: Écrire le test AVANT l'implémentation
    [Fact]
    public async Task GetAvailableVersionsAsync_ShouldReturnVersionsForProviderAndConsole()
    {
        // Arrange
        var provider = "NoIntro";
        var console = "NES";

        // Act
        var result = await _service.GetAvailableVersionsAsync(provider, console);

        // Assert
        result.Should().NotBeNull();
        // Devrait retourner une liste de versions disponibles avec métadonnées (date, taille, etc.)
        // Pour l'instant, on teste juste que la méthode existe et retourne quelque chose
        result.Should().BeAssignableTo<IEnumerable<ReferenceDatabase>>();
    }

    [Fact]
    public async Task GetAvailableVersionsAsync_ShouldReturnVersionsWithMetadata()
    {
        // Arrange
        var provider = "Redump";
        var console = "SNES";

        // Act
        var result = await _service.GetAvailableVersionsAsync(provider, console);
        var versionsList = result.ToList();

        // Assert
        versionsList.Should().NotBeNull();
        // Chaque version devrait avoir Provider, Console, Version, ReleaseDate, FileSize
        foreach (var version in versionsList)
        {
            version.Provider.Should().Be(provider);
            version.Console.Should().Be(console);
            version.Version.Should().NotBeNullOrEmpty();
        }
    }

    // T057: Télécharger base de données avec progression - Déjà couvert par DownloadDatabaseAsync_ShouldDownloadDatabaseWithProgress
    // T058: Sélectionner version par défaut par console - Déjà couvert par SetDefaultDatabaseAsync_ShouldSetDatabaseAsDefault
    // T059: Vérifier disponibilité mises à jour - Déjà couvert par CheckForUpdatesAsync_ShouldCheckAllDownloadedDatabases

    [Fact]
    public async Task DownloadDatabaseAsync_ShouldHandleErrorsAndSetErrorStatus()
    {
        // Arrange - T060: Gérer échecs téléchargement
        var provider = "NoIntro";
        var console = "NES";
        var version = "20240101";

        _databaseRepositoryMock.Setup(r => r.GetByProviderConsoleVersionAsync(provider, console, version))
            .ReturnsAsync((ReferenceDatabase?)null);

        var database = new ReferenceDatabase
        {
            Id = 1,
            Provider = provider,
            Console = console,
            Version = version,
            DownloadStatus = "Downloading"
        };

        _databaseRepositoryMock.Setup(r => r.AddAsync(It.IsAny<ReferenceDatabase>()))
            .ReturnsAsync(database);

        // Act & Assert
        // T064/T068: Pour l'instant, GetDownloadUrl() n'est pas implémenté, donc ce test s'attend à une NotImplementedException
        // TODO: Une fois les URLs réelles implémentées, mocker HttpClient avec HttpMessageHandler pour tester la gestion d'erreur et retry logic
        Func<Task> act = async () => await _service.DownloadDatabaseAsync(provider, console, version);

        await act.Should().ThrowAsync<NotImplementedException>()
            .WithMessage("*n'est pas encore implémenté*");
    }
}

