using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace RandomCreation
{
    // ════════════════════════════════════════════════════════════════════════════
    // ENUMS
    // ════════════════════════════════════════════════════════════════════════════

    public enum WeightTier { Normal, Low, Rare }

    public enum AppTheme { Dark, Light, System }

    public enum FontSizeScale { Normal, Large, ExtraLarge }


    // ════════════════════════════════════════════════════════════════════════════
    // CORE DATA MODELS — saved to categories.json
    // ════════════════════════════════════════════════════════════════════════════

    /// <summary>A single option within a category.</summary>
    public class RandomOption
    {
        public string     Name      { get; set; } = "";
        public WeightTier Weight    { get; set; } = WeightTier.Normal;
        public bool       IsEnabled { get; set; } = true;
    }

    /// <summary>A category containing options, belonging to a collection.</summary>
    public class RandomCategory
    {
        public string             Name      { get; set; } = "";
        public bool               IsEnabled { get; set; } = true;
        public List<RandomOption> Options   { get; set; } = new();
    }

    /// <summary>A named collection of categories. Top-level organizational unit.</summary>
    public class RandomCollection
    {
        public string               Name       { get; set; } = "";
        public bool                 IsEnabled  { get; set; } = true;
        public List<RandomCategory> Categories { get; set; } = new();
    }

    /// <summary>Root model saved to categories.json.</summary>
    public class CategoriesData
    {
        public List<RandomCollection> Collections { get; set; } = new();
    }


    // ════════════════════════════════════════════════════════════════════════════
    // RESULT & HISTORY MODELS — saved to history.json
    // ════════════════════════════════════════════════════════════════════════════

    /// <summary>One category/option pair in a generated result.</summary>
    public class ResultPair : INotifyPropertyChanged
    {
        public string Category { get; set; } = "";
        public string Option   { get; set; } = "";

        private bool _isDimmed;
        public bool IsDimmed
        {
            get => _isDimmed;
            set
            {
                if (_isDimmed == value) return;
                _isDimmed = value;
                OnPropertyChanged(nameof(IsDimmed));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    /// <summary>A single history entry recording a full generation.</summary>
    public class HistoryEntry
    {
        public DateTime        Timestamp  { get; set; }
        public List<ResultPair> Result    { get; set; } = new();

        // Generation context saved at time of generate
        public List<string> ActiveCollections      { get; set; } = new();
        public int          EnabledCategoryCount   { get; set; } = 0;
        public int          EnabledOptionCount     { get; set; } = 0;

        // Display-only properties — not saved to JSON
        [System.Text.Json.Serialization.JsonIgnore]
        public string DotColorHex { get; set; } = "#60aaff";

        [System.Text.Json.Serialization.JsonIgnore]
        public Brush DotColor => new SolidColorBrush(
            (Color)ColorConverter.ConvertFromString(DotColorHex));

        [System.Text.Json.Serialization.JsonIgnore]
        public string Summary => string.Join(" · ", Result.Select(r => r.Option));

        [System.Text.Json.Serialization.JsonIgnore]
        public string ShortTime
        {
            get
            {
                if (Timestamp.Date == DateTime.Today)              return Timestamp.ToString("h:mm tt");
                if (Timestamp.Date == DateTime.Today.AddDays(-1)) return "Yesterday";
                return Timestamp.ToString("MMM d");
            }
        }

        [System.Text.Json.Serialization.JsonIgnore]
        public string FullTimestamp => Timestamp.ToString("MMM d, yyyy  h:mm tt");

        [System.Text.Json.Serialization.JsonIgnore]
        public string CollectionsSummary
        {
            get
            {
                if (ActiveCollections == null || ActiveCollections.Count == 0)
                    return "Collections: My Collection";
                return "Collections: " + string.Join(", ", ActiveCollections);
            }
        }

        [System.Text.Json.Serialization.JsonIgnore]
        public string CountsSummary
        {
            get
            {
                // For migrated v1.0 entries EnabledOptionCount will be 0 — omit it
                if (EnabledOptionCount <= 0)
                    return $"{Result.Count} categories in result";
                return $"{EnabledCategoryCount} categories · {EnabledOptionCount} options enabled";
            }
        }

        /// <summary>True if this entry was migrated from v1.0 (no option count data).</summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public bool IsMigrated => EnabledOptionCount <= 0 && EnabledCategoryCount <= 0;
    }

    /// <summary>Root model saved to history.json.</summary>
    public class HistoryData
    {
        public List<HistoryEntry> History { get; set; } = new();
    }


    // ════════════════════════════════════════════════════════════════════════════
    // PRESET MODELS — saved to presets.json
    // ════════════════════════════════════════════════════════════════════════════

    /// <summary>Saved enable/disable state for a single option within a preset.</summary>
    public class PresetOptionState
    {
        public string OptionName { get; set; } = "";
        public bool   IsEnabled  { get; set; } = true;
    }

    /// <summary>Saved enable/disable state for a single category within a preset.</summary>
    public class PresetCategoryState
    {
        public string                   CategoryName { get; set; } = "";
        public bool                     IsEnabled    { get; set; } = true;
        public List<PresetOptionState>  Options      { get; set; } = new();
    }

    /// <summary>Saved enable/disable state for a single collection within a preset.</summary>
    public class PresetCollectionState
    {
        public string                     CollectionName { get; set; } = "";
        public bool                       IsEnabled      { get; set; } = true;
        public List<PresetCategoryState>  Categories     { get; set; } = new();
    }

    /// <summary>A named preset capturing the full enable/disable state of all collections,
    /// categories and options at the time it was saved.</summary>
    public class Preset
    {
        public string                       Name        { get; set; } = "";
        public List<PresetCollectionState>  Collections { get; set; } = new();

        // Display-only summary counts — not saved, computed on load
        [System.Text.Json.Serialization.JsonIgnore]
        public int EnabledCollectionCount =>
            Collections.Count(c => c.IsEnabled);

        [System.Text.Json.Serialization.JsonIgnore]
        public int EnabledCategoryCount =>
            Collections.SelectMany(c => c.Categories).Count(cat => cat.IsEnabled);

        [System.Text.Json.Serialization.JsonIgnore]
        public int EnabledOptionCount =>
            Collections.SelectMany(c => c.Categories)
                       .SelectMany(cat => cat.Options)
                       .Count(o => o.IsEnabled);

        [System.Text.Json.Serialization.JsonIgnore]
        public string Summary =>
            $"{EnabledCollectionCount} collections · {EnabledCategoryCount} categories · {EnabledOptionCount} options enabled";
    }

    /// <summary>Root model saved to presets.json.</summary>
    public class PresetsData
    {
        public List<Preset> Presets { get; set; } = new();
    }


    // ════════════════════════════════════════════════════════════════════════════
    // SETTINGS MODEL — saved to settings.json
    // ════════════════════════════════════════════════════════════════════════════

    /// <summary>All user preferences. Saved to settings.json.</summary>
    public class SettingsData
    {
        // Theme and font
        public AppTheme     Theme     { get; set; } = AppTheme.System;
        public FontSizeScale FontSize { get; set; } = FontSizeScale.Normal;

        // Window geometry
        public double WindowWidth   { get; set; } = 1050;
        public double WindowHeight  { get; set; } = 700;
        public double WindowLeft    { get; set; } = -1;
        public double WindowTop     { get; set; } = -1;
        public double SidebarWidth  { get; set; } = 280;

        // History
        public int  HistoryLimit    { get; set; } = 500;  // -1 = unlimited
        public bool ConfirmOnDelete { get; set; } = true;

        // Last generated result — stored in settings for main screen restore
        public List<ResultPair> LastResult     { get; set; } = new();
        public DateTime?        LastResultTime { get; set; }

        // Migration flag
        public bool MigrationComplete { get; set; } = false;
    }


    // ════════════════════════════════════════════════════════════════════════════
    // HELPERS
    // ════════════════════════════════════════════════════════════════════════════

    /// <summary>Assigns cycling dot colors to history entries at display time. Not saved.</summary>
    public static class HistoryColorHelper
    {
        private static readonly string[] Palette =
            { "#60aaff", "#f0a030", "#4caf50", "#e05050", "#c060ff" };

        public static void AssignColors(IEnumerable<HistoryEntry> entries)
        {
            int i = 0;
            foreach (var e in entries)
                e.DotColorHex = Palette[i++ % Palette.Length];
        }
    }

    /// <summary>Returns the font scale multiplier for a given FontSizeScale setting.</summary>
    public static class FontScaleHelper
    {
        public static double GetScale(FontSizeScale scale) => scale switch
        {
            FontSizeScale.Normal     => 1.0,
            FontSizeScale.Large      => 1.3,
            FontSizeScale.ExtraLarge => 1.7,
            _                        => 1.0
        };
    }

    /// <summary>Validates names for collections, categories, options and presets.
    /// Blocks duplicates within scope and disallowed characters.</summary>
    public static class NameValidator
    {
        // Characters that could cause issues in JSON or display
        private static readonly char[] BlockedChars = { '"', '\'', '\\', '/', '<', '>', '\0' };

        public static string Sanitize(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "";
            foreach (var c in BlockedChars)
                input = input.Replace(c.ToString(), "");
            return input.Trim();
        }

        public static bool IsValid(string input) =>
            !string.IsNullOrWhiteSpace(input) &&
            !input.Any(c => BlockedChars.Contains(c));

        public static bool IsDuplicateCollection(string name, IEnumerable<RandomCollection> existing, string? excludeName = null) =>
            existing.Any(c => c.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase)
                           && c.Name != excludeName);

        public static bool IsDuplicateCategory(string name, IEnumerable<RandomCategory> existing, string? excludeName = null) =>
            existing.Any(c => c.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase)
                           && c.Name != excludeName);

        public static bool IsDuplicateOption(string name, IEnumerable<RandomOption> existing, string? excludeName = null) =>
            existing.Any(o => o.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase)
                           && o.Name != excludeName);

        public static bool IsDuplicatePreset(string name, IEnumerable<Preset> existing, string? excludeName = null) =>
            existing.Any(p => p.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase)
                           && p.Name != excludeName);
    }


    // ════════════════════════════════════════════════════════════════════════════
    // VIEW MODELS
    // ════════════════════════════════════════════════════════════════════════════

    /// <summary>ViewModel for a collection row in the Manage Content sidebar top zone.</summary>
    public class CollectionViewModel : INotifyPropertyChanged
    {
        public RandomCollection Model      { get; }
        public bool             IsSelected { get; }

        public CollectionViewModel(RandomCollection model, bool isSelected)
        {
            Model      = model;
            IsSelected = isSelected;
        }

        public string Name          => Model.Name;
        public int    CategoryCount => Model.Categories.Count;
        public int    OptionCount   => Model.Categories.Sum(c => c.Options.Count);

        // Selected row styling — uses theme resources so light/dark both work
        public Brush RowBackground => IsSelected
            ? (Brush)System.Windows.Application.Current.Resources["BackgroundCardBrush"]
            : Brushes.Transparent;

        public Brush RowBorderBrush => IsSelected
            ? (Brush)System.Windows.Application.Current.Resources["BorderSelectedBrush"]
            : Brushes.Transparent;

        public CornerRadius RowCornerRadius =>
            IsSelected ? new CornerRadius(8) : new CornerRadius(8);

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string n) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    /// <summary>ViewModel for a category row in the Manage Content sidebar bottom zone.</summary>
    public class CategoryViewModel : INotifyPropertyChanged
    {
        public RandomCategory   Model      { get; }
        public RandomCollection Collection { get; }
        public bool             IsSelected { get; }

        public CategoryViewModel(RandomCategory model, RandomCollection collection, bool isSelected)
        {
            Model      = model;
            Collection = collection;
            IsSelected = isSelected;
        }

        public string Name        => Model.Name;
        public int    OptionCount => Model.Options.Count;

        public bool IsEnabled
        {
            get => Model.IsEnabled;
            set { Model.IsEnabled = value; OnPropertyChanged(nameof(IsEnabled)); }
        }

        // Row opacity — faded when disabled
        public double RowOpacity => Model.IsEnabled ? 1.0 : 0.35;

        public string     RenameLabel      => $"Rename or delete {Name}";
        public Visibility SubRowVisibility => IsSelected ? Visibility.Visible : Visibility.Collapsed;

        public CornerRadius RowCornerRadius =>
            IsSelected ? new CornerRadius(8, 8, 0, 0) : new CornerRadius(8);

        public Brush RowBackground => IsSelected
            ? (Brush)System.Windows.Application.Current.Resources["BackgroundCardBrush"]
            : Brushes.Transparent;

        public Brush RowBorderBrush => IsSelected
            ? (Brush)System.Windows.Application.Current.Resources["BorderSelectedBrush"]
            : Brushes.Transparent;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string n) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    /// <summary>ViewModel for an option row in the Manage Content right panel.
    /// Computes live percentage based on sibling options in the same category.</summary>
    public class OptionViewModel : INotifyPropertyChanged
    {
        public RandomOption   Model    { get; }
        public RandomCategory Category { get; }

        // All sibling options needed for percentage calculation
        private readonly List<RandomOption> _siblings;

        public OptionViewModel(RandomOption model, RandomCategory category)
        {
            Model     = model;
            Category  = category;
            _siblings = category.Options;
        }

        public string Name        => Model.Name;
        public string WeightLabel => Model.Weight.ToString();

        public bool IsEnabled
        {
            get => Model.IsEnabled;
            set
            {
                Model.IsEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
                OnPropertyChanged(nameof(RowOpacity));
                OnPropertyChanged(nameof(PercentageDisplay));
            }
        }

        // Row opacity — faded when disabled
        public double RowOpacity => Model.IsEnabled ? 1.0 : 0.35;

        // ── Percentage calculation ───────────────────────────────────────────

        private int GetWeight(RandomOption opt) => opt.Weight switch
        {
            WeightTier.Normal => 3,
            WeightTier.Low    => 2,
            WeightTier.Rare   => 1,
            _                 => 3
        };

        /// <summary>Live calculated probability as a whole number percentage string.</summary>
        public string PercentageDisplay
        {
            get
            {
                if (!Model.IsEnabled) return "0%";

                var enabledOptions = _siblings.Where(o => o.IsEnabled).ToList();
                if (enabledOptions.Count == 0) return "0%";
                if (enabledOptions.Count == 1) return "100%";

                int totalPool = enabledOptions.Sum(GetWeight);
                if (totalPool == 0) return "0%";

                int myWeight = GetWeight(Model);
                int pct = (int)Math.Round((double)myWeight / totalPool * 100);
                return $"{pct}%";
            }
        }

        // ── Weight badge colors ──────────────────────────────────────────────

        public Brush WeightBackground => Model.Weight switch
        {
            WeightTier.Normal => new SolidColorBrush(Color.FromRgb(0x1e, 0x3a, 0x1e)),
            WeightTier.Low    => new SolidColorBrush(Color.FromRgb(0x3a, 0x2e, 0x10)),
            WeightTier.Rare   => new SolidColorBrush(Color.FromRgb(0x3a, 0x1a, 0x1a)),
            _                 => Brushes.Transparent
        };

        public Brush WeightForeground => Model.Weight switch
        {
            WeightTier.Normal => new SolidColorBrush(Color.FromRgb(0x4c, 0xaf, 0x50)),
            WeightTier.Low    => new SolidColorBrush(Color.FromRgb(0xf0, 0xa0, 0x30)),
            WeightTier.Rare   => new SolidColorBrush(Color.FromRgb(0xe0, 0x50, 0x50)),
            _                 => Brushes.White
        };

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string n) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    /// <summary>Display info for one collection in an expanded preset row.
    /// Only enabled collections are included.</summary>
    public class PresetCollectionDisplayInfo
    {
        public string CollectionName  { get; set; } = "";
        public bool   IsEnabled       { get; set; } = true;
        public int    EnabledCats     { get; set; }
        public int    TotalCats       { get; set; }
        public int    EnabledOptions  { get; set; }
        public int    TotalOptions    { get; set; }

        public string StatsLabel =>
            $"{EnabledCats}/{TotalCats} categories · {EnabledOptions}/{TotalOptions} options";

        public double RowOpacity => 1.0;
    }

    /// <summary>ViewModel for a preset row on the Presets screen.</summary>
    public class PresetViewModel : INotifyPropertyChanged
    {
        public Preset Model      { get; }
        public bool   IsExpanded { get; private set; }

        public PresetViewModel(Preset model) => Model = model;

        public string Name    => Model.Name;
        public string Summary => Model.Summary;

        public void ToggleExpanded()
        {
            IsExpanded = !IsExpanded;
            OnPropertyChanged(nameof(IsExpanded));
            OnPropertyChanged(nameof(ExpandedVisibility));
            OnPropertyChanged(nameof(Chevron));
        }

        public Visibility ExpandedVisibility =>
            IsExpanded ? Visibility.Visible : Visibility.Collapsed;

        public string Chevron => IsExpanded ? "▼" : "▶";

        /// <summary>Per-collection breakdown for the expanded view — enabled collections only.</summary>
        public List<PresetCollectionDisplayInfo> CollectionDisplayInfo =>
            Model.Collections
                .Where(c => c.IsEnabled)
                .Select(c => new PresetCollectionDisplayInfo
                {
                    CollectionName = c.CollectionName,
                    IsEnabled      = c.IsEnabled,
                    EnabledCats    = c.Categories.Count(cat => cat.IsEnabled),
                    TotalCats      = c.Categories.Count,
                    EnabledOptions = c.Categories.SelectMany(cat => cat.Options).Count(o => o.IsEnabled),
                    TotalOptions   = c.Categories.SelectMany(cat => cat.Options).Count()
                }).ToList();

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string n) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    /// <summary>ViewModel for a collection row in the Collections Management screen.</summary>
    public class CollectionManagementViewModel : INotifyPropertyChanged
    {
        public RandomCollection Model { get; }

        public CollectionManagementViewModel(RandomCollection model) => Model = model;

        public string Name          => Model.Name;
        public int    CategoryCount => Model.Categories.Count;
        public int    OptionCount   => Model.Categories.Sum(c => c.Options.Count);
        public string StatsLabel    => $"{CategoryCount} categories · {OptionCount} options";

        public bool IsEnabled
        {
            get => Model.IsEnabled;
            set { Model.IsEnabled = value; OnPropertyChanged(nameof(IsEnabled)); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string n) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
