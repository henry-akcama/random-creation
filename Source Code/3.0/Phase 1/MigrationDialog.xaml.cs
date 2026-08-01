using System.Windows;

namespace RandomCreation
{
    /// <summary>
    /// One-time dialog shown after a v2.0 → v3.0 migration.
    /// Explains what changed, offers an optional history backup, then continues.
    /// Can be dismissed by clicking outside (Deactivated handler) or via Continue.
    /// </summary>
    public partial class MigrationDialog : Window
    {
        // Store handler in a field so we can detach it by reference
        private readonly System.EventHandler _deactivatedHandler;

        public MigrationDialog()
        {
            InitializeComponent();

            _deactivatedHandler = (_, _) => Close();
            Deactivated += _deactivatedHandler;

            // Hide Save Backup button if there is no history to back up
            if (string.IsNullOrEmpty(DataService.PreMigrationHistoryJson))
                SaveBackupButton.Visibility = Visibility.Collapsed;
        }

        private void DetachDeactivated()
        {
            Deactivated -= _deactivatedHandler;
        }

        private void SaveBackupButton_Click(object sender, RoutedEventArgs e)
        {
            // Detach first — SaveHistoryBackup and MessageBox.Show both shift
            // focus away from this window which would trigger Deactivated and
            // close the dialog mid-operation
            DetachDeactivated();

            bool saved = DataService.SaveHistoryBackup();

            if (saved)
            {
                SaveBackupButton.IsEnabled = false;
                if (SaveBackupButton.Template.FindName("BtnText", SaveBackupButton)
                    is System.Windows.Controls.TextBlock tb)
                    tb.Text = "Backup Saved ✓";
            }
            else
            {
                MessageBox.Show(
                    "Could not save the backup file. The data folder may not be writeable.",
                    "Backup Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }

            // Re-attach so clicking outside still dismisses after backup
            Deactivated += _deactivatedHandler;
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            DetachDeactivated();
            Close();
        }
    }
}
