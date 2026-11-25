# Kabul testleri

## Otomatik kontroller

```powershell
dotnet build IskurOtomasyonu.sln
dotnet run --project tests/Iskur.RulesTests
npm --prefix frontend ci
npm --prefix frontend run build
node --test tests/calendar.test.mjs tests/navigation.test.mjs
```

API kabul testi yazma işlemleri yapar. Her çalıştırmada **ayrı ve boş bir test veritabanı** oluşturun. Test, örnek hesapları ve planları kullanır.

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:ConnectionStrings__Default = 'Server=.\SQLEXPRESS;Database=IskurFinalKabul;Trusted_Connection=True;TrustServerCertificate=True'
$env:ISKUR_DEMO_PASSWORD = Read-Host 'Demo test parolası'
dotnet run --project backend/Iskur.Api -- --init-db
dotnet run --project tests/Iskur.FinalTests
Remove-Item Env:ISKUR_DEMO_PASSWORD
```

| Senaryo | Beklenen sonuç |
|---|---|
| Öğrenci profili ve filtreleme | Profil değişikliği kaydedilir. Ay, yıl ve bölüm seçimi ilgili öğrencileri getirir |
| Oturumsuz ve kapsam dışı istek | 401/403, başka öğrenciye erişim engellenir |
| TC ve IBAN alanı | Danışman profilinde ve bölüm başkanı dışa aktarmasında yer almaz |
| Yarım günlük izin ve ek belge | 0,5 gün bekleyen ve kullanılmış hak, öğrenci ve fakülte rollerinde indirme |
| İzin talebine ikinci karar | İlk karar ve bakiye korunur |
| Görev ve değerlendirme | Öğrenci güncel durumu ve tarihli notu görür |
| QR sayfası ve çalışma birimi | Danışman kendi biriminin kodunu açar. Öğrenci kodu okutur |
| QR penceresi ve belirteç süresi | İlk 20 dakikada geçerli, 30 saniyede süresi dolan kod reddedilir |
| Mükerrer QR ve izin | İkinci kayıt veya onaylı izne yoklama engellenir |
| Sabah QR aktarımı | İki oturumlu günde 450 dakika ve ayrı kaynak kaydı |
| Devamsızlık | Danışman bildirir, bölüm başkanı iletir, fakülte gerekçeli işlem yapar |
| Aylık döküm | Planlanan günler, gerçekleşen 450 dakika ve izinli gün sayısı doğru hesaplanır |
| Planın alt sınırı ve düzenleme aralığı | Eksik hafta veya kapalı düzenleme aralığı reddedilir |
| Kurum kayıtları | Dönem ve birim eklenir, yeni katılım önceki dönemi korur |
| Denetim kayıtları | Profil, izin, yoklama, görev ve rapor işlemleri, işlemi yapan kullanıcı ile kaydedilir |
| İki aya yayılan çalışma haftası | Kasım planı 30 Kasım seçilmeden gönderilir. Ortak hafta için 1–4 Aralık arasında gün seçilebilir. Haftanın iki ayındaki toplam üç günü aşamaz. |
| Plan geçmişi | Sınav günü ve önceki sürüm saklanır |

## Yerel gösterim

1. Vize veritabanı kopyasında profil ve eski planı açın. Yükseltmeden sonra aynı kayıtları tekrar açın.
2. Öğrenci olarak bir plan kaydedip gönderin, danışman olarak seçili günlere karar verin.
3. Bir öğrenci için izin formu, dosya indirme ve karar akışını deneyin.
4. Uygun mesai saatinde danışman hesabında **Yoklama karekodu → Karekodu göster** ile kodu açın. Öğrenci olarak ikinci bir tarayıcıda **Yoklamam** ekranından kodu okutun.
5. Ders sayısından bağımsız olarak seçilmemiş bir günün gri kaldığını, saatlerin sarı etiketlerde göründüğünü doğrulayın. Takvimde ay seçin, başka ekrana geçin, tarayıcı ve uygulamanın geri düğmeleriyle aynı aya dönün.
6. Hesap kapsamını dört rolle karşılaştırın. Yönetim ve bölüm başkanı için dışa aktarılan CSV sütunlarını kontrol edin.
