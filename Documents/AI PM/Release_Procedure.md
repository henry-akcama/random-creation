AI PM — Release Procedure

Version: 4.0 — August 2026. METHOD-OWNED: byte-identical in every project, replaced whole by an upgrade. MAINTAINER PROCEDURE — inert everywhere but the maintaining project; read in full when building a release. Reasoning lives in AI_Project_Manager.md.

THE RHYTHM. A release is the BUILD'S FINAL WORK UNIT: it lands as its own work commit with the developer's approval, mid-session, like every build unit before it — not inside a close-out. The session then closes out normally (the changelog's "MILESTONE: X.Y released" entry is the memory; the handoff points at the self-upgrade). Delivery to other projects follows whenever the developer chooses.

1. READINESS. The draft is complete in Source Code\ — which MIRRORS the shipped tree exactly (Source Code\.claude\commands\, Source Code\Documents\AI PM\, the Adoption Guide and Upgrade Notes at the root): package path IS install path, and assembly is a pure copy. The version is chosen (X.Y; Major on a declared milestone). The Manifest's header and file list are brought current, and this release's Upgrade Notes are written (see Manifest.md and the Notes' own three-section shape: config migration · project-file detect → propose pairs · warnings and one-time steps).

2. CROSS-CHECK SWEEP, mandatory, before assembly. Every drafted doc searched for stale version stamps, renamed terms, dead paths, and references to anything this release changed. The ripple list is reliably shorter than reality — the sweep is what closes the gap.

3. ASSEMBLY. Wholesale copy: Source Code\ → Releases\vX.Y\. No re-mapping, no per-file selection.

4. VERIFICATION — the right moment for byte-verify. The release tree byte-identical to Source Code\ AND matching the Manifest exactly, dot-entries shown.

5. COMMIT AND TAG. One work commit, approved by the developer: it ADDS Releases\vX.Y\ and REMOVES Releases\vX.(Y-1)\ — deletion in a commit IS archival; every past package stays recoverable from its tag, and Releases\ holds the current version only. Tag the commit vX.Y. When Push is on, the push explicitly includes the tag (a plain push moves no tags).

6. HAND-OFF TO DELIVERY. The maintaining project self-installs via /ai-pm-upgrade reading Releases\vX.Y\ directly. Other projects receive the package by hand into their Documents\AI PM\Upgrade\, whenever the developer chooses.
