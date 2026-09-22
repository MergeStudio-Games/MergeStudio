# Workspace delivery plan

Snapshot: 2026-09-22. Work branch: `feature/studio-readiness`.
This is an ordered delivery plan; unchecked items are not implemented or verified.

## 1. Reliable local validation

- [x] Repository audit and Android toolchain check pass locally.
- [x] One command writes evidence with commit, dirty state, UTC time and individual check outcomes.
- [x] Unity process timeout and explicit licensing failures; invalidate both old test reports.
- [x] Refuse an existing build output; verify new bundle entries, CRC and SHA256.
- [ ] Activate Editor license in Unity Hub. Current real invocation fails with exit 198.
- [ ] Run both Unity suites with fresh XML: zero failures/skips and nonzero test count.
- [ ] Build and verify a fresh Android AAB.

```powershell
# Environment only; does not claim that Unity tests or builds passed.
powershell -ExecutionPolicy Bypass -File Tools/Test-Workspace.ps1
# Tests; requires an active license and no Editor using this project.
powershell -ExecutionPolicy Bypass -File Tools/Test-Workspace.ps1 -RunUnity
# Tests followed by build; build is skipped when tests fail.
powershell -ExecutionPolicy Bypass -File Tools/Test-Workspace.ps1 -BuildAndroid
```

Evidence: ignored `artifacts/workspace-status.json`, `TestResults/*.xml`,
`Logs/*.log`, and timestamped `build/validation-*/MergeStudio.aab`.
Bundle verification checks archive integrity, not signing or runtime behavior.
Keep the report together with the tested revision; a dirty report includes local changes.

## 2. Repository and CI

- [x] GitHub main/develop protection: one approving review, stale approvals dismissed,
  required audit, up-to-date branch, resolved conversations, admins included,
  no force pushes or branch deletion. Applied and read back on 2026-09-22.
- [ ] Configure CI licensing using the team's Unity license arrangement.
  Repository secret listing currently returns no entries. Do not paste credentials into chat.
- [x] Give Android/iOS test jobs distinct check names and verify both NUnit XML suites.
- [ ] Require the Unity checks after a successful licensed CI run.
- [ ] Obtain a green PR run and green develop build with retained XML and AAB artifacts.
- [ ] Review the recently merged major Action upgrades using actual build evidence.

One approving reviewer must be someone other than the PR author. The branch
protections intentionally apply to administrators as well.

## 3. Device and release evidence

- [ ] Connect an authorized Android device; current adb device list is empty.
- [ ] Test cold start, touch input, offline start, background/resume and save recovery.
- [ ] Record device model, OS, build revision, screen ratio, memory, frame time and startup time.
- [ ] Establish measured performance budgets on a representative low-end device.
- [ ] Choose game-specific application ID, version and release owner.
- [ ] Configure upload signing outside Git and validate on the Play internal track.
- [ ] Retain release checksum, provenance, release notes and last known good build.

## 4. Repeatable new games

- [x] Validate and generate company/product/application identity from committed template files.
- [x] Exercise local creation, identifier validation and existing-destination rejection in automated fixtures.
- [x] Verify independent history, no remote, and exclusion of untracked template files.
- [ ] Verify a fresh generated project imports/tests in licensed Unity.
- [x] Add a first-playable brief, device/release record and initial asset provenance ledger.

## 5. Complete first playable

Use the existing merge game as the starting point; keep it separate from generic tooling changes.

- [x] Choose the existing offline bakery merge loop as the first-playable scope.
- [ ] Select a connected representative target device; current adb list is empty.
- [x] Implement tap/drag move-or-merge, responsive board, selection marker, pulse/audio feedback with mute, and localized first-order instructions.
- [ ] Visually verify and refine art, audio, accessibility and tutorial on-device; code changes are not visual QA.
- [x] Wire the configured diamond-to-energy offer through a channel; localize HUD and offer text.
- [ ] Validate shop and localization in the running game.
- [x] Add domain/EditMode move, shop and consent tests, plus a PlayMode drag test.
- [ ] Run new Unity tests and inspect screenshots on target ratios.
- [ ] Verify a complete first-session loop and save/relaunch on a device.

## 6. Production services

- [ ] Choose providers and consent requirements for the actual game.
- [x] Implement a consent-off-by-default forwarding boundary and revocation tests; document the event schema.
- [ ] Wire a real provider and consent UI after account/product setup; no production adapter is connected.
- [ ] Integrate crash reporting and verify offline behavior.
- [ ] Integrate ads/IAP only with verified purchase/reward callbacks.
- [ ] Add authenticated cloud save with explicit conflict recovery and economy validation.
- [ ] Finish content license records, support/privacy pages and store assets.

Provider selection, account configuration and game-specific content remain open;
the template's placeholder services must not be described as production integrations.

## Primary references

- [Unity Hub license management](https://docs.unity.com/en-us/hub/manage-license)
- [GitHub branch protection API](https://docs.github.com/en/rest/branches/branch-protection)
- [Unity device profiling](https://docs.unity3d.com/6000.6/Documentation/Manual/profiler-profiling-applications.html)

The 2026-09-18 readiness report is historical evidence. Its successful tests do
not supersede the current licensing failure.

## Latest local evidence

- Python tooling tests: 7 passed (including real local Git LFS materialization).
- Pure C# domain checks: 20 passed (CLI JSON adapter; not Unity Test Runner).
- C# syntax and supplementary imported-assembly type check: 51 files, zero errors.
- PowerShell parse checks passed.
- Unity retry after starting Hub still exits 198; account entitlement is unavailable.
- No Android device is connected, no CI license secrets are present, and no store/provider account is configured by this work.
- Work remains on a feature branch; visual acceptance, real Unity tests and release gates remain open.

