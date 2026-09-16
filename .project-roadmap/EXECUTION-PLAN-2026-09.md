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

`fork-intel`'s `preapprove` gate, rebuilt. It stays self-contained in this repository: no dependency
on any external plugin, no MCP, just CLIs found on `PATH`.

The design below is the corrected one. Four adversarial reviews of the first draft overturned three
of its choices; where that happened it is said so, because the reasoning matters more than the
conclusion.

### Panel composition

Surveyed 2026-09-16. Criteria: the vendor trains its own frontier models, ships an official agentic
CLI, offers a flat subscription, and is usable from the EU.

| Family | CLI | Plan | Notes |
|---|---|---|---|
| Anthropic | `claude` | Pro $20/mo | stable; not unlimited-flat — top-up credits at API rates past quota |
| OpenAI | `codex` | in ChatGPT tiers, Plus $20/mo | stable |
| xAI | `grok` | SuperGrok $30/mo | **still officially beta**, per xAI release notes dated 2026-09-16 |
| Mistral | `vibe` | Le Chat Pro $14.99/mo | stable, fully agentic, **French company, EU-hosted, GDPR, Romania listed explicitly** |

Google is a candidate but not adopted yet: Antigravity CLI (`agy`) replaced the standalone Gemini
CLI and *is* covered by Google AI Pro, which corrects the earlier assumption that Google was
API-metered only. Confidence on that finding is medium and it carries a trap — Antigravity also
serves Claude and GPT-OSS models from the same Google quota, so an unpinned "Google vote" can
silently be Claude answering. If adopted, the model must be pinned explicitly.

Ruled out: DeepSeek (China, and no subscription at all — pay-per-token only), Qwen Code (free tier
discontinued 2026-04-15, EU purchase path unresolved), GLM Coding Plan (cheap, but no official
terminal CLI — it is a model you point an existing CLI at, so it is a model swap, not a fourth
vote), MiniMax (thin tool-calling CLI, not an autonomous agent), Baidu Zulu (impractical from
Romania), and every multi-vendor wrapper — Cursor CLI, trae-agent, Trae IDE.

### Prerequisite — measure before building

Two things must be established first, and neither can be assumed:

1. **Terms of service.** Whether scripted batch invocation of each subscription CLI, at this
   volume, is within that provider's acceptable-use terms. This is read, not guessed.
2. **Rate limits and quota sharing.** Pilot 50–100 candidates against each CLI in isolation and
   record: sustained calls/hour before throttling, the exact behaviour on cap-hit, and — the one
   that bites hardest — **whether the pipeline's quota is the same pool as the maintainer's own
   interactive use.** Five hundred judgements that burn the quota needed for actual work is a
   failure even if every verdict is correct.

### How the gate works

- **Round 1 — sealed ballots.** Three reviewers, independent, no cross-talk; verdict plus reasoning
  stored per candidate.
- **Reviewer rotation.** The family that ran triage never votes on that candidate. This is not
  hypothetical: `fork_intel.py` defaults triage to `--agent claude`, so today Claude triages and
  then votes on its own triage.
- **Partial blinding.** Reviewers currently receive a byte-identical prompt carrying the prior
  triage verdict — a shared anchor across every vote. At least one reviewer must get the raw diff
  with no triage summary, so one ballot is genuinely blind.
- **Truncation is a flag, not a detail.** The prompt truncates the patch. A candidate whose diff
  exceeds the limit goes to manual review regardless of the vote; nobody votes on a patch they were
  only shown part of.
- **Git-ancestry batching, in one stateless prompt.** *Corrected from the first draft, which
  proposed a persistent session per cluster.* A commit and its later revert must be judged together
  to be understood — but that needs one prompt containing both diffs labelled in order, asking for a
  single verdict on the sequence. A multi-turn session buys the same understanding and adds
  anchoring, drift, and a model agreeing with its own earlier conclusions. The cluster boundary is
  git ancestry, nothing looser.
- **Round 2 — only on a split, and a split means any non-unanimous vote** (2-1 counts, not just an
  exact tie). Each reviewer gets the other arguments **anonymised** — "another reviewer argues X",
  never "Grok says X" — in a **fresh** session. Anonymised so brand deference does not drive
  herding; fresh so no reviewer is defending words it already committed to.
- **Quorum: unanimous to pass.** Unanimous APPROVE passes the gate. Unanimous REJECT is recorded via
  `mark` so it stops resurfacing. A split goes to round 2; if round 2 reaches unanimity, take it;
  if it is still split, a human decides. For an import gate into a credential manager, a false
  APPROVE costs far more than a false HOLD.
- **ABSTAIN and ERROR are distinct from REJECT.** A missing vote — CLI down, rate-limited, timed
  out — never folds into an APPROVE. Fewer than two successful votes means the candidate is held,
  never approved.
- **Security-flagged candidates skip the panel entirely** and go to a human. No number of model
  APPROVEs substitutes for review of a security-relevant change.
- **Sample-audit unanimity.** A stratified slice of unanimous APPROVEs goes to a fourth family
  anyway, prioritised by the risk proxies the tool already computes. Without it, "unanimous and
  wrong" — a failure this project has already lived through — is invisible by construction. The
  point is to produce a measured disagreement rate, not a reassurance.
- **Honest labels.** With fewer than two independent families voting it is a second opinion, not a
  consensus gate, and the report says so.

**Done when:** the prerequisite measurements exist and are written down; the gate runs from this
repository against subscription CLIs; rotation, blinding and truncation-flagging are in place;
round 2 fires only on a split and only with anonymised arguments in fresh sessions; the unanimity
audit reports a measured rate; and the report's wording matches what the gate actually is.

## Notes that constrain all of the above

- The security tripwire is mechanical and blocking, and the automated pipeline never bypasses it.
  It fired twice on 2026-09-16 — once on harvested third-party patch text — and was right both times.
- The harvested fork cache is no longer tracked; decisions live in `fork-intel/EXCLUDE.json`.
- Attempt budget: at most two fixes on an unproven premise, then instrumentation, then a human.
