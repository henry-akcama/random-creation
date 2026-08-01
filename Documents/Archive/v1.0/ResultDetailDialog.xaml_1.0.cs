using System.Windows;

namespace CreatureCrafter
{
    public partial class ResultDetailDialog : Window
    {
        public ResultDetailDialog(HistoryEntry entry)
        {
            InitializeComponent();
            TimestampLabel.Text = $"Generated: {entry.FullTimestamp}";
            PairsList.ItemsSource = entry.Result;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
    }
}
