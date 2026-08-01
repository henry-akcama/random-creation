# Random Creation — Project Context v3.0 Final

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

## File Naming in Project Knowledge
Claude project knowledge files use version suffixes for tracking:
`FileName_3_0.xaml`, `FileName_xaml_3_0.cs`, `FileName_3_0.cs`
Actual on-disk project files have no suffix.
`_2_0` files are still accurate for files not modified in v3.0.
See `RandomCreation_FileIndex_v3_0.md` for a complete file-by-file reference.

---

## Known Issues / Code Quality Notes

### ManageContentScreen is a god class
2,400+ lines, 15+ state variables, handles selection, drag, inline edit,
keyboard, search, clipboard, undo — all in one file. Works correctly but is
hard to extend. Flagged for v4.0 refactor.

### Full-rebuild pattern
`RefreshGroupsPanel()` rebuilds the entire sidebar on every change. Correct and
predictable but will feel slow with very large datasets (hundreds of categories).
Should be replaced with `ObservableCollection` + `INotifyPropertyChanged` in v4.0.

### Dual selection tracking
`_selectedOption` (single) and `_selectedOptions` (HashSet) coexist for
historical reasons. Both must be kept in sync. Consolidate in v4.0.

---

## v4.0 Todo List
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

## Migration Paths
- **v1.0 → v3.0:** Old `creature_crafter_data.json` detected → renamed to `.bak`
  → fresh start → `NoticeDialog` shown
- **v2.0 → v3.0:** `SchemaVersion` absent or 0 → each collection's flat category
  list wrapped into one `CategoryGroup` named after the collection → history and
  presets cleared → backup prompt → `MigrationDialog` shown
- **Unknown version:** All files backed up as `.bak` → fresh start → `NoticeDialog`

---

## Output Files Location
During development, Claude writes output files to `/mnt/user-data/outputs/`.
The user copies them into the actual project at:
`Source/RandomCreation/RandomCreation/`
Theme files go in the `Themes/` subfolder.
Data files (`changelog.txt`, `categories.json`) go in the `data/` folder
next to the built exe.
