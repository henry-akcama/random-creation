using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace RandomCreation
{
    /// <summary>
    /// Multiplies a base size value by the current AppScaleFactor resource.
    /// Use in XAML bindings to make font sizes and layout dimensions scale
    /// with the user's font size preference.
    ///
    /// Usage in XAML:
    ///   FontSize="{Binding Source={StaticResource FontSizeBody},
    ///              Converter={StaticResource ScaleConverter}}"
    /// </summary>
    public class ScaleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double baseValue = value switch
            {
                double d => d,
                int    i => (double)i,
                _        => 13.0
            };

            double scale = 1.0;
            try
            {
                if (Application.Current?.Resources.Contains(ThemeService.ScaleFactorKey) == true)
                    scale = (double)Application.Current.Resources[ThemeService.ScaleFactorKey];
            }
            catch { /* use 1.0 */ }

            return baseValue * scale;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Converts a bool IsEnabled value to a row opacity double.
    /// True (enabled)  → 1.0
    /// False (disabled) → 0.35
    /// </summary>
    public class EnabledToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && b ? 1.0 : 0.35;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Converts a bool to Visibility.
    /// True → Visible, False → Collapsed (default)
    /// Pass parameter "Inverse" to invert: True → Collapsed, False → Visible
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool b = value is bool bVal && bVal;
            bool inverse = parameter is string s && s == "Inverse";
            if (inverse) b = !b;
            return b ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value is Visibility v && v == Visibility.Visible;
    }

    /// <summary>
    /// Converts a bool IsEnabled to the correct ON/OFF pill background brush
    /// using the current theme resources.
    /// </summary>
    public class PillBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool on = value is bool b && b;
            var key = on ? "PillOnBackgroundBrush" : "PillOffBackgroundBrush";
            return Application.Current.Resources[key]
                ?? System.Windows.Media.Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Converts a bool IsEnabled to the correct ON/OFF pill foreground brush.
    /// </summary>
    public class PillForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool on = value is bool b && b;
            var key = on ? "PillOnForegroundBrush" : "PillOffForegroundBrush";
            return Application.Current.Resources[key]
                ?? System.Windows.Media.Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Converts a bool IsEnabled to the correct ON/OFF pill border brush.
    /// </summary>
    public class PillBorderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool on = value is bool b && b;
            var key = on ? "PillOnBorderBrush" : "PillOffBorderBrush";
            return Application.Current.Resources[key]
                ?? System.Windows.Media.Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Converts a bool IsEnabled to the ON/OFF pill text label.
    /// True → "ON", False → "OFF"
    /// </summary>
    public class PillTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && b ? "ON" : "OFF";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Converts a bool IsDimmed to a result card opacity.
    /// True (dimmed)  → 0.38
    /// False (normal) → 1.0
    /// </summary>
    public class DimmedToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && b ? 0.38 : 1.0;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Converts an int count to a Visibility.
    /// 0 → Visible (show empty state), > 0 → Collapsed (hide empty state)
    /// Pass parameter "Inverse" to show content when count > 0.
    /// </summary>
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int count = value switch
            {
                int    i => i,
                double d => (int)d,
                _        => 0
            };
            bool isEmpty = count == 0;
            bool inverse = parameter is string s && s == "Inverse";
            if (inverse) isEmpty = !isEmpty;
            return isEmpty ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
