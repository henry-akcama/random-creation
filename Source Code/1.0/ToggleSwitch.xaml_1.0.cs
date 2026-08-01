using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CreatureCrafter
{
    public partial class ToggleSwitch : UserControl
    {
        // ── Dependency property ──────────────────────────────────────────────
        public static readonly DependencyProperty IsOnProperty =
            DependencyProperty.Register(
                "IsOn", typeof(bool), typeof(ToggleSwitch),
                new FrameworkPropertyMetadata(
                    true,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnIsOnChanged));

        public static readonly RoutedEvent ToggledEvent =
            EventManager.RegisterRoutedEvent(
                "Toggled", RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(ToggleSwitch));

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

        public ToggleSwitch()
        {
            InitializeComponent();
            // Fix: apply initial visual state after template is applied
            Loaded += (_, _) => UpdateVisual();
        }

        // Called when the dependency property changes (including initial binding)
        private static void OnIsOnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((ToggleSwitch)d).UpdateVisual();

        private void UpdateVisual()
        {
            // Track color
            Track.Background = IsOn
                ? new SolidColorBrush(Color.FromRgb(0x0a, 0x84, 0xff))
                : new SolidColorBrush(Color.FromRgb(0x3a, 0x3a, 0x3e));

            // Thumb position
            Thumb.HorizontalAlignment = IsOn
                ? HorizontalAlignment.Right
                : HorizontalAlignment.Left;

            // Label text and color
            LabelText.Text = IsOn ? "On" : "Off";
            LabelText.Foreground = IsOn
                ? new SolidColorBrush(Color.FromRgb(0x60, 0xaa, 0xff))
                : new SolidColorBrush(Color.FromRgb(0x66, 0x66, 0x66));
        }

        private void Track_Click(object sender, MouseButtonEventArgs e)
        {
            IsOn = !IsOn;
            RaiseEvent(new RoutedEventArgs(ToggledEvent));
        }
    }
}
