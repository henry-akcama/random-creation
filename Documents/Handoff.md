Random Creation — Handoff

Project at v3.1. Written at the file-organization and CLAUDE.md close-out, August 1 2026.

WHERE THINGS STAND

The folder is organized. What arrived as five overlapping source trees, an un-extracted zip and three-deep duplication is now five folders and four files at the root: .claude\, docs\, Documents\, Releases\, Source\, plus .gitignore, categories.json, CLAUDE.md and a .vs folder Visual Studio left behind. Source Code\ and Sharable EXE\ are both gone — every file in them was hash-proven redundant before removal, and all tracked content remains recoverable from git. CLAUDE.md is no longer a stub: it now carries the full project guide, the live-code warning, build commands, architecture, conventions, and the working-style sections ported from the IFQ reference file (which has been retired).

The app itself is untouched — still a finished v3.0, assembly 3.0.0.0. No code work has happened under AI PM yet.

NEXT SESSION SHOULD TAKE UP

Backlog item 1, the project structure and storage design session. This is the developer's own stated next topic and the reason this close-out happened when it did: they want to understand how the project should be laid out — what is stored where and why, how a build becomes a release, where git and GitHub fit — rather than have it decided for them. TEACH IT, walk through it, do not hand over a finished scheme. The docs\ folder's fate, categories.json's home, and Visual Studio setup all hang off that conversation and are deliberately unsettled; the backlog carries the detail on each.

OPEN ITEMS RIDING THIS HANDOFF

* docs\ does not survive under that name — developer ruling. It was only ever a landing point for grabbing files out of the Claude project. It may be renamed; Documents\ is already this project's documentation folder, so that name is taken. Its contents are the v1.0/v2.0/v3.0 source snapshots plus README.md, the old project-guide CLAUDE.md (now redundant with the root CLAUDE.md, left in place deliberately so the folder is decided as a whole), the original export zip, and a stray _bridge_test.txt.
* The AI can now build and run the app. Visual Studio's .NET SDK (9.0.307) compiles this project from the command line in about seven seconds — verified this session, clean, zero warnings. This changes what a bug session can look like: changes can be built and the app launched for the developer to click through, rather than described. The developer does not read C#, so running the app is their only real review mechanism — protect that.
* One machine-level change was made this session: the first dotnet command triggered .NET's first-run setup, which installed an HTTPS development certificate (valid to August 2027). Standard, unrelated to this app, stated for the record.
