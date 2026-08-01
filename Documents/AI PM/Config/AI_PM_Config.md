AI PM — Project Config

The template as shipped, carrying no version number of its own — this is the project's own state rather than a doc on the method's release schedule, and its history is in git (AI_Project_Manager.md §7). THIS PROJECT'S OWN FILE: the AI maintains it, it is edited at close-out like any other memory, it is tracked in git, and no method upgrade ever touches it. Everything here is specific to this project; the method text lives one folder up in AI_PM_Instructions.md and AI_Project_Manager.md, which are byte-identical in every project.

Read in full at session start, before the handoff.

* Project: Random Creation — WPF desktop app (C# / .NET 8)
* Project version at adoption: 3.0 (Major.Minor; Minor bumps every close-out, Major on a declared milestone). Matches the shipped app version at adoption (assembly 3.0.0.0); the project version and the app's assembly version move independently from here.
* Record docs: all in Documents\Design\ (settled at the file-organization pass, August 2026). RandomCreation_ProjectContext_v3_0.md — authoritative for v3.0 design: architecture, every screen's layout, undo/toast/clipboard/drag specs, colour palettes, bug-fix table, deferred list. RandomCreation_FileIndex_v3_0.md — authoritative for what each source file does and what changed in v3.0. RandomCreation_ProjectContext_3_0.md — shorter v3.0 summary, subordinate to the full record. RandomCreation_ProjectContext_v2.0.md and RandomCreation_ProjectContext_v1.0.md — frozen records of their own versions, historical only. No amendment chain yet: each doc is authoritative for its own version and none amends another. Screen Shots\ (v1.0/v2.0/v3.0) and Icons\ sit alongside them as design artifacts, not records.
* Feedback Pass: on
* External storage map: none — all project files have been consolidated into this one folder (July 2026); the file-server origin is history, not a live location.
* Allowed write locations: none

Notes on the fields:

* RECORD DOCS is the list a stranger reads to know what is authoritative for what. It grows as docs are born and shrinks as they retire.
* FEEDBACK PASS toggles the one auto-running behaviour in AI PM: a self-check after each close-out that may SUGGEST a feedback report. It observes and notifies; it never acts. Set it to off to stop the auto-notice — the /feedback-pass and /self-check commands still work by hand.
* EXTERNAL STORAGE MAP records where this project's off-workspace storage lives and how to route to it, once that outgrows a line in CLAUDE.md. Folders not listed are deliberately out of scope.
* ALLOWED WRITE LOCATIONS adds to what the close-out's OUTSIDE THE PROJECT check treats as in bounds (AI_Project_Manager.md §7). Already in bounds without being listed: the project's own folder or folders, the session's own scratch area, and the platform's own configuration folder — so the usual entry here is a build output folder, a shared drive, or a sibling tool's directory this project legitimately writes to. One path per line; "none" is the normal answer. Listing a location silences ordinary writes to it, and nothing else: a change to what the AI is PERMITTED TO DO in a future session — permission settings, automatic hooks, tool-server configuration, persistent cross-session memory — flags no matter what this field says.
