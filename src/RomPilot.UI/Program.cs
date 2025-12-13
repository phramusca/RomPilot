using System;
using Avalonia;

namespace RomPilot.UI;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // Disable dconf and dbus to avoid permission denied warnings in dev containers
        // These services are not needed for the application to function
        Environment.SetEnvironmentVariable("DCONF_PROFILE", "");
        Environment.SetEnvironmentVariable("NO_AT_BRIDGE", "1");
        Environment.SetEnvironmentVariable("GTK_USE_PORTAL", "0");

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
