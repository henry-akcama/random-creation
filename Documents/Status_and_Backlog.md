Random Creation — Status and Backlog

Current status + what's next. Rewritten at every close-out; completed work leaves for the changelog.

CURRENT STATUS (August 1 2026, v3.2)

The app still has not changed: a finished v3.0, assembly 3.0.0.0, sitting between releases. What
changed is the scheme around it. The developer asked to be TAUGHT how a project like this should
be laid out and shipped rather than handed a scheme, and that teaching session is now a record
doc — RandomCreation_DevelopmentLifecycle.md, authoritative for how this project is worked and
released.

Three things settled. The docs\ folder is gone: its v1.0 and v2.0 source snapshots were rescued
into Documents\Archive\ after it emerged they are the ONLY surviving copies of pre-git source
(git history starts at v3.0; the v2.0 release zip contains no source), and everything else in it
was proven byte-identical to Source\ before deletion. categories.json is homed at
Source\...\SampleData\ and ships to samples\ beside the exe — deliberately never to data\, since
a release landing a file there would destroy the content of anyone updating over their existing
folder. And the project is on GitHub: public, all-rights-reserved, with LICENSE and README
written.

Live code is Source\RandomCreation\ ONLY.

WHAT'S NEXT (in intended order)

1. GitHub — finish what this session started. The repository exists at
   https://github.com/henry-akcama/random-creation, connected as origin, verified reachable and
   empty. Three steps remain, in order: (a) rewrite the 10 pre-adoption commits to the noreply
   address — safe ONLY while nothing has been pushed, so this happens first and git filter-branch
   is the tool, the tree needing to be clean for it to run; (b) the first push, which opens a
   browser once for Credential Manager to authenticate; (c) then the parts that need a populated
   repo — GitHub Actions for automated release builds, and GitHub Pages for the product page.
   Developer ruled this happens immediately after the v3.2 close-out.

2. Visual Studio setup — the other half of the structure session, not reached. The developer has
   been experimenting and wants help configuring it properly. A stray .vs folder at the project
   root dates from that experimentation; git-ignored and harmless. Deferred to after GitHub
   because VS's git integration configures more cleanly once a remote exists.

3. Bug fixes — the real work, and the reason for the tooling. A bug session should collect the
   full list before fixing anything; the developer has more to report and none are captured yet.
   * BUG 1 (found 2026-08-01, unfixed by developer ruling): shipped v3.0 uses v2.0 theme files.
     Source\...\Themes\DarkTheme.xaml and LightTheme.xaml date from May 31 2026; the real v3.0
     themes sit unreferenced one folder up at the project root, dated June 6 16:27 — the release
     exe was built at 20:40 the same evening. Hashes confirmed at the v3.2 close-out: the root
     copies are byte-identical to docs\v3.0\'s themes, so the fix material is safe and tracked.
     App.xaml, RandomCreation.csproj and ThemeService.cs all point at Themes\. Fix is to move the
     root copies into Themes\ and rebuild; verify VISUALLY rather than by compile, since
     compiling is exactly what missed it.

4. Installable build (learning goal, developer-requested). Move from portable to installed.
   Requires: user data relocated out of the program folder to %AppData%\RandomCreation\, since
   Program Files is not user-writable and DataService currently resolves data\ from
   AppDomain.CurrentDomain.BaseDirectory; the installer must not overwrite existing user data on
   upgrade; the uninstaller offers to remove user data or leave it; a migration path for existing
   portable users. Changes the updater story — an installed app can update itself in ways a
   portable zip cannot. Code signing acknowledged and deferred (~$200-400/year, and SmartScreen
   reputation builds slowly regardless). STANDING RULING: this gets a detailed design pass
   worked through with the developer BEFORE any building starts — not a build-first item.

DEFERRED / PARKED

* Updater — three options recorded in the lifecycle doc. The no-dependency one (query the GitHub
  Releases API at startup, offer a download link, ~40 lines with built-in .NET types) is the
  recommended fit and respects the no-NuGet conviction. Waits on item 4's shape.
* Sample categories.json is the smaller 10,685-byte file; the developer's own working copy in
  bin\ is 17,296 bytes and a richer example. Swapping is a five-second job whenever wanted.
  Developer chose the smaller one for now.
* README screenshots currently point into Documents\Design\Screen Shots\v3.0\ — internal design
  records serving a public landing page. Works, avoids duplication, and should be revisited when
  the Pages site is built, not before.
* RandomCreation_ProjectContext_3_0.md is a shorter v3.0 summary subordinate to the full
  RandomCreation_ProjectContext_v3_0.md. Possible retirement candidate; nobody has read it in
  anger yet, so it stays until someone confirms it carries nothing unique.
* Changelog milestone backfill (pre-adoption v1.0 → v2.0 → v3.0 history) — deferred at adoption;
  the app's own changelog.txt and the release archives hold the raw material.
* v4.0 feature backlog — an explicit deferred list from v3.0 (categories.json import, Back button
  redesign, single-collection export, history search, keyboard-shortcut customisation); nothing
  started. Recorded in the v3.0 context doc's Deferred section; carry into planning when v4.0
  opens.
* Releases\ slimming — the folder is 781 MB and git-ignored, so nothing in it has a safety net.
  The remaining v1.0 source tree could go once GitHub Releases is proven, but that waits until a
  release has actually been published and downloaded back.
