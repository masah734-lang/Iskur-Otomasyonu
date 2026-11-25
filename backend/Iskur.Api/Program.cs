using Iskur.Api;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics;
var builder=WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDb>(o=>o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddIdentityApiEndpoints<User>(o=>{o.Password.RequiredLength=10;o.Lockout.MaxFailedAccessAttempts=5;o.User.RequireUniqueEmail=true;}).AddRoles<IdentityRole>().AddEntityFrameworkStores<AppDb>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddAuthorization();builder.Services.AddControllers();
var app=builder.Build();
app.UseExceptionHandler(e=>e.Run(async c=>{
 var ex=c.Features.Get<IExceptionHandlerFeature>()?.Error;
 var status=ex is DbUpdateConcurrencyException or Microsoft.Data.SqlClient.SqlException { Number: 51000 or 1205 }?409:ex is ArgumentException?400:500;
 c.Response.StatusCode=status;
 await c.Response.WriteAsJsonAsync(new {message=status==409?"Kayıt değişti. Sayfayı yenileyip yeniden deneyin.":status==400?ex!.Message:"İşlem tamamlanamadı. Sunucu kayıtlarını kontrol edin."});
}));
if(args.Contains("--export-schema")){await File.WriteAllTextAsync("schema.sql",app.Services.CreateScope().ServiceProvider.GetRequiredService<AppDb>().Database.GenerateCreateScript());return;}
if(args.Contains("--reset-demo-password")){
 if(!app.Environment.IsDevelopment())throw new InvalidOperationException("Parola sıfırlama yalnız Development ortamında çalışır.");
 using var scope=app.Services.CreateScope();
 var users=scope.ServiceProvider.GetRequiredService<UserManager<User>>();
 var password=Environment.GetEnvironmentVariable("ISKUR_DEMO_PASSWORD")??throw new InvalidOperationException("Yeni parolayı belirtin.");
 var db=scope.ServiceProvider.GetRequiredService<AppDb>();
 await using var tx=await db.Database.BeginTransactionAsync();
 foreach(var email in new[]{"yonetim@demo.local","baskan@demo.local","danisman@demo.local","diger.danisman@demo.local","ogrenci1@demo.local","ogrenci2@demo.local","ogrenci3@demo.local"}){
  var user=await users.FindByEmailAsync(email);if(user==null)continue;
  var token=await users.GeneratePasswordResetTokenAsync(user);
  var result=await users.ResetPasswordAsync(user,token,password);
  if(!result.Succeeded)throw new InvalidOperationException(string.Join(" ",result.Errors.Select(e=>e.Description)));
  var unlocked=await users.SetLockoutEndDateAsync(user,null);
  if(!unlocked.Succeeded)throw new InvalidOperationException("Hesap kilidi kaldırılamadı.");
  await users.ResetAccessFailedCountAsync(user);
 }
 await tx.CommitAsync();Console.WriteLine("Demo hesap parolaları güncellendi. Öğrenci ve plan kayıtları korundu.");return;
}
if(args.Contains("--backup-db")){
 using var scope=app.Services.CreateScope();
 var directory=Environment.GetEnvironmentVariable("ISKUR_BACKUP_DIRECTORY")
  ?? throw new InvalidOperationException("ISKUR_BACKUP_DIRECTORY ortam değişkenini belirtin.");
 Console.WriteLine("Doğrulanan yedek: " + await Backup.Create(scope.ServiceProvider.GetRequiredService<AppDb>(),directory));
 return;
}
if(args.Contains("--upgrade-db")){
 using var scope=app.Services.CreateScope();
 await DatabaseUpgrade.Run(scope.ServiceProvider.GetRequiredService<AppDb>());
 Console.WriteLine("Veritabanı şeması güncel.");
 return;
}
if(args.Contains("--init-db")){
 if(!app.Environment.IsDevelopment()) throw new InvalidOperationException("Başlangıç verileri yalnız Development ortamında oluşturulur.");
 using var scope=app.Services.CreateScope();await Seed.Run(scope.ServiceProvider);return;
}
app.UseAuthentication();app.UseAuthorization();app.MapControllers();
app.MapGet("/api/health",()=>new {status="ok"});
app.Run();
