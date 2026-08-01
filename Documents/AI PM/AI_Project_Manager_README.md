# AI Project Manager

**AI PM gives an AI-assisted project a memory you can trust.**

An AI has no memory between sessions. AI PM's answer is to make the project's files *be* that memory — a living backlog for what's next, a changelog for what's done, permanent record documents for decisions and designs, and a disciplined close-out that moves knowledge out of the conversation and into the record. Four ideas hold it together:

- **The files are the AI's memory.** They're written AI-first: complete enough that the next session's AI rebuilds full working context from the files alone. You don't have to read them to get up to speed — you ask the AI, which is faster and always current.
- **Memory is written after the conversation, not during it.** A topic gets worked all the way through, every decision made, before anything is committed to memory — because a follow-up can reopen anything half-written. What the session *produces* — mockups, code, drafts — lands as you go, with your approval. Everything else waits for close-out. The test: if we keep talking, could this edit change? Then it waits.
- **Every memory change passes your review.** The close-out runs in clear, recognizable stages so nothing important is lost, nothing unwanted lands, and nothing is removed by accident — with git as proof anyone can check afterward.
- **One method, many projects.** It's portable and adoptable as-is, upgraded from a single source of truth, and improved by feedback that flows home to whoever maintains it.

The result: any new session — or any new person — can pick the project up from the files alone, whether it was set down yesterday or a year ago.

The method runs on Claude Code, where a project is simply a folder: a directory carrying a CLAUDE.md file and a git repository. The full procedure lives in AI_Project_Manager.md — AI PM for short — shipped beside this page in the release package. This page is the human-facing summary and glossary; if the two ever disagree, AI_Project_Manager.md wins.

## The Files and Their Roles

Every project that runs the method carries the same files at the same paths — the roles below are standard, not per-project choices. The organizing idea is ownership: you own CLAUDE.md, the method owns the `Documents\AI PM\` folder, and the AI maintains the cadence files in `Documents\`:

- **CLAUDE.md** — *your* file, at the project root: your project description, conventions, and instructions, auto-loaded into every session. The method's only footprint in it is a short fenced pointer block that routes each session into the method — keep that block intact, and the rest of the file is entirely yours.
- **AI PM instructions** — `Documents\AI PM\AI_PM_Instructions.md`: the method's front door, read in full by the AI at every session start. Pure method text — identical in every project running this version, edited by nobody, and replaced whole by an upgrade.
- **Config** — `Documents\AI PM\Config\AI_PM_Config.md`: the handful of facts specific to *your* project — record-docs list, toggles, version at adoption, storage notes, and the locations outside your project folder that the AI may write to without it being reported. The AI maintains it at close-out, and no upgrade ever touches it. It exists so project state and method text never share a file.
- **Backlog** — `Documents\Status_and_Backlog.md`: current status plus what's next. The only file that churns by design, and it stays short forever: completed work leaves it.
- **Changelog** — `Documents\Changelog.md`: what happened, when, and where the detail lives. Append-only, newest on top; the AI prepends entries directly at close-out. An entry locates the record; it never is the record.
- **Record docs** — the permanent record: designs, decisions, execution. Each is authoritative for a stated scope.
- **Handoff** — `Documents\Handoff.md`: the short note that opens the next session, leading with the current project version — and only that one, never the AI PM version, which lives in exactly one place (the instructions file's stamp) so it can't go stale in two. A tracked file, overwritten at every close-out — its git history is a free session-by-session trail.
- **AI Project Manager (AI PM)** — AI_Project_Manager.md itself, in `Documents\AI PM\`, present in every project that runs the method.

Two optional roles, adopted only when their trigger fires:

- **External storage map** — a curated map of the project's off-workspace storage (folder purposes and routing rules, never a file manifest). Adopt when routing questions keep coming up or the storage tree outgrows a section in CLAUDE.md.
- **Logs** — files that only ever grow by appending dated entries, prepended by the AI like the changelog. Example: a correspondence log tracking questions sent to an outside agency and the answers as they arrive.

The method's files travel together in `Documents\AI PM\`: AI_Project_Manager.md, the AI PM instructions, AI_Project_Manager_Feedback_Protocol.md (the procedure for reporting observations back to the project that maintains the method — see The Feedback Loop), and this README.

There's one line to remember about that folder: **the files in it belong to the method, and the subfolders belong to your project.** An upgrade replaces the files and never touches a subfolder. `Config\` holds your project's own settings and is tracked in git; `Feedback\` and `Upgrade\` are transit — reports on their way out, release packages on their way in — and are git-ignored, because mail passing through isn't project memory.

You keep full rights over every file in your own project. The AI won't edit the method's files or suggest you do, because the way to change the method is a feedback report rather than a local patch that would quietly fork it. But nothing stops you: if you hotfix something, the next upgrade notices and carries your change home to the maintainer instead of losing it.

## How to Run a Session

### Starting a session

1. Open Claude Code in the project's folder and type your task — that's all.
2. CLAUDE.md (auto-loaded) routes the AI to the AI PM instructions, read in full: the AI reads your config and the handoff, runs `git status` (uncommitted changes get a one-line "was this you?" check), checks whether a method upgrade is waiting, and opens with the alignment line — "Working off AI PM 3.2; project at v2.4 per the handoff." You know both numbers, so a stale read is caught at a glance. Each number is read from the one file that owns it: the method version from the instructions, your project's from the handoff.
3. The AI confirms where things stand and what the next move is — **align before building** — and only then starts work.

No useful handoff (first session, or one gone missing)? The AI aligns from the backlog.

### Ending a session

Type `/closeout`. This is the one place where plain language *doesn't* work: asking to "wrap up" gets you the list of commands rather than a close-out. It's deliberate — the trigger is mechanical, so a close-out never starts by accident, and the *kind* of close-out is always something you chose. The AI can suggest one in a single sentence, but suggesting is all it may do.

The full close-out runs in four stages, and every one of them looks the same every time, so you can scan it instead of reading it:

1. **Pre-flight.** Everything that needs your answer or action, numbered, before any document is planned — with the AI's suggested answer under each item where it has one. Answer by number, defer anything (deferred items ride the handoff and are never lost), or say **proceed**. Answering an item with a question of your own is normal and doesn't restart anything — the AI answers, you rule, and the close-out carries on. A sweep of `git status` and the session's touched files runs quietly behind it, and anything that changed outside your conversation shows up as an item.
2. **Proposed changes.** The document plan, grouped New → Edit → Delete, each file's changes beneath it. This stage asks no questions — those all belonged to pre-flight — so your reply is usually just **approve**. Veto or adjust anything by number; a disagreement is talked through and answered with a fresh plan. Nothing is written while you're deciding.
3. **Execute + self-review.** The AI writes the files, walks the full git diff against the plan you approved, and commits.
4. **Report.** The commit, a column of checks you can verify yourself, the session name to save the conversation under, and where the next session picks up.

Stages 3 and 4 arrive together without stopping to ask you again — your approval in stage 2 was the real decision, and a second gate asking "did I do what you approved?" would just teach you to click past it. The one exception: if writing the documents turns up something that genuinely needs *your* judgment rather than just execution, the AI stops before committing and asks.

Three rules in this pipeline are hard and never bend: every close-out ends in that single version-led commit; the diff is self-reviewed against the approved plan before committing — anything unplanned is reverted or flagged, never silently kept; and nothing leaves the project outside a close-out commit — git keeps every committed version recoverable forever, so deletion in a commit *is* archival (only untracked/ignored files need a copy or an explicit "disposable" ruling first).

Two lighter variants:

- **Light close-out** (`/closeout-light`) — backlog + changelog + handoff + commit, approval collapsed into one message. For sessions where no record docs changed.
- **Auto close-out** (`/closeout-auto`) — full scope with the interactive gates removed: the AI makes the judgment calls, flags them at the top of the report and in the commit message, and never guesses unanswered questions into documents. For walk-away endings; you review after instead of steering before.

Both variants still need their command typed; **auto** removes the gates *inside* the close-out, never the one at the door.

A close-out can also be **rehearsed** — walked through to see how it reads, or to show someone how it works. A dry run writes nothing, commits nothing, and decides nothing: an answer you give inside a rehearsal isn't a ruling, and the AI won't carry it out into the project.

**A note on permission modes.** Claude Code's permission modes (Manual / Accept edits / Plan / Auto / Bypass) control tool approval; AI PM's gates are conversation-level. They're independent and compose: *Accept edits* is the recommended everyday default, and *Plan mode + /closeout* is the strictest way to run a close-out (the plan-approval dialog doubles as the gate). Watch the name collision: the method's **auto close-out** removes AI PM's gates; Claude Code's **Auto mode** removes tool prompts. They're different things, and either can run under the other.

## The Commands

Slash commands are the canonical form, and plain language works for all but four of them — commands are otherwise recognized by meaning. The exceptions are marked **slash-only** below: the three close-out commands and the upgrade. Those four are the ones that write, commit, or replace files, so their trigger is deliberate rather than interpreted. AI_Project_Manager.md (§8) defines the command set; this table is the expanded reference it points to, and if the two ever drift apart, AI_Project_Manager.md wins.

| Command | What it does | When to use it |
|---|---|---|
| `/closeout` **slash-only** | Runs the full four-stage close-out: pre-flight → proposed changes → execute + self-review → report and the version-led commit. | The end of any working session — the default. |
| `/closeout-light` **slash-only** | Backlog + changelog + handoff + commit; plan approval collapsed into one message. | Sessions where no record docs changed — don't manufacture updates. |
| `/closeout-auto` **slash-only** | Full scope with the interactive gates removed: no pre-flight pause, no plan approval. Judgment calls are flagged at the top of the report and in the commit message; unanswered questions ride the handoff. | Walk-away endings; you review after instead of steering before. |
| `/ai-pm-upgrade` **slash-only** | Installs a method upgrade that's been staged in `Documents\AI PM\Upgrade\`: states the plan, checks for local edits, replaces the method files, verifies, commits, and clears the staging folder. | When a session start tells you an upgrade is waiting — see Upgrading the Method. |
| `/feedback-pass` | Runs the outbound feedback engine by hand: the close-out self-check plus friction-gathering, ending in a short notice with three options (write it now / talk it through first / skip). | When auto-reporting is off, or on but didn't fire and you want a report anyway — or to file a report not tied to a close-out. |
| `/self-check` | Runs just the audit that a close-out actually completed — everything approved landed, work committed, handoff ready, backlog and changelog updated, version stamps matching, method files untouched, nothing changed outside your project — with no report necessarily following. | Any time you want to confirm a close-out didn't leave the session uncommitted. |
| `consolidate [topic]` | Plans a consolidation session — folding a topic's spread-out amendment layers into one fresh base document — and queues it. Always its own session, never done inside a close-out. | When one topic's truth spans too many documents (the trigger is ~4–5), or when the AI has recommended it. |
| `help [topic]` | The AI summarizes AI PM, or goes deep on one part of it, from AI_Project_Manager.md. | Whenever you've forgotten how a piece of the method works. |
| `define [term]` | The AI defines a method or project term crisply — and if it can't, the term shouldn't exist. | When a word in the docs or conversation isn't landing. |

The AI also raises three things on its own initiative — as recommendations you can veto, never auto-run:

- **The consolidation recommendation** — a topic's document chain has grown deep enough to risk a silently dropped ruling; the AI recommends a consolidation session.
- **The retirement flag** — a document looks ready to leave the project; the AI proposes retiring it (deletion in the close-out commit, which git keeps recoverable forever) and verifies it on the spot.
- **The new-term flag** — a new concept needs a name, so the AI names it explicitly ("I'm calling this X, meaning Y") rather than letting vocabulary drift in by casual use.

Separately from those vetoable recommendations, the Feedback Pass runs automatically after each close-out by default — a standing behavior, not a recommendation. See The Feedback Loop.

## The Feedback Loop

A project running AI PM produces observations — a defect in a close-out, a spec change that's needed, a step that keeps fighting the work. Those observations travel back to the project that maintains AI PM through the Feedback Protocol (AI_Project_Manager_Feedback_Protocol.md), a permanent resident carried in every project the way AI_Project_Manager.md is. This keeps one source of truth: an adopting project observes and reports; the maintaining project decides and edits the method's source. No project ever patches its own local copy — that would fork the method into as many versions as there are projects.

**Three report channels:** bug/defect (a near-miss is the same channel at a lower severity), change request (a proposed change to the spec text — a feature request folds in here), and friction/improvement (the outbound route for the self-honing loop's output).

**How reports get produced:** the Feedback Pass runs automatically after a close-out completes (default on), sees the finished close-out, and — if anything's worth reporting — gives a short notice with three options: write it now, talk it through first, or skip. It's built on the close-out self-check, an audit that confirms every close-out stage actually happened rather than was merely described: everything approved landed, the work committed, the handoff really rewritten, backlog and changelog updated, version stamps matching, and nothing changed outside your project. To turn auto-reporting off for a project, ask the AI — it sets `Feedback Pass: off` in your config file.

**How reports come home:** reports are files. Adopters with access to the maintaining project drop them into its `Feedback\` folder; remote adopters write them to a local outbox and send them to the maintainer by any channel. Intake is its own session in the maintaining project — the AI reads the folder directly, walks the reports one at a time, version-checks each first (a bug against an old version may already be fixed), turns survivors into backlog items only with the developer's approval, and moves processed files to `Feedback\Archived\`. The maintaining project runs the Pass on its own sessions too; its reports just come home to itself.

Full detail — the templates, the group-by-kind rule, the folder scheme — lives in the Feedback Protocol itself.

## Upgrading the Method

AI PM improves over time, and a project moves to a newer version on its own schedule. The whole process is one command, and you can ignore it indefinitely.

**How an upgrade reaches you.** Whoever maintains the method hands you a release package, and you drop it into `Documents\AI PM\Upgrade\`. That folder is the standard place upgrades go, so you never have to remember where to put one.

**How you find out.** At the next session start, the AI checks that folder. If a newer package is waiting, the alignment line says so and names the command: *"AI PM 3.2 is staged — type `/ai-pm-upgrade` to run it, or ignore to defer."* If the folder's empty, it says nothing at all. Deferring costs you nothing; the offer simply comes back next session. An upgrade can never start mid-conversation — a session either begins as the upgrade session or it doesn't.

**What happens when you run it.** The AI states the plan, waits for your go, checks whether any method file was edited locally, replaces the method files, verifies the result, commits, and deletes the staged package so you're not offered it again.

Three things worth knowing:

- **Your project keeps its own version number.** An upgrade changes only which version of AI PM you're running. It's also the one commit in the whole method that isn't a close-out commit — you'll see it as *"AI PM upgrade 3.1 → 3.2"*.
- **Your handoff survives.** The upgrade adds one line noting what changed, and leaves the opener alone, so your next working session picks up exactly where it would have.
- **Local edits aren't silently overwritten.** If you'd patched a method file yourself, the AI notices before replacing anything and writes your change up as a report for the maintainer — so a fix you made on purpose travels home rather than disappearing.

## Glossary

One boundary rule: this glossary defines the method's words. Each project's domain jargon lives in that project's own docs.

### System vocabulary

- **Adoption session** — the procedure that gets AI PM running for a project, new or existing. Required Adoption sets up the folder (git, CLAUDE.md, commands) and drafts the starter files (backlog, changelog, the first handoff) from a short interview, ending in the project's first commit. Full Project Interview — planned, not yet built — would also draft the project's own domain, naming, and technical conventions. Run via AI_Project_Manager_Adoption_Guide.md, which arrives with the unzipped package and removes itself before the first commit.
- **AI Project Manager (AI PM)** — AI_Project_Manager.md itself: the procedure, carried as a file in every project that runs the method. Read in full by the AI at every close-out.
- **Align-before-building** — the session-start rule: the AI confirms where things stand and what the next move is before doing any work. A cold session can lose the thread; alignment catches it in the first exchange.
- **Amendment chain** — how a later document changes truth an earlier one states. History is never rewritten to match new reality: the newer doc declares authority over the conflict, and the record-docs list in your config file notes who amends whom. A doc's date and place in the chain tell a stranger exactly how much to believe it.
- **Attestation** — the self-review claim in the close-out report — "the diff contains only the planned changes" — made checkable by the commit: the session's single commit holds exactly the diff that was reviewed, so the claim can be re-verified with git at any time.
- **Backlog** — the living file holding current status and what's next. The only file that churns by design, and it stays short forever: completed work leaves it for the changelog and the record docs.
- **Changelog** — the append-only, newest-on-top history: what happened, when, and where the detail lives. An entry locates the record — date, what, which doc or commit holds the detail — it never is the record. The AI prepends entries directly at close-out; git log is complete but flat, and the changelog is the curated narrative over it.
- **CLAUDE.md** — the file Claude Code auto-loads from the project folder at every session start. In AI PM it is the developer's own file — project description, conventions, domain instructions — carrying one short fenced pointer block that routes the session to the AI PM instructions. Keeping the block intact is the developer's one obligation; the rest of the file is theirs.
- **Close-out** — the end-of-session procedure that moves knowledge from conversation into the record, ending in the session's single version-led commit. Full is the default four-stage pipeline (pre-flight → proposed changes → execute + self-review → report). Light is backlog + changelog + handoff + commit, for sessions where no record docs changed. Auto is full scope with the interactive gates removed, for walk-away endings.
- **Close-out self-check** — the audit inside the Feedback Pass (and runnable on its own via `/self-check`) that verifies every close-out stage actually happened rather than was described: work committed and the tree clean, the commit message version-led, the handoff really rewritten, backlog and changelog updated, the self-review reported, method files untouched, and nothing changed outside your project. Eight lines, shown as a column you can run your eye down. Any failed check becomes a proposed bug report. It exists to catch the close-out's worst failure — the uncommitted session.
- **Commit** — git's saved snapshot of the project: the whole tree at one moment, with a message, permanent in history. In AI PM every close-out ends in exactly one, its message leading with the new project version — the commit is the session's receipt, archive, and audit trail in a single mechanical act.
- **Consolidation** — a cleanup session that folds a topic's spread-out layers (a design plus every doc that later amended it) into one fresh base document, verified by a coverage audit, with the folded layers retired. Triggered when a topic's chain crosses ~4–5 documents. Always its own session, never done inside a close-out.
- **Coverage audit** — the verification step of a consolidation: walk each amendment and confirm its ruling appears in the new base document. A diff can't do this job — it's a ruling-by-ruling check, not a text comparison.
- **Feedback Pass** — the procedure that produces feedback reports. Runs automatically after a close-out completes (default on; toggled off with `Feedback Pass: off` in the config file), sees the finished close-out, and — if a report is warranted — gives a short notice with three options: write it now, talk it through first, or skip. Built on the close-out self-check; also runnable by hand with `/feedback-pass`.
- **Feedback Protocol** — AI_Project_Manager_Feedback_Protocol.md: the procedure by which a project running AI PM reports observations back to the project that maintains the method, and by which the maintaining project takes them in. A permanent resident carried in every project like AI_Project_Manager.md. Defines three report channels, the Feedback Pass, the close-out self-check, the intake walk, and the feedback folder scheme.
- **Field Report** — the file that batches a session's observations — bugs, near-misses, and friction — under one shared envelope, one body per item. One of the two report shapes the Feedback Protocol produces; the other is the change request, which gets its own file per proposed spec change. ("Report" is the umbrella term for both.)
- **Gate** — a point in a procedure where the AI stops and waits for the developer's answer or approval before proceeding. The full close-out has two (pre-flight and proposed changes); auto close-out removes the gates; light collapses them into one message. General beyond close-out — the Adoption Guide's scope-selection question is a gate too.
- **Handoff** — the short note that opens the next session: where things stand, the task, anything carried forward, leading with the current project version (the alignment line quotes it). A tracked file, overwritten at every close-out; transient in content, permanent as a file.
- **Instructions (AI PM instructions)** — Documents\AI PM\AI_PM_Instructions.md: the method-owned front door, read in full by the AI at every session start. Carries the session-start procedure and the standing rules under the AI PM version stamp. Pure method text — byte-identical in every project running this version, edited by nobody, and replaced whole by an upgrade. Anything project-specific lives in the config file instead.
- **Config** — Documents\AI PM\Config\AI_PM_Config.md: your project's own settings — record-docs list, the Feedback Pass toggle, version at adoption, storage notes. AI-maintained at close-out, tracked in git, and never touched by an upgrade.
- **Transit folders** — Documents\AI PM\Feedback\ and \Upgrade\: reports on their way out to the maintainer, and release packages on their way in. Git-ignored, because correspondence in flight isn't project memory.
- **Maintaining project** — the project that owns the method's source and has sole authority to change it; every other project is an adopting project, and its route to change the method is a report, never a local edit.
- **Managed doc** — any project file whose edits flow through close-out: the config file, the backlog, the changelog, and the record docs. Their changes land in the close-out commit; anything changed outside one surfaces mechanically in the next session's `git status` check.
- **Pre-flight** — stage one of a full close-out: before planning any documents, the AI surfaces every item needing your answer or action — loose ends, unresolved forks, physical to-dos — from conversational recall plus a mechanical sweep, and asks whether anything else should land somewhere. Documents are written once, after the answers.
- **Outside-project check** — the close-out self-check line reporting what the session changed beyond your project: files created, edited, deleted or moved outside its folders, and anything installed or uninstalled machine-wide. It reports a count grouped by location, with the full list on request, and it notifies rather than blocks — a change you approved is yours to make, and the point is only that you're told. Already in bounds without being listed: your project's folders, the session's own scratch area, and the platform's configuration folder; anything else you write to routinely goes in your config's allowed write locations. One thing always flags regardless — a change to what the AI is permitted to do in future sessions (permission settings, automatic hooks, tool-server configuration, cross-session memory), because that alters what a later session may do without asking you.
- **Project version** — the Major.Minor number the project itself carries. Minor bumps at every close-out automatically; Major when you declare a milestone. Close-out commit messages lead with it, so any change traces to its commit and back. Nothing ties it to any document's version, and a close-out never re-asks a release number a previous session already declared.
- **Proposed changes** — stage two of a full close-out: the document plan, one file per line, disposition leading (edit / new / delete), with each file's substantive changes beneath it. You approve, veto, or adjust before anything is written.
- **Release package** — the folder the maintaining project hands to an adopter (Releases\vX\, typically as a zip): the method docs under Documents\AI PM\ (AI PM itself, the Feedback Protocol, this README, the AI PM instructions), the config template under Documents\AI PM\Config\, the Adoption Guide at the root, and the six command files pre-placed in .claude\commands\. Unzipped directly into the project's folder — the method's tree lands ready-made. It deliberately ships no CLAUDE.md (an existing project's own file is never at risk from the unzip); no shared storage, no absolute paths.
- **Repo (repository)** — the git database living inside the project folder (.git\) that stores every committed snapshot. It is the project's archive: any committed version of any file is recoverable from it forever.
- **Record doc** — a permanent-record document: a design, a decision set, an execution record. Each is authoritative for a stated scope — if you can't say in one sentence what a doc is authoritative for, it shouldn't be a separate doc.
- **Report (close-out)** — stage four of a full close-out: a short, fixed-order summary — what changed per file, the self-review result, test results, your tasks if any, handoff confirmation, a suggested version-led session name — followed by the commit. Fixed order so nothing important hides in a wall of text.
- **Self-review** — the pre-commit discipline of hard rule 2: the session's git diff walked hunk by hunk against the approved plan, every change mapped to a plan item; anything unplanned is reverted or flagged as a delta, never silently kept.
- **Stranger test** — the standard every part of the method serves: a stranger — you after a year away, a new person, or the AI in the very next session — can rebuild full working context from the files alone, with no handoff and no one to ask.
- **Tag** — a git label pinned to one commit, used to mark releases (e.g. a version milestone) so that exact snapshot can be found and recovered by name.
- **Version stamp** — the version line a document carries, which the AI states back when it reads the file. A human-verifiable check: you know the current number, so a stale read is caught in the AI's first reply. One number per thing that actually moves — the method's shipped docs all carry the AI PM version and none of their own, since they only ever ship together; a file that would merely restate the project version carries no stamp at all; and record docs keep their own, which track that document's real revisions.

### Working vocabulary

- **Session** — one Claude Code conversation in the project folder, opened by typing a task and (ideally) ended with a close-out. The method's unit of work: one session, one task, one commit.
- **Fork** — a genuine decision point with more than one defensible path. Forks are raised for your call, never resolved silently — and an unresolved fork rides the handoff rather than being guessed into a document.
- **Milestone** — a completion worth declaring: a phase landing, a feature verified end to end. Milestones get changelog entries, and a declared milestone is what bumps the project's Major version.
- **Phase** — a project's largest unit of structure: a named stage of work with a beginning, an end, and its own record docs.
- **Slice** — a deliberately thin piece of a larger build, taken end to end before the next slice starts. Building in slices means something real works early, and each slice's lessons shape the next.

## Setting Up a New Project

Getting AI PM running on a project — new or existing — is called an adoption session. You don't need to be fluent in Markdown or git to run one: the AI performs the setup and writes the files; your job is the decisions.

Two starting conditions, one procedure: a project starting from zero and an existing project adopting the method mid-life both run the same adoption session — the guide asks a couple of extra questions for the mid-life case (where the project actually stands, and whether to backfill its pre-adoption history now or later).

### Running an adoption session

1. Get the current release package from the maintaining project — a zip of the Releases\vX\ folder: the method docs under Documents\AI PM\, the config template beneath them, the Adoption Guide at the root, and the six command files already placed in .claude\commands\. There is no CLAUDE.md in the package — if your project already has one, the unzip cannot touch it; the adoption updates it in place and never discards your content.
2. Create the project's folder (or pick the existing one) and unzip the package's contents directly into it — the method's folder tree lands ready-made.
3. Open Claude Code **in the project folder itself — the one that now contains the Adoption Guide, not its parent** — and type **"start the AI PM adoption"** (any phrasing naming AI PM or the guide works). The AI finds the guide at the folder root and follows it; its first reply should name the guide. If it doesn't, the session almost certainly opened in the wrong folder: close it, reopen Claude Code in the right folder, and try again.
4. The AI walks you through: a scope choice (Required Adoption — gets AI PM running; Full Project Interview — a deeper build-framework interview, planned but not yet built), then a short interview (project identity, starting version, Feedback Pass on/off, storage situation, and — for mid-life projects — current status and the backfill question). No file-naming questions: every AI PM project uses the same standard paths.
5. The AI then performs the setup: git init (checking your git identity is configured, and setting it for this project if not), CLAUDE.md created or amended with the pointer block, your config file filled in, the transit folders created and git-ignored, and the starter files drafted (backlog, changelog, the first handoff). It removes the guide file before committing — the project never carries it.
6. The session ends like every AI PM session will: a close-out with the project's first version-led commit and a written handoff — so the very next session opens exactly the way every session after it will.

All starter templates (the CLAUDE.md pointer block, the config file's fields) are canonical inside the Adoption Guide itself, since that's the document actually present at setup — this page doesn't duplicate them.

### Optional: the storage map

The guide's interview asks whether your project has significant off-workspace storage and can build the map on the spot, or defer it. The AI can gather the input itself (a folders-only tree of the storage root) and curate it with you into purposes and routing rules — never a file manifest.

## About This Page

Describes the **AI PM 3.2** release package — AI_Project_Manager.md, AI_PM_Instructions.md, AI_Project_Manager_Feedback_Protocol.md, this page, the AI_PM_Config.md template, and AI_Project_Manager_Adoption_Guide.md. They ship together and carry one version between them, so there is a single number to check rather than five to reconcile. If this page and AI_Project_Manager.md ever disagree, AI_Project_Manager.md wins.
