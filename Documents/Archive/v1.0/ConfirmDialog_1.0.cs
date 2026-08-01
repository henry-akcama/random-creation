using System.Windows;

namespace CreatureCrafter
{
    public partial class ConfirmDialog : Window
    {
        public ConfirmDialog(string title, string message)
        {
            InitializeComponent();
            TitleText.Text   = title;
            MessageText.Text = message;
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
            => DialogResult = true;

        private void NoButton_Click(object sender, RoutedEventArgs e)
            => DialogResult = false;
    }
}
