using System;
using System.Windows;
using System.Windows.Input;

namespace RandomCreation
{
    /// <summary>
    /// Borderless read-only dialog showing full result details for a history entry.
    /// Cards are static snapshots — no hover, no click interaction.
    /// Dimmed cards appear at reduced opacity reflecting the state at time of generation.
    ///
    /// Can be opened from two sources:
    ///   1. HistoryScreen — pass onDeleted callback so history list refreshes on delete
    ///   2. Main screen recent history — pass onClosed callback to return to main screen
    /// </summary>
    public partial class ResultDetailDialog : Window
    {
        private readonly HistoryEntry _entry;
        private readonly Action?      _onClosed;
        private readonly Action?      _onDeleted;

        // ── Constructors ─────────────────────────────────────────────────────

        /// <summary>Open from main screen recent history.</summary>
        public ResultDetailDialog(HistoryEntry entry, Action? onClosed)
            : this(entry, onClosed, null) { }

        /// <summary>Open from history screen — onDeleted fires after deletion.</summary>
        public ResultDetailDialog(HistoryEntry entry, Action? onClosed, Action? onDeleted)
        {
            InitializeComponent();

            _entry     = entry;
            _onClosed  = onClosed;
            _onDeleted = onDeleted;

            // Keyboard: Escape = Close
            KeyDown += (_, e) => { if (e.Key == Key.Escape) Close(); };

            PopulateHeader();
            PairsList.ItemsSource = entry.Result;
        }

        // ── Header population ─────────────────────────────────────────────────

        private void PopulateHeader()
        {
            TimestampLabel.Text  = $"Generated: {_entry.FullTimestamp}";
            CollectionsLabel.Text = _entry.CollectionsSummary;
            CountsLabel.Text      = _entry.CountsSummary;
        }

        // ── Button handlers ───────────────────────────────────────────────────

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataService.Settings.ConfirmOnDelete)
            {
                var dlg = new ConfirmDialog(
                    "Delete Entry",
                    "Delete this history entry? This cannot be undone.")
                { Owner = this };

                if (dlg.ShowDialog() != true) return;
            }

            DataService.DeleteHistoryEntry(_entry);

            // Fire the deletion callback before closing
            _onDeleted?.Invoke();

            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
            => Close();

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _onClosed?.Invoke();
        }
    }
}
