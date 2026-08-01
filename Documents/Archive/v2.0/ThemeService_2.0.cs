using System;
using System.Windows;
using Microsoft.Win32;

namespace RandomCreation
{
    /// <summary>
    /// Manages theme and font size resource dictionary switching for Random Creation.
    /// Call Apply() once on startup before the window is shown, then again whenever
    /// the user changes theme or font size in Settings.
    /// </summary>
    public static class ThemeService
    {
        // Resource dictionary URIs
        private const string DarkThemeUri  = "Themes/DarkTheme.xaml";
        private const string LightThemeUri = "Themes/LightTheme.xaml";

        // Resource key for the current scale factor used by ScaleConverter
        public const string ScaleFactorKey = "AppScaleFactor";

        /// <summary>
        /// Applies the current theme and font scale from DataService.Settings.
        /// Safe to call at any time — replaces the existing theme dictionary in place.
        /// </summary>
        public static void Apply()
        {
            ApplyTheme(DataService.Settings.Theme);
            ApplyFontScale(DataService.Settings.FontSize);
        }

        /// <summary>
        /// Applies a specific theme immediately without saving to settings.
        /// Used for live preview in the Settings screen.
        /// </summary>
        public static void ApplyTheme(AppTheme theme)
        {
            var resolved = ResolveTheme(theme);
            var uri      = resolved == AppTheme.Light ? LightThemeUri : DarkThemeUri;
            SwapThemeDictionary(new Uri(uri, UriKind.Relative));
        }

        /// <summary>
        /// Applies a specific font scale immediately without saving to settings.
        /// Used for live preview in the Settings screen.
        /// </summary>
        public static void ApplyFontScale(FontSizeScale scale)
        {
            double factor = FontScaleHelper.GetScale(scale);

            // Update or insert the scale factor resource in App.Resources
            if (Application.Current.Resources.Contains(ScaleFactorKey))
                Application.Current.Resources[ScaleFactorKey] = factor;
            else
                Application.Current.Resources.Add(ScaleFactorKey, factor);
        }

        /// <summary>
        /// Resolves AppTheme.System to either Dark or Light by reading
        /// the Windows registry preference.
        /// </summary>
        public static AppTheme ResolveTheme(AppTheme theme)
        {
            if (theme != AppTheme.System) return theme;

            try
            {
                // Windows stores light/dark preference here:
                // 0 = dark apps, 1 = light apps
                using var key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                var val = key?.GetValue("AppsUseLightTheme");
                if (val is int intVal)
                    return intVal == 1 ? AppTheme.Light : AppTheme.Dark;
            }
            catch { /* Registry read failed — fall back to dark */ }

            return AppTheme.Dark;
        }

        /// <summary>
        /// Returns true if the currently active resolved theme is Light.
        /// Useful for controls that need to know the active theme at runtime.
        /// </summary>
        public static bool IsLightTheme =>
            ResolveTheme(DataService.Settings.Theme) == AppTheme.Light;


        // ── Private helpers ──────────────────────────────────────────────────

        private static void SwapThemeDictionary(Uri newUri)
        {
            var appDicts = Application.Current.Resources.MergedDictionaries;

            // Find and remove the existing theme dictionary if present
            ResourceDictionary? existing = null;
            foreach (var dict in appDicts)
            {
                if (dict.Source != null &&
                   (dict.Source.OriginalString.Contains("DarkTheme") ||
                    dict.Source.OriginalString.Contains("LightTheme")))
                {
                    existing = dict;
                    break;
                }
            }

            var newDict = new ResourceDictionary { Source = newUri };

            if (existing != null)
            {
                // Replace in place to minimise flicker
                int idx = appDicts.IndexOf(existing);
                appDicts[idx] = newDict;
            }
            else
            {
                // First time — just add it
                appDicts.Add(newDict);
            }
        }
    }
}
