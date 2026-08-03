Random Creation — Handoff

Project at v3.5. Written at the repo-move close-out (AUTO), August 3 2026.

WHERE THINGS STAND

The GitHub repository now lives in the akcama organization:
https://github.com/akcama/random-creation. The developer performed the transfer on GitHub;
this session verified the new location held the same history, repointed the local remote,
and swept the old henry-akcama address out of eight files (commit 36b489d — docs, installer
publisher fields, release-notes link, the in-app GitHub link; build verified). GitHub
redirects the old address, so published links and the live user's install are unaffected.

NEXT SESSION SHOULD TAKE UP

The post-release chores, unchanged and in the backlog's order: download-back verification of
the published v4.0 assets, then the live user's manual upgrade, then the v4.0 record doc,
then Releases\ retirement (explicit ruling required — git-ignored, no safety net).

OPEN ITEMS RIDING THIS HANDOFF

* THE LIVE USER'S UPGRADE IS MANUAL AND MUST CARRY HER DATA — copy via the two Open-data-folder
  windows. Skipping it makes her collections appear to vanish. Coordinate with the developer.
* Download-back verification not yet done; Releases\ retirement is blocked behind it. Note the
  release download URLs now go through the redirect — verification doubles as a check that the
  redirected downloads work.
* AUTO-mode judgment calls this close-out, for review: (1) the reference sweep was committed
  mid-session as 36b489d at the developer's approval, so this close-out has two commits behind
  it rather than one — same friction as the v3.4 note below; (2) Documents\Changelog.md was
  deliberately NOT swept — its v3.2 entry truthfully records creation under henry-akcama and
  rewriting history would falsify it; (3) installer AppPublisher renamed henry-akcama → akcama
  to match the org — takes effect next release; (4) AI_PM_Config.md (a close-out-only memory
  file) was touched mid-session by the sweep — mechanical URL substitution only.
* Method note, carried from v3.4 and reinforced this session: work committing mid-session under
  the project's push-early ruling vs. AI PM's single-close-out-commit rule remains a genuine
  friction point; candidate feedback report.
* Automation caveat for future sessions: UI clicks/keys sent while the developer is actively
  using the machine can silently go to their windows instead. Ask for hands-off before driving
  the app.
