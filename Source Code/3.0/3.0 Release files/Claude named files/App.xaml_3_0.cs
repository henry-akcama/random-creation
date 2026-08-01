using System.Windows;
using System.Windows.Controls;

namespace RandomCreation
{
    /// <summary>
    /// Application entry point for Random Creation v2.0.
    ///
    /// Startup order:
    ///   1. DataService.Initialise() — loads all JSON data and runs v1.0 migration if needed
    ///   2. ThemeService.Apply()     — applies saved theme and font scale BEFORE window renders
    ///   3. MainWindow is constructed and shown (via StartupUri in App.xaml)
    ///
    /// This order ensures the correct theme and font size are applied before
    /// the first frame is drawn, preventing any visible flash of wrong theme.
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Step 1 — Load all data and run migration if needed
            DataService.Initialise();

            // Step 2 — Apply theme and font scale before any UI renders
            ThemeService.Apply();

            // Step 3 — Set tooltip delay globally (400ms vs WPF default ~700ms)
            ToolTipService.InitialShowDelayProperty.OverrideMetadata(
                typeof(DependencyObject),
                new FrameworkPropertyMetadata(400));

            // Step 4 — Continue normal WPF startup (shows MainWindow via StartupUri)
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Final save on exit — catches any unsaved state
            DataService.SaveAll();
            // Clear in-memory services — not persisted
            UndoService.Clear();
            ClipboardService.Clear();
            base.OnExit(e);
        }
    }
}
