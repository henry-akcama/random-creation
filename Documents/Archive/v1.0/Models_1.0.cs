using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace CreatureCrafter
{
    // ── Weight tiers ────────────────────────────────────────────────────────
    public enum WeightTier { Normal, Low, Rare }

    // ── A single option within a category ───────────────────────────────────
    public class CreatureOption
    {
        public string Name { get; set; } = "";
        public WeightTier Weight { get; set; } = WeightTier.Normal;
    }

    // ── A category containing options ───────────────────────────────────────
    public class CreatureCategory
    {
        public string Name { get; set; } = "";
        public bool IsEnabled { get; set; } = true;
        public List<CreatureOption> Options { get; set; } = new();
    }

    // ── One category/option pair in a generated result ──────────────────────
    public class ResultPair
    {
        public string Category { get; set; } = "";
        public string Option { get; set; } = "";
    }

    // ── A single history entry ───────────────────────────────────────────────
    public class HistoryEntry
    {
        public DateTime Timestamp { get; set; }
        public List<ResultPair> Result { get; set; } = new();

        // Dot color stored separately so it doesn't pollute the save file
        // (assigned at display time, not persisted)
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
                if (Timestamp.Date == DateTime.Today) return Timestamp.ToString("h:mm tt");
                if (Timestamp.Date == DateTime.Today.AddDays(-1)) return "Yesterday";
                return Timestamp.ToString("MMM d");
            }
        }

        [System.Text.Json.Serialization.JsonIgnore]
        public string FullTimestamp => Timestamp.ToString("MMM d, yyyy  h:mm tt");
    }

    // ── Root data model saved to JSON ────────────────────────────────────────
    public class AppData
    {
        public List<CreatureCategory> Categories { get; set; } = new();
        public List<ResultPair> LastResult { get; set; } = new();
        public DateTime? LastResultTime { get; set; }
        public List<HistoryEntry> History { get; set; } = new();

        // Window geometry
        public double WindowWidth { get; set; } = 1050;
        public double WindowHeight { get; set; } = 700;
        public double WindowLeft { get; set; } = -1;
        public double WindowTop { get; set; } = -1;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>Assigns cycling dot colors to a list of entries (display only, not saved).</summary>
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

    // ── CategoryViewModel ────────────────────────────────────────────────────
    public class CategoryViewModel : INotifyPropertyChanged
    {
        public CreatureCategory Model { get; }
        public bool IsSelected { get; }

        public CategoryViewModel(CreatureCategory model, bool isSelected)
        {
            Model = model;
            IsSelected = isSelected;
        }

        public string Name        => Model.Name;
        public int    OptionCount => Model.Options.Count;

        public bool IsEnabled
        {
            get => Model.IsEnabled;
            set { Model.IsEnabled = value; OnPropertyChanged(nameof(IsEnabled)); }
        }

        public string     RenameLabel      => $"Rename or delete {Name}";
        public Visibility SubRowVisibility => IsSelected ? Visibility.Visible : Visibility.Collapsed;

        public CornerRadius RowCornerRadius =>
            IsSelected ? new CornerRadius(8, 8, 0, 0) : new CornerRadius(8);

        public Brush RowBackground => IsSelected
            ? new SolidColorBrush(Color.FromRgb(0x2a, 0x2a, 0x2e))
            : Brushes.Transparent;

        public Brush RowBorderBrush => IsSelected
            ? new SolidColorBrush(Color.FromRgb(0x4a, 0x4a, 0x4e))
            : Brushes.Transparent;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string n) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    // ── OptionViewModel ──────────────────────────────────────────────────────
    public class OptionViewModel
    {
        public CreatureOption   Model    { get; }
        public CreatureCategory Category { get; }

        public OptionViewModel(CreatureOption model, CreatureCategory category)
        { Model = model; Category = category; }

        public string Name        => Model.Name;
        public string WeightLabel => Model.Weight.ToString();

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
    }
}