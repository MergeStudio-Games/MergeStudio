# Mimari

## Branch modeli
- `main`: yalnızca testleri ve yayın kontrolleri geçmiş production-ready kod; doğrudan push kapalı olmalı.
- `develop`: aktif geliştirme entegrasyonu; feature PR'larının hedefi.
- `feature/görev-adı`: develop'dan açılır, tek görev ve ilgili testleri içerir.
- `hotfix/açıklama`: main'den açılır; inceleme sonrası main'e ve develop'a taşınır.

GitHub required checks olarak Android/iOS test ve build job'larını seçin. En az bir inceleme isteyin. Sanatçı varlık ve meta'yı birlikte gönderir; tasarımcı Data Asset değişikliklerini PR ile sunar.

## Olay kanalları
MonoBehaviour sistemleri birbirine sahne hard-reference tutmaz. `Void/Int/Bool/StringEventChannelSO` varlıkları UnityAction event taşır. `EventListener<T>` OnEnable'da abone olur, OnDisable'da aynı kanaldan ayrılır. Generic bileşen doğrudan eklenmez; Inspector'a `IntEventListener`, `BoolEventListener`, `StringEventListener` eklenir. Kanalı ve UnityEvent yanıtını seçin. Bileşen aktifken kanal değiştirmek için önce disable edin.

Örnek: OrderSystem (OrderManager rolü) → OnGoldGained / GoldGained.asset → GameManager cüzdanı artırır → GoldChanged.asset → IntEventListener → CoinUIController.SetGold. GoldGained bir artış, GoldChanged toplam bakiyedir. UI böylece ödülü iki kez uygulamaz. Sunulan StudioUIController aynı kanalları dinler.

ServiceLocator yalnızca composition root için altyapıdır; gameplay mesajlaşmasının yerine geçmez. Saf C# modellerine constructor/metot parametreleriyle bağımlılık verilebilir. UI kendi çocuk widget referanslarını tutabilir; sahneler arası sistem referansı tutmaz.

## Oyun ve ekonomi
Varsayılan 7×9 tahta, aynı ID/seviyede merge, seviye üst sınırı 10. İki hücreye dokunma veya sürükle-bırak, boş hedefe taşıma ya da eşleşen öğeleri birleştirme isteği gönderir. UI yalnız event channel kullanır; `CellRequest=-1` bekleyen seçimi iptal eder. İlk sipariş tier-2 bread ister; tekrarlanan ödül engellenir. Economy.asset enerji kapasitesi, yenileme saniyesi, üretim fiyatı, bonus drop oranı ve mağaza paketini yönetir. FirstOrder.asset ödülü yönetir. ShopRequest kanalı elmas karşılığı enerji satın almayı bağlar; gerçek para satın alımı değildir. Yeni dokunma, mağaza ve responsive UI davranışlarının cihaz doğrulaması bekliyor.

Enerji UTC timestamp ile yenilenir, kapasitede biriken süre atılır. Saat geriye giderse referans zamanı sıfırlanır. Yerel saat ve yerel kayıt hileye dayanıklı sunucu otoritesi sağlamaz.

## Kayıt
SaveData v1: seviye, altın, elmas, enerji/zaman, tahta boyutları/hücreler ve tamamlanan siparişler. JSON, rastgele IV'li AES-256-CBC ve encrypt-then-HMAC-SHA256 ile saklanır. Geçici dosya flush edilir, eski dosya atomik replace ile .bak olur. Ana dosya bozuksa yedek denenir; ikisi de bozuksa hata görünür, üzerine boş oyun yazılmaz. DeleteSave ana/yedek/geçici kaydı temizler.

Anahtar persistentDataPath/save.key içinde yereldir; cihazı kontrol eden kullanıcıdan sır saklamaz. Üretimde platform keychain/keystore ve hesap tabanlı sunucu doğrulaması ekleyin. Şema sürümü bilinmiyorsa yükleme reddedilir; v2 için açık migration yazın. Cloud interface ve LocalOnlySaveProvider hazırdır; Firebase/PlayFab oturum, revision ve conflict policy entegrasyonu bekler.

## Addressables
ProjectSetup, Unity paket API'lerini kullanarak `UI_Remote`, `Characters_Remote`, `Events_Remote` gruplarını RemoteBuildPath/RemoteLoadPath ile oluşturur. İçerik henüz yoktur; kategorilerdeki sanat varlıklarını ilgili gruba atayın. Profilde RemoteLoadPath'i gerçek CDN adresi yapın, Addressables content build çıktısını CDN'e yükleyin. Bootstrap ve locale'ler yerel kalır.

Fast Follow: uygulama kurulumundan sonra erken indirilecek içerik. On Demand: oyuncu ihtiyaç duyunca indirilen karakter/etkinlik içerikleri. Bunlar Android Play Asset Delivery teslim modlarıdır; `_Remote` grup ismi PAD modunu etkinleştirmez. PAD istendiğinde ayrı Google PAD entegrasyonu, asset-pack yapılandırması ve cihaz testi gerekir; mevcut gruplar CDN tabanlıdır. iOS dağıtımı ayrıca planlanır.

`using var loader = new AddressableLoader(); using var lease = await loader.LoadAssetAsync<Sprite>(key);` örneğinde scope bitmeden asset kullanılmalıdır. LoadAssetAsync lease'i `Addressables.Release(handle)` ile, InstantiateAsync lease'i `Addressables.ReleaseInstance(handle)` ile kapanır. LoadAsset sonucuna ReleaseInstance uygulamak hatalıdır. Loader.Dispose unutulmuş lease'leri de kapatır; bekleyen yükleme dispose sonrası tamamlanırsa handle serbest bırakılır. Unity ana thread'inde kullanın; sahibi OnDestroy'da Dispose çağırmalıdır.

## Yerelleştirme ve ekran
İlk kurulum tr/en Locale, LocalizationSettings ve UI String Table koleksiyonunu oluşturur. UI'nin sekiz anahtarı LocalizationKeys'de tanımlıdır. Unity Localization cihaz dilini seçer; desteklenmeyen cihaz dilini ve font kapsamını cihazda doğrulayın. Örnek HUD altın/enerji başlıkları ve sipariş açıklaması geliştirme metnidir; final içerik tablosuna taşınmalıdır.

CanvasScaler 1080×2400 ve genişlik/yükseklik eşleşmesi 0.5 kullanır. SafeArea ekran/döndürme değişiminde normalize anchor uygular. 320×568, 390×844, 412×915 ve çentikli cihazlar test matrisidir; gerçek cihaz görsel QA henüz yapılmadı.

## CI ve kaynaklar
Android ve iOS build job'ları Linux üzerinde testMode=all koşan test job'una needs ile bağlıdır. iOS build macos-latest üzerinde GameCI native host yolunu kullanır. Test hatası build'i engeller. İlk import gerçek Unity varlıkları ürettiği için builder allowDirtyBuild açıktır. Feature push yalnız test çalıştırır. LFS checkout ve platform/bağımlılık bazlı Library cache vardır.

- [GameCI Builder](https://game.ci/docs/github/builder/): v4, macOS, androidExportType.
- [GameCI Test Runner](https://game.ci/docs/github/test-runner/): testMode=all.
- [Unity Localization](https://docs.unity3d.com/Packages/com.unity.localization@1.5/manual/index.html).
- [Unity Addressables](https://docs.unity3d.com/Packages/com.unity.addressables@2.3/manual/index.html).
