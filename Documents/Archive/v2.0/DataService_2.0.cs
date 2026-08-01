using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;

namespace RandomCreation
{
    /// <summary>
    /// Handles all data persistence for Random Creation v2.0.
    /// Manages four separate JSON files in a 'data' subfolder next to the exe.
    /// Also handles one-time migration from v1.0 creature_crafter_data.json.
    /// </summary>
    public static class DataService
    {
        // ── Paths ────────────────────────────────────────────────────────────

        private static readonly string BaseDir = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string DataDir = Path.Combine(BaseDir, "data");

        private static readonly string SettingsPath    = Path.Combine(DataDir, "settings.json");
        private static readonly string CategoriesPath  = Path.Combine(DataDir, "categories.json");
        private static readonly string HistoryPath     = Path.Combine(DataDir, "history.json");
        private static readonly string PresetsPath     = Path.Combine(DataDir, "presets.json");
        private static readonly string ChangelogPath   = Path.Combine(DataDir, "changelog.txt");

        // v1.0 legacy file path
        private static readonly string LegacyPath      = Path.Combine(BaseDir, "creature_crafter_data.json");
        private static readonly string LegacyBackupPath = Path.Combine(BaseDir, "creature_crafter_data.json.bak");

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };


        // ── Public data instances ────────────────────────────────────────────

        public static SettingsData    Settings   { get; private set; } = new();
        public static CategoriesData  Categories { get; private set; } = new();
        public static HistoryData     History    { get; private set; } = new();
        public static PresetsData     Presets    { get; private set; } = new();

        /// <summary>Set to true after a successful v1.0 migration this session,
        /// so the main window can show the one-time migration popup.</summary>
        public static bool MigrationJustCompleted { get; private set; } = false;


        // ── Initialise ───────────────────────────────────────────────────────

        /// <summary>
        /// Called once on app startup — before any UI is shown.
        /// 1. Checks for v1.0 legacy file and migrates if needed.
        /// 2. Loads all four JSON files.
        /// 3. Returns immediately so the caller can apply theme/font before rendering.
        /// </summary>
        public static void Initialise()
        {
            EnsureDataDir();
            CheckAndMigrateLegacy();
            LoadAll();
        }


        // ── Migration ────────────────────────────────────────────────────────

        private static void CheckAndMigrateLegacy()
        {
            // If migration already done (flag in settings) skip entirely
            if (File.Exists(SettingsPath))
            {
                try
                {
                    var s = JsonSerializer.Deserialize<SettingsData>(
                        File.ReadAllText(SettingsPath), JsonOpts);
                    if (s != null && s.MigrationComplete) return;
                }
                catch { /* corrupt settings — proceed to migrate safely */ }
            }

            // Check for legacy file
            if (!File.Exists(LegacyPath)) return;

            try
            {
                MigrateLegacyData();
                MigrationJustCompleted = true;

                // Rename legacy file to .bak
                try
                {
                    if (File.Exists(LegacyBackupPath)) File.Delete(LegacyBackupPath);
                    File.Move(LegacyPath, LegacyBackupPath);
                }
                catch
                {
                    // Rename failed — write migration flag to settings as fallback
                    // so we don't migrate again next launch
                    Settings.MigrationComplete = true;
                    SaveSettings();
                }
            }
            catch
            {
                // Migration failed entirely — start fresh, don't crash
            }
        }

        private static void MigrateLegacyData()
        {
            var json   = File.ReadAllText(LegacyPath);
            var legacy = JsonSerializer.Deserialize<LegacyAppData>(json, JsonOpts);
            if (legacy == null) return;

            // ── Build CategoriesData ─────────────────────────────────────────
            var myCollection = new RandomCollection { Name = "My Collection", IsEnabled = true };

            foreach (var cat in legacy.Categories ?? new())
            {
                var newCat = new RandomCategory
                {
                    Name      = cat.Name,
                    IsEnabled = cat.IsEnabled
                };
                foreach (var opt in cat.Options ?? new())
                {
                    newCat.Options.Add(new RandomOption
                    {
                        Name      = opt.Name,
                        Weight    = opt.Weight,
                        IsEnabled = true
                    });
                }
                myCollection.Categories.Add(newCat);
            }

            Categories = new CategoriesData();
            Categories.Collections.Add(myCollection);
            SaveCategories();

            // ── Build HistoryData ────────────────────────────────────────────
            History = new HistoryData();

            foreach (var entry in legacy.History ?? new())
            {
                var newEntry = new HistoryEntry
                {
                    Timestamp             = entry.Timestamp,
                    ActiveCollections     = new List<string> { "My Collection" },
                    EnabledCategoryCount  = 0,   // not available from v1.0
                    EnabledOptionCount    = 0    // not available from v1.0
                };
                foreach (var pair in entry.Result ?? new())
                {
                    newEntry.Result.Add(new ResultPair
                    {
                        Category = pair.Category,
                        Option   = pair.Option,
                        IsDimmed = false
                    });
                }
                History.History.Add(newEntry);
            }
            SaveHistory();

            // ── Build SettingsData ───────────────────────────────────────────
            Settings = new SettingsData
            {
                MigrationComplete = true,
                WindowWidth       = legacy.WindowWidth  >= 800  ? legacy.WindowWidth  : 1050,
                WindowHeight      = legacy.WindowHeight >= 600  ? legacy.WindowHeight : 700,
                WindowLeft        = legacy.WindowLeft,
                WindowTop         = legacy.WindowTop,
                LastResult        = legacy.LastResult?.Select(p => new ResultPair
                {
                    Category = p.Category,
                    Option   = p.Option,
                    IsDimmed = false
                }).ToList() ?? new(),
                LastResultTime = legacy.LastResultTime
            };
            SaveSettings();

            // ── Build PresetsData ────────────────────────────────────────────
            Presets = new PresetsData();
            SavePresets();
        }


        // ── Load ─────────────────────────────────────────────────────────────

        private static void LoadAll()
        {
            Settings   = Load<SettingsData>(SettingsPath)   ?? new SettingsData();
            Categories = Load<CategoriesData>(CategoriesPath) ?? new CategoriesData();
            History    = Load<HistoryData>(HistoryPath)      ?? new HistoryData();
            Presets    = Load<PresetsData>(PresetsPath)      ?? new PresetsData();

            // First launch with no data — create default collection
            if (Categories.Collections.Count == 0)
            {
                Categories.Collections.Add(new RandomCollection
                {
                    Name      = "My Collection",
                    IsEnabled = true
                });
                SaveCategories();
            }
        }

        private static T? Load<T>(string path) where T : class
        {
            if (!File.Exists(path)) return null;
            try
            {
                return JsonSerializer.Deserialize<T>(File.ReadAllText(path), JsonOpts);
            }
            catch { return null; }
        }


        // ── Save ─────────────────────────────────────────────────────────────

        public static void SaveSettings()   => Write(SettingsPath,   Settings);
        public static void SaveCategories() => Write(CategoriesPath, Categories);
        public static void SaveHistory()    => Write(HistoryPath,    History);
        public static void SavePresets()    => Write(PresetsPath,    Presets);

        public static void SaveAll()
        {
            SaveSettings();
            SaveCategories();
            SaveHistory();
            SavePresets();
        }

        private static void Write(string path, object data)
        {
            try
            {
                EnsureDataDir();
                File.WriteAllText(path, JsonSerializer.Serialize(data, JsonOpts));
            }
            catch { /* Swallow write errors gracefully — don't crash the app */ }
        }

        private static void EnsureDataDir()
        {
            if (!Directory.Exists(DataDir))
                Directory.CreateDirectory(DataDir);
        }


        // ── History helpers ──────────────────────────────────────────────────

        /// <summary>Adds a new history entry, enforcing the history limit,
        /// then saves history.json.</summary>
        public static void AddHistoryEntry(HistoryEntry entry)
        {
            History.History.Add(entry);
            EnforceHistoryLimit();
            SaveHistory();
        }

        /// <summary>Removes a single history entry by reference, then saves.</summary>
        public static void DeleteHistoryEntry(HistoryEntry entry)
        {
            History.History.Remove(entry);
            SaveHistory();
        }

        /// <summary>Clears all history, then saves.</summary>
        public static void ClearAllHistory()
        {
            History.History.Clear();
            SaveHistory();
        }

        private static void EnforceHistoryLimit()
        {
            int limit = Settings.HistoryLimit;
            if (limit <= 0) return; // -1 = unlimited
            while (History.History.Count > limit)
                History.History.RemoveAt(0); // remove oldest
        }


        // ── Preset helpers ───────────────────────────────────────────────────

        /// <summary>Captures current enabled/disabled state of all collections,
        /// categories and options into a new Preset object.</summary>
        public static Preset CaptureCurrentStateAsPreset(string name)
        {
            var preset = new Preset { Name = name };

            foreach (var col in Categories.Collections)
            {
                var colState = new PresetCollectionState
                {
                    CollectionName = col.Name,
                    IsEnabled      = col.IsEnabled
                };
                foreach (var cat in col.Categories)
                {
                    var catState = new PresetCategoryState
                    {
                        CategoryName = cat.Name,
                        IsEnabled    = cat.IsEnabled
                    };
                    foreach (var opt in cat.Options)
                    {
                        catState.Options.Add(new PresetOptionState
                        {
                            OptionName = opt.Name,
                            IsEnabled  = opt.IsEnabled
                        });
                    }
                    colState.Categories.Add(catState);
                }
                preset.Collections.Add(colState);
            }
            return preset;
        }

        /// <summary>Applies a preset to current categories data, matching by name.
        /// Unmatched items are left unchanged. Saves categories.json after applying.</summary>
        public static void ApplyPreset(Preset preset)
        {
            foreach (var colState in preset.Collections)
            {
                var col = Categories.Collections
                    .FirstOrDefault(c => c.Name == colState.CollectionName);
                if (col == null) continue;

                col.IsEnabled = colState.IsEnabled;

                foreach (var catState in colState.Categories)
                {
                    var cat = col.Categories
                        .FirstOrDefault(c => c.Name == catState.CategoryName);
                    if (cat == null) continue;

                    cat.IsEnabled = catState.IsEnabled;

                    foreach (var optState in catState.Options)
                    {
                        var opt = cat.Options
                            .FirstOrDefault(o => o.Name == optState.OptionName);
                        if (opt == null) continue;
                        opt.IsEnabled = optState.IsEnabled;
                    }
                }
            }
            SaveCategories();
        }


        // ── Window geometry helpers ──────────────────────────────────────────

        public static void SaveWindowGeometry(double width, double height, double left, double top)
        {
            Settings.WindowWidth  = width;
            Settings.WindowHeight = height;
            Settings.WindowLeft   = left;
            Settings.WindowTop    = top;
            SaveSettings();
        }

        public static void SaveSidebarWidth(double width)
        {
            Settings.SidebarWidth = width;
            SaveSettings();
        }

        public static void ResetWindowGeometry()
        {
            Settings.WindowWidth  = 1050;
            Settings.WindowHeight = 700;
            Settings.WindowLeft   = -1;
            Settings.WindowTop    = -1;
            Settings.SidebarWidth = 280;
            SaveSettings();
        }


        // ── Changelog ────────────────────────────────────────────────────────

        private const string FallbackChangelog =
            "Version 2.0 — May 2026\n" +
            "See changelog.txt in the data folder for full release notes.\n\n" +
            "Version 1.0 — 2026\n" +
            "Initial release.";

        /// <summary>
        /// Reads changelog.txt from the data folder.
        /// Creates the file with default content on first run if missing.
        /// Falls back to a minimal hardcoded string if the file can't be read.
        /// </summary>
        public static string ReadChangelog()
        {
            EnsureDataDir();

            // Create default changelog if it doesn't exist yet
            if (!File.Exists(ChangelogPath))
            {
                try { File.WriteAllText(ChangelogPath, DefaultChangelogContent); }
                catch { /* If we can't write, just return fallback */ }
            }

            try   { return File.ReadAllText(ChangelogPath); }
            catch { return FallbackChangelog; }
        }

        private const string DefaultChangelogContent =
@"================================================================================
Random Creation — Changelog
================================================================================


Version 2.0 — May 2026
================================================================================

Collections
--------------------------------------------------------------------------------
Added Collections as a new top-level container for organizing categories. You
can create as many collections as you want — Creatures, Starships, Guns, etc.
Each collection can be enabled or disabled independently. Only enabled collections
appear in the sidebar when managing content, and only enabled collections are
included when generating a result. Collections can be reordered via drag and drop.
All existing categories from v1.0 are automatically migrated into a collection
called ""My Collection"" on first launch.

Manage Content Screen
--------------------------------------------------------------------------------
Replaced the v1.0 Edit Categories screen with a new Manage Content screen. The
left sidebar now has two zones — the top zone shows your collections, the bottom
zone shows the categories belonging to whichever collection is selected. The right
panel shows the options for whichever category is selected. A gear icon in the
collections zone opens the Collections Management screen where you can add, rename,
delete, reorder and enable/disable your collections.

Presets
--------------------------------------------------------------------------------
Added a Presets screen accessible via the bookmark icon in the Manage Content title
bar. Presets let you save and instantly restore named snapshots of your current
enabled/disabled state — down to the individual option level. Each preset row shows
a summary of how many collections, categories and options are enabled. Rows expand
to show a per-collection breakdown. Save your current state as a new preset or
overwrite an existing one from the same dialog. Loading a preset asks for
confirmation before applying. Presets can be renamed, deleted and reordered via
drag and drop. Saved to data/presets.json.

Themes
--------------------------------------------------------------------------------
Added dark theme, light theme and System Default which follows your Windows setting.
The active theme can be changed in Settings or toggled instantly using the sun/moon
icon in the main title bar. All screens, dialogs and controls respond to the active
theme including the ON/OFF pills, weight badges, hover states and scrollbars.

Per-option enable/disable and probability
--------------------------------------------------------------------------------
Individual options can now be toggled on or off independently of their weight tier.
A live percentage column on each option row shows the exact probability of that
option being selected during generation. The percentage updates instantly as you
change weights or toggle options on and off. Disabled options always show 0% and
are excluded from the weighted pool entirely.

Result card dimming
--------------------------------------------------------------------------------
Clicking a result card on the main screen dims it to about 35% opacity, letting
you mark individual results as ignored for your current reference or drawing session.
Clicking a dimmed card restores it. The dim state is saved per history entry so past
results show exactly what was dimmed at the time. Generating a new result clears all
dims on the current result before saving it to history.

Drag and drop — rebuilt
--------------------------------------------------------------------------------
Drag and drop has been completely rebuilt for all lists in the app. Dragging now
shows a semi-transparent wireframe ghost of the item following your cursor, and a
blue insertion line centered between items shows exactly where the item will land.
The ghost border turns red over invalid drop zones. Categories can be dragged from
one collection to another — the target collection row highlights blue. Options can
be dragged from one category to another — the target category row highlights blue.
All sub-items, weights and enable/disable states move with the dragged item.

History improvements
--------------------------------------------------------------------------------
History entries can now be deleted individually or all at once via Clear All in
Settings. History limit is configurable — default 500 entries or set to unlimited.

Settings screen
--------------------------------------------------------------------------------
Added a dedicated Settings screen: Theme, Font Size (Normal/Large/Extra Large with
live preview), History limit, Confirm on delete toggle, Reset window size, Keyboard
shortcuts reference, and Changelog loaded from data/changelog.txt.

Resizable sidebar
--------------------------------------------------------------------------------
The sidebar divider is now draggable. Width is saved between sessions.

Summary bar
--------------------------------------------------------------------------------
The main screen shows a summary bar listing enabled collections with active category
and option counts — a quick overview of what will be included before generating.

Custom borderless window
--------------------------------------------------------------------------------
The standard Windows title bar has been replaced with a custom borderless design
matching the active theme, including sun/moon theme toggle, history, settings and
standard window controls.

Data folder
--------------------------------------------------------------------------------
All app data is now stored in a data/ folder next to the exe:
  data/settings.json   — theme, font size, window geometry, sidebar width
  data/categories.json — all collections, categories and options
  data/history.json    — full generation history
  data/presets.json    — saved presets
  data/changelog.txt   — this file
Copying the data/ folder to another machine transfers your full setup.

Other changes
--------------------------------------------------------------------------------
- App renamed from Creature Crafter to Random Creation
- ON/OFF pill controls replace toggle switches throughout
- Bulk Enable All / Disable All for categories and options
- Empty state messages throughout
- Keyboard shortcuts: Escape (back), Space/Enter (generate), Ctrl+H, Ctrl+M
- Destructive confirms use red button, non-destructive use blue button


================================================================================


Version 1.0 — Early 2026
================================================================================

Initial release
--------------------------------------------------------------------------------
Categories and options with weighted randomization (Normal, Low, Rare). Generation
history with timestamps. Drag and drop reordering. Per-category enable/disable.
Result cards in a 5-column grid. Window geometry saved between sessions.
";
    }


    // ════════════════════════════════════════════════════════════════════════════
    // LEGACY MODELS — used only for v1.0 migration, not used at runtime
    // ════════════════════════════════════════════════════════════════════════════

    /// <summary>Mirrors the v1.0 AppData structure for deserialization during migration only.</summary>
    internal class LegacyAppData
    {
        public List<LegacyCategory> Categories    { get; set; } = new();
        public List<LegacyResultPair> LastResult  { get; set; } = new();
        public DateTime?            LastResultTime { get; set; }
        public List<LegacyHistoryEntry> History   { get; set; } = new();
        public double WindowWidth                  { get; set; } = 1050;
        public double WindowHeight                 { get; set; } = 700;
        public double WindowLeft                   { get; set; } = -1;
        public double WindowTop                    { get; set; } = -1;
    }

    internal class LegacyCategory
    {
        public string              Name      { get; set; } = "";
        public bool                IsEnabled { get; set; } = true;
        public List<LegacyOption>  Options   { get; set; } = new();
    }

    internal class LegacyOption
    {
        public string    Name   { get; set; } = "";
        public WeightTier Weight { get; set; } = WeightTier.Normal;
    }

    internal class LegacyResultPair
    {
        public string Category { get; set; } = "";
        public string Option   { get; set; } = "";
    }

    internal class LegacyHistoryEntry
    {
        public DateTime              Timestamp { get; set; }
        public List<LegacyResultPair> Result   { get; set; } = new();
    }
}
