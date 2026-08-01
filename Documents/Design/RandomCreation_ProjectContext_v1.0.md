# Random Creation — Project Context Document

## Overview

**Application name:** Random Creation
**Current version:** 1.0
**Original working title:** Creature Crafter (used during development, renamed to Random Creation)

Random Creation is a Windows desktop application that lets users build randomized creatures (or any other random combinations) by defining categories and options. The user creates categories (e.g. "Head", "Body", "Feet") and adds multiple options to each. When they click Generate, the app picks one random option from each enabled category and displays the full combination as a result. The app is designed to be general purpose — while "creature crafting" is the primary use case, the category/option system can be used for any kind of random combination generator.

---

## Intended Users

Personal creative tool. Designed for writers, game designers, hobbyists, or anyone who wants to generate random combinations of traits. Not a commercial product at this stage — currently in human testing as of v1.0.

---

## Technical Stack

- **Language:** C#
- **Framework:** WPF (Windows Presentation Foundation)
- **Target:** .NET 8.0 (Windows)
- **Serialization:** System.Text.Json (built into .NET 8, no NuGet packages needed)
- **IDE:** Visual Studio Community 2022
- **Solution type:** Single Visual Studio solution, single project

---

## Save File

- Saved as `creature_crafter_data.json` in the same folder as the executable
- Saves automatically on every change and on app close
- Contains: all categories, options, weights, toggle states, full history, last result, window size/position
- On first launch (no save file) the app opens directly on the Edit Categories screen
- The JSON file is intentionally excluded from the single-file publish so users can back it up or transfer it

---

## File Structure & Versioning

All source files are named with a version suffix (e.g. `_1.0`) so that when a new version is released, new files can be uploaded alongside old ones for reference. This allows an AI assistant in a future session to compare versions and understand what changed.

### Version 1.0 Files

| File | Purpose |
|------|---------|
| `CreatureCrafter_1.0.csproj` | Project file — targets .NET 8 WPF |
| `Models_1.0.cs` | All data models: WeightTier, CreatureOption, CreatureCategory, ResultPair, HistoryEntry, AppData, HistoryColorHelper, CategoryViewModel, OptionViewModel |
| `MainWindow_1.0.xaml` | Main screen UI — result cards, generate button, recent history |
| `MainWindow.xaml_1.0.cs` | Main screen logic — navigation, generate, save/load, refresh |
| `EditScreen_1.0.xaml` | Edit screen UI — category sidebar, options panel |
| `EditScreen.xaml_1.0.cs` | Edit screen logic — CRUD, drag & drop, weight cycling |
| `HistoryScreen_1.0.xaml` | History screen UI — full history list |
| `HistoryScreen.xaml_1.0.cs` | History screen logic — refresh, navigation |
| `ToggleSwitch_1.0.xaml` | Custom On/Off toggle control UI |
| `ToggleSwitch.xaml_1.0.cs` | Custom On/Off toggle control logic |
| `InputDialog_1.0.xaml` | Dark-themed text input dialog UI |
| `InputDialog.xaml_1.0.cs` | Input dialog logic |
| `ConfirmDialog_1.0.xaml` | Dark-themed confirmation dialog UI (replaces default Windows MessageBox) |
| `ConfirmDialog_1.0.cs` | Confirmation dialog logic |
| `ResultDetailDialog_1.0.xaml` | Popup showing full category/option pairs for a history entry |
| `ResultDetailDialog_1.0.cs` | Result detail dialog logic |

---

## UI Design

The app follows a **WinUI 3 / Windows 11 dark theme** style. No gradients, no drop shadows. Rounded corners throughout.

### Color Palette

| Element | Hex |
|---------|-----|
| Outer background | `#1c1c1e` |
| Card / panel background | `#2a2a2e` |
| Title bar / sidebar background | `#161618` |
| Borders | `#3a3a3c` |
| Selected category border | `#4a4a4e` |
| Primary text | `#e0e0e0` |
| Blue accent | `#0a84ff` |
| Result option values | `#60aaff` |
| Section labels (uppercase) | `#c0c0c8` |
| History text | `#d0d0d8` |
| Timestamps / muted text | `#a0a0a8` |
| Button hover (dark) | `#2f2f35` |
| Back button hover | `#1a2a3a` |

### Weight Badge Colors

| Tier | Background | Text |
|------|-----------|------|
| Normal | `#1e3a1e` | `#4caf50` |
| Low | `#3a2e10` | `#f0a030` |
| Rare | `#3a1a1a` | `#e05050` |

### Toggle Colors

| State | Track | Label |
|-------|-------|-------|
| On | `#0a84ff` | `#60aaff` |
| Off | `#3a3a3e` | `#666666` |

---

## Key Design Decisions

- **All buttons use custom ControlTemplates** — the default WPF button template shows a light hover color that clashes with the dark theme. Every button in the app has a custom template to control hover appearance.
- **ConfirmDialog replaces MessageBox** — the default Windows MessageBox doesn't match the dark theme. A custom ConfirmDialog window was built with matching colors and a red "Yes, Delete" button.
- **InputDialog height is 200px** — was increased from 160px to prevent buttons being clipped.
- **Result cards use UniformGrid Columns="5"** — enforces exactly 5 columns regardless of window width. Cards have a fixed height of 70px.
- **CategoryViewModel and OptionViewModel live in Models.cs** — originally placed in EditScreen.xaml.cs but moved to Models.cs so the XAML compiler can resolve the types correctly.
- **x:Name for EditScreen and HistoryScreen panels** — in MainWindow.xaml these are named `EditPanel` and `HistoryPanel` (not `EditScreen`/`HistoryScreen`) to avoid naming conflicts with the class names.
- **Navigation deferred to Loaded event** — first-launch navigation to Edit screen is triggered in the Window.Loaded event, not the constructor, so that UserControls are fully in the visual tree before Window.GetWindow() is called.
- **Dot colors are display-only** — HistoryEntry.DotColorHex is marked `[JsonIgnore]` and assigned at display time via HistoryColorHelper, so colors are never written to the save file.
- **Toggle visual state initialized on Loaded** — ToggleSwitch.UpdateVisual() is called in the Loaded event to ensure correct initial appearance when bound to a false value.
- **Drag and drop** — implemented for both categories and options using WPF DragDrop.DoDragDrop, triggered from drag handle icons (⠿). Uses visual tree hit-testing to find the drop target.

---

## Window

- Default size: 1050 x 700
- Minimum size: 750 x 550
- Resizable: yes
- Size and position saved and restored between sessions

---

## Weighting System

Options have three weight tiers used during randomization:

| Tier | Weight | Meaning |
|------|--------|---------|
| Normal | 3 | Standard probability |
| Low | 2 | Less likely |
| Rare | 1 | Least likely |

New options always default to Normal. Clicking a weight badge cycles Normal → Low → Rare → Normal.

---

## History

- Every generation is saved to history with a timestamp
- Main screen shows last 3 entries in the Recent History card
- Full history available via the clock icon → History screen
- Clicking any history entry opens a ResultDetailDialog showing all category/option pairs
- Dot colors cycle through a palette of 5 colors (assigned at display time, not saved)

---

## Known Issues / Future Work (Post v1.0)

- No application icon set yet (planned)
- App name in UI still shows "Creature Crafter" in some places — needs updating to "Random Creation" for v2.0
- No way to export or share a generated result as text
- No way to import/export category sets
- Drag and drop has no visual drag ghost — the item just moves on drop with no preview
- No undo functionality for deletions

---

## Distribution (v1.0)

Published as a self-contained win-x64 build. Produces 6 files:
- `CreatureCrafter.exe` (165 MB, includes .NET 8 runtime)
- 5 WPF graphics DLLs that must stay alongside the exe

Shared as a zip file containing all 6 files. No installer. User unzips and runs directly.
