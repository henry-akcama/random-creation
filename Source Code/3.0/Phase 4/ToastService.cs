using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace RandomCreation
{
    /// <summary>
    /// Static service for showing themed toast notifications anchored to the
    /// bottom centre of MainWindow.
    ///
    /// Usage:
    ///   1. Call ToastService.Register(toastBorder) once in MainWindow.Loaded
    ///      passing the named Border element from MainWindow.xaml.
    ///   2. Call ToastService.Show("message") from anywhere.
    ///
    /// Behaviour:
    ///   - Fades in over 150ms, holds for 2 seconds, fades out over 300ms.
    ///   - If called while a toast is already visible, replaces it immediately.
    ///   - Uses DynamicResource so colours update with theme changes.
    /// </summary>
    public static class ToastService
    {
        private static Border?      _toast;
        private static TextBlock?   _toastText;
        private static DispatcherTimer? _holdTimer;

        private const double FadeInMs  = 150;
        private const double HoldMs    = 2000;
        private const double FadeOutMs = 300;

        /// <summary>
        /// Called once by MainWindow after InitializeComponent.
        /// Passes the toast Border element defined in MainWindow.xaml.
        /// </summary>
        public static void Register(Border toastBorder, TextBlock toastText)
        {
            _toast     = toastBorder;
            _toastText = toastText;

            _holdTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(HoldMs)
            };
            _holdTimer.Tick += (_, _) =>
            {
                _holdTimer.Stop();
                FadeOut();
            };
        }

        /// <summary>
        /// Shows a toast with the given message.
        /// Safe to call from any thread — marshals to UI thread automatically.
        /// </summary>
        public static void Show(string message)
        {
            if (_toast == null || _toastText == null) return;

            if (!_toast.Dispatcher.CheckAccess())
            {
                _toast.Dispatcher.Invoke(() => Show(message));
                return;
            }

            // Stop any existing animation/timer
            _holdTimer?.Stop();
            _toast.BeginAnimation(UIElement.OpacityProperty, null);

            // Update text
            _toastText.Text = message;

            // Show and fade in
            _toast.Visibility = Visibility.Visible;

            var fadeIn = new DoubleAnimation(0, 1,
                TimeSpan.FromMilliseconds(FadeInMs))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            fadeIn.Completed += (_, _) => _holdTimer?.Start();
            _toast.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        }

        private static void FadeOut()
        {
            if (_toast == null) return;

            var fadeOut = new DoubleAnimation(1, 0,
                TimeSpan.FromMilliseconds(FadeOutMs))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };
            fadeOut.Completed += (_, _) =>
            {
                if (_toast != null)
                    _toast.Visibility = Visibility.Collapsed;
            };
            _toast.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }
    }
}
