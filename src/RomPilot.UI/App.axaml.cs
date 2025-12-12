using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
        public static MainWindowViewModel? MainViewModel { get; private set; }

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
            
            // Initialize database and seed data
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<RomPilotDbContext>();
                context.Database.EnsureCreated();
                
                var seedService = scope.ServiceProvider.GetRequiredService<SeedDataService>();
                seedService.SeedAsync().Wait();
            }
            
            var mainViewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();
            MainViewModel = mainViewModel; // Store reference for access from other ViewModels
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel,
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Database
        services.AddDbContext<RomPilotDbContext>(options =>
            options.UseSqlite("Data Source=rompilot.db"));
        
        // Seed data service
        services.AddScoped<SeedDataService>();

        // Repositories
        services.AddScoped<IScannedFileRepository, ScannedFileRepository>();
        services.AddScoped<IExclusionFilterRepository, ExclusionFilterRepository>();
        services.AddScoped<IReferenceDatabaseRepository, ReferenceDatabaseRepository>();
        services.AddScoped<IExportConfigurationRepository, ExportConfigurationRepository>();
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
        services.AddScoped<RomPilot.Core.Services.IFilterService, RomPilot.Core.Services.FilterService>();
        services.AddScoped<RomPilot.Core.Services.IDatabaseManagerService, RomPilot.Core.Services.DatabaseManagerService>();
        services.AddScoped<RomPilot.Core.Services.IScanProgressReporter, RomPilot.Core.Services.ScanProgressReporter>();

        // ViewModels
        // MainWindowViewModel should be singleton to maintain state across navigation
        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient<ViewModels.ScanViewModel>();
        services.AddTransient<ViewModels.LibraryViewModel>();
    }

}
