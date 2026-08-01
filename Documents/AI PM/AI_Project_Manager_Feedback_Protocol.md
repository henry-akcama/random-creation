AI PM Feedback Protocol

Ships with AI PM 3.2 — July 2026. Shipped in the AI PM release package beside AI_Project_Manager.md; carried as a project file in every project that runs the method. This file carries NO version number of its own: it travels inside the release package and never ships apart from it, so the AI PM version IS its version (AI_Project_Manager.md §7, one number per thing that actually moves). Superseded versions live in the maintaining project's git history.

New in 3.2: the close-out self-check gains its eighth line, OUTSIDE THE PROJECT (§4); and this file stops carrying a separate version number, which only ever needed syncing.

This is the AI PM Feedback Protocol — the channel that carries observations from a project that USES the method back to the project that MAINTAINS it. Unlike the Adoption Guide (used once, read from the release package at setup), this is a permanent resident: it ships to every project as a project file, the way AI_Project_Manager.md does, because a project produces feedback across its whole life, not just at setup.

Two halves, one file:

* OUTBOUND (§§1–7) — how a project produces a report: the three channels, the templates, the Feedback Pass that runs after close-out, the close-out self-check it's built on, the commands, and the toggle.
* INBOUND (§§8–9) — how the maintaining project receives reports: the intake walk and the feedback folder scheme.

The maintaining project runs both halves — its own observations become reports that come home to itself (a zero-distance trip), then get processed through intake like any other.

***

WHY THIS EXISTS

AI PM is distributed software: one procedure, maintained in one place, carried as a copy in every project that runs it. Improvements are meant to flow back to the maintaining project (AI PM §11, the self-honing loop). But an adopting project can't just edit its own copy — that would fork the method into as many divergent versions as there are projects. The Feedback Protocol keeps one source of truth by splitting observation from authority: an adopting project OBSERVES (a defect, a friction, a needed change) and REPORTS; the maintaining project DECIDES and, if warranted, edits the method's source.

The failure this prevents: a good lesson learned in project A dies there for lack of a route home — or worse, gets patched into project A's local copy of AI PM, and now project A silently runs a different method than everyone else.

***

1. THE THREE CHANNELS

Every report is one of three kinds. The kind sets the body template (§2) and how it's grouped (§2, group-by-kind).

* BUG / DEFECT — the procedure was followed but produced a wrong or incomplete result, or allowed one through. A NEAR-MISS (caught before harm, but a gap was revealed) is this same channel carried at a lower severity — not a separate kind.
* CHANGE REQUEST — a proposal to change the spec text itself: a wording fix, a new rule, a new section. A feature request ("AI PM should also do X") folds in here — a fix and a feature are both "change this spec text," acted on as a unit.
* FRICTION / IMPROVEMENT — a step that fought the session without an outright failure: a suggested betterment. This is the outbound route for §11's self-honing output (see §6): in an adopting project, friction §11 notices is reported here rather than folded into the local doc.

The line between a CHANGE REQUEST and a FRICTION report: a change request already knows the fix and proposes exact text; a friction report names the problem and offers a candidate direction, leaving the maintaining project to design the fix. When in doubt, file friction — it asks less of the reporter and leaves the design where the authority is.

***

2. THE SHARED ENVELOPE AND THE TEMPLATES

Every report opens with the same short envelope, stated once per file:

* Originating project — the project's name.
* AI PM version in use — the version stamp the reporting project's AI_Project_Manager.md carries. (Intake reads this FIRST — §8 — because a bug against an old version may already be fixed.)
* Session — the date and/or the close-out commit the observation came from.
* Type — bug/defect · change request · friction/improvement.

(No model field — deliberately omitted; see the backlog's parked "model-fit handling.")

Below the envelope, the body depends on the type.

BUG / DEFECT body — one per item:

* Severity — DEFECT or NEAR-MISS.
* What happened — the concrete sequence, enough for a stranger to picture it.
* Impact if uncaught — what would have gone wrong had it slipped through. This is what sets the severity.
* Suspected cause — the reporter's best read on why, naming the spec wording that invited the error if any.

CHANGE REQUEST body:

* Target — the doc and section the change lands in (e.g. AI_Project_Manager.md §4).
* Proposed text — the actual words to add or replace, exact enough that the maintaining project can weigh the wording, not just the intent.
* Evidence — the worked example or observation that motivates it; why the current text is wrong or insufficient.

FRICTION / IMPROVEMENT body:

* What fought — the step or rule, and how it rubbed against the session.
* Candidate fix — a direction, not necessarily finished text (the finished-text version is a change request).

GROUP-BY-KIND. A session's observations are grouped by how they're acted on, not dumped into one file:

* All bugs, near-misses, and friction from a session batch into ONE file — a FIELD REPORT — envelope stated once, a body per item under a severity legend. These are observations the maintaining project logs and weighs together.
* Each CHANGE REQUEST gets its OWN file, because it proposes specific spec text and is acted on as a single unit.

So one messy close-out might produce one Field Report (three observations) plus two Change Requests — three files. The Feedback Pass states the tally up front: "3 observations + 2 proposed changes → one Field Report + two Change Requests."

FILENAMES. Report files are born unique, on the OUTBOUND side, because the inbound side cannot fix a collision it inherits — two reports arriving identically named is a real failure this convention exists to prevent. The form:

```
YYYY-MM-DD_Originating-Project_type_topic-slug.md
2026-07-18_IFQ-Tracker_field-report_edit-timing.md
2026-07-20_IFQ-Tracker_change-request_backlog-role-wording.md
```

Date first so a folder sorts chronologically; the originating project next so a maintainer with several adopters can see the source at a glance; then the type (field-report or change-request); then a short topic slug naming the subject, not the verdict. If two files would still collide — same project, same day, same type, same topic — append the time (`_1430`). The reporting project owns this: a report is named when it is written, never renamed on arrival.

***

3. THE FEEDBACK PASS (outbound)

The Feedback Pass is how a report gets produced. It is auto-triggered and default ON.

WHY IT STAYS AUTOMATIC, when almost nothing else in AI PM is. The Pass is a self-CHECK that may SUGGEST a report: it observes and notifies, and it never acts — nothing is written without the developer saying so. It stays automatic because it exists to catch a failed close-out, and a failed close-out is precisely the thing nobody thinks to go looking for. A check that only runs when asked cannot catch the case where no one suspected anything was wrong.

WHEN IT RUNS: after a close-out completes — never inside it. This is deliberate: the Pass needs to see the FINISHED close-out to audit it, including whether the session's work was actually committed and the handoff actually rewritten. That exact failure — the uncommitted session, a close-out described rather than completed — is one the Pass exists to catch, and it can only be seen from outside, after the close-out claims to be done.

WHAT IT DOES on auto-trigger:

1. Runs just enough of the close-out self-check (§4) to know whether anything is worth reporting, and surfaces any friction the session raised (§11).

2. If nothing is warranted, says so in one line and stops. Don't manufacture reports — the same discipline as "don't manufacture updates" at close-out.

3. If something is warranted, gives a SHORT notice — one or two lines on what and why — then three options:

   * Write it now — draft the report file(s) immediately: into the maintaining project's Feedback\ folder if this project can reach it, else the local outbox (§9). The fast, low-token default.
   * Talk it through first — open a real shaping conversation before writing. Change requests especially benefit; a friction report's candidate fix is often worth discussing.
   * Skip — file nothing this time. Beneath the three, one standing line: "To turn off auto-reporting, ask me to set `Feedback Pass: off` in AI_PM_Config.md."

The notice is short BY DESIGN. A close-out has just finished and its handoff is the developer's next real need; the Pass must not bury it under process. Keep the auto-notice to the tally plus the three options unless the developer opts into "talk it through." The three options ARE the directions the developer needs in that moment; the notice does not also explain the commands, which answer a different question — how to re-run this later — and belong in §5 and the README where someone goes looking for them.

The Pass is not a close-out stage and does not wear a stage banner. The close-out is over; this runs afterward, and it is the only thing following the report that can still write a file.

***

4. THE CLOSE-OUT SELF-CHECK

The close-out self-check is the audit the Feedback Pass is built on: a verification that every close-out stage actually HAPPENED rather than was described. It is general — worth running in any project, including the maintaining one, whose own close-outs can fail the same way.

It walks the close-out's own promises and confirms each is real, not described:

* Everything approved landed — the diff holds the approved changes and no others.
* Committed — the close-out ended in its single commit and the tree is now clean. Work left uncommitted is the failure the whole scheme guards hardest against.
* Handoff ready for the next session — the handoff file carries THIS session's opener and changed in the commit; a handoff pointed at but never rewritten fails this check.
* Backlog and changelog updated — the backlog reflects the new state, and the changelog gained this session's entry under the right version header.
* Version stamps match the commit — a doc whose internal version stamp was not bumped to match the commit is a mismatch that hides; the commit message itself is not checked, since it is printed in the report where the developer can read it.
* Method files untouched — no file in Documents\AI PM\ changed this session. This NOTIFIES only: the method is a guideline and the developer keeps full rights, so a deliberate local hotfix is theirs to make, and the next upgrade's drift check carries it home rather than losing it.
* Nothing outside the project changed — no file outside the project's own folders was created, edited, deleted or moved this session; nothing was installed or uninstalled machine-wide; and nothing landed in a method-owned path that has a designated folder. IN BOUNDS is the project's folders, this session's own scratch area, and whatever the project lists under ALLOWED WRITE LOCATIONS in AI_PM_Config.md. Like METHOD FILES UNTOUCHED this NOTIFIES only, and it reports a grouped COUNT with the list available on request. One class always flags regardless of the allowed list: a change to what the AI is permitted to do in future sessions — permission settings, automatic hooks, tool-server configuration, persistent cross-session memory. Full definition in AI PM §7.
* Method friction — did any part of the procedure fight this session (AI PM §11)? It is asked here, at the end, because the close-out itself is part of what might have chafed.

Most of these checks are mechanical — git status, git log, whether the handoff and changelog changed in the commit — which is the point: they are quick, and the developer can re-run every one of them by eye. Any failed check becomes a proposed BUG report, pre-filled from what the self-check saw. The common root it guards against: substituting a description of a deliverable for the deliverable itself — a handoff "pointed at" but never rewritten, work "wrapped up" but never committed. AI PM §10 names the shape: the uncommitted session.

***

5. THE COMMANDS

Two commands, slash-canonical with plain-language fallback (listed in AI_Project_Manager.md §8):

* /self-check (plain "run close-out self-check") — runs the §4 audit alone and reports what it finds. Useful on its own even in a project that never files a report: a pure "did this close-out actually complete?" check.
* /feedback-pass (plain "run Feedback Pass") — runs the full outbound engine (§3): the self-check plus friction-gathering, ending in the notice-and-three-options. Use it when auto is off; when it's on but didn't fire and you want a report anyway; or to file a report NOT bound to a close-out — a mid-session realization worth carrying home.

The Pass subsumes the self-check (it runs it internally), but the self-check stands alone as its own command because its "did the close-out complete?" answer has value independent of whether any report is filed.

***

6. §11 IN ADOPTING PROJECTS: DETECTOR-ONLY

AI PM §11 (the self-honing loop) tells the AI to notice friction and fold improvements back into AI_Project_Manager.md. That fold-it-in step is a MAINTAINING-project capability only. An adopting project never edits its own copy of AI PM — one source of truth, no divergent local edits.

So in an adopting project, §11 is DETECTOR-ONLY: it notices the friction and routes it to the Feedback Pass as a friction report, full stop. It folds nothing into the local doc. This resolves what would otherwise be a double-report overlap — friction should surface via the Pass, not also self-fold.

In the maintaining project, §11 keeps its full power: friction it notices can be folded directly into the method's source at a later close-out, because this is where the authority to edit AI PM lives. Its own friction can still ride the Pass too — reports come home to itself — but here the loop can also close directly.

***

7. THE TOGGLE

The Feedback Pass is default ON. To turn it off for a project, the developer asks the AI, which sets the standing line in Documents\AI PM\Config\AI_PM_Config.md:

```
Feedback Pass: off
```

Why the config file: it is read in full at every session start so the setting is always in view; it is naturally per-project; and it survives an AI PM version upgrade untouched — an upgrade replaces the method's FILES and never touches the subfolders, of which Config\ is one. Flipping the toggle is a normal AI edit that lands in a session's close-out commit like any other. The toggle used to live in AI_PM_Instructions.md's project section; that section no longer exists, because project state and method text no longer share a file.

Existing projects need no migration: default is on, so they get the Pass automatically. Adding the toggle line is optional — only if a project wants to opt out.

***

8. THE INTAKE HALF (inbound)

Intake is the receiving side — how the maintaining project turns reports into backlog items. It is its own session, not part of producing reports.

How it runs:

* No pasting: intake runs in the maintaining project, and the AI reads the report files directly from the feedback folder (§9) — "process the feedback folder" is prompt enough.
* The AI walks them in bite-size chunks — one report (or one item) at a time, confirming before moving on — per AI PM's information-pacing default.
* VERSION-CHECK FIRST, per report. The envelope's "AI PM version in use" is read before anything else: a bug filed against an older version may already be fixed in the current source, in which case the report is noted and closed, not queued. This check leads because it can retire a report before any effort is spent weighing it.
* Nothing lands in the backlog without the developer's approval. The AI proposes a backlog item per surviving report; the developer approves, adjusts, or drops it. Intake produces backlog items — it does not build.
* Two-stage flow: report → backlog (this intake session), then backlog → build (a later working session). Intake never folds a change straight into a doc; it queues the work.
* When a report has been processed, the AI moves its file to Feedback\Archived\ (§9) in the same session.

The maintaining project runs the Feedback Pass on its own sessions too, so its own observations arrive at intake the same way — the zero-distance trip. The only difference from an external report is the distance travelled; the processing is identical.

***

9. THE FEEDBACK FOLDER SCHEME

Reports live in the maintaining project's workspace, in a folder beside (not among) the method's shipped files:

* Incoming reports stage in Feedback\.
* Once processed through intake, they move to Feedback\Archived\.

"Feedback" is the umbrella term deliberately — the folder carries Field Reports, Change Requests, and friction reports alike, not just field reports. A report in Feedback\ is unprocessed; a report in Feedback\Archived\ has been walked through intake and its backlog items (if any) approved.

GIT HANDLING, and the principle behind it: CORRESPONDENCE IN TRANSIT IS NOT PROJECT MEMORY, BUT PROCESSED CORRESPONDENCE IS. A report on its way somewhere is mail; a report that has been read and acted on is part of the record of why the method changed.

* Documents\AI PM\Feedback\ and Documents\AI PM\Upgrade\ — GIT-IGNORED in every project. An outbound report waiting to be carried, and a release package waiting to be applied, are both passing through. The Adoption Guide installs these .gitignore lines at adoption and states the reason, so an adopting project never has to work it out.
* The maintaining project's intake folders follow the same line. Unprocessed reports staging in Feedback\ are ignored — they are inbound mail, and an untracked file appearing at every session start is noise the pre-flight sweep would keep flagging. Feedback\Archived\ IS tracked: once a report has been through intake it is evidence for a backlog item and belongs in the record, which matches what the maintaining project already does.

The rider from AI PM §9 applies to everything ignored here: git does not protect untracked files. Before deleting one, make a deliberate copy or get an explicit "genuinely disposable" ruling.

REMOTE ADOPTERS — projects without access to the maintaining project's folders: their Feedback Pass writes the report files locally instead, to the project's own outbox at Documents\AI PM\Feedback\Outbox\, and ends with one instruction — "send these to the AI PM maintainer." The maintainer receives them by any channel and drops them into Feedback\ for intake. Same protocol, one human transport hop. (The outbox lives under the method folder deliberately; an upgrade replaces the method files there, never the folder's subfolders, so sent-and-kept reports survive.)
