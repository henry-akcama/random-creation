Random Creation — Release Plan v4.0

Version: 1.0 — August 2026. Authoritative for WHAT v4.0 CONTAINS, IN WHAT ORDER, AND WHY.
Not authoritative for the app's existing design (RandomCreation_ProjectContext_v3_0.md), for
code-level engineering knowledge (RandomCreation_EngineeringNotes.md), or for how the project
is worked and shipped (RandomCreation_DevelopmentLifecycle.md) — the packaging decisions this
release acts on live in that doc's sections 6 and 7, not here. Written at the v3.3 close-out
from a full planning session: every item below was walked through with the developer, and the
fix approach is recorded alongside the problem so the build sessions have no thinking to redo.

Retires when v4.0 ships and its content has been absorbed into the version's own record.

--------------------------------------------------------------------------------
WHY 4.0 AND NOT 3.1
--------------------------------------------------------------------------------

The developer declared this a major release. Two changes earn it: user data moves out of the
program folder, and the app becomes installable. Together they break the mental model that has
been true since v1.0 — "I upgrade by replacing a folder." A minor number would understate that.

NOTE THE COLLISION, because two numbers now sit side by side meaning different things: the APP
is heading to 4.0; the PROJECT version (AI PM's, which bumps at every close-out) is at 3.3 and
runs on its own line. AI_PM_Config.md has always said they move independently. This is the
first release where the gap is wide enough to misread.

A SECOND COLLISION, already corrected: the backlog used to carry a parked item called "v4.0
feature backlog" — a v3.0-era placeholder meaning "some future major version." None of those
items are in this release. It has been renamed so nobody reads it as a list of things 4.0
promised and failed to deliver.

--------------------------------------------------------------------------------
1. THE TWELVE ITEMS
--------------------------------------------------------------------------------

BUG 1 — SHIPPED v3.0 RUNS v2.0 THEME DICTIONARIES
Source\...\Themes\DarkTheme.xaml and LightTheme.xaml date from May 31 and are byte-identical to
the v2.0 themes. The real v3.0 themes sit unreferenced one folder up at the project root, dated
June 6 16:27 — the release exe was built at 20:40 the same evening. App.xaml, the .csproj and
ThemeService.cs all point at Themes\, so the root copies are not compiled in at all.
FIX: move the root copies into Themes\ and rebuild.
VERIFY VISUALLY, NOT BY COMPILING. Compiling is exactly what missed this — it compiles cleanly,
which is why it shipped. A before-and-after screenshot pair of the same screen is the proof.

BUG 2 — A FRESH INSTALL ANNOUNCES A MIGRATION THAT NEVER HAPPENED
DataService.Initialise() calls EnsureDataDir() first, which CREATES the data folder. It then
reads SchemaVersion, gets 0 because settings.json does not exist, and tries to tell a v1.0 user
from a v2.0 user with `File.Exists(LegacyPath) && !Directory.Exists(DataDir)` — a test that can
never pass, because the folder was created four lines earlier. So every new user falls into the
v2→v3 branch and is told their collections were converted to the new format. They had none.
The same line makes the v1→v3 path DEAD CODE: a Creature Crafter user with a
creature_crafter_data.json would get the v2 message and no handling of their file.
FIX — REMOVAL, NOT REPAIR. Nobody holds pre-v3.0 data (see USERS below), so the v1 and v2
migration paths, MigrationDialog, the pre-migration history backup and the V2* model classes
all come out. Three cases remain:
  * no data files at all      → fresh start, NO DIALOG
  * SchemaVersion == 3        → load normally
  * anything else             → back up every file with .bak, start fresh, say so
THE THIRD CASE IS NOT OPTIONAL. Deleting migration without a catch-all is how silent data loss
happens: a v2.0 categories.json would deserialise into the v3 model as an empty structure, look
like no content, and be overwritten on the next save. Roughly ten lines stand between
simplification and a landmine.
bin\Debug\...\data old\ is a genuine v2.0-era data set and is the test material for this path.
Keep it until the fix is verified.

BUG 3 — A MAXIMISED WINDOW RUNS UNDER THE TASKBAR
MainWindow is borderless (WindowStyle="None", AllowsTransparency="True", WindowChrome), which
loses the sizing behaviour Windows supplies to ordinary windows. Nothing in the code references
the work area; MaximizeButton_Click just sets WindowState and leaves sizing to Windows, which
uses the full monitor rather than the monitor minus docked bars. Win+Up and drag-to-top snap
through the same path and are affected identically.
FIX: intercept WM_GETMINMAXINFO, ask which monitor the window is on, and report that monitor's
work area. Taskbar on any edge, any thickness, any monitor, and changes made while running are
then all handled by one mechanism, because the work area is already "monitor minus reserved
space, wherever it is."
DO NOT use the shorter SystemParameters.WorkArea variant: it reads the PRIMARY monitor and
breaks on a multi-monitor setup, which the developer runs.
AUTO-HIDE TASKBAR IS DELIBERATELY OUT OF SCOPE — a recorded decision, not an oversight. With
auto-hide on, a window sized to the exact full screen can stop the taskbar sliding back out;
the standard remedy is a 1px gap on the hidden edge. The developer does not use auto-hide and
chose not to carry code for a case they do not run. Revisit only if it is reported.

BUG 4 — PRINTING SILENTLY DISCARDS EVERYTHING PAST ONE PAGE
PrintButton_Click builds a FixedDocument containing exactly one FixedPage, with content in a
ScrollViewer whose scrollbars are disabled and whose height is fixed to a single sheet. Overflow
is not shrunk, not carried, and not flagged — it is clipped and gone.
THE PREVIEW IS WHY THIS WAS INVISIBLE. The preview's paper area scrolls, so it faithfully shows
everything. The preview has been reassuring the developer about output it never checked.
FIX: real pagination — measure each card, fill a page until the next will not fit, start a new
one. Keeps the existing card design.
NOT the FlowDocument route: a card grid does not map onto flow layout, and a card taller than a
page would clip again.

CHANGE 1 — SERIAL NUMBER PER GENERATION
Purpose: connect a sheet of paper to the generation that produced it. HistoryEntry currently
carries no identifier at all, only a timestamp.
DESIGN:
  * the counter lives in SettingsData, NOT derived from history — so it climbs forever
    regardless of the 500-entry history limit, the unlimited setting, or a history clear
  * assigned once at generate time, stored on the entry, never recomputed
  * formatted with thousands separators: #10,000
  * visible EVERYWHERE a result is: main screen, History list, ResultDetailDialog, print
    preview, printout. An identifier only on paper cannot be matched back to anything
NEVER RESETS, and no reset is built. Paper outlives the app's history. If clearing history
restarted the count, a printout marked #47 would start pointing at a different generation — the
identifier would become an active source of confusion rather than a missing feature. If a reset
is ever wanted it is a deliberate button that says what it does.
WHY A COUNTER AND NOT SOMETHING UNIQUE-BY-CONSTRUCTION. A timestamp-derived code needs no stored
state and cannot collide, but it is not a COUNT — and the developer explicitly wants the count
("if you generate 10,000 then that is your count"). The counter's one theoretical weakness,
collision after data loss, is already covered: the timestamp is on the page beside it.
NO BACKFILL. Existing history entries keep no serial. Adding fields is backward compatible —
old files load, absent fields take defaults — so there is no migration and no schema bump.

CHANGE 2 — PRINT HEADER AND PAGE FOOTER
Three parts, one edit, because they live in the same six lines.
  * COMPACT the header: title and serial on one line, timestamp and summary sharing a second.
    Three lines become two while carrying more information
  * LEGIBILITY: the timestamp and summary lines use RGB 170,170,170 — a light grey that paper
    renders lighter still. Darken substantially. The title and group headers use 51,51,51
  * FOOTER on EVERY page, including the first: "Random Creation · #1,234 · Page 2 of 3". A
    footer on page one is not redundant — it is what says "of 3", which is how a missing sheet
    is noticed. Small, and readable rather than the grey being fixed above
ONE SHARED BUILDER. BuildPreview() and BuildPreviewInto() are near-duplicate copies of the
header (20pt versus 16×1.3 = 20.8pt — drift, not design). The cards are ALREADY shared. Unifying
the header behind a scale parameter is not extra scope: it is what makes this item verifiable,
because otherwise the preview can show a corrected header while the printer produces the old one
and the developer's check would pass.
THE PREVIEW BECOMES PAGE-ACCURATE: real page breaks, footers, page numbers, scaled to fit the
window. What is seen is what prints. Agreed deliberately — the preview has already misled once.

CHANGE 3 — DIMMED ITEMS PRINT AGAIN
Grouped cards exclude dimmed rows entirely (`pairs.Where(p => !p.IsDimmed)`); flat cards include
them at 40% opacity. Two answers to one question in one file. The developer ruled this was DRIFT
and dimmed items were always meant to print.
FIX: grouped cards adopt the flat behaviour.
OPACITY IS AN OPEN QUESTION, deliberately. 40% on paper is about as faint as the grey being
fixed in CHANGE 2. Start at 60% and let the developer judge a real proof print rather than guess.

CHANGE 4 — "OPEN DATA FOLDER" BUTTON IN SETTINGS
Opens the app's CURRENT data folder, whatever it is: data\ beside the exe when portable,
%LocalAppData%\RandomCreation\ when installed. Same button, no change needed when the location
moves — it asks the app where its data lives.
Serves the one manual upgrade directly: open the old app, click, there is the JSON; open the new
app, click, there is where it goes. Drag between two Explorer windows.
Useful beyond that — backups, hand-editing categories.json (user-editable by design), and copying
a setup to another machine, which the lifecycle doc already describes as the portable build's
party trick and which currently requires knowing where to look.
DataService.DataDir is private and needs a public accessor.

CHANGE 5 — SAMPLE CONTENT INSTALLED ONLY WHEN THERE IS NONE
On startup, if the data folder has no categories.json, copy the sample from the samples\ folder
beside the exe. If one exists, do nothing.
THE APP DOES THIS, NOT THE INSTALLER. An installer-side check would protect only the installed
build; the portable zip has no installer to run it. One mechanism, both builds, testable without
building an installer, and it lands in the startup code BUG 2 is already rewriting.
Falls out correctly for every uninstall case: keep content → reinstall leaves it alone; remove
content → reinstall gets the sample; user deletes their own content → same, no reinstall needed.
THIS REVISES THE v3.2 RULING that the sample ships to samples\ and users copy it themselves. That
ruling exists because a release landing a file at data\categories.json would destroy the content
of anyone updating over their folder. A CONDITIONAL copy does not reintroduce that danger — the
rule was always "never overwrite user content," and "only write when there is none" honours it
completely. Recorded as a deliberate revision, not a forgotten ruling.

CHANGE 6 — PURPOSE-BUILT SAMPLE CONTENT
The current sample was never designed as one; it is whatever was lying around. CHANGE 5 makes it
the first thing every new user sees, which raises the stakes from "a file people might copy" to
"the product's first impression."
DESIGN:
  * THREE collections of visibly different kinds — creature, starship, swords. A sample of only
    creatures teaches that this is a monster generator. CLAUDE.md is emphatic that the app is
    general purpose and must never assume a subject; the sample is where that is proven or lost
  * only CREATURE enabled, so the collection enable/disable model is visible immediately
  * one GROUP disabled and a couple of OPTIONS disabled, so those levels are discoverable too
  * WEIGHTS varied, so the tier system is visible rather than theoretical
  * small — enough groups and options to show how it works, no more
The old sample files retire once this exists.

CHANGE 7 — changelog.txt MOVES OUT OF THE DATA FOLDER
Currently the .csproj copies changelog.txt to data\changelog.txt and DataService reads it from
there for the About section. That works only while data sits beside the exe. Once user data moves
to %LocalAppData%, the installer would place changelog.txt in the program folder's data\ subfolder
while the app looks in %LocalAppData%, find nothing, and fall back to a built-in placeholder —
About would quietly stop showing real release notes.
The underlying correction is a SPLIT the app has never had to make explicit:
  * PROGRAM FILES — beside the exe, read-only, replaced every release: the app, changelog.txt,
    samples\categories.json
  * USER DATA — %LocalAppData%\RandomCreation\ (or data\ when portable), writable, never touched
    by an installer: categories, history, presets, settings
changelog.txt is authored and shipped, so it belongs in the first group. It is only in the data
folder because that is where everything lived when there was one folder.
This also confirms CHANGE 5's direction of travel: read the sample from PROGRAM, write it to DATA.

CHANGE 8 — SCREENSHOTS RESHOT FOR v4.0
The existing v3.0 screenshots were taken from a build running the v2.0 themes, so they may be
documenting BUG 1 rather than the app. They feed the README today and any product page later.
Reshoot after BUG 1 is fixed, into a new Documents\Design\Screen Shots\v4.0\ folder, and update
the README's links.
NOT URGENT — the developer is explicit that the current shots serve the current audience, which
is one person besides themselves.

--------------------------------------------------------------------------------
2. THE PACKAGING WORK THAT RIDES ALONGSIDE
--------------------------------------------------------------------------------

Installer, uninstaller, the %LocalAppData% move and the GitHub Actions build are part of this
release but are NOT items on the list above — they are the subject of
RandomCreation_DevelopmentLifecycle.md sections 3, 4 and 6, which carry the decisions and the
reasoning. Summarised here only so the plan reads whole:

  * per-user install (no admin, no UAC — which matters more than usual for an unsigned app)
  * Inno Setup, free, script kept in the repository
  * %LocalAppData%\RandomCreation\ for user data
  * installer AND portable zip both ship, both self-contained, distinguished by a marker file
  * uninstaller asks whether to remove user content, defaulting to KEEP
  * a tag drives the version; Actions builds both artifacts and publishes a GitHub Release
  * NO UPDATER in 4.0 — one reachable user, updated by hand
  * NO GitHub Pages in 4.0 — Releases is already a download page; Pages is polish

--------------------------------------------------------------------------------
3. BUILD ORDER
--------------------------------------------------------------------------------

1. BUG 1, BUG 3 — independent, quick, immediately visible. Something to look at early.
2. THE DATA PACKAGE — BUG 2 + the %LocalAppData% move + installer/uninstaller + CHANGE 4 +
   CHANGE 5 + CHANGE 7. These all answer one question: where does data live and how is it found.
   Doing them separately means opening DataService four times and reasoning about the same code
   four ways.
3. CHANGE 6 — the sample content, once CHANGE 5 gives it a job.
4. CHANGE 1 — the serial. Must exist before the print layout can show it.
5. THE PRINT PACKAGE — BUG 4 + CHANGE 2 + CHANGE 3. One region of one file.
6. CHANGE 8 — screenshots, after the themes are right.
7. GitHub Actions, tag, release.

VERIFY AFTER EACH ITEM, not at the end. The developer does not read C#, so running the app is
their only real review mechanism, and a release this size has too many suspects if problems are
discovered in a batch. This is also why the plan was built in full before any building: the
developer chose "plan it all, then build it all, in one stretch" over either extreme.

--------------------------------------------------------------------------------
4. USERS — WHY SO MUCH IS SAFE TO DELETE
--------------------------------------------------------------------------------

ONE user besides the developer. She is on the current version, has built no generations yet, and
is reachable directly. That single fact licenses three decisions that would otherwise be reckless:
migration code can be deleted outright, the data location can move, and no updater is needed.

IT ALSO MAKES NOW THE CHEAPEST MOMENT THIS CHANGE WILL EVER HAVE. Every additional user makes
relocating data more expensive. The developer weighed the counter-argument — that a known-good
baseline makes installer problems easier to diagnose — and accepted the risk deliberately.

HER UPGRADE IS MANUAL AND MUST INCLUDE HER DATA. The installed app looks in %LocalAppData%, finds
nothing, and starts fresh silently. From her side every collection she has built would appear to
have vanished; they would still be in the old portable folder, but she would not know that.
CHANGE 4's button makes this a drag between two Explorer windows.

--------------------------------------------------------------------------------
5. OPEN, ON PURPOSE
--------------------------------------------------------------------------------

* DIMMED-ITEM OPACITY — start at 60%, settled by looking at a real proof print.
* SAMPLE CONTENT — CHANGE 6's spec is agreed; the actual content is written and then judged.
* RandomCreation_EngineeringNotes.md carries a MIGRATION PATHS section that is accurate today and
  goes stale the moment BUG 2 lands. Update it in the same session, not later.
