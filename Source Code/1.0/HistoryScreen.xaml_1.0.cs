using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CreatureCrafter
{
    public partial class HistoryScreen : UserControl
    {
        private MainWindow Main => (MainWindow)Window.GetWindow(this);

        public HistoryScreen() => InitializeComponent();

        public void RefreshHistory(List<HistoryEntry> history)
        {
            // Most recent first
            var entries = history.AsEnumerable().Reverse().ToList();
            // Assign display-only dot colors (not saved to JSON)
            HistoryColorHelper.AssignColors(entries);
            HistoryList.ItemsSource = entries;
        }

        private void HistoryItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is HistoryEntry entry)
                new ResultDetailDialog(entry) { Owner = Main }.ShowDialog();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
            => Main.NavigateToMain();
    }
}
