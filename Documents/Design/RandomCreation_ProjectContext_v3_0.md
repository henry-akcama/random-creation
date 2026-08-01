# Random Creation — Project Context Document

## Overview

**Application name:** Random Creation
**Current version:** 3.0
**Original working title:** Creature Crafter (v1.0), renamed to Random Creation in v2.0

Random Creation is a Windows desktop app for generating random combinations from
user-defined categories and options. The user creates Collections (e.g. Creatures,
Starships, Guns), adds Category Groups to each collection (e.g. HEAD, BODY, LIMBS),
adds Categories to each group (e.g. Head Style, Head Count, Head Size), and adds
Options to each Category (e.g. Horned, Round, Tiny). Hitting Generate picks one
random option from each enabled category in each enabled group in each enabled
collection and displays the full combination as a grouped result card. The app is
intentionally general purpose — the collection/group/category/option system works
for any kind of random combination generator.

---

## Intended Users

Personal creative tool. Designed for writers, game designers, hobbyists, or anyone
who wants to generate random combinations of traits. Currently in personal use /
small-pool testing. Not a commercial product.

---

## Technical Stack

- **Language:** C#
- **Framework:** WPF (Windows Presentation Foundation)
- **Target:** .NET 8.0 (Windows)
- **Serialization:** System.Text.Json (built into .NET 8, no NuGet packages)
- **IDE:** Visual Studio Community 2022
- **Solution type:** Single Visual Studio solution, single project

---

## Data Files

All data lives in a `data/` folder next to the exe. Copying this folder to another
machine transfers the full user setup.

| File | Contents |
|------|----------|
| `data/settings.json` | Theme, font size, window geometry, sidebar width, history limit, confirm-on-delete, schema version |
| `data/categories.json` | All collections, groups, categories and options — kept clean, no version field, user-editable |
| `data/history.json` | Full generation history with timestamps, dim states, group membership |
| `data/presets.json` | Saved presets (named snapshots of enabled/disabled states) |
| `data/changelog.txt` | Human-readable changelog, loaded at runtime by Settings screen |

### Schema Versioning

`settings.json` carries a `SchemaVersion` integer field. This is the single source
of truth for whether data files need migration.

| Version | Meaning |
|---------|---------|
| absent / 0 | Pre-v3.0 file (v1.0 or v2.0) — migration required |
| 3 | v3.0 file — no migration needed |

`categories.json` intentionally has no version field. It is kept clean for
hand-editing and AI-assisted editing by users. The structure itself is self-
describing — `CategoryGroup` objects are either present (v3.0) or absent (pre-v3.0).

---

## Migration Paths

`DataService.Initialise()` reads `SchemaVersion` from `settings.json` before
loading any other data. If the version is below 3 (or the file is absent), the
appropriate migration path runs.

### v2.0 → v3.0 Migration

Triggered when `settings.json` exists but `SchemaVersion` is 0 or absent.

1. Each `RandomCollection`'s flat `Categories` list is wrapped: one `CategoryGroup`
   is created per collection, named after the collection. All existing categories
   become `Category` objects inside that one group.
2. `history.json` is cleared (history entries reference the old flat structure).
3. `presets.json` is cleared (preset state references old category paths).
4. A one-time dialog informs the user:
   - What was migrated and how (groups created, named after collections)
   - That history and presets have been cleared
   - An option to save a backup of `history.json` to `data/history_backup.json`
     before clearing. The dialog offers "Save Backup" and "Continue without backup."
   - The migration dialog can be dismissed by clicking outside it (Deactivated
     handler) or via the single Close button.
5. `SchemaVersion` is written as `3` to `settings.json` and saved.

### v1.0 → v3.0 Migration

Triggered when `creature_crafter_data.json` exists next to the exe and no `data/`
folder exists (same detection as v2.0 used for v1.0).

1. The old file is renamed to `creature_crafter_data.json.bak`.
2. A fresh `data/` folder is created with default empty data files.
3. A dialog informs the user that the old file was not compatible with v3.0, that
   it has been saved as `creature_crafter_data.json.bak`, and that the app has
   started fresh.
4. `SchemaVersion` is written as `3`.

### Unknown version → Fresh start

Triggered when `settings.json` exists with a `SchemaVersion` higher than the
current app version (i.e. a file from a future version opened in an older build),
or a `SchemaVersion` that has no known upgrade path.

1. All data files are backed up by appending `.bak` to their filenames.
2. Fresh default data files are written.
3. A dialog informs the user that the data format was not recognised, that backups
   have been saved, and that the app has started fresh.

---

## File Structure & Versioning

Source files use a version suffix (e.g. `_3.0`) so new and old files coexist during
development. v1.0 and v2.0 files remain for reference.

### Version 3.0 Files

| File | Purpose |
|------|---------|
| `RandomCreation.csproj` | Project file — .NET 8 WPF, self-contained win-x64 publish |
| `App.xaml` | Application entry point, global resources, converter registrations |
| `App.xaml.cs` | DataService.Initialise() → ThemeService.Apply() → window shows |
| `Models_3.0.cs` | All data models, ViewModels, enums and helper classes |
| `DataService_3.0.cs` | All persistence — load, save, migrate, presets, history, changelog |
| `UndoService_3.0.cs` | In-memory undo stack — 10-action depth, lambda-based restore |
| `ToastService_3.0.cs` | Lightweight toast notification overlay — 2-second fade, bottom center |
| `ThemeService_3.0.cs` | Theme resource dictionary swapping, system default detection |
| `ScaleConverter_3.0.cs` | All WPF value converters used in XAML bindings |
| `Themes/DarkTheme.xaml` | Full dark theme color palette and scrollbar styles |
| `Themes/LightTheme.xaml` | Full light theme color palette and scrollbar styles |
| `PillToggle.xaml` / `.cs` | Custom ON/OFF pill control using DynamicResource Style triggers |
| `MainWindow.xaml` / `.cs` | Main screen, borderless window, navigation, generate logic |
| `ManageContentScreen.xaml` / `.cs` | Two-panel sidebar, group detail panel, options panel, drag and drop |
| `CollectionsManagementScreen.xaml` / `.cs` | Add/rename/delete/reorder/enable/disable collections |
| `HistoryScreen.xaml` / `.cs` | Full history list with drawn filter and delete |
| `PresetsScreen.xaml` / `.cs` | Presets list, save/load/rename/delete/reorder |
| `SettingsScreen.xaml` / `.cs` | Theme, font size, history, behavior, window, data export, changelog |
| `ResultDetailDialog.xaml` / `.cs` | Borderless popup showing grouped result cards |
| `PrintPreviewDialog.xaml` / `.cs` | Compact paper-proportioned print preview with Scheme D color theme |
| `ConfirmDialog.xaml` / `.cs` | Borderless themed confirm dialog, red or blue button variant |
| `InputDialog.xaml` / `.cs` | Borderless themed text input dialog with inline validation |
| `SavePresetDialog.xaml` / `.cs` | Preset save dialog — new name or overwrite existing |

---

## Architecture

### Navigation
`MainWindow` owns all navigation. Screens are `UserControl` panels stacked in a
root `Grid` — only one is `Visible` at a time. Overlay screens (Collections
Management, Presets) are shown via `ShowOverlay(UserControl)` / `HideOverlay()`.
`PrintPreviewDialog` and `ResultDetailDialog` are separate `Window` instances.

### Data flow
`DataService` is a static class. All screens read from and write to it directly.
No MVVM binding layer — screens call `DataService.SaveX()` after every mutation
and call their own `Refresh()` to rebuild their ItemsSource.

### Undo system
`UndoService` is a static class with an in-memory `Stack<UndoAction>` of maximum
depth 10. Each `UndoAction` stores a description string and a restore `Action`
lambda capturing all state needed to reverse the operation. Ctrl+Z calls
`UndoService.Undo()` which pops and invokes the top action. The stack is never
persisted — it is cleared on app close and lost on crash. Nothing is written to any
JSON file as part of undo.

**Actions pushed to the undo stack:**
- Delete anything (option, category, group, collection)
- Rename anything
- Add anything (option, category, group — each add is one undo step)
- Drag reorder or move
- Ctrl+X cut
- Ctrl+V paste
- Enable/disable toggle (one undo per click; bulk operations like Enable All = one undo)
- Weight change (one undo per click)

**Actions NOT pushed:**
- Navigation (selecting items in sidebar)
- Opening/closing dialogs

**One user gesture = one undo step.** A single click or keyboard action that
affects multiple items (e.g. Enable All, multi-select paste) is reversed as one
step.

### Toast system
`ToastService` is a static class that controls a single overlay element anchored
to the bottom center of `MainWindow`. Calling `ToastService.Show(message)` fades
in the toast, holds for 2 seconds, then fades out. If a new toast is shown while
one is already visible, it replaces it immediately. The toast uses `DynamicResource`
bindings so it renders correctly in both dark and light themes.

**Toast triggers:**
| Action | Toast message |
|--------|--------------|
| AI Prompt button | "Prompt copied to clipboard" |
| Export data | "Data exported successfully" |
| Ctrl+C copy | "Copied [item name]" |
| Ctrl+V paste | "Pasted [item name]" |
| Mark as Drawn toggle on | "Marked as drawn" |
| Mark as Drawn toggle off | "Unmarked as drawn" |
| Preset quick-load | "Preset loaded: [preset name]" |

### Theme system
Unchanged from v2.0. `ThemeService.ApplyTheme()` swaps merged resource
dictionaries. All XAML uses `DynamicResource`.

### Font scaling
`MainWindow.ApplyFontScale()` sets `LayoutTransform = new ScaleTransform(scale, scale)`
on `MainContentGrid`. Scale factors: Normal=1.0, Large=1.3, Extra Large=1.7.

**v3.0 fix:** All coordinate calculations in drag-and-drop now account for the
active `ScaleTransform` using `TranslatePoint` / `TransformToAncestor`. This fixes
both the drag insertion index drift at non-Normal font sizes and the visual flicker
when switching between Large and Extra Large. The scale change is batched through
`Dispatcher.InvokeAsync` at `DispatcherPriority.Render` to prevent the double-
render flicker.

---

## Data Models (`Models_3.0.cs`)

### Core data hierarchy

```
CategoriesData
  └── List<Collection>
        └── List<CategoryGroup>          ← NEW in v3.0
              └── List<Category>         ← was RandomCategory in v2.0
                    └── List<Option>     ← was RandomOption in v2.0
```

**Naming:** The `Random` prefix is dropped from all model class names in v3.0.
`RandomCollection` → `Collection`, `RandomCategory` → `Category`,
`RandomOption` → `Option`. `CategoryGroup` is new.

```csharp
public class Option
{
    public string     Name      { get; set; } = "";
    public WeightTier Weight    { get; set; } = WeightTier.Normal;
    public bool       IsEnabled { get; set; } = true;
}

public class Category
{
    public string         Name      { get; set; } = "";
    public bool           IsEnabled { get; set; } = true;
    public List<Option>   Options   { get; set; } = new();
}

public class CategoryGroup
{
    public string           Name        { get; set; } = "";
    public bool             IsEnabled   { get; set; } = true;
    public List<Category>   Categories  { get; set; } = new();
}

public class Collection
{
    public string               Name      { get; set; } = "";
    public bool                 IsEnabled { get; set; } = true;
    public List<CategoryGroup>  Groups    { get; set; } = new();
}

public class CategoriesData
{
    public List<Collection> Collections { get; set; } = new();
}
```

### ResultPair — updated for groups

```csharp
public class ResultPair : INotifyPropertyChanged
{
    public string GroupName { get; set; } = "";   // ← NEW in v3.0
    public string Category  { get; set; } = "";
    public string Option    { get; set; } = "";
    public bool   IsDimmed  { get; set; }
}
```

`GroupName` is used to group result cards in the result display. Pre-v3.0 history
entries have `GroupName = ""` and display as a flat card list (backward compatible).

### HistoryEntry — updated

`DotColorHex` and `DotColor` are removed. Dot appearance is now determined by the
`IsDrawn` flag, not a per-entry color.

```csharp
public class HistoryEntry
{
    public DateTime         Timestamp             { get; set; }
    public List<ResultPair> Result                { get; set; } = new();
    public List<string>     ActiveCollections     { get; set; } = new();
    public int              EnabledGroupCount     { get; set; } = 0;   // ← NEW
    public int              EnabledCategoryCount  { get; set; } = 0;
    public int              EnabledOptionCount    { get; set; } = 0;
    public bool             IsDrawn               { get; set; } = false; // ← NEW

    // Computed summary — Option C format
    // e.g. "Creatures · 4 groups · 17 results"
    [JsonIgnore]
    public string Summary { get; }
}
```

### SettingsData — updated

```csharp
public class SettingsData
{
    public int          SchemaVersion   { get; set; } = 3;    // ← NEW
    public AppTheme     Theme           { get; set; } = AppTheme.System;
    public FontSizeScale FontSize       { get; set; } = FontSizeScale.Normal;
    public double       WindowWidth     { get; set; } = 1050;
    public double       WindowHeight    { get; set; } = 700;
    public double       WindowLeft      { get; set; } = -1;
    public double       WindowTop       { get; set; } = -1;
    public double       SidebarWidth    { get; set; } = 280;
    public int          HistoryLimit    { get; set; } = 500;
    public bool         ConfirmOnDelete { get; set; } = true;
    public List<ResultPair> LastResult  { get; set; } = new();
    public DateTime?    LastResultTime  { get; set; }
    // MigrationComplete bool removed — replaced by SchemaVersion
}
```

### Preset models — updated for v3.0 hierarchy

```csharp
public class PresetOptionState
{
    public string OptionName { get; set; } = "";
    public bool   IsEnabled  { get; set; } = true;
}

public class PresetCategoryState
{
    public string                   CategoryName { get; set; } = "";
    public bool                     IsEnabled    { get; set; } = true;
    public List<PresetOptionState>  Options      { get; set; } = new();
}

public class PresetGroupState          // ← NEW in v3.0
{
    public string                    GroupName   { get; set; } = "";
    public bool                      IsEnabled   { get; set; } = true;
    public List<PresetCategoryState> Categories  { get; set; } = new();
}

public class PresetCollectionState
{
    public string                  CollectionName { get; set; } = "";
    public bool                    IsEnabled      { get; set; } = true;
    public List<PresetGroupState>  Groups         { get; set; } = new(); // ← replaces Categories
}

public class Preset
{
    public string                       Name        { get; set; } = "";
    public List<PresetCollectionState>  Collections { get; set; } = new();

    [JsonIgnore]
    public int EnabledCollectionCount => Collections.Count(c => c.IsEnabled);
    [JsonIgnore]
    public int EnabledGroupCount =>
        Collections.SelectMany(c => c.Groups).Count(g => g.IsEnabled);
    [JsonIgnore]
    public int EnabledCategoryCount =>
        Collections.SelectMany(c => c.Groups)
                   .SelectMany(g => g.Categories).Count(cat => cat.IsEnabled);
    [JsonIgnore]
    public int EnabledOptionCount =>
        Collections.SelectMany(c => c.Groups)
                   .SelectMany(g => g.Categories)
                   .SelectMany(cat => cat.Options).Count(o => o.IsEnabled);
    [JsonIgnore]
    public string Summary =>
        $"{EnabledCollectionCount} collections · {EnabledGroupCount} groups · " +
        $"{EnabledCategoryCount} categories · {EnabledOptionCount} options enabled";
}
```

---

## Generation Logic

The generator iterates `Collection → CategoryGroup → Category`. A category is
included in the roll only if ALL of the following are true:
- Its parent `Collection.IsEnabled` is true
- Its parent `CategoryGroup.IsEnabled` is true
- Its own `Category.IsEnabled` is true

Each included category picks one option from its enabled options using weighted
random selection (Normal=3, Low=2, Rare=1). The result is a `List<ResultPair>`
where each pair carries `GroupName`, `Category`, `Option`, and `IsDimmed=false`.

---

## UI Design

The app follows a WinUI 3 / Windows 11 aesthetic. No gradients, no drop shadows.
Rounded corners throughout. Custom borderless window with a matching title bar.

### Color Palette — Dark Theme (unchanged from v2.0)

| Element | Hex |
|---------|-----|
| Outer background | `#1c1c1e` |
| Card / panel background | `#2a2a2e` |
| Title bar / sidebar background | `#161618` |
| Zone header band | `#111114` |
| Expanded row / selected | `#1e1e22` |
| Selected category / group | `#1e2a38` |
| Borders (standard) | `#3a3a3c` |
| Borders (subtle) | `#2a2a2c` |
| Primary text | `#e0e0e0` |
| Muted text | `#a0a0a8` |
| Blue accent | `#0a84ff` |
| Result option values | `#60aaff` |
| Delete red | `#e05050` |
| Green success / drawn | `#30d158` |
| Drawn background tint | `#1a2a1a` |

### Color Palette — Light Theme (unchanged from v2.0)
See `LightTheme.xaml` for full palette.

---

## Main Screen

### Title bar
Left to right:
- App icon (16px, embedded resource, `RenderOptions.BitmapScalingMode=HighQuality`)
- Nav icons: History (clock), Settings (gear), Theme toggle (moon/sun) — unchanged from v2.0
- App name "Random Creation" — centered, 16px medium weight, 20% opacity (faded watermark style)
- Window controls: minimize, maximize, close — right side

### Summary bar
```
GENERATING FROM  [Creatures]  3 groups · 17 categories · 96 options        Presets
```
- Collection badge(s) on left
- Updated counts include group count
- "Presets" text link on right — opens preset quick-load popup

### Preset quick-load popup
- Lightweight popup anchored below the Presets link
- Each row: preset name + summary (collections · categories · options)
- Click row to load — closes popup and shows toast "Preset loaded: [name]"
- No active state indicator — too many ways state can drift
- "Manage →" link in popup header opens full Presets screen
- Click outside to close

### Result card area
**Header row:**
```
LAST GENERATED RESULT          [○ Mark as Drawn]  [✦scroll icon]  [🖨 printer icon]
```
- Mark as Drawn toggle: `○ Mark as Drawn` when off, `✓ Drawn` (green border, green text) when on
- AI Prompt icon: scroll with two 4-pointed sparkle diamonds (SVG path)
- Print icon: printer outline (SVG path)

**Group cards (new in v3.0):**
Results are displayed as one card per `CategoryGroup`. Cards use fixed 240px width.
With 1–2 groups the cards expand to fill available width evenly (`flex: 1 1 240px`).
With 3+ groups, fixed 240px, wrapping to next row naturally. The result area
scrolls independently — Generate button and Recent History always remain visible.

Each group card:
- Header: group name in 13px medium weight, blue left border accent (3px, `#0a84ff`),
  subtle background tint (`#f0f4ff` light / `#1c1c1e` dark)
- Body: two-column grid of rows — category name (left, muted) and option value
  (right, blue accent, bold). Both columns left-justified.
- Dimmed rows: 30% opacity. Click a row to toggle dim state. Dimmed rows excluded
  from AI Prompt output.
- Long names wrap to two lines rather than truncating.

**Timestamp:** small muted text at bottom of result area.

### Generate button
Full-width blue button. Unchanged from v2.0.

### Recent History strip
- Last 3 entries, "View all" button links to History screen
- Each row: dot indicator + summary text + timestamp
- Dot: grey circle (10px) when not drawn. Green filled checkmark (12px) when drawn.
  Both sit in an equal-size 12px wrapper so alignment is consistent.
- Summary text: Option C format — "Creatures · 4 groups · 17 results"

---

## Manage Content Screen

### Title bar
- App icon left
- `← Back` text link (unchanged from v2.0 — back button revisit deferred)
- "Manage Content" screen title centered (15px medium weight)
- Bookmark icon right (opens Presets screen)
- Window controls right

### Sidebar — two panel layout

The sidebar contains two distinct visual panels separated by a gap of ~8px. Each
panel has its own rounded border, dark background, and zone header band.

**Panel 1 — Collections:**
```
┌─────────────────────────────┐
│ COLLECTIONS            [⚙]  │  ← zone header band (#111114), gear unchanged
│ ⠿  Creatures           On   │  ← selected: blue left border, #1e3a5a bg
│ ⠿  Starships           On   │
└─────────────────────────────┘
```
- Gear icon opens `CollectionsManagementScreen` (unchanged from v2.0)
- Collection rows: drag handle, name, On/Off badge
- Selected collection has blue left border and highlight background

**Panel 2 — Groups and Categories:**
```
┌─────────────────────────────┐
│ GROUPS — CREATURES     [＋]  │  ← zone header band, blue ＋ to add group
│ [Search groups/categories…] │  ← always-visible search bar
│ ⠿ ▼ HEAD           3  On   │  ← expanded group — blue left border
│              ＋ Add Category │  ← right-justified dashed button
│    ⠿  Head Style    6  On   │  ← indented category rows
│       Rename or delete …✎🗑 │  ← inline edit sub-row when selected
│    ⠿  Head Count    4  On   │
│ ⠿ ▶ BODY           5  On   │  ← collapsed group
│ ⠿ ▶ LIMBS          4  Off  │  ← disabled group (dimmed)
└─────────────────────────────┘
```

**Groups panel header:** `GROUPS — [COLLECTION NAME]` left, blue `＋` right.
- Blue `＋` adds a new group via `InputDialog`. Has tooltip "Add Group".
- `＋` is the only add-group trigger. There is no separate management screen for groups.

**Group rows:** `⠿  ▼/▶  NAME  [count]  [On/Off pill]`
- Clicking expands/collapses (auto-collapses other groups — only one open at a time)
- Clicking also opens Group Detail in the right panel
- No inline edit actions in the sidebar for groups
- Drag handle enables reorder, cross-collection move, and Ctrl+copy

**Add Category button:** appears on its own line below the expanded group header,
right-justified, dashed border style: `＋ Add Category`

**Category rows (indented):** `⠿  Name  [count]  [On/Off pill]`
- Indented ~12px to visually nest under group
- Clicking selects the category and shows its options in the right panel
- Clicking shows inline edit sub-row: `Rename or delete [Name]  ✎  🗑`
- Drag handle enables reorder within group, cross-group move, and Ctrl+copy

**Search bar:** always visible below the zone header. Searches both group names
and category names simultaneously. Matching groups auto-expand. Non-matching items
dim. Matched text highlighted in amber. Add Group `＋` dims and is disabled during
active search. ✕ clears search.

**Enable All / Disable All:** operates at group level — toggles all groups in the
selected collection on or off. No category-level bulk toggle.

### Right panel — three states

**State 1 — Default (nothing selected):**
```
Select a group to view its details
or click a category to view its options
```

**State 2 — Group selected (Group Detail):**
Two-column layout:

Left column (group info, ~180px wide):
- Group name + pencil icon (opens `InputDialog` for rename)
- On/Off pill toggle (updates sidebar pill simultaneously, dims group when off)
- Thin divider
- Stats: Categories count, Total options, Enabled categories N/N
- Thin divider
- `🗑 Delete Group` button (red, destructive, respects Confirm on Delete setting)

Right column (category list):
- Header: `CATEGORIES` label left, `＋ Add Category` dashed button right
- Rows: `⠿  Category Name  [count]  [On/Off pill]  ✎  🗑`
- Clicking a category in this list also selects it in the sidebar and switches
  the right panel to the Options view
- Drag handle for reorder within group

**State 3 — Category selected (Options panel):**
Unchanged from v2.0:
- Header: `[Category Name] — Options` left, `＋ Add option` blue button right
- Option rows: `⠿  [On/Off pill]  Name  [Weight badge]  [%]  ✎  🗑`
- Options are now selectable — clicking an option row selects it (blue highlight)
  enabling Ctrl+C, Ctrl+X, Delete key on that option

---

## Drag and Drop System

### Implementations
Three drag implementations exist: `ManageContentScreen`, `CollectionsManagementScreen`,
`PresetsScreen`. All share the same fix for coordinate space handling.

### v3.0 Bug Fixes (applied to all three implementations)
- **Top/bottom edge clamp:** `GetInsertionIndex` now clamps to index 0 when the
  cursor is above the midpoint of the first item, and to `count` when below the
  last item's bottom edge. Fixes drag-to-top and drag-to-bottom failures.
- **Drop index validation:** At `MouseUp` commit time, `_dropIndex` is clamped to
  `[0, count]` and the mutation is aborted if `_dropIndex == -1` or `_dragActive`
  is false. Skips mutation entirely when `fromIdx == toIdx` or `fromIdx == toIdx-1`.
- **ScaleTransform coordinate fix:** All `e.GetPosition(container)` calls now use
  `TranslatePoint` / `TransformToAncestor` to account for the active
  `LayoutTransform ScaleTransform`. Fixes the insertion index drift at Large and
  Extra Large font sizes, and fixes the list scramble bug caused by stale
  coordinates at non-1.0 scale.

### Ctrl+Drag Copy (new in v3.0)

Holding Ctrl during a drag performs a **copy** (source stays, duplicate inserted).
Without Ctrl = **move** (existing behavior). The modifier can be toggled at any
point during the drag.

**Visual states:**
- **Move mode:** source item dims to ~40% opacity. Ghost is semi-transparent.
- **Copy mode (Ctrl held):** source item returns to full opacity. Ghost becomes
  more opaque/solid. Green border on ghost. Green "COPY" badge on ghost right side.
  Transition between states is immediate on Ctrl press/release.

**Ctrl state tracking:** `PreviewKeyDown` / `PreviewKeyUp` on the Window extend
the existing `OnDragKeyDown` handler. Escape still cancels the drag.

**Copy behavior by level:**
- **Option copy:** new `Option` with same Name, Weight, IsEnabled. Same-container
  duplicates allowed (two options with the same name in one category is valid).
  Cross-container: counter suffix if name collides.
- **Category copy:** new `Category` with same Name, IsEnabled, deep copy of all
  Options. Counter suffix on name collision in target group.
- **Group copy:** new `CategoryGroup` with same Name, deep copy of all Categories
  and Options. Counter suffix on name collision in target collection.
- **Cross-level drops always insert at bottom of target container.**
- **Counter suffix rule:** "Name (2)", "Name (3)" — lowest integer ≥ 2 that makes
  the name unique in the target container.

### Ctrl+C / Ctrl+X / Ctrl+V (new in v3.0)

Keyboard copy/cut/paste complements Ctrl+drag, solving the blind spot of copying
into a collapsed group.

**Selection:**
- Clicking an option row selects it (blue highlight) — same pattern as categories.
- Ctrl+click adds to selection within the same level and same container only.
- Multi-select allowed for options (within one category) and categories (within
  one group). Multi-select NOT allowed for groups.
- Selecting an item at a different level or container clears the current selection.
- "N selected" count shown in right panel header when multiple items selected.

**Ctrl+C (copy):**
- Copies selected item(s) to internal clipboard.
- Source shows subtle green outline until first paste or Escape.
- Clipboard persists for repeated pasting. Green outline clears after first paste
  but item stays on clipboard.
- Clipboard is internal only (not system clipboard). Cleared on Escape or app close.

**Ctrl+X (cut):**
- Marks selected item(s) for cut. Source dims to ~40%.
- Ctrl+V completes the move — item removed from source, inserted at target.
- Escape cancels — source returns to full opacity.
- Auto-cancels if user navigates away without pasting.
- Starting a drag while in cut mode cancels the cut and begins a drag-move instead.

**Ctrl+V (paste):**
- Pastes at bottom of selected target container.
- Level enforcement: options paste into categories, categories paste into groups.
  Wrong-level paste does nothing with a subtle visual indicator.
- Counter suffix on name collision.
- Multi-select pastes all items in original relative order.
- After paste: green outline clears on source, selection moves to first pasted item.

**Delete key:**
- Triggers delete on the currently selected item (option, category, group, or
  collection).
- Respects "Confirm on Delete" setting.
- Pushes to undo stack.
- Only fires when a list item is selected and no text input has focus.

---

## History Screen

### Layout
Unchanged from v2.0 except:
- Filter bar added below title bar: `[All]  [✓ Drawn]` pill toggle, drawn count right
- Each row: dot indicator + summary text + timestamp
- Dot: grey (undrawn) or green checkmark (drawn) — same as main screen recent history
- Summary text: "Creatures · 4 groups · 17 results" (Option C format)
- Clicking a row opens `ResultDetailDialog`

### Drawn filter
- "All" shows all entries (default)
- "✓ Drawn" shows only entries with `IsDrawn = true`
- Drawn count displayed as "N drawn" on the right of the filter bar

### History storage
Pre-v3.0 history is cleared on migration. New entries store `GroupName` per
`ResultPair` and `IsDrawn` per entry. Pre-v3.0 entries (if somehow present) have
`GroupName = ""` and display as flat card list in `ResultDetailDialog`.

---

## ResultDetailDialog

**Size:** 800px wide, height fixed, `ResizeMode="NoResize"`.
**Behavior:** Close on Escape (existing), close on click outside (Deactivated handler, new in v3.0).

### Layout

**Top bar:**
- Left: timestamp, collection name, summary stats (groups · categories · options)
- Right: `○ Mark as Drawn` toggle button
  - Undrawn: `○ Mark as Drawn`, standard style
  - Drawn: `✓ Drawn`, green border, green text, top bar gets subtle green background tint

**Cards area (scrollable):**
- Grouped card layout — same as main screen result cards
- Each group gets a card with blue left border header and two-column category rows
- Pre-v3.0 history entries display as flat card list (no group headers)

**Bottom bar:**
- Left: `Delete Entry` (red bordered, destructive)
- Right: `AI Prompt` (scroll+sparkle icon) · `🖨 Print` · `Close`

---

## Print and Print Preview

### Trigger locations
- Main screen: print icon (printer SVG) in result header row right side
- `ResultDetailDialog`: Print button in bottom bar

### Print preview dialog (`PrintPreviewDialog`)
- Fixed size, compact, not resizable — represents a miniaturized A4/Letter page
  (approx. 420×594px — paper proportions at small scale)
- Shows a rendered preview of the print output using WPF controls styled for paper
- Footer: `Close` button and `🖨 Print…` button
- `Print…` opens the standard Windows `PrintDialog`
- On print confirm → auto-close preview
- On print cancel → preview stays open

### Print color scheme (Scheme D)
White page background. Color preserved but adapted for paper:
- Group card header: blue left border accent (3px, `#0a84ff`), light blue tint
  background (`#f0f4ff`), dark text (`#333333`)
- Category label: medium gray (`#888888`)
- Option value: dark blue (`#0055cc`), bold — readable on both color and mono printers
- Card border: light gray (`#e0e0e0`)
- App name + timestamp: small gray at top of page
- No large colored backgrounds, no dark fills

### Print layout
- Timestamp, collection name, summary stats at top (mirrors ResultDetailDialog top bar)
- 4 columns on standard A4/Letter portrait
- Standard 1-inch / 25mm margins
- Group separators span full printable width
- `FixedDocument` built programmatically

---

## AI Prompt Feature

**Trigger locations:** scroll+sparkle icon in main screen result header, AI Prompt
button in `ResultDetailDialog` bottom bar.

**Behavior:** Silently copies a prompt to the system clipboard. Shows toast:
"Prompt copied to clipboard". No dialog, no preview.

**Prompt format (Option C — collection-agnostic structured list):**
```
Generate an image based on the following randomly generated traits.
Collection: [Collection Name]. [GROUP NAME] — [Category]: [Option],
[Category]: [Option]. [GROUP NAME] — [Category]: [Option]...
```

- Collection name used as context, not interpreted
- Group names and all category/option names used exactly as entered by user
- Dimmed rows (`IsDimmed = true`) excluded from prompt
- Works for any collection type regardless of content

---

## Settings Screen

### Sections (unchanged from v2.0 unless noted)
- **Theme:** Dark / Light / System Default buttons
- **Font Size:** Normal / Large / Extra Large slider with preview
- **History:** History limit, Clear All History
- **Behavior:** Confirm on Delete toggle
- **Window:** Reset window size
- **Data (new in v3.0):**
  - Export categories data: `[Export]` button — opens `SaveFileDialog`, saves entire
    `categories.json` as-is to user-chosen location. Default filename:
    `RandomCreation_categories.json`. Toast: "Data exported successfully."
- **Keyboard Shortcuts:** read-only reference list — updated for v3.0 shortcuts
- **About:** app name, version (read from `Assembly.GetExecutingAssembly().GetName().Version`
  — no longer hardcoded), changelog

### Updated keyboard shortcuts reference
| Action | Shortcut |
|--------|----------|
| Go back / Close dialog | Escape |
| Confirm dialog | Enter |
| Generate (main screen) | Space / Enter |
| Open History | Ctrl+H |
| Open Manage Content | Ctrl+M |
| Undo | Ctrl+Z |
| Copy selected item | Ctrl+C |
| Cut selected item | Ctrl+X |
| Paste | Ctrl+V |
| Delete selected item | Delete |
| Copy modifier during drag | Ctrl (hold while dragging) |

---

## Presets Screen

### Changes from v2.0
- Summary counts updated to include group count: "3 collections · 4 groups · 17 categories · 96 options enabled"
- `PresetCollectionState` now contains `List<PresetGroupState>` instead of
  `List<PresetCategoryState>` — group layer added between collection and category
- `DataService.ApplyPreset()` traverses `Collection → Group → Category` for matching
- `DataService.CaptureCurrentStateAsPreset()` captures group state
- Preset quick-load accessible from main screen summary bar (popup) without
  navigating to this screen

---

## CollectionsManagementScreen

Unchanged from v2.0. Add, rename, delete, reorder, enable/disable collections.
Enable/disable here hides the collection from ManageContent sidebar and excludes
it from generation.

---

## Visual Polish — v3.0 Changes

### App icon in title bar
16px `Image` element at far left of title bar. Uses embedded resource
(`Build Action = Resource`). `RenderOptions.BitmapScalingMode="HighQuality"`.
Fixes the missing icon at 100% DPI on integrated graphics machines.

### Theme toggle tooltip
`← Back` button and theme toggle now have tooltips. Theme toggle shows
"Switch to light mode" or "Switch to dark mode" depending on current state.
`ToolTipService.InitialShowDelay` reduced to 400ms globally in `App.xaml`.

### Gear icon
Unicode `⚙` replaced with a custom SVG path gear icon in both ManageContent
and Settings, matching the style of other icons in the app.

### Title hierarchy
- **Main screen:** app name "Random Creation" centered, 16px, 20% opacity (Variant B)
- **Sub-screens:** screen title centered, 15px medium weight. App name not shown.
  The faded app name only appears on the main screen.

### Result summary bar counts
Updated to include groups: "3 groups · 17 categories · 96 options"

---

## New Files Details

### `UndoService_3.0.cs`
```csharp
public static class UndoService
{
    private static readonly Stack<UndoAction> _stack = new();
    private const int MaxDepth = 10;

    public static void Push(UndoAction action) { ... }
    public static void Undo() { if (_stack.TryPop(out var a)) a.Restore(); }
    public static void Clear() => _stack.Clear();
    public static bool CanUndo => _stack.Count > 0;
}

public class UndoAction
{
    public string Description { get; set; } = "";
    public Action Restore     { get; set; } = () => { };
}
```

`UndoService.Clear()` is called on app close (`Application.Exit`). Stack is
never persisted — lost on close or crash. `MainWindow.PreviewKeyDown` handles
Ctrl+Z and calls `UndoService.Undo()`.

### `ToastService_3.0.cs`
Controls a `Border` overlay element in `MainWindow` anchored to bottom center.
`ToastService.Show(string message)` is the public API. Uses `DispatcherTimer`
for the 2-second hold and `DoubleAnimation` for fade in/out. Replaces current
toast if one is showing. Uses `DynamicResource` for theme-aware colors.

### `PrintPreviewDialog_3.0.xaml/.cs`
A separate `Window` (not a `UserControl`). Fixed size ~420×594px (A4 proportions
scaled down). Contains a scrollable area with a white `Border` representing the
paper, filled with WPF controls styled to the Scheme D print theme. Footer has
`Close` and `Print…` buttons. `Print…` builds a `FixedDocument` and opens the
standard `PrintDialog`. On print confirm, `this.Close()` is called.

---

## Bug Fixes in v3.0

| Bug | Fix |
|-----|-----|
| Drag top/bottom edge miss | Clamp `GetInsertionIndex` result at both ends |
| Rare list scramble on drop | Validate and clamp `_dropIndex` at `MouseUp`; skip same-position mutations |
| Font size flicker (Large↔Extra Large) | Batch scale change via `Dispatcher.InvokeAsync(DispatcherPriority.Render)` |
| Drag index drift at non-Normal font size | Use `TranslatePoint`/`TransformToAncestor` for all coordinate calculations |
| Migration dialog has two Close buttons | Single Close button; click-outside also dismisses |
| History limit blank on first load | Fix `Template.FindName` call timing |
| Double-click Add buttons plays error sound | Suppress double-click event on Add buttons |
| Gear icon renders as flower | Replace Unicode ⚙ with custom SVG path icon |
| App title bar icon missing at 100% DPI | Embed icon as compiled resource, not runtime file path |
| Assembly version hardcoded in Settings | Read from `Assembly.GetExecutingAssembly().GetName().Version` |

---

## Deferred / Future Considerations

- **Back button redesign** — current `← Back` text is acknowledged as visually weak
  but deferred. Revisit in v4.0 with more design options.
- **Multi-level nesting for CategoryGroups** — architecture note: `CategoryGroup`
  should not be designed in a way that makes adding child groups a full rewrite.
  For v3.0 depth is exactly one level. Not planned beyond v4.0.
- **Import for categories.json** — export is implemented in v3.0 (full file export).
  Import (pick a file, validate, merge or replace) is deferred to v4.0.
- **Undo for multi-level** — currently single-stack, 10 actions. A branch-aware
  undo tree is not planned.
- **Keyboard shortcut customization** — deliberately dropped. If needed in a future
  project, design it from scratch as a first-class feature, not a retrofit.
- **Export single collection** — v3.0 exports the entire `categories.json`. Exporting
  a single collection as a shareable file is a natural future addition.
- **History search / filter beyond drawn** — history rows now show Option C summary
  format. If more filtering is needed in future, text search is the natural next step.
- **A–Z sort button** — not implemented. The drag scramble bug that was misidentified
  as a sort issue has been fixed. Deliberate sort is deferred.

---

## v3.0 Changes from v2.0

| Area | v2.0 | v3.0 |
|------|------|------|
| Data hierarchy | Collection → Category → Option | Collection → CategoryGroup → Category → Option |
| Class naming | RandomCollection, RandomCategory, RandomOption | Collection, CategoryGroup, Category, Option |
| Schema versioning | `MigrationComplete` bool in settings | `SchemaVersion` integer in settings |
| Result display | Flat card grid (5 columns) | Grouped cards (one card per group, 240px fixed width) |
| Result area scrolling | Fixed height | Independent scroll within result area |
| History row | Flat dot-separated values | Option C: "Collection · N groups · N results" |
| History drawn state | None | `IsDrawn` flag, grey/green dot, drawn filter |
| Drag copy | Not available | Ctrl+drag or Ctrl+C/X/V |
| Undo | Not available | Ctrl+Z, 10-action in-memory stack |
| Delete key | Not available | Deletes selected item |
| Print | Not available | Print preview (Scheme D) + Windows PrintDialog |
| AI Prompt | Not available | Clipboard copy, Option C format |
| Preset quick-load | Buried in Manage Content | Popup from main screen summary bar |
| ManageContent sidebar | Two zones (collections + categories) | Two panels (collections panel + groups+categories panel) |
| ManageContent groups | Not available | Navigational in sidebar, detail in right panel |
| Toast notifications | Not available | ToastService, multiple triggers |
| Settings data export | Not available | Export full categories.json |
| App icon in title bar | Missing at 100% DPI | Fixed, embedded resource |
| Assembly version | Hardcoded "Version 2.0" | Read from assembly |
| Gear icon | Unicode ⚙ (renders as flower on some machines) | Custom SVG path icon |
| Tooltip delay | WPF default (slow) | 400ms |
| Theme toggle tooltip | None | Dynamic "Switch to light/dark mode" |
| Font size flicker | Present | Fixed |
| Drag edge cases | Top/bottom miss, rare scramble | Fixed |
