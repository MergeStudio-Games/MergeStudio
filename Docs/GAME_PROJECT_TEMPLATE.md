# MergeStudio Games game-project template / oyun proje şablonu

## English

### Local-first generator

```powershell
powershell -ExecutionPolicy Bypass -File Tools/New-GameProject.ps1 -Repository GardenMerge -ApplicationId com.mergestudio.gardenmerge -Directory D:\Games\GardenMerge -LocalOnly
```

The generator exports the selected committed revision (`develop` by default),
sets company/product/application identifiers, and initializes an independent Git
history. Existing destination folders are rejected. `-LocalOnly` creates no
remote repository. Omit it to commit and publish a new repository with GitHub CLI;
there are no force pushes. Configure Git identity before publishing. Use `-Revision`
only when deliberately testing another reviewed commit. Uncommitted template changes
are never copied. `Docs/GAME-IDENTITY.json` records the originating template commit.

Unity repair/build preserves the product identity in ProjectSettings. Game-specific
README, repository links, CODEOWNERS, privacy and signing configuration still require
review; the generator does not claim to personalize those documents automatically.

### Recommended creation flow

1. Open the [MergeStudio Games repositories](https://github.com/orgs/MergeStudio-Games/repositories), select **MergeStudio**, then choose **Use this template → Create a new repository**.
2. Create the new repository under `MergeStudio-Games` with a short game name such as `GardenMerge`. Choose public or private visibility deliberately.
3. Clone the new repository, open it in Unity, and run `Tools/Bootstrap-Developer.ps1`.
4. Update the company name, product name, Android/iOS identifiers, and README before feature work.
5. Invite contributors with the least access they need. Work on `feature/<short-name>` branches and open pull requests into `develop`.

The template includes Unity settings, the package manifest, test layout, event-channel conventions, CI workflows, AI instructions, issue forms, and the pull-request checklist. It is a starting point: generated Unity folders and game-specific data belong to the new game and must not be copied between games.

### Naming and repository properties

- Repository: short `PascalCase` game name, no spaces.
- Unity product name: the player-facing name.
- Android/iOS identifiers: `com.mergestudio.<game-slug>`.
- Topics: `unity`, `unity6`, `mobile-game`, the genre, and the platform.
- Description: one sentence explaining the game and its current status.

### After cloning

The repository root is the developer workspace. Read `AGENTS.md`, open the pinned Unity version, run the bootstrap audit, and work in a feature branch. Giving the repository root to Codex or Claude is enough; the agent instructions and contribution rules travel with the game repository.

### Shared versus game-specific

Shared: code standards, branch flow, CI shape, test conventions, save/recovery rules, localization approach, and AI collaboration rules.

Game-specific: scenes, art, balance, product identifiers, remote content, analytics keys, ads, store signing, and production secrets. Configure these in the new repository; never put secrets in the template.

## Türkçe

### Önerilen oluşturma akışı

1. [MergeStudio Games repos](https://github.com/orgs/MergeStudio-Games/repositories) sayfasında **MergeStudio** reposunu açın ve **Use this template → Create a new repository** seçin.
2. Yeni repo’yu `MergeStudio-Games` altında `GardenMerge` gibi kısa bir oyun adıyla oluşturun. Public/private kararını bilinçli verin.
3. Yeni repo’yu clone edin, Unity’de açın ve `Tools/Bootstrap-Developer.ps1` çalıştırın.
4. Özellik geliştirmeden önce şirket adını, ürün adını, Android/iOS uygulama kimliklerini ve README’yi güncelleyin.
5. Geliştiricilere ihtiyaçları kadar erişim verin. `feature/<kisa-ad>` dallarında çalışıp `develop` dalına pull request açın.

Şablon; Unity ayarlarını, paket manifestini, test yapısını, event channel kurallarını, CI akışlarını, AI talimatlarını, issue formlarını ve pull request kontrol listesini içerir. Bu bir başlangıç noktasıdır; üretilen Unity klasörleri ve oyuna özel veriler yeni oyuna ait olmalı, oyunlar arasında kopyalanmamalıdır.

### İsimlendirme ve repo ayarları

- Repo: boşluksuz, kısa `PascalCase` oyun adı.
- Unity ürün adı: oyuncunun göreceği ad.
- Android/iOS kimlikleri: `com.mergestudio.<oyun-slug>`.
- Konular: `unity`, `unity6`, `mobile-game`, oyun türü ve platform.
- Açıklama: oyunu ve mevcut durumunu anlatan tek cümle.

### Clone sonrasında

Repo kökü geliştiricinin çalışma alanıdır. `AGENTS.md` dosyasını okuyun, sabit Unity sürümünü açın, bootstrap kontrolünü çalıştırın ve feature dalında çalışın. Repo kökünü Codex veya Claude’a vermeniz yeterlidir; AI kuralları ve ekip çalışma standartları oyun repo’su ile birlikte gelir.

### Ortak ve oyuna özel içerik

Ortak içerik: kod standartları, branch akışı, CI yapısı, test kuralları, kayıt/kurtarma yaklaşımı, localization düzeni ve AI çalışma kurallarıdır.

Oyuna özel içerik: sahneler, sanat, denge, ürün kimlikleri, uzak içerik, analytics anahtarları, reklamlar, mağaza imzası ve üretim secret’larıdır. Bunları yeni repo’da yapılandırın; secret’ları template’e koymayın.
