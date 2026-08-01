using System.Windows;
using System.Windows.Input;

namespace RandomCreation
{
    /// <summary>
    /// Themed single-button informational dialog.
    /// Use for notices that require no decision — just acknowledgement.
    /// No system beep, no destructive styling.
    ///
    /// Usage:
    ///   new NoticeDialog("Title", "Message") { Owner = this }.ShowDialog();
    /// </summary>
    public partial class NoticeDialog : Window
    {
        private readonly System.EventHandler _deactivatedHandler;

        public NoticeDialog(string title, string message)
        {
            InitializeComponent();
            TitleText.Text = title;
            MessageText.Text = message;

            KeyDown += (_, e) => { if (e.Key == Key.Escape) Close(); };

            // Clicking outside dismisses — same behaviour as ConfirmDialog
            _deactivatedHandler = (_, _) => Close();
            Deactivated += _deactivatedHandler;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Deactivated -= _deactivatedHandler;
            Close();
        }
    }
}
