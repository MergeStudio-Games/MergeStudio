# Mixo Kitchen — 100 oyunluk tasarım karşılaştırması

İnceleme tarihi: 27 Eylül 2026. Sonuç: **Mixo Kitchen henüz üst düzey görsel kaliteye ulaşmadı.** Bu değerlendirme bir tasarım incelemesidir; kullanıcı araştırması sonucu veya mağaza sıralaması değildir.

## Kapsam ve kanıt

Google Play'deki 100 ayrı uygulamanın sayfası ve her birinin ilk iki mağaza görseli incelendi. Bunların içinden 12 oyunda toplam 70 görsel ayrıntılı incelendi; ilk taramadaki tekrarlar çıkarılınca **246 farklı mağaza görseli** değerlendirildi. Seçim: 20 match, 20 merge, 20 tile/nesne eşleştirme, 15 yemek, 15 genel mobil oyun/oyun platformu ve 10 sanat yönü referansı. Roblox bir oyun platformudur; tek bir oyun sanat dili gibi değerlendirilmedi.

Bu liste **Google Play'in resmî en iyi 100 tasarım sıralaması değildir**. Tür yakınlığı ve farklı görsel yaklaşımlar üzerinden seçilmiş referans kümesidir; indirme, gelir veya beğeni sıralaması iddiası yoktur. US/en mağaza sayfaları kullanıldı; Türkiye sıralaması veya Türkiye'de erişilebilirlik ölçülmedi. Kingdom Rush aramasıyla bulunan ürün Kingdom Rush 5: Alliance TD olarak kaydedildi.

Kaynaklar ve oyun başına gözlemler: [100 oyunluk kayıt](MIXO-KITCHEN-DESIGN-100.md). Mağaza sayfaları, ekran dosyaları ve inceleme panoları yerelde `Build/Research/DesignBenchmark` altında tutuluyor; oyunun varlıklarına veya dağıtımına eklenmedi. Görseller yayıncıların tanıtım materyalleridir. Özellikle Royal Match'in erişilen görselleri kurtarma sahneleriydi: bunlardan standart oyun HUD'ı, zorluk veya gerçek oynanış akışı çıkarılmadı.

Mixo karşılaştırma kanıtı: `Build/VisualQA/MixoKitchen-Premium.png` ve mevcut `StudioUIController.cs`. Görüntü önceki görsel sürüme aittir; son APK'nın yeni duraklatma ekranını ve değişen yemek dağılımını göstermiyor. Son otomatik ekran yakalamaları boş olduğundan güncel cihaz görsel doğrulaması yapılmış sayılmadı. 43 testin geçmiş olması görsel kalite kanıtı değildir.

## Mixo'nun mevcut durumu

| Alan | Gözlenen sorun | Tasarım kararı |
|---|---|---|
| Ekran odağı | Raflar, bitkiler, metal kaplar ve yemekler aynı anda dikkat çekiyor. | Oyun merkezini sade, mat bir tezgâh/tepsi yüzeyi yap; dekoru üst kenara taşı. |
| Mekân ve perspektif | Yemekler dikey mutfak cephesi önünde havada duruyor. | Oyun yüzeyi ve yiyecekler aynı kamera açısı, ışık ve temas gölgesine sahip olsun. |
| Sanat bütünlüğü | Ayrıntılı arka plan, parlak yemek kesitleri ve düz dikdörtgen butonlar farklı diller konuşuyor. | Tek stilize 3D/2.5D yemek dili; tüm UI için ortak malzeme, kenar ve gölge kuralları. |
| Arayüz oranları | Yedi yuva uzun dikdörtgenler; üç alt buton büyük renk blokları. | Kareye yakın yuvalar, kompakt araç ikonları; kazanılan alanı oyun yüzeyine ver. |
| Yemek okunurluğu | Sık üst üste binme ve benzer yuvarlak tabanlar siluetleri zayıflatıyor. | Nesneyi görselde küçük tutarken seçilebilir alanı koru; siluet ve katman ayrımını test et. |
| İçerik ve kimlik | 36 yemek var; bölüm teması ve ilerleme ekranı güçlü bir marka hikâyesi oluşturmuyor. | Tematik mutfak bölümleri, tarif kitabı ve özgün şef/servis kimliği geliştir. |
| Hareket ve ses | Kodda seçim uçuşu, basit parçacıklar ve üretilmiş tonlar var. | Seçim, üçlü, kombo, sipariş ve bölüm bitişi için farklı geri bildirim tasarla; kayıttan doğrula. |
| Kalite güvencesi | Son APK'nın gerçek cihaz görüntüsü ve performans kaydı yok. | Görsel onay, dokunma testi ve cihaz profili tamamlanmadan yayın kalitesi iddiası kurma. |

## Ayrıntılı incelenen 12 referans

Buradaki çıkarımlar mağaza görsellerinin görsel okumasıdır; oyunlar bu çalışma sırasında oynanmadı.

| Referans | Görselde görülen güçlü taraf | Mixo'ya aktarılacak ilke |
|---|---|---|
| [Royal Match](https://play.google.com/store/apps/details?id=com.dreamgames.royalmatch) | Kurtarma kompozisyonunda ana hareket, renkli taşlar ve karakter kolay seçiliyor. | Bir ekranda tek ana eylem ve belirgin odak; tanıtımı oynanış kanıtıyla karıştırma. |
| [Travel Town](https://play.google.com/store/apps/details?id=io.randomco.travel) | Ek görsellerde sakin kum rengi tahta üzerinde çok sayıda küçük ve ayrı siluetli nesne var. | Çeşitliliği arka plan ayrıntısıyla değil nesne siluetleriyle oluştur. |
| [Gossip Harbor](https://play.google.com/store/apps/details?id=com.mergegames.gossipharbor) | Hikâye, mekân yenileme ve mavi merge tahtası farklı kompozisyonlar olarak sunuluyor. | Hikâye/dekor ekranını yoğun eşleştirme yüzeyinden ayır. |
| [Merge Cooking](https://play.google.com/store/apps/details?id=com.merge.cooking.theme.restaurant.food) | Malzeme, yemek, müşteri ve restoran ilişkisi görsel olarak kuruluyor. | Yiyecek toplamanın tarif/servis amacı olsun. Lisanslı karakterleri referans alma. |
| [Triple Match 3D](https://play.google.com/store/apps/details?id=com.master.triple3d.find) | Sade koyu zemin, üstte hedefler, altta ince hazne; renk temalı nesne yığınları. | Bize en yakın ekran hiyerarşisi. Hedef → yemek alanı → hazne sıralaması. |
| [Triple Tile](https://play.google.com/store/apps/details?id=com.tripledot.triple.tile.match.pair.game.three.master.object) | Tutarlı taş kenarı ve simgeler; farklı zeminlerde değişmeyen nesne dili. | Tema değişse bile taş/yiyecek okunurluğu ve boyut dili değişmesin. |
| [Tile Busters](https://play.google.com/store/apps/details?id=com.spyke.tilebusters) | İnce hazne, küçük araç ikonları, katmanı okunabilen taşlar. | Alt arayüzü küçült; görsel katmanları açıkça ayır. |
| [Triple Find](https://play.google.com/store/apps/details?id=and.lihuhu.findmatch) | Hedef nesne + sayı kartları; tematik yığın ve sade oyun zemini. | Oyuncu ne topladığını tek bakışta görsün; hedefli sipariş sistemi tasarla. |
| [Good Pizza, Great Pizza](https://play.google.com/store/apps/details?id=com.tapblaze.pizzabusiness) | Karakter, tezgâh, yiyecek ve mekânda aynı çizgi/palet dili. | Kaliteyi detay yoğunluğu değil stil bütünlüğü belirlesin. |
| [Good Coffee, Great Coffee](https://play.google.com/store/apps/details?id=com.tapblaze.coffeebusiness) | Malzemeler işlevsel istasyonlarda; tarif hazırlama alanı belirgin. | Yiyecek ailesi ve üretim aşamalarını anlaşılır gruplar halinde sun. |
| [Animal Restaurant](https://play.google.com/store/apps/details?id=droidhang.twgame.restaurant) | Sınırlı palet, tutarlı çizgi, karakterlerle tariflerin aynı dünyaya ait olması. | Logodan sonuç ekranına kadar tanınabilir Mixo kimliği oluştur. |
| [Monument Valley](https://play.google.com/store/apps/details?id=com.ustwo.monumentvalley) | Kontrollü renk, boşluk ve gölge; tek odaklı sahneler. | Boşluğu koru, ancak oyun alanıyla ilgisiz büyük boşluk bırakma. Tür mekaniğini kopyalama. |

## Önerilen sanat yönü

**Mixo Kitchen: sıcak, canlı, stilize bir dünya mutfağı atölyesi.** Koyu petrol yeşili çerçeve, krem oyun yüzeyi ve mercan vurgu korunabilir. Fotoğraf benzeri mutfak yerine yemeklerle aynı üretim diline sahip sade bir set gerekir. Yemekler parlak ama küçük boyutta seçilebilir olmalı; tek ışık yönü ve kamera kullanılmalı. Tüm öğeleri aynı beyaz disk üzerine koymak yerine gerçek yemek siluetlerini koruyan temas gölgeleri denenmeli.

Özgünlük için öneri: İstanbul kahvaltısından başlayıp farklı mutfaklara açılan tarif yolculuğu; simit, börek, menemen, mantı ve baklava gibi yerel öğeler global yemeklerle aynı görsel standarda sahip olsun. Bu içerik henüz uygulanmadı. Mevcut 36 görselin kalitesi standartlaştırıldıktan sonra 60 ayrı yemeğe genişleme öneriliyor; yalnız renk değişimleri yeni çeşit sayılmamalı.

## Uygulama sırası ve kabul koşulları

Aşağıdaki sayılar rakiplerden ölçülmüş veriler değil, önerilen iç hedeflerdir.

1. **Tek ekranı tamamla.** Önce HUD, oyun yüzeyi, hazne ve araçları yeniden düzenle. Güvenli alan içinde başlangıç bütçesi: üst bilgi %12, oyun alanı %60, hazne %10, araçlar %10, aralıklar %8. Taslak, gerçek farklı ekranlarda sınanarak ayarlansın.
2. **12 yemekle sanat standardını kanıtla.** Meyve, ana yemek, tatlı ve içecekten farklı siluetler seç. Aynı kamera, ölçek, ışık ve gölgeyle üret; küçük boyutta benzer yiyecekleri ayırt etme testi yap. Başarılı olduktan sonra tüm kataloğa uygula.
3. **Hedef ve servis döngüsünü ekle.** Üstte en fazla üç sipariş kartı; eşleşme sonucu hedef ilerlesin. Tarif kitabı ve mutfak açılışı, dekorasyon ekranında gösterilsin. Bugünkü sistemin tüm taşı temizleme mekaniğinden ayrı bir geliştirmedir.
4. **Hareket/ses paketini tamamla.** Seçim için kısa tepki; üçlü için servis vurgusu; komboda kontrollü artış; kazanma/kaybetmede farklı ses ve hareket. Hazne doluluğu yalnız renkle anlatılmasın. Yoğun efekt hedefleri ve yuvaları örtmesin.
5. **İlk 10 bölümü elle dengele.** Başlangıçta az tür ve temiz görünürlük; ileride benzer siluetler, katmanlar ve siparişler sırayla açılsın. Zorluğu yalnız öğeleri küçülterek veya sayıyı artırarak kurma. 100 bölümün otomatik üretimi, tasarlanmış 100 bölümle eşdeğer değildir.
6. **Ölçerek onayla.** 360×800, 390×844, 412×915 ve tablet düzenlerini kontrol et. Önerilen pilot: 10 yeni oyuncudan en az 9'u ilk hedefi yardım almadan anlasın; tanıma görevlerinde üç saniye hedeflensin. Bunlar henüz ölçülmedi. Seçilen orta seviye telefonda 20 dakikalık oturum, kare süreleri, bellek, yanlış dokunuşlar ve ısınma kaydedilsin; 60 FPS hedefi henüz doğrulanmış değil.

“En iyi” iddiası bu rapordan çıkmaz. Hedef, benzer oyunlarla yan yana gösterildiğinde tutarlı, özgün ve anlaşılır bulunan bir ürün oluşturmak; bunu kullanıcı denemeleri ve gerçek cihaz kayıtlarıyla kanıtlamaktır.
