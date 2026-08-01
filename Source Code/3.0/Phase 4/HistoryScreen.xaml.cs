using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RandomCreation
{
    public partial class HistoryScreen : UserControl
    {
        private MainWindow Main => (MainWindow)Window.GetWindow(this);
        private bool _showDrawnOnly = false;

        public HistoryScreen() => InitializeComponent();

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Main.NavigateToMain();
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        public void RefreshHistory()
        {
            var all      = DataService.History.History;
            int drawnCount = all.Count(h => h.IsDrawn);

            DrawnCountText.Text = drawnCount > 0 ? $"{drawnCount} drawn" : "";

            var entries = all.AsEnumerable().Reverse().ToList();

            if (_showDrawnOnly)
                entries = entries.Where(h => h.IsDrawn).ToList();

            if (entries.Count == 0)
            {
                HistoryList.ItemsSource = null;
                EmptyText.Text = _showDrawnOnly
                    ? "No drawn entries."
                    : "No history yet. Hit Generate to get started.";
                EmptyText.Visibility = Visibility.Visible;
            }
            else
            {
                EmptyText.Visibility    = Visibility.Collapsed;
                HistoryList.ItemsSource = entries;
            }
        }

        // ── Filter ────────────────────────────────────────────────────────────

        private void FilterAll_Click(object sender, MouseButtonEventArgs e)
        {
            _showDrawnOnly = false;
            UpdateFilterPills();
            RefreshHistory();
        }

        private void FilterDrawn_Click(object sender, MouseButtonEventArgs e)
        {
            _showDrawnOnly = true;
            UpdateFilterPills();
            RefreshHistory();
        }

        private void UpdateFilterPills()
        {
            var activeColor = (Brush)Application.Current.Resources["AccentBlueBrush"];
            var inactiveColor = (Brush)Application.Current.Resources["ButtonDarkBrush"];
            var activeFg = System.Windows.Media.Brushes.White;
            var inactiveFg = (Brush)Application.Current.Resources["TextMutedBrush"];

            FilterAllPill.Background   = _showDrawnOnly ? inactiveColor : activeColor;
            FilterDrawnPill.Background = _showDrawnOnly ? activeColor   : inactiveColor;

            if (FilterAllPill.Child is TextBlock allTb)
                allTb.Foreground = _showDrawnOnly ? inactiveFg : activeFg;
            if (FilterDrawnPill.Child is TextBlock drawnTb)
                drawnTb.Foreground = _showDrawnOnly ? activeFg : inactiveFg;
        }

        // ── History item click ────────────────────────────────────────────────

        private void HistoryItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement src && IsChildOfDeleteButton(src)) return;
            if (sender is FrameworkElement fe && fe.DataContext is HistoryEntry entry)
            {
                new ResultDetailDialog(entry, null, OnEntryDeletedFromDialog, RefreshHistory)
                    { Owner = Main }.ShowDialog();
            }
        }

        private void OnEntryDeletedFromDialog()
        {
            RefreshHistory();
            Main.RefreshMainScreen();
        }

        // ── Delete ────────────────────────────────────────────────────────────

        private void DeleteHistoryItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is HistoryEntry entry)
            {
                if (DataService.Settings.ConfirmOnDelete)
                {
                    if (new ConfirmDialog("Delete Entry",
                        "Delete this history entry? This cannot be undone.")
                    { Owner = Main }.ShowDialog() != true) return;
                }
                DataService.DeleteHistoryEntry(entry);
                RefreshHistory();
                Main.RefreshMainScreen();
            }
        }

        // ── Clear all ─────────────────────────────────────────────────────────

        private void ClearAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataService.History.History.Count == 0) return;
            if (DataService.Settings.ConfirmOnDelete)
            {
                if (new ConfirmDialog("Clear All History",
                    "Delete all history entries? This cannot be undone.")
                { Owner = Main }.ShowDialog() != true) return;
            }
            DataService.ClearAllHistory();
            RefreshHistory();
            Main.RefreshMainScreen();
        }

        // ── Back ─────────────────────────────────────────────────────────────

        private void BackButton_Click(object sender, RoutedEventArgs e)
            => Main.NavigateToMain();

        // ── Helpers ──────────────────────────────────────────────────────────

        private static bool IsChildOfDeleteButton(System.Windows.DependencyObject element)
        {
            var current = element;
            while (current != null)
            {
                if (current is Button btn && btn.Tag is HistoryEntry)
                    return true;
                current = System.Windows.Media.VisualTreeHelper.GetParent(current);
            }
            return false;
        }
    }
}
