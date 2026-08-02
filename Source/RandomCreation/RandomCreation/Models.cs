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

    public enum WeightTier { UltraHigh, High, Normal, Low, Rare, UltraRare }

    public enum AppTheme { Dark, Light, System }

    public enum FontSizeScale { Normal, Large, ExtraLarge }

    /// <summary>Set when startup found data files this version cannot read
    /// (backed up with .bak, fresh start), so MainWindow shows a one-time notice.</summary>
    public enum MigrationKind { None, UnrecognisedData }


    // ════════════════════════════════════════════════════════════════════════════
    // CORE DATA MODELS — saved to categories.json
    // ════════════════════════════════════════════════════════════════════════════

    /// <summary>A single option within a category.</summary>
    public class Option
    {
        public string     Name      { get; set; } = "";
        public WeightTier Weight    { get; set; } = WeightTier.Normal;
        public bool       IsEnabled { get; set; } = true;
    }

    /// <summary>A category containing options, belonging to a group.</summary>
    public class Category
    {
        public string       Name      { get; set; } = "";
        public bool         IsEnabled { get; set; } = true;
        public List<Option> Options   { get; set; } = new();
    }

    /// <summary>A group of categories, belonging to a collection.</summary>
    public class CategoryGroup
    {
        public string           Name        { get; set; } = "";
        public bool             IsEnabled   { get; set; } = true;
        public List<Category>   Categories  { get; set; } = new();
    }

    /// <summary>A named collection of category groups. Top-level organizational unit.</summary>
    public class Collection
    {
        public string               Name      { get; set; } = "";
        public bool                 IsEnabled { get; set; } = true;
        public List<CategoryGroup>  Groups    { get; set; } = new();
    }

    /// <summary>Root model saved to categories.json.</summary>
    public class CategoriesData
    {
        public List<Collection> Collections { get; set; } = new();
    }


    // ════════════════════════════════════════════════════════════════════════════
    // RESULT & HISTORY MODELS — saved to history.json
    // ════════════════════════════════════════════════════════════════════════════

    /// <summary>One category/option pair in a generated result.
    /// GroupName is new in v3.0 — pre-v3.0 entries have GroupName = "".</summary>
    public class ResultPair : INotifyPropertyChanged
    {
        public string GroupName { get; set; } = "";   // v3.0 — group membership
        public string Category  { get; set; } = "";
        public string Option    { get; set; } = "";

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
        public DateTime         Timestamp            { get; set; }
        public List<ResultPair> Result               { get; set; } = new();

        // Generation context saved at time of generate
        public List<string> ActiveCollections     { get; set; } = new();
        public int          EnabledGroupCount     { get; set; } = 0;   // v3.0
        public int          EnabledCategoryCount  { get; set; } = 0;
        public int          EnabledOptionCount    { get; set; } = 0;

        // Drawn state — v3.0, replaces per-entry dot color
        public bool IsDrawn { get; set; } = false;

        // ── Computed display properties — not saved ──────────────────────────

        /// <summary>Option C format: "My Collection · 3 groups · 17 results"
        /// Falls back to flat option list for pre-v3.0 entries (GroupName == "").</summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public string Summary
        {
            get
            {
                // Pre-v3.0 entry — no group data, show flat option list
                if (Result.Count > 0 && Result.All(r => string.IsNullOrEmpty(r.GroupName)))
                    return string.Join(" · ", Result.Select(r => r.Option));

                string collectionPart = (ActiveCollections == null || ActiveCollections.Count == 0)
                    ? "My Collection"
                    : string.Join(", ", ActiveCollections);

                int groupCount  = Result
                    .Select(r => r.GroupName)
                    .Where(g => !string.IsNullOrEmpty(g))
                    .Distinct()
                    .Count();

                int resultCount = Result.Count;

                return $"{collectionPart} · {groupCount} group{(groupCount == 1 ? "" : "s")} · {resultCount} result{(resultCount == 1 ? "" : "s")}";
            }
        }

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
        public string                  CategoryName { get; set; } = "";
        public bool                    IsEnabled    { get; set; } = true;
        public List<PresetOptionState> Options      { get; set; } = new();
    }

    /// <summary>Saved enable/disable state for a single group within a preset.
    /// New in v3.0 — sits between collection and category.</summary>
    public class PresetGroupState
    {
        public string                    GroupName   { get; set; } = "";
        public bool                      IsEnabled   { get; set; } = true;
        public List<PresetCategoryState> Categories  { get; set; } = new();
    }

    /// <summary>Saved enable/disable state for a single collection within a preset.</summary>
    public class PresetCollectionState
    {
        public string                  CollectionName { get; set; } = "";
        public bool                    IsEnabled      { get; set; } = true;
        public List<PresetGroupState>  Groups         { get; set; } = new();
    }

    /// <summary>A named preset capturing the full enable/disable state of all collections,
    /// groups, categories and options at the time it was saved.</summary>
    public class Preset
    {
        public string                      Name        { get; set; } = "";
        public List<PresetCollectionState> Collections { get; set; } = new();

        // Display-only summary counts — not saved, computed on load
        [System.Text.Json.Serialization.JsonIgnore]
        public int EnabledCollectionCount =>
            Collections.Count(c => c.IsEnabled);

        [System.Text.Json.Serialization.JsonIgnore]
        public int EnabledGroupCount =>
            Collections.SelectMany(c => c.Groups).Count(g => g.IsEnabled);

        [System.Text.Json.Serialization.JsonIgnore]
        public int EnabledCategoryCount =>
            Collections.SelectMany(c => c.Groups)
                       .SelectMany(g => g.Categories)
                       .Count(cat => cat.IsEnabled);

        [System.Text.Json.Serialization.JsonIgnore]
        public int EnabledOptionCount =>
            Collections.SelectMany(c => c.Groups)
                       .SelectMany(g => g.Categories)
                       .SelectMany(cat => cat.Options)
                       .Count(o => o.IsEnabled);

        [System.Text.Json.Serialization.JsonIgnore]
        public string Summary =>
            $"{EnabledCollectionCount} collections · {EnabledGroupCount} groups · " +
            $"{EnabledCategoryCount} categories · {EnabledOptionCount} options enabled";
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
        // Schema version — single source of truth for migration
        public int SchemaVersion { get; set; } = 3;

        // Theme and font
        public AppTheme      Theme    { get; set; } = AppTheme.System;
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

        // Last export destination — restored in SaveFileDialog on next export
        public string? LastExportPath { get; set; } = null;
    }


    // ════════════════════════════════════════════════════════════════════════════
    // HELPERS
    // ════════════════════════════════════════════════════════════════════════════

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

    /// <summary>Validates names for collections, groups, categories, options and presets.
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

        public static bool IsDuplicateCollection(string name, IEnumerable<Collection> existing, string? excludeName = null) =>
            existing.Any(c => c.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase)
                           && c.Name != excludeName);

        public static bool IsDuplicateGroup(string name, IEnumerable<CategoryGroup> existing, string? excludeName = null) =>
            existing.Any(g => g.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase)
                           && g.Name != excludeName);

        public static bool IsDuplicateCategory(string name, IEnumerable<Category> existing, string? excludeName = null) =>
            existing.Any(c => c.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase)
                           && c.Name != excludeName);

        public static bool IsDuplicateOption(string name, IEnumerable<Option> existing, string? excludeName = null) =>
            existing.Any(o => o.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase)
                           && o.Name != excludeName);

        public static bool IsDuplicatePreset(string name, IEnumerable<Preset> existing, string? excludeName = null) =>
            existing.Any(p => p.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase)
                           && p.Name != excludeName);
    }


    // ════════════════════════════════════════════════════════════════════════════
    // VIEW MODELS
    // ════════════════════════════════════════════════════════════════════════════

    /// <summary>ViewModel for a collection row in the Manage Content sidebar.</summary>
    public class CollectionViewModel : INotifyPropertyChanged
    {
        public Collection Model      { get; }
        public bool       IsSelected { get; }

        public CollectionViewModel(Collection model, bool isSelected)
        {
            Model      = model;
            IsSelected = isSelected;
        }

        public string Name       => Model.Name;
        public int    GroupCount => Model.Groups.Count;
        public int    CategoryCount => Model.Groups.Sum(g => g.Categories.Count);
        public int    OptionCount   => Model.Groups.SelectMany(g => g.Categories).Sum(c => c.Options.Count);


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string n) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    /// <summary>ViewModel for a category row in the Manage Content right panel.</summary>
    public class CategoryViewModel : INotifyPropertyChanged
    {
        public Category       Model      { get; }
        public CategoryGroup  Group      { get; }
        public Collection     Collection { get; }
        public bool           IsSelected { get; }
        public bool           IsVisible  { get; }
        public bool           IsCut      { get; }  // dimmed during Ctrl+X cut

        public CategoryViewModel(Category model, CategoryGroup group, Collection collection, bool isSelected)
            : this(model, group, collection, isSelected, true) { }

        public CategoryViewModel(Category model, CategoryGroup group, Collection collection,
                                 bool isSelected, bool isVisible)
            : this(model, group, collection, isSelected, isVisible, false) { }

        public CategoryViewModel(Category model, CategoryGroup group, Collection collection,
                                 bool isSelected, bool isVisible, bool isCut)
        {
            Model      = model;
            Group      = group;
            Collection = collection;
            IsSelected = isSelected;
            IsVisible  = isVisible;
            IsCut      = isCut;
        }

        public string Name        => Model.Name;
        public int    OptionCount => Model.Options.Count;

        public bool IsEnabled
        {
            get => Model.IsEnabled;
            set { Model.IsEnabled = value; OnPropertyChanged(nameof(IsEnabled)); }
        }

        // Row opacity — faded when disabled, not matching search, or being cut
        public double RowOpacity => (IsCut || !Model.IsEnabled || !IsVisible) ? 0.35 : 1.0;


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string n) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    /// <summary>ViewModel for an option row in the Manage Content right panel.
    /// Computes live percentage based on sibling options in the same category.</summary>
    public class OptionViewModel : INotifyPropertyChanged
    {
        public Option    Model      { get; }
        public Category  Category   { get; }
        public bool      IsSelected { get; }
        public bool      IsCut      { get; }  // dimmed during Ctrl+X cut

        private readonly List<Option> _siblings;

        public OptionViewModel(Option model, Category category)
            : this(model, category, false) { }

        public OptionViewModel(Option model, Category category, bool isSelected)
            : this(model, category, isSelected, false) { }

        public OptionViewModel(Option model, Category category, bool isSelected, bool isCut)
        {
            Model      = model;
            Category   = category;
            IsSelected = isSelected;
            IsCut      = isCut;
            _siblings  = category.Options;
        }

        public string Name        => Model.Name;
        public string WeightLabel => Model.Weight switch
        {
            WeightTier.UltraHigh => "Ultra High",
            WeightTier.High      => "High",
            WeightTier.Normal    => "Normal",
            WeightTier.Low       => "Low",
            WeightTier.Rare      => "Rare",
            WeightTier.UltraRare => "Ultra Rare",
            _                    => "Normal"
        };

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

        // Row opacity — faded when disabled or being cut
        public double RowOpacity => (IsCut || !Model.IsEnabled) ? 0.35 : 1.0;

        // ── Percentage calculation ───────────────────────────────────────────

        private int GetWeight(Option opt) => opt.Weight switch
        {
            WeightTier.UltraHigh => 6,
            WeightTier.High      => 5,
            WeightTier.Normal    => 4,
            WeightTier.Low       => 2,
            WeightTier.Rare      => 1,
            WeightTier.UltraRare => 0,
            _                    => 4
        };

        /// <summary>Live calculated probability as a whole number percentage string.</summary>
        public string PercentageDisplay
        {
            get
            {
                if (!Model.IsEnabled) return "0%";
                var enabled = _siblings.Where(o => o.IsEnabled).ToList();
                if (enabled.Count == 0) return "0%";
                if (enabled.Count == 1) return "100%";

                bool hasUltraHigh = enabled.Any(o => o.Weight == WeightTier.UltraHigh);
                bool hasUltraRare = enabled.Any(o => o.Weight == WeightTier.UltraRare);
                var standard = enabled.Where(o =>
                    o.Weight != WeightTier.UltraHigh &&
                    o.Weight != WeightTier.UltraRare).ToList();

                // Fixed-rate tiers
                if (Model.Weight == WeightTier.UltraHigh) return "98%";
                if (Model.Weight == WeightTier.UltraRare) return "2%";

                // Standard options share the remaining % after fixed-rate tiers
                int reservedPct = (hasUltraHigh ? 98 : 0) + (hasUltraRare ? 2 : 0);
                int availablePct = 100 - reservedPct;
                if (availablePct <= 0 || standard.Count == 0) return "0%";

                int totalPool = standard.Sum(GetWeight);
                if (totalPool == 0) return "0%";

                int myWeight = GetWeight(Model);
                int pct = (int)Math.Round((double)myWeight / totalPool * availablePct);
                return $"{pct}%";
            }
        }

        // ── Weight badge colors ──────────────────────────────────────────────

        public Brush WeightBackground => Model.Weight switch
        {
            WeightTier.UltraHigh => new SolidColorBrush(Color.FromRgb(0x1a, 0x2a, 0x4a)),
            WeightTier.High      => new SolidColorBrush(Color.FromRgb(0x1a, 0x30, 0x40)),
            WeightTier.Normal    => new SolidColorBrush(Color.FromRgb(0x1e, 0x3a, 0x1e)),
            WeightTier.Low       => new SolidColorBrush(Color.FromRgb(0x3a, 0x2e, 0x10)),
            WeightTier.Rare      => new SolidColorBrush(Color.FromRgb(0x3a, 0x1a, 0x1a)),
            WeightTier.UltraRare => new SolidColorBrush(Color.FromRgb(0x2a, 0x1a, 0x3a)),
            _                    => Brushes.Transparent
        };

        public Brush WeightForeground => Model.Weight switch
        {
            WeightTier.UltraHigh => new SolidColorBrush(Color.FromRgb(0x60, 0xaa, 0xff)),
            WeightTier.High      => new SolidColorBrush(Color.FromRgb(0x40, 0xc0, 0xd0)),
            WeightTier.Normal    => new SolidColorBrush(Color.FromRgb(0x4c, 0xaf, 0x50)),
            WeightTier.Low       => new SolidColorBrush(Color.FromRgb(0xf0, 0xa0, 0x30)),
            WeightTier.Rare      => new SolidColorBrush(Color.FromRgb(0xe0, 0x50, 0x50)),
            WeightTier.UltraRare => new SolidColorBrush(Color.FromRgb(0xb0, 0x70, 0xe0)),
            _                    => Brushes.White
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
        public int    EnabledGroups   { get; set; }
        public int    TotalGroups     { get; set; }
        public int    EnabledCats     { get; set; }
        public int    TotalCats       { get; set; }
        public int    EnabledOptions  { get; set; }
        public int    TotalOptions    { get; set; }

        public string StatsLabel =>
            $"{EnabledGroups}/{TotalGroups} groups · {EnabledCats}/{TotalCats} categories · {EnabledOptions}/{TotalOptions} options";

        public double RowOpacity => 1.0;
    }

    /// <summary>ViewModel for a preset row on the Presets screen.</summary>
    public class PresetViewModel : INotifyPropertyChanged
    {
        public Preset Model      { get; }
        public bool   IsExpanded { get; private set; }
        public bool   IsSelected { get; set; }

        public PresetViewModel(Preset model, bool isSelected = false)
        {
            Model      = model;
            IsSelected = isSelected;
        }

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
                    EnabledGroups  = c.Groups.Count(g => g.IsEnabled),
                    TotalGroups    = c.Groups.Count,
                    EnabledCats    = c.Groups.SelectMany(g => g.Categories).Count(cat => cat.IsEnabled),
                    TotalCats      = c.Groups.SelectMany(g => g.Categories).Count(),
                    EnabledOptions = c.Groups.SelectMany(g => g.Categories).SelectMany(cat => cat.Options).Count(o => o.IsEnabled),
                    TotalOptions   = c.Groups.SelectMany(g => g.Categories).SelectMany(cat => cat.Options).Count()
                }).ToList();

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string n) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    /// <summary>ViewModel for a collection row in the Collections Management screen.</summary>
    public class CollectionManagementViewModel : INotifyPropertyChanged
    {
        public Collection Model      { get; }
        public bool       IsSelected { get; set; }

        public CollectionManagementViewModel(Collection model, bool isSelected = false)
        {
            Model      = model;
            IsSelected = isSelected;
        }

        public string Name          => Model.Name;
        public int    GroupCount    => Model.Groups.Count;
        public int    CategoryCount => Model.Groups.Sum(g => g.Categories.Count);
        public int    OptionCount   => Model.Groups.SelectMany(g => g.Categories).Sum(c => c.Options.Count);
        public string StatsLabel    => $"{GroupCount} groups · {CategoryCount} categories · {OptionCount} options";

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
