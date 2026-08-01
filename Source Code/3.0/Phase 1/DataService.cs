using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;

namespace RandomCreation
{
    /// <summary>
    /// Handles all data persistence for Random Creation v3.0.
    /// Manages four separate JSON files in a 'data' subfolder next to the exe.
    /// Handles migration from v1.0 (creature_crafter_data.json) and v2.0 (flat categories).
    /// </summary>
    public static class DataService
    {
        // ── Paths ────────────────────────────────────────────────────────────

        private static readonly string BaseDir = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string DataDir = Path.Combine(BaseDir, "data");

        private static readonly string SettingsPath       = Path.Combine(DataDir, "settings.json");
        private static readonly string CategoriesPath     = Path.Combine(DataDir, "categories.json");
        private static readonly string HistoryPath        = Path.Combine(DataDir, "history.json");
        private static readonly string PresetsPath        = Path.Combine(DataDir, "presets.json");
        private static readonly string ChangelogPath      = Path.Combine(DataDir, "changelog.txt");
        private static readonly string HistoryBackupPath  = Path.Combine(DataDir, "history_backup.json");

        // v1.0 legacy file path
        private static readonly string LegacyPath       = Path.Combine(BaseDir, "creature_crafter_data.json");
        private static readonly string LegacyBackupPath = Path.Combine(BaseDir, "creature_crafter_data.json.bak");

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        private const int CurrentSchemaVersion = 3;


        // ── Public data instances ────────────────────────────────────────────

        public static SettingsData   Settings   { get; private set; } = new();
        public static CategoriesData Categories { get; private set; } = new();
        public static HistoryData    History    { get; private set; } = new();
        public static PresetsData    Presets    { get; private set; } = new();

        /// <summary>Set during Initialise() to identify which migration ran this session,
        /// so MainWindow can show the appropriate one-time dialog.</summary>
        public static MigrationKind MigrationKind { get; private set; } = MigrationKind.None;

        /// <summary>The raw history.json text captured before v2→v3 migration clears it,
        /// so the user can optionally save it as a backup.</summary>
        public static string? PreMigrationHistoryJson { get; private set; } = null;


        // ── Initialise ───────────────────────────────────────────────────────

        /// <summary>
        /// Called once on app startup — before any UI is shown.
        /// 1. Reads SchemaVersion from settings.json to determine migration path.
        /// 2. Runs the appropriate migration (or none if already v3.0).
        /// 3. Loads all four JSON files.
        /// </summary>
        public static void Initialise()
        {
            EnsureDataDir();

            int version = ReadSchemaVersion();

            if (version == CurrentSchemaVersion)
            {
                // Already v3.0 — load normally
                LoadAll();
                return;
            }

            if (version == 0)
            {
                // Could be v1.0 or v2.0 — distinguish by presence of legacy file
                if (File.Exists(LegacyPath) && !Directory.Exists(DataDir))
                    RunV1ToV3Migration();
                else
                    RunV2ToV3Migration();
            }
            else
            {
                // version > 3 or some unknown value — future version or corrupt
                RunUnknownVersionMigration();
            }

            LoadAll();
        }


        // ── Schema version detection ─────────────────────────────────────────

        /// <summary>Reads only the SchemaVersion field from settings.json without
        /// deserializing the full object. Returns 0 if absent or unreadable.</summary>
        private static int ReadSchemaVersion()
        {
            if (!File.Exists(SettingsPath)) return 0;

            try
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(SettingsPath));
                if (doc.RootElement.TryGetProperty("SchemaVersion", out var prop))
                    return prop.GetInt32();

                // File exists but has no SchemaVersion — v2.0 file
                return 0;
            }
            catch
            {
                return 0;
            }
        }


        // ── Migration: v1.0 → v3.0 ──────────────────────────────────────────

        /// <summary>Triggered when creature_crafter_data.json exists and no data/ folder exists.
        /// The old format is incompatible — rename to .bak, start fresh.</summary>
        private static void RunV1ToV3Migration()
        {
            try
            {
                EnsureDataDir();

                // Rename legacy file to .bak
                try
                {
                    if (File.Exists(LegacyBackupPath)) File.Delete(LegacyBackupPath);
                    File.Move(LegacyPath, LegacyBackupPath);
                }
                catch { /* Rename failed — not fatal */ }

                // Write fresh default data
                WriteDefaultData();

                MigrationKind = MigrationKind.V1ToV3;
            }
            catch
            {
                // Migration failed — start fresh silently
                WriteDefaultData();
            }
        }


        // ── Migration: v2.0 → v3.0 ──────────────────────────────────────────

        /// <summary>Triggered when settings.json exists but SchemaVersion is 0 or absent.
        /// Wraps each collection's flat Categories list in one CategoryGroup named after
        /// the collection. Clears history and presets (incompatible with new structure).</summary>
        private static void RunV2ToV3Migration()
        {
            try
            {
                // ── 1. Load existing v2.0 categories ────────────────────────
                V2CategoriesData? v2cats = null;
                if (File.Exists(CategoriesPath))
                {
                    try { v2cats = JsonSerializer.Deserialize<V2CategoriesData>(File.ReadAllText(CategoriesPath), JsonOpts); }
                    catch { /* Corrupt — treat as empty */ }
                }

                // ── 2. Capture history.json text for optional backup ─────────
                if (File.Exists(HistoryPath))
                {
                    try { PreMigrationHistoryJson = File.ReadAllText(HistoryPath); }
                    catch { /* Not readable — no backup available */ }
                }

                // ── 3. Load existing settings to preserve user preferences ───
                SettingsData? oldSettings = null;
                if (File.Exists(SettingsPath))
                {
                    try { oldSettings = JsonSerializer.Deserialize<SettingsData>(File.ReadAllText(SettingsPath), JsonOpts); }
                    catch { /* Corrupt — use defaults */ }
                }

                // ── 4. Build v3.0 CategoriesData ────────────────────────────
                var newCats = new CategoriesData();

                foreach (var v2col in v2cats?.Collections ?? new List<V2Collection>())
                {
                    var col = new Collection
                    {
                        Name      = v2col.Name,
                        IsEnabled = v2col.IsEnabled
                    };

                    // One group per collection, named after the collection
                    var group = new CategoryGroup
                    {
                        Name      = v2col.Name,
                        IsEnabled = true
                    };

                    foreach (var v2cat in v2col.Categories ?? new List<V2Category>())
                    {
                        var cat = new Category
                        {
                            Name      = v2cat.Name,
                            IsEnabled = v2cat.IsEnabled
                        };
                        foreach (var v2opt in v2cat.Options ?? new List<V2Option>())
                        {
                            cat.Options.Add(new Option
                            {
                                Name      = v2opt.Name,
                                Weight    = v2opt.Weight,
                                IsEnabled = v2opt.IsEnabled
                            });
                        }
                        group.Categories.Add(cat);
                    }

                    col.Groups.Add(group);
                    newCats.Collections.Add(col);
                }

                // First launch with no data — add default collection
                if (newCats.Collections.Count == 0)
                    newCats.Collections.Add(new Collection { Name = "My Collection", IsEnabled = true });

                Categories = newCats;
                Write(CategoriesPath, Categories);

                // ── 5. Clear history and presets ────────────────────────────
                History = new HistoryData();
                Write(HistoryPath, History);

                Presets = new PresetsData();
                Write(PresetsPath, Presets);

                // ── 6. Write updated settings with SchemaVersion = 3 ────────
                Settings = oldSettings ?? new SettingsData();
                Settings.SchemaVersion  = CurrentSchemaVersion;
                Settings.LastResult     = new List<ResultPair>();
                Settings.LastResultTime = null;
                Write(SettingsPath, Settings);

                MigrationKind = MigrationKind.V2ToV3;
            }
            catch
            {
                // Migration failed — start fresh
                WriteDefaultData();
                MigrationKind = MigrationKind.V2ToV3;
            }
        }


        // ── Migration: unknown version → fresh start ─────────────────────────

        /// <summary>Triggered when SchemaVersion > 3 (future version) or unrecognised.
        /// Backs up all data files with .bak suffix, starts fresh.</summary>
        private static void RunUnknownVersionMigration()
        {
            try
            {
                foreach (var path in new[] { SettingsPath, CategoriesPath, HistoryPath, PresetsPath })
                {
                    if (File.Exists(path))
                    {
                        try
                        {
                            string bakPath = path + ".bak";
                            if (File.Exists(bakPath)) File.Delete(bakPath);
                            File.Copy(path, bakPath);
                        }
                        catch { /* Best effort */ }
                    }
                }
            }
            catch { /* Best effort */ }

            WriteDefaultData();
            MigrationKind = MigrationKind.UnknownVersion;
        }


        // ── History backup helper ─────────────────────────────────────────────

        /// <summary>Saves the pre-migration history.json text to data/history_backup.json.
        /// Called by MainWindow when the user clicks "Save Backup" in the migration dialog.</summary>
        public static bool SaveHistoryBackup()
        {
            if (string.IsNullOrEmpty(PreMigrationHistoryJson)) return false;
            try
            {
                EnsureDataDir();
                File.WriteAllText(HistoryBackupPath, PreMigrationHistoryJson);
                return true;
            }
            catch
            {
                return false;
            }
        }


        // ── Default data ─────────────────────────────────────────────────────

        private static void WriteDefaultData()
        {
            EnsureDataDir();

            Settings = new SettingsData { SchemaVersion = CurrentSchemaVersion };
            Write(SettingsPath, Settings);

            Categories = new CategoriesData();
            Categories.Collections.Add(new Collection { Name = "My Collection", IsEnabled = true });
            Write(CategoriesPath, Categories);

            History = new HistoryData();
            Write(HistoryPath, History);

            Presets = new PresetsData();
            Write(PresetsPath, Presets);
        }


        // ── Load ─────────────────────────────────────────────────────────────

        private static void LoadAll()
        {
            Settings   = Load<SettingsData>(SettingsPath)     ?? new SettingsData();
            Categories = Load<CategoriesData>(CategoriesPath) ?? new CategoriesData();
            History    = Load<HistoryData>(HistoryPath)       ?? new HistoryData();
            Presets    = Load<PresetsData>(PresetsPath)       ?? new PresetsData();

            // First launch with no data — create default collection
            if (Categories.Collections.Count == 0)
            {
                Categories.Collections.Add(new Collection
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
        /// groups, categories and options into a new Preset object.</summary>
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

                foreach (var grp in col.Groups)
                {
                    var grpState = new PresetGroupState
                    {
                        GroupName = grp.Name,
                        IsEnabled = grp.IsEnabled
                    };

                    foreach (var cat in grp.Categories)
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
                        grpState.Categories.Add(catState);
                    }
                    colState.Groups.Add(grpState);
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

                foreach (var grpState in colState.Groups)
                {
                    var grp = col.Groups
                        .FirstOrDefault(g => g.Name == grpState.GroupName);
                    if (grp == null) continue;

                    grp.IsEnabled = grpState.IsEnabled;

                    foreach (var catState in grpState.Categories)
                    {
                        var cat = grp.Categories
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
            "Version 3.0 — 2026\n" +
            "See changelog.txt in the data folder for full release notes.\n\n" +
            "Version 2.0 — May 2026\n" +
            "See changelog.txt for previous release notes.";

        /// <summary>
        /// Reads changelog.txt from the data folder.
        /// Creates the file with default content on first run if missing.
        /// Falls back to a minimal hardcoded string if the file can't be read.
        /// </summary>
        public static string ReadChangelog()
        {
            EnsureDataDir();

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


Version 3.0 — 2026
================================================================================

Category Groups
--------------------------------------------------------------------------------
Added Category Groups as a new organizational layer between Collections and
Categories. Each collection can contain multiple groups (e.g. HEAD, BODY, LIMBS),
and each group contains categories. Groups can be enabled or disabled independently.
Existing categories from v2.0 are automatically migrated into a single group named
after their collection.

Result Display
--------------------------------------------------------------------------------
Results are now displayed as grouped cards — one card per group showing all
category/option pairs for that group. Cards have a fixed width of 240px and scroll
horizontally within the result area.

History
--------------------------------------------------------------------------------
History rows now show a summary in the format 'Collection · N groups · N results'.
A drawn filter lets you hide or show entries marked as drawn. Entries can be marked
as drawn directly from the history list.

And more
--------------------------------------------------------------------------------
See the project documentation for the full list of v3.0 changes.


================================================================================


Version 2.0 — May 2026
================================================================================

Collections, Presets, Themes, Drag and drop rebuild, and much more.
See previous changelog for full details.


================================================================================


Version 1.0 — Early 2026
================================================================================

Initial release. Categories, options, weighted randomization, history.
";
    }


    // ════════════════════════════════════════════════════════════════════════════
    // V2.0 LEGACY MODELS — used only during v2.0 → v3.0 migration
    // ════════════════════════════════════════════════════════════════════════════

    // These mirror the v2.0 CategoriesData structure (flat: Collection → Category → Option)
    // and are used only during migration to deserialize the old categories.json.

    internal class V2Option
    {
        public string     Name      { get; set; } = "";
        public WeightTier Weight    { get; set; } = WeightTier.Normal;
        public bool       IsEnabled { get; set; } = true;
    }

    internal class V2Category
    {
        public string          Name      { get; set; } = "";
        public bool            IsEnabled { get; set; } = true;
        public List<V2Option>  Options   { get; set; } = new();
    }

    internal class V2Collection
    {
        public string           Name       { get; set; } = "";
        public bool             IsEnabled  { get; set; } = true;
        public List<V2Category> Categories { get; set; } = new();
    }

    internal class V2CategoriesData
    {
        public List<V2Collection> Collections { get; set; } = new();
    }


    // ════════════════════════════════════════════════════════════════════════════
    // V1.0 LEGACY MODELS — kept for reference, v1→v3 starts fresh (no data migration)
    // ════════════════════════════════════════════════════════════════════════════

    /// <summary>Mirrors the v1.0 AppData structure — kept for reference only.
    /// v1.0 → v3.0 migration starts fresh rather than attempting data conversion.</summary>
    internal class LegacyAppData
    {
        public List<LegacyCategory>     Categories    { get; set; } = new();
        public List<LegacyResultPair>   LastResult    { get; set; } = new();
        public DateTime?                LastResultTime { get; set; }
        public List<LegacyHistoryEntry> History       { get; set; } = new();
        public double WindowWidth                      { get; set; } = 1050;
        public double WindowHeight                     { get; set; } = 700;
        public double WindowLeft                       { get; set; } = -1;
        public double WindowTop                        { get; set; } = -1;
    }

    internal class LegacyCategory
    {
        public string             Name      { get; set; } = "";
        public bool               IsEnabled { get; set; } = true;
        public List<LegacyOption> Options   { get; set; } = new();
    }

    internal class LegacyOption
    {
        public string     Name   { get; set; } = "";
        public WeightTier Weight { get; set; } = WeightTier.Normal;
    }

    internal class LegacyResultPair
    {
        public string Category { get; set; } = "";
        public string Option   { get; set; } = "";
    }

    internal class LegacyHistoryEntry
    {
        public DateTime             Timestamp { get; set; }
        public List<LegacyResultPair> Result  { get; set; } = new();
    }
}
