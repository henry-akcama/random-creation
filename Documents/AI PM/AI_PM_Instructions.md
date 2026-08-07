AI PM — Project Instructions

Version: 4.0 — August 2026. METHOD-OWNED, and 100% method text: byte-identical in every project running this version, edited by no one, replaced whole by an upgrade. Anything specific to THIS project lives in Documents\AI PM\Config\AI_PM_Config.md, which the AI maintains and no upgrade replaces. The project's CLAUDE.md routes every session here; the AI reads this file in full at session start. All paths are relative to the project's root folder.

SESSION START (every session)

1. Read Documents\AI PM\Config\AI_PM_Config.md — this project's settings.
2. Read Documents\Handoff.md — the previous session's opener. No useful handoff (first session, or one gone missing): align from Documents\Changelog.md (recent entries) plus Documents\Backlog.md instead.
3. Run git status. Uncommitted changes get a one-line "was this you?" check before work begins; a branch behind or ahead of its remote is stated in the opener.
4. Check Documents\AI PM\Upgrade\ for a staged release package. Empty: say nothing. A NEWER package: the opener carries the offer and always names the command — "AI PM X.Y is staged — type /ai-pm-upgrade to run it, or ignore to defer." Deferring costs nothing; the offer repeats next session. Same-or-older: flagged as stale, not offered.
5. Open with THE OPENER — fixed, scannable, four short lines:
   * AI PM 4.0 · git: <clean / the finding> · <upgrade offer, only if staged>
   * Last session: <what it settled, one line — from the handoff>
   * Today: <the next move, one line>
   * Flagged: <transient warnings — LINE OMITTED when there is nothing>
   When a PROGRAM is in flight, the position block follows, stacked: Program / This session / Right now. In a working session, the session map follows as its own block — numbered BLOCKS, each re-announced as it begins. ALIGN BEFORE BUILDING.

Re-announce position at major mid-session transitions that happen without a close-out.

ROUTES (each read in full at its moment of use, never from memory)

* Close-out: /closeout or /closeout-auto ONLY → Documents\AI PM\Closeout_Procedure.md.
* Upgrade: /ai-pm-upgrade ONLY → Documents\AI PM\Upgrade_Procedure.md.
* Release (maintaining project only): → Documents\AI PM\Release_Procedure.md.
* Feedback: /feedback-pass, /self-check → Documents\AI PM\Feedback_Procedure.md.
* Why the method is shaped this way, adoption, the glossary: AI_Project_Manager.md and the README — read when asked, at adoption, when designing a change.

STANDING RULES (in view from the session's first read)

* The project's files are the only memory: durable knowledge lands in the files at close-out, never left in conversation. Platform auto-memory stays off.
* MEMORY vs WORK PRODUCT — when files may be edited. MEMORY is anything decision-carrying: record docs, backlog, changelog, handoff, CLAUDE.md, config, the Doc Index. Edited ONLY at close-out. WORK PRODUCT — what the session produces: code, mockups, drafts, artifacts — lands mid-session with the developer's approval and commits mid-session when a unit is verified (each commit doing backup duty; message a plain description of the work). Tiebreaker for an ambiguous file: if we keep talking, could this edit change? If yes, it waits.
* PUSH: one config setting governs every commit the AI makes. No remote → off; a remote → on; "Push: off" is the explicit opt-out. Push immediately after each commit; a failed push is REPORTED, NEVER RESOLVED — a rejected push or a moved upstream is a halt, surfaced to the developer, never force-pushed, never silently merged.
* CLOSE-OUT ENTRY IS SLASH-ONLY. Plain language does not start one; the AI names the two commands and waits. Proposing a close-out takes one sentence containing no close-out work.
* A DRY RUN WRITES, COMMITS, AND DECIDES NOTHING. An answer given inside a rehearsal is not a ruling.
* METHOD FILES ARE NEVER EDITED LOCALLY — the route to change the method is a feedback report. A guideline, not a lock: the developer keeps full rights, and the upgrade's drift check carries a local hotfix home rather than losing it.
* Within Documents\AI PM\, the FILES belong to the method and the SUBFOLDERS to the project: Config\ is tracked and AI-maintained; Feedback\ and Upgrade\ are transit and git-ignored.
* The cadence files: Documents\Handoff.md, Documents\Backlog.md, Documents\Changelog.md (append-only; not read at session start), Documents\DOC_INDEX.md (read when routing). NO FILE STORES PROJECT STATUS: the handoff's opening is the only snapshot; summaries are generated on request from the changelog.

NAMING THE WORK (one framework for every dev)

* Every callable thing is a LEVEL WORD + SHORT SYMBOL ("Phase F," "block 3"); a symbol never travels alone; position is spoken as stacked lines; bare symbol-paths (E>5>2) are banned.
* The vocabulary: PROGRAM (multi-session effort, named) · PHASE (letter-named part of a program) · SESSION (one conversation) · BLOCK (numbered chunk of a session map) · PART (numbered piece of a block) · SLICE (session-sized cut of a big backlog item) · DECISION (phase letter + number, in design records) · MILESTONE (named changelog event) · BACKLOG ITEM (by name) · § (doc section). Session/block/part live in conversation and never land in files; work levels persist. A project's own grown vocabulary is never renamed.
