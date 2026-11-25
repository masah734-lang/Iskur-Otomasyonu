# Final teslim kontrolü

| Özellik | Doğrulanan sonuç |
|---|---|
| Veritabanı ve ilişkiler | SQL Server 2022'de sıfır kurulum ve mevcut vize şemasından yükseltme başarılı. Profil ve plan kayıtları korundu. |
| Öğrenci profili ve çalışma planı | Profil değişikliği kaydedildi. Aylık plan, haftalık sınır, sınav günü, düzenleme aralığı ve plan geçmişi doğrulandı. Ders saatleri takvimde sarı etiketlerde gösterildi. |
| Rol bazlı yetkilendirme | Öğrenci, danışman, bölüm başkanı ve fakülte yönetiminin erişim kapsamları doğrulandı. Kimlik ve IBAN yetkisiz erişimlere kapalı. |
| Ay/yıl ve bölüm filtreleri | Ay, yıl ve bölüm birlikte seçildiğinde ilgili öğrenci kayıtları döndü. |
| İzin talebi ve izin hakkı hesabı | Yarım gün izin, belge, onay, kalan hak ve ikinci kararın engellenmesi doğrulandı. |
| Dinamik karekodla devam | Danışman birim yetkisi, 30 saniyelik kod, ilk 20 dakika, onaylı izin ve tekrar okutma kontrolleri başarılı. Tarayıcıda karekodun gösterilmesi ve yüklenen görselden okunması denendi. |
| Danışman görev ve değerlendirme modülü | Görev atama, değerlendirme geçmişi ve danışman raporu doğrulandı. |
| Aylık son döküm ve dışa aktarma | Çalışma ve izin toplamları, CSV içeriği, tarayıcıdan indirme ve role göre alan sınırları doğrulandı. |
| Denetim kayıtları, yedekleme ve güvenlik | Temel işlemler kullanıcı bilgisiyle kaydedildi. Yedek doğrulandı ve ayrı bir SQL Server veritabanına geri yüklenerek öğrenci ve plan kayıtları karşılaştırıldı. |

## Derleme ve arayüz

- .NET çözümü 0 hata ve 0 uyarıyla derlendi. Planlama kurallarının 13 testi geçti.
- React üretim derlemesi ile 12 takvim ve gezinme testi geçti.
- Chromium'da yönetim ve öğrenci ekranları, gri gün kutusunda sarı saat etiketi, geri ve ileri düğmeleri, önceki aya dönüş ve karekod görseli doğrulandı.
- SQL Server API kabul senaryoları hem sıfır kurulumda hem vizeden yükseltilmiş veritabanında geçti.
- Ay sınırı güncellemesi SQL Server üzerinde sınandı. 30 Kasım boş bırakılarak Kasım planı gönderildi ve 2 Aralık seçilerek ortak hafta tamamlandı. Önceki aydaki seçimin sayılması, eksik haftanın tarihli uyarısı ve iki ay toplamındaki üç gün sınırı doğrulandı.
- Karekod zamanı otomatik test saatiyle sınandı. Fiziksel kamera ve Windows çift tıklama başlatıcıları için [yerel gösterim adımları](Kabul_Testleri.md) bulunur.
