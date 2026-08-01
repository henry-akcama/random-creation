Random Creation — Changelog

Append-only, newest on top, entries grouped under project-version headers. An entry locates the record; it never is the record. Pre-adoption history (v1.0 → v2.0 → v3.0, 2026) is NOT yet backfilled — deferred at adoption to a later dedicated session; raw material lives in data\changelog.txt (user-facing released changes) and the release archives.

## v3.1

* 2026-08-01 — File organization pass and CLAUDE.md rework, in eight commits (06888ff…a315c82). The Claude-project export zip was extracted into docs\ and hash-verified against the archive copies; .gitignore scoped to exclude build output, Releases\ and Sharable EXE\; design records and all screenshots consolidated into Documents\Design\; icon assets consolidated into Documents\Design\Icons\ with verified-identical duplicates removed; the v1.0 build moved from Sharable EXE\ to Releases\Random Creation V1.0\Creature Crafter\, retiring that root folder; the Source Code\ tree retired entirely after every file was hash-proven redundant or, for 63 phase snapshots, superseded by shipped v3.0 and retained in git. Root went from eight folders to five. CLAUDE.md rewritten from a 12-line stub to the full project guide, merging docs\CLAUDE.md and porting three working-style sections from the IFQ Tracker reference file, which was then retired. Two findings: the shipped v3.0 loads v2.0 theme dictionaries (backlog item 3, BUG 1), and the .NET SDK installed with Visual Studio builds this project from the command line in ~7 seconds, so the AI can build and run the app directly. Detail: this commit and the eight it follows.

## v3.0

* 2026-07-31 — AI PM 3.2 adoption. Method installed and configured: git repository initialized (branch main) on the existing v3.0 project, config filled (Feedback Pass on, no external storage, no allowed write locations), transit folders created and git-ignored, backlog/changelog/handoff born, adoption guide removed. Project version negotiated at 3.0 to match the shipped app. Detail: this commit.
