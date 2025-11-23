using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using RomPilot.Core.Database;
using RomPilot.Core.Repositories;
using RomPilot.Core.Services;
using RomPilot.Core.Checksums;
using RomPilot.Core.Archives;
using RomPilot.Core.Preferences;
using RomPilot.UI.ViewModels;
using RomPilot.UI.Views;
using System;

namespace RomPilot.UI;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;
    
    public static IServiceProvider? Services { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Configure dependency injection
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
        Services = _serviceProvider;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);
            desktop.MainWindow = new MainWindow
            {
                DataContext = _serviceProvider.GetRequiredService<MainWindowViewModel>(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Database
        services.AddDbContext<RomPilotDbContext>(options =>
            options.UseSqlite("Data Source=rompilot.db"));

        // Repositories
        services.AddScoped<IRomFileRepository, RomFileRepository>();
        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<IChecksumRepository, ChecksumRepository>();
        services.AddScoped<IConsoleRepository, ConsoleRepository>();
        services.AddScoped<IDatabaseSourceRepository, DatabaseSourceRepository>();
        services.AddScoped<IMetadataRepository, MetadataRepository>();

        // Services
        services.AddScoped<IChecksumCalculator, ChecksumCalculator>();
        services.AddScoped<IArchiveScanner, ArchiveScanner>();
        services.AddScoped<IConsoleDetectionService, ConsoleDetectionService>();
        services.AddScoped<IUserPreferencesService, UserPreferencesService>();
        services.AddScoped<RomPilot.Core.Services.IScanService, RomPilot.Core.Services.ScanService>();
        services.AddScoped<RomPilot.Core.Services.IGameIdentificationService, RomPilot.Core.Services.GameIdentificationService>();
        services.AddScoped<RomPilot.Core.Services.IScanProgressReporter, RomPilot.Core.Services.ScanProgressReporter>();

        // ViewModels
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<ViewModels.ScanViewModel>();
        services.AddTransient<ViewModels.ScanResultsViewModel>();
    }

}
