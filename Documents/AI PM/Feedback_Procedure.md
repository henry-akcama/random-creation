AI PM — Feedback Procedure

Version: 4.0 — August 2026. METHOD-OWNED: byte-identical in every project, replaced whole by an upgrade. This file is what the AI EXECUTES for feedback work: the Feedback Pass (auto after each full close-out when enabled, or /feedback-pass), the close-out self-check (/self-check), report writing, and the maintaining project's intake. Read in full at those moments. Reasoning — why feedback exists, why the Pass is automatic — lives in AI_Project_Manager.md.

1. THE THREE CHANNELS. Every report is one kind:

* BUG / DEFECT — the procedure was followed but produced a wrong or incomplete result, or allowed one through. A near-miss is this channel at lower severity.
* CHANGE REQUEST — proposes exact spec text: a wording fix, a new rule, a new section.
* FRICTION / IMPROVEMENT — a step fought the session without failing outright; names the problem and a candidate direction, leaving the design to the maintainer. When in doubt, file friction.

2. THE ENVELOPE AND BODIES. Every report file opens with: Originating project · AI PM version in use · Session (date and/or commit) · Type. Bodies by type:

* BUG/DEFECT, one per item: Severity (DEFECT or NEAR-MISS) · What happened · Impact if uncaught · Suspected cause, naming the spec wording that invited it if any.
* CHANGE REQUEST: Target (doc + section) · Proposed text (exact words) · Evidence.
* FRICTION: What fought · Candidate fix (a direction, not finished text).

GROUPING: a session's bugs, near-misses, and friction batch into ONE file — a FIELD REPORT, envelope once, a body per item. Each CHANGE REQUEST gets its OWN file. FILENAMES, born unique on the outbound side: YYYY-MM-DD_Originating-Project_type_topic-slug.md (e.g. 2026-07-18_IFQ-Tracker_field-report_edit-timing.md); append the time (_1430) if that still collides. Named when written, never renamed on arrival.

3. THE FEEDBACK PASS. Runs AFTER a close-out completes, never inside it — it audits the finished close-out from outside. Auto when "Feedback Pass: on" in the config (the default); /feedback-pass runs it by hand, including mid-session for a realization worth carrying home. It observes and notifies; it never writes without the developer's word. No step banner — the close-out is over.

1. Run the self-check (§4) and gather any friction the session raised.
2. Nothing warranted: ONE LINE — "Feedback Pass — ✅ clean, nothing filed." — and stop. Don't manufacture reports.
3. Something warranted: a short notice — the tally plus one or two lines on what and why (e.g. "3 observations + 1 proposed change → one Field Report + one Change Request") — then three options: WRITE IT NOW (into the maintainer's inbox if reachable, else the local outbox) · TALK IT THROUGH FIRST · SKIP. Beneath them, one standing line: "To turn off auto-reporting, ask me to set Feedback Pass: off in AI_PM_Config.md."

4. THE CLOSE-OUT SELF-CHECK — the audit the Pass is built on; /self-check runs it alone, findings only. It confirms every close-out promise HAPPENED rather than was described (the common root it guards: a description of a deliverable substituting for the deliverable — a handoff "pointed at" but never rewritten, work "wrapped up" but never committed). The checks, mechanical where possible (git status, git log, what changed in the commit):

* Everything approved landed — the memory commit holds the approved changes and no others.
* Committed — the close-out ended in its memory commit and the tree is clean.
* Pushed — per the Push setting: pushed / off / failed-with-reason; any mid-session push failure still unresolved is named.
* Handoff ready — Documents\Handoff.md carries THIS session's opener and changed in the commit.
* Backlog and changelog updated — the backlog reflects the new state; the changelog gained this session's dated entry.
* Method files untouched — notify only; a deliberate hotfix is the developer's right and the upgrade's drift check carries it home.
* Nothing outside the project changed — grouped count when in-bounds writes happened, list on request; a change to what the AI is PERMITTED to do in future sessions always flags. Full definition in Closeout_Procedure.md.
* Method friction — did any part of the procedure fight the session?

A failed check becomes a proposed BUG report, pre-filled from what the self-check saw.

5. DETECTOR-ONLY IN ADOPTING PROJECTS. Friction noticed in an adopting project routes OUT via the Pass, full stop — the local method copy is never edited. In the maintaining project the loop may also close directly: friction folds into the method's source at a later session, with a report or without.

6. INTAKE (maintaining project). Its own session — "process the feedback inbox" is prompt enough; the AI reads the files directly, no pasting.

* One report (or one item) at a time, confirming before moving on.
* VERSION-CHECK FIRST, per report: a bug filed against an older version may already be fixed — noted and closed, never queued.
* The AI proposes a backlog item per surviving report; the developer approves, adjusts, or drops. Intake QUEUES work; it never folds a change straight into a doc.
* Processed reports move to the inbox's Archived\ subfolder the same session.

7. FOLDERS. Correspondence in transit is not project memory; processed correspondence is.

* Every project: Documents\AI PM\Feedback\Outbox\ — outbound reports awaiting transport; git-ignored (the .gitignore lines land at adoption). Remote adopters end the Pass with "send these to the AI PM maintainer"; the maintainer receives by any channel.
* Maintaining project only: Feedback Inbox\ at the workspace root — unprocessed reports stage there, git-ignored (inbound mail); Feedback Inbox\Archived\ is TRACKED — a processed report is evidence for a backlog item.
* The hard-rule rider from Closeout_Procedure.md applies to everything ignored here: git does not protect untracked files — deliberate copy or an explicit "genuinely disposable" ruling before deleting one.
