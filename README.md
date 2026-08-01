# Random Creation

**Random Creation builds random combinations out of content you define yourself.**

Describe the pieces of something — a creature, a starship, a gun, a character, a
planet — and let the app assemble them into combinations you didn't plan.

![Random Creation, main screen](Documents/Design/Screen%20Shots/v3.0/Main_Dark_Result_3_0.png)

---

## How it works

You build a small hierarchy of your own content:

| Level | Example |
|-------|---------|
| **Collection** | Creatures |
| **Category Group** | Head |
| **Category** | Head Shape |
| **Option** | Round · Elongated · Flat · Skull-like · Beak-fronted |

Press **Generate** and the app picks one option from every enabled category, then
lays the result out as cards grouped by their group name.

Nothing in the app is specific to creatures. It has no built-in subject matter at
all — a collection is whatever you decide it is, so the same app works equally well
for monsters, spaceships, loot tables, NPCs, or anything else you can break into
parts.

---

## What it does

- **Collections, groups, categories and options** — organise content as deeply or as
  loosely as you like
- **Enable and disable anything** — at any level, so you can narrow a generation
  without deleting content
- **Weighted options** — make some results common and others rare
- **Presets** — save a whole enable/disable configuration and switch between setups
- **History** — every generation is kept, with a "drawn" marker for tracking what
  you've already used
- **Print preview** — get results onto paper
- **Dark, light and system themes**, plus three font sizes
- **Undo** for content edits, drag-and-drop reordering, and an internal
  cut/copy/paste clipboard

---

## Screenshots

| Managing content | Presets |
|---|---|
| ![Manage content](Documents/Design/Screen%20Shots/v3.0/Manage_Dark_3_0.png) | ![Presets](Documents/Design/Screen%20Shots/v3.0/Presets_3_0.png) |

| History | Light theme |
|---|---|
| ![History](Documents/Design/Screen%20Shots/v3.0/History_3_0.png) | ![Light theme](Documents/Design/Screen%20Shots/v3.0/Main_Light_Result_3_0.png) |

---

## Getting it

Download the latest version from the **[Releases](../../releases)** page.

- **Windows only**
- **No installation.** Unzip it anywhere and run `RandomCreation.exe`
- **Nothing to install first** — everything the app needs is bundled, including the
  .NET runtime

> **A note on the Windows warning.** The download isn't code-signed, so Windows may
> show a blue *"Windows protected your PC"* screen. Click **More info**, then **Run
> anyway**. This is what Windows shows for any application from a developer who
> hasn't bought a signing certificate; it isn't a detection of anything.

---

## Your content

Everything you create lives in a `data` folder next to the .exe:

| File | Holds |
|------|-------|
| `categories.json` | All your collections, groups, categories and options |
| `history.json` | Your generation history |
| `presets.json` | Your saved presets |
| `settings.json` | Theme, font size, window position |

Copy that folder to another machine and your entire setup goes with it. Back it up
by copying it somewhere safe.

A sample `categories.json` ships in the `samples` folder to show what the format
looks like. Copy it into `data` if you want a starting point — but note it will
replace anything already there.

---

## Status

Version 3.0. A personal creative tool in active use, not a commercial product.

---

## Licence

**This is not an open source project.** The source is published for reference only.
All rights reserved — see [LICENSE](LICENSE) for the full terms.

Bug reports and suggestions are welcome. Code contributions are not being accepted.

Copyright © 2026 Henry Robinson.
