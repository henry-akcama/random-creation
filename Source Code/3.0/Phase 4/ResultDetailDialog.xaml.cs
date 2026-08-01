using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RandomCreation
{
    /// <summary>
    /// Borderless read-only dialog showing full result details for a history entry.
    /// Groups results by GroupName, one card per group.
    /// Pre-v3.0 entries (GroupName = "") display as a flat card list.
    /// </summary>
    public partial class ResultDetailDialog : Window
    {
        private readonly HistoryEntry _entry;
        private readonly Action?      _onClosed;
        private readonly Action?      _onDeleted;
        private readonly Action?      _onDrawnChanged;  // fires when IsDrawn state changes

        private readonly System.EventHandler _deactivatedHandler;

        // ── Constructors ─────────────────────────────────────────────────────

        public ResultDetailDialog(HistoryEntry entry, Action? onClosed)
            : this(entry, onClosed, null, null) { }

        public ResultDetailDialog(HistoryEntry entry, Action? onClosed, Action? onDeleted)
            : this(entry, onClosed, onDeleted, null) { }

        public ResultDetailDialog(HistoryEntry entry, Action? onClosed, Action? onDeleted,
                                  Action? onDrawnChanged)
        {
            InitializeComponent();
            _entry          = entry;
            _onClosed       = onClosed;
            _onDeleted      = onDeleted;
            _onDrawnChanged = onDrawnChanged;

            KeyDown += (_, e) => { if (e.Key == Key.Escape) Close(); };

            _deactivatedHandler = (_, _) => Close();
            Deactivated += _deactivatedHandler;

            PopulateHeader();
            BuildCards();
            UpdateMarkAsDrawnButton();
        }

        // ── Header ────────────────────────────────────────────────────────────

        private void PopulateHeader()
        {
            TimestampLabel.Text   = $"Generated: {_entry.FullTimestamp}";
            CollectionsLabel.Text = _entry.CollectionsSummary;
            CountsLabel.Text      = _entry.Summary;

            // Drawn state tints the top bar
            if (_entry.IsDrawn)
                TopBar.Background = new SolidColorBrush(Color.FromArgb(40, 0x30, 0xd1, 0x58));
        }

        // ── Cards ─────────────────────────────────────────────────────────────

        private void BuildCards()
        {
            CardPanel.Children.Clear();
            bool isFlat = _entry.Result.All(r => string.IsNullOrEmpty(r.GroupName));
            bool isSingleGroup = _entry.Result.Select(r => r.GroupName).Distinct().Count() <= 2;

            if (isFlat)
            {
                // Pre-v3.0 entry — flat card list, one card per pair
                foreach (var pair in _entry.Result)
                    CardPanel.Children.Add(BuildFlatCard(pair));
            }
            else
            {
                var groups = _entry.Result
                    .GroupBy(p => string.IsNullOrEmpty(p.GroupName) ? "" : p.GroupName);

                foreach (var grp in groups)
                    CardPanel.Children.Add(BuildGroupCard(grp.Key, grp.ToList(), isSingleGroup));
            }
        }

        private Border BuildFlatCard(ResultPair pair)
        {
            var card = new Border
            {
                Background      = (Brush)Application.Current.Resources["BackgroundCardBrush"],
                CornerRadius    = new CornerRadius(8),
                BorderBrush     = (Brush)Application.Current.Resources["BorderStandardBrush"],
                BorderThickness = new Thickness(1),
                Padding         = new Thickness(12, 8, 12, 8),
                Margin          = new Thickness(4),
                Width           = 220,
                MinHeight       = 70,
                VerticalAlignment = VerticalAlignment.Top,
                Opacity         = pair.IsDimmed ? 0.38 : 1.0
            };
            var sp = new StackPanel();
            sp.Children.Add(new TextBlock
            {
                Text       = pair.Category,
                Foreground = (Brush)Application.Current.Resources["TextPrimaryBrush"],
                FontSize   = 11, FontWeight = FontWeights.Medium,
                TextTrimming = System.Windows.TextTrimming.CharacterEllipsis
            });
            sp.Children.Add(new TextBlock
            {
                Text       = pair.Option,
                Foreground = (Brush)Application.Current.Resources["ResultOptionBrush"],
                FontSize   = 13, FontWeight = FontWeights.SemiBold,
                TextWrapping = TextWrapping.Wrap,
                Margin     = new Thickness(0, 4, 0, 0)
            });
            card.Child = sp;
            return card;
        }

        private Border BuildGroupCard(string groupName, System.Collections.Generic.List<ResultPair> pairs,
                                      bool expandFill)
        {
            var card = new Border
            {
                CornerRadius    = new CornerRadius(8),
                BorderBrush     = (Brush)Application.Current.Resources["BorderStandardBrush"],
                BorderThickness = new Thickness(1),
                Margin          = new Thickness(4),
                VerticalAlignment = VerticalAlignment.Top,
                ClipToBounds    = true
            };
            if (expandFill) card.MinWidth = 240;
            else            card.Width    = 240;

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            if (!string.IsNullOrEmpty(groupName))
            {
                var header = new Border
                {
                    Background      = (Brush)Application.Current.Resources["BackgroundExpandedBrush"],
                    BorderBrush     = (Brush)Application.Current.Resources["AccentBlueBrush"],
                    BorderThickness = new Thickness(3, 0, 0, 0),
                    Padding         = new Thickness(10, 7, 10, 7)
                };
                Grid.SetRow(header, 0);
                header.Child = new TextBlock
                {
                    Text       = groupName,
                    Foreground = (Brush)Application.Current.Resources["TextPrimaryBrush"],
                    FontSize   = 13, FontWeight = FontWeights.Medium
                };
                grid.Children.Add(header);
            }

            var body = new StackPanel { Margin = new Thickness(10, 8, 10, 8) };
            Grid.SetRow(body, 1);

            foreach (var pair in pairs)
            {
                var rowGrid = new Grid { Margin = new Thickness(0, 2, 0, 2) };
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition
                    { Width = new GridLength(1, GridUnitType.Star) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition
                    { Width = new GridLength(1, GridUnitType.Star) });

                var catTb = new TextBlock
                {
                    Text         = pair.Category,
                    Foreground   = (Brush)Application.Current.Resources["TextMutedBrush"],
                    FontSize     = 12,
                    TextWrapping = TextWrapping.Wrap
                };
                Grid.SetColumn(catTb, 0);

                var optTb = new TextBlock
                {
                    Text         = pair.Option,
                    Foreground   = (Brush)Application.Current.Resources["ResultOptionBrush"],
                    FontSize     = 12,
                    FontWeight   = FontWeights.SemiBold,
                    TextWrapping = TextWrapping.Wrap
                };
                Grid.SetColumn(optTb, 1);
                rowGrid.Children.Add(catTb);
                rowGrid.Children.Add(optTb);

                body.Children.Add(new Border
                {
                    Child   = rowGrid,
                    Opacity = pair.IsDimmed ? 0.38 : 1.0
                });
            }

            grid.Children.Add(body);
            card.Child = grid;
            return card;
        }

        // ── Mark as Drawn ─────────────────────────────────────────────────────

        private void MarkAsDrawnButton_Click(object sender, RoutedEventArgs e)
        {
            DetachDeactivated();
            _entry.IsDrawn = !_entry.IsDrawn;
            DataService.SaveHistory();
            UpdateMarkAsDrawnButton();

            TopBar.Background = _entry.IsDrawn
                ? new SolidColorBrush(Color.FromArgb(40, 0x30, 0xd1, 0x58))
                : System.Windows.Media.Brushes.Transparent;

            // Notify calling screen so history list and recent history refresh
            _onDrawnChanged?.Invoke();

            Deactivated += _deactivatedHandler;
        }

        private void UpdateMarkAsDrawnButton()
        {
            MarkAsDrawnButton.ApplyTemplate();
            if (MarkAsDrawnButton.Template.FindName("DrawnText", MarkAsDrawnButton)
                is TextBlock tb)
            {
                tb.Text = _entry.IsDrawn ? "✓ Drawn" : "○ Mark as Drawn";
                tb.Foreground = _entry.IsDrawn
                    ? new SolidColorBrush(Color.FromRgb(0x30, 0xd1, 0x58))
                    : (Brush)Application.Current.Resources["TextMutedBrush"];
            }
            if (MarkAsDrawnButton.Template.FindName("DrawnBd", MarkAsDrawnButton)
                is Border bd)
            {
                bd.BorderBrush = _entry.IsDrawn
                    ? new SolidColorBrush(Color.FromRgb(0x30, 0xd1, 0x58))
                    : (Brush)Application.Current.Resources["BorderStandardBrush"];
            }
        }

        // ── AI Prompt ─────────────────────────────────────────────────────────

        private void AiPromptButton_Click(object sender, RoutedEventArgs e)
        {
            DetachDeactivated();

            var undimmed = _entry.Result.Where(p => !p.IsDimmed).ToList();
            if (undimmed.Count == 0)
            {
                new NoticeDialog("Nothing to Copy",
                    "All result rows are dimmed. Undim at least one row to copy the prompt.")
                { Owner = this }.ShowDialog();
                Deactivated += _deactivatedHandler;
                return;
            }

            string collectionPart = _entry.ActiveCollections?.Count > 0
                ? string.Join(", ", _entry.ActiveCollections) : "My Collection";

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
                new NoticeDialog("Clipboard Error",
                    "Could not copy to clipboard. Try again.")
                { Owner = this }.ShowDialog();
            }

            Deactivated += _deactivatedHandler;
        }

        // ── Print ─────────────────────────────────────────────────────────────

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            DetachDeactivated();
            new PrintPreviewDialog(_entry) { Owner = this }.ShowDialog();
            Deactivated += _deactivatedHandler;
        }

        // ── Delete ────────────────────────────────────────────────────────────

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            DetachDeactivated();

            if (DataService.Settings.ConfirmOnDelete)
            {
                if (new ConfirmDialog("Delete Entry",
                    "Delete this history entry? This cannot be undone.")
                { Owner = this }.ShowDialog() != true)
                {
                    Deactivated += _deactivatedHandler;
                    return;
                }
            }

            DataService.DeleteHistoryEntry(_entry);
            _onDeleted?.Invoke();
            Close();
        }

        // ── Close ─────────────────────────────────────────────────────────────

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DetachDeactivated();
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _onClosed?.Invoke();
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private void DetachDeactivated() => Deactivated -= _deactivatedHandler;
    }
}
