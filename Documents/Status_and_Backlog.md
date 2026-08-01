Random Creation — Status and Backlog

Current status + what's next. Rewritten at every close-out; completed work leaves for the changelog.

CURRENT STATUS (July 31 2026, at AI PM adoption)

Random Creation is a finished v3.0, sitting between releases. The app shipped: assembly stamped 3.0.0.0, a ~70 MB release zip built, eight development phases archived, and the big v3.0 push — the CategoryGroup layer, undo, internal clipboard, toasts, print, AI prompt export, and a pile of drag-and-drop coordinate fixes — is done and documented down to the colour hex. What is happening now is not feature work, it is tooling: assembling the scaffolding to develop the app differently — the AI PM system, six slash commands, a CLAUDE.md, and the exported Claude-project knowledge base. The honest gap: the workflow is still folder-copies-as-version-control with five overlapping source trees, and git only arrived with this adoption — exactly the thing worth closing before v4.0 work starts rather than after.

Live code is Source\RandomCreation\ ONLY. Every other source tree in the folder (Source Code\1.0|2.0|3.0, Releases\, docs export) is archive or reference.

WHAT'S NEXT (in intended order)

1. File organization pass — the folder holds duplicates and archives from the file-server consolidation and the Claude-project export. Extract docs\RandomCreation_ClaudeCode_setup.zip (a landing point, not a plan — developer's words), map the duplicates, agree what is archive vs. reference vs. live, tidy the root (stray categories.json), and settle .gitignore scope for build output and large binaries. Deletions only with explicit, clearly-flagged developer approval — standing preference.
2. CLAUDE.md rework — merge the project guide (currently docs\CLAUDE.md, 9.8 KB: architecture, live-code warning, build commands, conventions, data files) into the root CLAUDE.md around the AI PM block; port the style-of-working sections (working pace, information pacing, recorded-decisions-vs-better-ideas) from the developer's reference CLAUDE.md ("refrance CLAUDE.md" at root, from the IFQ Tracker project — everything else in it is IFQ-specific and stays behind). Remove the reference file after mining it (developer approval required).
3. GitHub — developer has never used GitHub, no account yet; teach from zero. Decision leaning yes (off-machine backup for a repo that lives on a network share). Account creation is developer-only; AI handles the git side. Repo size question ties into item 1's .gitignore decisions.
4. Bug fixes — the real work. Developer has bugs in the v3.0 app to report; none captured yet. First bug session should collect the list.

DEFERRED / PARKED

* Changelog milestone backfill (pre-adoption v1.0 → v2.0 → v3.0 history at milestone granularity) — deferred at adoption; the app's own data\changelog.txt and the release archives hold the raw material.
* v4.0 feature backlog — exists as an explicit deferred list from v3.0 (categories.json import, Back button redesign, single-collection export, history search, keyboard-shortcut customisation); nothing started. Recorded in the v3.0 context doc's Deferred section; carry into planning when v4.0 opens.
