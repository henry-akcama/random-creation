using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RandomCreation
{
    /// <summary>
    /// Settings screen — theme, font size, history, behavior, window, shortcuts, about.
    /// All changes apply instantly and save to settings.json immediately.
    /// </summary>
    public partial class SettingsScreen : UserControl
    {
        private MainWindow Main => (MainWindow)Window.GetWindow(this);

        // Prevent ValueChanged from firing during initial load
        private bool _loading = true;

        public SettingsScreen()
        {
            InitializeComponent();
            // Fix confirm delete toggle visual on first load
            Loaded += (_, _) =>
            {
                UpdateHistoryLimitDisplay(DataService.Settings.HistoryLimit);
            };
        }

        // ── Refresh — called when navigating to this screen ──────────────────

        public void Refresh()
        {
            _loading = true;

            var s = DataService.Settings;

            // Theme buttons
            UpdateThemeButtonHighlights(s.Theme);

            // Font size slider
            FontSizeSlider.Value = s.FontSize switch
            {
                FontSizeScale.Normal     => 0,
                FontSizeScale.Large      => 1,
                FontSizeScale.ExtraLarge => 2,
                _                        => 0
            };

            // Update preview to current font size
            UpdateFontPreview(s.FontSize);

            // History limit — defer to after template is applied
            Dispatcher.BeginInvoke(new Action(() =>
                UpdateHistoryLimitDisplay(s.HistoryLimit)));

            // Confirm on delete — set after loading flag so toggle fires correctly
            ConfirmDeleteToggle.IsOn = s.ConfirmOnDelete;

            // Load changelog from file
            ChangelogTextBox.Text = DataService.ReadChangelog();

            _loading = false;
        }

        // ── Escape key ───────────────────────────────────────────────────────

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                Main.NavigateToMain();
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        // ── Theme ────────────────────────────────────────────────────────────

        private void ThemeDark_Click(object sender,
            System.Windows.Input.MouseButtonEventArgs e)
            => ApplyTheme(AppTheme.Dark);

        private void ThemeLight_Click(object sender,
            System.Windows.Input.MouseButtonEventArgs e)
            => ApplyTheme(AppTheme.Light);

        private void ThemeSystem_Click(object sender,
            System.Windows.Input.MouseButtonEventArgs e)
            => ApplyTheme(AppTheme.System);

        private void ApplyTheme(AppTheme theme)
        {
            DataService.Settings.Theme = theme;
            DataService.SaveSettings();
            ThemeService.ApplyTheme(theme);
            UpdateThemeButtonHighlights(theme);
            // Update the main window theme toggle icon
            Main.UpdateThemeToggleIcon();
        }

        private void UpdateThemeButtonHighlights(AppTheme theme)
        {
            // Reset all borders to standard then highlight active one
            var standard = (Brush)Application.Current.Resources["BorderStandardBrush"];
            var accent   = (Brush)Application.Current.Resources["AccentBlueBrush"];

            ThemeDarkBtn.BorderBrush   = theme == AppTheme.Dark   ? accent : standard;
            ThemeLightBtn.BorderBrush  = theme == AppTheme.Light  ? accent : standard;
            ThemeSystemBtn.BorderBrush = theme == AppTheme.System ? accent : standard;

            var activeThickness   = new Thickness(2);
            var inactiveThickness = new Thickness(1);

            ThemeDarkBtn.BorderThickness   = theme == AppTheme.Dark   ? activeThickness : inactiveThickness;
            ThemeLightBtn.BorderThickness  = theme == AppTheme.Light  ? activeThickness : inactiveThickness;
            ThemeSystemBtn.BorderThickness = theme == AppTheme.System ? activeThickness : inactiveThickness;
        }

        // ── Font size ────────────────────────────────────────────────────────

        private void FontSizeSlider_ValueChanged(object sender,
            RoutedPropertyChangedEventArgs<double> e)
        {
            if (_loading) return;

            var scale = (int)FontSizeSlider.Value switch
            {
                0 => FontSizeScale.Normal,
                1 => FontSizeScale.Large,
                2 => FontSizeScale.ExtraLarge,
                _ => FontSizeScale.Normal
            };

            // Apply live preview immediately
            ThemeService.ApplyFontScale(scale);
            UpdateFontPreview(scale);

            // Save setting first so ApplyFontScale reads the correct value
            DataService.Settings.FontSize = scale;
            DataService.SaveSettings();

            // Apply LayoutTransform to whole window content
            Main.ApplyFontScale();
        }

        private void UpdateFontPreview(FontSizeScale scale)
        {
            double factor = FontScaleHelper.GetScale(scale);

            // Update preview text sizes to show effect
            PreviewCategoryText.FontSize = 13 * factor;
            PreviewCardCategory.FontSize = 12 * factor;
            PreviewCardOption.FontSize   = 13 * factor;
        }

        // ── History limit ────────────────────────────────────────────────────

        private void HistoryLimitToggle_Click(object sender, RoutedEventArgs e)
        {
            // Toggle between 500 and unlimited (-1)
            bool isUnlimited = DataService.Settings.HistoryLimit == -1;
            DataService.Settings.HistoryLimit = isUnlimited ? 500 : -1;
            DataService.SaveSettings();
            UpdateHistoryLimitDisplay(DataService.Settings.HistoryLimit);
        }

        private void UpdateHistoryLimitDisplay(int limit)
        {
            bool unlimited = limit == -1;
            HistoryLimitDescription.Text = unlimited
                ? "All history is kept indefinitely"
                : $"Keeps the last {limit} entries, oldest are removed automatically";

            // Update button text via template — find TextBlock in template
            if (HistoryLimitToggle.Template.FindName("LimitBtnText", HistoryLimitToggle)
                is System.Windows.Controls.TextBlock btnText)
            {
                btnText.Text = unlimited ? "Set limit (500)" : "Set unlimited";
            }
        }

        // ── Clear all history ────────────────────────────────────────────────

        private void ClearHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataService.History.History.Count == 0) return;

            var dlg = new ConfirmDialog(
                "Clear All History",
                "Delete all history entries? This cannot be undone.")
            { Owner = Main };

            if (dlg.ShowDialog() != true) return;

            DataService.ClearAllHistory();
            Main.RefreshMainScreen();
        }

        // ── Confirm on delete ────────────────────────────────────────────────

        private void ConfirmDeleteToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (_loading) return;
            DataService.Settings.ConfirmOnDelete = ConfirmDeleteToggle.IsOn;
            DataService.SaveSettings();
        }

        // ── Reset window size ────────────────────────────────────────────────

        private void ResetWindowButton_Click(object sender, RoutedEventArgs e)
        {
            DataService.ResetWindowGeometry();
            var win = Main;
            win.Width       = 1050;
            win.Height      = 700;
            win.WindowState = WindowState.Normal;
        }

        // ── Back ─────────────────────────────────────────────────────────────

        private void BackButton_Click(object sender, RoutedEventArgs e)
            => Main.NavigateToMain();
    }
}
