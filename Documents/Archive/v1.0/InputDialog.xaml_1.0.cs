using System.Windows;
using System.Windows.Input;

namespace CreatureCrafter
{
    public partial class InputDialog : Window
    {
        public string Result { get; private set; } = "";

        public InputDialog(string title, string prompt, string defaultValue)
        {
            InitializeComponent();
            Title          = title;
            PromptText.Text = prompt;
            InputBox.Text  = defaultValue;
            // Select all text so the user can overtype immediately
            Loaded += (_, _) => { InputBox.SelectAll(); InputBox.Focus(); };
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Result = InputBox.Text;
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
            => DialogResult = false;

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)  OkButton_Click(sender, e);
            if (e.Key == Key.Escape) DialogResult = false;
        }
    }
}
