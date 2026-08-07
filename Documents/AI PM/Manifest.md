---
release: "4.0"
upgrades_from: "3.2"
date: 2026-08
---

AI PM — Package Manifest

Pure inventory, nothing else: every file this package contains, by path (package path IS install path), each with its role and a one-phrase description. Roles are a closed vocabulary: INSTALLED (lands and stays; the next upgrade adds/replaces/removes by old-Manifest-vs-new-Manifest arithmetic) · TRANSIENT (does its job during install, removed at cleanup) · ADOPTION-ONLY (used only when adopting; never touches an existing project's copy). Assembly diffs the built package against this list; adoption and upgrade install what it names and verify what landed, dot-entries shown. The Manifest itself is installed and stays: the next upgrade reads it to know what this version put on disk.

| Path | Role | What it is |
| --- | --- | --- |
| .claude\commands\closeout.md | installed | thin trigger → Closeout_Procedure.md, FULL mode |
| .claude\commands\closeout-auto.md | installed | thin trigger → Closeout_Procedure.md, AUTO mode |
| .claude\commands\ai-pm-upgrade.md | installed | thin trigger → Upgrade_Procedure.md |
| .claude\commands\feedback-pass.md | installed | thin trigger → Feedback_Procedure.md, Pass |
| .claude\commands\self-check.md | installed | thin trigger → Feedback_Procedure.md, self-check |
| Documents\AI PM\AI_PM_Instructions.md | installed | session-start procedure, standing rules, routes |
| Documents\AI PM\Closeout_Procedure.md | installed | the close-out, executed at /closeout and /closeout-auto |
| Documents\AI PM\Upgrade_Procedure.md | installed | the upgrade, executed at /ai-pm-upgrade |
| Documents\AI PM\Release_Procedure.md | installed | maintainer procedure — building a release |
| Documents\AI PM\Feedback_Procedure.md | installed | the Feedback Pass and the close-out self-check |
| Documents\AI PM\AI_Project_Manager.md | installed | the rationale doc — mission, spirit, reasoning, glossary of whys |
| Documents\AI PM\AI_Project_Manager_README.md | installed | human-facing: the three recipes, explainer, Doc Map |
| Documents\AI PM\Manifest.md | installed | this file |
| Documents\AI PM\Config\AI_PM_Config.md | adoption-only | config TEMPLATE — an existing project's config is never replaced; upgrades edit it in place |
| AI_Project_Manager_Adoption_Guide.md | adoption-only | interactive adoption procedure; self-deletes before the first commit |
| Upgrade_Notes.md | transient | this release's config migration, detect → propose survey checklist, and warnings; removed at cleanup |
