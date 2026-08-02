Random Creation — Handoff

Project at v3.3. Written at the GitHub, Visual Studio and v4.0 planning close-out, August 1 2026.

WHERE THINGS STAND

The tooling is finished and the plan is written. GitHub is live and in sync — the commit rewrite
and first push both ran and were verified. Visual Studio runs the app; F5 was only ever failing
because the build configuration sat on Release.

The app itself is STILL UNCHANGED: a finished v3.0, assembly 3.0.0.0. Not one line of C# has been
edited under AI PM yet. That ends next session.

NEXT SESSION SHOULD TAKE UP

Building v4.0. Read Documents\Design\RandomCreation_ReleasePlan_v4_0.md FIRST and in full — it is
authoritative for what the release contains, in what order, and why, and every fix approach was
already argued out and decided. Do not re-plan it. The build order is in its section 3; start with
BUG 1 and BUG 3, which are quick, independent, and give the developer something visible early.

The packaging decisions that ride alongside are NOT in the plan doc — they are in
RandomCreation_DevelopmentLifecycle.md sections 6 and 8, which carry the reasoning.

OPEN ITEMS RIDING THIS HANDOFF

* THE DEVELOPER DOES NOT READ C#. Running the app is their only real review mechanism. Verify
  after EACH item rather than batching — they chose "plan it all, then build it all" precisely so
  that building could go item by item. BUG 1 must be checked with screenshots, never by compiling:
  compiling cleanly is exactly what shipped that bug.
* They are new to git and GitHub and are deliberately learning, not delegating. Explain operations
  plainly AS THEY RUN. This session's pattern worked well — say what a command does, say what is
  irreversible, verify afterwards and show the evidence.
* THE "RUN" BUTTON ON CODE BLOCKS DID NOT WORK for them three times running; commands only
  executed when the AI ran them. Assume it is unreliable and confirm state rather than trusting a
  reported run.
* Two questions are deliberately left open for the developer to settle by LOOKING, not by
  discussion: the dimmed-item opacity on a real proof print (start at 60%), and the final content
  of the new sample file. Do not resolve either in advance.
* Ask before building BUG 3 whether anything changed about their taskbar: auto-hide support was
  deliberately excluded on the grounds that they do not use it.
* bin\Debug\...\data old\ is a genuine v2.0-era data set and is the test material for BUG 2's
  unrecognised-data path. Keep it until that fix is verified, then it can go.
* One scratch file was written to this session's own scratch area; nothing landed outside the
  project.
