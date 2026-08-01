# Random Creation — Claude Code Project Guide

**App:** Random Creation (Windows desktop) · **Current version:** 3.0
**Original working title:** Creature Crafter (v1.0), renamed in v2.0.

Random Creation generates random combinations from user-defined content. The user
creates **Collections** (Creatures, Starships, Guns), adds **Category Groups** to each
(HEAD, BODY, LIMBS), adds **Categories** to each group (Head Style, Head Count), and adds
**Options** to each category (Horned, Round, Tiny). Generate picks one random option from
every enabled category in every enabled group in every enabled collection and displays the
combination as grouped result cards. The system is deliberately general purpose — it works
for any kind of random combination generator, so never hard-code assumptions about
creatures or any other subject matter.

Personal creative tool, not a commercial product. Currently in personal use and
small-pool testing.

---

## ⚠️ Where the live code is

This folder holds several copies of the source. **Only one of them is real.**

| Path | What it is | Edit it? |
|------|-----------|----------|
| `Source\RandomCreation\` | **The live Visual Studio solution.** `RandomCreation.sln` + the `RandomCreation\` project. | **YES — this is the only place to make changes.** |
| `Source Code\3.0\3.0 Release files\` | Frozen snapshot of what shipped as v3.0. | No — reference only |
| `Source Code\3.0\Phase 1..8\` | Per-phase snapshots taken during v3.0 development. | No — history |
| `Source Code\3.0\...\Claude named files\` | The same v3.0 files with `_3_0` suffixes, as uploaded to the Claude project. | No |
| `Source Code\2.0\`, `Source Code\1.0\` | Previous version archives. | No |
| `docs\` | Everything from the Claude project knowledge base — context docs, file index, changelogs, all source at every version. | Read for context |
| `Releases\` | Built output and release zips (~70 MB each). | No |

Before editing any file, confirm the path starts with `Source\RandomCreation\`. If a
search turns up five copies of `ManageContentScreen.xaml.cs`, the one to change is
`Source\RandomCreation\RandomCreation\ManageContentScreen.xaml.cs`.

---

## Tech stack and build

- **C# / WPF**, targeting **.NET 8.0 (Windows)**
- **No NuGet packages.** Serialization is `System.Text.Json`, built into .NET 8. Keep it
  that way — do not add dependencies without asking.
- Single solution, single project. Visual Studio Community 2022 is the normal IDE.
- Assembly version lives in `RandomCreation.csproj` (`AssemblyVersion` / `FileVersion`),
  currently `3.0.0.0`. The Settings About section reads it from the assembly at runtime —
  never hard-code a version string in the UI.

```
cd "Source\RandomCreation"
dotnet build
dotnet run --project RandomCreation
```

Publish profile: `RandomCreation\Properties\PublishProfiles\FolderProfile.pubxml`
(self-contained win-x64).

---

## Architecture

**Navigation.** `MainWindow` owns everything. Screens are `UserControl` panels stacked in
a root `Grid`; exactly one is `Visible` at a time (`MainPanel`, `ManageContentPanel`,
`HistoryPanel`, `SettingsPanel`, `OverlayPanel`). Overlay screens (Collections Management,
Presets) go through `ShowOverlay(UserControl)` / `HideOverlay()`. `ResultDetailDialog` and
`PrintPreviewDialog` are separate `Window` instances.

**No MVVM.** There is no binding/command layer and none should be introduced. `DataService`
is a static class; screens read and write it directly, call `DataService.SaveX()` after
every mutation, then call their own `Refresh()` to rebuild their ItemsSource. ViewModel
classes in `Models.cs` are plain UI wrappers, not an MVVM layer.

**Services are all static classes with no UI:**

| Service | Responsibility |
|---------|----------------|
| `DataService` | All persistence — load, save, migrate, presets, history, changelog |
| `ThemeService` | Swaps merged resource dictionaries; resolves System theme |
| `UndoService` | In-memory undo stack, depth 10, lambda restore actions |
| `ClipboardService` | Internal clipboard (not the system one) at Option/Category/Group level |
| `ToastService` | Bottom-centre toast overlay, 150 ms in / 2 s hold / 300 ms out |

**Data hierarchy** (`Models.cs`):

```
CategoriesData → List<Collection> → List<CategoryGroup> → List<Category> → List<Option>
```

`CategoryGroup` is the v3.0 addition. The `Random` prefix was dropped from model names in
v3.0 (`RandomCollection` → `Collection`, etc.). Depth is exactly one group level — but do
not write code that would make adding child groups a rewrite.

**Generation rule.** A category is rolled only if its collection, its group, *and* itself
are all enabled. Weighted random selection over the enabled options.

---

## Conventions that matter

- **Theming: always `DynamicResource`, never `StaticResource`,** for any brush that differs
  between themes. Both `Themes\DarkTheme.xaml` and `Themes\LightTheme.xaml` must define
  every key. `App.xaml`-level resources outcompete merged dictionaries, which is why
  `ThemeService.ApplyTheme()` explicitly reassigns `SelectedCategoryBrush` after the swap.
- **Undo: one user gesture = one undo step.** A single click that changes many items
  (Enable All, multi-select paste) reverses as one step. Push to `UndoService` for delete,
  rename, add, drag reorder/move, cut, paste, enable/disable toggle, and weight change.
  Do *not* push navigation or dialog open/close. The stack is never persisted.
- **Font scaling** is a `LayoutTransform ScaleTransform` on `MainContentGrid`
  (Normal 1.0 / Large 1.3 / Extra Large 1.7). Any drag-and-drop coordinate maths must go
  through `TranslatePoint` / `TransformToAncestor` to stay correct at non-1.0 scale — this
  was a real v3.0 bug, don't reintroduce it with a raw `e.GetPosition(container)`.
- **Drag and drop exists in three places** — `ManageContentScreen`,
  `CollectionsManagementScreen`, `PresetsScreen`. A fix to one usually belongs in all three.
  Insertion index must clamp to `[0, count]` at both ends, and `_dropIndex` is re-validated
  at `MouseUp`.
- **Name collisions** use a counter suffix: `Name (2)`, `Name (3)` — lowest integer ≥ 2 that
  is unique in the target container.
- **UI aesthetic:** WinUI 3 / Windows 11. No gradients, no drop shadows, rounded corners,
  custom borderless window with matching title bar. Dark accent `#0a84ff`, result values
  `#60aaff`, destructive `#e05050`, drawn/success `#30d158`.
- **`ManageContentScreen.xaml.cs` is ~2,400 lines** and the most complex file in the
  project. Read the relevant region before editing; it has a right-panel state machine
  (Default / GroupDetail / Options) and several parallel selection HashSets.

---

## Data files

Runtime data lives in a `data\` folder next to the exe. Copying that folder moves a user's
whole setup to another machine.

| File | Contents |
|------|----------|
| `data\settings.json` | Theme, font size, window geometry, sidebar width, history limit, confirm-on-delete, **`SchemaVersion`** |
| `data\categories.json` | All collections/groups/categories/options. Intentionally **no version field** — kept clean for hand-editing and AI-assisted editing |
| `data\history.json` | Generation history with timestamps, dim states, group membership, `IsDrawn` |
| `data\presets.json` | Named snapshots of enable/disable state down to option level |
| `data\changelog.txt` | Human-readable changelog, loaded at runtime by the Settings About section, newest at top |

**`SchemaVersion` in `settings.json` is the single source of truth for migration.**
Absent or `0` means pre-v3.0; `3` means current. `DataService.Initialise()` reads it before
loading anything else and routes to v2→v3 (wrap flat categories into one group per
collection, clear history and presets, offer a history backup) or v1→v3 (rename the old
`creature_crafter_data.json`, start fresh) or unknown→fresh (back up everything with `.bak`).

---

## Working agreements

- **Update `changelog.txt` in the same change** that adds a user-visible feature or fix.
  Newest version at top. It is the About section's content, so write it for a user.
- **Keep the docs in step.** `docs\` holds the project context document and file index. If
  a change alters architecture, data shape, or a screen's layout, say so rather than
  letting the docs silently drift.
- Ask before adding a NuGet package, introducing MVVM, or changing the JSON shape of
  `categories.json` — the last one is user-editable by design.
- The `← Back` button styling, an A–Z sort button, `categories.json` import, and
  keyboard-shortcut customisation are **deliberately deferred**, not oversights. See the
  Deferred section of the context doc before "fixing" them.

## Existing tooling in this folder

- `.claude\commands\` — slash commands already set up: `/closeout`, `/closeout-auto`,
  `/closeout-light`, `/self-check`, `/feedback-pass`, `/ai-pm-upgrade`.
- `Documents\AI PM\` — an AI Project Manager system (instructions, feedback protocol,
  README, config). Read `Documents\AI PM\AI_PM_Instructions.md` before running those
  commands, and `AI_Project_Manager_Adoption_Guide.md` at this folder's root for how it is
  meant to be used.

## Doc map

| Doc | Read it for |
|-----|-------------|
| `docs\v3.0\RandomCreation_ProjectContext_v3_0.md` | **The deep one.** Full v3.0 architecture, every screen's layout, undo/toast/clipboard/drag specs, colour palettes, bug-fix table, deferred list |
| `docs\v3.0\RandomCreation_FileIndex_v3_0.md` | What each source file does and what changed in v3.0 |
| `docs\v3.0\RandomCreation_ProjectContext_3_0.md` | Shorter v3.0 summary |
| `docs\v2.0\`, `docs\v1.0\` | Earlier context docs and full source archives |
| `docs\v3.0\changelog_3_0.txt` | Released changes, user-facing wording |
