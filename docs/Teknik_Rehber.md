# Teknik rehber

## Bileşenler

- `backend/Iskur.Api` ASP.NET Core API, Identity, EF Core modelleri ve migration dosyaları.
- `frontend` React portalı. Geliştirme sırasında Vite, `/api` çağrılarını `127.0.0.1:5225` adresine iletir.
- `tests/Iskur.RulesTests` çalışma planı kuralları ve `tests/Iskur.FinalTests` gerçek SQL Server ile API kabul senaryoları.
- `docs/schema.sql` SQL Server için güncel EF modeli. Normal kurulumda uygulamanın migration komutu kullanılır.

Rol ve kapsam doğrulaması sunucudadır. Öğrenci kendi katılımına, danışman güncel atamalarına, bölüm başkanı çalışma biriminin bölümüne, fakülte yönetimi kendi fakültesine erişir. Parolalar Identity tarafından özetlenir. Tarayıcı oturum verisi bellekte tutulur. İzin ekleri yalnız öğrenci ve yetkili fakülte yönetimi tarafından indirilebilir. Kimlik ve IBAN yetkilendirmesi öğrenci profili ile raporda ayrı uygulanır. İşlem kayıtları, karar geçmişleri ve eski atamalar SQL Server'da saklanır.

Yoklama, izin kararı ve plan kararı aynı katılım için işlem kilidi kullanır. Plan düzenlemeleri sürüm alanıyla doğrulanır. Yoklama kaydı, çalışma günü ve oturumu için benzersizdir. Çalışma günü kararından önceki plan sürümleri saklanır. İzin belgesi ve talep aynı işlem içinde saklanır. Belgeler bu pakette SQL Server içinde en fazla 10 MB tutulur ve yedek kapsamına girer.

Kurum ortamında API ve React portalını aynı HTTPS adresi altında sunun. Dağıtım adresini `AllowedHosts` ayarına ekleyin. Data Protection anahtarları için erişimi sınırlandırılmış, kalıcı ve şifreli bir anahtar deposu kullanın. Geliştirme bağlantısı ve demo hesaplarını kurum dağıtım ayarlarına taşımayın. Erişim, rapor dışa aktarma ve yedekleme yetkilerini kurum sorumlularıyla belirleyin.

## Yedek ve geri yükleme

1. Migration öncesi `--backup-db` komutuyla kopya yedek alın. Ayrı yedek deposunun izinlerini ve boş alanını kontrol edin.
2. Uygulama yedeği `RESTORE VERIFYONLY WITH CHECKSUM` ile doğrular. Bu kontrol, yedeğin okunabilir olduğunu gösterir.
3. SQL Server Management Studio üzerinden ayrı bir test veritabanına geri yükleyin. Geri yükleme hedefini ve dosyaların yeni konumlarını kontrol edin.
4. Test kopyasında uygulamayı açın ve örnek öğrenci, plan, izin, görev, belge ve rapor kayıtlarını karşılaştırın. Doğrulama tarihini kaydedin.
5. Sürekli kullanımda kurumun yedekleme planına tam, fark ve gerekiyorsa işlem günlüğü yedeklerini ekleyin. SQL Server Express için Windows Görev Zamanlayıcı veya mevcut kurum yedekleme altyapısı kullanılabilir.

## Kurum onayı gerektiren varsayımlar

- Haftada 1–3 farklı tarih seçilir. Yarım gün haftalık sınırda bir tarih sayılır. Hafta pazartesiden pazara hesaplanır. İki aya yayılan haftada sonraki ayda çalışılabilecek ve planlanabilecek en az bir gün varsa haftalık alt sınır sonraki ayın planı gönderilirken kontrol edilir. Önceki ayın incelemeye gönderilmiş ve reddedilmemiş günleri alt sınırda sayılır. Üç gün üst sınırı iki ayın seçimleri toplamına uygulanır. Tam tatil ve işaretlenen sınav tarihleri haftalık alt sınırın uygun günlerinden çıkarılır.
- Aylık düzenleme aralığı tanımlıysa aralık günleri dahil uygulanır. Tanım bulunmadığında ay sonu kapanış kabul edilir.
- Varsayılan mesai 08.00–12.00 ve 13.30–17.00'dır. Öğrenci ilk sabah oturumunda yoklama verdiğinde izinli olmayan ikinci planlı oturum da kabul edilir. Sabah kaçırıldığında öğleden sonra kaydı yalnız ikinci oturumu kapsar.
- İzin hakkı başlangıçta katılım başına 10 gün, kullanım tam gün için 1 ve yarım gün için 0,5 gündür. Sağlık raporlarının haktan düşümü yönetim ayarına bağlıdır. Talep oluşturulduğundaki kullanım miktarı geçmişte saklanır.
- Varsayılan 2 gün izin uyarı eşiği ve 3 gün devamsızlık eşiği kurum tarafından değiştirilebilir. Uyarı, idari karar başlatmak için referanstır.
- İş saatleri yoklaması kabul edilen planlı sürelerdir. Fiziksel kalış süresi ve bordro hesabı süreç dışındadır.
- Sabit ulusal günler örnek veriye eklenir. Dinî bayram ve idari tatil tarihleri fakülte yönetimi tarafından girilir.
