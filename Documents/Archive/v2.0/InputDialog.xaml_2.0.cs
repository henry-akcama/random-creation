using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace RandomCreation
{
    /// <summary>
    /// Borderless themed text input dialog matching ConfirmDialog style.
    /// Supports inline validation — duplicate detection and blocked character prevention.
    ///
    /// Basic usage:
    ///   var dlg = new InputDialog("Add Category", "Enter category name:", "") { Owner = ... };
    ///   if (dlg.ShowDialog() == true) { var name = dlg.Result; }
    ///
    /// With duplicate validation:
    ///   var dlg = new InputDialog("Add Category", "Enter category name:", "",
    ///       name => NameValidator.IsDuplicateCategory(name, existingCategories))
    ///       { Owner = ... };
    /// </summary>
    public partial class InputDialog : Window
    {
        public string Result { get; private set; } = "";

        // Optional validation function — returns true if the name is invalid
        // (i.e. is a duplicate). Caller provides context-specific check.
        private readonly Func<string, bool>? _isDuplicate;

        // ── Constructors ─────────────────────────────────────────────────────

        /// <summary>Basic input dialog with no duplicate validation.</summary>
        public InputDialog(string title, string prompt, string defaultValue)
            : this(title, prompt, defaultValue, null) { }

        /// <summary>Input dialog with duplicate validation function.</summary>
        public InputDialog(string title, string prompt, string defaultValue,
                           Func<string, bool>? isDuplicate)
        {
            InitializeComponent();

            TitleText.Text  = title;
            PromptText.Text = prompt;
            InputBox.Text   = defaultValue;
            _isDuplicate    = isDuplicate;

            // Select all text so user can overtype immediately
            Loaded += (_, _) => { InputBox.SelectAll(); InputBox.Focus(); };

            // Keyboard: Escape = Cancel
            KeyDown += (_, e) => { if (e.Key == Key.Escape) DialogResult = false; };
        }

        // ── Event handlers ───────────────────────────────────────────────────

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateAndCommit()) return;
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
            => DialogResult = false;

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!ValidateAndCommit()) return;
                DialogResult = true;
            }
        }

        private void InputBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // Clear error as soon as user starts typing again
            ErrorText.Visibility = Visibility.Collapsed;
            ErrorText.Text       = "";
        }

        // ── Validation ───────────────────────────────────────────────────────

        private bool ValidateAndCommit()
        {
            var raw       = InputBox.Text;
            var sanitized = NameValidator.Sanitize(raw);

            // Empty after sanitizing
            if (string.IsNullOrWhiteSpace(sanitized))
            {
                ShowError("Name cannot be empty.");
                return false;
            }

            // Input contained blocked characters — show what was stripped
            if (sanitized != raw.Trim())
            {
                // Update the input box to show the sanitized version
                // and let the user confirm it looks right
                InputBox.Text = sanitized;
                InputBox.CaretIndex = sanitized.Length;
                ShowError("Some characters were removed. Special characters (\\ / \" ' < >) are not allowed.");
                return false;
            }

            // Duplicate check if validator was provided
            if (_isDuplicate != null && _isDuplicate(sanitized))
            {
                ShowError("This name is already in use. Please choose a different name.");
                return false;
            }

            Result = sanitized;
            return true;
        }

        private void ShowError(string message)
        {
            ErrorText.Text       = message;
            ErrorText.Visibility = Visibility.Visible;
            InputBox.Focus();
            InputBox.SelectAll();
        }
    }
}
