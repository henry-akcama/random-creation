AI Project Manager

Version: 3.2 — July 2026. This file is the method — carried by every project that runs it, and the version number above tells you at a glance whether a project's copy is stale. The MAINTAINING project (the one that designs and revises the method) is the single source of truth: improvements flow back to it (§11, §13), and it hands the method to adopters as a release package (§12). Human-facing README + glossary: AI_Project_Manager_README.md, shipped in the same package. This file names no paths outside a project's own folder; inside one, the method's files live at standard paths (§3).

Internal changelog (five lines max; older history lives in the maintaining project's git history):

* v3.2 (July 2026) — The close-out self-check gains an eighth line, OUTSIDE THE PROJECT (§7): the changes a session made beyond the project's own folders, reported as a grouped count with the list on request, the in-bounds set configurable per project (§3, ALLOWED WRITE LOCATIONS), and one class — a change to what the AI may do in future sessions — that always flags. §7's check bands now stack one item per line at three items or more, because a seven-item middot run wraps into prose in a real client width. Version numbering collapses to one number per thing that moves: the shipped method docs carry the AI PM version alone, the clause tying spine-doc versions to the project Major is gone, and a close-out never re-asks a declared release number (§7). The handoff carries the PROJECT version only (§3), so an upgrade cannot leave a stale method version in a preserved opener. A pre-flight item answered with a question is a normal Stage 1 round trip, not a bounce (§7). Docs are named for their scope, never for a version number (§4).
* v3.1 (July 2026) — The mission statement opens the document. Close-out entry becomes slash-only and §7 gains fixed stage skeletons with a defined check band per stage, plus the two-tier deviation rule and the halt; stages 3 and 4 run through with no gate between them. New §12 upgrade subsystem (/ai-pm-upgrade, staging, detection, drift check) and its commit carve-out in §1. §3 gains the files-versus-subfolders ownership scheme and the config file, which takes over the project section so AI_PM_Instructions.md becomes pure method text. §5 restated around memory versus work product with the volatility test. Dry runs defined. Design record: AI_PM_vNext_Design_Decisions.md in the maintaining project.
* v3.0 (July 2026) — Milestone: the Claude Code migration. The method re-targets from Claude Projects to Claude Code: git becomes the verification layer (three new hard rules, §1), file delivery dies (the AI edits directly; every close-out ends in one version-led commit), the handoff becomes a tracked file, slash commands become the canonical command form, and shipped files are path-free (the release-package model). The method's footprint moves into Documents\AI PM\ under a clean ownership split: CLAUDE.md belongs to the developer, the method's front door is the method-owned AI_PM_Instructions.md, and the cadence files carry standard names in every project (§3). Substance — roles, stranger test, gates, self-honing, feedback loop — unchanged. Design record: AI_PM_v3.0_Claude_Code_Migration_Design.md in the maintaining project.
* v2.0 (July 2026) — Milestone: the Feedback Protocol subsystem shipped (AI_Project_Manager_Feedback_Protocol.md v1.0, a new permanent resident). New §13 points to it; §8 gains two feedback commands; §11 gains the detector-only rule for adopting projects.
* v1.3 (July 2026) — §12 now points to AI_Project_Manager_Adoption_Guide.md (v1.0, new), the full interactive adoption procedure; §12 itself stays the checklist summary.
* v1.2 (July 2026) — Naming fix: the "CADENCE DOC" role renamed to AI PROJECT MANAGER (AI PM); "AI PM" is the standing short form.

***

MISSION

AI PM gives an AI-assisted project a memory the human can trust.

1. The files are the AI's memory. The AI has none between sessions, so the project's files are made to be it: written AI-first, complete enough that the next session's AI rebuilds full working context from the files alone. Humans get up to speed by asking the AI.

2. Memory is written after the conversation, not during it. A topic gets worked all the way through, every decision made, before anything is committed to memory, because a follow-up can reopen anything half-written. Work product (mockups, code, artifacts) lands mid-session with the human's approval; everything else waits for close-out. The test: if the conversation keeps going, could this edit change? Then it waits.

3. Every memory change passes human review. Whoever is instructing the AI holds the gates, one developer or a team; what matters is that it is a human. Close-out runs in clear, recognizable stages so nothing important is lost, nothing unwanted lands, and nothing is removed by accident, with git as proof anyone can check.

4. One method, many projects. Portable and adoptable as-is, upgraded from one source of truth, improved by feedback flowing home to the maintainer.

***

1. SPIRIT

This document is defaults and triggers, applied with judgment — not a script to recite. Only three rules are hard: (1) every close-out ends in a single commit whose message leads with the project version; (2) before that commit, the session's diff is self-reviewed against the approved plan — git diff walked hunk by hunk, every change mapped to a plan item; anything unplanned is reverted or flagged as a delta, never silently kept; (3) nothing leaves the project outside a close-out commit — git is the archive, every committed file is recoverable from history forever, so deletion in a commit IS archival. Rider on rule 3: untracked and ignored files are NOT protected by git — an explicit copy somewhere deliberate, or an explicit "genuinely disposable" ruling, comes before deleting one. Everything else bends when the situation calls for it — and when a step fights a session, that friction is a candidate improvement to this document (see §11), not something to grit through forever.

CARVE-OUT to hard rule 1: the AI PM UPGRADE COMMIT is the one commit species that is not a close-out commit (§12). It contains only method files, its message takes the form "AI PM upgrade X.Y → X.Z", and it does not move the project's own version. Everything else that lands in the project still arrives through a close-out commit.

2. WHAT THIS IS

A method for running long, multi-session projects with an AI assistant. The AI has no memory between conversations, so the project's files are made to BE the memory. The test everything here serves: a stranger — the developer after a year away, a new person, or the AI itself in the very next session — can rebuild full working context from the files alone, with no handoff and no one to ask.

Platform: the method targets Claude Code. A project is a FOLDER — a directory carrying a CLAUDE.md and a git repository (.git\, the snapshot archive that gives the method mechanical, complete, human-inspectable history). The assistant auto-loads the folder's CLAUDE.md at session start; that is the whole project-detection mechanism, and it needs no registration step.

3. THE ROLES

The method is defined in role terms so it ports to any project. Under v3.0 the roles bind to STANDARD files at standard paths — identical in every project that runs the method, so no per-project role mapping exists. Ownership is the organizing idea, and inside Documents\AI PM\ it falls on a boundary §12 already draws: the FILES are the method's, replaced only by an upgrade and edited by no one; the SUBFOLDERS are the project's. So: the developer owns CLAUDE.md, the method owns the files in Documents\AI PM\, the project owns that folder's subfolders, and the AI maintains the cadence files in Documents\. The full scheme:

| Path | Owner | Git | Edited when |
| --- | --- | --- | --- |
| CLAUDE.md | developer | tracked | whenever they like |
| Documents\ cadence files | AI | tracked | close-out |
| Documents\AI PM\ files | method | tracked | upgrade only |
| Documents\AI PM\Config\ | AI (project state) | tracked | close-out |
| Documents\AI PM\Feedback\, \Upgrade\ | transit | ignored | n/a |

Method files MUST stay tracked: the upgrade's drift check diffs against committed history, and the upgrade commit records the version transition. The transit subfolders are git-ignored because reports and release packages are correspondence passing through, not project memory — the Adoption Guide installs those .gitignore lines at adoption.

* CLAUDE.md — the developer's front door, USER-OWNED, at the project root: their project description, conventions, domain orientation — whatever they want auto-loaded into every session. The method's only claim on it is one short fenced pointer block (installed at adoption; canonical text in the Adoption Guide) routing the session to AI_PM_Instructions.md. Keeping that block intact is the developer's one obligation; everything else in the file is theirs to edit freely, and the method never carries orientation here.
* AI PM INSTRUCTIONS — Documents\AI PM\AI_PM_Instructions.md, METHOD-OWNED and 100% method text: byte-identical in every project that runs this version, edited by no one for any reason, replaced whole by an upgrade. It carries the session-start procedure (§6) stamped with the AI PM version, the standing rules that must be in view from a session's first read, and the route to the config file. Read in full at every session start.
* CONFIG — Documents\AI PM\Config\AI_PM_Config.md: the few genuinely per-project facts — record-docs list, the Feedback Pass toggle, project version at adoption, external-storage note, and ALLOWED WRITE LOCATIONS (§7). AI-maintained, edited at close-out like any other memory, tracked, and untouched by upgrades. It carries no version stamp of its own: it is the project's own state, not a shipped doc on a release schedule, and its history is in git. It exists so the instructions file can be pure method: project state and method text never share a file.
* BACKLOG — Documents\Status_and_Backlog.md: current status + what's next. The only file that churns by design — but only ever at close-out, like every managed doc. It stays short forever: completed work leaves it (its record goes to the changelog and the record docs).
* CHANGELOG — Documents\Changelog.md: what happened, when, and where the detail lives. Append-only, newest on top, entries grouped under project-version headers; the AI prepends new entries directly at close-out. git log is complete but flat — the changelog is the curated narrative that passes the stranger test: an entry LOCATES the record (date, what, which doc or commit holds the detail), it never IS the record; entries stay slim and may point at commits for mechanical detail. NOT read at session start; read on demand — dating or locating past work, a consolidation pass, walking a returning human through the project.
* RECORD DOCS — the permanent record: designs, decisions, execution. Each is authoritative for a stated scope.
* HANDOFF — Documents\Handoff.md: the transient next-session opener (see §7), a tracked file overwritten at every close-out, opening with the current project version (the alignment reply quotes it). Transient in content, permanent as a file — its history in git is a free session-by-session trail. THE PROJECT VERSION ONLY: a handoff never states the AI PM version, whose single home is the stamp on AI_PM_Instructions.md. Two copies of a number is one copy too many, and this one goes stale on a schedule nobody controls — an upgrade preserves the opener by design (§12 point 7), so a method version written into it is wrong three lines below the annotation saying so. The alignment reply reads each number from the file that owns it. DISCIPLINE: the handoff carries the next session's opener and transient working notes ONLY. Standing items — queued sessions, parked ideas, long-lived rulings — live in the backlog and are never re-listed here. A handoff that restates the backlog turns every pre-flight into a duplication hunt.
* AI PROJECT MANAGER (AI PM) — this file, in Documents\AI PM\, present in every project that runs the method — beside the Feedback Protocol, the README, and the AI PM instructions, with the project's feedback outbox beneath it (Feedback\Outbox\, created when first needed).

Optional roles, adopted on a trigger and skipped otherwise:

* EXTERNAL STORAGE MAP — a curated map of the project's off-workspace storage (folder purposes + routing rules, never a file manifest; folders not listed are deliberately out of scope). Adopt when routing questions recur or the storage tree outgrows a section in CLAUDE.md. Build input: a folder tree (the AI can run tree "path" /A itself), curated together.
* LOGS — append-only files (correspondence logs, outcome logs). Same direct-prepend handling as the changelog.

4. DOCUMENTATION PHILOSOPHY

* Who docs are for: project docs are written for a reader doing the work — dense, plain, complete, diff-friendly markdown; primarily the AI, but deliberately legible to a human, carrying the why and not just the conclusions. Human-facing outputs (close-out reports, summaries, the README) get real formatting — headers, bold, tables.
* Self-revision vs. the amendment chain: a doc revising ITSELF — fixing its own error, extending its own scope — is a normal versioned revision. The amendment chain is for a LATER doc changing truth an EARLIER one states: never rewrite history to match new reality; the newer doc declares authority over the conflict, and the record-docs list in AI_PM_Config.md notes who amends whom. A doc's date and place in the chain tell a stranger exactly how much to believe it.
* New doc vs. extend: a new doc when a new durable scope is born (a design that didn't exist, a build executing a design); extend when the same scope grows. The tell: if you can't state in one sentence what the doc is authoritative FOR, it's an append, not a doc.
* CONSOLIDATION: over time the truth about one topic spreads across several documents — a design plus every later doc that amended it. The AI must stack all the layers to know what's current; each layer costs context and risks a silently dropped ruling. Consolidation is a cleanup session: fold all the layers into one fresh base doc (new major version), verify by a coverage audit (walk each amendment and confirm its ruling appears in the new base — a diff can't do this job), retire the folded layers (§9), and restart the chain clean. Trigger: a topic's chain crosses ~4–5 documents, or a session hits an assembly error (a dropped layer discovered the hard way — that's a fire alarm, not a statistic). Always its own session, never done inside a close-out.
* NAMING: a doc is named for its SCOPE, never for a version number. A doc that lives on states its version in its first line, where it can be updated; a version in the FILENAME forces a rename every time the scope moves, and a rename costs every reference to it. The one exception is a file that is a frozen record OF a single version — a shipped release folder, an archived package — where naming the version is the whole job.
* Vocabulary discipline: the AI uses the method's established terms (glossary in the README) and does not coin new process vocabulary by casual use. A new concept that needs a name is named explicitly — "I'm calling this X, meaning Y; glossary at close-out." The developer's standing move: "define [term]" — and if the AI can't define it crisply, the term shouldn't exist.

5. TRUST CONVENTIONS

The verification scheme depends on these; they are stated so they survive to every project:

* MEMORY vs WORK PRODUCT — the rule that decides WHEN a file may be edited. MEMORY is anything decision-carrying: the record docs, backlog, changelog, handoff, CLAUDE.md, the config file. It is edited only at close-out, because a conversation that keeps going can reopen anything half-written. WORK PRODUCT is what the session produces: mockups, graphics, code, drafts, artifacts. It lands mid-session with the developer's approval — and is still verified by the stage-3 diff review, since everything rides the single close-out commit. The line moves WHEN edits happen, never whether they are reviewed. The tiebreaker for an ambiguous file is the volatility test: IF WE KEEP TALKING, COULD THIS EDIT CHANGE? If yes, it waits. This is deliberately strict — even an index paired with renamed files waits, because the running conversation already holds that context and nothing is misled by the delay. Mid-session editing is for the case where WAITING would actively mislead the session in progress.
* No declaration ritual is needed beyond that: git status at session start mechanically surfaces any change made outside a close-out. Residual: git shows THAT the tree changed, not who or why — a dirty tree gets a one-line check ("uncommitted changes in X — was that you?") before work begins.
* Git history is the version archive. Every committed version of every file is recoverable forever; version-led commit messages are the pointers, and changelog entries may cite commits for mechanical detail. Conversations no longer need to be kept for file recovery.
* One commit per close-out, not commits sprinkled mid-session: the session is the method's unit of work, and a single commit means the self-review diff is exactly the whole session.
* Verification checks are designed so the HUMAN can verify them — the session-start alignment line ("Working off AI PM 3.2; project at v2.4" — the developer knows both numbers, so a stale read is caught at a glance), the version-led commit message, the clean tree after close-out. A check only the AI can attest is not a check.

6. SESSION START

The developer types only their task. CLAUDE.md — auto-loaded into every session — carries the fenced pointer block, whose whole job is one route: read Documents\AI PM\AI_PM_Instructions.md in full and follow it. That file carries the procedure: read the config file (this project's own state) and then the handoff; run git status (a dirty tree triggers the §5 one-line check); check Documents\AI PM\Upgrade\ for a staged release package and, if a newer one is there, carry the offer and the exact command in the alignment reply (§12); open with the alignment reply — "Working off AI PM 3.2; project at vX.Y per the handoff," the git result, and what the next move is. The AI PM version comes from the instructions stamp and the project version from the handoff — each from the one file that owns it, and neither file repeating the other's number — so the one line proves both files were actually read. ALIGN BEFORE BUILDING, always; a cold session can lose the thread. If there is no useful handoff (first session, or one gone missing), align from the backlog. Mid-session, if the instructions may have gone stale in the AI's view (long sessions), the remedy is simply re-reading the file — always available, always fresh.

7. CLOSE-OUT

ENTRY IS SLASH-ONLY. A close-out starts one way: /closeout, /closeout-light, or /closeout-auto. Plain language does NOT start one — the AI names the three commands and waits. The AI may PROPOSE a close-out in one sentence that contains no close-out work; presenting a pre-flight list IS starting one uninvited. This is a deliberate, stated exception to §8's plain-language philosophy, and it buys two things: the entry gate is mechanical rather than interpretive, and the close-out TYPE is always explicit. AUTO removes gates INSIDE the procedure, never the entry command.

The variants:

* LIGHT (/closeout-light) — pre-flight, then backlog + changelog + handoff + the commit; plan approval collapsed into one message. For sessions where no record docs changed.
* AUTO (/closeout-auto) — full scope with the interactive gates removed: no pre-flight pause, no plan approval; the AI makes the judgment calls and proceeds, flagging them at the top of the report and in the commit message. Unanswered questions are NEVER guessed into docs — they ride the handoff as open items. For walk-away close-outs; the developer reviews after instead of steering before.

THE FULL PIPELINE, and where it stops:

Stage 1 → the developer answers or defers → Stage 2 → the developer approves → Stage 3 writes, self-reviews, and commits → Stage 4 reports. Stages 3 and 4 arrive TOGETHER, in one message, with no gate between them and none after Stage 3. A gate there would ask "did I do what you already approved?", whose answer is yes nearly every time; gates that get rubber-stamped are worse than no gate, because they train the eye to skip the gates that matter. Stage 2 is the consent moment — the last point at which changing course is free. The single exception is the halt (below), where Stage 3 stops and nothing commits.

PRESENTATION IS FIXED. Each stage below is a literal skeleton: fixed banner, fixed sections in fixed order, the check band last. The AI FILLS the skeleton and never composes the presentation — a close-out that looks the same every time is one the developer can scan instead of read. Standing conventions:

* One continuous number line runs across a stage's sections, so every item has a unique number and "4: agree" is never ambiguous.
* A horizontal rule sits between every section, including under the banner. Each section is a band the eye can land in.
* Items are plain English, one or two short sentences, readable cold — no project shorthand, no jargon. Where the AI has a suggested answer it rides the item as a quiet italic line, so the common reply collapses to "agree" or the exit word alone.
* Empty content sections are HIDDEN, not shown empty. Check bands are ALWAYS shown: the band is what proves a check ran, so an empty section announcing it had nothing was a second copy of that guarantee and dead space on every close-out.
* A check band of THREE OR MORE items stacks one item per line, so the status emoji form a column the eye can run down and a ❌ cannot hide inside a wrapped sentence. One- and two-item bands stay on a single line — a list of one is clutter. This is a rendering rule with teeth: a seven-item middot run does not fit a real client's width, it wraps into a run-on paragraph, and it degrades worst exactly when it matters most, because a band carrying a failure has more text and wraps harder. A band that reads as prose has lost the only thing it was for.
* The skeletons carry no explanation of these rules. They are learned once, from this document. Re-teaching them in every close-out is exactly the clutter a fixed template exists to prevent.
* Emoji are the status vocabulary and nothing else: 1️⃣–4️⃣ stage banners · ✅ a check came back clean · ❌ a check flagged something · 🔴 the delete group, so a departing file cannot slip past a scan · ⚠️ the AI is OBJECTING. ⚠️ is never merely a status — a flagged check is not an objection.

STAGE 1 — PRE-FLIGHT. Surface every item needing the developer's answer or action before any doc is planned: loose ends, unresolved forks, physical to-dos. Conversational recall plus a mechanical sweep of git status and the session's touched files, looking for anything that changed outside the conversation. The sweep asks BOTH DIRECTIONS of one question: did anything change in the tree outside this conversation, and did this conversation change anything outside the tree (Stage 4, OUTSIDE THE PROJECT)? git answers only the first — it reports the project's own folders and nothing else — so the second half is not a git question and never was. Sweep findings do NOT live in the check band — they become numbered items, and the band points at them. Docs are written once, after the answers, never revised post-hoc for a question that surfaced late.

> 1️⃣ **Close Out — Pre Flight**
>
> **Needs your answer** — numbered questions and forks, each with the AI's suggestion beneath it where it has one
>
> **Needs your action** — to-dos only a human can do; numbering continues
>
> **Also surfaced** — the catch-all, so an unanticipated item always has a home and never hijacks another section; numbering continues
>
> **File check** — ✅ nothing changed outside this conversation, and nothing changed outside the project — or the line that points at the items above
>
> **Over to you** — answer by number · defer any item, or defer all · **proceed**

"Defer all" is a valid one-word answer; deferred items ride the handoff into the next session and are never lost. "Proceed" while items are still open gets a defer-check first — open items are never silently dropped.

The rare ⚠️ item at this stage means the close-out came TOO SOON: the session is not at a safe stopping point, most often because a rewrite is half-landed on disk and committing now would freeze it mid-change. It is a flag, never a block — the developer can answer it, defer it, or proceed anyway. A small open question never earns one.

STAGE 2 — PROPOSED CHANGES. The doc plan, grouped by disposition in the fixed order New → Edit → 🔴 Delete. Files come into existence, then change, then leave, so the group whose approval destroys something sits last, immediately before the developer approves. A group with no files does not appear. The delete header carries a red mark and a file count for one reason: a departing file must not slip past a scan.

> 2️⃣ **Close Out — Proposed Changes**
>
> **New** / **Edit** / **🔴 Delete — N files** — under each group, numbered entries: the FILENAME in bold, its substantive changes as bullets beneath, then a quiet italic footer carrying the folder path and the doc version bump
>
> **Also surfaced**
>
> **Standing checks** — ✅ Consolidation · ✅ Retirement
>
> **Over to you** — veto or adjust by number · ask about anything · **approve**

THIS STAGE ASKS NO QUESTIONS. Questions belong to Stage 1. A question the AI ITSELF discovers while building the plan sends the close-out BACK to Stage 1 with it; a disagreement with the write-up is talked through and answered with a fresh Stage 2. Nothing is written either way.

A PRE-FLIGHT ITEM ANSWERED WITH A QUESTION is not that case, and does not bounce the close-out. It is a normal Stage 1 round trip: the AI answers, the developer confirms or rules, and the close-out proceeds to Stage 2. Pre-flight is precisely where the AI raises what it cannot decide alone, so being asked something back is the stage working rather than failing — nothing has been mis-planned, and restarting the close-out over it would be a penalty for participating. If the answer settles something durable, it enters Stage 2 as its own numbered, vetoable entry, so the consent moment still covers it.

The standing checks run here because a check belongs where its outcome can be acted on: the consolidation trigger (§4) and the retirement flag (§9) both change the plan in front of the developer and fold into the same approval and the same commit. They run AFTER the plan is on the table and judge the PROPOSED state, not the current one — which is why the band sits below the plan. The friction check (§11) is deliberately NOT here: it gives the developer nothing to act on mid-plan and cannot be answered honestly until the close-out itself is done, so it runs once, in Stage 4.

STAGE 3 — EXECUTE + SELF-REVIEW. Full rewrites or targeted edits of the changed docs, reading the current file in full before rewriting it. Then hard rule 2: the git diff walked hunk by hunk against the approved plan. Then the commit — single, its message leading with the new project version.

> 3️⃣ **Close Out — Execute + Self Review**
>
> **Created** / **Edited** / **🔴 Deleted — N files** — past tense, mirroring Stage 2's groups; each entry the filename, "as approved" or the deviation, and a footer carrying the path and the diff numbers
>
> **Also surfaced**
>
> **Diff check** — ✅ the diff contains only the approved changes

The per-file diff numbers are mechanical proof and belong in every run: "as approved" sitting beside +200 −0 says something is wrong even when the prose does not.

DEVIATION HAS TWO TIERS, and the test between them is whether the change needs a DECISION or merely EXECUTION — not how large it is. Size is a symptom; the decision boundary is the rule.

* EXECUTION. The deviation is forced and mechanical — a path that no longer exists, a stale cross-reference, a version stamp. The AI makes it, says so on that file's own line, marks the diff check ❌, and commits. Approving the plan authorized this.
* JUDGMENT — THE HALT. The deviation is one the developer could plausibly have ruled differently. The AI stops. NOTHING COMMITS. The halt block goes FIRST, above every other section, deliberately breaking band order: every other stage leads with content because content is the point, but in a halt the reason for stopping is the point, and the exception must look exceptional the moment it lands. It states the decision needed, says plainly that nothing is committed and the files are written and waiting, and offers the two ways out — answer and the AI finishes and commits, or revert and return to Stage 2. This is the only time Stage 3 has a reply block.

STAGE 4 — REPORT. What Stage 3 cannot carry: the commit that now exists, the verification that the close-out truly completed, and where the next session starts. It repeats Stage 3's file list nowhere.

> 4️⃣ **Close Out — Report**
>
> **Committed** — the commit message, then a quiet italic line with the short hash and the totals
>
> **Self check**
> * ✅ everything approved landed
> * ✅ tree committed
> * ✅ handoff ready for the next session
> * ✅ backlog and changelog updated
> * ✅ version stamps match the commit
> * ✅ method files untouched
> * ✅ nothing outside the project folders changed
> * ✅ method friction
>
> **Session name** — the version-led name, on its own
>
> **What's next** — a sentence or two, no more

The self-check is §13's, and it is the antidote to §10's unverifiable claim: every line is something the developer can confirm WITHOUT trusting the AI. It deliberately restates "everything approved landed," which Stage 3 also reports, because the last band read must stand alone — nobody should have to scroll up to trust a green column. A check whose evidence is already on screen earns no line, which is why the version in the commit message is not checked: it is printed directly above.

Three of the eight lines reach past the commit's contents. METHOD FILES UNTOUCHED notifies, and only notifies, if a method file changed this session — the method is a guideline and the developer keeps full rights (§12); a deliberate local hotfix is theirs to make, and the upgrade's drift check later carries it home rather than losing it. METHOD FRICTION is §11's self-honing question, and it can only be asked here, because the close-out itself is part of what might have chafed. OUTSIDE THE PROJECT is the third, and the only line that looks beyond the project's tree at all.

OUTSIDE THE PROJECT, in full. Every other line is scoped to git, and git reports the project's own folders and nothing else — so a session that edited a file somewhere else, or installed something machine-wide, passes every other check in silence. Note the asymmetry with METHOD FILES UNTOUCHED, the one older line that reaches into ownership rather than contents: that one works because method files are TRACKED, so git sees them. An out-of-project write is exactly the case git cannot see.

WHAT IT REPORTS: the changes THIS SESSION MADE — files created, edited, deleted or moved; programs or packages installed or uninstalled. Not reads: reading a file outside the project is how the AI answers questions, and a line that fires constantly stops being read. Not the platform's own writing either — a platform keeps transcripts, caches, session records and config backups whether the session acts or not, and that bookkeeping is the application's, not the session's. A check that goes red every time for something nobody can act on is worse than no check.

IN BOUNDS: the project's own folder — or FOLDERS, since a session may have more than one directory in scope — plus this session's own scratch area, plus the platform's own configuration folder, plus anything the project lists under ALLOWED WRITE LOCATIONS in AI_PM_Config.md (§3). The platform's configuration folder is in bounds because it is the assistant's own house: it is where the tool legitimately keeps its working state, and a developer who has installed the tool has accepted that. The carve-out below is what keeps that from being a blank cheque. Everything else is out, and no list at all means those three places alone; a project that routinely writes somewhere else lists it once and stops seeing it. Two boundaries are worth stating because a shared path root makes them easy to miss: ANOTHER project's scratch area is outside, and so is another SESSION's. A session reaching into a sibling project's working files is exactly what this line exists to surface.

IN BOUNDS BUT IN THE WRONG PLACE counts too, and it is the case a project is likeliest to hit. A method-owned path with a designated folder has one correct destination: an outbound report belongs in Feedback\Outbox\, a staged release package in Upgrade\. A report written straight into Documents\AI PM\ instead is inside the project, so git sees it and no other line objects — but it is in a place the method does not keep files, and it will be missed by everyone looking for it where it belongs. The line names it.

ONE CLASS ALWAYS FLAGS, wherever it lives and whatever the allowed list says: a change to what the AI is PERMITTED TO DO in future sessions. The platform's permission settings, its automatic-hook configuration, its tool-server configuration, and any persistent cross-session memory it keeps all belong to this class. They differ in kind from a working file — they alter what a later session may do without asking, across every project on the machine, and a developer who never sees the change cannot un-grant it. An allowed location silences ordinary writes; it never silences these.

THE BAND CARRIES A COUNT, not a list — grouped by location, with the list available on request, because a dozen scratch files would otherwise bury the report they are part of. ✅ when nothing changed outside the project; ✅ WITH THE COUNT when everything that changed was in bounds, since in bounds is not the same as invisible and a developer learning what their AI touches is entitled to the number; ❌ when something landed outside. Like METHOD FILES UNTOUCHED it NOTIFIES rather than blocks: an out-of-project change the developer approved is theirs to make, and the whole point is that it is STATED rather than passing unseen.

WHAT THIS CHECK IS WORTH, stated plainly rather than oversold. It rests on the AI's own record of what it did, which makes it a notification and not an attestation (§5, "a check only the AI can attest is not a check"). Two limits ride in the open: it is self-reported, and in a long session whose early context was summarized the honest answer is "these are the changes I can account for; earlier detail was compacted" — never a clean bill the AI cannot back. The durable version records paths mechanically as the session runs, and waits on machinery the method does not yet ship. Partial coverage that asks the question at every close-out still beats no coverage while that is designed.

A ❌ in Stage 4 is the awkward one: it lands after the commit, so it means the close-out did not fully complete. The AI says what is missing and proposes a follow-up commit — reporting an incomplete close-out honestly rather than amending history to hide it.

The session name comes before what's next and stays on its own line, because it is the one thing the developer acts on immediately: session names are how they scan and review past sessions in the app, a distinct job the commit message does not do.

ACROSS EVERY STAGE, a ❌ NEVER CARRIES THE DETAIL. It points, and the detail lives in that stage's normal content — a sweep finding becomes a Stage 1 item, a retirement becomes a Stage 2 delete entry, a consolidation becomes a bullet on the backlog file. Nothing is ever decided outside the plan the developer is approving.

DRY RUNS. A close-out may be rehearsed — walked through to see how it reads, to design or test the templates, or to teach the procedure. A dry run WRITES NOTHING, COMMITS NOTHING, and DECIDES NOTHING: an answer given inside a rehearsal is not a ruling, and the AI must not carry one out into the project. It is announced as a dry run when it starts and never entered from a close-out command. If a rehearsal surfaces a real decision, it is raised separately, outside the rehearsal, and settled there.

Human-facing formatting (§4) applies to every stage; everything else stays plain.

VERSIONING: the project carries a Major.Minor version. Minor bumps at every close-out, automatically; Major when the developer declares a milestone. Per-doc versions are INDEPENDENT of the project version — each doc bumps when it changes, at its own pace, and nothing ties the two numbers together. The changelog groups entries under project-version headers and the close-out commit message leads with the version, so any change traces to its commit and back. A new project starts at 0.1 or negotiates an honest number at adoption.

ONE NUMBER PER THING THAT ACTUALLY MOVES. Every version number a project carries costs someone the work of holding it, and a number that duplicates another is pure liability — the stale copy is the one that gets read. So: the METHOD's shipped docs travel as a single package and carry the AI PM VERSION ALONE, none of them numbered separately, because they never ship apart and a per-file number would only need syncing. A doc that merely RESTATES the project version carries no stamp at all — that number's home is the commit message and the changelog. RECORD DOCS keep their own versions, which earn their keep: they track one doc's own revisions and the amendment chain (§4) depends on them. And A CLOSE-OUT NEVER RE-ASKS a release number a prior session already declared; it states the number and moves on. A number re-asked in good faith comes back different, and the conflict that follows was never real.

8. THE COMMAND SET

Slash commands are the canonical form; recognized-by-meaning stays the fallback — plain language always works, no operators needed. TWO EXCEPTIONS, both stated for the same reason: the action is irreversible enough that the trigger should be mechanical rather than interpretive. The three CLOSE-OUT commands are slash-only (§7): plain language never starts a close-out, and the AI answers a plain-language request by naming the commands. /ai-pm-upgrade is slash-only for the same reason (§12): detection informs, but only the command executes, so an upgrade can never begin mid-conversation. Command files are TRIGGERS, not procedure copies: each is a few lines pointing at this document (or the Feedback Protocol) and naming the mode, so the procedure lives in one place and command files survive method upgrades unchanged. Full table in the README.

Developer-issued: /closeout (= full) · /closeout-light · /closeout-auto — slash-only · /ai-pm-upgrade (§12) — slash-only · /feedback-pass · /self-check (both §13) · consolidate [topic] (plan a consolidation session and queue it) · help [topic] (summarize AI PM, or go deep on one part, from this doc) · define [term]. AI-initiated recommendations (developer vetoes; never auto-run): the consolidation recommendation · the retirement flag (§9) · the new-term flag (§4). (Distinct from these: the Feedback Pass runs on its own after a close-out by default — it is not a "never auto-run" recommendation; see §13 and the Feedback Protocol.)

PERMISSION MODES (platform note): Claude Code's permission modes (Manual / Accept edits / Plan / Auto / Bypass) govern tool approval; AI PM's gates are conversation-level. They are independent layers that compose. Plan mode + /closeout is the strictest-safe combination: stages 1–2 are read-and-discuss and run fine; at stage 3 the AI requests plan-mode exit, and that approval dialog doubles as the gate. Accept edits is the recommended everyday default; Manual works but prompts per write; Auto pairs naturally with /closeout-auto; Bypass is never needed. Name collision, stated once: the method's AUTO close-out (removes AI PM's interactive gates) is not Claude Code's Auto mode (removes tool prompts) — either can run under the other.

9. RETIREMENT AND ARCHIVING

Git is the archive. Routine edits keep no copies — every committed version is already recoverable. A document LEAVING the project entirely — consolidation folds, deliberate decommissioning — is deleted in the close-out commit, and that deletion IS the archival: the file stays recoverable from history forever. The AI performs the retirement itself and verifies it in the same session; the changelog entry records what was retired and where its content went. Rider (hard rule 3): untracked and ignored files are NOT protected by git — before deleting one, make an explicit copy somewhere deliberate or get an explicit "genuinely disposable" ruling. The retirement flag surfaces at stage 2 (§7) when a candidate is spotted; there is no next-session tail-check — verification happens on the spot.

10. FAILURE MODES THIS PREVENTS

* The wall: a close-out report so dense the developer skips reading it — so errors ship unreviewed. (The short fixed-order report; worth-your-eyes first.)
* The late question: a question surfacing after docs are written, forcing post-hoc rewrites. (Pre-flight before planning.)
* The stale rewrite: rewriting a document from a partial or out-of-view copy. (Read the current file in full before rewriting; re-read when in doubt.)
* The unverifiable claim: the AI attesting to something only it can check. (Human-verifiable checks — the version stamp, the version-led commit, the clean tree.)
* The manufactured update: inventing doc changes at close-out when nothing durable changed. (Light mode; "don't manufacture updates.")
* The uncommitted session: work left uncommitted, a handoff described but never rewritten, a commit message missing the version, a backlog or changelog silently skipped — the session's memory evaporates. (Hard rule 1; the close-out self-check, §13.)
* The deep chain: truth about one topic spread across so many docs that assembly silently drops a layer. (Consolidation trigger.)
* The coined term: process vocabulary drifting until the developer can't follow it. (No-coining rule; define command; the glossary.)
* The growing backlog: completed history inflating the one file rewritten every session. (The changelog absorbs history; the backlog stays forward-looking.)
* The forked method: an adopting project patching its own copy of AI PM until it silently runs a different method than everyone else. (§11 detector-only in adopting projects; the Feedback Protocol carries changes home instead.)

11. THE SELF-HONING LOOP

At every full close-out the AI asks itself: did any part of this procedure fight the session? A yes becomes a candidate improvement — noted in the report, queued if the developer agrees, folded into this doc at a later close-out with a version bump and an internal-changelog line. This document is expected to change; that is how it was built.

One boundary: the fold-it-in power belongs to the MAINTAINING project only — the project that owns this doc's source. In an adopting project, §11 is DETECTOR-ONLY: it notices friction and routes it outbound via the Feedback Pass (§13), and never edits the local copy of this doc. One source of truth; no divergent local edits. (See the Feedback Protocol for how a friction report is produced and carried home.)

12. ADOPTING IN A NEW PROJECT

The full interactive procedure lives in AI_Project_Manager_Adoption_Guide.md, which arrives with the release package and removes itself before the project's first commit. This section is the checklist it expands on.

1. Obtain the current release package from the maintaining project: the method docs under Documents\AI PM\ (this file, the Feedback Protocol, the README, AI_PM_Instructions.md), the AI_PM_Config.md template under Documents\AI PM\Config\, the Adoption Guide at the package root, and the command files pre-placed in .claude\commands\. The package ships NO CLAUDE.md — an existing project's own file can never be overwritten by the unzip.
2. Create the project folder (or pick the existing one) and unzip the package's contents directly into it — the method's folder tree lands ready-made.
3. Open Claude Code in the folder — the project folder itself, not its parent — and type "start the AI PM adoption" (any phrasing naming AI PM or the guide); the AI finds the guide by name at the folder root and follows it. The first reply should name the guide; if it doesn't, the session opened in the wrong folder.
4. The adoption session, AI-performed from a short interview: git init (verifying git identity beside it); the .gitignore lines installed for the transit subfolders (Documents\AI PM\Feedback\ and \Upgrade\), with the reason stated — reports and packages are correspondence in transit, not project memory; CLAUDE.md created or amended with the fenced pointer block — the developer's existing content is never overwritten; AI_PM_Config.md filled in (starting version, Feedback Pass, record docs, storage note); the backlog and changelog drafted at their standard paths (a from-zero changelog starts as one header line + "history starts at v[X]"; a project adopting mid-life births its changelog with the adoption entry and can backfill pre-adoption history later at milestone granularity).
5. The guide file deleted, then the first commit, version-led ("0.1 — adoption", or the negotiated honest number).
6. Point new people at the README.

UPGRADES: a method upgrade replaces the method FILES in Documents\AI PM\ — never the folder's SUBFOLDERS. Config\ survives untouched (it is the project's own state), and so do the transit folders Feedback\ and Upgrade\. That files-versus-subfolders line is the same boundary §3 draws for ownership.

THE UPGRADE IS NOT A CLOSE-OUT. It has its own short flow and its own commit species:

1. STAGING. The maintainer delivers a release package by hand into Documents\AI PM\Upgrade\ — a standard location, git-ignored, so the developer always knows where upgrades go.
2. DETECTION. Session start checks that folder (§6). Empty: the AI says nothing. A NEWER package: the alignment reply carries the offer and ALWAYS names the exact command — "AI PM X.Y is staged — type /ai-pm-upgrade to run it, or ignore to defer." Deferring costs nothing and the offer simply repeats next session. A SAME-OR-OLDER package is flagged as stale rather than offered.
3. THE COMMAND. Only /ai-pm-upgrade executes. Detection informs; it never acts. So an upgrade can never start mid-conversation — a session either begins as the upgrade session or defers.
4. THE FLOW. State the plan (which files are replaced, what is preserved) → the developer's go → drift check → replace the method files → verify → commit → annotate the handoff.
5. DRIFT CHECK, before overwriting anything. Method files are tracked, so the AI can see whether any was locally edited — git history since the last install or upgrade, plus the current diff. Any drift is captured as a feedback report into the project's own outbox FIRST, then overwritten. A local hotfix the developer approved therefore travels home to the maintainer instead of dying in the overwrite. Strictly in-folder: the check reads the project's own git, the report lands in the project's own outbox, and the human transports it, as with every report.
6. VERSIONING. An upgrade changes ONLY the AI PM version the project runs on; the project's own version does not move. Its commit is the one non-close-out commit species (§1 carve-out): message form "AI PM upgrade X.Y → X.Z", containing method files and nothing else.
7. THE HANDOFF IS PRESERVED. The upgrade adds one annotation line — "upgraded AI PM X.Y → X.Z on DATE; opener below unchanged" — and leaves the opener alone, so the next working session aligns exactly as it would have.
8. VERIFY, then CLEAN UP. Version stamps consistent, config and subfolders intact, git diff showing only method files. Then the staged package is deleted and the summary says so, so the next session start does not re-offer it.

SCOPE LIMIT, for now: the package must be exactly one version ahead. Anything else is flagged for the maintainer rather than attempted; version-skipping and cumulative migration notes wait until real multi-version adopters exist.

METHOD FILES ARE NOT EDITED LOCALLY. The AI never edits a method file in an adopting project and never proposes doing so; the route to change the method is a feedback report (§13). This is a GUIDELINE, not a lock — AI PM ships no enforcement, and the developer keeps full rights over every file in their own project. If they override and hotfix locally, nothing blocks them, and the drift check above is what makes sure the change is not lost.

13. THE FEEDBACK LOOP

Projects that run AI PM report observations — defects, needed spec changes, friction — back to the maintaining project, so the method improves without any project forking its own local copy. The full procedure lives in AI_Project_Manager_Feedback_Protocol.md — a permanent resident, carried as a project file in every project the way this doc is.

It defines: three report channels (bug/defect, change request, friction/improvement); the Feedback Pass that produces report files after a close-out completes (auto-triggered, default on; toggled in AI_PM_Config.md); the close-out self-check the Pass is built on — verifying that every close-out stage actually HAPPENED rather than was described: everything approved landed, the tree committed, the handoff really rewritten, backlog and changelog updated, version stamps matching the commit, method files untouched, and §11's friction question asked; and the intake procedure the maintaining project uses to turn reports into approved backlog items. Adopters with direct access to the maintaining project drop report files into its feedback folder; remote adopters write them locally and send them to the maintainer by any channel — same protocol, one human transport hop. §11's self-honing output is the friction channel's source; /feedback-pass and /self-check trigger the machinery by hand when needed.
