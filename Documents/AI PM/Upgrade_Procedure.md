AI PM — Upgrade Procedure

Version: 4.0 — August 2026. METHOD-OWNED: byte-identical in every project, replaced whole by an upgrade. This file is what the AI EXECUTES when /ai-pm-upgrade runs — read it in full at that moment, then follow it. Reasoning lives in AI_Project_Manager.md. THE UPGRADE IS NEVER A CLOSE-OUT AND NEEDS NONE: it writes its own memory (step 8). It runs in ONE SITTING — the plan lives in conversation, never on disk; the escape hatch is aborting before step 6, when nothing has moved, and re-running the survey fresh next time.

1. TRIGGER. /ai-pm-upgrade is the only door, and it works whenever a package is staged in Documents\AI PM\Upgrade\ — the session-start notice is a courtesy, never a gate. SELF-INSTALL: in the maintaining project nothing stages; the command reads Releases\vX.Y\ directly. Everything else below is identical.

2. READ THE PACKAGE. The package's Manifest first: its "upgrades from" must match the installed version — mismatch stops here, flagged for the maintainer (no version-skipping). Then the package's Upgrade Notes (at the package root, beside the Adoption Guide).

3. DRIFT CHECK, before anything is overwritten. Diff the installed method files against the last upgrade commit — or the adoption commit, when no upgrade has ever run. Any local edit is surfaced and captured as a feedback report into the project's outbox FIRST, so a deliberate hotfix travels home to the maintainer instead of dying in the overwrite.

4. SURVEY. Read the project's lived-in files against the Notes' checklist — each item a DETECT → PROPOSE pair. Only firing items enter this project's worklist. Survey findings that suggest the method lacks a drawer are flagged as feedback material: the survey doubles as the method's field instrument.

5. THE PLAN, approved before anything moves. Two tiers, shown together: the MECHANICAL tier (Manifest arithmetic, config edits) listed compactly, visible but not itemized for debate; the PROJECT-FILE tier (every detect that fired) itemized — what was found, where, what is proposed — and this tier is the conversation: the developer strikes, reshapes, adds. The Notes' WARNINGS section is read aloud here. ONE approval covers the shaped plan; no per-item sign-offs. A light upgrade's whole plan is a few mechanical lines and one yes — the project's own state sets the ceremony.

6. EXECUTE.
   * Method files by MANIFEST ARITHMETIC: old Manifest vs new Manifest derives add / replace / remove mechanically — including deleting files the old version installed and the new one drops. The FILES in Documents\AI PM\ are the method's; the SUBFOLDERS (Config\, Feedback\, Upgrade\) are the project's and survive untouched.
   * CONFIG edited IN PLACE, never replaced: add settings the version introduces (arriving in the no-behaviour-change state), rename fields (values carried), remove settings no longer defined. Never change a value the developer chose; never turn a behaviour on unasked — a default the release changes is surfaced at plan time, so approval covers it.
   * The approved project-file work. Deviation logic as at close-out: trivial mismatches (a detect's fix already true) noted and skipped; anything needing JUDGMENT halts, returns for plan amendment and re-approval, resumes. An upgrade never improvises on project files.

7. VERIFY. Post-install listing against the new Manifest, dot-entries shown (.claude\ included).

8. REPORT AND MEMORY. The summary names every config line and every file touched, and records struck plan items. A dated changelog entry. Then the UPGRADE COMMIT — message "AI PM upgrade X.Y → X.Z", scoped by AUTHORSHIP: the method files plus whatever this procedure itself wrote, nothing from the session's own work — pushed per the Push setting. The handoff is preserved: one annotation line ("upgraded AI PM X.Y → X.Z on DATE; opener below unchanged"), the opener untouched.

9. CLEANUP. The staged package leaves Upgrade\ (the Upgrade Notes with it), verified, dot-entries shown, stated in the summary so the next session does not re-offer it. No-op in the maintaining project — Releases\ stays.
