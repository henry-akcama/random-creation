Random Creation — Handoff

Project at v3.4. Written at the v4.0 ship close-out (AUTO), August 2 2026.

WHERE THINGS STAND

v4.0 IS RELEASED. One session took the app from untouched v3.0 to published v4.0: all twelve
plan items built and verified item-by-item in the running app, the installer and portable zip
published by the new tag-driven pipeline, and the developer personally verified the installer,
portable build, uninstaller and a real multi-page proof print before the tag went out.
Release: https://github.com/henry-akcama/random-creation/releases/tag/v4.0

The release plan doc was followed with zero re-planning; its fix approaches all held. The two
deliberately-open questions (dimmed-print opacity, sample content) were settled by the
developer looking, as designed: 60% stays, the sample stays.

NEXT SESSION SHOULD TAKE UP

The post-release chores, in the backlog's order: download-back verification of the published
assets, then the v3.0 historical release upload, then Releases\ retirement (explicit ruling
required — git-ignored, no safety net). The bigger piece when the developer wants it: the
v4.0 record doc that absorbs and retires the release plan.

OPEN ITEMS RIDING THIS HANDOFF

* THE LIVE USER'S UPGRADE IS MANUAL AND MUST CARRY HER DATA — copy via the two Open-data-folder
  windows. Skipping it makes her collections appear to vanish. Coordinate with the developer.
* Download-back verification not yet done; Releases\ retirement is blocked behind it.
* The dev data folder (bin\Debug\...\data) had its GenerationCounter reset to 0 and today's
  test history entries removed, on the developer's ruling. Real counting starts at their next
  generation.
* AUTO-mode judgment calls this close-out, for review: CLAUDE.md updated substantially (version,
  data-location fork, three-case startup, serials, pipeline — all facts this session made
  stale); Presets→Collections README screenshot swap was developer-approved mid-session.
* Method note: this session committed work in eleven per-item commits rather than a single
  close-out commit — the project's push-early ruling and the developer's verify-per-item review
  model. Deliberate, developer-visible, and a genuine friction point between the project's git
  workflow and AI PM hard rules 1/3; candidate feedback report.
* The WhatsApp screenshot incident: one automation capture accidentally caught the developer's
  chat window; the file was deleted immediately and recaptured. Noted for transparency.
* Automation caveat for future sessions: UI clicks/keys sent while the developer is actively
  using the machine can silently go to their windows instead — the false "Generate is broken"
  alarm this session was exactly that. Ask for hands-off before driving the app.
