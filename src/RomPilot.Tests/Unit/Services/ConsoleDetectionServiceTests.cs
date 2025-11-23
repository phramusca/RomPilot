using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using RomPilot.Core.Database;
using RomPilot.Core.Models;
using RomPilot.Core.Repositories;
using RomPilot.Core.Services;
using Models = RomPilot.Core.Models;
using Xunit;

namespace RomPilot.Tests.Unit.Services;

/// <summary>
/// Unit tests for ConsoleDetectionService.
/// Tests console detection based on file extension and size.
/// </summary>
public class ConsoleDetectionServiceTests
{
    private readonly Mock<IConsoleRepository> _consoleRepositoryMock;
    private readonly ConsoleDetectionService _service;

    public ConsoleDetectionServiceTests()
    {
        _consoleRepositoryMock = new Mock<IConsoleRepository>();
        _service = new ConsoleDetectionService(_consoleRepositoryMock.Object);
    }

    [Fact]
    public async Task DetectConsoleAsync_ShouldDetectNESFromExtension()
    {
        // Arrange
        var filePath = "test.nes";
        var console = new Models.Console { Id = 1, Name = "Nintendo Entertainment System", ShortName = "nes" };
        _consoleRepositoryMock.Setup(r => r.GetByShortNameAsync("nes"))
            .ReturnsAsync(console);

        // Act
        var result = await _service.DetectConsoleAsync(filePath, 40960);

        // Assert
        result.Should().Be("nes");
    }

    [Fact]
    public async Task DetectConsoleAsync_ShouldDetectSNESFromExtension()
    {
        // Arrange
        var filePath = "test.snes";
        var console = new Models.Console { Id = 2, Name = "Super Nintendo", ShortName = "snes" };
        _consoleRepositoryMock.Setup(r => r.GetByShortNameAsync("snes"))
            .ReturnsAsync(console);

        // Act
        var result = await _service.DetectConsoleAsync(filePath, 1024000);

        // Assert
        result.Should().Be("snes");
    }

    [Fact]
    public async Task DetectConsoleAsync_ShouldDetectGameBoyFromExtension()
    {
        // Arrange
        var filePath = "test.gb";
        var console = new Models.Console { Id = 3, Name = "Game Boy", ShortName = "gb" };
        _consoleRepositoryMock.Setup(r => r.GetByShortNameAsync("gb"))
            .ReturnsAsync(console);

        // Act
        var result = await _service.DetectConsoleAsync(filePath, 32768);

        // Assert
        result.Should().Be("gb");
    }

    [Fact]
    public async Task DetectConsoleAsync_ShouldReturnNullForUnknownExtension()
    {
        // Arrange
        var filePath = "test.unknown";
        _consoleRepositoryMock.Setup(r => r.GetByShortNameAsync(It.IsAny<string>()))
            .ReturnsAsync((Models.Console?)null);

        // Act
        var result = await _service.DetectConsoleAsync(filePath, 1024);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DetectConsoleAsync_ShouldReturnNullWhenConsoleNotInDatabase()
    {
        // Arrange
        var filePath = "test.nes";
        _consoleRepositoryMock.Setup(r => r.GetByShortNameAsync("nes"))
            .ReturnsAsync((Models.Console?)null);

        // Act
        var result = await _service.DetectConsoleAsync(filePath, 40960);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DetectConsoleAsync_ShouldHandleCaseInsensitiveExtensions()
    {
        // Arrange
        var filePath = "test.NES";
        var console = new Models.Console { Id = 1, Name = "Nintendo Entertainment System", ShortName = "nes" };
        _consoleRepositoryMock.Setup(r => r.GetByShortNameAsync("nes"))
            .ReturnsAsync(console);

        // Act
        var result = await _service.DetectConsoleAsync(filePath, 40960);

        // Assert
        result.Should().Be("nes");
    }

    [Fact]
    public async Task DetectConsoleAsync_ShouldHandleFilesWithoutExtension()
    {
        // Arrange
        var filePath = "test";
        _consoleRepositoryMock.Setup(r => r.GetByShortNameAsync(It.IsAny<string>()))
            .ReturnsAsync((Models.Console?)null);

        // Act
        var result = await _service.DetectConsoleAsync(filePath, 1024);

        // Assert
        result.Should().BeNull();
    }
}

