# docs\ — Claude project knowledge export

Every text document from the **Random Creation** Claude project, exported 30 July 2026.
85 files, organised by version. **Filenames are the project's own names, suffixes intact**
(`Models_3_0.cs`, `App.xaml_2.0.cs`) so they can be matched back to the project one-to-one.

**These are reference copies, not build inputs.** The live solution is
`Source\RandomCreation\` — see `CLAUDE.md` at the folder root.

## What's here

| Folder | Files | Contents |
|--------|-------|----------|
| `v3.0\` | 35 | Current version. Full source, both context docs, file index, changelog |
| `v2.0\` | 33 | Previous version. Full source, context doc, changelog |
| `v1.0\` | 17 | Original *Creature Crafter* release. Source and context doc |

## Start here

| File | Why |
|------|-----|
| `v3.0\RandomCreation_ProjectContext_v3_0.md` | **The important one — 42 KB.** Full v3.0 design document: architecture, every screen's layout, undo/toast/clipboard/drag specifications, colour palettes, bug-fix table, deferred-to-v4.0 list. This was the one file that existed *only* in the Claude project and nowhere on disk. |
| `v3.0\RandomCreation_FileIndex_v3_0.md` | What each source file does and what changed in v3.0 |
| `v3.0\RandomCreation_ProjectContext_3_0.md` | Condensed 6.8 KB v3.0 summary — a different document, not a duplicate of the above |
| `v3.0\changelog_3_0.txt` | Released changes in user-facing wording |
| `v2.0\RandomCreation_ProjectContext_v2.0.md` | v2.0 design document — useful for "why is it like this" questions |
| `v1.0\RandomCreation_ProjectContext_v1.0.md` | Original design document |

## Overlap with the rest of the folder

Most of the source here already exists on disk, and that's deliberate — this is a complete
snapshot of the project knowledge base rather than only the gaps.

- `v3.0\` source duplicates `Source Code\3.0\3.0 Release files\Claude named files\`
- `v2.0\` source duplicates `Source Code\2.0\`
- `v1.0\` source duplicates `Source Code\1.0\`

83 of the 85 files are **byte-for-byte identical** to a copy already on disk. Exactly two
were not on disk anywhere, and both are the long-form context documents:

- `v3.0\RandomCreation_ProjectContext_v3_0.md` — 42 KB (the disk copy,
  `Source Code\3.0\3.0 Release files\RandomCreation_ProjectContext.md`, is the 6.8 KB summary)
- `v2.0\RandomCreation_ProjectContext_v2.0.md` — 20 KB (the disk copy,
  `Source Code\2.0\RandomCreation_ProjectContext_2.0.md`, is a shorter 16.6 KB version)

## Not included

The project's 36 image files — the 7 app icons and all v1.0/v2.0/v3.0 UI screenshots —
can't be exported as text. They are already on disk:

- Icons → `Graphics\`
- v3.0 screenshots → `Source Code\3.0\3.0 Release files\Screen Shots\`
- v2.0 screenshots → `Source Code\2.0\Screen shots\`
- v1.0 screenshots → `Graphics\Screen Shots 1.0\`

## Two things the export turned up

- **v1.0 naming inconsistency (in the project, not the export):** the v1.0 context doc's
  file table lists `ResultDetailDialog_1.0.cs`, but the project stores that code-behind as
  `ResultDetailDialog.xaml_1.0.cs`. Preserved as-is.
- **Verbatim formatting kept:** in `v2.0\ManageContentScreen.xaml_2.0.cs` the `return`
  statements inside `IsOverCollectionZone` and `IsOverCategoryScrollViewer` sit
  un-indented at column 0 in the project doc. Not corrected — these are faithful copies.
