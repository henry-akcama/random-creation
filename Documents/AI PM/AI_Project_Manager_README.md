AI PM — README

Version: 4.0 — August 2026. METHOD-OWNED: byte-identical in every project, replaced whole by an upgrade. The HUMAN-FACING doc: how to do the three things you'll actually do, what AI PM is, the glossary, and the Doc Map. Everything here routes; nothing here is procedure the AI executes.

***

HOW DO I…

ADOPT AI PM IN A NEW PROJECT
1. Copy the release package's contents into your project folder (the folder tree lands ready-made).
2. Open Claude Code in that folder and type: "start the AI PM adoption".
That's it — the AI finds the Adoption Guide at the folder root and walks you through everything else in conversation, one question at a time. The guide deletes itself when done.

ADOPT AI PM IN AN EXISTING PROJECT
Same two steps, same folder-first rule. Your existing CLAUDE.md is never overwritten — the package ships none, and the adoption only appends one short pointer block to yours. Carving up an existing CLAUDE.md or migrating old files is never adoption's job; it waits for an upgrade survey or a session you choose.

UPGRADE TO A NEW AI PM VERSION
1. Copy the new release package into Documents\AI PM\Upgrade\.
2. Type: /ai-pm-upgrade — in a fresh session, or after the session-start offer.
The AI reads the package, surveys your project, and shows you one plan for approval before anything moves. Deferring costs nothing; the offer repeats each session until you run or remove it.

***

WHAT AI PM IS

A method for running long, multi-session projects with an AI assistant. The AI has no memory between conversations, so the project's files are made to BE the memory: written by the AI, reviewed by you, committed to git at a session close-out you trigger with /closeout (or /closeout-auto to review after instead of steering before). Session start reads the previous close-out's handoff and aligns before building. Everything in between is yours — your pace, your tools; the method only holds one boundary (memory is written at close-out, not mid-conversation) and one guarantee (every memory change passes your review, with git as proof).

The method is one product, many projects: your project carries a copy, upgrades come as packages from the maintainer, and observations flow home as feedback reports — your copy is never edited locally, so every project runs the same method. Why it is all shaped this way: AI_Project_Manager.md, the rationale doc.

***

THE DOC MAP — which file answers which question

Method files (in Documents\AI PM\; identical in every project):

* What does the AI do at session start? → AI_PM_Instructions.md
* What happens at /closeout? → Closeout_Procedure.md
* What happens at /ai-pm-upgrade? → Upgrade_Procedure.md
* How is a release built? → Release_Procedure.md (maintainer procedure; inert elsewhere)
* How do feedback reports work? → Feedback_Procedure.md
* What's in the release package? → Manifest.md
* Why is the method shaped this way? → AI_Project_Manager.md
* How do I adopt/upgrade; what does a term mean? → this README

Project files (yours; the AI maintains them at close-out):

* What is this project? Settings? → CLAUDE.md (yours to edit freely) · Documents\AI PM\Config\AI_PM_Config.md (settings only)
* What happened, and where's the detail? → Documents\Changelog.md — an entry LOCATES the record, it never IS the record
* What's next? → Documents\Backlog.md — forward-looking to-dos only; done items leave entirely
* Where does the next session start? → Documents\Handoff.md — rewritten whole every close-out
* What record docs exist and what is each authoritative for? → Documents\DOC_INDEX.md
* PROJECT STATUS: NOT STORED — by design. The handoff's opening is the only snapshot; ask the AI for a summary and it generates one from the changelog.

Fact routing, for anything new: a decision → the owning record doc (the changelog entry locates it) · a to-do → the backlog · reference data → its own doc, listed in the Doc Index · a fact with no home → the AI surfaces it at close-out and you rule; if the method itself lacks a drawer, that's feedback.

***

GLOSSARY

* CLOSE-OUT — the end-of-session procedure that writes memory and commits: /closeout (interactive) or /closeout-auto (gates removed, review after).
* MEMORY vs WORK PRODUCT — memory (decision-carrying files) is edited only at close-out; work product (code, drafts, artifacts) lands and commits mid-session with your approval.
* MEMORY COMMIT / WORK COMMIT — the close-out's single commit ("close-out — <headline>") vs a mid-session unit of work (plain description).
* HANDOFF — the next session's opener. CHANGELOG — the story, told once. BACKLOG — what's next. DOC INDEX — the record docs' directory.
* RECORD DOC — a permanent doc authoritative for a stated scope; living, edited in place, git as history.
* CHARTER — a standing file's written job description; the FILE HEALTH CHECK lints drafts against the charters.
* THE HALT — a deviation needing your judgment stops everything; nothing commits until you rule.
* DRY RUN — a rehearsal that writes, commits, and decides nothing.
* FEEDBACK PASS — the auto self-check after a full close-out that may suggest a report; SELF-CHECK — its audit alone (/self-check).
* MAINTAINING vs ADOPTING project — where the method's source lives vs every project carrying a copy; adopters report, never edit.
* RELEASE PACKAGE / MANIFEST / UPGRADE NOTES — the shipped version; its self-inventory; its per-release migration instructions.
* NAMING THE WORK — PROGRAM (multi-session effort) · PHASE (letter-named part of one) · SESSION · BLOCK (numbered chunk of a session) · PART (piece of a block) · SLICE (session-sized cut of a big backlog item) · DECISION (phase letter + number) · MILESTONE (named changelog event) · § (doc section). A symbol never travels alone ("Phase F", "block 3").
* PUSH SETTING — follow remote | on | off; governs every commit the AI makes.
