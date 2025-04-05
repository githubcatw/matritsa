using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using matritsa.Util;

namespace matritsa.Desktop;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) {
        var app = BuildAvaloniaApp();
        if (args.Length > 0) {
            if (args[0] == "--follow-the-white-rabbit") {
                Debug.WriteLine("мы вас поняли, но эта программа - немного другая матрица)");
                Utils.OpenUrl("https://www.youtube.com/watch?v=pFS4zYWxzNA");
                app = app.With(new FontManagerOptions() {
                    DefaultFamilyName = "Office Code Pro",
                    FontFallbacks = new FontFallback[] {
                        new FontFallback { FontFamily = "Consolas" },
                        new FontFallback { FontFamily = "DejaVu Mono" },
                    },
                });
            }
        }
        app.StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();
}
