# Mixo Kitchen — seçilen A giriş ekranı

Kullanıcı üç tasarım arasından ilkini (A) seçti. Bu değişiklik açılış ekranını uygular; oyun tahtasının yeniden tasarımı değildir.

- Turkuaz mutfak, sıcak yemek sunumu ve krem/mercan logo için özgün arka plan.
- Unity tarafından çizilen mercan renkli, kenarlı ve basılma tepkili OYNA butonu.
- Yazı tipi gliflerinden bağımsız vektör ayarlar, ses ve bilgi simgeleri.
- Ses/titreşim tercihleri mevcut `mixo.sound` / `mixo.haptics` anahtarlarına kaydedilir.
- Yardım ekranı mevcut üçlü eşleştirme ve yedi yuvalı hazne kurallarını açıklar.
- Gerçek kayıtlı bölümü bilmeyen menüde sahte “Bölüm 1” yerine “Üçünü eşleştir” rozeti kullanılır.
- Oyun açılışı mevcut SceneRequest event channel üzerinden yapılır; kayıt silinmez.
- Arka plan en-boy oranını korur; geniş ekranlarda turkuaz çevre alanıyla tamamlanır. Kontroller SafeArea içinde kalır.

## Varlık ve üretim kaydı

Yerleşik Imagegen aracı kullanıldı, CLI kullanılmadı. Üretilen dosya projeye kopyalandı:
`Assets/Resources/MixoKitchen/UI/home-kitchen-a.png`.

Menü uygulaması: `Assets/Scripts/UI/StudioUIController.Menu.cs`.
Vektör simgeler: `Assets/Scripts/UI/MenuIconGraphic.cs`.

Referans: kullanıcıya sunulan A/B/C panosunun yalnız solundaki A tasarımı.
Üretimde kullanılan prompt:

> Production game art extraction/recreation from the supplied concept sheet. Use ONLY LEFT design A as the selected approved reference, preserve its artistic identity closely. Produce ONE portrait 9:20 full-bleed mobile game main menu background at high resolution, not three panels, not phone frame. Bright turquoise kitchen cabinetry, sunlit window left, soft plants at edges, warm coral and cream accents, appetizing central serving board with burger, sushi, pancakes, croissant and strawberries. Same beautiful stylized polished 3D-like art as A. Preserve large cream dimensional 'MIXO' logo and coral 'KITCHEN' below near upper center, occupying x 15%-85%, y 12%-28%; exact spelling only. No other writing anywhere: remove chalkboard sentences, text on vessels, letters A B C. Crucially REMOVE ALL UI: no gear, no buttons, no OYNA, no level badge, no sound or info controls, no painted placeholder panels. Those will be rendered as live Unity UI. Composition central food board occupies x 8%-92%, y 40%-66%. Bottom y 69%-100% must be quiet uninterrupted turquoise tabletop/soft teal shadow, with leaves only in far corners, ready for live UI overlay. Keep foods completely above y=67%. Warm window light, coherent shadows, restrained background detail; preserve the joyful inviting character of the approved LEFT A design. The central top logo and food board must not be cut off. Complete original game background asset, no watermark.

Logo arka planın parçasıdır; etkileşimli kontroller resme gömülü değildir. Arka plan statiktir; hareketli 3D mutfak değildir.

## Doğrulama

SceneSmokeTests artık menü ayarlarını açar, ses tercihinin değiştiğini kontrol eder, yardım penceresini açıp kapatır ve gerçek OYNA butonunun scene event üzerinden Game sahnesine geçtiğini doğrular. Testler görsel kaliteyi ölçmez; APK ve cihaz ekranı ayrıca doğrulanmalıdır.
