using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace Iskur.Api;
public static class Seed {
 public static async Task Run(IServiceProvider services){
  var db=services.GetRequiredService<AppDb>();var users=services.GetRequiredService<UserManager<User>>();var roles=services.GetRequiredService<RoleManager<IdentityRole>>();
  await DatabaseUpgrade.Run(db);
  await using var tx=await db.Database.BeginTransactionAsync();
  if(await db.Faculties.AnyAsync()){Console.WriteLine("Mevcut fakülte ve öğrenci kayıtları korundu. Demo verisi yeniden eklenmedi.");return;}
  foreach(var role in Roles.All){var r=await roles.CreateAsync(new(role));if(!r.Succeeded)throw new Exception(string.Join(",",r.Errors.Select(x=>x.Description)));}
  var faculty=new Faculty{Name="Teknoloji Fakültesi"};db.Faculties.Add(faculty);await db.SaveChangesAsync();
  var comp=new Department{Name="Bilgisayar Mühendisliği",FacultyId=faculty.Id};var elec=new Department{Name="Elektrik Elektronik Mühendisliği",FacultyId=faculty.Id};db.Departments.AddRange(comp,elec);await db.SaveChangesAsync();
  var pending=new WorkUnit{Name="Atama bekliyor",FacultyId=faculty.Id};var lab=new WorkUnit{Name="Bilgisayar Laboratuvarı",FacultyId=faculty.Id,DepartmentId=comp.Id};var library=new WorkUnit{Name="Fakülte Kütüphanesi",FacultyId=faculty.Id};db.WorkUnits.AddRange(pending,lab,library);
  var period=new ProgramPeriod{Name="2026–2027 İŞKUR",Start=new(2026,10,1),End=new(2027,6,30)};db.Periods.Add(period);await db.SaveChangesAsync();
  var password=Environment.GetEnvironmentVariable("ISKUR_DEMO_PASSWORD")??throw new InvalidOperationException("ISKUR_DEMO_PASSWORD ortam değişkenini belirleyin.");
  async Task<User> Add(string email,string name,string role,int? dept){
   var u=new User{UserName=email,Email=email,FullName=name,FacultyId=faculty.Id,DepartmentId=dept,PhoneNumber="05000000000"};
   var r=await users.CreateAsync(u,password);if(!r.Succeeded)throw new Exception(string.Join(",",r.Errors.Select(x=>x.Description)));r=await users.AddToRoleAsync(u,role);if(!r.Succeeded)throw new Exception("Rol atanamadı.");return u;
  }
  await Add("yonetim@demo.local","Ayşe Yönetici",Roles.Management,null);
  await Add("baskan@demo.local","Bölüm Başkanı",Roles.Head,comp.Id);
  var advisor=await Add("danisman@demo.local","Deniz Danışman",Roles.Advisor,comp.Id);
  var otherAdvisor=await Add("diger.danisman@demo.local","Diğer Danışman",Roles.Advisor,elec.Id);
  for(var i=1;i<=3;i++){
   var dept=i==3?elec:comp;var u=await Add($"ogrenci{i}@demo.local",new[]{"Ece Kaya","Mert Aydın","Ali Çelik"}[i-1],Roles.Student,dept.Id);
   var p=new Participation{Student=new(){UserId=u.Id,StudentNumber=$"20260000{i}",NationalId=$"0000000000{i}",Iban="TR000000000000000000000000",RegistrationYear=2026,DepartmentId=dept.Id},ProgramPeriodId=period.Id,Start=period.Start,End=period.End};db.Participations.Add(p);await db.SaveChangesAsync();
   db.Assignments.Add(new(){ParticipationId=p.Id,WorkUnitId=i==3?library.Id:lab.Id,AdvisorId=i==3?otherAdvisor.Id:advisor.Id,Start=FinalRules.StartOf(p.Start,WorkSlot.Morning)});
   db.Courses.Add(new(){ParticipationId=p.Id,Name="Veri Tabanı Yönetimi",DayOfWeek=1,StartMinute=540,EndMinute=660});
   db.Plans.Add(new(){ParticipationId=p.Id,Year=2026,Month=11,Status="Submitted",Days=[new(){Date=new(2026,11,2),Kind="Afternoon",Minutes=210},new(){Date=new(2026,11,4),Kind="Full",Minutes=450}]});
  }
  // Sabit ulusal tarihler. Dinî ve idari tatiller yönetim ekranından ayrıca girilir.
  foreach(var year in new[]{2026,2027})foreach(var item in new[]{(1,1,"Yılbaşı",false),(4,23,"Ulusal Egemenlik ve Çocuk Bayramı",false),(5,1,"Emek ve Dayanışma Günü",false),(5,19,"Atatürk'ü Anma Gençlik ve Spor Bayramı",false),(7,15,"Demokrasi ve Millî Birlik Günü",false),(8,30,"Zafer Bayramı",false),(10,28,"Cumhuriyet Bayramı arifesi",true),(10,29,"Cumhuriyet Bayramı",false)})db.Holidays.Add(new(){Date=new(year,item.Item1,item.Item2),Name=item.Item3,AfternoonOnly=item.Item4});
  await db.SaveChangesAsync();await tx.CommitAsync();Console.WriteLine("Örnek hesaplar oluşturuldu. Parola ISKUR_DEMO_PASSWORD değeridir. Veriler gerçek kişilere ait değildir.");
 }
}
