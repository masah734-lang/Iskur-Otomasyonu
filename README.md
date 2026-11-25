# İŞKUR Öğrenci Çalışma Takip Sistemi · Final

Teknoloji Fakültesi için ASP.NET Core 10, Entity Framework Core 10, Identity, React ve SQL Server tabanlı öğrenci çalışma otomasyonu. Final sürümü 5 Kasım tarihli güncel vize paketindeki uygulama ve veritabanı üzerinden geliştirilmiştir.

## Windows'ta çalıştırma

.NET 10 SDK, Node.js 22.12 veya üzeri ve SQL Server Express 2022 veya üzeri gerekir. Visual Studio'da `IskurOtomasyonu.sln` açılabilir.

1. GitHub deposunu bilgisayarınıza klonlayın ve proje klasörünü açın.
2. SQL Server Express `.\SQLEXPRESS` örneğini başlatın.
3. **Backend-Baslat.bat** dosyasını açın. Yeni kurulumda **E** seçin ve demo parolası belirleyin. Mevcut `IskurVize` veritabanına bağlanıyorsanız önce yedek alın, sonra **H** seçin. Başlatıcı şemayı yükseltip API'yi `http://127.0.0.1:5225` adresinde açar.
4. **Frontend-Baslat.bat** dosyasını açın. `http://127.0.0.1:5173` üzerinden giriş yapın.

LocalDB veya farklı örnek için proje kökünde PowerShell açın. Yeni demo veritabanı istediğinizde kurulum betiğini çalıştırın.

```powershell
.\scripts\Setup.ps1 -SqlServer '(localdb)\AkilliYurtLocal17' -Database 'IskurFinalDemo'
$env:ConnectionStrings__Default = 'Server=(localdb)\AkilliYurtLocal17;Database=IskurFinalDemo;Trusted_Connection=True;TrustServerCertificate=True'
.\Backend-Baslat.bat
```

Ayrı bir pencerede `Frontend-Baslat.bat` çalıştırın. Daha önceki `IskurVize` kayıtlarını kullanmak için bağlantıdaki veritabanı adını kendinize uygun değerle ayarlayın.

### Mevcut vize veritabanı

Yükseltme, vize paketinin `EnsureCreated` ile oluşturduğu şemayı ilk migration olarak tanır ve final tablolarını ekler. Önce doğrulanabilir bir yedek alın ve veritabanının bir kopyasında deneyin. İlk çalıştırmada SQL Server hesabının tablo oluşturma yetkisi gerekir. Geçmiş profil, plan ve danışman atamaları aynı kimliklerle saklanır.

```powershell
$env:ConnectionStrings__Default = 'Server=.\SQLEXPRESS;Database=IskurVize;Trusted_Connection=True;TrustServerCertificate=True'
$env:ISKUR_BACKUP_DIRECTORY = 'C:\SqlBackups'
dotnet run --project backend/Iskur.Api -- --backup-db
dotnet run --project backend/Iskur.Api -- --upgrade-db
```

Yedek dizini SQL Server hizmetinin çalıştığı makinede bulunmalı ve hizmet hesabı tarafından yazılabilmelidir. Komut kopya yedek oluşturur ve SQL Server ile doğrular. Gerçek geri yükleme denemesi için [teknik kılavuzu](docs/Teknik_Rehber.md) kullanın.

### Demo hesapları

| Rol | Hesap |
|---|---|
| Fakülte yönetimi | `yonetim@demo.local` |
| Bölüm başkanı | `baskan@demo.local` |
| Danışman | `danisman@demo.local` |
| Diğer danışman | `diger.danisman@demo.local` |
| Öğrenciler | `ogrenci1@demo.local`, `ogrenci2@demo.local`, `ogrenci3@demo.local` |

Ortak demo parolası kurulum sırasında belirlenir. `Parola-Sifirla.bat` mevcut demo parolasını değiştirir. Demo verileri sentetiktir.

## Uygulama akışı

1. Yönetim hesabıyla öğrenci, çalışma birimi ve danışman atamalarını açın. Gerekirse **Kurum yönetimi** ekranından birim, bölüm, personel ve program dönemi ekleyin.
2. Öğrenci hesabıyla **Ders programım** ekranından dersleri kaydedin. **Aylık çalışma planım** ekranında, ders saatleri gün kutusunun içindeki sarı etiketlerde görünür. Taslağı kaydedip incelemeye gönderin. Yönetim veya danışman her gün için ayrı karar verir.
3. Öğrenci onaylı bir çalışma tarihi için **İzinlerim** ekranından talep açar. Fakülte yönetimi öğrenciyi seçip **İzin talepleri** ekranından karar verir.
4. Danışman veya yetkili personel kendi hesabında **Yoklama karekodu → Çalışma birimi → Karekodu göster** yolunu izler. Öğrenci ayrı bir oturumda **Yoklamam** ekranını açar. Yetkilinin ekranındaki kodu kamerayla veya QR görseliyle okur ve **Yoklama ver** düğmesine basar.
5. Danışman **Görevler** ekranında görev atar, not ekler ve tamamlanma durumunu günceller. **Yoklama ve devamsızlık** ekranındaki bildirimi bölüm başkanı fakülte yönetimine iletebilir.
6. **Raporlar** ekranında ay, yıl ve diğer filtreleri seçin. Fakülte yönetimi ve bölüm başkanı CSV alabilir. Kimlik ve IBAN sütunları fakülte yönetimi dökümünde bulunur.

Sayfalar arasında tarayıcının geri ve ileri düğmeleriyle geçebilirsiniz. Üst menüdeki **Geri dön** düğmesi de bir önceki uygulama sayfasını açar. Çalışma planında seçili ay adres geçmişinde saklanır.

Demo planları Kasım 2026 dönemine aittir. Canlı yoklama için öğrenciye atanmış birim, o gün için onaylanmış çalışma oturumu ve olası izin kaydının kontrolü gerekir. Karekod Türkiye saatiyle 08.00–08.20 ve 13.30–13.50 arasında açılır, 30 saniyede bir yenilenir. Tek bilgisayarda iki rolü denemek için ikinci tarayıcı penceresinde veya gizli pencerede öğrenci hesabıyla giriş yapıp kodu görsel olarak yükleyebilirsiniz. Başka cihazdan kamera kullanımı için portalın HTTPS üzerinden erişilebilir olması gerekir.

## Kurallar ve doğrulama

Varsayılan izin hakkı katılım başına 10 gündür. Her onaylı oturum 0,5 gün sayılır. Fakülte yönetimi toplam hakkı, uyarı eşiklerini ve sağlık raporu istisnasını **Program ayarları** üzerinden belirler. Katılımı sonlandırma kararı yetkili kullanıcı tarafından gerekçesiyle kaydedilir. Raporlanan çalışma saatleri kabul edilen oturumların süresidir. Ödeme hesabı kurum kurallarına bağlıdır.

- [Teknik rehber, yedekleme ve kurum kararları](docs/Teknik_Rehber.md)
- [Kabul senaryoları ve çalıştırma komutları](docs/Kabul_Testleri.md)
- [Doğrulama sonuçları](docs/Dogrulama.md)
- [API uçları](docs/API.md)
- [Final veritabanı ilişkileri](docs/ER_Final.mmd) ve [SQL Server şeması](docs/schema.sql)
