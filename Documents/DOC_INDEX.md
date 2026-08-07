Random Creation — Doc Index

Every record doc, its scope in one sentence, and its lifecycle state. This is the list a
stranger reads to know what is authoritative for what. It grows as docs are born and shrinks
as they retire; provenance and rename history live in the changelog.

All record docs live in Documents\Design\ (settled at the file-organization pass, August 2026).

| Doc | Scope | State |
| --- | --- | --- |
| RandomCreation_DevelopmentLifecycle.md | HOW THE PROJECT IS WORKED AND SHIPPED: storage scheme, build cycle, git/GitHub, licensing, packaging, sample content, the program-files versus user-data split. Distinct from every other record doc, which describe the APP. | live |
| RandomCreation_ReleasePlan_v4_0.md | WHAT v4.0 CONTAINS, IN WHAT ORDER, AND WHY. | live; retires once absorbed into a v4.0 record doc (backlog item 3) |
| RandomCreation_ProjectContext_v3_0.md | v3.0 design: architecture, every screen's layout, undo/toast/clipboard/drag specs, colour palettes, bug-fix table, deferred list. | live (still the deep architecture record) |
| RandomCreation_FileIndex_v3_0.md | What each source file does and what changed in v3.0. | live; a version behind (v3.0) |
| RandomCreation_EngineeringNotes.md | CODE-LEVEL ENGINEERING KNOWLEDGE: WPF traps, resource-precedence rules, weight-tier probability anchors, code-quality warnings, refactor candidates. | live |
| RandomCreation_ProjectContext_v2.0.md | v2.0 design record. | frozen, historical only |
| RandomCreation_ProjectContext_v1.0.md | v1.0 design record. | frozen, historical only |

No amendment chain: each doc is authoritative for its own scope and none amends another.
Screen Shots\ (v1.0–v4.0) and Icons\ sit alongside them as design artifacts, not records.
