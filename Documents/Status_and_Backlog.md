Random Creation — Status and Backlog

Current status + what's next. Rewritten at every close-out; completed work leaves for the changelog.

CURRENT STATUS (August 3 2026, v3.5)

REPOSITORY MOVED: GitHub home is now the akcama organization —
https://github.com/akcama/random-creation — remote updated, references swept (commit 36b489d),
old address redirects.

v4.0 SHIPPED. All twelve planned items built and verified in the running app, the installer and
portable zip both published by the tag-driven GitHub Actions pipeline, and the developer
personally tested the installer, the portable build, the uninstaller's keep-content default, and
a real multi-page proof print before the tag was pushed. Release:
https://github.com/akcama/random-creation/releases/tag/v4.0. Assembly 4.0.0.0. The two
print open questions (60% dimmed opacity, darkened header grey) were settled by looking at
paper, as planned. One session took the app from untouched v3.0 to released v4.0.

Live code is Source\RandomCreation\ ONLY.

WHAT'S NEXT (in intended order)

1. DOWNLOAD-BACK VERIFICATION. Download the published v4.0 assets from GitHub and confirm they
   run — the lifecycle doc's precondition for retiring the local Releases\ folder. The
   developer tested the DRY-RUN artifacts thoroughly; the published files are a fresh build of
   the same commit and still deserve the direct check.

2. THE ONE LIVE USER'S UPGRADE — manual, and it MUST carry her data. Old app: Settings → Open
   data folder → copy everything. Install v4.0 → Open data folder → paste. Without this her
   collections will appear to vanish (they'd still be in the old portable folder). She has
   built no generations yet, which is what made the data relocation cheap — but she may have
   collections.

3. v4.0 RECORD DOC, then retire the release plan. Write the v4.0 design record (what shipped,
   how each item was implemented, deviations: none) absorbing
   RandomCreation_ReleasePlan_v4_0.md, which retires on absorption per its own header. A good
   candidate for a dedicated session; FileIndex is also a version behind (v3.0).

4. RELEASES\ RETIREMENT (781 MB, git-ignored, no safety net). After item 1: upload the v3.0 zip
   to GitHub as a historical release (the only exact copy of what the live user runs — git
   history starts after v3.0 shipped), decide v1.0/v2.0 zips (their source is already safe in
   Documents\Archive\), then delete the local folder on an explicit ruling.

DEFERRED / PARKED

* GitHub Pages — a branded product page. The precondition it was waiting on ("a real download
  to point at and correct screenshots") is now met; still optional polish, build when wanted.
* Updater — deferred from v4.0 by ruling. Three options in the lifecycle doc section 6; the
  no-dependency GitHub-Releases-API check remains the recommended fit.
* Workflow action versions — the release pipeline's helper actions (checkout@v4,
  setup-dotnet@v4, upload-artifact@v4) run on Node.js 20, which GitHub is deprecating on its
  runners. Cosmetic warning today; bump the @vN numbers whenever convenient.
* Remaining v4.0 screenshots — only the five README-facing shots were reshot. The deeper v3.0
  design-record set (settings, dialogs, print preview, presets) still documents v3.0; reshoot
  if a product page or the v4.0 record doc wants them.
* Future refactor and feature candidates — the v3.0-era list (ManageContentScreen refactor,
  ObservableCollection, Redo, weight tier customisation, categories.json import,
  single-collection export, full group detail interactivity). Recorded in
  RandomCreation_EngineeringNotes.md.
