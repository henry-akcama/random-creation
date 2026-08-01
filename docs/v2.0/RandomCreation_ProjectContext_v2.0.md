# Random Creation — Project Context Document

## Overview

**Application name:** Random Creation
**Current version:** 2.0
**Original working title:** Creature Crafter (v1.0), renamed to Random Creation in v2.0

Random Creation is a Windows desktop app for generating random combinations from
user-defined categories and options. The user creates Collections (e.g. Creatures,
Starships, Guns), adds Categories to each (e.g. Head, Body, Legs), and adds Options
to each Category (e.g. Horned, Round, Tiny). Hitting Generate picks one random
option from each enabled category in each enabled collection and displays the full
combination as a result card grid. The app is intentionally general purpose — the
collection/category/option system works for any kind of random combination generator.

---

## Intended Users

Personal creative tool. Designed for writers, game designers, hobbyists, or anyone
who wants to generate random combinations of traits. Currently in human testing /
personal use. Not a commercial product.

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
machine transfers the full user setup. The folder is created at runtime by
`DataService.Initialise()` if it doesn't exist.

| File | Contents |
|------|----------|
| `data/settings.json` | Theme, font size, window geometry, sidebar width, history limit, confirm-on-delete |
| `data/categories.json` | All collections, categories and options with weights and enable/disable states |
| `data/history.json` | Full generation history with timestamps and dim states |
| `data/presets.json` | Saved presets (named snapshots of enabled/disable states) |
| `data/changelog.txt` | Human-readable changelog, loaded at runtime by Settings screen |

The `changelog.txt` is also included in the build output via the .csproj `Content`
item with `TargetPath=data\changelog.txt` so it ships with every release.

**v1.0 migration:** On first launch, if `creature_crafter_data.json` exists next to
the exe and no `data/` folder exists, `DataService` automatically migrates all
categories and history into a new collection called "My Collection" and renames the
old file to `creature_crafter_data.json.bak`. A one-time dialog informs the user.

---

## File Structure & Versioning

Source files use a version suffix (e.g. `_2.0`) so new and old files can coexist
during development. The v1.0 files remain in the project folder for reference.

### Version 2.0 Files

| File | Purpose |
|------|---------|
| `RandomCreation.csproj` | Project file — .NET 8 WPF, self-contained win-x64 publish |
| `App.xaml` | Application entry point, global resources, converter registrations |
| `App.xaml.cs` | DataService.Initialise() → ThemeService.Apply() → window shows |
| `Models.cs` | All data models, ViewModels, enums and helper classes |
| `DataService.cs` | All persistence — load, save, migrate, presets, history, changelog |
| `ThemeService.cs` | Theme resource dictionary swapping, system default detection |
| `ScaleConverter.cs` | All WPF value converters used in XAML bindings |
| `Themes/DarkTheme.xaml` | Full dark theme color palette and scrollbar styles |
| `Themes/LightTheme.xaml` | Full light theme color palette and scrollbar styles |
| `PillToggle.xaml` / `.cs` | Custom ON/OFF pill control using DynamicResource Style triggers |
| `MainWindow.xaml` / `.cs` | Main screen, borderless window, navigation, generate logic |
| `ManageContentScreen.xaml` / `.cs` | Two-zone sidebar, options panel, full drag and drop system |
| `CollectionsManagementScreen.xaml` / `.cs` | Add/rename/delete/reorder collections |
| `HistoryScreen.xaml` / `.cs` | Full history list with delete |
| `PresetsScreen.xaml` / `.cs` | Presets list, save/load/rename/delete/reorder |
| `SettingsScreen.xaml` / `.cs` | Theme, font size, history, behavior, window, changelog |
| `ResultDetailDialog.xaml` / `.cs` | Borderless popup showing full category/option pairs |
| `ConfirmDialog.xaml` / `.cs` | Borderless themed confirm dialog, red or blue button variant |
| `InputDialog.xaml` / `.cs` | Borderless themed text input dialog with inline validation |
| `SavePresetDialog.xaml` / `.cs` | Preset save dialog — new name or overwrite existing |

---

## Architecture

### Navigation
`MainWindow` owns all navigation. Screens are `UserControl` panels stacked in a
root `Grid` — only one is `Visible` at a time. Overlay screens (Collections
Management, Presets) are shown via `ShowOverlay(UserControl)` / `HideOverlay()`
which places them on top of the current panel using an overlay `ContentControl`.

### Data flow
`DataService` is a static class. All screens read from and write to it directly.
There is no MVVM binding layer between screens and data — screens call
`DataService.SaveX()` after every mutation and call their own `Refresh()` to
rebuild their ItemsSource from the current data state.

### Theme system
`ThemeService.ApplyTheme()` swaps the merged resource dictionary in
`Application.Current.Resources` between `DarkTheme.xaml` and `LightTheme.xaml`.
All XAML uses `DynamicResource` bindings so every element updates instantly on swap.
System default is resolved by reading the Windows registry key
`HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize\AppsUseLightTheme`.

### Font scaling
`MainWindow.ApplyFontScale()` sets `LayoutTransform = new ScaleTransform(scale, scale)`
on `MainContentGrid` (the inner content Grid, Row 1 of the root). This scales all
visual content proportionally — text, padding, row heights, card heights. The title
bar (Row 0) is excluded. Scale factors: Normal=1.0, Large=1.3, Extra Large=1.7.

### Drag and drop (ManageContentScreen)
Uses raw mouse capture on the Window — no `DragDrop.DoDragDrop`. A Canvas overlay
(`DragOverlay`) covers the entire screen at z-order top with `IsHitTestVisible=False`.
Ghost and insertion line are drawn directly on this Canvas using coordinates in
`ManageContentScreen` space (`e.GetPosition(this)`). This avoids all coordinate
transform issues. Cross-container drop targets (collection for category drag,
category for option drag) are stored as `_pendingCollectionTarget` /
`_pendingCategoryTarget` during `MouseMove` and committed on `MouseUp` — the overlay
is hidden before committing so it doesn't interfere with hit testing.

---

## UI Design

The app follows a WinUI 3 / Windows 11 dark theme aesthetic. No gradients, no drop
shadows. Rounded corners throughout. Custom borderless window with a matching title
bar. All buttons use custom `ControlTemplate`s to control hover appearance.

### Color Palette — Dark Theme

| Element | Hex |
|---------|-----|
| Outer background | `#1c1c1e` |
| Card / panel background | `#2a2a2e` |
| Title bar / sidebar background | `#161618` |
| Expanded row background | `#1e1e22` |
| Borders (standard) | `#3a3a3c` |
| Borders (selected/active) | `#4a4a4e` |
| Borders (subtle) | `#2a2a2c` |
| Primary text | `#e0e0e0` |
| Section labels | `#c0c0c8` |
| Muted text / timestamps | `#a0a0a8` |
| History text | `#d0d0d8` |
| Blue accent | `#0a84ff` |
| Blue accent hover | `#1a8fff` |
| Result option values | `#60aaff` |
| Delete red | `#e05050` |
| Hover (dark) | `#2f2f35` |
| Back button hover | `#1a2a3a` |
| Nav icons | `#808088` |
| Empty state text | `#505058` |
| Dialog background | `#2a2a2e` |
| Dialog border | `#3a3a3c` |
| Input background | `#1c1c1e` |
| Button dark | `#3a3a3c` |
| Button dark hover | `#4a4a4e` |
| Window button hover | `#3a3a3c` |
| Window button close hover | `#c42b1c` |
| Window button foreground | `#c0c0c8` |

### Color Palette — Light Theme

| Element | Hex |
|---------|-----|
| Outer background | `#f0f4f8` |
| Card / panel background | `#ffffff` |
| Title bar / sidebar background | `#e2e8f0` |
| Expanded row background | `#f8fafc` |
| Primary text | `#1a202c` |
| Muted text | `#64748b` |
| Section labels | `#475569` |
| Blue accent | `#0a84ff` |
| Result option values | `#0055bb` |
| Hover (dark) | `#d0dae8` |

### Weight Badge Colors

| Tier | Background | Text |
|------|-----------|------|
| Normal | `#1e3a1e` | `#4caf50` |
| Low | `#3a2e10` | `#f0a030` |
| Rare | `#3a1a1a` | `#e05050` |

### ON/OFF Pill Colors

| State | Background | Border | Foreground |
|-------|-----------|--------|-----------|
| ON | `#1a3a5a` | `#0a84ff` | `#60aaff` |
| OFF | `#2a2a2e` | `#3a3a3c` | `#606068` |

---

## Key Design Decisions

- **All buttons use custom ControlTemplates** — default WPF button hover clashes
  with dark theme. Every interactive element has a custom template.

- **ConfirmDialog replaces MessageBox** — matches the theme. Two overloads:
  destructive (red "Yes, Delete") and non-destructive (blue custom label pair).
  Blue button template is built programmatically via `FrameworkElementFactory`
  with `SetResourceReference` so it responds to theme changes.

- **PillToggle uses DynamicResource Style triggers** — not converters. Converters
  fetch brush values at bind time and go stale when the theme dictionary swaps.
  Style triggers with `DynamicResource` re-evaluate automatically on theme change.

- **Canvas overlay for drag and drop** — `DragDrop.DoDragDrop` blocks the UI
  thread preventing live updates. Raw mouse capture on the Window with a Canvas
  overlay allows the ghost and insertion line to update continuously. The Canvas
  uses `ManageContentScreen` coordinate space throughout — no screen coordinate
  transforms needed.

- **Pending target pattern for cross-container drops** — `VisualTreeHelper.HitTest`
  and container `ActualHeight` are unreliable at `MouseUp` time because hiding the
  overlay triggers a layout pass that zeroes container sizes. The solution is to
  store the detected target ViewModel during `MouseMove` (when layout is stable)
  and consume it at `MouseUp` without any hit testing.

- **LayoutTransform for font scaling** — setting `FontSize` on the Window doesn't
  override hardcoded `FontSize` values in XAML. `LayoutTransform ScaleTransform`
  on the content grid scales everything visually including explicitly-sized elements.

- **Navigation via panel visibility** — no page navigation framework. Screens are
  UserControls stacked in the root Grid. `NavigateToX()` methods set Visibility.
  Overlay screens use a separate `OverlayPanel` ContentControl on top.

- **DataService is static** — simplifies access from all screens without dependency
  injection. All screens call `DataService.SaveX()` directly after mutations.

- **x:Name for ManageContentScreen named zones** — `CollectionsZone` (Grid),
  `CategoryScrollViewer` and `OptionsScrollViewer` are named so drag hit testing
  uses zone bounds rather than ItemsControl bounds (which report 0 height).

- **Drag uses `_lastDragPt`** — at `MouseUp` time `e.GetPosition(this)` can differ
  slightly from the last `MouseMove` position. The last move position is stored in
  `_lastDragPt` and used for zone detection at commit time.

---

## Window

- Default size: 1050 x 700
- Minimum size: 800 x 600
- Resizable: yes — all edges and corners
- Borderless custom title bar with `WindowChrome`
- Size, position and sidebar width saved and restored between sessions

---

## Weighting System

| Tier | Pool entries | Meaning |
|------|-------------|---------|
| Normal | 3 | Standard probability |
| Low | 2 | Less likely |
| Rare | 1 | Least likely |

Only enabled options enter the pool. Disabled options always show 0% probability.
New options default to Normal. Clicking the weight badge cycles Normal→Low→Rare→Normal.

---

## History

- Every generation saved with timestamp, full result pairs and dim states
- Main screen shows last 3 entries in the Recent History card
- Full history on the History screen — click any entry to open ResultDetailDialog
- Entries can be deleted individually or all at once via Settings → Clear All History
- History limit configurable in Settings (default 500, or unlimited)
- Dot colors cycle through 5 colors, assigned at display time, not saved

---

## Collections

- Top-level container for categories
- Managed via the Collections Management screen (gear icon in sidebar)
- Enable/disable per collection — only enabled collections appear in Manage Content
  sidebar and are included in generation
- Reorderable via drag and drop
- v1.0 migration: all existing categories moved into "My Collection" automatically

---

## Presets

- Saved snapshots of the full enabled/disabled state (collection, category and
  option level)
- Stored in `data/presets.json`
- Accessible via bookmark icon in Manage Content title bar
- Rows collapsed by default showing name and summary counts
- Expandable to show per-collection breakdown (enabled collections only)
- Save current state as new preset or overwrite existing from same dialog
- Load with confirmation, returns user to previous screen after loading
- Rename, delete, reorder via drag and drop

---

## Drag and Drop

Implemented in three screens: ManageContentScreen, CollectionsManagementScreen,
PresetsScreen. All use the same Popup-based approach (Collections and Presets) or
Canvas overlay approach (ManageContent).

### ManageContentScreen (Canvas overlay)
- **Category reorder** within same collection — blue insertion line between items
- **Category to different collection** — target collection row highlights blue
- **Option reorder** within same category — blue insertion line
- **Option to different category** — target category row highlights blue
- **Invalid zone** — ghost border turns red, no line, no highlight
- Ghost width resizes to match the list currently being hovered

### CollectionsManagementScreen and PresetsScreen (Popup-based)
- Same-list reorder only
- Ghost Popup follows cursor, insertion line Popup shows landing point
- Ghost turns red outside the valid list zone

---

## Distribution

Published as a self-contained win-x64 build via Visual Studio Publish.
Settings: Release, Self-contained, win-x64, Produce single file.
Output: `RandomCreation.exe` (~165MB) plus a small number of WPF DLLs that
cannot be bundled, `icon.ico`, and `data/changelog.txt`.
Shared as a zip named `Random Creation v2.0.zip`. No installer. User unzips and
runs. Windows SmartScreen warning appears on first run — users click "More info"
then "Run anyway" (exe is not code-signed).

---

## Known Issues / v2.1 Backlog

### Bugs
- **Migration dialog has two Close buttons** — called with `"Close", "Close"` on
  both button labels. Should use a single OK button instead.
- **History limit defaults blank** — Settings screen history limit button text is
  blank on first load. Should show "Set unlimited" or "Set limit (500)" correctly
  on first render. Issue is `Template.FindName` called before template is applied.
- **Double-click on Add Option / Add Category / Add Collection plays system error
  sound** — the button's default WPF behavior fires an error beep on double-click.
  Should suppress the sound by handling or ignoring the double-click event.
- **Title bar icon missing on some machines** — icon displays correctly at 125%+
  DPI scaling but missing at 100% on machines with integrated graphics. Fix by
  embedding icon as a compiled resource rather than loading from file path at
  runtime.

### UI / Visual Issues
- **Settings gear icon looks like a flower** — the Unicode ⚙ character renders
  differently across Windows builds. Replace with a custom SVG path gear icon
  matching the style of the other custom icons in the app.
- **Collections gear icon in Manage Content also looks like a flower** — same fix
  as above.
- **No tooltip on dark/light theme toggle button** — add a tooltip that shows
  "Switch to light mode" or "Switch to dark mode" depending on current state.
- **Tooltip hover delay is slow** — the default WPF tooltip delay feels sluggish.
  Reduce `ToolTipService.InitialShowDelay` globally in App.xaml.
- **Program title blends in with sub-section titles** — "Random Creation" in the
  main title bar and "Manage Content" / "History" etc. in sub-screens look too
  similar in weight and size. The app title needs to be more visually distinct —
  consider larger size, different weight, or a colored accent on the app name.
- **Manage Content category zone label overlaps Enable All / Disable All** — when
  a collection name is long, "CATEGORIES — LONG COLLECTION NAME" overlaps the bulk
  controls. Move Enable All / Disable All to a second line below the section label,
  or right-align them with a minimum gap enforced.
- **Collections and Categories in Manage Content don't stand out clearly** — the
  section labels (COLLECTIONS, CATEGORIES) are not prominent enough relative to the
  collection/category names. The hierarchy is hard to read at a glance, especially
  with only one collection. Consider stronger visual separation — background tint,
  larger section labels, or a more distinct header treatment.
- **Back button in Manage Content is hard to find** — the ← Back button in the
  top-left is easy to miss. Consider alternate or supplementary navigation — a
  persistent bottom bar, a more prominent button style, or Escape key hint text.

### Features
- **Copy/paste for categories and options** — deferred from v2.0. Copy a category
  (with all its options) or an individual option. Paste into any compatible target.
  Duplicate names get a counter suffix e.g. "Horned (2)". Triggered via right-click
  context menu or Ctrl+C / Ctrl+V.
- **Assembly version shown dynamically in Settings** — currently hardcoded as
  "Version 2.0". Should read from
  `Assembly.GetExecutingAssembly().GetName().Version` so bumping the .csproj
  `<Version>` tag automatically updates the Settings display.
- **Preset quick-load button on main screen** — add a Presets button to the main
  screen for fast access without going through Manage Content. After loading a
  preset return the user to whichever screen they were on when they opened it.
- **First-run demo / walkthrough** — new users with no data need guidance. Options
  to consider: a short overlay walkthrough on first launch, a "Getting Started"
  help screen accessible from Settings, or a sample data file bundled with the
  release. A sample `categories.json` with a "Sample Collection" containing
  example categories and options would serve as both a demo and a template.

### Sample Data
- **Bundle sample categories.json with release** — include a starter file with a
  collection called "Sample Collection" containing representative categories and
  options so new users can see the app working immediately without having to build
  their own data from scratch. The four-collection sample (Creatures, Guns, Swords,
  Ships) created during v2.0 testing is a good candidate.

---

## v2.0 Changes from v1.0

| Area | v1.0 | v2.0 |
|------|------|------|
| App name | Creature Crafter | Random Creation |
| Namespace / exe | CreatureCrafter | RandomCreation |
| Data storage | Single `creature_crafter_data.json` | 4 JSON files in `data/` folder |
| Category organization | Flat list | Collections → Categories → Options |
| Edit screen | Edit Categories (single panel) | Manage Content (two-zone sidebar) |
| Drag and drop | Basic jump-on-drop | Ghost + insertion line + cross-container |
| Themes | Dark only | Dark, Light, System Default |
| Font size | Fixed | Normal / Large / Extra Large via LayoutTransform |
| Toggle control | Custom ToggleSwitch | ON/OFF Pill (DynamicResource triggers) |
| Window | Standard Windows title bar | Custom borderless with WindowChrome |
| Result cards | Fixed 5-column UniformGrid | Same, plus click-to-dim |
| History | View only | View, delete individual, delete all |
| Presets | None | Full presets system |
| Settings | None | Dedicated Settings screen |
| Changelog | None | Loaded from `data/changelog.txt` |
| Confirm dialogs | Windows MessageBox | Custom themed ConfirmDialog |
| Summary bar | None | Active collections + counts on main screen |
| Sidebar | Fixed width | Resizable, width saved |
