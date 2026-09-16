# Upstream alignment — intelligence, 2026-09-16

Gathered in one pass: our state, upstream's direction, how far apart the two trees are,
and what is sitting unanswered on the side. Written so the plan below can be argued with.

## 1. Where we are

| | |
|---|---|
| `main` | `e4a043d1c`, clean, pushed; CI green on all five workflows |
| Version | 1.83.0 — stable tag `v1.83.0` (2026-08-16), nightly rolling on HEAD |
| Tests | 6,842 passing |
| Open issues | 12 |

Waiting on the reporter: #182 (RDP memory, attempt budget used), #179 (External Tools),
#177 (splitter redraw, instrumented).

**A crash family is growing and nobody has worked it.** `System.IO.FileNotFoundException:
Could not load file or assembly 'ExternalConnectors, Version=1.0.0.0'` thrown from
`ConnectionInitiator.OpenConnection` — **#175, #178, #191, #192**, the last two filed today.
The DLL *is* in the build output, in `publish\`, and in the MSI (`_GeneratedFiles.wxs:354`),
and the #120 MSI fix (`AllowSameVersionUpgrades` + `Schedule="afterInstallInitialize"`) is
already in `Package.wxs`. So the packaging explanation is spent; the cause is somewhere else
and needs a real investigation.

## 2. Where upstream is

Default branch `v1.78.2-dev`, last commit **today, 2026-09-16 09:29**. 111 commits in
September alone. They are not dormant.

**They have caught up to us technically.** Upstream now targets
`net10.0-windows10.0.26100.0` — the same TFM as our fork. They have also adopted, after us,
VS2026 runner images, the MSBuild SDK resolution fix in the release workflow, skipping native
runtime checks for portable builds, and a WiX migration (they went to WiX v5; we are on the
WiX 6 SDK). Patch portability between the two trees is much better than it was in March.

**Their new direction is a plugin architecture.** Not a refactor — a re-shaping of what the
application *is*:

- new project `mRemoteNG.PluginContracts` (18 files) → `mRp.Contracts.dll`
- host side `mRemoteNG/Plugins/` (12 files): `PluginService`, `PluginContext`,
  `PluginCatalogEntry`, `PluginCompatibility`, `PluginConnectionAdapter`,
  `PluginProtocolMapper`, `PluginToolWindow`, `LegacyPluginDataMigrator`
- contracts: `IPlugin` (Id, DisplayName, Version, MinimumHostVersion, Initialize),
  `IPluginContext` (Connections, Messages, Resources), `IToolWindowPlugin`,
  `ITreeContextActionPlugin`, `IConnectionPropertyProviderPlugin`,
  `IConnectionAddressResolverPlugin`
- a **separate solution** `mRemoteNG.Plugins.sln`; the app no longer references the plugin
  projects and loads their DLLs at runtime from `Plugins\`, with the contract assembly in
  `Assemblies\`
- three plugins shipped that way already: **PortScan, SshTransfer, AWS**
- consequently they **deleted** `UI/Window/PortScanWindow.cs` and `UI/Window/SSHTransferWindow.cs`

We still have both windows, hardcoded.

Secondary and much less interesting: a DEBUG-only SQLite options browser
(`IOptionsRepository` / `OptionsStore` / `OptionsRepositoryManager` +
`OptionsManagementDevGuide.md`). Developer tooling, not a settings-architecture change.
They also deleted `DotNetRuntimeCheck.cs` and `LocalSettingsManager.cs`, which we still carry.

## 3. How far apart the trees are

Merge base `1c3e8acbc` — **2026-03-03**, six and a half months ago.

| | commits | files touched |
|---|---|---|
| ours ahead | 1,788 | 2,905 |
| theirs ahead | 525 | 324 |
| **files both sides changed** | | **216** |

The collision surface is concentrated exactly where both projects have been busy:
OptionsPages (17), UI/Window (12), Language (12), App (11), Properties (7), Themes (6),
RDP (6), ConnectionTree (5), Credential (9).

**A merge is not on the table.** Our 1,788 commits include the .NET 10 migration and the
5,247-warning cleanup; their 525 include a re-architecture. Alignment has to mean *direction
and interfaces*, not history.

## 4. The side we stopped looking at

**Six pull requests from @jafin have sat unanswered since 5–7 August — about six weeks.**
The only reactions on any of them are bots (qodo, copilot). Every one carries tests.

| PR | Size | What |
|---|---|---|
| #154 | +45/−36, 24f | Zero build warnings + test suite runnable on Win11 23H2 |
| #155 | +1/−3, 4f | Drop redundant direct `ZstdSharp.Port` references |
| #157 | +154/−12, 2f | Keep the active tab when a tab close is cancelled (DockPanelSuite `TryCloseTab`) |
| #158 | +247/−6, 6f | Close a PuTTY tab in one step, without PuTTY's own prompt |
| #159 | +123/−8, 2f | Restore connection-tree state when the search filter is cleared |
| #161 | +2577/−1093, 55f | Port scan: one address field (range/CIDR) + port mode selector |

This has already cost us once: PR **#156** from the same contributor was ignored, we
re-derived its fix ourselves while working #176, and only then closed it (2026-09-05).

Note the collision: **#161 rewrites the port-scan UI at the same time upstream is turning
port scan into a plugin.** Same component, two directions, neither of them ours yet.

Also open: two dependabot majors from 09-14 — Roslynator 4.12.11 → 5.0.0, Meziantou
2.0.194 → 3.0.257.

**Fork network:** last full radar run was 2026-07-26. Refreshed today — 1,709 forks seen,
124 active candidates, 154 tracked; screened candidates went 186 → 337 (151 new). AI triage
of those is still running; its output lands in `fork-intel/reports/`.

## 5. The tension worth naming

Our fork's value is *process*: zero warnings, 6,842 tests, a lab UI battery, an issue
pipeline that answers reporters. Upstream's new value is *extensibility*.

If they keep moving capability into plugins while we keep it hardcoded, every future idea of
theirs gets more expensive for us to take, not less — the opposite of the reason to stay
aligned. The cheapest durable move is to adopt their **contract**, so a plugin written for
either tree runs on both, without adopting their code or their history.

One hard constraint: loading DLLs from a `Plugins\` folder is arbitrary code execution inside
a credential manager. Our own `OnAssemblyResolve` is deliberately restricted to
`Assemblies\` and `Languages\` under the app base. Any plugin host we build needs a trust
story (signature or allowlist) decided by a human, and it will trip the security tripwire —
correctly.

## 6. Fork radar, re-run today — the result reframes the plan

Full pipeline re-run (discover → diverge → screen → triage → report). 252 commits judged.

| Tier | Count |
|---|---|
| A — ready to cherry-pick | 1 |
| B — worth porting by hand | 72 |
| C — watch list | 7 |
| Quarantine — security review first | 69 |
| D — rejected | 400 |

**Where the value sits, by author, across Tier A+B:**

| Author | Candidates |
|---|---|
| **Jason Finch (jafin)** | **84** |
| local / lovaszlaszlo | 12 / 5 |
| vindict6, sanay | 6, 6 |
| Kyle Meeks | 5 |

Eighty-four of roughly ninety worthwhile candidates in the entire 1,709-fork network come
from **the one contributor whose six pull requests we have left unanswered for six weeks.**
The top-ranked Tier B item is `b7c487412f` — jafin's connection-tree filter fix, the same
change as his PR #159, which the radar independently ties to our issue **#149**.

He also has work in his fork that he has *not* opened PRs for: an SFTP pane series, a
notifications fix, SQL sentinel hardening, task-dialog text measurement.

**Two other finds worth naming:**

- **A whole cluster attacking #177** in `lovaszlaszlo/mRemoteNG`: `4edeaba5c1` "Re-run the
  resize when the panel moved again while it was applied" (score 8, REIMPLEMENT),
  `08b056f698` "Measure the panel, not the RDP control, when resizing the session",
  `9216eeec3f` "Never reconnect an RDP 8 session just to resize it", `f8b3b93411` "Anchor the
  RDP control to the panel instead of assigning its size". Someone else hit our stuck issue
  and iterated on it. Note they later **reverted** `4edeaba5c1` — that reversal is itself
  evidence, and it saves us repeating the attempt.
- **`04a6c9cfc9` (jafin), tied to #175:** startup messages are lost before the log writers
  attach — which is exactly why the `ExternalConnectors` loader errors (#175/#178/#191/#192)
  arrive with no usable diagnostics. Fixing that would give the crash family the evidence it
  currently lacks.

Report: `fork-intel/reports/2026-09-16_fork-radar.md`. Queue: `fork-intel/IMPORT_QUEUE.md`.
Counter-opinions (`preapprove`) were **not** run — that needs an explicit decision.
