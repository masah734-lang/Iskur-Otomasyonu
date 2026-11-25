# API

Temel URL `http://127.0.0.1:5225/api`. `POST /auth/login` isteği e-posta ve parola alır. Dönen `accessToken` değeri `Authorization: Bearer <token>` üstbilgisi ile gönderilir.

| İşlem | Yöntem ve yol |
|---|---|
| Oturum ve seçim verileri | `GET /me`, `GET /lookups` |
| Öğrenci listesi ve kaydı | `GET /students`, `POST /students` |
| Profil ve birim ataması | `GET/PUT /students/{id}`, `PUT /students/{id}/assignment` |
| Yeni dönem katılımı | `POST /students/{id}/participations` |
| Ders programı ve tatil | `GET/PUT /students/{id}/courses`, `GET/PUT /holidays` |
| Aylık plan | `GET/PUT /students/{id}/plans/{year}/{month}` |
| Plan geri çekme ve geçmiş | `POST .../withdraw`, `GET .../history` |
| Gün kararı | `PUT /plan-days/{dayId}/decision`. Ret kararında gerekçe zorunludur. |
| İzin talebi ve bakiye | `GET/POST /students/{id}/leaves` |
| İzin kararı ve belge | `POST /students/{id}/leaves/{requestId}/decision`, `GET .../attachment` |
| QR üretimi | `GET /attendance/qr?unitId={id}` |
| Yoklama | `GET /students/{id}/attendance?year={y}&month={m}`, `POST .../check-in`, `PUT .../{dayId}` |
| Görev ve not | `GET/POST /students/{id}/tasks`, `POST .../{taskId}/evaluation` |
| Devamsızlık bildirimi | `GET/POST /students/{id}/absence-reports`, `POST .../{reportId}/decision` |
| Döküm ve CSV | `GET /reports/work?year={y}&month={m}&csv={bool}` |
| Görev dökümü | `GET /reports/tasks?year={y}&month={m}&completed={bool}` |
| Eşik uyarısı | `GET /reports/warnings` |
| Fakülte ayarları | `GET/PUT /settings/policy`, `GET/PUT /settings/plan-windows` |
| Kurum kayıtları | `GET /administration`, `POST /administration/{staff/departments/units/periods}` |

Plan günleri `Full`, `Morning`, `Afternoon` değerlerini kullanır. Plan güncellemesinde son okunan `version` gönderilir. `examDates` isteğe bağlı tarih dizisidir. İzin başvurusu alanları `start`, `end`, `type`, `reason` ve isteğe bağlı `file` olan multipart formu kullanır. Tarihler ISO `yyyy-MM-dd` biçimindedir.
