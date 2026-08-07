Random Creation — Development Lifecycle

Authoritative for HOW THIS PROJECT IS WORKED AND SHIPPED: what
is stored where and why, how a session becomes a commit, how a commit becomes a release,
and where git, GitHub and Visual Studio fit. It is not authoritative for the app's design
(that is RandomCreation_ProjectContext_v3_0.md), for code-level engineering knowledge (that
is RandomCreation_EngineeringNotes.md), or for AI PM's own procedure (that is
Documents\AI PM\). Written at the v3.2 close-out, from a teaching session held at the
developer's request — they wanted to learn the scheme rather than be handed one, so this
doc carries the reasoning and not only the conclusions.

v1.1 (v3.3 close-out) closes the open questions v1.0 left parked. Section 4 records that
the commit rewrite and first push are done and adds the automated release pipeline; section
6's portable/installed fork is decided rather than described; section 7's sample-content
ruling is revised to a conditional install; and a new section 8 draws the program-files
versus user-data line the installer forces the project to make explicit.

--------------------------------------------------------------------------------
1. THE ORGANIZING TEST
--------------------------------------------------------------------------------

One question decides almost every storage decision in this project:

    IF THIS FILE VANISHED, COULD I GET IT BACK BY PRESSING BUILD?

Files a machine can regenerate are DISPOSABLE — they live wherever the tools put them and
version control ignores them entirely. Files that exist only because a human made them are
PRECIOUS — tracked, backed up, and holding exactly one home each.

| Kind | What it is | Regenerable | Git tracks it |
|------|-----------|-------------|---------------|
| Source | The C# and XAML | No — it is the origin | Yes |
| Records | Design docs, screenshots, changelog, AI PM files | No — written by hand | Yes |
| Build output | bin\, obj\, .vs\ | Yes, in ~6 seconds | No |
| Releases | A build frozen, named and zipped | Yes, from a tagged source version | No |
| Runtime data | The data\ folder the app writes | No — but it belongs to the USER | No |

Two consequences carry weight beyond the table:

PRECIOUS THINGS GET EXACTLY ONE HOME. Not one home plus a backup copy plus an older copy
elsewhere. A second copy is a bug, not insurance — git is the insurance. This project
arrived (July 2026) carrying five overlapping source trees from exactly that habit, and
the v3.1 file-organization pass existed to undo it.

GIT CAN ONLY INSURE WHAT IT WAS PRESENT FOR. History starts 2026-07-31, at v3.0. That is
why docs\v1.0\ and docs\v2.0\ were NOT redundant and were rescued to Documents\Archive\
rather than deleted: they are the only surviving copies of pre-git source. The v2.0 release
zip contains no source at all. Verified at the v3.2 close-out.

--------------------------------------------------------------------------------
2. WHERE THINGS LIVE
--------------------------------------------------------------------------------

| Path | Holds | Tracked |
|------|-------|---------|
| Source\RandomCreation\ | The only live source. Solution + project | Yes |
| Source\...\SampleData\ | Sample content shipped with releases | Yes |
| Documents\Design\ | Design records, screenshots, icons | Yes |
| Documents\Archive\ | Pre-git v1.0/v2.0 source snapshots | Yes |
| Documents\AI PM\ | The method; Config\ is this project's state | Yes |
| Documents\ (root) | Handoff, backlog, changelog | Yes |
| .github\workflows\ | Build automation (not yet created) | Yes |
| Releases\ | Local release archive, ~780 MB | No — ignored |
| bin\, obj\, .vs\ | Build output and IDE scratch | No — ignored |

--------------------------------------------------------------------------------
3. THE CYCLE
--------------------------------------------------------------------------------

    PLAN      session -> design docs + backlog updated         -> commit -> push
    BUILD     session(s) -> code into Source\ -> developer runs and tests
              iterate, possibly across many sessions           -> commit -> push
    RELEASE   bump AssemblyVersion -> finalise changelog.txt -> tag
              automated build -> attach to a GitHub Release    -> users download
    REPEAT

PUSH IS NOT A RELEASE STEP. This is the single most important correction to the intuitive
model. They are unrelated acts:

* PUSH sends commits to GitHub. It happens constantly — every session, every close-out. It
  is the off-machine backup. The repository lives on a K: drive; until a push happens, that
  drive is the only thing standing between the project and total loss.
* RELEASE deliberately declares "this exact state is v3.2" and attaches downloadable files
  to it. Rare and intentional.

Treating push as a release-time act means weeks of work sitting unbacked-up.

TAGS are what make a release reproducible. A tag is a permanent label on one exact commit
(v3.2). It is what allows "rebuild precisely what that user is running" a year later, and
it is what an automated build listens for. Without it, "the version people downloaded"
becomes guesswork.

TWO STEPS BELONG JUST BEFORE THE TAG, both easy to forget and both user-visible:
bump AssemblyVersion / FileVersion in RandomCreation.csproj (the About screen reads it from
the assembly at runtime), and finalise the version heading in changelog.txt.

BRANCHES ARE DELIBERATELY NOT ADOPTED. Real teams branch per feature and every tutorial
pushes it. This is one developer on a personal tool: working directly on main is correct
here, and branches would add ceremony that buys nothing. Revisit only if someone else ever
contributes, or if two features need to be in flight at once. This is a current decision,
not a rule.

THE DEVELOPER IS THE REVIEW MECHANISM. They do not read C#, so running the app is their
only real way to verify a change. A build server proves code compiles; it cannot prove a
GUI works. The standing proof is BUG 1 — shipped v3.0 loads v2.0 theme dictionaries and
compiles perfectly. Protect the developer's ability to click through a running build.

WHEN THE AI DRIVES THE APP ITSELF (UI automation — sending clicks and keystrokes): ask the
developer for hands-off first. Input sent while they are actively using the machine can
silently land in their windows instead of the app's. (Routed here from the v3.5 handoff at
the AI PM 4.0 upgrade.)

--------------------------------------------------------------------------------
4. GITHUB
--------------------------------------------------------------------------------

Set up at the v3.2 close-out session. Repository: https://github.com/akcama/random-creation

VISIBILITY: public. Chosen because GitHub Actions and GitHub Pages are both free for public
repositories, and because release downloads from a PRIVATE repository are not publicly
accessible — which would defeat the point of shipping.

COMMIT IDENTITY: 311688069+henry-akcama@users.noreply.github.com, set globally on this
machine. GitHub's private-address alias, adopted so a public repository does not publish
the developer's real address. The real address is appropriate when an employer requires it,
when the project lives outside GitHub (where the alias attributes to nobody), or when the
author wants to be contactable — none of which apply here.

AUTHENTICATION: Git Credential Manager, already installed. Authorised at the v3.3 session:
one browser round trip, token stored in the Windows vault, silent thereafter. The OAuth
consent asks for repositories, workflow and gists — broader than one push needs, because
GCM cannot know in advance what git will be asked to do; reviewable and revocable under
GitHub Settings → Applications. The token never passes through the AI.

DONE AT THE v3.3 SESSION: the 10 pre-adoption commits rewritten to the noreply address via
git filter-branch (safe only because nothing had been pushed — rewriting published history
is what breaks repositories), verified three ways (all 11 commits on the noreply address,
tree hash identical before and after, GitHub's tip hash matching local), then the first push,
then refs/original deleted and the old objects garbage-collected. The real address exists
nowhere in the repository.

STILL TO DO: GitHub Actions. Pages is deliberately deferred past v4.0 — see below.

GITHUB RELEASES replaces the Releases\ folder's distribution role. Release attachments
allow up to 2 GB per file, so the ~70 MB zips are fine; the 100 MB limit applies only to
files committed into the repository itself.

RELEASES IS ALREADY A DOWNLOAD PAGE, and this is the correction worth stating plainly
because it is easy to assume otherwise: publishing a release gives GitHub-hosted downloads
and a permanent "latest release" address that never changes. No Pages site is required to
distribute the app. PAGES IS POLISH — a branded landing page with screenshots — and is
therefore scheduled AFTER v4.0 ships, when there is a real download to point at and
corrected screenshots to use.

THE AUTOMATED RELEASE PIPELINE, the shape being built toward:

    push a tag (v4.0)
      -> GitHub Actions runs on a Windows machine
      -> dotnet publish
      -> zip the output          = the portable download
      -> run the Inno Setup script = the installer download
      -> create a GitHub Release, attach both

THREE CONSEQUENCES THAT CHANGE HOW THINGS ARE WRITTEN, starting now:

* THE BUILD MUST COMPLETE WITH NO HUMAN INTERACTION. Nothing that works only because
  Visual Studio is open, and nothing that depends on a file existing on the developer's
  machine.
* THE INSTALLER SCRIPT LIVES IN THE REPOSITORY, versioned like source.
* THE VERSION COMES FROM THE TAG. v1.0 of this doc had the developer hand-editing
  AssemblyVersion before tagging — a step easy to forget, and forgetting it ships a build
  labelled wrongly. The workflow reads the tag instead, so the tag is the single source of
  truth and the two-steps-before-the-tag rule below reduces to one: finalise changelog.txt.

THE README NEEDS NO PER-RELEASE EDIT: download links written once against the permanent
latest-release address keep working forever.

RETIRING THE LOCAL Releases\ ARCHIVE (781 MB, git-ignored, therefore unprotected). Once the
pipeline exists, GitHub holds every release and the folder never needs to grow again. For
what is already in it, one wrinkle matters: git history starts on 2026-07-31 with the AI PM
adoption commit, while v3.0 SHIPPED on June 6 — so no commit represents the exact released
state, and a retroactive v3.0 tag would be an approximation. That makes the v3.0 zip the only
exact copy of what the one live user is running, which is worth keeping. Plan: upload v3.0 to
GitHub as a historical release, upload or discard v1.0/v2.0 (their SOURCE is already safe in
Documents\Archive\, which is the precious part), then delete the local folder. NOT BEFORE a
release has been published AND downloaded back — proving the mechanism before deleting the
only local copies.

--------------------------------------------------------------------------------
5. LICENSING — PUBLIC IS NOT OPEN SOURCE
--------------------------------------------------------------------------------

PUBLIC means people can read it. OPEN SOURCE means they have been granted a licence to use,
modify and redistribute it. Only the second gives anything away, and it only happens by
explicit act.

Copyright is automatic and default-restrictive: code published with no licence file is "all
rights reserved" — nobody may use, copy, modify or distribute it. Silence is the RESTRICTIVE
option. The LICENSE file at the project root converts that legal default into an explicit
statement, which is what makes the intent unmistakable rather than merely implied.

THE ONE THING THAT WOULD GENUINELY HINDER SELLING is accepting code contributions. A merged
pull request means its author holds copyright on that contribution, and selling then needs
their permission. Companies solve this with contributor agreements; this project solves it
by declining code contributions, stated in the README. Bug reports and suggestions are
welcome — code is what carries copyright.

Two lesser caveats, recorded so they are not rediscovered: forks made while public are
permanent and do not reliably disappear if the repository is later made private; and
copyright protects this specific code, not the idea of a random-combination generator.

CODE SIGNING is acknowledged and deferred. Unsigned downloads trigger Windows SmartScreen
("Windows protected your PC"); a certificate runs roughly $200-400/year and still takes
time to build reputation. The README explains the warning to users instead, which reads as
confident rather than evasive.

--------------------------------------------------------------------------------
6. PACKAGING — THE PORTABLE / INSTALLED FORK
--------------------------------------------------------------------------------

The app is currently PORTABLE, and that is a load-bearing architectural fact:

    BaseDir = AppDomain.CurrentDomain.BaseDirectory;
    DataDir = Path.Combine(BaseDir, "data");

User data lives beside the .exe. That is why copying data\ moves a whole setup to another
machine, and it is why the .csproj deliberately excludes data\ from publish.

GITHUB ACTIONS CAN BUILD INSTALLERS — it runs a real Windows machine, so Inno Setup (.exe),
WiX (.msi) or MSIX all work. But an installer puts the program in C:\Program Files\, which
normal users cannot write to, so data\ beside the exe BREAKS.

Therefore "add an installer" is not a packaging choice. It requires relocating user data,
changing DataService, and handling existing portable users.

DECIDED AT THE v3.3 DESIGN PASS — the standing ruling that this gets worked through before
any building was honoured, and this is its output. The work lands in v4.0.

BOTH BUILDS SHIP. An installer .exe and a portable .zip, as two clearly-labelled downloads
on the same release. Not one installer offering a portable mode: the appeal of portable is
precisely that you do not run an installer. Both fall out of one dotnet publish.

PER-USER INSTALL, not per-machine. The usual reasoning ("nobody has two accounts on their
PC") is a reason per-machine would not HURT; the real argument is that a per-user install
lands in the user's own folder, needs no administrator rights, and raises no UAC prompt.
That matters more than usual here because the app is UNSIGNED — an unsigned installer that
also demands admin is a genuinely alarming thing to hand someone.

%LocalAppData%\RandomCreation\ for user data. Local rather than Roaming: this is one
person's content on one machine, not something that should follow a domain account around.

INNO SETUP. Free permanently, mature, script-based, well supported on GitHub's Windows
build machines, and it does everything needed: per-user install, a choosable install
location, an uninstaller, Start Menu entry, Add/Remove Programs registration. The
alternatives and why not:
* WiX (.msi) — free, but a steep learning curve and an enterprise deployment model that
  buys this project nothing.
* MSIX — genuinely nice and can auto-update, but sideloaded packages MUST BE SIGNED, and
  code signing is deferred. It defers with it.
* ClickOnce — dated, and gives poor control over install location.

THE INSTALL LOCATION IS CHOOSABLE, because people expect it. Consequence to expect rather
than be surprised by: the default is the user's own folder, and anyone who redirects it to
Program Files will then need admin.

USER DATA DOES NOT LIVE IN THE INSTALL FOLDER, and this was considered and rejected rather
than never raised. Keeping it there is superficially attractive — everything in one place,
exactly what portable gives you. Three problems killed it: the install location is
user-choosable, so data-beside-exe silently depends on nobody using that choice; UPGRADES
rewrite that folder by definition; and UNINSTALL becomes incoherent, because "keep my
collections" and "the collections are inside the folder being deleted" do not reconcile.
The contained experience is not lost — it is what the portable build IS.

THE UNINSTALLER ASKS whether to also remove collections, history and presets, and DEFAULTS
TO KEEPING THEM. The safe answer must be the one you get by clicking through without
reading: someone uninstalling in order to reinstall should not lose their content through
inattention.

BOTH BUILDS SELF-CONTAINED, at least to start. Framework-dependent publishing plus an
installer that downloads the .NET runtime would cut the download from ~70 MB to a few, and
it is a real option — but the PORTABLE build cannot bootstrap anything, so it would either
stay large anyway or fail confusingly on a machine without .NET. One build configuration,
no runtime detection, no download step that can fail, works offline. Revisit only if the
size actually bothers someone, and not while the pipeline itself is new.

A MARKER FILE DISTINGUISHES THE TWO. Same binary in both downloads, so the app decides at
startup: a small marker file beside the exe (shipped in the zip, absent from the installer)
means portable, and data lives in data\ beside the exe; no marker means installed, and data
lives in %LocalAppData%\RandomCreation\. About ten lines in DataService.

NO UPDATER IN v4.0. There is one live user, she is reachable, and building an update
mechanism to serve a single person you can phone is work obsolete before it runs once. The
options remain on the table for later, at three levels:

1. NO DEPENDENCIES — the app queries the GitHub Releases API at startup, compares versions,
   and offers a download link. Roughly 40 lines using HttpClient and System.Text.Json, both
   already in .NET 8. Fits this project's no-NuGet conviction. RECOMMENDED.
2. MSIX + App Installer — Windows handles updates natively from a hosted .appinstaller file.
   No code in the app, but it drags in the signing requirement above.
3. Velopack / Squirrel — full silent auto-update with deltas, but a real NuGet dependency
   in the application.

THE ONE MANUAL UPGRADE, and the trap in it. The single live user moves from portable to
installed by hand. The installed app looks in %LocalAppData%, finds nothing, and — correctly,
by the v4.0 design — starts fresh SILENTLY. From her side every collection she has built
would appear to have vanished, while actually sitting untouched in the old portable folder.
Her upgrade must therefore include carrying data\ across. The "Open data folder" button
added in v4.0 turns that into a drag between two Explorer windows.

WHY NOW WAS THE RIGHT MOMENT, recorded because the counter-argument was real. Shipping the
installer on top of an app with four known bugs means two unknowns at once, and a known-good
baseline would have made installer problems easier to diagnose. Weighed against that: one
user with no generations is the cheapest this data relocation will ever be, and every
additional user makes it more expensive. The developer took the risk deliberately, with
clean machines available for install testing.

--------------------------------------------------------------------------------
7. SAMPLE CONTENT
--------------------------------------------------------------------------------

Settled at the v3.2 close-out. The sample categories.json lives at
Source\RandomCreation\RandomCreation\SampleData\categories.json and the .csproj copies it
to samples\categories.json beside the exe.

IT MUST NEVER TARGET data\ UNCONDITIONALLY. changelog.txt used exactly that mechanism (and
moves out of data\ entirely in v4.0 — see section 8). categories.json is the opposite: it is
the user's entire content. A release that lands a file at data\categories.json destroys the
work of anyone who installs an update over their existing folder. Verified at the v3.2
close-out: a build shipping the sample left an existing data\ folder untouched.

REVISED AT THE v3.3 DESIGN PASS. The v3.2 ruling was that users copy the sample into data\
themselves. v4.0 replaces that with a CONDITIONAL install: on startup, if the data folder
has no categories.json, the app copies the sample across from samples\; if one exists, it
does nothing.

THE ORIGINAL RULING'S REASON SURVIVES INTACT. The rule was never "never write to data\" —
it was NEVER OVERWRITE USER CONTENT, and "only write when there is none" honours that
completely. What changes is that a new user gets a working example instead of an empty
shell. Recorded as a deliberate revision so it does not read as a forgotten ruling.

THE APP DOES THIS, NOT THE INSTALLER. An installer-side check would protect only the
installed build, and the portable zip has no installer to run one. One mechanism covers
both, is testable without building an installer, and lands in the startup code that v4.0
is rewriting anyway. It also sets the direction of travel the next section formalises: read
from PROGRAM, write to DATA.

It falls out correctly for every uninstall case — keep content, reinstall leaves it alone;
remove content, reinstall gets the sample; user deletes their own content, same again.

THE SAMPLE IS NOW THE PRODUCT'S FIRST IMPRESSION, which raises the stakes on its contents.
v4.0 replaces the current file — which was never designed as a sample, just whatever was
lying around — with a purpose-built one: three collections of visibly different kinds
(creature, starship, swords), only one enabled, one group and a couple of options disabled,
weights varied. A sample containing only creatures would teach that this is a monster
generator, which is precisely the assumption CLAUDE.md forbids the project to make.

Historical note: v3.0 DID ship a data\categories.json, but by accident of the release
process rather than by design — the data folder in the zip is timestamped after the build,
with empty history and presets files, the signature of running the app once and zipping
what appeared. There was no mechanism to preserve.

--------------------------------------------------------------------------------
8. PROGRAM FILES VERSUS USER DATA
--------------------------------------------------------------------------------

The installer forces a line the project has never had to draw, because until now everything
lived in one folder:

| Group | Where | Written by | Lifetime |
|-------|-------|-----------|----------|
| PROGRAM | beside the exe | the release | replaced every install |
| USER DATA | %LocalAppData%\RandomCreation\ (or data\ when portable) | the app and the user | never touched by an installer |

PROGRAM holds the exe, changelog.txt and samples\categories.json. USER DATA holds
categories.json, history.json, presets.json and settings.json.

changelog.txt IS CURRENTLY ON THE WRONG SIDE, and would break silently. The .csproj copies
it to data\changelog.txt and DataService reads it from there for the About section. That
works only while data sits beside the exe. Once user data moves to %LocalAppData%, the
installer would place changelog.txt in the program folder's data\ subfolder while the app
looked in %LocalAppData% — finding nothing and falling back to a built-in placeholder. The
About screen would quietly stop showing real release notes, with nothing failing loudly.
v4.0 moves it to the program group.

THE TEST for which side a file belongs on: WHO WROTE IT. Anything the author ships and
replaces every release is PROGRAM. Anything the user creates or edits is USER DATA. It is
the same question section 1 asks about git, applied to a running installation.

