using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;

namespace RandomCreation
{
    /// <summary>
    /// Handles all data persistence for Random Creation.
    /// Manages four separate JSON files in the user data folder.
    ///
    /// PROGRAM FILES vs USER DATA (see RandomCreation_DevelopmentLifecycle.md §8):
    /// program files (exe, changelog.txt, samples\) sit beside the exe and are
    /// replaced every release; user data (the four JSON files) is written by the
    /// app and never touched by an installer. Where user data lives depends on
    /// the build: a portable.txt marker beside the exe (shipped in the portable
    /// zip, absent from the installer) means data\ beside the exe; no marker
    /// means %LocalAppData%\RandomCreation\.
    /// </summary>
    public static class DataService
    {
        // ── Paths ────────────────────────────────────────────────────────────

        private static readonly string BaseDir = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>True when the portable marker file ships beside the exe.
        /// Decides where user data lives; initialised once at startup.</summary>
        public static bool IsPortable { get; } = File.Exists(Path.Combine(BaseDir, "portable.txt"));

        private static readonly string DataDir = IsPortable
            ? Path.Combine(BaseDir, "data")
            : Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "RandomCreation");

        /// <summary>The folder holding the user's data files, wherever this build
        /// keeps it. Used by the Settings screen's "Open data folder" button.</summary>
        public static string DataFolderPath => DataDir;

        private static readonly string SettingsPath       = Path.Combine(DataDir, "settings.json");
        private static readonly string CategoriesPath     = Path.Combine(DataDir, "categories.json");

        /// <summary>Returns the full path to categories.json for data export.</summary>
        public static string GetCategoriesFilePath() => CategoriesPath;
        private static readonly string HistoryPath        = Path.Combine(DataDir, "history.json");
        private static readonly string PresetsPath        = Path.Combine(DataDir, "presets.json");

        // Program files — shipped beside the exe, read-only at runtime
        private static readonly string ChangelogPath      = Path.Combine(BaseDir, "changelog.txt");
        private static readonly string SamplePath         = Path.Combine(BaseDir, "samples", "categories.json");

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

        /// <summary>Set during Initialise() when the data folder held files this
        /// version cannot read, so MainWindow can show the one-time notice.</summary>
        public static MigrationKind MigrationKind { get; private set; } = MigrationKind.None;


        // ── Initialise ───────────────────────────────────────────────────────

        /// <summary>
        /// Called once on app startup — before any UI is shown. Three cases:
        ///   * no data files at all      → fresh start, no dialog
        ///   * SchemaVersion == 3        → load normally
        ///   * anything else             → back up every file with .bak,
        ///                                 start fresh, tell the user
        /// The catch-all is not optional: without it, an unrecognised
        /// categories.json would deserialise into the current model as an empty
        /// structure, look like no content, and be overwritten on the next save.
        /// A fresh data folder receives the sample content from samples\ so a
        /// new user starts with a working example instead of an empty shell.
        /// </summary>
        public static void Initialise()
        {
            bool anyData = File.Exists(SettingsPath) || File.Exists(CategoriesPath)
                        || File.Exists(HistoryPath)  || File.Exists(PresetsPath);

            if (anyData && ReadSchemaVersion() != CurrentSchemaVersion)
                BackUpUnrecognisedData();

            // Fresh (or just-reset) data folder — install the sample content.
            // Never runs when a categories.json exists: user content is sacred.
            if (!File.Exists(CategoriesPath))
                TryInstallSampleContent();

            LoadAll();

            // Make sure a fresh install lands a settings.json stamped with the
            // current SchemaVersion straight away, so the next launch does not
            // mistake this folder for unrecognised data.
            if (!File.Exists(SettingsPath))
                SaveSettings();
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


        // ── Unrecognised data → back up and start fresh ──────────────────────

        /// <summary>Triggered when data files exist but SchemaVersion is not the
        /// current one — an older version's data, a future version's, or corrupt.
        /// Every file is moved to a .bak name so nothing is lost, then the app
        /// starts fresh and MainWindow shows a one-time notice.</summary>
        private static void BackUpUnrecognisedData()
        {
            foreach (var path in new[] { SettingsPath, CategoriesPath, HistoryPath, PresetsPath })
            {
                if (!File.Exists(path)) continue;
                try
                {
                    string bakPath = path + ".bak";
                    if (File.Exists(bakPath)) File.Delete(bakPath);
                    File.Move(path, bakPath);
                }
                catch { /* Best effort — a file that cannot move stays in place */ }
            }

            MigrationKind = MigrationKind.UnrecognisedData;
        }


        // ── Sample content ───────────────────────────────────────────────────

        /// <summary>Copies the shipped sample into the data folder. Only ever
        /// called when no categories.json exists — a conditional install can
        /// never overwrite user content. If the sample is missing, LoadAll
        /// falls back to an empty default collection.</summary>
        private static void TryInstallSampleContent()
        {
            try
            {
                if (!File.Exists(SamplePath)) return;
                EnsureDataDir();
                File.Copy(SamplePath, CategoriesPath);
            }
            catch { /* Best effort — LoadAll provides the default */ }
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
            "Random Creation\n" +
            "The changelog.txt shipped with the app could not be read.\n" +
            "Release notes for every version are published at\n" +
            "https://github.com/henry-akcama/random-creation";

        /// <summary>
        /// Reads changelog.txt from beside the exe. It is a program file —
        /// authored, shipped and replaced with every release — so the app never
        /// writes it. Falls back to a minimal string if it cannot be read.
        /// </summary>
        public static string ReadChangelog()
        {
            try
            {
                if (File.Exists(ChangelogPath))
                    return File.ReadAllText(ChangelogPath);
            }
            catch { /* Fall through to the fallback */ }

            return FallbackChangelog;
        }
    }
}
