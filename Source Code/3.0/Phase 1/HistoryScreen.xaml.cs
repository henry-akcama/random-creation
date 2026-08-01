using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RandomCreation
{
    public partial class HistoryScreen : UserControl
    {
        private MainWindow Main => (MainWindow)Window.GetWindow(this);

        public HistoryScreen() => InitializeComponent();

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

        // ── Refresh ──────────────────────────────────────────────────────────

        public void RefreshHistory()
        {
            var history = DataService.History.History;

            if (history.Count == 0)
            {
                HistoryList.ItemsSource = null;
                EmptyText.Visibility    = Visibility.Visible;
                return;
            }

            EmptyText.Visibility = Visibility.Collapsed;

            // Most recent first — dot colour is now determined by IsDrawn flag, not assigned here
            var entries = history.AsEnumerable().Reverse().ToList();
            HistoryList.ItemsSource = entries;
        }

        // ── History item click — open detail dialog ───────────────────────────

        private void HistoryItem_Click(object sender, MouseButtonEventArgs e)
        {
            // Ignore clicks that originated from the delete button
            if (e.OriginalSource is System.Windows.FrameworkElement src &&
                IsChildOfDeleteButton(src)) return;

            if (sender is FrameworkElement fe && fe.DataContext is HistoryEntry entry)
            {
                new ResultDetailDialog(entry, null, OnEntryDeletedFromDialog)
                    { Owner = Main }.ShowDialog();
            }
        }

        // Called when an entry is deleted from within the ResultDetailDialog
        private void OnEntryDeletedFromDialog()
        {
            RefreshHistory();
            Main.RefreshMainScreen();
        }

        // ── Delete single entry ───────────────────────────────────────────────

        private void DeleteHistoryItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is HistoryEntry entry)
            {
                if (DataService.Settings.ConfirmOnDelete)
                {
                    var dlg = new ConfirmDialog(
                        "Delete Entry",
                        "Delete this history entry? This cannot be undone.")
                    { Owner = Main };

                    if (dlg.ShowDialog() != true) return;
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
                var dlg = new ConfirmDialog(
                    "Clear All History",
                    "Delete all history entries? This cannot be undone.")
                { Owner = Main };

                if (dlg.ShowDialog() != true) return;
            }

            DataService.ClearAllHistory();
            RefreshHistory();
            Main.RefreshMainScreen();
        }

        // ── Back ─────────────────────────────────────────────────────────────

        private void BackButton_Click(object sender, RoutedEventArgs e)
            => Main.NavigateToMain();

        // ── Helpers ──────────────────────────────────────────────────────────

        /// <summary>
        /// Checks if a visual element is a child of the delete button
        /// to prevent the row click firing when the delete button is clicked.
        /// </summary>
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
