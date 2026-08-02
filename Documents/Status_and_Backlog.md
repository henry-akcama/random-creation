Random Creation — Status and Backlog

Current status + what's next. Rewritten at every close-out; completed work leaves for the changelog.

CURRENT STATUS (August 1 2026, v3.3)

The app still has not changed — a finished v3.0, assembly 3.0.0.0 — but everything around it is
now ready for it to. The repository is LIVE on GitHub, pushed and in sync, with all eleven
commits carrying the noreply address. Visual Studio runs the app (F5 was failing only because
the build configuration was left on Release). And v4.0 is planned end to end.

THE NEXT SESSION BUILDS. Planning is finished, not partially finished — twelve items, each with
its fix approach already decided, plus the installer design pass the standing ruling required.
That work is NOT listed here; it lives in Documents\Design\RandomCreation_ReleasePlan_v4_0.md,
which is authoritative for what v4.0 contains, in what order, and why. Read it before building.

Live code is Source\RandomCreation\ ONLY.

WHAT'S NEXT (in intended order)

1. BUILD v4.0. Per the release plan's build order: themes and window fix first, then the data
   package (migration removal, %LocalAppData% move, installer and uninstaller, folder button,
   conditional sample content, changelog relocation), then the sample content, then the serial
   number, then the print package. Verify by running the app after EACH item, not at the end —
   the developer does not read C#, so a batch has too many suspects.

2. GITHUB ACTIONS, then the release. A tag drives the version; the workflow builds the installer
   and the portable zip and publishes a GitHub Release. Scheme in the lifecycle doc section 4.

3. SCREENSHOTS for v4.0, reshot after the theme fix into Documents\Design\Screen Shots\v4.0\,
   with the README's links updated. Not urgent — the developer is explicit that the current
   shots serve the current audience.

DEFERRED / PARKED

* GitHub Pages — a branded product page. Deliberately AFTER v4.0: GitHub Releases is already a
  download page with a permanent latest-release address, so nothing about distribution waits on
  this. Build it when there is a real download to point at and correct screenshots to use.
* Updater — deferred from v4.0 by ruling, since there is one reachable user and nothing to
  update from. Three options recorded in the lifecycle doc section 6; the no-dependency one
  (query the GitHub Releases API at startup) remains the recommended fit.
* Releases\ slimming — 781 MB, git-ignored, no safety net. Plan recorded in the lifecycle doc
  section 4: upload v3.0 to GitHub as a historical release (it is the only exact copy of what
  the live user runs, since git history starts after v3.0 shipped), then delete locally. NOT
  before a release has been published and downloaded back.
* Changelog milestone backfill — DONE at this close-out. Retained here only as a pointer:
  pre-adoption v1.0/v2.0/v3.0 history now sits in Documents\Changelog.md at milestone
  granularity, at the precision the sources support.
* Future refactor and feature candidates — the v3.0-era list (ManageContentScreen refactor,
  ObservableCollection, Redo, weight tier customisation, categories.json import, single-collection
  export, full group detail interactivity). Recorded in RandomCreation_EngineeringNotes.md.
  RENAMED from "v4.0 feature backlog": that label was a placeholder meaning "some future major
  version", and none of it is in the actual v4.0.
* RandomCreation_ProjectContext_3_0.md retirement — RESOLVED at this close-out. It was NOT
  redundant: a comparison found none of its content in the full v3.0 record. Renamed to
  RandomCreation_EngineeringNotes.md, two obsolete sections cut.
* Installable build — RESOLVED. The standing ruling that it gets a design pass before any
  building was honoured at this close-out; the decisions are in the lifecycle doc section 6 and
  the work is inside v4.0.
