Run a FULL close-out.

Read Documents\AI PM\AI_Project_Manager.md in full and follow its close-out
procedure (§7), FULL mode. Fill the stage skeletons exactly as §7 gives
them — do not compose your own presentation.

Stage 1 pre-flight (numbered items, your suggestion under each where you
have one; the git-and-touched-files sweep runs quietly and its findings
become items) → the developer answers, defers, or says proceed → Stage 2
proposed changes (grouped New / Edit / Delete; this stage asks NO
questions — a question discovered here sends the close-out back to Stage
1) → the developer approves → Stage 3 execute, self-review the git diff
against the approved plan, and commit → Stage 4 report.

Stages 3 and 4 arrive together in one message with no gate between them.
The exception is the halt: if a deviation from the approved plan needs the
developer's JUDGMENT rather than mere execution, stop before committing,
lead with the halt block, and ask.

Write the next-session handoff to Documents\Handoff.md (overwrite) —
opener and transient notes only; standing items live in the backlog. End
in a single commit whose message leads with the project version. The
Feedback Pass runs after, per the Feedback Protocol.
