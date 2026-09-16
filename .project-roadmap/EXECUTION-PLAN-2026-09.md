# Execution plan — September 2026

Derived from [UPSTREAM-ALIGNMENT-2026-09-16.md](UPSTREAM-ALIGNMENT-2026-09-16.md). Everything here
is about mRemoteNG and nothing else; see the scope boundary in [../CLAUDE.md](../CLAUDE.md).

Order is by what is hurting, then by what unblocks the most future work. Each item states what
"done" means, so it can be checked rather than believed.

---

## W1 — Answer jafin *(highest leverage, start here)*

Six pull requests open since 5–7 August, no maintainer reply. The fork radar independently ranks
this one contributor as the source of **84 of ~90** worthwhile candidates in the whole 1,709-fork
network. Ignoring him already cost us once: PR #156 went unanswered, we re-derived its fix at #176,
then closed it.

Tested locally against current `main` with `git merge-tree`:

| PR | Merges | Plan |
|---|---|---|
| #155 drop redundant `ZstdSharp.Port` refs | clean | read, build, merge |
| #159 restore tree state when filter cleared | clean | **first** — top Tier B, radar ties it to #149 |
| #157 keep active tab when close cancelled | clean | read, build, merge |
| #158 close PuTTY tab in one step | clean | read, build, merge |
| #154 zero build warnings | conflicts | likely superseded by our own 2026-08-13 cleanup — say so plainly, do not close in silence |
| #161 port scan single address field | conflicts | hold for W4; it collides with upstream turning port scan into a plugin |

Also open: two dependabot majors (Roslynator 4→5, Meziantou 2→3) from 09-14.

**Done when:** every PR has a maintainer reply naming what happens to it and why; the four clean
ones are merged or have a stated reason not to be; #154 and #161 have an explicit decision.

## W2 — The `ExternalConnectors` crash family

`System.IO.FileNotFoundException: Could not load file or assembly 'ExternalConnectors'` from
`ConnectionInitiator.OpenConnection` — **#175, #178, #191, #192**, the last two on 2026-09-16.

The easy explanations are already spent: the DLL is in the build output, in `publish\`, and in the
MSI, and the #120 MSI fix (`AllowSameVersionUpgrades` + `afterInstallInitialize`) is in
`Package.wxs`. So this needs evidence, not another guess.

Start with the evidence problem, not the crash: jafin's `04a6c9cfc9` fixes startup messages being
lost before the log writers attach — which is exactly why these loader failures arrive with no
usable diagnostics. Fix the logging first, then read a real trace.

**Done when:** we can state the mechanism from a trace, or we have shipped instrumentation and said
plainly that we could not reproduce it. The attempt budget applies.

## W3 — Decouple PortScan and SshTransfer behind an internal interface

Not the upstream contract — ours, internal. Upstream's `mRemoteNG.PluginContracts` is **two days
old** (first commit 2026-09-14, one of its three commits literally labelled "WIP"). Mirroring a
contract that its own authors are still reshaping daily buys nothing today: there are no
third-party plugins for either tree, so cross-compatibility is theoretical while the maintenance
cost of chasing their churn is real.

What is worth doing now is the half that is valuable regardless of what upstream settles on:
separate these two tool windows from the host behind an interface we control.

**Done when:** `PortScanWindow` and `SSHTransferWindow` talk to the host through an interface, with
tests, and the app behaves exactly as before.

**Re-evaluate upstream's contract in 4–6 weeks.** If it has stabilised, aligning to a fixed target
is cheap from this position. If it is still moving, we lost nothing.

## W4 — jafin's #161 lands on the decoupled port scan

Once W3 is done, his port-scan rework applies to the extracted component instead of the hardcoded
window. Resolves the collision with upstream's direction without adopting their code.

## W5 — Upstream radar

We watch the fork network but nothing watches upstream itself, which is how 525 unseen commits
accumulated and how their plugin work went unnoticed for two days. Same screen/triage machinery,
source = upstream.

**Done when:** a scheduled pass reports upstream commits worth attention, with the same tiering as
the fork radar.

## W6 — `CHARTER.md`

There is no document holding *why*. `CLAUDE.md` mixes constitution with manual and its own rule 16
forbids it from being a journal; `.project-roadmap/README.md` rotted (it still points at a deleted
`CURRENT_PLAN.md` and advertises v1.81.0-beta.2). Decisions taken today have nowhere to live.

Contents: what this fork is for; precedence when rules conflict; which rules are inviolable and who
may lift them (the tripwire is human-only, the attempt budget, never closing over an unanswered
"still broken"); a decision log with the reasoning and what would reverse each entry; how a rule
changes; and the posture toward upstream (direction, not history).

Constitutional rules move out of `CLAUDE.md`, which keeps the manual and points here.

**Done when:** the charter exists, `CLAUDE.md` no longer duplicates it, and
`.project-roadmap/README.md` stops lying.

## W7 — The review panel

`fork-intel`'s `preapprove` gate, rebuilt around the CLIs actually available on subscription. It
stays self-contained in this repository: no dependency on any external plugin, no MCP, just CLIs
found on `PATH`.

Design:

- **Round 1 — sealed ballots.** Independent reviewers, no cross-talk, verdict plus reasoning stored
  per candidate. Reviewers get the raw evidence, never a pre-formed conclusion: the framing channel
  is the contamination risk that crosses model families, not the family mix.
- **Round 2 — only on divergence.** Each reviewer gets the other arguments **anonymised** ("another
  reviewer argues X", never "Grok says X") in a **fresh** session. Anonymised so brand deference
  does not drive herding; fresh so no reviewer is defending words it already committed to.
  Continuing the original sessions would be cheaper but buys a reviewer anchored on its own verdict.
- **Final call is a vote, not convergence.** Agreement reached by deliberation is not evidence of
  correctness.
- **Sample-audit unanimity.** A small random slice of unanimous verdicts goes to an extra family
  anyway. Without it, "unanimous and wrong" — the failure mode this project has already lived
  through — is invisible by construction. This turns an assumption into a measured rate.
- **Security-flagged candidates skip the gate** and go straight to a human.
- **Honest labels.** With fewer than two independent families voting it is a second opinion, not a
  consensus gate, and the report must say so.

Open decisions: quorum (2-of-3 or unanimous), what a missing vote means when a CLI is rate-limited
(fail closed is the safe default), and the final panel composition pending the CLI survey.

**Done when:** the gate runs from this repository against subscription CLIs, round 2 fires only on
divergence, the unanimity audit reports a measured disagreement rate, and the report's wording
matches what the gate actually is.

---

## Notes that constrain all of the above

- The security tripwire is mechanical and blocking, and the automated pipeline never bypasses it.
  It fired twice on 2026-09-16 — once on harvested third-party patch text — and was right both times.
- The harvested fork cache is no longer tracked; decisions live in `fork-intel/EXCLUDE.json`.
- Attempt budget: at most two fixes on an unproven premise, then instrumentation, then a human.
