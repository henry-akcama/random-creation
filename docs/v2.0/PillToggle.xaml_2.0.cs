using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RandomCreation
{
    /// <summary>
    /// A small ON/OFF pill toggle control that replaces the v1.0 ToggleSwitch.
    /// Displays "ON" or "OFF" in a small rounded pill.
    /// The parent row's opacity is controlled externally via EnabledToOpacityConverter
    /// bound to the same IsOn/IsEnabled value — this control itself is always
    /// fully opaque and interactive even when the row appears faded.
    /// </summary>
    public partial class PillToggle : UserControl
    {
        // ── Dependency property ──────────────────────────────────────────────

        public static readonly DependencyProperty IsOnProperty =
            DependencyProperty.Register(
                "IsOn", typeof(bool), typeof(PillToggle),
                new FrameworkPropertyMetadata(
                    true,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnIsOnChanged));

        public static readonly RoutedEvent ToggledEvent =
            EventManager.RegisterRoutedEvent(
                "Toggled", RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(PillToggle));

        public bool IsOn
        {
            get => (bool)GetValue(IsOnProperty);
            set => SetValue(IsOnProperty, value);
        }

        public event RoutedEventHandler Toggled
        {
            add    => AddHandler(ToggledEvent, value);
            remove => RemoveHandler(ToggledEvent, value);
        }

        public PillToggle() => InitializeComponent();

        // Dependency property changed callback — WPF updates bindings automatically
        // since the XAML uses RelativeSource bindings on IsOn, so no manual
        // visual update needed here unlike the old ToggleSwitch approach.
        private static void OnIsOnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Bindings handle the visual update automatically.
            // This callback is kept for any future logic needs.
        }

        private void Pill_Click(object sender, MouseButtonEventArgs e)
        {
            IsOn = !IsOn;
            RaiseEvent(new RoutedEventArgs(ToggledEvent));
            // Prevent the click from bubbling up to the parent row
            // which would accidentally trigger row selection
            e.Handled = true;
        }
    }
}
