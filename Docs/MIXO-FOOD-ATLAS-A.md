# Mixo Kitchen — A stilinde yemek seti ve oyun yüzeyi

36 yemek için yeni, tutarlı ışık ve kamera açısına sahip bir atlas üretildi. Kaynak dosya değiştirilmeden `Assets/Resources/MixoKitchen/UI/food-atlas-a.png` içine kopyalandı. Yerleşik Imagegen aracı kullanıldı; CLI kullanılmadı.

Sabit 6×6 kare kesimi yerine her ikonun alfa kanalındaki ana bağlı bileşen sınırı ve iki piksellik pay kullanıldı. Dikdörtgenler `food-atlas-a-layout.json` verisinde tutulur. Unity `FoodCatalog` bu sınırlarla sprite oluşturur; dosya üzerinde yeniden boyama, yeniden örnekleme veya arka plan silme yapılmadı. Atlas, boyutların değişmemesi ve küçük ikon kenarlarının sıkıştırmadan zarar görmemesi için mipmapsız ve sıkıştırmasız yüklenir. Bu, GPU belleği ile ikon keskinliği arasında bilinçli bir tercihtir.

Yeni otomatik kontrol 36 benzersiz adı, ortak atlası ve kesim dikdörtgenlerinin birbirine taşmamasını doğrular. Bu kontrol tek başına sanat kalitesini veya yanlış yemek tanınma oranını ölçmez.

Oyun yüzeyi fotoğraf benzeri mutfak yerine sakin turkuaz çevre ve krem servis tepsisidir. Yiyeceklerin arkasındaki yuvarlak diskler kaldırıldı; gölge yiyecek siluetine uygulanır. Alt hazne ve araçların yüksekliği azaltıldı. Seçilen A menüsündeki kenarlı buton dili araçlara, devam etme ve sonuç eylemine taşındı.

İçerik sayısı hâlâ 36'dır; bu çalışma 36 yeni tür eklememiş, mevcut kataloğun görsellerini yenilemiştir. 100 bölümün insanlarla dengelendiği veya oyunun mağaza yayınına hazır olduğu iddia edilmez.

## Doğrulama — 27 Eylül 2026

- Unity XML raporları: 41 EditMode ve 3 PlayMode testi; sıfır hata ve atlanan test.
- Statik depo kontrolü: 60 C# dosyası, sıfır hata.
- Windows oyuncusunda gerçek menü, oyun, duraklatma ve kazanma akışı görüntülendi. Kazanma ekranına oyun öğelerine tıklanarak ulaşıldı.
- Yerel görüntüler: `Build/VisualQA/final-menu.png`, `final-game.png`, `final-pause.png`, `final-result.png`. Bunlar Android cihaz görüntüsü değildir.
- Fiziksel Android cihazı son bağlantı kontrolünde ADB listesinde görünmedi; yeni sürümün telefonda doğrulandığı iddia edilmez.

## Üretim promptu

> Use case: stylized-concept. Asset type: production sprite atlas for Mixo Kitchen, a bright cheerful polished mobile food puzzle. Generate exactly one SQUARE image, ideally 1536x1536 or 2048x2048, with REAL transparent alpha background. Exactly 6 columns and 6 rows in a mathematically REGULAR invisible grid, 36 equal square cells. Every object centered exactly in its own cell, with at least 18% fully transparent padding on ALL FOUR sides of each cell; artwork must fit entirely within middle 64% of cell. No objects may touch another cell. No frame, grid lines, text, labels, plates behind objects, props, drop shadow outside silhouette, checkerboard texture or background color. Clean alpha edges, no fringe, no noise or stray pixels. Consistent premium soft stylized 3D mobile game rendering, simplified appetizing shapes, subtle highlights, warm upper-left light, same 3/4 camera, saturated but controlled palette. Consistent visual weight and perceptual size. Each silhouette easily identifiable at 64 pixels, not a photograph. Exactly one food object/group per cell. Row 1 left-to-right: hamburger, red carton french fries, triangular pizza slice, croissant, pink cupcake, pink ring donut. Row 2: red apple with leaf, avocado half with pit, strawberry, yellow lemon, red tomato with green top, orange carrot with leaves. Row 3: sushi maki roll with salmon center, salmon nigiri, folded taco, stack of pancakes, square waffle with butter, ice cream cone. Row 4: dark chocolate bar, round chocolate chip cookie, fried egg, Swiss cheese wedge, broccoli, brown mushroom. Row 5: roast chicken, grilled steak, hot dog, red white popcorn carton, white latte cup, pink smoothie cup. Row 6: dark ramen bowl, green salad bowl, single birthday cake slice with candle, watermelon wedge, bunch of purple grapes, pineapple. Precise grid alignment and generous EMPTY gutters are more important than filling the image. Cohesive production-ready food icons. Absolutely transparent outside each isolated food, no background marks.

Üretilen gerçek kaynak boyutu 1254×1254'tür. Modelin düzenli ızgara/padding isteğini birebir uyguladığı varsayılmadı; gerçek nesne sınırlarının kullanılmasının nedeni budur.
