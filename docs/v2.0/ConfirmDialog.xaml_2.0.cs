using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RandomCreation
{
    /// <summary>
    /// Borderless themed confirmation dialog.
    ///
    /// Standard destructive (red button):
    ///   new ConfirmDialog("Title", "Message")
    ///
    /// Non-destructive (blue button, custom labels):
    ///   new ConfirmDialog("Title", "Message", "Cancel", "Load")
    /// </summary>
    public partial class ConfirmDialog : Window
    {
        public ConfirmDialog(string title, string message)
        {
            InitializeComponent();
            TitleText.Text   = title;
            MessageText.Text = message;
            KeyDown += (_, e) => { if (e.Key == Key.Escape) DialogResult = false; };
        }

        public ConfirmDialog(string title, string message, string noLabel, string yesLabel)
            : this(title, message)
        {
            // Apply custom No button label via template
            NoButton.ApplyTemplate();
            if (NoButton.Template.FindName("BtnText", NoButton) is TextBlock noText)
                noText.Text = noLabel;

            // Replace YesButton with a blue non-destructive version
            // Build a new template in code to avoid flash of red
            YesButton.Template = BuildBlueButtonTemplate(yesLabel);
            YesButton.ApplyTemplate();
        }

        /// <summary>Builds a blue confirm button template with the given label.</summary>
        private static ControlTemplate BuildBlueButtonTemplate(string label)
        {
            var template = new ControlTemplate(typeof(Button));

            var border = new FrameworkElementFactory(typeof(Border));
            border.Name = "Bd";
            border.SetResourceReference(Border.BackgroundProperty, "AccentBlueBrush");
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(6));
            border.SetValue(Border.PaddingProperty, new Thickness(18, 7, 18, 7));

            var text = new FrameworkElementFactory(typeof(TextBlock));
            text.SetValue(TextBlock.TextProperty, label);
            text.SetValue(TextBlock.ForegroundProperty, Brushes.White);
            text.SetValue(TextBlock.FontSizeProperty, 13.0);
            border.AppendChild(text);

            template.VisualTree = border;

            // Hover trigger
            var trigger = new Trigger
            {
                Property = IsMouseOverProperty,
                Value    = true
            };
            trigger.Setters.Add(new Setter(
                Border.BackgroundProperty,
                Application.Current.Resources["AccentBlueHoverBrush"],
                "Bd"));
            template.Triggers.Add(trigger);

            return template;
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
            => DialogResult = true;

        private void NoButton_Click(object sender, RoutedEventArgs e)
            => DialogResult = false;
    }
}
