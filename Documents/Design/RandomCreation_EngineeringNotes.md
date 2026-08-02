# Random Creation — Engineering Notes

**Version: 1.0 — August 2026.** Authoritative for CODE-LEVEL ENGINEERING KNOWLEDGE: the traps,
the patterns, and the reasons behind implementation choices that reading the code does not
reveal. Distinct in kind from `RandomCreation_ProjectContext_v3_0.md`, which is the design
record — screens, layouts, colour palettes, data models, what the app *is*. This doc is what was
learned building it.

Renamed from `RandomCreation_ProjectContext_3_0.md` at the v3.3 close-out. The old name differed
from the design record's by a single underscore and read as a duplicate of it; a comparison
confirmed the opposite — none of the material below appears in that document. Two sections were
cut at the same time: a Claude-project-knowledge file-naming scheme that no longer applies, and
an output-file-locations section that stated `categories.json` belongs in the `data\` folder,
which contradicts the ruling recorded in `RandomCreation_DevelopmentLifecycle.md` section 7 and
describes the exact mistake that would destroy a user's content.

---

## What This App Is
A WPF/.NET 8 desktop app for generating random combinations. The user organises
content into Collections → Groups → Categories → Options and presses Generate to
get one random option per enabled category, weighted by tier. Results display as
grouped cards, save to history, and can be printed or copied as an AI prompt.

---

## Tech Stack
- **.NET 8 / WPF** — C#, XAML
- **No third-party libraries** — pure WPF, System.Text.Json for serialisation
- **Custom borderless window** — `WindowChrome`, manual resize/drag
- **Theming** — runtime merged dictionary swap via `ThemeService`
- **Data** — flat JSON files in `data/` folder next to the exe

---

## Data Hierarchy
```
CategoriesData
  └── List<Collection>
        └── List<CategoryGroup>          ← added in v3.0
              └── List<Category>
                    └── List<Option>
```
`SchemaVersion` in `settings.json` controls migration. Value `3` = v3.0.

---

## Key Architectural Patterns

### Services layer (static classes)
- `DataService` — all persistence, migration, preset apply/capture
- `ThemeService` — runtime theme swap + `SelectedCategoryBrush` update
- `UndoService` — lambda stack, max 10, `Push(desc, action)` / `Undo()`
- `ClipboardService` — internal clipboard, option/category/group level
- `ToastService` — animated overlay toast, `Register()` once in MainWindow.Loaded

### Screen navigation
`MainWindow` owns all panels. `NavigateToX()` methods show/hide panels.
`OverlayPanel` (ContentControl) hosts `CollectionsManagementScreen` and
`PresetsScreen` as overlays via `ShowOverlay(UserControl)`.

### ManageContentScreen state
15+ interdependent state variables. Key ones:
```csharp
Collection?            _selectedCollection
CategoryGroup?         _selectedGroup
HashSet<CategoryGroup> _selectedGroups      // multi-select
HashSet<CategoryGroup> _expandedGroups
Category?              _selectedCategory
HashSet<Category>      _selectedCategories  // multi-select
Option?                _selectedOption
HashSet<Option>        _selectedOptions     // multi-select
string                 _searchText
bool                   _dragActive
DragMode               _dragMode
```
`RefreshGroupsPanel()` rebuilds the entire sidebar from scratch on every change.
`BuildGroupViewModel()` handles search filtering, expand state, selection state
and cut state in one pass.

### ViewModels
Thin wrappers — created fresh on every `RefreshGroupsPanel()` call. They hold
`IsSelected`, `IsVisible`, `IsCut` flags computed at build time. No
`INotifyPropertyChanged` needed for most properties since the whole list
rebuilds. `OptionViewModel` implements `INotifyPropertyChanged` for weight
percentage display which updates live.

### WPF DataTrigger gotchas — IMPORTANT
Three rules learned the hard way in v3.0:

1. **`Background="Transparent"` blocks triggers.** Use element syntax instead:
   ```xml
   <Border.Background>
       <SolidColorBrush Color="Transparent"/>
   </Border.Background>
   ```
   This creates a mutable instance the trigger can replace. The attribute syntax
   creates a shared frozen brush that cannot be changed.

2. **`DynamicResource` in `DataTemplate.Triggers` needs the resource in
   `App.xaml`.** Resources defined only in theme files (merged dictionaries)
   may not resolve in `DataTrigger Setter Value="{DynamicResource ...}"` because
   merged dicts have lower precedence than `App.xaml`. Solution: define the
   resource in `App.xaml` as a fallback AND update it in `ThemeService.ApplyTheme()`
   using `Application.Current.Resources[key] = newBrush`.

3. **`SelectedCategoryBrush` specifically** is defined in `App.xaml` (dark value
   `#1e2a3a`) and updated by `ThemeService.ApplyTheme()` on every theme swap
   (light = `#e0eaf8`, dark = `#1e2a3a`). This is the selection highlight colour
   for options, categories, collections and presets.

### Weight tier probability
6 tiers with fixed anchor points:
- UltraHigh = 98% guaranteed (pool weight calculated to achieve this)
- UltraRare = 2% guaranteed
- High, Normal, Low, Rare share the remaining probability proportionally
Left-click badge cycles up, right-click cycles down.

---

## Known Issues / Code Quality Notes

### ManageContentScreen is a god class
2,400+ lines, 15+ state variables, handles selection, drag, inline edit,
keyboard, search, clipboard, undo — all in one file. Works correctly but is
hard to extend. Flagged for a future refactor.

### Full-rebuild pattern
`RefreshGroupsPanel()` rebuilds the entire sidebar on every change. Correct and
predictable but will feel slow with very large datasets (hundreds of categories).
Should be replaced with `ObservableCollection` + `INotifyPropertyChanged`.

### Dual selection tracking
`_selectedOption` (single) and `_selectedOptions` (HashSet) coexist for
historical reasons. Both must be kept in sync. Consolidate when the god class
is broken up.

---

## Future Refactor and Feature Candidates

**None of these are in v4.0.** This list was written in the v3.0 era under the heading
"v4.0 Todo List", when "v4.0" meant "some future major version." The actual v4.0 —
see `RandomCreation_ReleasePlan_v4_0.md` — contains none of it. Retitled at the v3.3
close-out so it cannot be read as a promise the release failed to keep.

- **Refactor ManageContentScreen** into proper ViewModels — split selection
  manager, drag handler, inline edit controller, keyboard handler
- **Replace `RefreshGroupsPanel` full-rebuild** with `ObservableCollection`
  differential updates
- **Consolidate `_selectedOption` / `_selectedOptions`** into single HashSet
- **Extract `InlineEditTextBox`** as a reusable custom control
- **Redo (Ctrl+Y)** — requires UndoService branch-aware stack
- **Weight tier customization** in Settings
- **Import categories.json** — complement to existing export
- **Export single collection** as shareable file
- **Full group detail interactivity** — drag, copy, full edit parity with sidebar

---

## Startup Data Paths

**Rewritten for v4.0** (release plan BUG 2 — the v1/v2 migration paths, `MigrationDialog`
and the `V2*`/`Legacy*` model classes were deleted outright; no user held pre-v3.0 data).
`DataService.Initialise()` now has exactly three cases:

- **No data files at all:** fresh start, no dialog. The shipped sample is copied from
  `samples\categories.json` into the data folder (CHANGE 5) — only ever when no
  `categories.json` exists, so it can never overwrite user content.
- **`SchemaVersion` == 3:** load normally.
- **Anything else** (older, newer, or corrupt data): every existing data file is MOVED
  to a `.bak` name → fresh start (sample installs, since the folder is now empty) →
  `NoticeDialog` shown. This catch-all is what stands between "no migration code" and
  silent data loss: an old-format `categories.json` would deserialise into the current
  model as an empty structure and be overwritten on the next save.

The old bug to not reintroduce: never create the data folder before deciding which case
applies — shipped v3.0 called `EnsureDataDir()` first, which made the "fresh install"
test (`!Directory.Exists(DataDir)`) unpassable and routed every new user into the v2→v3
message.

**Where data lives (v4.0):** `DataService.IsPortable` — a `portable.txt` marker beside
the exe (shipped in the portable zip, excluded by the installer) — decides between
`data\` beside the exe and `%LocalAppData%\RandomCreation\`. `changelog.txt` and
`samples\` are PROGRAM files beside the exe; the app reads them and never writes them.
