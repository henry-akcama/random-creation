Install a staged AI PM upgrade.

Read Documents\AI PM\AI_Project_Manager.md §12 in full and follow the
upgrade flow. THIS IS NOT A CLOSE-OUT: it has its own steps and its own
commit species.

State the plan first (which method files are replaced, what is preserved)
and wait for the developer's go. Then: run the drift check against git
history and the current diff, and capture any locally edited method file
as a feedback report into the project's own outbox BEFORE overwriting it;
replace the method FILES in Documents\AI PM\, never its subfolders —
Config\, Feedback\ and Upgrade\ all survive untouched; verify (version
stamps consistent, config and subfolders intact, git diff showing only
method files); commit with the message "AI PM upgrade X.Y → X.Z",
containing method files and nothing else. The project's own version does
NOT move.

Preserve Documents\Handoff.md — add only one annotation line ("upgraded AI
PM X.Y → X.Z on DATE; opener below unchanged") and leave the opener alone.

Finally, delete the staged package from Documents\AI PM\Upgrade\ and say so
in the summary, so the next session start does not re-offer it.

If the staged package is not exactly one version ahead, stop and flag it
for the maintainer instead of attempting it.
