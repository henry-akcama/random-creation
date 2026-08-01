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
        // Tracks whether the current last result is marked as drawn
        private bool _currentResultIsDrawn = false;

        public MainWindow()
        {
            InitializeComponent();

            var s = DataService.Settings;
            if (s.WindowWidth  >= 800) Width  = s.WindowWidth;
            if (s.WindowHeight >= 600) Height = s.WindowHeight;
            if (s.WindowLeft   >= 0)   Left   = s.WindowLeft;
            if (s.WindowTop    >= 0)   Top    = s.WindowTop;

            PreviewKeyDown += MainWindow_KeyDown;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Register toast service
            ToastService.Register(ToastBorder, ToastText);

            ApplyFontScale();
            UpdateThemeToggleIcon();

            switch (DataService.MigrationKind)
            {
                case MigrationKind.V2ToV3:
                    new MigrationDialog { Owner = this }.ShowDialog();
                    break;
                case MigrationKind.V1ToV3:
                    new NoticeDialog(
                        "Data Not Compatible",
                        "Your existing save file (creature_crafter_data.json) is not compatible " +
                        "with Random Creation v3.0 and has been saved as creature_crafter_data.json.bak. " +
                        "The app has started fresh.")
                    { Owner = this }.ShowDialog();
                    break;
                case MigrationKind.UnknownVersion:
                    new NoticeDialog(
                        "Unrecognised Data Format",
                        "The data files were created by a newer version of Random Creation and could " +
                        "not be loaded. Backups have been saved with a .bak extension and the app has " +
                        "started fresh.")
                    { Owner = this }.ShowDialog();
                    break;
            }

            bool hasContent = DataService.Categories.Collections
                .Any(c => c.Groups.Any(g => g.Categories.Count > 0));

            if (!hasContent) NavigateToManageContent();
            else             NavigateToMain();
        }


        // ── Window controls ──────────────────────────────────────────────────

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1) DragMove();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
            => WindowState = WindowState.Minimized;

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal : WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            SaveWindowGeometry();
            DataService.SaveAll();
            base.OnClosing(e);
        }

        private void SaveWindowGeometry()
        {
            if (WindowState == WindowState.Normal)
                DataService.SaveWindowGeometry(Width, Height, Left, Top);
        }


        // ── Keyboard shortcuts ───────────────────────────────────────────────

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (ManageContentPanel.Visibility == Visibility.Visible)
                {
                    // Let ManageContentScreen handle Escape if cut is active — don't navigate away
                    if (ClipboardService.Mode != ClipboardService.ClipMode.Cut)
                        { NavigateToMain(); e.Handled = true; return; }
                    return; // ManageContentScreen.OnKeyDown handles it
                }
                if (HistoryPanel.Visibility == Visibility.Visible)
                    { NavigateToMain(); e.Handled = true; return; }
                if (SettingsPanel.Visibility == Visibility.Visible)
                    { NavigateToMain(); e.Handled = true; return; }
                if (OverlayPanel.Visibility == Visibility.Visible)
                    { HideOverlay(); e.Handled = true; return; }
                if (PresetPopup.IsOpen)
                    { PresetPopup.IsOpen = false; e.Handled = true; return; }
            }

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
                    NavigateToHistory(); e.Handled = true; break;
                case Key.M when Keyboard.Modifiers == ModifierKeys.Control:
                    NavigateToManageContent(); e.Handled = true; break;
                case Key.Z when Keyboard.Modifiers == ModifierKeys.Control:
                    if (ManageContentPanel.Visibility == Visibility.Visible)
                    {
                        var desc = UndoService.Undo();
                        if (desc != null)
                        {
                            ManageContentPanel.Refresh();
                            ToastService.Show($"Undone: {desc}");
                        }
                        e.Handled = true;
                    }
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

        public void ShowOverlay(System.Windows.Controls.UserControl screen)
        {
            OverlayPanel.Content    = screen;
            OverlayPanel.Visibility = Visibility.Visible;
            if (screen is CollectionsManagementScreen cms) cms.Refresh();
            else if (screen is PresetsScreen ps)           ps.Refresh();
        }

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
            // Keep "GENERATING FROM" label (index 0), clear the rest
            while (SummaryBar.Children.Count > 1)
                SummaryBar.Children.RemoveAt(1);

            var enabledCollections = DataService.Categories.Collections
                .Where(c => c.IsEnabled).ToList();

            if (enabledCollections.Count == 0)
            {
                SummaryBar.Children.Add(new TextBlock
                {
                    Text              = "No collections enabled",
                    Foreground        = (Brush)Application.Current.Resources["EmptyStateBrush"],
                    FontSize          = 11,
                    VerticalAlignment = VerticalAlignment.Center
                });
                return;
            }

            const int MaxPills = 8;
            int shown    = Math.Min(enabledCollections.Count, MaxPills);
            int overflow = enabledCollections.Count - shown;

            for (int i = 0; i < shown; i++)
                SummaryBar.Children.Add(CreateSummaryPill(enabledCollections[i].Name));

            if (overflow > 0)
                SummaryBar.Children.Add(CreateSummaryPill($"+{overflow} more"));

            int totalGroups = enabledCollections
                .SelectMany(c => c.Groups)
                .Count(g => g.IsEnabled);
            int totalCats = enabledCollections
                .SelectMany(c => c.Groups).Where(g => g.IsEnabled)
                .SelectMany(g => g.Categories).Count(cat => cat.IsEnabled);
            int totalOpts = enabledCollections
                .SelectMany(c => c.Groups).Where(g => g.IsEnabled)
                .SelectMany(g => g.Categories).Where(cat => cat.IsEnabled)
                .SelectMany(cat => cat.Options).Count(o => o.IsEnabled);

            SummaryBar.Children.Add(new TextBlock
            {
                Text              = $"· {totalGroups} groups · {totalCats} categories · {totalOpts} options",
                Foreground        = (Brush)Application.Current.Resources["TextMutedBrush"],
                FontSize          = 11,
                VerticalAlignment = VerticalAlignment.Center,
                Margin            = new Thickness(2, 0, 0, 0)
            });
        }

        private static Border CreateSummaryPill(string text)
        {
            return new Border
            {
                Background    = (Brush)Application.Current.Resources["SummaryPillOnBackgroundBrush"],
                CornerRadius  = new CornerRadius(10),
                Padding       = new Thickness(8, 2, 8, 2),
                Margin        = new Thickness(0, 0, 6, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Child = new TextBlock
                {
                    Text       = text,
                    Foreground = (Brush)Application.Current.Resources["SummaryPillOnForegroundBrush"],
                    FontSize   = 11,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
        }

        private void RefreshResultCards()
        {
            var lastResult = DataService.Settings.LastResult;

            ResultGroupCards.Children.Clear();

            if (lastResult.Count == 0)
            {
                ResultsEmptyText.Visibility  = Visibility.Visible;
                MarkAsDrawnButton.Visibility = Visibility.Collapsed;
                AiPromptButton.Visibility    = Visibility.Collapsed;
                PrintButton.Visibility       = Visibility.Collapsed;
                ResultTimestamp.Visibility   = Visibility.Collapsed;
                return;
            }

            ResultsEmptyText.Visibility  = Visibility.Collapsed;
            MarkAsDrawnButton.Visibility = Visibility.Visible;
            AiPromptButton.Visibility    = Visibility.Visible;
            PrintButton.Visibility       = Visibility.Visible;

            // Group result pairs by GroupName
            var groups = lastResult
                .GroupBy(p => string.IsNullOrEmpty(p.GroupName) ? "" : p.GroupName)
                .ToList();

            bool isSingleGroup = groups.Count <= 2;

            foreach (var grp in groups)
            {
                var card = BuildGroupCard(grp.Key, grp.ToList(), isSingleGroup);
                ResultGroupCards.Children.Add(card);
            }

            // Timestamp
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

            // Update Mark as Drawn button state
            UpdateMarkAsDrawnButton();
        }

        private Border BuildGroupCard(string groupName, List<ResultPair> pairs, bool expandFill)
        {
            var card = new Border
            {
                CornerRadius  = new CornerRadius(8),
                BorderBrush   = (Brush)Application.Current.Resources["BorderStandardBrush"],
                BorderThickness = new Thickness(1),
                Margin        = new Thickness(4),
                VerticalAlignment = VerticalAlignment.Top,
                ClipToBounds  = true
            };

            // Expand to fill for 1-2 groups, fixed 240px for 3+
            if (expandFill)
                card.MinWidth = 240;
            else
                card.Width = 240;

            var cardGrid = new Grid();
            cardGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            cardGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Group header with blue left border accent
            if (!string.IsNullOrEmpty(groupName))
            {
                var header = new Border
                {
                    Background    = (Brush)Application.Current.Resources["BackgroundExpandedBrush"],
                    BorderBrush   = (Brush)Application.Current.Resources["AccentBlueBrush"],
                    BorderThickness = new Thickness(3, 0, 0, 0),
                    Padding       = new Thickness(10, 7, 10, 7)
                };
                Grid.SetRow(header, 0);
                header.Child = new TextBlock
                {
                    Text       = groupName,
                    Foreground = (Brush)Application.Current.Resources["TextPrimaryBrush"],
                    FontSize   = 13,
                    FontWeight = FontWeights.Medium
                };
                cardGrid.Children.Add(header);
            }

            // Body — two-column grid of category/option rows
            var body = new StackPanel { Margin = new Thickness(10, 8, 10, 8) };
            Grid.SetRow(body, 1);

            foreach (var pair in pairs)
            {
                var rowGrid = new Grid
                {
                    Margin   = new Thickness(0, 2, 0, 2),
                    Cursor   = System.Windows.Input.Cursors.Hand,
                    Tag      = pair
                };
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var catText = new TextBlock
                {
                    Text       = pair.Category,
                    Foreground = (Brush)Application.Current.Resources["TextMutedBrush"],
                    FontSize   = 12,
                    TextWrapping = TextWrapping.Wrap
                };
                Grid.SetColumn(catText, 0);

                var optText = new TextBlock
                {
                    Text       = pair.Option,
                    Foreground = (Brush)Application.Current.Resources["ResultOptionBrush"],
                    FontSize   = 12,
                    FontWeight = FontWeights.SemiBold,
                    TextWrapping = TextWrapping.Wrap
                };
                Grid.SetColumn(optText, 1);

                rowGrid.Children.Add(catText);
                rowGrid.Children.Add(optText);

                // Apply dim state
                var rowBorder = new Border
                {
                    Tag    = pair,
                    Opacity = pair.IsDimmed ? 0.3 : 1.0,
                    Cursor  = System.Windows.Input.Cursors.Hand
                };
                rowBorder.Child = rowGrid;
                rowBorder.MouseLeftButtonUp += ResultCard_Click;

                body.Children.Add(rowBorder);
            }

            cardGrid.Children.Add(body);
            card.Child = cardGrid;
            return card;
        }

        private void RefreshRecentHistory()
        {
            var recent = DataService.History.History.TakeLast(3).Reverse().ToList();
            if (recent.Count > 0)
            {
                RecentHistoryList.ItemsSource     = recent;
                RecentHistoryEmptyText.Visibility = Visibility.Collapsed;
            }
            else
            {
                RecentHistoryList.ItemsSource     = null;
                RecentHistoryEmptyText.Visibility = Visibility.Visible;
            }
        }

        public void RefreshGenerateButtonState()
        {
            bool canGenerate = DataService.Categories.Collections
                .Where(col => col.IsEnabled)
                .SelectMany(col => col.Groups).Where(grp => grp.IsEnabled)
                .SelectMany(grp => grp.Categories).Where(cat => cat.IsEnabled)
                .SelectMany(cat => cat.Options).Any(opt => opt.IsEnabled);

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
                foreach (var grp in col.Groups)
                {
                    if (!grp.IsEnabled) continue;
                    foreach (var cat in grp.Categories)
                    {
                        if (!cat.IsEnabled) continue;
                        var pool = new List<Option>();
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
                        if (pool.Count == 0) continue;
                        result.Add(new ResultPair
                        {
                            GroupName = grp.Name,
                            Category  = cat.Name,
                            Option    = pool[rng.Next(pool.Count)].Name,
                            IsDimmed  = false
                        });
                    }
                }
            }

            if (result.Count == 0) return;

            var enabledCollections = DataService.Categories.Collections
                .Where(c => c.IsEnabled).ToList();

            // Always save the new result to history immediately
            var entry = new HistoryEntry
            {
                Timestamp            = DateTime.Now,
                Result               = result.ToList(),
                ActiveCollections    = enabledCollections.Select(c => c.Name).ToList(),
                EnabledGroupCount    = enabledCollections
                                       .SelectMany(c => c.Groups).Count(g => g.IsEnabled),
                EnabledCategoryCount = enabledCollections
                                       .SelectMany(c => c.Groups).Where(g => g.IsEnabled)
                                       .SelectMany(g => g.Categories).Count(cat => cat.IsEnabled),
                EnabledOptionCount   = enabledCollections
                                       .SelectMany(c => c.Groups).Where(g => g.IsEnabled)
                                       .SelectMany(g => g.Categories).Where(cat => cat.IsEnabled)
                                       .SelectMany(cat => cat.Options).Count(o => o.IsEnabled)
            };
            DataService.AddHistoryEntry(entry);

            // Update last result
            DataService.Settings.LastResult     = result;
            DataService.Settings.LastResultTime = DateTime.Now;
            DataService.SaveSettings();

            _currentResultIsDrawn = false;

            RefreshMainScreen();
        }


        // ── Result card dim/click ─────────────────────────────────────────────

        private void ResultCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is ResultPair pair)
            {
                pair.IsDimmed = !pair.IsDimmed;
                // Update opacity on the clicked border directly
                if (fe is Border b) b.Opacity = pair.IsDimmed ? 0.3 : 1.0;
                DataService.SaveSettings();
            }
        }


        // ── Mark as Drawn ─────────────────────────────────────────────────────

        private void MarkAsDrawnButton_Click(object sender, RoutedEventArgs e)
        {
            _currentResultIsDrawn = !_currentResultIsDrawn;
            UpdateMarkAsDrawnButton();

            // Find the matching history entry for the current last result
            var lastTime = DataService.Settings.LastResultTime;
            if (lastTime.HasValue)
            {
                var entry = DataService.History.History
                    .OrderByDescending(h => h.Timestamp)
                    .FirstOrDefault(h => Math.Abs((h.Timestamp - lastTime.Value).TotalSeconds) < 2);
                if (entry != null)
                {
                    entry.IsDrawn = _currentResultIsDrawn;
                    DataService.SaveHistory();
                }
            }

            ToastService.Show(_currentResultIsDrawn ? "Marked as drawn" : "Unmarked as drawn");
            RefreshRecentHistory();
        }

        private void UpdateMarkAsDrawnButton()
        {
            if (MarkAsDrawnButton.Template.FindName("DrawnText", MarkAsDrawnButton)
                is TextBlock tb)
            {
                tb.Text = _currentResultIsDrawn ? "✓ Drawn" : "○ Mark as Drawn";
                tb.Foreground = _currentResultIsDrawn
                    ? new SolidColorBrush(Color.FromRgb(0x30, 0xd1, 0x58))
                    : (Brush)Application.Current.Resources["TextMutedBrush"];
            }
            if (MarkAsDrawnButton.Template.FindName("DrawnBd", MarkAsDrawnButton)
                is Border bd)
            {
                bd.BorderBrush = _currentResultIsDrawn
                    ? new SolidColorBrush(Color.FromRgb(0x30, 0xd1, 0x58))
                    : (Brush)Application.Current.Resources["BorderStandardBrush"];
            }
        }


        // ── AI Prompt ─────────────────────────────────────────────────────────

        private void AiPromptButton_Click(object sender, RoutedEventArgs e)
        {
            var result = DataService.Settings.LastResult;
            if (result.Count == 0) return;

            // Build Option C format prompt — exclude dimmed rows
            var undimmed = result.Where(p => !p.IsDimmed).ToList();
            if (undimmed.Count == 0)
            {
                ToastService.Show("All results are dimmed — nothing to copy");
                return;
            }

            var collections = DataService.Settings.LastResult
                .Select(p => p.GroupName).Distinct().FirstOrDefault() ?? "";

            var collectionNames = DataService.Categories.Collections
                .Where(c => c.IsEnabled).Select(c => c.Name).ToList();
            string collectionPart = collectionNames.Count > 0
                ? string.Join(", ", collectionNames) : "My Collection";

            var grouped = undimmed
                .GroupBy(p => string.IsNullOrEmpty(p.GroupName) ? "Results" : p.GroupName);

            var sb = new System.Text.StringBuilder();
            sb.Append("Generate an image based on the following randomly generated traits. ");
            sb.Append($"Collection: {collectionPart}. ");

            foreach (var grp in grouped)
            {
                sb.Append($"{grp.Key.ToUpper()} — ");
                sb.Append(string.Join(", ", grp.Select(p => $"{p.Category}: {p.Option}")));
                sb.Append(". ");
            }

            try
            {
                Clipboard.SetText(sb.ToString().Trim());
                ToastService.Show("Prompt copied to clipboard");
            }
            catch
            {
                ToastService.Show("Could not copy to clipboard");
            }
        }


        // ── Print ─────────────────────────────────────────────────────────────

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            var result = DataService.Settings.LastResult;
            if (result.Count == 0) return;

            var entry = new HistoryEntry
            {
                Timestamp  = DataService.Settings.LastResultTime ?? DateTime.Now,
                Result     = result,
                ActiveCollections = DataService.Categories.Collections
                    .Where(c => c.IsEnabled).Select(c => c.Name).ToList()
            };

            new PrintPreviewDialog(entry) { Owner = this }.ShowDialog();
        }


        // ── Preset quick-load popup ───────────────────────────────────────────

        private void PresetsLinkButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshPresetPopup();
            PresetPopup.IsOpen = !PresetPopup.IsOpen;
        }

        private void RefreshPresetPopup()
        {
            var presets = DataService.Presets.Presets;
            // Force rebind by setting null first — WPF won't refresh if same reference
            PresetPopupList.ItemsSource = null;
            if (presets.Count == 0)
            {
                PresetPopupEmptyText.Visibility = Visibility.Visible;
            }
            else
            {
                PresetPopupList.ItemsSource     = new System.Collections.Generic.List<Preset>(presets);
                PresetPopupEmptyText.Visibility = Visibility.Collapsed;
            }
        }

        private void PresetPopupRow_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is Preset preset)
            {
                PresetPopup.IsOpen = false;
                DataService.ApplyPreset(preset);
                RefreshMainScreen();
                ToastService.Show($"Preset loaded: {preset.Name}");
            }
        }

        private void OpenPresetsScreen_Click(object sender, RoutedEventArgs e)
        {
            PresetPopup.IsOpen = false;
            var screen = new PresetsScreen();
            screen.OnClosed = () => RefreshMainScreen();
            ShowOverlay(screen);
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
            var current  = ThemeService.ResolveTheme(DataService.Settings.Theme);
            var newTheme = current == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark;
            DataService.Settings.Theme = newTheme;
            DataService.SaveSettings();
            ThemeService.ApplyTheme(newTheme);
            UpdateThemeToggleIcon();

            if      (SettingsPanel.Visibility      == Visibility.Visible) SettingsPanel.Refresh();
            else if (ManageContentPanel.Visibility == Visibility.Visible) ManageContentPanel.Refresh();
            else if (HistoryPanel.Visibility       == Visibility.Visible) HistoryPanel.RefreshHistory();
            else RefreshMainScreen();
        }

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
                sun.Visibility  = Visibility.Visible;
                moon.Visibility = Visibility.Visible;
                sun.Opacity     = 0.5;
                moon.Opacity    = 0.5;
            }
            else
            {
                sun.Opacity  = 1.0;
                moon.Opacity = 1.0;
                sun.Visibility  = resolved == AppTheme.Dark  ? Visibility.Visible : Visibility.Collapsed;
                moon.Visibility = resolved == AppTheme.Light ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void RecentHistoryItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is HistoryEntry entry)
                new ResultDetailDialog(entry, NavigateToMain, null, RefreshRecentHistory)
                    { Owner = this }.ShowDialog();
        }

        public void ApplyFontScale()
        {
            double scale = FontScaleHelper.GetScale(DataService.Settings.FontSize);
            Dispatcher.InvokeAsync(() =>
            {
                MainContentGrid.LayoutTransform = new ScaleTransform(scale, scale);
            }, System.Windows.Threading.DispatcherPriority.Render);
        }
    }
}
