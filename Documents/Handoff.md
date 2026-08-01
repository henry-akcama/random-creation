Random Creation — Handoff

Project at v3.0. Written at the AI PM adoption close-out, July 31 2026.

WHERE THINGS STAND

Adoption is complete: git repository live (branch main, first commit is this one), config filled, cadence files born, six slash commands verified. The app itself is a finished, shipped v3.0 (assembly 3.0.0.0) — no code work has happened under AI PM yet. The folder still carries the un-organized aftermath of two migrations (file-server consolidation + Claude-project export): five overlapping source trees, an un-extracted knowledge-base zip (docs\RandomCreation_ClaudeCode_setup.zip), duplicates, and a root-level "refrance CLAUDE.md" from another project kept only as style reference. Live code is Source\RandomCreation\ ONLY.

NEXT SESSION SHOULD TAKE UP

The file-organization pass (backlog item 1): extract the docs zip, map duplicates, agree archive-vs-reference-vs-live per tree, settle .gitignore scope for build output (bin\, obj\, .vs\) and large binaries (Releases\ zips ~70 MB — note GitHub's 100 MB file limit if item 3 proceeds), tidy the root. Then the CLAUDE.md rework (backlog item 2) — the two go naturally together in one session. GitHub (item 3) and bug collection (item 4) follow.

OPEN ITEMS RIDING THIS HANDOFF

* .gitignore currently covers ONLY the AI PM transit folders. Build output and binaries are tracked-eligible until item 1 rules otherwise — deliberately left for the organization pass rather than guessed at adoption.
* Standing developer preferences, to land in CLAUDE.md at item 2: deletions must be loudly flagged in advance or handed to the developer; developer is new to git and GitHub — explain git operations plainly as they happen.
