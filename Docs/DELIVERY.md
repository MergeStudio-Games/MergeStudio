# Delivery status / Teslim durumu

> Historical snapshot. For current implementation and blockers, see
> [WORKSPACE-ROADMAP.md](WORKSPACE-ROADMAP.md). The 2026-09-22 Editor invocation
> fails on licensing; the successful test counts below are from 2026-09-18.

**Snapshot / Anlık kayıt:** 2026-09-18 · `develop` · Unity `6000.6.0f1`

## Current status / Güncel durum

| Area / Alan | Status / Durum | Evidence / Kanıt |
| --- | --- | --- |
| Unity Editor | ✅ Ready / Hazır | `D:\unity\6000.6.0f1\Editor\Unity.exe` |
| Android modules | ✅ Ready / Hazır | Build Support, SDK/NDK, OpenJDK 17, CMake |
| Android SDK | ✅ Ready / Hazır | API 34/36, Build Tools 36, platform-tools 36 |
| Package import | ✅ Clean / Temiz | Unity 6.6 import completed |
| EditMode tests | ✅ 25 passed | XML report in local `TestResults/EditMode.xml` |
| PlayMode tests | ✅ 2 passed | XML report in local `TestResults/PlayMode.xml` |
| Static audit | ✅ 0 errors | `python Tools/validate_repo.py` |
| GitHub template | ✅ Public | `MergeStudio-Games/MergeStudio` |
| CI Unity tests | ⚠️ Pending secrets | Add `UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD` |
| CI Android AAB | ⚠️ Not proven | Run workflow after Unity secrets are configured |
| Store release | ⏳ Game-specific | Signing, privacy, store listing, device QA remain |

## What was fixed / Yapılan düzeltmeler

- Removed the invalid `com.unity.modules.inputlegacy` dependency that blocked Unity 6.6 package resolution.
- Updated the 2D package line to `2D Animation 16.0.0`, `SpriteShape 16.0.0`, and `Tilemap Extras 9.0.0`.
- Committed the generated `Packages/packages-lock.json` and Unity project settings after a successful import.
- Made Addressables setup create Unity's conventional `Assets/AddressableAssetsData` pointer directory on a fresh clone.
- Set Android target API explicitly to 36 for current Google Play submission rules.
- Replaced deprecated object lookup APIs to keep the Console free of avoidable warnings.
- Added repository audit workflow, CODEOWNERS, Dependabot, security policy, conduct rules, and a local Android toolchain checker.

## Run locally / Yerelde çalıştırma

```powershell
python Tools/validate_repo.py
powershell -ExecutionPolicy Bypass -File Tools/Check-AndroidToolchain.ps1
powershell -ExecutionPolicy Bypass -File Tools/Test-Unity.ps1
```

`Test-Unity.ps1` fails unless each XML report exists, has at least one test, and has zero failed or skipped tests. Do not report a test as passing without the XML evidence.

## CI and release gates / CI ve yayın kapıları

1. Add Unity GameCI secrets in the repository or an environment. Fork pull requests do not receive secrets.
2. Run `Build Android` on `develop` and on a dummy PR. The workflow must pass Unity tests, produce an `.aab`, and validate `BundleConfig.pb` plus `base/manifest/AndroidManifest.xml`.
3. Generate an Android upload keystore outside Git. Store it safely and add its four GameCI secrets only when the game has a real package identifier.
4. Protect `main` and `develop`; require the repository audit, Unity tests, and one review before merge.
5. For every new game created from this template, replace company/product identifiers, content, package name, analytics policy, privacy links, signing configuration, and release metadata.

See [STUDIO-READINESS-REPORT.md](STUDIO-READINESS-REPORT.md) for the full bilingual technology, product, marketing, AI, and production-gap audit.

## English note

This repository is a reusable Unity 6.6 foundation and template. A green local test run proves the checked-in foundation; it does not prove a future game's content, device performance, privacy compliance, signed store build, or monetization.

## Türkçe not

Bu repo yeniden kullanılabilir Unity 6.6 temelidir ve şablondur. Yerel testlerin geçmesi, commit edilmiş temelin çalıştığını gösterir; yeni oyunun içeriğinin, cihaz performansının, privacy uyumunun, imzalı mağaza derlemesinin veya monetizasyonunun hazır olduğunu göstermez.

