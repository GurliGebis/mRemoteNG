# Import Queue

Generated 2026-09-16 from the fork radar. **Nothing is applied automatically.**

Both mRemoteNG and its forks are GPL-2.0, so importing is licence-compatible. `git cherry-pick` preserves the original author and `-x` records the source commit; add a `Ported-from:` trailer with the upstream URL so the origin stays visible.

After a decision, record it so future runs stop proposing it:

```bash
python .project-roadmap/fork-intel/fork_intel.py mark --sha <sha> --decision imported|rejected|deferred --note "why"
```

## Tier A - ready to cherry-pick

### `7349e5a6aa` Fix main window stuck behind other windows after startup - needs manual review

Removes redundant window activation code that causes inconsistent WinForms state when blocked by Windows on startup. Highly beneficial UX fix.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/7349e5a6aa3b85440a6f934e5269555c476fbb04

Counter-opinions: codex **REJECT** / grok **REJECT**
- codex: REJECT - The diagnosis is unproven here, the lifecycle diverged after the shared code, and no tests or reproduction justify deleting a deliberate focus safeguard.
- grok: REJECT - Diff contradicts claimed fix; startup focus is delicate and unverified here.

```bash
git remote add fi-k-meeks https://github.com/k-meeks/mRemoteNG.git
git fetch fi-k-meeks --depth=50 7349e5a6aa3b85440a6f934e5269555c476fbb04
git cherry-pick -x 7349e5a6aa3b85440a6f934e5269555c476fbb04
# then: build.ps1 + run-tests.ps1 -Headless before committing anything
```

## Tier B - worth porting by hand

### `b7c487412f` fix: restore connection tree state when the search filter is cleared

Our ApplyFilter/RemoveFilter still store live ExpandedObjects and skip RebuildAll; stale row map after filter clear is the same IndexOf/RedrawItems family as #149. Protected RebuildAll(IList,IEnumerable,IList) exists. Tests included.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/b7c487412fc8ee89d9001dcbfad754484a42b31b

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `c4837b6551` fix: measure task dialog text the way it is drawn, and size buttons to the client area

Our frmTaskDialog still uses Graphics.MeasureString and Width for command buttons (GDI+/GDI mismatch, same class as #163). Take the two code hunks; skip the resx strings, which are jafin's storage-hardening feature.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/c4837b6551b9d8fe8ffc9980159905d0442824f3

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `dd54616a2e` Fix NullReferenceException + recursive dialog cascade on failed decrypt - needs manual review

Our XML null guard already prevents the NRE, but still throws into Runtime’s recursive reload path; adapt the null-return behavior to current nullable code.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/dd54616a2e47bdb94e18b2fbafbd2a30764a3728

Counter-opinions: codex **REJECT** / grok **APPROVE** / claude **REJECT**
- codex: REJECT - This fork already prevents the null dereference and duplicate file dialog through guarded validation and explicit-file loading; importing this patch is redundant and regressive.
- claude: REJECT - Fork already prevents the crash differently; null-return would only slightly change which error dialog shows for legacy decrypt cancel — marginal value.

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `eb03e059b2` Add configurable interface font (Options > Appearance) - needs manual review

Adds a highly useful, clean accessibility feature allowing user-customized interface fonts without restarting. Worth importing.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/eb03e059b2ecc1a1b00dc70056b70cdb348a2195

Counter-opinions: codex **REJECT** / grok **NEEDS_HUMAN**
- codex: REJECT - The accessibility idea is useful, but this untested global override conflicts with existing font behavior and requires target-specific redesign, not direct import.
- grok: NEEDS_HUMAN - Nice accessibility tweak, but side effects on panels/DPI and leaks need maintainer review first.

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `2a9a06a5c7` Translate the menus, dialogs and options pages into Hungarian

Our hu.resx has 88 of 910 keys; this adds 371. Drop keys we removed (update channels, e.g. AskUpdatesContent) and verify against our Language.resx before landing.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/2a9a06a5c7ba6485ca429c9da7ccee0e66831b12

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `2cb2b55cc7` fix: drop the redundant panel-close prompt after a tab disconnect (#46)

Our Connection_FormClosing still counts connDock.Documents.Any(); HasConnectionTabs already exists so the LiveConnectionTabCount refactor drops in. Verify with AutoClosePanelOnLastTabClose UI repro.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/2cb2b55cc723d161a670d4ad41f98ad830871d4f

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `4edeaba5c1` Re-run the resize when the panel moved again while it was applied

Size<=0 guard already ours; settle-recheck after UpdateSessionDisplaySettings is not. No GetAvailableContentSize here, use InterfaceControl.Size. Candidate for #177 splitter race.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/4edeaba5c11fe819d1cf1fd37584e6b4fa79c325

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `c4d0596f24` Keep the tab in front when its close is cancelled

Our OnFormClosing cancels on No without re-activating; same wrong-tab-in-front applies. Also affects our KeepTabsOpenAfterDisconnect cancel path. Verify in lab UI.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/c4d0596f2485d6493ea30bd27219f40c2a068759

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `0121f0be4c` fix: port scan reports why a scan can't start instead of doing nothing

Real bug: throw leaves button stuck on Stop, no reason shown. Our PortScanner already diverged (8119ae123 timeout fix), so port the ordering idea, not the diff.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/0121f0be4cea9a485f91f41d2bae56ea2b288f65

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `07532bce52` Translate the connection properties into Hungarian

Hungarian strings for standard property keys we do have (AudioCapture etc. missing in our hu.resx). Context lines reference Vault/OpenBao keys we lack, so hand-apply the added data entries.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/07532bce526b55e47bc43768c4152bcdca922d2a

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `0d8b8f6c56` Add "Copy All to Clipboard" to PuTTY connection tab context menu - needs manual review

The backend exists, but the requested tab action does not. Add only UI wiring using the existing method and resource, avoiding duplicate backend and localization.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/0d8b8f6c56485861217abdc30a25ae0420827ccf

Counter-opinions: codex **REJECT** / gemini **REJECT**
- codex: REJECT - Menu exposure is useful, but only its UI wiring should be reimplemented against the existing method; this commit is not directly landable.
- gemini: REJECT - The backend method already exists in our fork. This commit would cause merge conflicts and code duplication, requiring a clean manual reimplementation.

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `199b1f362b` docs(sftp): reshape the change to a dual-pane file manager with a transfer queue

Spec-only for dual-pane SFTP manager with transfer queue; we have single file-transfer window (ab1be61fd). Idea worth watching once code lands, nothing to import.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/199b1f362b0e4203c5f322b752d1e1169a2802f1

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `232fbf32ff` fix: multi-selection Enter opens connections, and task dialog buttons show focus (#52)

Our Enter handler still opens only SelectedNode; task dialog focus fallback and CommandButton focus ring absent. Skip docs-website hunk; verify against our multi-select tree and lab UI.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/232fbf32ffc324b12ceb8b8e251e4637742a8073

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `2d1411667e` 修复：容器的ID现保持与文件中一致 - needs manual review

Current XML loading discards serialized container IDs because CopyFrom cannot set get-only ConstantID. Reimplement constructor-based preservation with malformed-ID and round-trip tests.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/2d1411667e60c4e001933d60f8de825dd2ac9213

Counter-opinions: codex **NEEDS_HUMAN** / grok **NEEDS_HUMAN**
- codex: NEEDS_HUMAN - The defect is real and absent here, but land a tested fork-aware reimplementation covering Container and Entity instead of this stale patch.
- grok: NEEDS_HUMAN - Real container ID-stability fix, but needs clean reimplementation and fork check.

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `3c2fd1770a` fix: drop the trailing comma from the port scan open/closed port columns

Our ScanHost.cs still has the trailing ', ' loop; string.Join is a trivial cosmetic fix for the port scan grid.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/3c2fd1770ab8b9c2ee1ece2bb5dd6ad255f3bc09

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `3f94a2c239` Dark mode: follow the OS, honor the theming setting, dark title bars - needs manual review

Follow-OS dark mode + DWM dark title bars addresses open #47. Clean idea, but flips ThemingActive default and our ThemeManager/settings diverged; re-derive carefully.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/3f94a2c23980a384cbf15386ae7ffc506a92e6e5

Counter-opinions: codex **REJECT** / gemini **NEEDS_HUMAN**
- codex: REJECT - OS matching is valuable, but this untested patch conflicts with live-switch and high-contrast theming, assumes restart-only behavior, and requires a scoped reimplementation.
- gemini: NEEDS_HUMAN - Valuable dark mode UX improvements matching modern Windows settings, but requires careful refactoring of settings and ThemeManager initialization to prevent regressions.

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `53451f91f5` Default the main window to 90% of the screen

Ours falls back to designer size on first run; #171 MainWindowPlacement restructured this method. Add centred 90% working-area default only when nothing saved.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/53451f91f5195273903d0ac51e92ec1ee45e13ab

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `556127ef31` fix: keep Options usable when a settings secret cannot be decrypted (#18)

Options page survives undecryptable settings secret; no ErrorOptionsPageSettingsNotLoaded here. Symptom unconfirmed in our fork; jafin's crypto layers differ. Verify before reimplementing.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/556127ef31c31603313145d28cbfc63fec4378fa

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `6c1dbeaf82` fix: make ConnectionsFileResolver sole-candidate test deterministic

Our test still named StartupConnectionPathReturnsSavedPathWhenItIsTheSoleCandidate and scans real OS paths; host-dependent flake. Rewrite against ConnectionsFileResolver.Resolve is deterministic. Test-only.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/6c1dbeaf8209f23145c456805bb5cd72d03cff32

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `6e684bc7f9` Fix the spin buttons and the crowded backup page

Our mrngNumericUpDown still hard-codes 96-DPI SetBounds; DPI-aware LayOutButtons is real. Skip Hungarian resx edits and BackupPage designer (ours already resized).  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/6e684bc7f9fe7b5e711598862639629e1e6e815a

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `6eabb55063` fix: remove the dead space between the timeout row and the progress bar

Our PortScanWindow.Designer still has 159F row; 4-line layout fix, verify visually in built app.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/6eabb55063017b51f2a2a0097f9714508ae98b19

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `6ecce9b6be` fix(notifications): render the message text on startup messages

We have the #53 _pendingItems deferral but not the posted flush; startup messages likely render timestamp-only here too. Reproduce first.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/6ecce9b6becc1bc5f81b8190157996e009ae292f

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `7d47769f49` Put the units back on the Hungarian spin box labels

Hungarian AutoSaveEvery label lacks units in ours; RdpOverallConnectionTimeout/RdpReconnectCount keys absent in our hu.resx. Cherry-pick only applicable string.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/7d47769f49cd4658beeafc4f3a482ff763784a15

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `85056294af` fix: stop connection edits from being lost without a word (#10)

Save-failure propagation already ours (b7126a004 TrySave). Still open here: Shutdown.SaveConnections drops Unassigned default to 'never' and never flushes the 2s debounced save. Port those two only.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/85056294af97fa3ad7316a36097e0d38f67a9500

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `8ec20405e6` test: run the WinForms tab tests on a real message loop with a reliable timeout

Our RunWithMessagePump is a bare STA thread with no Application.Run; Interrupt cannot unwind a wedged UI thread. Test-only, same file, clean apply.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/8ec20405e6151c033b6d9193e57cb5cd74bddca2

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `90aa5e8c05` feat: double the default width of the port scan Hostname column (130 -> 260)

Ours still 130; FQDNs truncate. One-line designer change, matches our bolder-UI preference.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/90aa5e8c05dcf7e0c85429a9312def4ca6456279

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `9c4b85f18a` fix: set temp key-file attribute via File.SetAttributes - needs manual review

Replaces redundant throwaway FileInfo instantiation with clean, direct File.SetAttributes call in two PuttyBase temp key generation paths.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/9c4b85f18ab51f04b455d198cbf86b284dd6c3f8

Counter-opinions: codex **REJECT** / grok **REJECT**
- codex: REJECT - It provides no correctness or stability gain; reimplementation would be churn because both APIs set the same attribute and current code has zero warnings.
- grok: REJECT - Original object-initializer already sets attributes on disk; pure idiom tweak, not a real fix.

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `9c5c4484ce` perf: stop the port scan flooding the UI thread; add common-ports button

Our MessageCollector still unlocked with per-item RemoveAt trim; scan adds messages per host. Take lock+RemoveRange; common-ports button optional, designer diff will conflict.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/9c5c4484ce6059312dfc4a75dcf2bb89675bf3be

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `a677fae337` Fix ObjectDisposedException when closing a connection tab - needs manual review

Closes TOCTOU race in Prot_Event_Closed Invoke; our guards (IsDisposed check) exist but not the try/catch + marshaled re-check. Small defensive win; code diverged.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/a677fae337a8c49890c6a0e2d87b9739d708d25d

Counter-opinions: codex **REJECT** / grok **APPROVE** / claude **REJECT**
- codex: REJECT - Current HandleProtocolClosed already has stronger handle, marshaling, disposal-race, and close guards, so this commit offers no unique value and conflicts with intentional semantics.
- claude: REJECT - Fork diverged: same race already fixed better (non-blocking BeginInvoke re-marshal, ConnectionWindow.cs:2223-2246). Import adds nothing, code no longer matches.

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `b1e1dcfe5d` fix: stop docking the placeholder window a handed-off console leaves behind (#48)

PseudoConsoleWindow absent in our tree; ExternalProcessProtocolBase.cs exists. Win11 Windows Terminal handoff docks 0x0 placeholder. Small, tested, with docs note.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/b1e1dcfe5db54b1acd6d191c57dd3980a38ab3cf

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `c535880a14` feat: single address field and port mode selector for the port scan

We still IPAddress.Parse two fields; CIDR/range/IPv6 parser with tests is self-contained. PortScanWindow diverged (timeout fix 8119ae123), expect merge work.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/c535880a14c0395a18dfe24a3c22d4c1853dfe2a

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `d37a901670` fix: Options UI polish and task dialog button spacing (#45)

Our BackupPage still has 11 Salmon debug BackColors; ConfigurationPage still 3 columns. Small designer-only cleanup, low risk.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/d37a901670287d284fb9cbffcd73c348d6a87f09

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `d500a8e9dd` CustomConsPath为相对路径时，主窗口标题也能正确显示全路径（上一提交引入） - needs manual review

Displays absolute path in main window title when loaded with relative path. Simple and safe UX bugfix, needs minor adjustment for our namespaces.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/d500a8e9dda08af453e3e69f3d891e2be4145686

Counter-opinions: codex **REJECT** / grok **NEEDS_HUMAN**
- codex: REJECT - Current paths are already normalized and CustomConsPath is unused; remaining relative inputs should be normalized at load time, not during rendering.
- grok: NEEDS_HUMAN - Small useful title fix for relative paths; confirm null safety and no local equivalent first

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `d6f4872b8b` 标签右键中增加关闭菜单 - needs manual review

Adds Close item to panel-tab context menu; minor UX win. Old mRemoteV1 paths, trivial to redo in our PanelAdder if wanted.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/d6f4872b8bd1a73e4f293a78243ed424b7347e3e

Counter-opinions: codex **REJECT** / grok **NEEDS_HUMAN**
- codex: REJECT - Importing this stale duplicate adds no capability and risks conflicts or regressions against the maintained implementation already present.
- grok: NEEDS_HUMAN - Small useful tab UX, but verify duplication and correct ConnectionWindow close semantics first.

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `f634039a17` fix: offer the SSH transfer window for the protocols it can reach (#24)

Our gate still SSH1|SSH2 at 4 call sites (ConnectionWindow:1489/1587, ContextMenu:1022/1062); SSH1 offered, OpenSSH withheld. No ProtocolFeature class here; add helper without SSHNative.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/f634039a177f9204454e5fd8680b3cce3ee50863

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `3567ecabb1` fix: validate the ports argument in the PortScanner constructor

Our PortScanner ctor still does bare _ports.AddRange(ports) with no null/empty/range check; depends on jafin's PortListParser and Language keys, so re-do with local constants.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/3567ecabb18f190897415f43b4061f06fb270319

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `7746827c2b` fix: take a command-line switch value whole, whatever it contains (#41)

Bug confirmed here: CmdArgumentsInterpreter splitter regex `=|:` still splits a space-separated value like C:\path, silently dropping --cons. Our CommandLineParser diverged from jafin's; port the logic plus tests, skip openspec/docs-website.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/7746827c2b4af03f16d59fe482cb6f050bef70c9

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `95f621308b` fix: validate the ports in the port-range PortScanner constructor too

Our PortScanner has no port validation at all (no PortListParser/ValidatePorts); the range ctor loop can allocate an absurd range. Small standalone guard worth adding with tests, independent of jafin's parser.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/95f621308b5afe3d59675a5721fc186c663fca40

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `9db6e8b09b` feat: put the port scan IP range on one line and tighten the layout

Cosmetic PortScan layout (IP range one line, IPv6-wide boxes). Our Designer lacks pnlIpRange; self-contained, low risk, minor benefit.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/9db6e8b09be0d332ba383b5d3489649ccd577b06

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `08b056f698` Measure the panel, not the RDP control, when resizing the session

Core fix (never measure Control.Size) already ours: we use InterfaceControl.Size. Padding-aware ClientRectangle measurement is new; connection-frame padding may oversize session and re-show scrollbars. Candidate for #177.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/08b056f698587be31a2582070549e24dc7147fcb

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `4b2768dcea` fix: claim the foreground for the main window at startup

Our FrmMain_Shown still plain Activate/SetForegroundWindow; AttachThreadInput already declared. Reasonable startup foreground fix, but #143/#168 history: attach must not be blindly detached, verify in lab.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/4b2768dcea09bbec5dc3f857d37f3163dd2ac0c4

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `9216eeec3f` Never reconnect an RDP 8 session just to resize it

Our RdpProtocol8.DoResizeClient still calls Reconnect on FitToWindow. Real session-drop bug, but RdpVersion=Highest ships RdpProtocol11 so only forced-RDC8 users hit it; our resize path diverged.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/9216eeec3fc53ba08f2ea15804a58983916a8f6e

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `0045263765` Show auto-detected PuTTY path on Advanced options page - needs manual review

Small UX win: shows auto-detected PuTTY path in options. Our Designer/options pages diverged heavily; re-do by hand, not cherry-pick.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/004526376515164a858c98a9a1c782d04a28c33c

Counter-opinions: codex **REJECT** / gemini **REJECT**
- codex: REJECT - The fork already exposes the custom override and otherwise always launches bundled PuTTYNG.exe, so this UI is redundant, misleading, and upstream-specific.
- gemini: REJECT - Our fork bundles `PuTTYNG.exe` and has not imported the unbundling candidate. This change is redundant and will break the build due to missing auto-detection dependencies.

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `23bc5b533a` docs: archive fix-command-line-value-parsing (#42)

openspec archive docs only, no code. But our CmdArgumentsInterpreter.cs:46 still splits on ':' so `--cons C:\path` breaks; track jafin's code commit, not this.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/23bc5b533a708fc2ca070070fbe401246848c486

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `404e7291d5` Fix native RDP launch when signing fails

Fixes a file (NativeRdpLauncher.cs) that does not exist here; only relevant if we ever adopt the native mstsc mode from 405520de.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/404e7291d589796fae4332a40cae13b166dd3e7e

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `938611f022` fix: close a PuTTY tab in one step, without PuTTY's own prompt

Our TryClosePuttyGracefully just WaitForExit(1000); PuTTY warn-on-close double prompt real. But diff also touches ConnectionTab close semantics (disconnectOnly) overlapping #61/#172 work; port dialog-dismiss part only.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/938611f02255a6e9bbe40fe59bffd8b588c85dd2

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `a320091188` perf: batch the passes RemoveFilter runs when clearing the tree filter

Our RemoveFilter assigns ExpandedObjects, not RebuildAll; jafin's base fix (EnsureVisible index throw) absent too. Batch+rebuild worth doing; adjacent to #144/#149 row-index family.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/a32009118810e2237c029756814292f30a01343c

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `ead4062d23` refactor(ui): remove the unused session split host

Removes jafin's own SessionHost/SplitContainer scaffolding (SFTP side panel) that never existed in our fork; nothing to remove here.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/ead4062d23408fdbd127e9c7813ad4a96004c410

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `ec9f699a2c` Say which terminal this is while PuTTY is still around

Temporary debug banner in fork-only xterm.js SSH terminal; we have no Resources/Terminal/terminal.html or native SSH.NET terminal.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/ec9f699a2cc3f09e0fa2e036206bc9337ebcf6fe

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `215eb6c564` Adapt native RDP serializer to fork model

RdpFileSerializer.cs does not exist here; commit adapts it to that fork's model via reflection-style lookups. Native .rdp export is interesting but needs whole feature, typed against our ConnectionInfo.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/215eb6c564eacd35cd55dc124cfd90d9232fad7a

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `5293862e10` feat: make port scan parallelism configurable from the UI

Depends on jafin's Parallel.ForEachAsync scanner rewrite; our PortScanner is sequential thread with no MaxConcurrentHosts. Nothing to configure without prerequisite.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/5293862e1066e03c86909fccf7d33bdf478f433c

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `ba164119c1` feat: localize the port scan panel

Our PortScanWindow still hardcodes English; but commit depends on jafin's IpRangeParser/PortListParser (CIDR/IPv6 rewrite) absent here. Only resx strings portable.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/ba164119c17eac43c6536d4012963df68f6f3f52

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `015c152817` Use connection names for temporary RDP files

Temp .rdp filenames from connection name: minor diagnostic nicety, but leaks connection names into LocalAppData filenames. Their TemporaryRdpFileStore is fork-local.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/015c15281728dbf87fc4fb882492ccd0de2ff515

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `98a262b7b7` fix: recognise the embedded PuTTY window as our own foreground

ApplicationIsInForeground does not exist in our frmMain; #168 solved differently (bf99b7ad9, reporter-confirmed). GA_ROOT idea useful only if a PuTTY-foreground regression resurfaces.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/98a262b7b79fc3a6736234e9b6caa31a1ef7e047

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `c31a3c8161` feat(sftp): make transfers reachable, add mutation commands and theming

Increment on jafin's SFTP browser panel; no FileTransfer/FileManagerTab in our tree. Only importable as the whole feature chain once it stabilises.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/c31a3c8161251515726eaedf6f46933946e9f252

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `d239f5b9e5` feat(sftp): add the pane abstraction, local browser and transfer queue

Dual-pane SFTP browser foundations (browser abstraction, queue, tests) are self-contained and tested, but only half a feature; wait for jafin's series to complete, then evaluate as a whole.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/d239f5b9e5f776df781d2263cb31b63f2185b24e

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `f8b3b93411` Anchor the RDP control to the panel instead of assigning its size

Anchor-instead-of-Size idea targets our #177 symptom, but diff is against their AutoScroll variant; our SetResolution/DoResizeControl differ and lab trace showed resize sizes are correct. Test idea in lab, don't port.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/f8b3b934116f748df84a075f36a9f07d55cb228b

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `f92a9cc609` security: stop writing every debug message to the log by default

Our TextLogMessageWriterWriteDebugMsgs already defaults False; only new bit is %USERPROFILE% redaction in the log path. Small, but touches diagnostics (tripwire, human review).  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/f92a9cc609c15c3afcbe7edbe958f77406e81d78

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `8f39c112b5` fix(rdp): reapply performance flags and input finalizer on all reconnect paths - needs manual review

Reapplying performance flags on mstscax auto-reconnect is a plausible real fix, but patch depends on fork-only view-only/input-finalizer infrastructure we lack. Note idea, not code.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/8f39c112b57865efa6c34cd735c1b35394c203dc

Counter-opinions: codex **REJECT** / grok **NEEDS_HUMAN**
- codex: REJECT - Exact commit cannot land: RdpProtocol6 was deleted, passive helpers are absent, RdpProtocol8 is refactored, and no tests or reproducible evidence are provided.
- grok: NEEDS_HUMAN - Reapplying pFlags on reconnect is useful, but diff is fork-specific and needs local path checks.

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `7e126049b3` feat(sftp): transfer directories recursively, with one overwrite choice

Builds on jafin's SFTP/FileTransfer subsystem (ISftpSession, SftpSession) which our fork lacks entirely; 2555 lines depend on it. Track jafin's SFTP branch as a whole.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/7e126049b31f49b2100f69e6a20c704154fd7029

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `8ded8c7535` Native SSH: take the appearance from the named PuTTY session

Reads PuTTY session font/colours for an xterm.js SSHNative protocol we lack (no ProtocolSshNative, terminal.html). Idea sound; only relevant if we ever add a WebView2 terminal.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/8ded8c7535559aef80bdcae609b431df7a3eb8d6

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `952dd3b37c` Native SSH: verify the host key and support private keys

Host-key verification for ProtocolSshNative, which our fork does not have (SSH1/SSH2/OpenSSH only). Valuable only if we ever adopt a native SSH.NET terminal; note reusing password as passphrase and SSHOptions parsing.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/952dd3b37cb18f9a1c39fabb986d150d7940216a

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `b0b8020946` fix(sftp): wire the agent provider, and stop mangling server paths

Fix inside jafin's SFTP feature (no mRemoteNG/Connection/Sftp here). Backslash-preservation point is sound; bundle with feature import if SFTP browser ever evaluated.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/b0b80209466e4d6e4b341c767da6f86fad61c402

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `b939635404` feat(sftp): add the dual-pane file manager tab

Dual-pane SFTP file manager, 1610 lines over 12 files, depends on IFileSystemBrowser and earlier openspec commits absent here. Attractive later, needs full series plus security review of file transfer; not now.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/b939635404025d16afbc64b0f7de2b4de3baf3f1

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `cb423ff6ec` security: stop backup recovery misreporting a key it cannot unwrap (#35)

Depends on jafin's per-file key / KeyProtectionException feature absent here; our loader has no unwrap path, so bug does not exist yet. Revisit if we adopt per-file keys.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/cb423ff6ecf8949d2dd730cacba61f55b082c6ba

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `39aa9177e0` fix(sftp): attach the pane's icons where the list actually looks

Fixes jafin's SFTP FilePaneControl, which we do not have; only the OLV SetSmallImageList-vs-property lesson transfers. Watch the file-transfer feature itself.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/39aa9177e059dc7ed421c7a4d151f7a90b23d1a1

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `127570bac6` feat(ui): host connection sessions in a split so a side panel can share the tab

Groundwork for jafin's SFTP side panel: wraps sessions in a split, reparents InterfaceControl. Touches handle/reparent paths relevant to #182/#177; only worth it if we adopt the SFTP browser panel.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/127570bac6d10d312d1461a3a6cb755a16bee039

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `2a693c85c2` Added SSH Tunnel via SSH_DotNet - needs manual review

Native SSH.NET forwarding is potentially valuable, but this untested patch depends on an absent protocol and rewrites obsolete tunnel logic; monitor, do not port.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/2a693c85c2525ff21e0f35968f9a0745dc612022

Counter-opinions: codex **REJECT** / gemini **REJECT**
- codex: REJECT - It adds an untested parallel SSH stack and omits SQL/MariaDB persistence, conflicting with stability, storage consistency, and quick verification requirements.
- gemini: REJECT - We do not have the SSH_DotNet protocol implemented. Importing this will break the build and introduces excessive complexity.

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `42f04f6eb6` Native SSH: do not lose the message when something goes wrong

ProtocolSshNative (WebView2 terminal) does not exist here; patch only meaningful if that whole feature is ever imported.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/42f04f6eb66fd55845ae5da9176520579ba13c4d

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `932e6f6116` Enhance connection handling and UI features - needs manual review

Mixed bag: new inheritance props (ExternalAddressProvider, RDP StartProgram, gateway token), notification detail, plus personal junk (.vscode, WorldOfFanXP.xml). Partly overlaps our upstream ports; cherry-pick only if users ask.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/932e6f611674e6227db18d977f67a1b577af25a2

Counter-opinions: codex **REJECT** / gemini **REJECT**
- codex: REJECT - A 732-line mixed, untested commit also bypasses notification filters, leaks writer subscriptions, duplicates shipped UI/retry features, adds untranslated labels, and uses noncanonical tooling.
- gemini: REJECT - This is a mixed bag of personal settings, French locale scripts, and features already integrated or overlapping with our upstream ports. Not suitable for import.

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `718f5c28c1` feat(sftp): reconnect a dropped session, and make the listing readable

Builds on jafin's SFTP/FileTransfer subsystem (mRemoteNG/Connection/Sftp, FileTransfer/) which our fork does not have; no standalone import possible.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/718f5c28c1300e2745e3a95700bf5787d8c03f61

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

### `df13f1685d` Sign native RDP files in-process with CMS

Rewrites NativeRdpFileSigner.cs we do not have; native mstsc launch + CMS signing is a fork-specific feature. Crypto code, tripwire-gated. Niche demand.  
Source: https://github.com/mRemoteNG/mRemoteNG/commit/df13f1685d1cdcffadd756b73f7210878ad60eaa

Port by hand - the patch will not apply cleanly over our tree. Read the source diff, reimplement, and credit the original author in the commit body.

