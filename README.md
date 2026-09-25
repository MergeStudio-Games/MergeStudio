<p align="center">
  <img src="Brand/mergestudio-mark.svg" alt="MergeStudio" width="600">
</p>

<h1 align="center">MergeStudio</h1>

<p align="center">Unity 6.6 mobile merge-game foundation for MergeStudio Games.</p>

<p align="center">
  <a href="https://github.com/MergeStudio-Games/MergeStudio/actions">CI</a> ·
  <a href="https://github.com/MergeStudio-Games/MergeStudio/issues">Issues</a> ·
  <a href="https://github.com/MergeStudio-Games/MergeStudio/discussions">Discussions</a>
</p>

> **Status / Durum:** The repository is an extensible development foundation. Device validation, Unity Test Runner results, CI licensing, Android signing, and store release checks are still required before production release.

Current ordered work and validation commands / Güncel çalışma sırası ve kontrol komutları:
[Workspace roadmap](Docs/WORKSPACE-ROADMAP.md).

## Türkçe

### Proje

MergeStudio, Unity 6.6 ile geliştirilen mobil merge-2 oyun temelidir. Birleştirme oynanışı, siparişler, enerji ve para ekonomisi, yerel kayıt, event channel iletişimi, localization, Addressables, EditMode/PlayMode testleri ve GitHub Actions akışlarını içerir.

### Kurulum

1. Unity Hub ile `6000.6.0f1` kurun. Android için **Android Build Support**, **Android SDK & NDK Tools** ve **OpenJDK** bileşenlerini seçin.
2. Git LFS kurun ve repo kökünde çalıştırın:

   ```powershell
   git lfs install
   git lfs pull
   ```

3. Repo kökünü Unity Hub ile açın ve paket importunun bitmesini bekleyin.
4. Üretilen dosyalar eksikse Unity menüsünden `MergeStudio > Ensure Project Setup` komutunu çalıştırın.
5. `Assets/Scenes/Init.unity` sahnesini açıp Play'e basın. `Init → MainMenu → Game` akışını kontrol edin.
6. Unity Test Runner üzerinden EditMode ve PlayMode testlerini çalıştırın.

İlk geliştirici kontrolü:

```powershell
powershell -ExecutionPolicy Bypass -File Tools/Bootstrap-Developer.ps1
python Tools/validate_repo.py
```

### Ekip akışı

- `main`: yayınlanabilir üretim dalı.
- `develop`: entegrasyon dalı.
- `feature/<kisa-ad>`: yeni özellikler.
- `hotfix/<kisa-ad>`: acil düzeltmeler.

Pull request açıklamasında davranış değişikliğini, çalıştırılan testleri ve varsa sınırlamaları belirtin. Unity testleri veya Android AAB gerçekten çalıştırılmadıysa başarılı olarak raporlamayın.

Codex, Claude ve diğer geliştirme araçları kod değiştirmeden önce [AGENTS.md](AGENTS.md), [CONTRIBUTING.md](CONTRIBUTING.md), [Docs/ARCHITECTURE.md](Docs/ARCHITECTURE.md) ve [Docs/CODING_STANDARDS.md](Docs/CODING_STANDARDS.md) dosyalarını okumalıdır.

2D paketleri, sürümleri ve AI araçları için [Docs/TECHNOLOGY-BASELINE.md](Docs/TECHNOLOGY-BASELINE.md) dosyasına bakın.

Güncel test, Android toolchain, CI, ürün, pazarlama ve yayın eksiklerinin kaynaklı denetimi için [Docs/STUDIO-READINESS-REPORT.md](Docs/STUDIO-READINESS-REPORT.md) ve [Docs/DELIVERY.md](Docs/DELIVERY.md) dosyalarını okuyun.

## English

### Project

MergeStudio is a Unity 6.6 mobile merge-game foundation. It includes merge gameplay, orders, energy and currency systems, local persistence, event-channel communication, localization, Addressables, EditMode/PlayMode tests, and GitHub Actions workflows.

### Setup

1. Install Unity `6000.6.0f1` from Unity Hub with **Android Build Support**, **Android SDK & NDK Tools**, and **OpenJDK**.
2. Install Git LFS and run this from the repository root:

   ```powershell
   git lfs install
   git lfs pull
   ```

3. Open the repository root in Unity Hub and wait for package import to finish.
4. Run `MergeStudio > Ensure Project Setup` if generated assets are missing.
5. Open `Assets/Scenes/Init.unity`, press Play, and verify the `Init → MainMenu → Game` flow.
6. Run both EditMode and PlayMode tests in Unity Test Runner.

First developer check:

```powershell
powershell -ExecutionPolicy Bypass -File Tools/Bootstrap-Developer.ps1
python Tools/validate_repo.py
```

### Team workflow

- `main`: releasable production branch.
- `develop`: integration branch.
- `feature/<short-name>`: feature work.
- `hotfix/<short-name>`: urgent fixes.

Every pull request should describe the behavior changed, validation performed, and known limitations. Do not report Unity tests or Android AAB builds as passing unless they were actually run.

Codex, Claude, and other development tools must read [AGENTS.md](AGENTS.md), [CONTRIBUTING.md](CONTRIBUTING.md), [Docs/ARCHITECTURE.md](Docs/ARCHITECTURE.md), and [Docs/CODING_STANDARDS.md](Docs/CODING_STANDARDS.md) before editing.

See [Docs/TECHNOLOGY-BASELINE.md](Docs/TECHNOLOGY-BASELINE.md) for the 2D package baseline, versions, and AI tooling policy.

See [Docs/STUDIO-READINESS-REPORT.md](Docs/STUDIO-READINESS-REPORT.md) and [Docs/DELIVERY.md](Docs/DELIVERY.md) for the dated readiness evidence, CI gates, product, marketing, and release gaps.

## Repository layout / Repo yapısı

| Path | Purpose |
| --- | --- |
| `Assets/Scripts` | Core, gameplay, economy, events, persistence, UI, network, localization, analytics |
| `Assets/Scenes` | `Init`, `MainMenu`, and `Game` scenes |
| `Assets/Settings` | ScriptableObject configs and event channels |
| `Assets/Tests` | EditMode and PlayMode tests |
| `Packages` | Unity package manifest and lock file |
| `.github/workflows` | Test, Android AAB, and iOS build workflows |
| `Docs` | Architecture, coding standards, and delivery notes |
| `Tools` | Repository audit, Unity test, and developer bootstrap scripts |

## CI and release notes

GitHub Actions uses GameCI. Configure `UNITY_LICENSE`, `UNITY_EMAIL`, and `UNITY_PASSWORD` repository secrets before expecting cloud tests/builds to pass. Android store releases also require a repository-managed upload keystore and signing secrets. Fork pull requests do not receive secrets.

The Android workflow verifies that a real `.aab` file exists and that its required bundle entries are readable. The iOS workflow produces an unsigned Xcode project; Apple signing is still required for an IPA.

## License and contribution

See the repository settings and [CONTRIBUTING.md](CONTRIBUTING.md) for the current contribution and licensing policy.
