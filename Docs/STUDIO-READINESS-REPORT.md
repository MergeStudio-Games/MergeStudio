# MergeStudio Games studio readiness report

> **2026-09-22 update:** The tables below are the historical 2026-09-18 snapshot.
> Current Unity runs fail with licensing exit 198, so the old passing counts are
> not current validation. Follow [WORKSPACE-ROADMAP.md](WORKSPACE-ROADMAP.md) for
> implementation progress, evidence and unresolved account/device dependencies.

**Snapshot / Anlık durum:** 2026-09-18 · `develop` · Unity `6000.6.0f1` · Android target API 36  
**Scope / Kapsam:** MergeStudio template and its first mobile 2D merge-game foundation

## Executive decision / Yönetici kararı

**English:** The repository is a strong, reproducible foundation for starting a 2D mobile game. The local Unity project now imports cleanly and its real EditMode and PlayMode suites pass. It is not yet a store-ready game: art/content, consent-backed production analytics, monetization, cloud save, signing, device coverage, and a licensed CI run still have to be completed for each game.

**Türkçe:** Repo, 2D mobil oyun geliştirmeye başlamak için tekrarlanabilir ve sağlam bir temel durumunda. Yerel Unity projesi artık hatasız import oluyor; gerçek EditMode ve PlayMode testleri geçiyor. Mağazaya çıkmaya hazır bir oyun değil: sanat/içerik, izinli production analytics, monetizasyon, cloud save, imzalama, cihaz kapsamı ve lisanslı CI çalışması her oyun için ayrıca tamamlanmalı.

The right product choice is a **small, version-pinned core** plus optional adapters. Adding every popular package would increase build size, upgrade risk, and attack surface without improving a merge game. The template therefore keeps runtime AI, ad networks, IAP, backend, and vendor SDKs out of the default dependency graph.

## Evidence collected / Toplanan kanıt

| Check / Kontrol | Result / Sonuç | Meaning / Anlamı |
| --- | --- | --- |
| Repository audit | `142 GUIDs`, `46 C# files`, `0 errors` | Tracked Unity metadata and source references are consistent. |
| Unity EditMode | `25 passed`, `0 failed`, `0 skipped` | Economy, board, persistence, and domain tests pass in Unity. |
| Unity PlayMode | `2 passed`, `0 failed`, `0 skipped` | `Init → MainMenu → Game` smoke flow and runtime guards pass. |
| Editor | `6000.6.0f1` installed | Matches `ProjectVersion.txt`. |
| Android Build Support | Installed | Unity Android extension is present. |
| Android SDK | Build Tools `36.0.0`, platforms `34` and `36`, platform-tools `36.0.0` | Local AAB prerequisites are present. |
| Android NDK | `r27c` | Matches Unity 6 Android requirements. |
| Java | Unity OpenJDK `17` | Matches Unity 6 Android requirements. |
| CMake | `3.22.1` | Unity Android native build dependency is present. |
| CI AAB | **Not proven yet** | GitHub Actions needs Unity license secrets; signing is a separate release step. |

The first test run exposed two real defects and they were fixed: the obsolete `com.unity.modules.inputlegacy` dependency was removed, and 2D packages were moved to versions compatible with Unity 6.6. The setup script now creates Unity's conventional Addressables pointer folder before assigning settings, so a fresh clone does not fail during import.

## Current dependency inventory / Mevcut kütüphane envanteri

The table shows the package requested in `Packages/manifest.json` and the version actually resolved in `Packages/packages-lock.json`. Built-in packages are supplied by Unity 6.6 and should be read from the lock file, not assumed from a previous editor release.

| Package | Requested / resolved | Role | Decision |
| --- | --- | --- | --- |
| `com.unity.inputsystem` | `1.20.0 / 1.20.0` | Touch, mouse, keyboard, controller, rebinding | Keep; make touch actions the first game-specific input map. |
| `com.unity.cinemachine` | `3.1.7 / 6.6.0 (built-in)` | Camera follow, framing, shake, transitions | Keep Unity 6.6 built-in package; do not force an older registry version. |
| `com.unity.2d.animation` | `16.0.0 / 16.0.0` | Skeletal 2D animation and skinning | Keep; `16.x` is the Unity 6.6-compatible line. |
| `com.unity.2d.spriteshape` | `16.0.0 / 16.0.0` | Procedural 2D paths and terrain | Keep when a game needs spline terrain. |
| `com.unity.2d.tilemap.extras` | `9.0.0 / 9.0.0` | Rule Tiles and authoring brushes | Keep for tile-based games; `10.x` targets a later Unity line. |
| `com.unity.addressables` | `2.11.2 / 2.11.2` | Async local/remote content and catalogs | Keep; add CDN and content-build validation per game. |
| `com.unity.localization` | `1.5.13 / 1.5.13` | `tr`/`en` strings and locale selection | Keep; add pseudo-localization and font/device QA. |
| `com.unity.test-framework` | `1.4.6 / 1.8.0 (built-in)` | EditMode and PlayMode tests | Keep Unity 6.6 built-in version. |
| `com.unity.ugui` | `2.0.0 / 2.6.0 (built-in)` | Runtime UI widgets | Keep; replace generated prototype UI with a game UI system. |
| Built-in `Burst` / `Collections` / `Mathematics` | transitive `2.0.0 / 6.6.0 / 1.4.0` | Optional data-oriented performance | Do not write Burst code until profiling proves a need. |
| Modules: audio, JSON, UI, web request, physics 2D, tilemap | Unity built-in | Core engine services | Keep only the modules a game uses; they add no third-party vendor lock-in. |

**Not currently used / Şu an kullanılmıyor:** URP/2D Renderer, Shader Graph, Netcode, Firebase, GameAnalytics SDK, ad mediation, Unity IAP, Unity Gaming Services authentication/cloud save, crash reporting, remote config, Sentis or another runtime AI model, and any paid asset-store package. This is intentional; each needs a product decision, privacy review, license check, mobile performance budget, and integration tests before inclusion.

## What is already in the foundation / Temelde hazır olanlar

- Scene boot contract: `Init`, `MainMenu`, and `Game` are in Build Settings and the smoke test traverses them.
- Merge domain: bounded board, item tiers, merge resolution, orders, energy, and currency rules are isolated from scene UI.
- Persistence: local save data is validated and authenticated; corrupt data is rejected without silently overwriting the previous file.
- Communication: ScriptableObject event channels keep scene objects decoupled.
- Content: Addressables settings, local/remote group layout, and a disposable async loader are present.
- Language: Turkish and English locales and the initial UI table are generated by the editor setup.
- Automation: repository audit, developer bootstrap, Unity test runner, Android AAB build workflow, iOS Xcode export workflow, Dependabot for GitHub Actions, CODEOWNERS, security policy, and bilingual onboarding docs.

## Production gaps and acceptance gates / Üretim eksikleri ve kabul kapıları

### P0 — before the first team feature sprint / ilk ekip sprintinden önce

1. **CI identity:** add `UNITY_LICENSE`, `UNITY_EMAIL`, and `UNITY_PASSWORD` as repository or environment secrets. The workflows now stop with a clear message when they are absent.
2. **CI proof:** run Android workflow on `develop` and on a dummy PR; require the test job and the ZIP-structure AAB verification job to pass. The current repository history contains earlier failures, so those are not evidence of a successful AAB.
3. **Clean import:** every developer runs `Tools/Bootstrap-Developer.ps1`, waits for package import, and gets the two local test suites green.
4. **Source control:** protect `main` and `develop`, require one review, require the repository audit and Unity tests, and disallow force pushes. Keep generated `Library`, builds, logs, credentials, and keystores out of Git.
5. **Game brief:** decide one audience, core loop, session length, art direction, monetization model, and one measurable first-week activation goal before adding provider SDKs.

### P1 — before closed testing / kapalı testten önce

1. Add real board visuals, animation, audio, haptics, tutorial, accessibility labels, pause/resume, offline behavior, and a deterministic content seed.
2. Replace analytics stubs with one adapter behind consent. Define an event schema (`first_open`, `tutorial_complete`, `merge`, `order_complete`, `session_start`, `purchase`) and document retention and deletion behavior.
3. Add a server-authoritative economy boundary before purchases or cloud sync. Local HMAC protects accidental tampering; it is not a substitute for server validation against a modified client.
4. Add crash reporting and a privacy consent flow. Test opt-out, data deletion, child-directed settings, and offline startup.
5. Add a device matrix covering low-memory Android, common aspect ratios, 60/90/120 Hz devices, API 34–36, rotation policy, background/foreground, and a slow network profile. Record frame time, memory, startup, AAB size, and battery impact budgets.

### P2 — before public launch / herkese açık lansmandan önce

1. Create the upload keystore outside Git, store it in a password manager, and configure GameCI signing secrets. Google Play App Signing and the internal test track must be validated with a real package identifier.
2. Finish store listing assets, Turkish/English copy, age/content declarations, privacy policy URL, support URL, screenshots, feature graphic, and release notes.
3. Run a staged rollout with crash-free sessions, ANR, retention, tutorial completion, and payer conversion thresholds. Keep a rollback build and a signed artifact checksum.
4. Conduct a license audit for art, fonts, music, SDKs, AI-generated content, and user-generated content. Archive attribution notices with every release.

## 2D rendering decision / 2D render kararı

The current foundation uses Unity's built-in renderer because a merge board does not require a render-pipeline dependency. Add URP only when the game needs 2D lights, normal maps, shadows, Shader Graph, or Pixel Perfect Camera. Unity's 2D URP workflow requires a URP asset with a 2D Renderer and project-wide graphics assignment; this is a deliberate migration, not a package-only edit. Profile low-end devices before enabling extra lights or post-processing.

## AI and research policy / AI ve araştırma politikası

Codex, Claude, and Copilot are development assistants. They must read `AGENTS.md`, inspect existing code, make reviewable changes, and report real test evidence. No model runtime, API key, prompt log, or cloud inference belongs in the game by default. Runtime AI is an optional feature only after its latency, memory, cost, privacy, offline fallback, safety, content license, and abuse controls are written into the game brief.

For asset generation, keep the source prompt/reference, license terms, generated file checksum, and human approval with the asset. Do not train or upload player data to a third-party model without explicit product and legal approval.

## Marketing and product operating system / Pazarlama ve ürün işletim sistemi

The repository is a technology template, not a finished market proposition. For each new game, create a one-page brief with:

| Area | Required decision |
| --- | --- |
| Audience | Country, age band, motivation, accessibility needs |
| Promise | One sentence explaining why this merge game is different |
| Loop | Discover → merge → reward → unlock → return, with a 60-second first session |
| Proof | 5–10 user playtests, retention notes, and a prioritized friction list |
| Acquisition | Store keywords, short video hook, screenshots, creator/community plan |
| Measurement | Activation, D1/D7 retention, session length, tutorial completion, crash-free users |
| Release | Internal test → closed test → staged rollout → rollback owner |

Do not optimize ad spend before the first playable proves fun and retention. Store assets, localization, and community feedback should be versioned alongside the game repo so marketing claims stay consistent with the shipped build.

## Team workflow / Ekip akışı

Use `Use this template` for a new game repository; do not fork the studio template. Keep the generated repository's history independent, rename the company/product identifiers, replace the placeholder assets, and open feature PRs into `develop`. The organization profile and this repository are bilingual so a teammate can give the repo to Codex or Claude and receive the same operating rules.

## Research basis / Araştırma temeli

The following primary sources were checked on 2026-09-18:

- [Unity 6 release support](https://unity.com/releases/unity-6/support) — Unity documents Update releases as production-ready for new/mid-cycle work and LTS as the choice for production lock-in; Unity 6.3 LTS is listed through December 2027.
- [Unity 6 system requirements](https://docs.unity3d.com/6000.0/Documentation/Manual/system-requirements.html) — Android development requirements include SDK API 35, NDK r27c, and OpenJDK 17 for the documented Unity 6 line. This repo uses API 36 to meet the newer Play requirement.
- [Unity 2D URP workflow](https://docs.unity3d.com/6000.1/Documentation/Manual/2d-game-creation-wokflow.html) and [2D Renderer setup](https://docs.unity3d.com/cn/6000.0/Manual/urp/Setup.html) — 2D lights and Pixel Perfect features require URP and a 2D Renderer asset assignment.
- [GitHub repository templates](https://docs.github.com/en/repositories/creating-and-managing-repositories/creating-a-repository-from-a-template) — template repositories create independent project histories and can be used by anyone with read access.
- [GitHub workflow templates](https://docs.github.com/en/actions/how-tos/reuse-automations/create-workflow-templates) — organization `.github` repositories can provide reusable workflow templates.
- [GitHub Actions secrets](https://docs.github.com/en/actions/how-tos/write-workflows/choose-what-workflows-do/use-secrets) — secrets are not passed to workflows from fork pull requests.
- [GameCI Android builder](https://game.ci/docs/2/github/builder/) — `androidAppBundle` produces `.aab`; keystore values should be supplied through secrets.
- [Google Play target API requirement](https://developer.android.com/google/play/requirements/target-sdk) — from 2026-08-31, new apps and updates must target Android 16/API 36 or higher.

This report distinguishes repository evidence from recommendations. Package versions and test counts above come from the local manifest/lock files and the generated Unity XML reports; they are not a claim that every future game will be production-ready without its own content, device, privacy, signing, and store validation.
