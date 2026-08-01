using System.Windows;
using System.Windows.Input;

namespace RandomCreation
{
    /// <summary>
    /// Dialog for saving the current state as a preset.
    /// User can type a new name OR click an existing preset to overwrite it.
    /// After ShowDialog() == true, check PresetName and OverwriteTarget.
    /// </summary>
    public partial class SavePresetDialog : Window
    {
        /// <summary>The name typed for a new preset. Empty if overwriting.</summary>
        public string PresetName { get; private set; } = "";

        /// <summary>The existing preset to overwrite. Null if creating new.</summary>
        public Preset? OverwriteTarget { get; private set; }

        // Currently selected existing preset for overwrite
        private Preset? _selectedExisting;

        public SavePresetDialog(Preset? preselected = null)
        {
            InitializeComponent();

            // Populate existing presets list
            ExistingPresetsList.ItemsSource = DataService.Presets.Presets;

            // If a preset was preselected (e.g. right-click overwrite), highlight it
            if (preselected != null)
                _selectedExisting = preselected;

            Loaded += (_, _) => { NameInput.Focus(); };
            KeyDown += (_, e) => { if (e.Key == Key.Escape) DialogResult = false; };
        }

        // ── Existing preset selection ─────────────────────────────────────────

        private void ExistingPreset_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is Preset preset)
            {
                _selectedExisting = preset;
                // Clear the name input to signal overwrite mode
                NameInput.Text    = "";
                ErrorText.Visibility = Visibility.Collapsed;

                // Visually highlight selected row — rebuild list to apply selection style
                // Simple approach: put name in input as hint
                NameInput.Text = preset.Name;
                NameInput.SelectAll();
                NameInput.Focus();
            }
        }

        // ── Input handling ────────────────────────────────────────────────────

        private void NameInput_TextChanged(object sender,
            System.Windows.Controls.TextChangedEventArgs e)
        {
            ErrorText.Visibility = Visibility.Collapsed;

            // If user types something different from the selected existing name,
            // they want a new preset — deselect existing
            if (_selectedExisting != null &&
                NameInput.Text.Trim() != _selectedExisting.Name)
            {
                _selectedExisting = null;
            }
        }

        private void NameInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) TrySave();
        }

        // ── Save / Cancel ────────────────────────────────────────────────────

        private void SaveButton_Click(object sender, RoutedEventArgs e) => TrySave();

        private void CancelButton_Click(object sender, RoutedEventArgs e)
            => DialogResult = false;

        private void TrySave()
        {
            var raw       = NameInput.Text;
            var sanitized = NameValidator.Sanitize(raw);

            if (string.IsNullOrWhiteSpace(sanitized))
            {
                ShowError("Please enter a name or select an existing preset to overwrite.");
                return;
            }

            // Check if this exactly matches an existing preset name → overwrite mode
            var existing = DataService.Presets.Presets
                .FirstOrDefault(p => p.Name == sanitized);

            if (existing != null)
            {
                // Overwrite
                OverwriteTarget = existing;
                PresetName      = "";
            }
            else
            {
                // New preset — check for duplicate (case-insensitive)
                if (NameValidator.IsDuplicatePreset(sanitized, DataService.Presets.Presets))
                {
                    ShowError("A preset with this name already exists.");
                    return;
                }
                PresetName      = sanitized;
                OverwriteTarget = null;
            }

            DialogResult = true;
        }

        private void ShowError(string message)
        {
            ErrorText.Text       = message;
            ErrorText.Visibility = Visibility.Visible;
            NameInput.Focus();
        }
    }
}
