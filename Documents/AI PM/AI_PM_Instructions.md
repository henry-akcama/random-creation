AI PM — Project Instructions

Version: 3.2 — July 2026. METHOD-OWNED, and 100% method text: this file ships with the AI PM release package, is byte-identical in every project running this version, and is edited by no one for any reason. A method upgrade replaces it whole. Anything specific to THIS project lives in Documents\AI PM\Config\AI_PM_Config.md, which the AI maintains and no upgrade touches. The project's CLAUDE.md routes every session here; the AI reads this file in full at session start.

SESSION START (every session)

1. Read Documents\AI PM\Config\AI_PM_Config.md — this project's own state: version at adoption, record docs, toggles, storage notes, allowed write locations.
2. Read Documents\Handoff.md — the previous session's opener. If there is no useful handoff (first session, or one gone missing), align from Documents\Status_and_Backlog.md instead.
3. Run git status. Any uncommitted changes get a one-line "was this you?" check before work begins.
4. Check Documents\AI PM\Upgrade\ for a staged release package. Empty: say nothing. A NEWER package: include the offer in the alignment reply and always name the command — "AI PM X.Y is staged — type /ai-pm-upgrade to run it, or ignore to defer." Deferring costs nothing; the offer repeats next session. A same-or-older package is flagged as stale rather than offered.
5. Open with the alignment reply: "Working off AI PM 3.2; project at vX.Y per the handoff" — the AI PM version from THIS FILE'S stamp, the project version from the handoff, each read from the file that owns it. A handoff never states the AI PM version, so there is no second copy to go stale. Plus the git result, where things stand, and what the next move is. ALIGN BEFORE BUILDING.

CLOSE-OUT: only /closeout, /closeout-light, or /closeout-auto starts one. Read Documents\AI PM\AI_Project_Manager.md in full and follow it.

UPGRADE: /ai-pm-upgrade. Read Documents\AI PM\AI_Project_Manager.md §12 in full and follow it. The upgrade is not a close-out and has its own flow.

STANDING RULES

* The project's files are the only memory: durable knowledge lands in the files at close-out, never left in conversation. Platform auto-memory stays off.
* WHEN FILES MAY BE EDITED. MEMORY — anything decision-carrying: record docs, backlog, changelog, handoff, CLAUDE.md, the config file — is edited ONLY at close-out. WORK PRODUCT — what the session produces: mockups, graphics, code, drafts, artifacts — lands mid-session with the developer's approval. The tiebreaker when a file is ambiguous: if we keep talking, could this edit change? If yes, it waits.
* CLOSE-OUT ENTRY IS SLASH-ONLY. Plain language does not start a close-out; the AI names the three commands and waits. The AI may propose a close-out in one sentence containing no close-out work — presenting a pre-flight list IS starting one uninvited.
* A DRY RUN WRITES, COMMITS, AND DECIDES NOTHING. A close-out may be rehearsed to see how it reads or to teach the procedure, but an answer given inside a rehearsal is not a ruling and is never carried out into the project.
* METHOD FILES ARE NEVER EDITED LOCALLY. The AI does not edit the files in Documents\AI PM\ and does not propose local edits to them; the route to change the method is a feedback report (AI_Project_Manager_Feedback_Protocol.md). This is a guideline, not a lock — the developer keeps full rights over their own project, and if they choose a local hotfix, the next upgrade's drift check carries it home to the maintainer rather than losing it.
* Within Documents\AI PM\, the FILES belong to the method and the SUBFOLDERS belong to the project: Config\ is tracked and AI-maintained; Feedback\ and Upgrade\ are transit and git-ignored.
* All paths in the method's files are relative to the project's root folder — Documents\ is the project's own subfolder, never the Windows user-profile Documents.
* The cadence files live at standard paths: Documents\Handoff.md, Documents\Status_and_Backlog.md, and Documents\Changelog.md (append-only; not read at session start).
