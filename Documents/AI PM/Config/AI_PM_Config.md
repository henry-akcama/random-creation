AI PM — Project Config

THIS PROJECT'S OWN FILE — settings only. The AI maintains it, it is edited at close-out like any other memory, tracked in git, and NO UPGRADE EVER REPLACES IT: an upgrade edits it IN PLACE to match the new version's shape (adding, renaming, removing settings), never changing a value the developer chose. Which settings exist is the method's; what they are set to is the developer's. Read at session start. Everything that is not a setting lives elsewhere: record docs in Documents\DOC_INDEX.md, the story in the changelog, to-dos in the backlog.

* Project: Random Creation — WPF desktop app (C# / .NET 8)
* Feedback Pass: on
* Push: follow remote
* Allowed write locations: none
* External storage map: none — all project files have been consolidated into this one folder (July 2026); the file-server origin is history, not a live location. Documents\Archive\ (added August 2026) holds the pre-git v1.0/v2.0 source snapshots and is the only surviving copy of them, git history starting at v3.0. Off-machine backup is GitHub: akcama/random-creation, public.

The settings:

* FEEDBACK PASS — on | off. The one auto-running behaviour: a self-check after each full close-out that may SUGGEST a feedback report; observes and notifies, never acts. Off stops the auto-notice; /feedback-pass and /self-check still work by hand.
* PUSH — follow remote | on (<remote>) | off. Governs EVERY commit the AI makes. "Follow remote": no git remote → no pushing; a remote → push immediately after each commit. "Off" is the explicit opt-out. A failed push is reported, never resolved.
* ALLOWED WRITE LOCATIONS — adds to what the close-out's outside-the-project check treats as in bounds. Listing a location silences ordinary writes to it, nothing else: a change to what the AI is PERMITTED to do in future sessions always flags. "None" means the project folder, the session's scratch area, and the platform's own config folder alone.
* EXTERNAL STORAGE MAP — where this project's off-workspace storage lives and how to route to it, once that outgrows a line in CLAUDE.md. Folders not listed are deliberately out of scope.
