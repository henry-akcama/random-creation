IFQ Tracker — FileMaker database redesign for Alaska halibut/sablefish IFQ (Individual Fishing Quota) data: quota-share holdings, holders, certificates, annual limits, prices, and the recurring NOAA import pipelines.

--- AI PM — keep this block intact ---
This project runs on AI Project Manager (AI PM).
All paths below are relative to this project's folder.
Session start: read Documents\AI PM\AI_PM_Instructions.md in full and
follow it.
Close-out (/closeout, /closeout-light, /closeout-auto — these commands
only): read Documents\AI PM\AI_Project_Manager.md in full and follow it.
--- end AI PM block ---

(Most sections below were mined July 2026 from IFQ_Tracker_Instructions.md
v1.7 at the AI PM 3.0 adoption, with paths updated to the reorganized
tree. The AMENDMENT CHAIN and the per-doc key-documents catalog remain in
that file until the queued chain/catalog migration pass — unblocked by the
v1.13 pillar consolidation — moves them out; see ORIENTATION.)

THE APPLICATION

IFQ Tracker is a FileMaker-based application that provides IFQ (Individual
Fishing Quota) permit holders, industry participants, and other authorized
users with access to quota share and annual quota information for Alaska
Halibut and Black Cod fisheries.

Current users include fishermen who fish other people's pounds. The
planned user base will grow to include brokers and fish plants, scaling
from a handful of users to approximately 20 internal/industry users, plus
a future public-facing web portal. End users have NO write access to
fishery data (holdings, holders, annual limits, prices, landings,
certificates, change history) — only the admin enters or edits that data
via import or direct entry. End users CAN write to their own
account-scoped state: their settings, starred holders, dashboard
preferences, saved searches (USER PREFERENCES), and personal price
estimates that overlay only their own calculated views without altering
the underlying data (USER ESTIMATES). The rule of thumb: a user action is
allowed if it changes only what that user sees, and forbidden if it
changes what the data says for everyone. In the target architecture,
FileMaker is the admin tool and backend; all end users access data
through the web portal, read-only on fishery data and read-write only on
their own user-state rows.

PROJECT OVERVIEW AND DESIGN PHILOSOPHY

This project is a ground-up redesign of an existing FileMaker solution
that accumulated significant technical debt through organic growth. The
existing solution is reference material only — a source of business
logic, domain knowledge, and data to be migrated; not a blueprint to
preserve.

- Start from the business domain, not the existing structure
- Ask what the application should do before asking what it currently does
- Extract business rules and data from the existing solution without
  inheriting its design decisions
- Prioritize clarity, maintainability, and scalability over familiarity
- Design with the future web portal in mind from the beginning
- Species and areas are data, not structure — no per-species or per-area
  fields, ever
- Facts are stored; derivations (pounds, income, change percentages) are
  calculated
- Imports never destroy history; every imported row carries provenance;
  every imported source file is archived. ARCHIVE FILES is the byte-level
  backstop: one row per distinct file version, bytes embedded in the
  container and SHA-256-verified against the recorded checksum. Rows link
  to an import session when a session used the file (key_ImportSessions);
  the recurring-import spine also archives changed files no session has
  used yet — those rows carry an empty session key by design. Files never
  downloaded into the system live in the project folder's ignored areas,
  not in the database.

FUTURE DEVELOPMENT PLANS

- Web portal: a companion customer-facing portal in Next.js / React,
  after the redesign is complete. Subscription-based access.
- Integration: OData is the portal API surface (record:
  IFQ_Tracker_API_Surface_Decision_Phase5.md). The schema is the public
  API; field names are contractual. Derivations are computed in the
  portal layer from stored facts; OData webhooks drive cache
  invalidation, with import-script-initiated revalidation as the
  dependable primary.
- Timeline: no hard deadline — quality and maintainability take priority
  over speed.

DEVELOPER CONTEXT

- Experience: original developer, ~12 years of FileMaker development.
- Team: solo on this redesign; a second team member is allocated
  elsewhere.
- AI role: Claude is the analysis and design partner — domain analysis,
  schema/relationship review, naming standards, mining the existing
  scripts for business logic, proposing structures, building and testing
  the migration pipeline against samples, and documenting findings and
  decisions. See WORKING PACE for how to read short developer replies.

TECHNICAL ENVIRONMENT

- FileMaker Pro: 2026 (v26)
- FileMaker Server: 2026 (v26), hosted with SSH access and a custom domain
- Server OS: Linux (a move to Windows Server remains possible; no macOS
  anywhere in the environment). The server clock runs one hour ahead of
  the developer's local time — schedules and server-side timestamps
  (heartbeat, check rows) are server time.
- Server-run scripts stay OS-portable (standing rule): no plugins, no OS
  scripting steps (AppleScript/Send Event), no server-side file paths —
  bytes move URL→container; cURL/ExecuteSQL/CryptDigest/JSON are the
  platform-neutral toolset. A server OS move is then install + host +
  recreate schedules, zero script changes. (Client-side scripts run from
  the developer's Windows machine — e.g. the migration binary loads via
  filewin:/ paths — are exempt; they never run on the server.)
- Server schedules run under the "Server Automation" account (its name
  stamps Log_CreatedBy on schedule-created rows).
- Solution structure: single file. Hosting: FileMaker Server (not Claris
  Cloud). Current users: 2–3, growing to approximately 20.
- Python: 3.12.10 on the dev machine (installed July 2026;
  reportlab/pillow/pypdfium2), plus the side computer. Runs the
  migration/backfill pipeline and generates collateral (e.g. the Marketing\
  handout PDF).
- HOSTED FILES (twins minted at v1.39, July 31 2026, at a CERTIFIED
  baseline). TWO hosted files, one schema line: `IFQ Tracker Dev` — the
  working/build file, whose domain data is disposable churn; every
  harness run, TestConfig, and Data Viewer query belongs here
  (WINDOW-TITLE CHECK before every run — wrong-file slips are a proven
  hazard with twins). `IFQ Tracker Live` — the import-idle
  disaster-recovery twin; not opened in normal work. Both bank daily
  (Dev 05:00, Live 05:10) and both stamp check runs natively. The
  baseline they share was minted from the v1.38 migration rehearsal
  (Dev's schema over Live's pristine data via the Data Migration Tool)
  and CERTIFIED at v1.39: VERIFY suite clean (standing HELD coverage
  red only), census exact (2026 current certs 10,829, GroupUnits fully
  stamped), archive byte-proof 330/330 (VERIFY | Archive Checksums),
  B-green trio mirror exactly 10,829/0/0. Old Dev and old Live are
  decommissioned; their pre-migration backups (dated 7-30-26) live in
  Database Versions\. THE RELEASE PLAN, rehearsed once end to end and
  repeatable: at cutover the same operation mints production — clone
  Dev's schema, migrate Live's data via the DMT, certify, christen it
  `IFQ Tracker`; Dev lives on as the permanent development file.
  Because every file descends from one schema line, no code is ever
  pasted between them. The cutover checklist — carrying every
  rehearsal lesson (mandatory post-migration index-integrity probe;
  mandatory archive byte-proof; 509 repair shrunk to residue-verify;
  check-run backfill now inspect-and-verify) — is backlog item 1a.

DATABASE SCALE

- Existing primary table (OWNERS): ~244,000 records, ~8,000 per year;
  MANAGEMENTS: 32 records — one per year, 32 years of history.
- The replacement schema (18 domain tables; 28 in-file with GLOBALS,
  the five operations tables — IMPORT SOURCES, IMPORT CHECKS, IMPORT
  CHECK RUNS, NAV MENU, IMPORT PIPELINES — HOLDER JOURNAL, and the three
  staging tables) is defined in IFQ_Tracker_Schema_Design.md — the
  consolidated single-document schema truth — and fully built in the
  hosted file. (Its Forward Schema section is RETIRED as of
  v2.9: the slice-3 additions are built and live in the catalog.)
- The 32-year backfill is imported and verified; per-table migrated
  counts: IFQ_Tracker_Backfill_Build_Phase6.md §8/§10.

WORKING PACE AND CONFIRMATIONS

When the developer gives a short reply like "do what you think is best
next," "sounds good," or "go ahead," read it as trust in Claude's
judgment about the next action — NOT as a signal to go faster, skip
confirmation, or stop asking. The developer sets the pace, and asking is
always welcome. If a real decision or fork arises (a scope choice, a
design tradeoff, or anything Claude itself flagged as an open question),
Claude raises it even right after being told to proceed, rather than
resolving it silently to save a turn. Match the established working pace
rather than accelerating. Prefer plain language over project shorthand
when introducing a step the developer may not have seen named before, and
state milestones plainly when they are reached.

Information pacing: a full picture up front is welcome when it helps the
developer see the shape of something — the map before the walk. But work
through that picture in digestible chunks, one piece at a time,
confirming before moving on, rather than presenting a wall of detail to
act on all at once. This is a default, not a rule: genuinely
interdependent decisions should be kept together when separating them
would hide a dependency or force a choice without the context that
determines it. Coupled points get decided together; independent points
get worked one at a time. When in doubt, the developer prefers smaller
chunks while working, even if the overview was large.

Recorded decisions vs. better ideas (developer preference, July 2026):
locked/recorded rulings are CURRENT DECISIONS with provenance, not laws
— their job is to prevent accidental drift and silent re-litigation,
never to suppress improvement. If Claude has an idea that a recorded
ruling seems to forbid, the idea wins the airtime: surface it, name the
ruling it challenges, and let the developer decide. Frame rulings as
"current decision" rather than "hard rule" in prose.

The session map (adopted v1.19): at the start of every working session,
after alignment, Claude lays out a lettered map of the session's planned
work (A, B, C … with short two-or-three-word headers) and re-announces
each letter + header as that section begins. The decisions behind a
build are often days or weeks old by the time it runs; the map and the
re-announcements are the reminders that keep the work oriented.

Lettered stage references — the session map's A, B, C, and long-lived
labels like G — always carry a short descriptor on first use in a stretch
of conversation ("G — the orchestrator, IMPORT | Trio Import"), never a
bare letter. Claude holds the backlog in context and the developer does
not; the burden of recall belongs on Claude's side (developer request,
v1.31).

(These are preferences to apply with judgment, not rigid rules.)

EDIT PROTOCOL (in-FileMaker scripts, calculations, and file edits)

Every edit instruction names: (1) the script/file being edited; (2) the
LOCATION — for scripts, the Script Workspace step number PAIRED with the
verbatim step text as confirmation ("step 37: `Set Variable [ … ]`"; the
number finds it fast, the quote proves it's the right line — blank lines
and comments count as steps). NUMBERING (corrected v1.26; the v1.17 claim
that the two schemes agree was wrong): the snapshot XML's `index`
attribute is ZERO-BASED and the workspace gutter is ONE-BASED, so
GUTTER = SNAPSHOT INDEX + 1. Claude quotes gutter numbers; the paired
verbatim text is what makes the instruction land even when the number is
off by one (it did exactly that in the v1.26 phase-3 replacement); for
everything else, an ANCHOR — the verbatim text of a line that already exists in the
developer's copy, quoted exactly; (3) the action relative to that
location (insert after / replace / delete); (4) the complete new
content, paste-ready — whole calculations, never "add X to the existing
calc"; (5) what the result should look like. Multi-edit instructions to
one script run BOTTOM-UP so earlier edits don't shift later step
numbers. When Claude hasn't seen the current version of a script since
it changed, ask for a fresh structure snapshot rather than guessing at
its content. Blank steps in the developer's scripts are DELIBERATE
readability whitespace (v1.19): they count in numbering, and they are
never flagged as defects or proposed for cleanup.

CLIPBOARD DELIVERY (adopted v1.22; first-draft rules — refine with
use): for new-step insertions, Claude composes real script steps from
the FMClip schema library (Tools\FMClip\; 57 step types banked) and
loads them into the developer's paste
buffer via FMClip-Write.ps1 — the developer clicks the named location
in Script Workspace and pastes. The edit instruction still names
script + location per the protocol above, but the content arrives as
a SHORT SUMMARY of what the pasted block does, not the full step text
— the developer reads the real code in FileMaker after pasting
(Ctrl+Z reverts a bad paste). ONE BLOB AT A TIME: the clipboard holds
one payload, so each blob is delivered, pasted, and confirmed before
the next is loaded; every composed blob is kept as a file for the
session, so re-loading a lost blob is one command. The loop runs both
directions: the developer can copy any script and Claude reads the
clipboard (FMClip-Read.ps1) to verify the built state directly —
faster than a snapshot for one script. READ-BACK USES THE WHOLE-SCRIPT
COPY (v1.35): the developer copies the script from the SIDEBAR
(right-click → Copy), not its steps — the Mac-XMSC wrapper carries the
script name, so the read-back proves LOCATION as well as content (a
step-level copy diffs perfectly from the wrong near-name script;
Techniques §30). Chat delivery remains for
calculation-text replacements inside existing steps and whenever the
developer wants the code discussed rather than just placed.

LAYOUT OBJECTS ARE THE SIXTH SURFACE (v1.28): whole layout objects —
fields, text, rectangles, buttons with wired actions and icons, all
wrapped in a group — are composed and pasted the same way (Mac-XML2).
The rules are NOT obvious and every one was earned from a silent
failure: Tools\FMClip\Schema Library\Layout Objects\README.md is the
MANDATORY READ before composing any layout object, with
Tools\FMMockup\Calibration.md for the measured constants. Delivery
differs from script steps in two ways: the developer PLACES the pasted
group (Inspector Left/Top, then Ungroup) because paste never keeps the
composed origin, and READ-BACK VERIFICATION IS NOT OPTIONAL — the paste
parser silently discards what it does not accept, so every layout paste
is copied back and diffed. Whole-body blobs for structural iterations,
surgical mini-blobs for targeted fixes.

THE STRUCTURE SNAPSHOT (convention adopted v1.16; replaces script PDFs
entirely — pulling a fresh XML is faster than exporting one PDF): the
developer exports Save-a-Copy-as-XML from the Ottomatic server console
in under a minute (FMSaveAsXML from a clone: full schema, scripts,
layouts, relationships; no data), with the "Include Script Text"
checkbox ON — it adds the DDR_INFO cross-reference section (every
calculation re-rendered as typed chunks with resolved object UUIDs),
the standard for where-used audits. Snapshots land in Reference
Material\FileMaker Exports\Structure Snapshots\. LATEST = newest
modified time (console filenames vary; never trust the embedded name
date). The developer ANNOUNCES every new snapshot; Claude tracks what
changed in the database since the last announcement and judges
freshness from that — confirm with one yes/no when unsure, never open
a session with an export request. THE SNAPSHOT IS THE FIRST STOP for
any question about the database's CURRENT BUILT STATE — which tables,
fields, scripts, layouts, or predicates actually exist right now;
never carry a built-state "unknown" into planning, or a
doc-vs-database discrepancy into an audit list, when the snapshot can
answer it in a grep (Design docs state intent; the snapshot states
what is built — v1.18, after a planning pass listed a
snapshot-answerable question as an unknown). The snapshot is working
reference,
never the durable record (the Design docs are), and is never committed
(Reference Material\ is git-ignored). Note: the export is UTF-16 —
convert before grepping.

FILEMAKER EXPORTS (convention adopted v1.16): everything exported FROM
the database lands under Reference Material\FileMaker Exports\ —
Structure Snapshots\ (above) · Archive File Exports\ (files exported
from ARCHIVE FILES containers; the EXPORT | Archive Files script is
backlogged, build on first need) · Data Exports\ (record-data xlsx
slices; name them what-plus-date-range) · AI Reports\ (script-written
diagnostic bundles for Claude, and where TEST | Run's outputFile JSONs
land — e.g. Session AI Report.md). A SESSION'S runs sit at AI Reports\'
top level and the CLOSE-OUT moves ALL of them into AI Reports\Archive\,
so that top level is empty at the start of every session and everything
in it belongs to the session in progress (developer ruling, v1.32 —
standing close-out procedure; wording corrected v1.37: runs land in AI
Reports\, not at the FileMaker Exports top level). The fixed-filename rule
is RETIRED: TEST | Run takes an optional `outputFile` in
GLOBALS::TestConfig (default `Test Result.json`), so several runs can be
read together instead of each overwriting the last; a name containing a
path separator or `..` is REFUSED rather than silently redirected (built
v1.32, backlog 4g closed). Two rules: exports are COPIES for
inspection, never reference truth (the database is the truth they
drifted from); and exports bound for Claude never pass through Excel —
export straight to disk and hand the file over untouched (Excel strips
leading zeros and rewrites the bytes; a July 2026 incident corrupted a
set of archive-file copies exactly this way).

BUILD STANDARD (v1 — evolving)

1. Readable names. Variables, fields, and object names in every draft use
   full words ($rowsRemoved, never $n) — including calculation-scoped Let
   names. Existing terse names change only when the script/calc is
   already being touched.
2. Mockup first, always. No layout building without an approved mockup.
   Approval marker: the developer saves it to the mockup folder
   (Documents\Design\Layout Mockups\, replacing any superseded version);
   the saved mockup is the build spec, and design changes re-mock before
   re-building. Every mockup ships with a field-bindings table in chat —
   annotations never live inside the mockup. Presentation idioms:
   ghost-plus-backdrop-plus-notes-strip for card mockups; plain canvas
   for full layouts.
3. Complete build instructions. Layout work arrives as numbered steps in
   build order, including: the layout part list with heights; exact
   Inspector positions and sizes wherever geometry matters; fonts
   (face/size/color); fills and borders; button specs with all states
   (normal/hover/pressed — colors, icon, icon size); conditional
   formatting as ordered tables; hide conditions and tooltips verbatim;
   the bindings table. The instruction FORMAT itself — the
   object-type-first entry template, style-before-position ordering,
   styles defined up front, icon section dividers, chunked delivery —
   is specified in IFQ_Tracker_Build_Instruction_Standard.md (born
   v1.16 from build-session feedback); every layout build follows it.
   MANDATORY READ (v1.17, after a skipped-read incident; widened
   v1.18 after a second): Claude reads BOTH that doc AND
   IFQ_Tracker_FileMaker_Techniques.md IN FULL before delivering any
   build instructions in a session — the Standard for format, the
   Techniques doc for the platform's recipes and traps. Being cited
   here is routing, not a substitute for reading.
4. Styles over raw formatting. Recurring treatments are saved as NAMED
   STYLES in the file's custom theme — named Slate (renamed v1.16) —
   and applied by name; raw values appear in instructions only when
   DEFINING a style. The first occurrence defines and names one, every
   later instruction says the name ONLY (the Link Action style,
   Techniques doc §9, is the model). Style NAMING (v1.17): new styles
   carry their OBJECT KIND in the name (Filter Button, Link Action
   Button, Day Band Label) and a name never repeats across kinds —
   FileMaker styles are per-object-kind, so a bare name can point at
   the wrong picker (the Link Action incident). The pre-v1.17 library's
   retrofit rename rides the naming-convention session.

NAMING CONVENTIONS

- Tables: ALL CAPS with spaces, plural. Example: QUOTA SHARES, ANNUAL
  LIMITS.
- Fields: PascalCase, singular. Example: QuotaShare, PermitHolder.
- Keys: key_Primary — primary key; key_AnchorTable — foreign key to that
  table's key_Primary, where AnchorTable is the related table's name in
  PascalCase with plurality preserved (HOLDERS → key_Holders, IMPORT
  SESSIONS → key_ImportSessions). The key_ prefix is reserved for
  surrogate foreign keys that join to a key_Primary UUID; a field that
  joins on a natural or external identifier (a NOAA GroupId, an NmfsId)
  is not a key_ field and takes a plain descriptive name (MatchedGroupId,
  NmfsId), so key_ stays a reliable signal of "UUID FK to a key_Primary."
  A SECOND FK to the same table with role semantics takes
  key_AnchorTable_Descriptor — a plain-English descriptor after an
  underscore, chosen to survive the future-blank test
  (key_ImportSessions_RetiredBy — ruling v1.18, borrowing the TO
  same-target-descriptor pattern; the naming session ratifies formally).
- Table Occurrences: Anchor-Buoy. Naming, color, and graph-layout rules
  are fully specified in TO_Conventions.md — the single source of truth
  (TO names are internal and non-contractual, unlike field names).

Log Fields — standard audit fields on every table, named exactly:
Log_CreatedBy, Log_CreationTimestamp, Log_ModifiedBy,
Log_ModificationTimestamp.

FIELD BUILD CONVENTIONS

- Booleans: stored as Number, 1 = true / 0 = false (FileMaker has no
  native boolean type). Names still use the Is/Has/Can prefix.
- Primary keys: key_Primary is Text, auto-enter Get(UUID), "do not
  replace existing value" ON, validated unique + not empty; indexing
  Minimal (v1.18 standard, all tables swept — the value index serves
  every join and find; a word index is dead weight on UUIDs).
- Staging tables are exempt from the key/Log conventions (a stated
  exception alongside GLOBALS): Text columns mirroring the source
  file's headers with underscores in place of hostile characters
  (QS_UNITS, COMPANY_LAST_NAME — v1.18 ruling; the header gate makes
  positional import mapping safe), plus SourceRowNum, SessionKey
  (deliberately not key_-prefixed — never related), and the adapter's
  computed key columns — transient scratch, cleared each run, never
  related, never on OData (shapes: Schema Design §3).
- Two separable auto-enter controls during migration (record consolidated
  into Schema Design v2.0; original Build Decisions doc archived in
  Documents\Design\Archive\):
  (a) the per-import "Perform auto-enter options while importing"
  checkbox stays OFF for EVERY staged import — permanently, including the
  Phase 7 historical backfills — so staged UUIDs and audit stamps pass
  through verbatim. (It is correctly ON for live-row seeding imports that
  carry no UUIDs/stamps of their own — e.g. the IMPORT SOURCES registry
  seed and the NAV MENU seed; Recurring Import Pillar doc §4.4.)
  (b) the field-level auto-enter definitions (Log fields, key_Primary)
  are held OFF during migration for domain tables (scripts stamp
  Log_Modified* where they modify rows) and turn back ON at the cutover
  audit once the remaining historical backfills are done. Stated
  exceptions carry them ON from birth: SPECIES ALIASES / AREAS ALIASES,
  and the operations tables (IMPORT SOURCES, IMPORT CHECKS, NAV MENU —
  IMPORT PIPELINES joins the ops class at birth) — ops tables never
  receive staged imports. Observed drift (ARCHIVE FILES, SETTINGS found
  ON — harmless; v1.21 adds HOLDINGS and HOLDING CHANGES found fully ON,
  and the Holdings Pass now RELIES on it — its creates and updates stamp
  nothing manually) makes the cutover item an AUDIT (backlog,
  HOUSEKEEPING).
- Import field mapping is by matching NAMES (staging headers match table
  field names), robust to column order; the saved import-step arrangement
  is still positional, so re-arrange by name whenever a staging shape
  changes.
- Further field-level rulings — value lists, enums, the migration/QA
  provenance fields, PipelineVersion, GLOBALS, the MANUAL HOLDINGS shape,
  and everything else field-level — are recorded in
  IFQ_Tracker_Schema_Design.md, the single schema document.

VARIABLES (scripting only)

$$GlobalVariable — global; $ScriptVariable — local script;
calculationVariable — calculation-scoped (lowercase camelCase, to read
differently from script variables).

Prefix conventions (scripts and calculations): is — Boolean flag
(isActive); has — possession/state (hasError); can — capability
(canEdit); should — suggested behavior (shouldRetry); to — target
(toProcess); will — future action (willRetry); num — numeric (numUsers);
str — string (strName); lst/arr — collection (arrItems). These prefixes
are a guide, not a mandate: use one when it adds clarity, drop it when
the bare name already reads clearly (a variable that mirrors a field is
clearest named after that field). The `str` prefix in particular is
optional and often better dropped. Keep prefixes (num, json, is/has/can)
where they carry real meaning the name doesn't already convey. (A fuller
variable-readability convention is queued to the naming-convention
conversation; see the backlog.)

Boolean gates in calculations (standing pattern): test booleans with
`field = 1` / `not ( field = 1 )` rather than `field = 0` — an empty
boolean field fails `= 0` but is handled correctly by the `= 1` test.
(For NUMBER booleans; a text-enum test like `AttentionState ≠ "OK"` is
safe against empty values as written.)

Calculation context (standing rule, learned v1.16): a calculation
inside a SCRIPT STEP (Set Variable, If, and the rest) has no home
table — every field reference must be fully TO-qualified from the
layout's context (IMPORT SESSIONS::Field; related fields through the
buoys). Bare field names are valid only inside Manage Database field
definitions, where they resolve against the defining table. Claude
writes every script-embedded calc fully qualified and paste-ready;
layout-object calcs also qualify fully (the Techniques doc §4 refresh
rule makes it load-bearing there).

ExecuteSQL patterns (standing): (1) never compare against '' in FileMaker
SQL — the engine treats an empty string as NULL, so
`X NOT IN ('OK', '')` is `NOT IN (…, NULL)` and matches NOTHING, silently
(three-valued logic); write `X <> 'OK'` and let NULL rows exclude
themselves. (2) On any calculation field whose formula is pure ExecuteSQL
(references no fields), UNCHECK "Do not evaluate if all referenced fields
are empty" — with zero referenced fields the condition is trivially true
and the field never evaluates; the Data Viewer masks this because it
evaluates expressions directly. (3) SQL returns timestamps as text
needing locale parsing — where a calculation needs timestamp arithmetic,
prefer a relationship read over ExecuteSQL; for DISPLAY, use the
DisplayTimestampShort custom function (ISO-aware — recipe:
IFQ_Tracker_FileMaker_Techniques.md §6).

Dialog posture (standing convention): hand-run admin scripts may report
via Show Custom Dialog; any script that can run server-side or unattended
reports via its JSON envelope and/or log rows ONLY — a server-side dialog
is silently skipped and the script continues. Design each script onto the
right side of this line at birth (the commit is the interactive shell;
its subscripts are dialog-free envelope-returners).

CUSTOM FUNCTIONS

Prefix CF_; group related functions with a topic segment after the prefix
(e.g. CF_VReport_SetResult / _SetRecordCount / _SetOverall). Parameter
names must not collide with built-in function names (use numRecords, not
count). Every custom function opens with a comment block stating
PURPOSE / INPUT / RETURNS / USED BY (the USED BY line is the CF
equivalent of a by-name call-site inventory — update it when a new caller
adopts the function).

LAYOUT AND SCRIPT NAMING

Layouts and scripts use a "Function | Specific" pattern with the pipe as
the category separator — the house separator already in use for
`Root Table | <TABLE>` layouts and `VERIFY | …` scripts. Examples:
`IMPORT | Prices` (scaling to `IMPORT | Landings`), `ADMIN | Import
Dashboard`, `FETCH | Download File`, `NOTIFY | Attention Changed`,
`TEST | Check Source`, `NAV | Go To`. Organize related layouts/scripts
into FOLDERS by function, but keep the functional prefix in the NAME as
well — `Go to Layout` and `Perform Script` steps show the name, not the
folder, so the name must be self-describing on its own. (FileMaker
permits duplicate script names — steps bind to script IDs — so a Perform
Script step whose label doesn't follow a rename is pointing at a
different script than intended; watch for stray duplicates. Near-identical
names are a proven miswire hazard — two live incidents in the v1.7
session; the naming-convention conversation in the backlog owns the fix.)

Subscript-as-function (standing pattern): a self-contained step is its
own subscript returning one JSON envelope ({ok, …payload | error}) via
Exit Script; the caller checks ok. No $$-global coupling. A subscript
that changes layout restores it (Go to Layout [original layout]) before
exiting — it must not move the caller's context. Callers write
cross-context state to variables and commit via explicit
navigate-and-find blocks; Insert-family steps (including Insert from URL)
require their target field to be ON the current layout, unlike Set Field.

Navigation (standing convention; record:
IFQ_Tracker_Navigation_Framework_Phase8.md): navigation is data-driven
through the NAV MENU table and the Navigate card (☰ Menu on every
designed layout's chrome bar); Home = ADMIN | Import Dashboard; Back is
contextual-workflow-only; Close is card-windows-only; root-table/utility
layouts get title + Home + Menu only; not every layout appears in the
menu (context-dependent layouts navigate through their workflow). Adding
a menu destination = adding a NAV MENU row + running NAV | Rebuild Menu
Cache; renaming a layout requires updating its NAV MENU row(s) — the Go
To trap makes a stale name loud.

UI OBJECTS

- Region buttons and fixed-region objects: UpperCamelCase, format
  [Location][Use][Object]. Example: HeaderNavButton, FooterSaveButton.
- Feature content objects (an object belonging to a feature rather than a
  screen region): [Feature][Purpose][Type]. Example:
  ImportPricesPreviewWeb. Use this form for content objects (web viewers,
  portals, named panels) tied to a specific feature.

FORMATTING NOTE (Claude output; rewritten v1.17)

The old never-emphasis-on-identifiers rule is RETIRED: live tests in the
Claude Code client (v1.17 session) showed neither emphasis-wrapped
identifiers nor bare dollar-sign pairs garble as LaTeX here — the
original incident belonged to the previous client. Identifiers may
render plain, in inline code, or bold; in build instructions BOLD marks
a copy target (the Build Instruction Standard owns the details). If
garbling ever reappears in a future client, the first remedy is putting
variables in backticks (inline code shields `$` from math parsing).

DATABASE MANAGEMENT

All objects in Database Management must be sorted alphabetically. Sorting
must be completed before hosting.

WHAT RIDES WHERE

Churning state → Documents\Status_and_Backlog.md. Session record →
Documents\Changelog.md. Durable detail → the record docs in
Documents\Design\. Durable conventions/orientation → this file. Platform
recipes/traps → IFQ_Tracker_FileMaker_Techniques.md. A line that would
duplicate a doc belongs in the doc.

WHERE THINGS LIVE (updated at the July 2026 reorganization)

- Record docs: flat in Documents\Design\; frozen/retired layers in
  Documents\Design\Archive\. Design artifacts: Documents\Design\Layout
  Mockups\ (Slate is current), Documents\Design\Flow Charts\ (SVGs + the
  Python generator scripts), Documents\Design\Graphics\ (button-icon
  SVGs in Icons\).
- The migration/backfill Python pipeline: Migration\ (Pipeline\ holds the
  current script + CHANGELOG; Archive\ holds prior versions; the run
  guide is README_local_pipeline.md). It hardcodes the "Import Files"
  folder name — never rename that folder.
- Dev tooling: Tools\FMClip\ (born v1.21; restructured self-contained
  v1.22 for sharing across projects/people) — the FMClip clipboard
  bridge: FMClip-Read.ps1 / FMClip-Write.ps1 move fmxmlsnippet XML
  between files and FileMaker's Windows clipboard formats (Mac-XMSS
  script steps and siblings; 4-byte LE length header + UTF-8 XML);
  FMClip-BankSteps.ps1 splits captures into per-step-type schema files;
  Schema Library\ holds one banked paste-ready example of each script
  step type the project uses (57 banked; Catalog.md is the index), plus
  per-surface grammar banks: Fields\ · Scripts\ · Layout Objects\ ·
  Tables\ (born v1.33 — the whole-table wrapper had been proven since
  v1.25 but never banked, so the first composed table had nothing to copy
  from) — the basis
  for CLIPBOARD DELIVERY (see EDIT PROTOCOL). Generic to any FileMaker
  project; git-tracked; the whole folder lifts as a unit. PROVEN LIVE
  ACROSS FIVE PASTE SURFACES (v1.25): script steps (Mac-XMSS), FIELDS
  (Mac-XMFD, into Manage Database ▸ Fields), WHOLE TABLES with all their
  fields (Mac-XMTB, into Manage Database ▸ Tables), WHOLE SCRIPTS
  (Mac-XMSC — name + all steps, into Script Workspace), and CUSTOM
  FUNCTIONS (Mac-XMFN, into Manage ▸ Custom Functions). SIXTH SURFACE
  PROVEN v1.28: LAYOUT OBJECTS (Mac-XML2) — composed from scratch and
  pasted, up to a whole layout body in one blob; its grammar bank is
  Schema Library\Layout Objects\ (README = the composition rules, the
  Slate style map, the template and specimens). THE WRAPPER DETERMINES
  THE FORMAT, and a mismatch pastes NOTHING, silently (learned twice in
  one session, v1.32): a `<Script>`-wrapped payload is Mac-XMSC, bare
  `<Step>`s are Mac-XMSS, `<Field>`s are Mac-XMFD. When a payload
  composed as a whole script is needed as a step-set, STRIP the wrapper
  — do not just change the format name (Techniques §30).
  Whole-script paste is the delivery vehicle for a NEW script; to
  replace an existing script's
  body while preserving its id (so harness/Perform-Script bindings
  survive), open it and Ctrl+A → Delete → Ctrl+V a Mac-XMSS step-set.
  COMPOSITION GRAMMAR BANKED (v1.27, every piece read-verified from a
  live copy rather than inferred): Schema Library\Fields\ — calculation
  fields stored+indexed and unstored, plain fields, global storage, and
  the dataType strings (Text, Number, TimeStamp — capital S);
  Schema Library\Scripts\ — the Mac-XMSC whole-script wrapper; and in
  the step files, all four Go to Layout destinations plus by-name
  dispatch on all three Perform Script variants (a <Calculated> element
  REPLACES the <Script> reference; the step-level <Calculation> stays
  the parameter).
  Caveats: pasting a table AUTO-CREATES a table occurrence (rename it
  per TO_Conventions); table occurrences and relationships NEVER paste —
  hand-built, using the 🔗 relationship-build format (Build Instruction
  Standard §17); a CF pasted with parameter names colliding with
  built-ins (bare first/last) arrives as a NAME-ONLY SHELL, silently —
  collision-safe names always. THE GENERAL RULE behind that class
  (Techniques §29): an unrecognized enum is SILENTLY DEFAULTED, never
  rejected, and can take neighbouring content down with it — three
  instances in one session, across a layout destination, a field
  dataType, and storage attributes. Never guess an option value; have
  the developer configure one real example, copy it, read it, bank it.
  And VERIFY EVERY PASTE BY READ-BACK (copy the pasted object,
  FMClip-Read it) — the paste's silence is not confirmation.
- Tools\FMSnapshot\ (born v1.34) — deterministic READING of the
  structure snapshot and clipboard captures: Render-Script (both XML
  dialects → one canonical step-text format), Get-Tables / Get-Fields
  (full definitions incl. auto-enter and validation), Get-Callers
  (reference AND calculated-dispatch binding — a reference-only search
  lies), Get-Headers (every script's header as one report). Its README
  is the dialect map and trap list — the two XML dialects differ, and a
  renderer for one silently garbles the other. THE RULE IT ENFORCES:
  parsing is a script's job, not a session's — ad-hoc XML reading is how
  a finished script got misreported as half-built (v1.34). Snapshots are
  read via a UTF-8 cache the tools maintain (_utf8cache\, regenerable).
  Script headers follow the SCRIPT HEADER STANDARD (Build Instruction
  Standard §18).
- Tools\FMMockup\ (born v1.28) — the HTML-mockup → FileMaker-layout
  converter: an approved mockup becomes a spec table, the spec becomes
  an emitted Mac-XML2 blob (Emit-LayoutObjects.ps1), the paste is
  read-back-diffed against the spec, and every caught delta becomes a
  rule. Its README carries the BUILD LOOP followed on every layout
  build; Calibration.md carries the measured constants and the
  unmappable list; References\ holds a published third-party spec of
  the format (CC BY 4.0 — reference only, verify on first use).
  Depends on FMClip for delivery. Developer ruling at birth: it grows
  INSIDE IFQ builds, one layout at a time, never as its own project;
  near-term bar is "the developer refines minutes, not hours."
- NOAA source data: Import Files\Originals\ (canonical, read-only);
  Import Files\Staging\ (regenerable pipeline output); seed CSVs
  (IMPORT_SOURCES_seed.csv, nav_menu_seed.csv) in
  Import Files\IMPORT SOURCES\.
- The FileMaker scripts, layouts, and calcs themselves (PRICES importer,
  Holder Detail cluster, spine FETCH/IMPORT/NOTIFY/TEST families, ADMIN
  surface, NAV family, VERIFY suite, Session Detail card) live in the
  hosted database file (see HOSTED FILES) — NOT in this folder. Their
  designs/records are the Design docs; the code is retrievable as the
  Save-a-Copy-as-XML structure snapshot (see EDIT PROTOCOL).
- Database exports: Reference Material\FileMaker Exports\ — the one
  home for everything exported from the database (structure snapshots,
  archive-file exports, data exports, AI reports; the convention is in
  EDIT PROTOCOL). Git-ignored; regenerable; copies never truth.
- Marketing\ — go-to-market home (tracked; born July 2026):
  Go-To-Market.md is the business/market record (target buyers, pricing
  read, leads, outreach log); Collateral\ holds reusable customer-facing
  output (the IFQ Tracker Overview handout PDF + its reportlab generator).
  NOT the same as Business\ (human-only contracts/billing, NEVER TOUCH).
- DS-05 (TAC pools) PDFs ARE in ARCHIVE FILES — corrected v1.33, the
  prior wording said they were not stored in the database and read as the
  opposite of the truth. The whole backfill set plus a 2026 DS-06 file are
  rows in the table, held in FileMaker's EXTERNAL CONTAINER STORAGE (the
  bytes live outside the .fmp12, still managed by FileMaker); "external
  storage" is almost certainly what the old note was garbling. The
  separate observation that the 2026 file's dotted filename causes trouble
  is left standing but UNVERIFIED — nobody has re-tested it. The on-disk
  originals under Import Files\Originals\ remain the canonical source.
- The plain-language "How the Holdings Import Works" walkthrough lives on
  the developer's Notion (shipped v1.11, one-and-done); its section list
  is recorded in the Trio Adapter Design doc §11.

NEVER TOUCH

Deny rules in .claude\settings.json block Edit/Write on these paths; this
convention additionally covers Bash/PowerShell, which deny rules do NOT
intercept — never write to, move, rename, or delete anything under these
paths by any means:

* Database Versions\ — the live database build (IFQ Tracker 0.5.2.fmp12).
* *.fmp12 files anywhere in the project.
* Import Files\Originals\ — canonical NOAA source files; the migration
  pipeline READS them, nothing ever writes them. (Import Files\Staging\ is
  regenerable pipeline output; Import Files\Home Server Copies\ holds
  home-server pulls, some irreplaceable — NOAA stopped publishing them.)
* Import Files\Import Files backup.zip — the insurance copy of Originals +
  Home Server Copies with original timestamps. Never modify or delete.
* Archive\ — human-only archive (legacy runtime solution, old databases,
  user manual). The AI never needs anything here.
* Business\ — contracts, agreements, billing (human-only).

GIT AND IGNORED FILES: git is the archive — deletion in a close-out commit
is archival. But ignored/untracked files (everything in the ignored areas
above) are NOT protected by git: deleting one requires an explicit
developer ruling first, every time (AI PM hard rule 3 rider).

ORIENTATION (spine record docs; all in Documents\Design\)

* IFQ_Tracker_Recurring_Import_Pillar.md (v2.7) — THE recurring-import
  pillar base: the two-plane pipelines model, the spine, ops tables,
  scheduling, attention, provenance, sessions, rollback, verify, and the
  admin surface — born at the v1.13 consolidation from the nine-doc
  pillar chain (all retired; recoverable from git). Companion:
  IFQ_Tracker_Trio_Adapter_Design.md (v1.12), the standalone slice-3
  design.
* IFQ_Tracker_Schema_Design.md (v2.15) — THE authoritative schema, read
  alone, no layering required; its Forward Schema section is RETIRED (the
  slice-3 deltas are built and live in the catalog).
* IFQ_Tracker_Build_Instruction_Standard.md (v1.8, born v1.16) — the
  format contract for layout build instructions (the Build Standard's
  point-3 companion; a MANDATORY READ before delivering build
  instructions).
* IFQ_Tracker_Verification_Architecture.md (v1.0, born v1.30) — THE
  verification record: what the standing integrity suite should be — its
  report contract, persistence, and surfaces. Read it before touching any
  `VERIFY | …` script. AMENDS the Pillar's §12.4 suite-upgrade
  description; the trio's own in-transaction verify stays Trio Adapter
  Design §3.7. Its thesis generalizes past this suite: a verification
  result that reports only its verdict is unfalsifiable — every check must
  report what it examined.
* IFQ_Tracker_Instructions.md (v1.7) — pre-migration instructions file.
  Its conventions/domain content now lives in THIS file (mined July
  2026); it remains AUTHORITATIVE for the AMENDMENT CHAIN (who amends
  whom) and the annotated key-documents catalog until the queued
  chain/catalog migration pass (unblocked by the v1.13 consolidation)
  moves them out. Its session procedures are superseded by AI PM 3.0.
* IFQ_Tracker_Changelog.md — pre-adoption history (through v1.11).
  Post-adoption history: Documents\Changelog.md.
* IFQ_Tracker_Project_Status_and_Backlog.md (v2.13) — the frozen
  pre-adoption backlog record (settled-decision digests live there). The
  LIVE backlog is Documents\Status_and_Backlog.md.
* IFQ_Tracker_FileMaker_Techniques.md — platform recipes and traps.
* TO_Conventions.md — table-occurrence naming/color/graph rules.
* Claris_FileMaker_Docs_Index_IFQ_Subset.md — FIRST STOP for a platform
  DOCUMENTATION question, before any web search (convention v1.26, after
  a session searched the web for a rule this doc already routed to). It is
  the Claris help index curated to this project, English only, and every
  entry links the markdown rendering of the page; any page not listed
  follows `https://help.claris.com/markdown/en/pro-help/<slug>.md`, and
  the live index (`https://help.claris.com/llms.txt`, also served as
  llms-full.txt) is re-fetchable. Deliberately NOT stored as a file —
  Claris regenerates it, and a stale local copy is worse than the link.
