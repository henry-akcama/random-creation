# Random Creation — File Index v3.0

## Naming Convention
Source files in Claude project knowledge use a version suffix:
`FileName_3_0.xaml`, `FileName_xaml_3_0.cs`, `FileName_3_0.cs`
Actual project files on disk have no suffix: `FileName.xaml`, `FileName.cs`
The `_2_0` files in project knowledge are v2.0 originals — still accurate for
files not modified in v3.0.

---

## Application Entry & Shell

### `App.xaml` / `App_xaml_3_0.cs`
WPF application entry point. Loads `Themes/DarkTheme.xaml` as the default merged
resource dictionary. Defines global converters (`DimmedToOpacityConverter`,
`EnabledToOpacityConverter`, `ScaleConverter`). Defines `SelectedCategoryBrush`
as a fallback resource so `DynamicResource` in `DataTemplate.Triggers` always
resolves regardless of theme timing. `App.xaml.cs` calls `DataService.Initialise()`
on startup and `UndoService.Clear()` / `ClipboardService.Clear()` on exit.
**Modified in v3.0:** Added `SelectedCategoryBrush` fallback resource.

### `MainWindow.xaml` / `MainWindow_xaml_3_0.cs`
The single application window. Custom borderless design using `WindowChrome`.
Contains all screen panels stacked in a `Grid` — only one visible at a time:
`MainPanel`, `ManageContentPanel`, `HistoryPanel`, `SettingsPanel`, `OverlayPanel`.
`OverlayPanel` is a `ContentControl` used for `CollectionsManagementScreen` and
`PresetsScreen`. Title bar has app icon, nav icons (history clock, settings gear,
theme toggle), app name centred, window controls right.
Main screen: summary bar, result card area (scrollable, grouped by group),
Generate button, recent history strip.
**Modified in v3.0:** Result cards grouped by `CategoryGroup`. Summary bar shows
group count. Mark as Drawn, AI Prompt, Print buttons. Preset quick-load popup.
Toast overlay. Space key generates.

### `RandomCreation.csproj` / `RandomCreation_3_0.csproj`
.NET 8 WPF project file. `AssemblyVersion` and `FileVersion` set to `3.0.0.0`.
App icons embedded as resources (`Build Action = Resource`).
**Modified in v3.0:** Version bumped to 3.0.0.0.

---

## Themes

### `DarkTheme.xaml` / `DarkTheme_3_0.xaml`
Dark theme resource dictionary. Defines all `SolidColorBrush` resources used
throughout the app via `DynamicResource`. Key colours: outer bg `#1c1c1e`,
card bg `#2a2a2e`, accent blue `#0a84ff`, result option blue `#60aaff`.
**Modified in v3.0:** Added `SelectedCategoryBrush` (`#1e2a3a`),
`GroupCardBorderBrush`, `GroupCardHeaderBrush`, `GroupCatSeparatorBrush`.

### `LightTheme.xaml` / `LightTheme_3_0.xaml`
Light theme resource dictionary. Same keys as `DarkTheme.xaml` with light values.
**Modified in v3.0:** Added `SelectedCategoryBrush` (`#e0eaf8`) and matching
group card brushes.

---

## Services (static classes, no UI)

### `DataService.cs` / `DataService_3_0.cs`
Central data access layer. Loads and saves all four data files: `categories.json`,
`settings.json`, `history.json`, `presets.json`. Runs migration on `Initialise()`
— detects schema version and routes to v1→v3 or v2→v3 migration. Exposes:
`Categories`, `Settings`, `History`, `Presets`, `MigrationKind`.
Key methods: `SaveCategories()`, `SaveSettings()`, `SaveHistory()`, `SavePresets()`,
`AddHistoryEntry()`, `ApplyPreset()`, `CaptureCurrentStateAsPreset()`,
`GetCategoriesFilePath()`, `SaveWindowGeometry()`, `SaveSidebarWidth()`.
**Modified in v3.0:** Full rewrite for `Collection → Group → Category → Option`
hierarchy. Migration logic. Group-aware preset capture/apply.

### `ThemeService.cs` / `ThemeService_3_0.cs`
Manages theme swapping at runtime. `ApplyTheme(AppTheme)` swaps the merged
resource dictionary in `Application.Current.Resources`. `ResolveTheme()` resolves
`AppTheme.System` to dark or light based on Windows setting.
**Modified in v3.0:** `ApplyTheme()` now also explicitly updates
`Application.Current.Resources["SelectedCategoryBrush"]` to the correct
per-theme colour after swapping — necessary because `App.xaml`-level resources
have higher precedence than merged dictionaries and cannot be overridden by them.

### `UndoService.cs` / `UndoService_3_0.cs`
In-memory undo stack. Max depth 10. `Push(description, restoreAction)` adds a
lambda-based restore action. `Undo()` pops and invokes the top action, returns
the description string for toast display. `Clear()` empties the stack.
**New in v3.0.**

### `ClipboardService.cs` / `ClipboardService_3_0.cs`
Internal clipboard (not system clipboard). Holds copied or cut items at one of
three levels: `Option`, `Category`, `Group`. Tracks cut state — cut items dim
in the UI via `IsCut` on their ViewModels. `Clear()` resets all state.
`PasteOptions()`, `PasteCategories()`, `PasteGroups()` return deep-cloned items
ready for insertion.
**New in v3.0.**

### `ToastService.cs` / `ToastService_3_0.cs`
Static toast notification service. `Register(Border, TextBlock)` called once in
`MainWindow.Loaded`. `Show(message)` fades in over 150ms, holds 2 seconds, fades
out over 300ms. Replaces any currently showing toast immediately. Thread-safe.
**New in v3.0.**

---

## Screens (UserControl, full screens)

### `ManageContentScreen.xaml` / `ManageContentScreen_xaml_3_0.cs`
The most complex file in the project (~2,400 lines). Three-panel layout:
left sidebar (collections + groups/categories), right panel (group detail OR
options). Key systems:
- **Selection:** `_selectedCollection`, `_selectedGroup` + `_selectedGroups`
  (HashSet for multi-select), `_selectedCategory` + `_selectedCategories`,
  `_selectedOption` + `_selectedOptions`
- **Groups:** card-per-group layout, expand/collapse, `_expandedGroups` HashSet,
  `BuildGroupViewModel()`, `RefreshGroupsPanel()`
- **Inline edit:** second click on name shows TextBox in-place, Enter commits,
  Escape cancels, all changes push to UndoService
- **Search:** `_searchText`, search collapses non-matching groups, dims
  non-matching categories, placeholder text overlay
- **Drag/drop:** `DragMode` enum, ghost border with insertion line, Ctrl+drag
  copy mode (green ghost + COPY badge), cross-group category drag, cross-category
  option drag
- **Keyboard:** Ctrl+C/X/V, Ctrl+D, Ctrl+Z, Delete — all level-aware
- **Right panel state machine:** Default / GroupDetail / Options
**Major rewrite in v3.0.**

### `HistoryScreen.xaml` / `HistoryScreen_xaml_3_0.cs`
History list screen. Filter bar: All / Drawn pills. Each row shows a dot indicator
(grey circle = undrawn, green checkmark = drawn), Option C summary text, timestamp,
delete button. Clicking a row opens `ResultDetailDialog`. `RefreshHistory()`
rebuilds the list respecting the active filter.
**Modified in v3.0:** Filter bar, drawn dot, Option C summary format.

### `SettingsScreen.xaml` / `SettingsScreen_xaml_3_0.cs`
Settings screen with 7 sections: Theme, Font Size (with live preview), History,
Behavior, Window, Data (export button), Keyboard Shortcuts (13 rows), About
(version from assembly + changelog). `Refresh()` re-reads settings and updates
all controls.
**Modified in v3.0:** Data section with export, 13-row shortcuts table updated
for v3.0, version read from assembly.

### `CollectionsManagementScreen.xaml` / `CollectionsManagementScreen_xaml_3_0.cs`
Overlay screen for managing collections. Add, rename (inline), delete (Delete key),
reorder (drag), duplicate (Ctrl+D), enable/disable, Ctrl+Z undo. Uses unified
selection style (blue left accent + row highlight).
**Modified in v3.0:** Inline rename, Ctrl+D, Delete key, Ctrl+Z, unified
selection style, disabled fading.

### `PresetsScreen.xaml` / `PresetsScreen_xaml_3_0.cs`
Overlay screen for managing presets. Unified selection style. Click to select,
second click to rename inline, Ctrl+D to duplicate, Delete key to delete, Ctrl+Z
undo. Load button per row with confirmation dialog. Drag to reorder. No pencil
or trash icons.
**Major rewrite in v3.0.**

---

## Dialogs (Window or UserControl)

### `ResultDetailDialog.xaml` / `ResultDetailDialog_xaml_3_0.cs`
Modal dialog showing a history entry in detail. Grouped card layout matching main
screen. Top bar: timestamp, collection name, stats, Mark as Drawn toggle (green
tint when drawn). Bottom bar: Delete Entry (red), AI Prompt, Print, Close.
Closes on Escape or click outside.
**Modified in v3.0:** Grouped card layout, Mark as Drawn, AI Prompt, Print.

### `PrintPreviewDialog.xaml` / `PrintPreviewDialog_xaml_3_0.cs`
Modal window showing a print preview. 860×900px. Two-column `UniformGrid` layout.
Group cards with blue left accent header, dark category labels, bold dark blue
option values, `#aaaaaa` card borders. Footer: Close + Print… buttons.
Print… builds a `FixedDocument` and opens the Windows `PrintDialog`.
**Modified in v3.0:** Doubled all sizes, two-column layout, darker borders,
larger fonts, `UniformGrid`.

### `ConfirmDialog.xaml` / `ConfirmDialog.xaml.cs` *(unchanged — v2.0)*
Generic two-button confirmation dialog. Used everywhere for delete confirmation
and preset load confirmation. Constructor takes title, message, cancel label,
confirm label.

### `InputDialog.xaml` / `InputDialog.xaml.cs` *(unchanged — v2.0)*
Single text input dialog. Takes title, prompt, default value, and an optional
duplicate-check function. Returns `Result` string on `ShowDialog() == true`.

### `SavePresetDialog.xaml` / `SavePresetDialog.xaml.cs` *(unchanged — v2.0)*
Dialog for saving a preset. Shows existing preset names for potential overwrite.
Returns `PresetName` string or `OverwriteTarget` preset.

### `MigrationDialog.xaml` / `MigrationDialog.xaml.cs` *(unchanged — v2.0)*
One-time dialog shown after v2→v3 migration. Explains what changed, offers
backup option for history.

### `NoticeDialog.xaml` / `NoticeDialog.xaml.cs` *(unchanged — v2.0)*
Simple one-button notice dialog. Used for v1→v3 and unknown version migration
notices.

---

## Controls

### `PillToggle.xaml` / `PillToggle.xaml.cs` *(unchanged — v2.0)*
Custom On/Off toggle control used throughout the app. `IsOn` property, `Toggled`
routed event, `Tag` passthrough for identifying the source in handlers.
Styled pill shape, blue when on, grey when off.

### `ScaleConverter.cs` *(unchanged — v2.0)*
`IValueConverter` implementations: `DimmedToOpacityConverter` (bool→double),
`EnabledToOpacityConverter` (bool→double for Generate button). Static helper
`FontScaleHelper.GetScale(FontSizeOption)` returns 1.0 / 1.3 / 1.7.

---

## Models

### `Models.cs` / `Models_3_0.cs`
All data model classes and ViewModels. Key sections:

**Data models (serialised to JSON):**
- `CategoriesData` — root, contains `List<Collection>`
- `Collection` — name, enabled, `List<CategoryGroup>`
- `CategoryGroup` — name, enabled, `List<Category>` *(new in v3.0)*
- `Category` — name, enabled, `List<Option>`
- `Option` — name, `WeightTier`, enabled
- `WeightTier` enum — UltraHigh, High, Normal, Low, Rare, UltraRare *(expanded v3.0)*
- `AppSettings` — theme, font size, window geometry, sidebar width, last result,
  last result time, schema version, last export path
- `HistoryData` / `HistoryEntry` / `ResultPair` — history with `GroupName`,
  `IsDrawn`, Option C `Summary`
- `PresetsData` / `Preset` / `PresetCollectionState` / `PresetGroupState` /
  `PresetCategoryState` / `PresetOptionState`

**ViewModels (UI-only, not serialised):**
- `CollectionViewModel` — wraps `Collection`, `IsSelected`
- `GroupViewModel` — wraps `CategoryGroup`, `IsExpanded`, `IsVisible`,
  `IsSelected`, `CategoryViewModels` list
- `CategoryViewModel` — wraps `Category`, `IsSelected`, `IsVisible`, `IsCut`
- `OptionViewModel` — wraps `Option`, `IsSelected`, `IsCut`, weight display
  properties (`WeightLabel`, `WeightBackground`, `WeightForeground`,
  `PercentageDisplay`)
- `CollectionManagementViewModel` — for CollectionsManagementScreen rows
- `PresetViewModel` — `IsSelected`, `IsExpanded`, `Summary`,
  `CollectionDisplayInfo`
- `PresetCollectionDisplayInfo` — one row in the preset expand detail

**Modified in v3.0:** `CategoryGroup` layer added. `WeightTier` expanded to 6.
`ResultPair.GroupName` added. `HistoryEntry.IsDrawn`, `EnabledGroupCount` added.
Preset models updated for group layer. Dead view properties removed
(`RowBackground`, `RowBorderBrush`, `RowCornerRadius`, `RenameLabel`,
`SubRowVisibility`).

---

## Assets

### `icon_16px.png`, `icon_32px.png`, `icon_64px.png`, `icon_128px.png`,
### `icon_256px.png`, `icon_512px.png`, `icon_master.png`
App icon at various sizes. Embedded as compiled resources (`Build Action =
Resource`). `icon_16px.png` shown in the title bar using `RenderOptions.
BitmapScalingMode="HighQuality"`. `icon_32px.png` and `icon_256px.png` used
in the `.csproj` for the window icon.

---

## Data Files (runtime, not compiled)

### `data/categories.json`
All collections, groups, categories and options. Loaded on startup.
`SchemaVersion` field in `settings.json` determines migration path.

### `data/settings.json`
App settings including theme, font size, window geometry, sidebar width,
last generated result, schema version (3 for v3.0).

### `data/history.json`
All history entries. Each entry has timestamp, result pairs (with GroupName
and IsDimmed), active collections, group/category/option counts, IsDrawn flag.

### `data/presets.json`
Named presets capturing full enable/disable state down to option level,
including group layer.

### `data/changelog.txt`
Human-readable changelog displayed in Settings About section. Newest version
at top. Safe to edit manually.

---

## Screenshots (Claude project knowledge)

UI reference screenshots uploaded as `_3_0.png` files. Used by Claude to verify
visual appearance, catch regressions and match mockups to reality.

| File | What it shows |
|------|---------------|
| `Main_Dark_Empty_3_0.png` | Main screen dark mode, no result yet |
| `Main_Dark_Result_3_0.png` | Main screen dark mode, grouped result cards |
| `Main_Light_Result_3_0.png` | Main screen light mode, grouped result cards |
| `Main_Preset_Popup_3_0.png` | Main screen with preset quick-load popup open |
| `Manage_Dark_3_0.png` | Manage Content dark, groups expanded, category + options selected |
| `Manage_Light_3_0.png` | Manage Content light, same state |
| `Manage_Group_Detail_3_0.png` | Manage Content with group detail panel open on right |
| `Manage_Search_3_0.png` | Manage Content with search active, groups collapsed/filtered |
| `Manage_Disabled_3_0.png` | Manage Content showing faded disabled group or category |
| `Collections_3_0.png` | Collections Management overlay screen |
| `Presets_3_0.png` | Presets overlay screen |
| `History_3_0.png` | History screen with mix of drawn and undrawn entries |
| `Result_Detail_3_0.png` | Result Detail dialog open |
| `Print_Preview_3_0.png` | Print preview dialog |
| `Settings_Top_3_0.png` | Settings — Theme, Font Size, History, Behavior sections |
| `Settings_Bottom_3_0.png` | Settings — Keyboard Shortcuts and About/changelog |
| `Confirm_Dialog_3_0.png` | Confirm delete dialog |
| `Input_Dialog_3_0.png` | Add group or add category input dialog |

---

## Files Copied Unchanged from v2.0

These files were not modified in v3.0 but were included in the `_3_0` upload
batch for completeness. The `_2_0` versions in project knowledge are identical.

- `MigrationDialog_3_0.xaml` / `MigrationDialog.xaml_3_0.cs`
- `NoticeDialog_3_0.xaml` / `NoticeDialog.xaml_3_0.cs`
- `ConfirmDialog_3_0.xaml` / `ConfirmDialog.xaml_3_0.cs`
- `InputDialog_3_0.xaml` / `InputDialog.xaml_3_0.cs`
- `SavePresetDialog_3_0.xaml` / `SavePresetDialog.xaml_3_0.cs`
- `PillToggle_3_0.xaml` / `PillToggle.xaml_3_0.cs`
- `ScaleConverter_3_0.cs`
