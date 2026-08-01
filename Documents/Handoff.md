Random Creation — Handoff

Project at v3.2. Written at the structure, storage and GitHub close-out, August 1 2026.

WHERE THINGS STAND

The scheme is settled and written down. The developer asked to be taught how a project like this
should be laid out and shipped rather than handed a finished scheme, and that session produced
Documents\Design\RandomCreation_DevelopmentLifecycle.md — read it before touching storage, build
or release questions, because it carries the reasoning and not just the conclusions.

docs\ is gone. Its v1.0 and v2.0 source snapshots live at Documents\Archive\ and are the only
surviving copies of pre-git source; everything else in it was hash-proven redundant first.
categories.json is homed at Source\...\SampleData\ and ships to samples\, never data\.

The project is on GitHub — repository created, public, all-rights-reserved, connected as origin,
LICENSE and README written — but NOTHING HAS BEEN PUSHED YET.

The app itself is still untouched: a finished v3.0, assembly 3.0.0.0. No code work has happened
under AI PM yet beyond one .csproj ItemGroup.

NEXT SESSION SHOULD TAKE UP

The push sequence, immediately — backlog item 1, and the developer ruled it happens right after
this close-out. Order matters: rewrite the 10 pre-adoption commits to the noreply address FIRST
(git filter-branch; it refuses to run on a dirty tree, and the rewrite is only safe while nothing
has been pushed), then the first push, which opens a browser once for Credential Manager. Only
after that do Actions and Pages become possible.

If the push already happened, the next topic is Visual Studio setup — backlog item 2, the half of
the structure session that was not reached.

OPEN ITEMS RIDING THIS HANDOFF

* The developer is new to git and GitHub and is deliberately learning, not delegating. Explain
  operations plainly AS THEY RUN. They dismissed two option-picker prompts in a row this session
  and engaged well with plain prose — prefer conversational answers over structured choice lists
  unless a decision genuinely has discrete branches.
* Two changes landed outside the project this session, both stated at the time: the global
  .gitconfig now carries the noreply email for every repo on this machine, and one scratch file
  was written to the system temp folder rather than the session scratch area.
* The developer does not read C#. Running the app is their only real review mechanism — protect
  it. A build server proves code compiles and cannot prove a GUI works; BUG 1 is the standing
  proof, since it compiles perfectly.
