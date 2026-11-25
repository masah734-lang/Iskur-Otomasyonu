using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Iskur.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

var password = Environment.GetEnvironmentVariable("ISKUR_DEMO_PASSWORD")
    ?? throw new InvalidOperationException("Demo parolasını ortam değişkeninde belirtin.");
var time = new TestTime { Current = new DateTimeOffset(2026, 11, 1, 10, 0, 0, TimeSpan.FromHours(3)) };
using var app = new WebApplicationFactory<AppDb>().WithWebHostBuilder(builder =>
{
    builder.UseEnvironment("Development");
    builder.ConfigureServices(services =>
    {
        services.RemoveAll<TimeProvider>();
        services.AddSingleton<TimeProvider>(time);
        services.PostConfigure<BearerTokenOptions>(IdentityConstants.BearerScheme, o => o.TimeProvider = TimeProvider.System);
    });
});
using var anonymous = app.CreateClient();
await Status(anonymous.GetAsync("/api/students"), HttpStatusCode.Unauthorized);
using var management = await Login("yonetim");
using var advisor = await Login("danisman");
using var head = await Login("baskan");
using var student = await Login("ogrenci1");
var students = await Read(management, "/api/students");
var first = FindStudent("202600001");
var second = FindStudent("202600002");
var third = FindStudent("202600003");
var advisorLookups = await Read(advisor, "/api/lookups");
var unit = advisorLookups.GetProperty("workUnits").EnumerateArray()
    .Single(row => row.GetProperty("name").GetString() == "Bilgisayar Laboratuvarı").GetProperty("id").GetInt32();
var otherUnit = advisorLookups.GetProperty("workUnits").EnumerateArray()
    .Single(row => row.GetProperty("name").GetString() == "Fakülte Kütüphanesi").GetProperty("id").GetInt32();
Assert(advisorLookups.GetProperty("qrUnits").GetArrayLength() == 1 &&
    advisorLookups.GetProperty("qrUnits")[0].GetProperty("id").GetInt32() == unit, "Danışmanın QR birimleri");
Assert((await Read(student, "/api/lookups")).GetProperty("qrUnits").GetArrayLength() == 0, "Öğrenci QR dağıtamaz");
await Status(student.GetAsync($"/api/attendance/qr?unitId={unit}"), HttpStatusCode.Forbidden);
await Status(advisor.GetAsync($"/api/attendance/qr?unitId={otherUnit}"), HttpStatusCode.Forbidden);
await Status(student.GetAsync($"/api/students/{second}/leaves"), HttpStatusCode.Forbidden);
await Status(advisor.GetAsync($"/api/students/{third}/tasks"), HttpStatusCode.Forbidden);
Assert((await Read(advisor, $"/api/students/{first}")).GetProperty("iban").ValueKind == JsonValueKind.Null, "IBAN erişimi");
await Status(student.GetAsync("/api/reports/work?year=2026&csv=true"), HttpStatusCode.Forbidden);
Console.WriteLine("PASS: Roller, kayıt kapsamı ve hassas alanlar");

var profilePath = $"/api/students/{first}";
var profile = await Read(student, profilePath);
var updated = new ProfileDto(profile.GetProperty("fullName").GetString()!, profile.GetProperty("email").GetString()!,
    "05000000001", profile.GetProperty("nationalId").GetString()!, profile.GetProperty("iban").GetString()!);
await Status(student.PutAsJsonAsync(profilePath, updated), HttpStatusCode.OK);
Assert((await Read(student, profilePath)).GetProperty("phone").GetString() == updated.Phone, "Profil değişikliği kaydedildi");
await Status(student.PutAsJsonAsync(profilePath, updated with { Phone = profile.GetProperty("phone").GetString()! }), HttpStatusCode.OK);

var departmentId = profile.GetProperty("departmentId").GetInt32();
var filtered = await Read(management, $"/api/students?year=2026&month=11&departmentId={departmentId}");
Assert(filtered.GetArrayLength() == 2 && filtered.EnumerateArray().All(x => x.GetProperty("id").GetInt32() != third),
    "Ay/yıl ve bölüm filtresi doğru kayıtları döndürür");
Assert((await Read(management, "/api/students?year=2025&month=11")).GetArrayLength() == 0, "Başka yıl verisi görünmez");
Console.WriteLine("PASS: Öğrenci profili kaydı ve ay/yıl ile bölüm filtreleri");


await Status(management.PutAsJsonAsync("/api/settings/policy", new
    { leaveDays = 10, leaveWarningDays = 2, absenceWarningDays = 1, medicalLeaveExempt = false }), HttpStatusCode.OK);
foreach (var id in new[] { first, second })
    foreach (var date in new[] { "2026-11-02", "2026-11-04" })
        await ApproveDay(id, date);

var leavePath = $"/api/students/{first}/leaves";
using var form = new MultipartFormDataContent();
form.Add(new StringContent("2026-11-02"), "start");
form.Add(new StringContent("2026-11-02"), "end");
form.Add(new StringContent("Personal"), "type");
form.Add(new StringContent("Örnek izin testi"), "reason");
form.Add(new ByteArrayContent("%PDF-1.7\n%%EOF\n"u8.ToArray()), "file", "ornek.pdf");
var leave = await Json(await Status(student.PostAsync(leavePath, form), HttpStatusCode.OK));
var leaveId = leave.GetProperty("id").GetInt32();
var balance = await Read(student, leavePath);
Assert(balance.GetProperty("pending").GetDecimal() == 0.5m, "Bekleyen yarım gün");
await Status(advisor.GetAsync($"{leavePath}/{leaveId}/attachment"), HttpStatusCode.Forbidden);
var attachment = await Status(student.GetAsync($"{leavePath}/{leaveId}/attachment"), HttpStatusCode.OK);
Assert(attachment.Content.Headers.ContentType?.MediaType == "application/pdf", "İzin belgesi");
await Status(management.PostAsJsonAsync($"{leavePath}/{leaveId}/decision", new { status = "Approved" }), HttpStatusCode.OK);
balance = await Read(student, leavePath);
Assert(balance.GetProperty("remaining").GetDecimal() == 9.5m &&
    balance.GetProperty("pending").GetDecimal() == 0m, "İzin onayı ve bakiye");
await Status(management.PostAsJsonAsync($"{leavePath}/{leaveId}/decision", new { status = "Approved" }), HttpStatusCode.Conflict);
Console.WriteLine("PASS: İzin belgesi, onay, bakiye ve tekrar karar kontrolü");

var taskPath = $"/api/students/{first}/tasks";
var task = await Json(await Status(advisor.PostAsJsonAsync(taskPath,
    new { description = "Test raporunu hazırla", dueDate = "2026-11-17" }), HttpStatusCode.OK));
var taskId = task.GetProperty("id").GetInt32();
await Status(advisor.PostAsJsonAsync($"{taskPath}/{taskId}/evaluation",
    new { completed = true, note = "Teslim alındı." }), HttpStatusCode.OK);
var tasks = await Read(student, taskPath + "?completed=true");
Assert(tasks.GetArrayLength() == 1 && tasks[0].GetProperty("notes").GetArrayLength() == 1 &&
    tasks[0].GetProperty("history").GetArrayLength() == 2, "Görev ve değerlendirme geçmişi");
var taskReport = await Read(advisor, "/api/reports/tasks?year=2026&month=11&completed=true");
Assert(taskReport.GetArrayLength() == 1 && taskReport[0].GetProperty("assessmentCount").GetInt32() == 1,
    "Danışman görev raporu");
Console.WriteLine("PASS: Görev ataması, değerlendirme, öğrenci görünümü ve danışman raporu");

var qrPath = $"/api/attendance/qr?unitId={unit}";
var punchPath = $"/api/students/{first}/attendance/check-in";
time.Current = Istanbul(2026, 11, 2, 13, 30, 5);
var leaveToken = (await Read(advisor, qrPath)).GetProperty("token").GetString();
await Status(student.PostAsJsonAsync(punchPath, new { token = leaveToken }), HttpStatusCode.Conflict);
time.Current = Istanbul(2026, 11, 4, 8, 0, 5);
var firstToken = (await Read(advisor, qrPath)).GetProperty("token").GetString();
time.Current = Istanbul(2026, 11, 4, 8, 0, 30);
await Status(student.PostAsJsonAsync(punchPath, new { token = firstToken }), HttpStatusCode.BadRequest);
await Status(student.PostAsJsonAsync(punchPath, new { token = "bozuk-token" }), HttpStatusCode.BadRequest);
var newToken = (await Read(advisor, qrPath)).GetProperty("token").GetString();
await Status(student.PostAsJsonAsync(punchPath, new { token = newToken }), HttpStatusCode.OK);
await Status(student.PostAsJsonAsync(punchPath, new { token = newToken }), HttpStatusCode.Conflict);
var attendance = await Read(student, $"/api/students/{first}/attendance?year=2026&month=11");
Assert(attendance.GetProperty("actualMinutes").GetInt32() == 450, "Tam gün QR");
Assert(attendance.GetProperty("rows").EnumerateArray().Any(row => row.GetProperty("source").GetString() == "DailyQr"), "Öğleden sonra aktarımı");
time.Current = Istanbul(2026, 11, 4, 8, 20, 0);
await Status(advisor.GetAsync(qrPath), HttpStatusCode.Conflict);
Console.WriteLine("PASS: 30 saniye, ilk 20 dakika, izin, mükerrer ve tam gün QR");

var secondAttendance = await Read(advisor, $"/api/students/{second}/attendance?year=2026&month=11");
var absent = secondAttendance.GetProperty("rows").EnumerateArray().First(row => row.GetProperty("status").GetString() == "Absent");
var absencesPath = $"/api/students/{second}/absence-reports";
var absence = await Json(await Status(advisor.PostAsJsonAsync(absencesPath, new
    { planDayId = absent.GetProperty("planDayId").GetInt32(), reason = "Test devamsızlığı" }), HttpStatusCode.OK));
var absenceId = absence.GetProperty("id").GetInt32();
Assert((await Read(management, absencesPath)).GetArrayLength() == 0, "Bildirim bölüm incelemesinde");
await Status(head.PostAsJsonAsync($"{absencesPath}/{absenceId}/decision", new { status = "Forwarded", reason = "İletildi" }), HttpStatusCode.OK);
await Status(management.PostAsJsonAsync($"{absencesPath}/{absenceId}/decision", new { status = "Closed", reason = "İncelendi" }), HttpStatusCode.OK);
Assert((await Read(management, absencesPath))[0].GetProperty("history").GetArrayLength() == 3, "Devamsızlık geçmişi");
Console.WriteLine("PASS: Danışman → bölüm başkanı → fakülte yönetimi devamsızlık akışı");

var reports = $"/api/reports/work?year=2026&month=11&participationId={first}";
var monthly = (await Read(management, reports))[0];
Assert(monthly.GetProperty("actualMinutes").GetInt32() == 450 && monthly.GetProperty("leaveDays").GetInt32() == 1,
    "Yoklama ve izin rapor toplamı");
Assert(monthly.GetProperty("nationalId").GetString() == "00000000001", "Yetkili rapor kimlik alanı");
var csv = await Status(management.GetAsync(reports + "&csv=true"), HttpStatusCode.OK);
Assert((await csv.Content.ReadAsStringAsync()).Contains("Gerçekleşen dakika"), "CSV");
var headCsv = await Status(head.GetAsync(reports + "&csv=true"), HttpStatusCode.OK);
Assert(!(await headCsv.Content.ReadAsStringAsync()).Contains("IBAN"), "Bölüm başkanı CSV alan sınırı");
Console.WriteLine("PASS: Aylık rapor toplamları ve role göre CSV");

using var thirdStudent = await Login("ogrenci3");
var novemberPath = $"/api/students/{third}/plans/2026/11";
var november = await Read(thirdStudent, novemberPath);
var draftNovember = await Json(await Status(
    thirdStudent.PostAsJsonAsync(novemberPath + "/withdraw", november.GetProperty("version").GetString()), HttpStatusCode.OK));
var novemberDays = new[] { "2026-11-03", "2026-11-10", "2026-11-17", "2026-11-24" }
    .Select(date => new { date, kind = "Morning", note = (string?)null }).ToArray();
var submittedNovember = await Json(await Status(thirdStudent.PutAsJsonAsync(novemberPath, new
{
    submit = true,
    version = draftNovember.GetProperty("version").GetString(),
    days = novemberDays
}), HttpStatusCode.OK));

var decemberPath = $"/api/students/{third}/plans/2026/12";
var remainingWeeks = new[] { "2026-12-08", "2026-12-15", "2026-12-22", "2026-12-29" };
var missingWeek = await Json(await Status(thirdStudent.PutAsJsonAsync(decemberPath, new
{
    submit = true,
    days = remainingWeeks.Select(date => new { date, kind = "Morning" }).ToArray()
}), HttpStatusCode.BadRequest));
Assert(missingWeek.GetProperty("message").GetString()!.Contains("30.11.2026–06.12.2026"), "Eksik haftanın tarih aralığı");
var submittedDecember = await Json(await Status(thirdStudent.PutAsJsonAsync(decemberPath, new
{
    submit = true,
    days = remainingWeeks.Prepend("2026-12-02").Select(date => new { date, kind = "Morning" }).ToArray()
}), HttpStatusCode.OK));
var reopenedDecember = await Json(await Status(thirdStudent.PostAsJsonAsync(decemberPath + "/withdraw",
    submittedDecember.GetProperty("version").GetString()), HttpStatusCode.OK));
var reopenedNovember = await Json(await Status(thirdStudent.PostAsJsonAsync(novemberPath + "/withdraw",
    submittedNovember.GetProperty("version").GetString()), HttpStatusCode.OK));
await Status(thirdStudent.PutAsJsonAsync(novemberPath, new
{
    submit = true,
    version = reopenedNovember.GetProperty("version").GetString(),
    days = novemberDays.Append(new { date = "2026-11-30", kind = "Morning", note = (string?)"Öğrencinin ders çakışması açıklaması." }).ToArray()
}), HttpStatusCode.OK);
var decemberVersion = reopenedDecember.GetProperty("version").GetString();
var overLimit = await Json(await Status(thirdStudent.PutAsJsonAsync(decemberPath, new
{
    submit = true,
    version = decemberVersion,
    days = remainingWeeks.Concat(new[] { "2026-12-01", "2026-12-02", "2026-12-03" })
        .Select(date => new { date, kind = "Morning" }).ToArray()
}), HttpStatusCode.BadRequest));
Assert(overLimit.GetProperty("message").GetString()!.Contains("en fazla 3"), "Aya yayılan haftanın üst sınırı");
await Status(thirdStudent.PutAsJsonAsync(decemberPath, new
{
    submit = true,
    version = decemberVersion,
    days = remainingWeeks.Select(date => new { date, kind = "Morning" }).ToArray()
}), HttpStatusCode.OK);
Console.WriteLine("PASS: Ortak haftada Kasım veya Aralık seçilebiliyor, eksik hafta ve üç gün sınırı doğrulandı");

using var thirdAdvisor = await Login("diger.danisman");
var reviewPlan = await Read(thirdAdvisor, novemberPath);
var overlapping = reviewPlan.GetProperty("days").EnumerateArray()
    .Single(row => row.GetProperty("date").GetString() == "2026-11-30");
Assert(overlapping.GetProperty("conflict").GetBoolean(), "Danışman ders çakışmasını görüyor");
var decisionPath = $"/api/plan-days/{overlapping.GetProperty("id").GetInt32()}/decision";
await Status(thirdAdvisor.PutAsJsonAsync(decisionPath,
    new { decision = "Rejected", reason = " ", version = reviewPlan.GetProperty("version").GetString() }),
    HttpStatusCode.BadRequest);
await Status(thirdAdvisor.PutAsJsonAsync(decisionPath,
    new { decision = "Rejected", reason = "Ders saatinde görev planlanmış.", version = reviewPlan.GetProperty("version").GetString() }),
    HttpStatusCode.OK);
var rejected = await Read(thirdAdvisor, novemberPath);
var rejectedDay = rejected.GetProperty("days").EnumerateArray()
    .Single(row => row.GetProperty("date").GetString() == "2026-11-30");
Assert(rejectedDay.GetProperty("reason").GetString() == "Ders saatinde görev planlanmış.", "Ret gerekçesi saklanıyor");
await Status(thirdAdvisor.PutAsJsonAsync(decisionPath,
    new { decision = "Accepted", version = rejected.GetProperty("version").GetString() }), HttpStatusCode.OK);
var accepted = await Read(thirdAdvisor, novemberPath);
Assert(accepted.GetProperty("days").EnumerateArray()
    .Single(row => row.GetProperty("date").GetString() == "2026-11-30").GetProperty("decision").GetString() == "Accepted",
    "Ders çakışması görünen gün gerekçesiz kabul edilebiliyor");
Console.WriteLine("PASS: Gerekçe yalnız ret için gerekiyor ve kabul tek istekle kaydediliyor");


var december = $"/api/students/{first}/plans/2026/12";
await Status(student.PutAsJsonAsync(december, new { submit = true,
    days = new[] { new { date = "2026-12-01", kind = "Morning" } } }), HttpStatusCode.BadRequest);
var plan = await Json(await Status(student.PutAsJsonAsync(december, new { submit = true,
    days = new[] { "2026-12-01", "2026-12-08", "2026-12-15", "2026-12-22", "2026-12-29" }
        .Select(date => new { date, kind = "Morning" }).ToArray(), examDates = new[] { "2026-12-02" } }), HttpStatusCode.OK));
Assert(plan.GetProperty("examDates").GetArrayLength() == 1, "Sınav istisnasının kayıt edilmesi");
await Status(student.PostAsJsonAsync(december + "/withdraw", plan.GetProperty("version").GetString()), HttpStatusCode.OK);
Assert((await Read(student, december + "/history")).GetArrayLength() == 1, "Plan geçmişinin korunması");
await Status(management.PutAsJsonAsync("/api/settings/plan-windows", new
    { year = 2026, month = 12, opens = "2026-12-01", closes = "2026-12-20" }), HttpStatusCode.OK);
await Status(student.PutAsJsonAsync(december, new { submit = false,
    days = new[] { new { date = "2026-12-01", kind = "Morning" } } }), HttpStatusCode.BadRequest);
Console.WriteLine("PASS: Haftalık alt sınır, sınav günü, plan geçmişi ve düzenleme aralığı");

await Status(management.PostAsJsonAsync("/api/administration/departments",
    new { name = "Test Bölümü" }), HttpStatusCode.OK);
var organization = await Read(management, "/api/administration");
var extraDepartment = organization.GetProperty("departments").EnumerateArray()
    .Single(x => x.GetProperty("name").GetString() == "Test Bölümü").GetProperty("id").GetInt32();
await Status(management.PostAsJsonAsync("/api/administration/units",
    new { name = "Test Birimi", departmentId = extraDepartment }), HttpStatusCode.OK);
await Status(management.PostAsJsonAsync("/api/administration/periods",
    new { name = "Yeni test dönemi", start = "2027-10-01", end = "2028-06-30" }), HttpStatusCode.OK);
organization = await Read(management, "/api/administration");
var period = organization.GetProperty("periods").EnumerateArray()
    .Single(x => x.GetProperty("name").GetString() == "Yeni test dönemi").GetProperty("id").GetInt32();
var participation = await Json(await Status(management.PostAsJsonAsync($"/api/students/{first}/participations",
    new { programPeriodId = period, workUnitId = unit }), HttpStatusCode.OK));
Assert((await Read(student, "/api/students")).GetArrayLength() == 2 &&
    participation.GetProperty("id").GetInt32() != first, "Yeni katılım geçmişi korur");
Console.WriteLine("PASS: Kurum kayıtları ve geçmişi koruyan dönem katılımı");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDb>();
    var actions = new[] { "ProfileUpdate", "LeaveApproved", "AttendanceCheckIn", "TaskEvaluated", "WorkReportExport" };
    var logged = await db.Logs.Where(x => actions.Contains(x.Action) && x.ActorId != "")
        .Select(x => x.Action).Distinct().ToListAsync();
    Assert(actions.All(logged.Contains), "Temel işlemlerin denetim kayıtları");
}
Console.WriteLine("PASS: Denetim kayıtları işlem yapan kullanıcıyla SQL Server'da saklanıyor");


int FindStudent(string number) => students.EnumerateArray()
    .Single(row => row.GetProperty("studentNumber").GetString() == number).GetProperty("id").GetInt32();

async Task<HttpClient> Login(string name)
{
    var client = app.CreateClient();
    var response = await Status(client.PostAsJsonAsync("/api/auth/login", new { email = name + "@demo.local", password }), HttpStatusCode.OK);
    var auth = await Json(response);
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.GetProperty("accessToken").GetString());
    return client;
}

async Task ApproveDay(int id, string date)
{
    var plan = await Read(advisor, $"/api/students/{id}/plans/2026/11");
    var day = plan.GetProperty("days").EnumerateArray().Single(row => row.GetProperty("date").GetString() == date);
    await Status(advisor.PutAsJsonAsync($"/api/plan-days/{day.GetProperty("id").GetInt32()}/decision",
        new { decision = "Accepted", version = plan.GetProperty("version").GetString() }), HttpStatusCode.OK);
}

static async Task<JsonElement> Read(HttpClient client, string path) =>
    await Json(await Status(client.GetAsync(path), HttpStatusCode.OK));

static async Task<JsonElement> Json(HttpResponseMessage response) =>
    await response.Content.ReadFromJsonAsync<JsonElement>();

static async Task<HttpResponseMessage> Status(Task<HttpResponseMessage> request, HttpStatusCode expected)
{
    var response = await request;
    if (response.StatusCode != expected)
        throw new InvalidOperationException($"{response.RequestMessage?.Method} {response.RequestMessage?.RequestUri}: " +
            $"beklenen {(int)expected}, alınan {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
    return response;
}

static void Assert(bool ok, string step)
{
    if (!ok) throw new InvalidOperationException(step);
}

static DateTimeOffset Istanbul(int year, int month, int day, int hour, int minute, int second) =>
    new(year, month, day, hour, minute, second, TimeSpan.FromHours(3));

sealed class TestTime : TimeProvider
{
    public DateTimeOffset Current { get; set; }
    public override DateTimeOffset GetUtcNow() => Current.ToUniversalTime();
}
