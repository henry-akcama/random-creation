AI PM — Close-Out Procedure

Version: 4.0 — August 2026. METHOD-OWNED: byte-identical in every project, replaced whole by an upgrade. This file is what the AI EXECUTES at close-out — read it in full when a close-out command arrives, then follow it. The reasoning behind these rules lives in AI_Project_Manager.md (read when someone asks why, not here, not now).

ENTRY

* Slash-only: /closeout (interactive) or /closeout-auto (gates removed). Plain language never starts a close-out — name the two commands and wait. The AI may propose a close-out in one sentence containing no close-out work; presenting a pre-flight list IS starting one uninvited.
* AUTO mode: same scope and steps, interactive gates removed — no pre-flight pause, no plan approval. Judgment calls are flagged at the top of the report and in the commit message. Unanswered questions are never guessed into docs; they ride the handoff. Auto pushes per the Push setting, runs every check, and a judgment-tier HALT still halts.

HARD RULES (the three that never bend)

1. Every close-out ends in one MEMORY COMMIT, message "close-out — <the changelog headline>". Work product commits mid-session with the developer's approval (its own plain-description commits); memory commits only here.
2. Before the memory commit, its diff is self-reviewed hunk by hunk against the approved plan. Anything unplanned is reverted or flagged as a deviation — never silently kept. The review covers the memory commit only; work commits were reviewed when they landed and are never re-diffed.
3. Nothing leaves the project except through a commit. Git is the archive: deletion in a commit IS archival. Rider: untracked and ignored files are NOT protected — explicit copy or an explicit "genuinely disposable" ruling before deleting one.

THE PIPELINE

Step 1 (pre-flight) → developer answers or defers → Step 2 (proposed changes) → developer approves → Step 3 writes, self-reviews, commits → Step 4 reports. Steps 3 and 4 arrive together, one message, no gate between or after — the single exception is the HALT, where nothing commits.

PRESENTATION (fixed; fill the skeletons, never compose)

* Fixed banners, fixed sections in fixed order, check band last. Horizontal rule between sections.
* One continuous number line across a step's sections, so "4: agree" is never ambiguous.
* Items in plain English, readable cold; the AI's suggested answer rides an item as a quiet italic line.
* Empty content sections are hidden. Check bands are always shown. A band of 3+ items stacks one per line.
* Emoji vocabulary: 1️⃣–4️⃣ step banners · ✅ check clean · ❌ check flagged · 🔴 the delete group · ⚠️ the AI is objecting (never a mere status).
* A ❌ never carries the detail — it points at a numbered item in that step's normal content.
* Clean results are one line, everywhere. Findings earn detail; a "nothing happened" never does.

STEP 1 — PRE-FLIGHT. Surface everything needing the developer's answer or action before any doc is planned: loose ends, unresolved forks, physical to-dos. Mechanical sweep both directions: did anything change in the tree outside this conversation (git status), and did this conversation change anything outside the project (feeds Step 4's line). Sweep findings become numbered items; the band points at them.

* SEAM CHECK: uncommitted work-product changes in the tree are surfaced — commit as a final work commit, or leave deliberately by name. Never riding the memory commit, never evaporating.
* HOMELESS FACTS: a durable fact with no home under the Doc Map is surfaced with a recommendation, like any finding; the developer rules. A project-scoped fix is ordinary project work; feedback is suggested only when the fix would change the method itself.
* Docs are written once, after the answers — never revised post-hoc for a late question.
* "Defer all" is valid; deferred items ride the handoff. "Proceed" with open items gets a defer-check first.
* A ⚠️ here means the close-out came TOO SOON (e.g. a rewrite half-landed on disk). A flag, never a block.

> 1️⃣ **Close Out — Pre Flight**
>
> **Needs your answer** — numbered questions and forks, suggestion beneath each
>
> **Needs your action** — human-only to-dos; numbering continues
>
> **Also surfaced** — the catch-all; numbering continues
>
> **File check** — ✅ tree clean and nothing outside the project — or the line pointing at the items above
>
> **Over to you** — answer by number · defer any or all · **proceed**

STEP 2 — PROPOSED CHANGES. The doc plan, grouped New → Edit → 🔴 Delete (the destructive group last, immediately before approval). Under each group, numbered entries: FILENAME in bold, substantive changes as bullets, a quiet italic footer with the folder path.

* THIS STEP ASKS NO QUESTIONS. A question the AI discovers while planning sends the close-out back to Step 1. A pre-flight item ANSWERED with a question is a normal Step 1 round trip, not a bounce; if the answer settles something durable it enters Step 2 as its own vetoable entry.
* Standing checks, judged against the PROPOSED state: COHERENCE — has any doc absorbed enough edits that it stops reading as a whole? (flag → a queued coherence session, never done inside a close-out) · RETIREMENT — should any file leave the project entirely? (flag → a 🔴 entry in this plan).

> 2️⃣ **Close Out — Proposed Changes**
>
> **New** / **Edit** / **🔴 Delete — N files**
>
> **Also surfaced**
>
> **Standing checks** — ✅ Coherence · ✅ Retirement
>
> **Over to you** — veto or adjust by number · ask about anything · **approve**

STEP 3 — EXECUTE + SELF-REVIEW.

1. Write the approved changes. Read the current file in full before editing it. Targeted edits are the normal delivery; a full rewrite only when the doc's shape changes.
2. FILE HEALTH CHECK, after drafting, before presenting: the drafts against the standing files' charters (below) and the Doc Map's routing. Changelog entries LOCATE the record, they never ARE it. No graveyards, no riders, no homeless facts — a fact with no home is flagged as a missing drawer, never dropped. Findings are fixed if execution-tier, halted if judgment-tier. Clean = one ✅ line.
3. The memory-commit diff, hunk by hunk against the approved plan (hard rule 2).
4. Commit: "close-out — <the changelog headline>". Then push per the Push setting — immediately, silently if off; a failed push is reported, never resolved.

DEVIATION HAS TWO TIERS — the test is DECISION vs EXECUTION, not size:

* EXECUTION (forced and mechanical — a dead path, a stale cross-reference): make it, say so on that file's line, mark the diff check ❌, commit.
* JUDGMENT — THE HALT: the developer could plausibly have ruled differently. Stop; NOTHING COMMITS. The halt block goes first, above every section: the decision needed, the plain statement that files are written and waiting, the two ways out (answer → finish and commit · revert → back to Step 2). A rejected push, or an upstream moved since session start, is a halt: surfaced, resolved with the developer, never force-pushed, never silently merged.

> 3️⃣ **Close Out — Execute + Self Review**
>
> **Created** / **Edited** / **🔴 Deleted — N files** — past tense, mirroring Step 2; each entry the filename, "as approved" or the deviation, and a footer with the path and the diff numbers
>
> **Also surfaced**
>
> **Diff check** — ✅ the diff contains only the approved changes · ✅ file health — drafts match charters

STEP 4 — REPORT. What Step 3 cannot carry: the commit that now exists, the proof the close-out completed, where the next session starts. Repeats Step 3's file list nowhere.

> 4️⃣ **Close Out — Report**
>
> **Committed** — the commit message, then a quiet italic line with the short hash and totals
>
> **Work commits this session — N** — bare one-liners, hash + description. A list, not a re-review. Hidden when none.
>
> **Self check**
> * ✅ everything approved landed
> * ✅ tree committed
> * ✅ pushed (<remote>) — or "— off" or ❌ with the reason; carries any mid-session push failure still unresolved
> * ✅ handoff ready for the next session
> * ✅ backlog and changelog updated
> * ✅ method files untouched — notifies if one changed; the developer keeps full rights
> * ✅ nothing outside the project changed — or ✅ with a grouped count when in-bounds writes happened (list on request); ❌ when something landed outside. A change to what the AI is PERMITTED TO DO in future sessions always flags, wherever it lives.
> * ✅ method friction — did any part of this procedure fight the session? A yes is a candidate improvement, queued if the developer agrees.
>
> **Session name** — the suggested name: the changelog headline, no number. Format is the developer's preference.
>
> **What's next** — a sentence or two, no more.

A ❌ in Step 4 lands after the commit: say what is missing and propose a follow-up commit — an incomplete close-out is reported honestly, never amended into hiding.

THE WRITE-LIST — the files a close-out maintains, each with its charter:

* Changelog.md — the story, told once: what happened, when, where the detail lives. Append-only, newest on top, entries lead DATE + HEADLINE. An entry locates the record; it never is the record. Not read at session start.
* Handoff.md — ONE JOB: open the next session. Rewritten whole; content bounded by the next session's needs. No standing riders — a standing fact goes to the backlog or its owning doc.
* Backlog.md — forward-looking to-dos only. Items are a line or two pointing at the doc with the full picture; done or declined items leave the file entirely; parked-with-trigger items stay in their own short section. Empty on a quiet project is correct.
* Doc Index (Documents\DOC_INDEX.md) — every record doc, its one-sentence scope, its lifecycle state. Touched only when a doc was born, changed scope, or retired.
* Config — only when a setting changed.
* Record docs — as the approved plan names them.

No file stores project status: the handoff's opening is the only snapshot; human-facing summaries are generated on request from the changelog.

DRY RUNS. A close-out may be rehearsed — announced as a dry run, never entered from a command. It WRITES NOTHING, COMMITS NOTHING, DECIDES NOTHING: an answer inside a rehearsal is not a ruling. A real decision a rehearsal surfaces is raised separately, outside it.

After a full close-out completes, the Feedback Pass runs if enabled (see Feedback_Procedure.md). Clean = one line: "Feedback Pass — ✅ clean, nothing filed."
