AI Project Manager

Version: 4.0 — August 2026. METHOD-OWNED: byte-identical in every project, replaced whole by an upgrade. THE RATIONALE DOC — why the method is shaped as it is: mission, spirit, design reasoning, the failure catalog, history. Read at adoption, when someone asks why, when designing a change to the method. NOT read at close-out or session start — what the AI executes lives in the procedure files (AI_PM_Instructions.md, Closeout_Procedure.md, Upgrade_Procedure.md, Release_Procedure.md, Feedback_Procedure.md), each lean and read at its moment of use. Human-facing recipes, glossary, and the Doc Map: AI_Project_Manager_README.md.

***

MISSION

AI PM gives an AI-assisted project a memory the human can trust.

1. The files are the AI's memory. The AI has none between sessions, so the project's files are made to be it: written AI-first, complete enough that the next session's AI rebuilds full working context from the files alone. Humans get up to speed by asking the AI.

2. Memory is written after the conversation, not during it. A topic gets worked all the way through, every decision made, before anything is committed to memory, because a follow-up can reopen anything half-written. Work product (mockups, code, artifacts) lands mid-session with the human's approval; everything else waits for close-out. The test: if the conversation keeps going, could this edit change? Then it waits.

3. Every memory change passes human review. Whoever is instructing the AI holds the gates; what matters is that it is a human. Close-out runs in clear, recognizable steps so nothing important is lost, nothing unwanted lands, and nothing is removed by accident, with git as proof anyone can check.

4. One method, many projects. Portable and adoptable as-is, upgraded from one source of truth, improved by feedback flowing home to the maintainer.

***

1. WHAT THIS IS

A method for running long, multi-session projects with an AI assistant. The test everything serves: a STRANGER — the developer after a year away, a new person, or the AI itself in the very next session — can rebuild full working context from the files alone, with no handoff and no one to ask.

Platform: Claude Code. A project is a FOLDER — a directory carrying a CLAUDE.md and a git repository. The assistant auto-loads CLAUDE.md at session start; that is the whole project-detection mechanism. Git is the verification layer: mechanical, complete, human-inspectable history.

2. SPIRIT

The method is defaults and triggers, applied with judgment — not a script to recite. Three rules are hard (stated where they execute, in Closeout_Procedure.md: the single memory commit; the hunk-by-hunk self-review of it; git as the archive, deletion in a commit IS archival). Everything else bends when the situation calls for it — and when a step fights a session, that friction is a candidate improvement to the method (§8), not something to grit through forever.

GOOD ROADS, NOT TRAIN TRACKS. A miswrite is a recoverable annoyance, not a catastrophe: git status at session start, the diff review, and human review of every memory commit are what earn the relaxed posture. Rules are built as good roads — the easy path is the right one — not as locks. The method ships no enforcement; the developer keeps full rights over every file in their own project, and the safety net is designed so that overriding a rule is visible rather than forbidden.

THE BOOKEND PRINCIPLE. AI PM is a bookend method: session start buys orientation, close-out buys the memory guarantee, and between them the method owns nothing but one boundary — memory waits. Mid-session belongs to the developer: their pace, their tools, their commit habits. Consistency across developers and projects lives at the bookends, which is where a PROJECT needs consistency (how it is documented, what is next), not in how anyone works. Design test for any proposed feature: it must attach to a bookend; a feature that can't find one is evidence against building it. (This is why hooks and other mid-session platform machinery stay the developer's own business, and why the Feedback Pass — the one auto-running behaviour — fires at a bookend's edge.)

3. THE DOC SET — why it is shaped this way

EXECUTION AND RATIONALE NEVER SHARE A READ. The AI is the method's user: the files are read by the AI, the procedures executed by it. Mixing the two kinds of text made every close-out re-read ~9,000 words of which the executed part was a fifth, with the load-bearing rules hidden in narrative. So: PROCEDURE FILES are lean and checklist-shaped, carrying one-line whys only where the why is load-bearing at execution time, read in full at the moment of use — cheap, because they are small. THIS DOC holds the reasoning, read when reasoning is the need. The stated risk — split docs drifting apart — is carried at release time (they ship together, bound by the Manifest; the release sweep reads both) rather than taxing every session.

LIVING DOCS. Record docs are edited in place; git and the changelog are the history. The old cross-doc amendment chain — never rewrite an earlier doc; pile later docs on top — solved a dead platform's problem (files could not be edited, so changes accumulated as new documents) and taxed every read with layer-stacking. Its failure mode, old truth silently destroyed by an edit, is prevented mechanically now: every fact has ONE DECLARED HOME, the owning doc is edited under the safety net (read-in-full before editing, the hunk-by-hunk diff review, a changelog entry locating the change), and provenance on demand is git's job — paid only when asked. No developer reads files to judge how much to believe them; they ask the AI, and the AI answers from git. When one doc absorbs enough edits that it stops reading as a whole, a COHERENCE SESSION re-reads and smooths it — triggered by symptom (patchy reading, an edit contradicting an unrevised paragraph), never by count.

ONE FACT, ONE HOME. The same truth deliberately stated in several files for several audiences is a correctness hazard for an AI reader: four sites can be read and acted on while a contradicting fifth sits unread. The Doc Map (in the README) is the routing table — which doc answers which question, where each fact-kind lives — and it is what makes read-when-relevant workable and a homeless fact VISIBLE instead of silently wedged into whichever mandatory-read file cannot refuse it. Sessions produce fact-kinds beyond the obvious (registries, standing reference data, decline-rationales, as-built state); a fact with no drawer is surfaced at close-out for the developer to rule on, and a drawer the method lacks is feedback material.

CHARTERS. Every standing file carries a written job description (the cadence files' charters live beside the close-out's write-list; the Doc Index lists each record doc's scope). Uncharted files drift into junk drawers — the field evidence behind this rule was one project's status file at 1,539 lines while another's, under identical method text, held 60. The FILE HEALTH CHECK (run over close-out drafts before the developer sees them, and on demand over the standing files) lints against the charters and the Doc Map's routing: no graveyards, no standing riders in the handoff, changelog entries that LOCATE records rather than BE them, no homeless facts. It observes and notifies; it never acts.

NO STORED STATUS. Project status is a question, not a file. Every stored status section observed in the field held only duplicates of facts whose real home existed — a section with no reader of its own has no boundary either. The handoff's opening is the only structurally trustworthy snapshot (rewritten whole each close-out, expires on read); human-facing summaries are GENERATED on request from the changelog. ONE SESSION, ONE STORY, TOLD ONCE: the changelog tells it, the backlog points forward, the handoff opens the next session, and no file grows another telling.

CLAUDE.MD IS FOUR LAYERS, and only one is this repo's. The METHOD layer is the fenced pointer block (identical everywhere). The PERSONAL layer — pacing, tone, working style — belongs to a person and lives in their user-level CLAUDE.md (C:\Users\<name>\.claude\CLAUDE.md), outside every repo; anything a project DECIDES OR KNOWS may not live there, invisible as it is to git and teammates. The PROJECT-TYPE layer (platform traps, edit protocols shared by FileMaker or Unity or web projects) belongs in type docs read on trigger. The PROJECT layer — what this project is, where things live — is the only part that belongs in the repo's CLAUDE.md. The placement rule: NOTHING LIVES IN CLAUDE.MD UNLESS IT MUST BE READ IN FULL EVERY SESSION; everything else moves to a file with a trigger the AI reliably hits. CLAUDE.md piled up historically because it was the only file guaranteed to be read — the fix is routing, not discipline.

4. MEMORY, WORK PRODUCT, AND COMMITS

WHY MEMORY WAITS. The edit COST died with the old platform (the AI edits files directly now), but the edit HAZARD did not: a half-settled decision written to disk looks settled. Waiting until close-out means writing when nothing can reopen the decision, reviewed once, coherently. The volatility test (if we keep talking, could this edit change?) is deliberately strict — even an index paired with renamed files waits, because the running conversation already holds that context and nothing is misled by the delay.

THE COMMIT/REVIEW SPLIT. The review unit and the commit unit are different things. WORK PRODUCT commits mid-session when a unit is verified, with the developer's approval — each commit doing backup duty, push-early honored. MEMORY lands in the close-out's single commit, and the hunk-by-hunk review covers THAT COMMIT ONLY: it is where the execution-matches-approved-plan guarantee lives. Code is never re-diffed at close-out — its review already happened at the only moment it was real, when the change landed and the developer approved it; re-reviewing with less context is a rubber-stamp gate. The close-out report lists the session's work commits so the footprint sits on one screen — a list, not a review. The knowing trade: the old "one diff = the whole session" single lens is given up for several smaller lenses along the way.

GATES THAT GET RUBBER-STAMPED ARE WORSE THAN NO GATE — they train the eye to skip the gates that matter. This principle placed every gate in the close-out: the consent moment is plan approval (the last point where changing course is free); no gate follows execution, because "did I do what you approved?" is a yes nearly every time. The HALT is the exception that proves it: when a deviation needs a DECISION rather than execution, everything stops and nothing commits — the developer's judgment is never simulated. The same two-tier logic governs upgrades.

WHY SLASH-ONLY CLOSE-OUT ENTRY. The action is consequential enough that the trigger must be mechanical rather than interpretive — a close-out drifting into existence from a phrase the AI read as a request is a real failure. A deliberate, stated exception to the method's plain-language philosophy; /ai-pm-upgrade is slash-only for the same reason.

WHY THE PRESENTATION IS FIXED. A close-out that looks identical every run is one the developer can scan instead of read — the template look is what makes review cheap enough to actually happen. Fixed banners, fixed band order, one continuous number line, the destructive group last, clean results one line everywhere, the emoji as pure status vocabulary. The skeletons carry no explanation of their own rules; they are learned once, from here.

5. VERSIONING — one number

AI PM HAS ONE NUMBER: the method version, X.Y, moving only at releases — stamped in the shipped docs, tagged in git, naming Releases\vX.Y\, led in upgrade commits. It survives because the failures it prevents are real: a project running stale files while believing itself current, an upgrade applied to the wrong base, a package not matching its claim. Everything else is identified by WHAT it was (descriptions, headlines), WHEN (dates, git timestamps), and WHO (git authors) — git counts what needs counting. The retired numbers each turned out to be a counter doing contract cosplay: a project version bumped per close-out promised nothing to anyone; per-doc stamps defended an amendment chain that no longer exists; session numbers duplicated the platform's own sort column. MILESTONES survive as named, declared changelog events — the rare human-meaningful marker was the part that worked. Commits lead with their SPECIES (close-out — … / plain work description / AI PM upgrade X.Y → X.Z); citations point at date + headline, or a git hash when exactness matters.

6. TRUST AND MULTI-DEVELOPER

Verification checks are designed so the HUMAN can verify them: the opener's version line, the species-led commit message, the clean tree, the check bands re-runnable by eye. A check only the AI can attest is not a check — which is why the OUTSIDE THE PROJECT line is honestly a notification, resting on the AI's own record of what it did, its two limits (self-reported; summarized-context sessions can only account for what survived compaction) carried in the open.

PUSH FOLLOWS THE REMOTE: no remote → off; a remote → on; "Push: off" is the explicit opt-out. With the setting on, commit and push are one motion — a session's memory reaches the shared remote the moment it exists, which is what makes two developers on one project safe by default: the second session's push is REJECTED by git if the remote moved, and a rejected push or moved upstream is a HALT — surfaced, resolved with the developer, never force-pushed, never silently merged. That floor closes the catastrophic case (one session's memory silently destroying another's) mechanically. A designed merge procedure for the colliding-handoffs case waits on a backlog trigger: the first real collision. Developers coordinate WORK overlap the human way; the method owns only the collision its own files create.

A failed push is REPORTED, NEVER RESOLVED — resolution is a human decision, made with the developer, at the moment it is real.

7. THE LIFECYCLE — authoring, release, upgrade, adoption

THE MIRROR. Source Code\ (the maintainer's drafting workspace) mirrors the shipped tree exactly: package path IS install path, assembly is a wholesale copy, and the class of bugs born in re-mapping steps is structurally gone. A draft command in Source Code\.claude\ is just a file — the platform loads commands only from the project root.

THE MANIFEST. The package lists its own contents — pure inventory, a closed role vocabulary (installed / transient / adoption-only), nothing else allowed to grow in it. Two mechanical checks read the same list: release verification diffs the built package against it; adoption and upgrade install what it names and verify what landed, dot-entries shown. It stays installed, so the NEXT upgrade derives add / replace / remove arithmetically from old-vs-new — no old package needed, no guessing, deletions included.

THE RELEASE IS A WORK UNIT. Sweep, Manifest, copy, verify, tag — mechanical work verified mechanically, landing as the build's final work commit. It is deliberately NOT part of a close-out: a human hunk-review of thousands of copied lines is the definition of a rubber-stamp gate. The release commit also retires its predecessor's folder — Releases\ holds the current version only; every past package lives at its tag.

UPGRADES ARE TAILORED, NOT GENERIC. Every release ships UPGRADE NOTES — config migration one-liners, a survey checklist of DETECT → PROPOSE pairs, warnings — and the upgrade reads the project's actual lived-in files against them, building THAT project's worklist live. One notes file serves differently-shaped projects because only firing detects enter the plan. The plan is approved once, as a whole; ceremony is weight-proportional by construction (a light upgrade's plan is five mechanical lines and one yes). The survey doubles as the method's field instrument: findings that suggest a missing drawer are feedback material. The upgrade never improvises on project files, writes its own memory, and needs no close-out. Config: which settings EXIST is the method's; what they are SET TO is the developer's — upgrades edit shape in place, never values, never enabling behaviour unasked (a changed default is surfaced at plan time, so approval covers it).

ADOPTION: copy the package in, type the prompt — every question asked in conversation at the moment it matters, nothing to study first. It installs what the Manifest names, verifies what landed, births the standing files, commits its own work (the drift check's baseline when no upgrade has yet run), and the guide deletes itself. A botched adoption is structurally detectable.

8. THE SELF-HONING LOOP AND FEEDBACK

At every full close-out the AI asks: did any part of this procedure fight the session? A yes is a candidate improvement. In the MAINTAINING project the loop closes directly — friction folds into the method's source at a later session. In an ADOPTING project the loop is DETECTOR-ONLY: it notices and routes outbound, never edits the local copy — one source of truth, no forks. The failure this prevents: a good lesson learned in project A dies there for lack of a route home, or gets patched into A's local copy until A silently runs a different method than everyone else. Observation is split from authority: projects OBSERVE and REPORT; the maintainer DECIDES.

WHY THE FEEDBACK PASS IS AUTOMATIC, when almost nothing else is: it exists to catch a FAILED CLOSE-OUT — the uncommitted session, the handoff described but never rewritten — and a failed close-out is precisely the thing nobody thinks to go looking for. It runs after the close-out, outside it, because that failure can only be seen from outside. It observes and notifies; nothing is written without the developer's word.

9. FAILURE MODES THE METHOD PREVENTS

* The wall: a report so dense the developer skips reading it. (Fixed skeletons; clean results one line; worth-your-eyes first.)
* The late question: a question surfacing after docs are written. (Pre-flight before planning.)
* The stale rewrite: rewriting from a partial or out-of-view copy. (Read the current file in full before editing.)
* The unverifiable claim: the AI attesting what only it can check. (Human-verifiable checks.)
* The manufactured update: inventing doc changes when nothing durable changed. (Don't manufacture updates — or reports.)
* The uncommitted session: memory evaporating unwritten or unpushed. (The memory commit; the self-check; the Pass.)
* The junk drawer: facts wedged into whichever mandatory-read file couldn't refuse them. (One fact one home; the Doc Map; charters; the file health check.)
* The graveyard: done items and old narratives inflating living files. (No-graveyards charters; deletion in a commit is archival.)
* The rubber-stamp gate: an approval that is always yes, training the eye to skip real ones. (Gates only where a decision is free to change.)
* The coined term: process vocabulary drifting past the developer. (The naming system; define [term]; the glossary.)
* The forked method: a project's local copy silently diverging. (Detector-only; the drift check; feedback flowing home.)
* The silent overwrite: one session's memory destroying another's. (Push-follows-remote; the rejected-push halt.)

10. ADVISORIES (practice, not procedure)

* MODEL PER PART: emerging practice is the strongest model for heavy-compression close-outs and design sessions, a cheaper one for routine work. Procedure files stay model-agnostic; mid-session is the developer's.
* MODEL DRIFT: the method's procedures are executed by a model that changes underneath it every few months. Watch item — evidence of model-version-correlated procedure misbehavior routes through the Feedback Pass.
* SUBAGENTS are permitted as an implementation detail where the procedure stays identical and every result still lands in front of the developer; never a required dependency. HOOKS run mid-session, which the method does not own — they exist and are the developer's. PLATFORM PERMISSION MODES compose independently with the method's conversation-level gates; the method's AUTO close-out is unrelated to any platform mode of the same name.
* VOCABULARY DISCIPLINE: the AI does not coin process vocabulary by casual use. A concept that needs a name is named explicitly — "I'm calling this X, meaning Y" — and the developer's standing move is "define [term]": if the AI can't define it crisply, the term shouldn't exist.

11. HISTORY (milestones; full story in the maintaining project's git and changelog)

* 4.0 (August 2026) — THE REDESIGN. The product layer designed whole: execution/rationale split (this doc's birth in its current role), living docs, one number, the lifecycle subsystem (mirror, Manifest, tailored upgrades, release-as-work-unit), push-follows-remote, status retired, charters and the file health check, the naming system. Design record: AI_PM_Redesign_Design.md in the maintaining project.
* 3.x (July 2026) — the Claude Code migration (3.0: git as verification layer, slash commands, release packages), the upgrade subsystem and config extraction (3.1), the outside-the-project check (3.2).
* 2.0 (July 2026) — the Feedback Protocol subsystem.
* 1.x (July 2026) — born on Claude Projects: roles, stranger test, close-out gates, self-honing.
