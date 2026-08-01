Random Creation — Development Lifecycle

Version: 1.0 — August 2026. Authoritative for HOW THIS PROJECT IS WORKED AND SHIPPED: what
is stored where and why, how a session becomes a commit, how a commit becomes a release,
and where git, GitHub and Visual Studio fit. It is not authoritative for the app's design
(that is RandomCreation_ProjectContext_v3_0.md) and not for AI PM's own procedure (that is
Documents\AI PM\). Written at the v3.2 close-out, from a teaching session held at the
developer's request — they wanted to learn the scheme rather than be handed one, so this
doc carries the reasoning and not only the conclusions.

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

--------------------------------------------------------------------------------
4. GITHUB
--------------------------------------------------------------------------------

Set up at the v3.2 close-out session. Repository: https://github.com/henry-akcama/random-creation

VISIBILITY: public. Chosen because GitHub Actions and GitHub Pages are both free for public
repositories, and because release downloads from a PRIVATE repository are not publicly
accessible — which would defeat the point of shipping.

COMMIT IDENTITY: 311688069+henry-akcama@users.noreply.github.com, set globally on this
machine. GitHub's private-address alias, adopted so a public repository does not publish
the developer's real address. The real address is appropriate when an employer requires it,
when the project lives outside GitHub (where the alias attributes to nobody), or when the
author wants to be contactable — none of which apply here.

AUTHENTICATION: Git Credential Manager, already installed. The first push opens a browser
once and is remembered thereafter. No tokens or SSH keys needed.

STILL TO DO: rewrite the 10 pre-adoption commits to the noreply address (safe only because
nothing has been pushed), first push, then GitHub Actions and Pages.

GITHUB RELEASES replaces the Releases\ folder's distribution role. Release attachments
allow up to 2 GB per file, so the ~70 MB zips are fine; the 100 MB limit applies only to
files committed into the repository itself.

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

Therefore "add an installer" is not a packaging choice. It requires relocating user data to
%AppData%\RandomCreation\, changing DataService, and migrating existing portable users. On
the backlog as its own item, with a standing ruling that it gets a detailed design pass
before any building starts. Portable-zip remains a legitimate permanent answer.

UPDATERS, at three levels:

1. NO DEPENDENCIES — the app queries the GitHub Releases API at startup, compares versions,
   and offers a download link. Roughly 40 lines using HttpClient and System.Text.Json, both
   already in .NET 8. Fits this project's no-NuGet conviction. RECOMMENDED.
2. MSIX + App Installer — Windows handles updates natively from a hosted .appinstaller file.
   No code in the app, but it drags in the installer question above.
3. Velopack / Squirrel — full silent auto-update with deltas, but a real NuGet dependency
   in the application.

--------------------------------------------------------------------------------
7. SAMPLE CONTENT
--------------------------------------------------------------------------------

Settled at the v3.2 close-out. The sample categories.json lives at
Source\RandomCreation\RandomCreation\SampleData\categories.json and the .csproj copies it
to samples\categories.json beside the exe.

IT MUST NEVER TARGET data\. changelog.txt uses exactly that mechanism and SHOULD be
overwritten by every release — it is the author's text. categories.json is the opposite: it
is the user's entire content. A release that lands a file at data\categories.json destroys
the work of anyone who installs an update over their existing folder. Verified at the v3.2
close-out: a build shipping the sample left an existing data\ folder untouched.

Historical note: v3.0 DID ship a data\categories.json, but by accident of the release
process rather than by design — the data folder in the zip is timestamped after the build,
with empty history and presets files, the signature of running the app once and zipping
what appeared. There was no mechanism to preserve.
