Random Creation — WPF desktop app (C# / .NET 8)

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

--- AI PM — keep this block intact ---
This project runs on AI Project Manager (AI PM).
All paths below are relative to this project's folder.
Session start: read Documents\AI PM\AI_PM_Instructions.md in full and
follow it.
Close-out (/closeout, /closeout-light, /closeout-auto — these commands
only): read Documents\AI PM\AI_Project_Manager.md in full and follow it.
--- end AI PM block ---

---

## ⚠️ Where the live code is

**Only one copy of the source is real.**

| Path | What it is | Edit it? |
|------|-----------|----------|
| `Source\RandomCreation\` | **The live Visual Studio solution.** `RandomCreation.sln` + the `RandomCreation\` project. | **YES — this is the only place to make changes.** |
| `docs\` | Claude project knowledge base — source snapshots at v1.0/v2.0/v3.0. Reference only. Slated for reorganization; the folder does not survive under that name. | Read for context |
| `Documents\Design\` | Design records, screenshots by version, icon assets. | Read for context |
| `Releases\` | Built releases (v1.0 source + build, v2.0/v3.0 zips). Git-ignored. | No |

Before editing any file, confirm the path starts with `Source\RandomCreation\`. If a
search turns up several copies of `ManageContentScreen.xaml.cs`, the one to change is
`Source\RandomCreation\RandomCreation\ManageContentScreen.xaml.cs`.

---

## Version control

Git arrived with the AI PM adoption (July 2026) and the developer is **new to git and
GitHub — explain git operations plainly as they happen**, in plain language, at the time
they run. `.gitignore` excludes build output (`bin\`, `obj\`, `.vs\`), the `Releases\`
archive, `Thumbs.db`, and `*.csproj.user`; those stay on disk but out of the repository.
Everything else — live source, `docs\`, `Documents\` — is tracked.

**Deletions must be loudly flagged in advance or handed to the developer** — never
deleted quietly. Tracked files are recoverable from git history after deletion; ignored
and untracked files (anything under `Releases\`, or any `bin\`/`obj\` folder) have no
safety net at all, so those need an explicit ruling every time.

---

## Tech stack and build

- **C# / WPF**, targeting **.NET 8.0 (Windows)**
- **No NuGet packages.** Serialization is `System.Text.Json`, built into .NET 8. Keep it
  that way — do not add dependencies without asking.
- Single solution, single project. Visual Studio Community 2022 is installed; the .NET
  SDK it ships (9.0.307) builds this project from the command line in about seven
  seconds, so Visual Studio does not need to be open to build or run.
- Assembly version lives in `RandomCreation.csproj` (`AssemblyVersion` / `FileVersion`),
  currently `3.0.0.0`. The Settings About section reads it from the assembly at runtime —
  never hard-code a version string in the UI.

```
cd "Source\RandomCreation"
dotnet build
dotnet run --project RandomCreation
```

Publish profile: `RandomCreation\Properties\PublishProfiles\FolderProfile.pubxml`
(self-contained win-x64). A published build is ~170 MB uncompressed, ~70 MB zipped,
because it bundles the whole .NET runtime.

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
  **Known bug, found August 2026, unfixed:** the app loads themes from `Themes\`, but those
  two files are byte-identical to the **v2.0** themes. The updated v3.0 themes sit
  unreferenced at the project root, dated four hours before the v3.0 release was built —
  so shipped v3.0 runs v2.0 theme dictionaries. It compiles cleanly, which is why it
  shipped. On the bug list; do not half-fix it in passing.
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

The shipped v3.0 release carries a starter `data\categories.json` as sample content. The
root-level `categories.json` in this folder is a sample for people testing the app; its
permanent home is an open question.

---

## How we work together

(Ported July 2026 from the developer's IFQ Tracker project; preferences to apply with
judgment, not rigid rules.)

- **Working pace.** When the developer gives a short reply like "do what you think is
  best next," "sounds good," or "go ahead," read it as trust in Claude's judgment about
  the next action — NOT as a signal to go faster, skip confirmation, or stop asking. The
  developer sets the pace, and asking is always welcome. If a real decision or fork
  arises (a scope choice, a design tradeoff, or anything Claude itself flagged as an open
  question), Claude raises it even right after being told to proceed, rather than
  resolving it silently to save a turn. Prefer plain language over project shorthand when
  introducing a step the developer may not have seen named before, and state milestones
  plainly when they are reached.
- **Information pacing.** A full picture up front is welcome when it helps the developer
  see the shape of something — the map before the walk. But work through that picture in
  digestible chunks, one piece at a time, confirming before moving on, rather than
  presenting a wall of detail to act on all at once. Coupled points get decided together;
  independent points get worked one at a time. When in doubt, the developer prefers
  smaller chunks while working, even if the overview was large.
- **Recorded decisions vs. better ideas.** Locked/recorded rulings are CURRENT DECISIONS
  with provenance, not laws — their job is to prevent accidental drift and silent
  re-litigation, never to suppress improvement. If Claude has an idea that a recorded
  ruling seems to forbid, the idea wins the airtime: surface it, name the ruling it
  challenges, and let the developer decide. Frame rulings as "current decision" rather
  than "hard rule" in prose.

---

## Working agreements

- **Update `changelog.txt` in the same change** that adds a user-visible feature or fix.
  Newest version at top. It is the About section's content, so write it for a user.
- **Keep the docs in step.** `Documents\Design\` holds the project context document and
  file index. If a change alters architecture, data shape, or a screen's layout, say so
  rather than letting the records silently drift.
- Ask before adding a NuGet package, introducing MVVM, or changing the JSON shape of
  `categories.json` — the last one is user-editable by design.
- The `← Back` button styling, an A–Z sort button, `categories.json` import, and
  keyboard-shortcut customisation are **deliberately deferred**, not oversights. See the
  Deferred section of the context doc before "fixing" them.

## Doc map

| Doc | Read it for |
|-----|-------------|
| `Documents\Design\RandomCreation_ProjectContext_v3_0.md` | **The deep one.** Full v3.0 architecture, every screen's layout, undo/toast/clipboard/drag specs, colour palettes, bug-fix table, deferred list |
| `Documents\Design\RandomCreation_FileIndex_v3_0.md` | What each source file does and what changed in v3.0 |
| `Documents\Design\RandomCreation_ProjectContext_3_0.md` | Shorter v3.0 summary |
| `Documents\Design\RandomCreation_ProjectContext_v2.0.md`, `_v1.0.md` | Earlier version design records |
| `Documents\Design\Screen Shots\` | UI screenshots by version (v1.0, v2.0, v3.0) |
| `docs\v3.0\changelog_3_0.txt` | Released changes, user-facing wording |
