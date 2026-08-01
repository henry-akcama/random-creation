using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RandomCreation
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Restore window geometry from settings
            var s = DataService.Settings;
            if (s.WindowWidth  >= 800) Width  = s.WindowWidth;
            if (s.WindowHeight >= 600) Height = s.WindowHeight;
            if (s.WindowLeft   >= 0)   Left   = s.WindowLeft;
            if (s.WindowTop    >= 0)   Top    = s.WindowTop;

            // Keyboard shortcuts at window level — PreviewKeyDown fires before child controls
            PreviewKeyDown += MainWindow_KeyDown;

            // Defer navigation until fully loaded
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Apply font scale to window
            ApplyFontScale();

            // Set initial theme toggle icon
            UpdateThemeToggleIcon();

            // Show one-time migration popup if migration just ran
            if (DataService.MigrationJustCompleted)
            {
                new ConfirmDialog(
                    "Data Migrated",
                    "Your v1.0 data has been migrated to the new format. " +
                    "All your categories and history have been moved into a collection called \"My Collection\". " +
                    "Your original save file has been backed up as creature_crafter_data.json.bak.",
                    "Close", "Close")
                { Owner = this }.ShowDialog();
            }

            // Navigate to Manage Content on first launch if no collections have categories
            bool hasContent = DataService.Categories.Collections
                .Any(c => c.Categories.Count > 0);

            if (!hasContent)
                NavigateToManageContent();
            else
                NavigateToMain();
        }


        // ── Window controls ──────────────────────────────────────────────────

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Only drag on left button single click, not double click (which maximizes)
            if (e.ClickCount == 1)
                DragMove();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
            => WindowState = WindowState.Minimized;

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
            => Close();

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            SaveWindowGeometry();
            DataService.SaveAll();
            base.OnClosing(e);
        }

        private void SaveWindowGeometry()
        {
            // Don't save maximized dimensions — save the restored size
            if (WindowState == WindowState.Normal)
                DataService.SaveWindowGeometry(Width, Height, Left, Top);
        }


        // ── Keyboard shortcuts ───────────────────────────────────────────────

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            // Escape — go back from any sub-screen to main
            if (e.Key == Key.Escape)
            {
                if (ManageContentPanel.Visibility == Visibility.Visible)
                {
                    NavigateToMain();
                    e.Handled = true;
                    return;
                }
                if (HistoryPanel.Visibility == Visibility.Visible)
                {
                    NavigateToMain();
                    e.Handled = true;
                    return;
                }
                if (SettingsPanel.Visibility == Visibility.Visible)
                {
                    NavigateToMain();
                    e.Handled = true;
                    return;
                }
                if (OverlayPanel.Visibility == Visibility.Visible)
                {
                    HideOverlay();
                    e.Handled = true;
                    return;
                }
            }

            // Only fire main screen shortcuts when main panel is visible and no dialog is open
            if (MainPanel.Visibility != Visibility.Visible) return;

            switch (e.Key)
            {
                case Key.Space:
                case Key.Enter when !IsDialogOpen():
                    if (GenerateButton.IsEnabled)
                        GenerateButton_Click(this, new RoutedEventArgs());
                    e.Handled = true;
                    break;

                case Key.H when Keyboard.Modifiers == ModifierKeys.Control:
                    NavigateToHistory();
                    e.Handled = true;
                    break;

                case Key.M when Keyboard.Modifiers == ModifierKeys.Control:
                    NavigateToManageContent();
                    e.Handled = true;
                    break;
            }
        }

        private static bool IsDialogOpen()
            => Application.Current.Windows.OfType<Window>()
                .Any(w => w != Application.Current.MainWindow && w.IsVisible);


        // ── Navigation ───────────────────────────────────────────────────────

        public void NavigateToMain()
        {
            MainPanel.Visibility          = Visibility.Visible;
            ManageContentPanel.Visibility = Visibility.Collapsed;
            HistoryPanel.Visibility       = Visibility.Collapsed;
            SettingsPanel.Visibility      = Visibility.Collapsed;
            OverlayPanel.Visibility       = Visibility.Collapsed;
            OverlayPanel.Content          = null;
            RefreshMainScreen();
        }

        public void NavigateToManageContent()
        {
            MainPanel.Visibility          = Visibility.Collapsed;
            ManageContentPanel.Visibility = Visibility.Visible;
            HistoryPanel.Visibility       = Visibility.Collapsed;
            SettingsPanel.Visibility      = Visibility.Collapsed;
            OverlayPanel.Visibility       = Visibility.Collapsed;
            OverlayPanel.Content          = null;
            ManageContentPanel.Refresh();
        }

        private void NavigateToHistory()
        {
            MainPanel.Visibility          = Visibility.Collapsed;
            ManageContentPanel.Visibility = Visibility.Collapsed;
            HistoryPanel.Visibility       = Visibility.Visible;
            SettingsPanel.Visibility      = Visibility.Collapsed;
            OverlayPanel.Visibility       = Visibility.Collapsed;
            OverlayPanel.Content          = null;
            HistoryPanel.RefreshHistory();
        }

        private void NavigateToSettings()
        {
            MainPanel.Visibility          = Visibility.Collapsed;
            ManageContentPanel.Visibility = Visibility.Collapsed;
            HistoryPanel.Visibility       = Visibility.Collapsed;
            SettingsPanel.Visibility      = Visibility.Visible;
            OverlayPanel.Visibility       = Visibility.Collapsed;
            OverlayPanel.Content          = null;
            SettingsPanel.Refresh();
        }

        /// <summary>
        /// Shows a sub-screen overlaid on top of the current panel.
        /// Used for CollectionsManagementScreen and PresetsScreen.
        /// The sub-screen calls HideOverlay() when the user navigates back.
        /// </summary>
        public void ShowOverlay(System.Windows.Controls.UserControl screen)
        {
            OverlayPanel.Content    = screen;
            OverlayPanel.Visibility = Visibility.Visible;

            // Call Refresh() on the screen if it has one
            if (screen is CollectionsManagementScreen cms) cms.Refresh();
            else if (screen is PresetsScreen ps)           ps.Refresh();
        }

        /// <summary>Hides the overlay and clears its content.</summary>
        public void HideOverlay()
        {
            OverlayPanel.Visibility = Visibility.Collapsed;
            OverlayPanel.Content    = null;
        }


        // ── Main screen refresh ──────────────────────────────────────────────

        public void RefreshMainScreen()
        {
            RefreshSummaryBar();
            RefreshResultCards();
            RefreshRecentHistory();
            RefreshGenerateButtonState();
        }

        private void RefreshSummaryBar()
        {
            // Keep the "GENERATING FROM" label, rebuild pills after it
            while (SummaryBar.Children.Count > 1)
                SummaryBar.Children.RemoveAt(1);

            var enabledCollections = DataService.Categories.Collections
                .Where(c => c.IsEnabled).ToList();

            if (enabledCollections.Count == 0)
            {
                var none = new TextBlock
                {
                    Text              = "No collections enabled",
                    Foreground        = (Brush)Application.Current.Resources["EmptyStateBrush"],
                    FontSize          = 11,
                    VerticalAlignment = VerticalAlignment.Center
                };
                SummaryBar.Children.Add(none);
                return;
            }

            // Show enabled collection pills — "+X more" if overflow
            const int MaxPills = 8;
            int shown    = Math.Min(enabledCollections.Count, MaxPills);
            int overflow = enabledCollections.Count - shown;

            for (int i = 0; i < shown; i++)
                SummaryBar.Children.Add(CreateSummaryPill(enabledCollections[i].Name, true));

            if (overflow > 0)
                SummaryBar.Children.Add(CreateSummaryPill($"+{overflow} more", true));

            // Add separator then counts
            int totalEnabledCats = enabledCollections
                .SelectMany(c => c.Categories)
                .Count(cat => cat.IsEnabled);
            int totalEnabledOpts = enabledCollections
                .SelectMany(c => c.Categories)
                .Where(cat => cat.IsEnabled)
                .SelectMany(cat => cat.Options)
                .Count(o => o.IsEnabled);

            var countText = new TextBlock
            {
                Text              = $"· {totalEnabledCats} categories · {totalEnabledOpts} options",
                Foreground        = (Brush)Application.Current.Resources["TextMutedBrush"],
                FontSize          = 11,
                VerticalAlignment = VerticalAlignment.Center,
                Margin            = new Thickness(2, 0, 0, 0)
            };
            SummaryBar.Children.Add(countText);
        }

        private static Border CreateSummaryPill(string text, bool isEnabled)
        {
            var bgKey = isEnabled ? "SummaryPillOnBackgroundBrush" : "SummaryPillOffBackgroundBrush";
            var fgKey = isEnabled ? "SummaryPillOnForegroundBrush" : "SummaryPillOffForegroundBrush";

            return new Border
            {
                Background    = (Brush)Application.Current.Resources[bgKey],
                CornerRadius  = new CornerRadius(10),
                Padding       = new Thickness(8, 2, 8, 2),
                Margin        = new Thickness(0, 0, 6, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Child = new TextBlock
                {
                    Text       = text,
                    Foreground = (Brush)Application.Current.Resources[fgKey],
                    FontSize   = 11,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
        }

        private void RefreshResultCards()
        {
            var lastResult = DataService.Settings.LastResult;
            ResultsGrid.ItemsSource = lastResult.Count > 0 ? lastResult : null;

            ResultsEmptyText.Visibility = lastResult.Count == 0
                ? Visibility.Visible : Visibility.Collapsed;

            if (DataService.Settings.LastResultTime.HasValue)
            {
                var dt = DataService.Settings.LastResultTime.Value;
                ResultTimestamp.Text = dt.Date == DateTime.Today
                    ? $"Generated today at {dt:h:mm tt}"
                    : $"Generated {dt:MMM d} at {dt:h:mm tt}";
                ResultTimestamp.Visibility = Visibility.Visible;
            }
            else
            {
                ResultTimestamp.Visibility = Visibility.Collapsed;
            }
        }

        private void RefreshRecentHistory()
        {
            var recent = DataService.History.History
                .TakeLast(3).Reverse().ToList();

            HistoryColorHelper.AssignColors(recent);

            if (recent.Count > 0)
            {
                RecentHistoryList.ItemsSource  = recent;
                RecentHistoryEmptyText.Visibility = Visibility.Collapsed;
            }
            else
            {
                RecentHistoryList.ItemsSource  = null;
                RecentHistoryEmptyText.Visibility = Visibility.Visible;
            }
        }

        public void RefreshGenerateButtonState()
        {
            // Button is enabled only if at least one enabled option exists
            // across all enabled collections and enabled categories
            bool canGenerate = DataService.Categories.Collections
                .Where(col => col.IsEnabled)
                .SelectMany(col => col.Categories)
                .Where(cat => cat.IsEnabled)
                .SelectMany(cat => cat.Options)
                .Any(opt => opt.IsEnabled);

            GenerateButton.IsEnabled = canGenerate;
        }


        // ── Generate ─────────────────────────────────────────────────────────

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            var rng    = new Random();
            var result = new List<ResultPair>();

            foreach (var col in DataService.Categories.Collections)
            {
                if (!col.IsEnabled) continue;

                foreach (var cat in col.Categories)
                {
                    if (!cat.IsEnabled) continue;

                    // Build weighted pool from enabled options only
                    var pool = new List<RandomOption>();
                    foreach (var opt in cat.Options)
                    {
                        if (!opt.IsEnabled) continue;
                        int weight = opt.Weight switch
                        {
                            WeightTier.Normal => 3,
                            WeightTier.Low    => 2,
                            WeightTier.Rare   => 1,
                            _                 => 3
                        };
                        for (int i = 0; i < weight; i++) pool.Add(opt);
                    }

                    // Skip categories with no enabled options silently
                    if (pool.Count == 0) continue;

                    result.Add(new ResultPair
                    {
                        Category = cat.Name,
                        Option   = pool[rng.Next(pool.Count)].Name,
                        IsDimmed = false
                    });
                }
            }

            if (result.Count == 0) return; // Button should be disabled — safety check

            // Capture dim state of current result before clearing
            var currentResult = DataService.Settings.LastResult;

            // Build history entry
            var enabledCollections = DataService.Categories.Collections
                .Where(c => c.IsEnabled).ToList();

            var entry = new HistoryEntry
            {
                Timestamp            = DateTime.Now,
                Result               = currentResult.Count > 0
                                       ? currentResult.Select(p => new ResultPair
                                           {
                                               Category = p.Category,
                                               Option   = p.Option,
                                               IsDimmed = p.IsDimmed
                                           }).ToList()
                                       : result.ToList(),
                ActiveCollections    = enabledCollections.Select(c => c.Name).ToList(),
                EnabledCategoryCount = enabledCollections
                                       .SelectMany(c => c.Categories)
                                       .Count(cat => cat.IsEnabled),
                EnabledOptionCount   = enabledCollections
                                       .SelectMany(c => c.Categories)
                                       .Where(cat => cat.IsEnabled)
                                       .SelectMany(cat => cat.Options)
                                       .Count(o => o.IsEnabled)
            };

            // Save previous result to history (with its dim states)
            // then update last result to new undimmed result
            if (currentResult.Count > 0)
                DataService.AddHistoryEntry(entry);

            // Set new result — all dims cleared
            DataService.Settings.LastResult     = result;
            DataService.Settings.LastResultTime = DateTime.Now;
            DataService.SaveSettings();

            // If this is the very first generate, save it to history too
            if (currentResult.Count == 0)
                DataService.AddHistoryEntry(new HistoryEntry
                {
                    Timestamp            = DateTime.Now,
                    Result               = result.ToList(),
                    ActiveCollections    = entry.ActiveCollections,
                    EnabledCategoryCount = entry.EnabledCategoryCount,
                    EnabledOptionCount   = entry.EnabledOptionCount
                });

            RefreshMainScreen();
        }


        // ── Result card dim/ignore ────────────────────────────────────────────

        private void ResultCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is ResultPair pair)
            {
                // IsDimmed setter fires INotifyPropertyChanged — WPF updates
                // the Opacity binding live without needing a list rebind
                pair.IsDimmed = !pair.IsDimmed;
                DataService.SaveSettings();
            }
        }


        // ── Nav button handlers ──────────────────────────────────────────────

        private void ManageContentButton_Click(object sender, RoutedEventArgs e)
            => NavigateToManageContent();

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
            => NavigateToHistory();

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
            => NavigateToSettings();

        private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            // Toggle between dark and light only
            var current  = ThemeService.ResolveTheme(DataService.Settings.Theme);
            var newTheme = current == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark;

            DataService.Settings.Theme = newTheme;
            DataService.SaveSettings();
            ThemeService.ApplyTheme(newTheme);
            UpdateThemeToggleIcon();

            // Force all screens to refresh their dynamic resource bindings
            // by re-navigating to the current screen
            if (SettingsPanel.Visibility == Visibility.Visible)
                SettingsPanel.Refresh();
            else if (ManageContentPanel.Visibility == Visibility.Visible)
                ManageContentPanel.Refresh();
            else if (HistoryPanel.Visibility == Visibility.Visible)
                HistoryPanel.RefreshHistory();
            else
                RefreshMainScreen();
        }

        /// <summary>
        /// Updates the sun/moon/auto icon in the theme toggle button.
        /// Sun = dark mode active, Moon = light mode active,
        /// Both visible at half opacity = system default
        /// </summary>
        public void UpdateThemeToggleIcon()
        {
            ThemeToggleBtn.ApplyTemplate();
            var sun  = ThemeToggleBtn.Template.FindName("SunIcon",  ThemeToggleBtn) as UIElement;
            var moon = ThemeToggleBtn.Template.FindName("MoonIcon", ThemeToggleBtn) as UIElement;

            if (sun == null || moon == null) return;

            var setting  = DataService.Settings.Theme;
            var resolved = ThemeService.ResolveTheme(setting);

            if (setting == AppTheme.System)
            {
                // Show both at half opacity to indicate "follows system"
                sun.Visibility  = Visibility.Visible;
                moon.Visibility = Visibility.Visible;
                sun.Opacity     = 0.5;
                moon.Opacity    = 0.5;
            }
            else
            {
                sun.Opacity  = 1.0;
                moon.Opacity = 1.0;
                sun.Visibility  = resolved == AppTheme.Dark  ? Visibility.Visible  : Visibility.Collapsed;
                moon.Visibility = resolved == AppTheme.Light ? Visibility.Visible  : Visibility.Collapsed;
            }
        }

        private void RecentHistoryItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is HistoryEntry entry)
                new ResultDetailDialog(entry, NavigateToMain) { Owner = this }.ShowDialog();
        }

        /// <summary>
        /// Applies font size scaling using LayoutTransform on the main content area.
        /// This scales all visual content including elements with hardcoded FontSize.
        /// Called on startup and whenever the font size setting changes.
        /// </summary>
        public void ApplyFontScale()
        {
            double scale = FontScaleHelper.GetScale(DataService.Settings.FontSize);
            var transform = new ScaleTransform(scale, scale);
            // Apply to the inner Grid (Grid.Row="1") which contains all screen panels
            // The title bar (Row 0) is excluded so it doesn't scale awkwardly
            MainContentGrid.LayoutTransform = transform;
        }
    }
}
