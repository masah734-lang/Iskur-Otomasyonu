using System.ComponentModel.DataAnnotations;
using static Iskur.Api.PlanningRules;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace Iskur.Api;
public record LoginDto([Required,EmailAddress]string Email,[Required]string Password);
public record ProfileDto([Required,StringLength(100)]string FullName,[Required,EmailAddress]string Email,[Required,StringLength(20)]string Phone,[Required,RegularExpression(@"\d{11}")]string NationalId,[Required,RegularExpression(@"TR\d{24}")]string Iban);
public record StudentDto([Required] ProfileDto Profile,[Required,StringLength(30)]string StudentNumber,int DepartmentId,int RegistrationYear,int ProgramPeriodId,[Required,MinLength(10)]string Password);
public record DayDto(DateOnly Date,string Kind,string? Note);
public record PlanDto(List<DayDto> Days,bool Submit,string? Version,List<DateOnly>? ExamDates);
public record CourseDto(int Id,[Required,StringLength(100)]string Name,int DayOfWeek,int StartMinute,int EndMinute);
public record AssignmentDto(int WorkUnitId,string? AdvisorId);
public record DecisionDto(string Decision,[StringLength(500)]string? Reason,string Version);
public record HolidayDto(DateOnly Date,[Required,StringLength(100)]string Name,bool AfternoonOnly);
[ApiController,Route("api/auth")]
public class AuthController(SignInManager<User> signIn):ControllerBase {
 [HttpPost("login")]
 public async Task<IResult> Login(LoginDto input){
  var user=await signIn.UserManager.FindByEmailAsync(input.Email);
  if(user==null)return Results.Unauthorized();
  var result=await signIn.CheckPasswordSignInAsync(user,input.Password,true);
  if(!result.Succeeded)return Results.Unauthorized();
  return Results.SignIn(await signIn.CreateUserPrincipalAsync(user),authenticationScheme:IdentityConstants.BearerScheme);
 }
}
[ApiController,Authorize,Route("api")]
public partial class ApiController(AppDb db,UserManager<User> users):ControllerBase {
 string Uid=>User.FindFirstValue(ClaimTypes.NameIdentifier)!;
 bool Management=>User.IsInRole(Roles.Management);
 IQueryable<Participation> Scope(){
  var q=db.Participations.AsQueryable();
  if(User.IsInRole(Roles.Student))return q.Where(x=>x.Student.UserId==Uid);
  if(Management)return q.Where(x=>db.Assignments.Any(a=>a.ParticipationId==x.Id&&a.End==null&&a.WorkUnit.FacultyId==db.Users.Where(u=>u.Id==Uid).Select(u=>u.FacultyId).First()));
  if(User.IsInRole(Roles.Head))return q.Where(x=>db.Assignments.Any(a=>a.ParticipationId==x.Id&&a.End==null&&a.WorkUnit.DepartmentId!=null&&a.WorkUnit.DepartmentId==db.Users.Where(u=>u.Id==Uid).Select(u=>u.DepartmentId).First()));
  if(AdvisorRole)return q.Where(x=>db.Assignments.Any(a=>a.ParticipationId==x.Id&&a.End==null&&a.AdvisorId==Uid));
  return q.Where(x=>false);
 }
 async Task<bool> Can(int id)=>await Scope().AnyAsync(x=>x.Id==id);
 void Log(string action,object id)=>db.Logs.Add(new(){ActorId=Uid,Action=action,Entity=id.ToString()!});
 [HttpGet("me")]public async Task<object> Me(){var u=(await users.FindByIdAsync(Uid))!;return new{u.Id,u.FullName,u.Email,roles=await users.GetRolesAsync(u)};}
 [HttpGet("lookups")]public async Task<object> Lookups(){
  var u=(await users.FindByIdAsync(Uid))!;var faculty=u.FacultyId;
  return new{departments=await db.Departments.Where(x=>x.FacultyId==faculty).Select(x=>new{x.Id,x.Name,x.FacultyId}).ToListAsync(),
   workUnits=await db.WorkUnits.Where(x=>x.FacultyId==faculty && (!HeadRole || x.DepartmentId==u.DepartmentId)).Select(x=>new{x.Id,x.Name,x.DepartmentId}).ToListAsync(),
   qrUnits=await QrUnits(u).OrderBy(x=>x.Name).Select(x=>new{x.Id,x.Name}).ToListAsync(),
   periods=await db.Periods.ToListAsync(),advisors=await db.Users.Where(x=>x.FacultyId==faculty&&db.UserRoles.Any(r=>r.UserId==x.Id&&db.Roles.Any(z=>z.Id==r.RoleId&&z.Name==Roles.Advisor))).Select(x=>new{x.Id,x.FullName}).ToListAsync()};
 }
 [HttpGet("students")]
 public async Task<object> Students(string? search,int? departmentId,string? advisorId,int? year,int? month,bool? active,int? workUnitId){
  var q=Scope();if(workUnitId!=null)q=q.Where(x=>db.Assignments.Any(a=>a.ParticipationId==x.Id&&a.End==null&&a.WorkUnitId==workUnitId));if(active!=null)q=q.Where(x=>x.Active==active);
  if(departmentId!=null)q=q.Where(x=>x.Student.DepartmentId==departmentId);
  if(!string.IsNullOrWhiteSpace(search))q=q.Where(x=>x.Student.User.FullName.Contains(search)||x.Student.StudentNumber.Contains(search));
  if(advisorId!=null)q=q.Where(x=>db.Assignments.Any(a=>a.ParticipationId==x.Id&&a.End==null&&a.AdvisorId==advisorId));
  if(year!=null)q=q.Where(x=>db.Plans.Any(p=>p.ParticipationId==x.Id&&p.Year==year&&(month==null||p.Month==month)));
  Log("StudentList", "filters");await db.SaveChangesAsync();
  return await q.OrderBy(x=>x.Student.User.FullName).Select(x=>new{x.Id,x.Active,x.Start,x.End,x.Student.StudentNumber,fullName=x.Student.User.FullName,department=x.Student.Department.Name,
   assignment=db.Assignments.Where(a=>a.ParticipationId==x.Id&&a.End==null).Select(a=>new{unit=a.WorkUnit.Name,advisor=a.Advisor==null?null:a.Advisor.FullName}).FirstOrDefault()}).ToListAsync();
 }
 [HttpGet("students/{id:int}")]public async Task<IActionResult> Student(int id){
  if(!await Can(id))return Forbid();
  var p=await db.Participations.Include(x=>x.Student).ThenInclude(x=>x.User).Include(x=>x.Student.Department).SingleAsync(x=>x.Id==id);
  var s=p.Student;Log("ProfileRead",id);await db.SaveChangesAsync();
  return Ok(new{p.Id,p.Active,p.Start,p.End,s.StudentNumber,s.DepartmentId,department=s.Department.Name,s.RegistrationYear,s.User.FullName,s.User.Email,phone=s.User.PhoneNumber,nationalId=Management||StudentRole?s.NationalId:null,iban=Management||StudentRole?s.Iban:null,
   assignment=await db.Assignments.Where(x=>x.ParticipationId==id&&x.End==null).Select(x=>new{x.WorkUnitId,x.AdvisorId,unit=x.WorkUnit.Name,advisor=x.Advisor==null?null:x.Advisor.FullName}).FirstOrDefaultAsync()});
 }
 [HttpPut("students/{id:int}")]public async Task<IActionResult> UpdateProfile(int id,ProfileDto input){
  if(!await Can(id)||(!Management&&!User.IsInRole(Roles.Student)))return Forbid();
  var p=await db.Participations.Include(x=>x.Student).ThenInclude(x=>x.User).SingleAsync(x=>x.Id==id);var s=p.Student;var u=s.User;
  if(await db.Students.AnyAsync(x=>x.Id!=s.Id&&x.NationalId==input.NationalId))return BadRequest(new{message="T.C. kimlik numarası zaten kayıtlı."});
  u.FullName=input.FullName.Trim();u.PhoneNumber=input.Phone;
  // Kimlik ve hesap e-postası öğrenci tarafından değiştirilemez.
  if(Management){var result=await users.SetEmailAsync(u,input.Email);if(!result.Succeeded)return BadRequest(new{message="E-posta güncellenemedi."});s.NationalId=input.NationalId;}
  s.Iban=input.Iban;Log("ProfileUpdate",id);await db.SaveChangesAsync();return Ok();
 }
 [HttpPost("students"),Authorize(Roles=Roles.Management)]public async Task<IActionResult> CreateStudent(StudentDto input){
  var manager=(await users.FindByIdAsync(Uid))!;
  var dept=await db.Departments.SingleOrDefaultAsync(x=>x.Id==input.DepartmentId&&x.FacultyId==manager.FacultyId);
  var period=await db.Periods.FindAsync(input.ProgramPeriodId);
  if(dept==null||period==null||input.RegistrationYear<2000||input.RegistrationYear>2100)return BadRequest(new{message="Bölüm, kayıt yılı veya dönem geçersiz."});
  if(await db.Students.AnyAsync(x=>x.StudentNumber==input.StudentNumber||x.NationalId==input.Profile.NationalId))return BadRequest(new{message="Öğrenci numarası veya T.C. kimlik numarası zaten kayıtlı."});
  await using var tx=await db.Database.BeginTransactionAsync();var v=input.Profile;
  var u=new User{UserName=v.Email,Email=v.Email,FullName=v.FullName,PhoneNumber=v.Phone,FacultyId=manager.FacultyId,DepartmentId=dept.Id};
  var result=await users.CreateAsync(u,input.Password);if(!result.Succeeded)return BadRequest(new{message=string.Join(" ",result.Errors.Select(e=>e.Description))});
  result=await users.AddToRoleAsync(u,Roles.Student);if(!result.Succeeded)throw new ArgumentException("Rol atanamadı.");
  var p=new Participation{Student=new(){UserId=u.Id,StudentNumber=input.StudentNumber,NationalId=v.NationalId,Iban=v.Iban,DepartmentId=dept.Id,RegistrationYear=input.RegistrationYear},ProgramPeriodId=period.Id,Start=period.Start,End=period.End};
  db.Participations.Add(p);await db.SaveChangesAsync();
  var unit=await db.WorkUnits.Where(x=>x.FacultyId==manager.FacultyId)
   .OrderByDescending(x=>x.Name=="Atama bekliyor").ThenBy(x=>x.Id).FirstOrDefaultAsync();
  if(unit is null)return Invalid("Öğrenci eklemeden önce bir çalışma birimi tanımlayın.");
  db.Assignments.Add(new(){ParticipationId=p.Id,WorkUnitId=unit.Id});Log("StudentCreate",p.Id);await db.SaveChangesAsync();await tx.CommitAsync();return Ok(new{p.Id});
 }
 [HttpPut("students/{id:int}/assignment"),Authorize(Roles=Roles.Management+","+Roles.Head)]
 public async Task<IActionResult> Assign(int id,AssignmentDto input){
  await using var tx=await db.Database.BeginTransactionAsync();await db.LockParticipation(id);
  if(!await Can(id))return Forbid();var me=(await users.FindByIdAsync(Uid))!;
  var old=await db.Assignments.SingleAsync(x=>x.ParticipationId==id&&x.End==null);
  var unit=await db.WorkUnits.SingleOrDefaultAsync(x=>x.Id==input.WorkUnitId&&x.FacultyId==me.FacultyId);
  if(unit==null||(!Management&&old.WorkUnitId!=unit.Id))return Forbid();
  if(input.AdvisorId!=null){var advisor=await users.FindByIdAsync(input.AdvisorId);if(advisor==null||advisor.FacultyId!=me.FacultyId||!await users.IsInRoleAsync(advisor,Roles.Advisor))return BadRequest(new{message="Geçersiz danışman."});}
  var assignmentTime=Now;old.End=assignmentTime;await db.SaveChangesAsync();
  db.Assignments.Add(new(){ParticipationId=id,WorkUnitId=unit.Id,AdvisorId=input.AdvisorId,Start=assignmentTime});Log("AssignmentUpdate",id);await db.SaveChangesAsync();await tx.CommitAsync();return Ok();
 }
 [HttpGet("students/{id:int}/courses")]public async Task<IActionResult> Courses(int id){if(!await Can(id))return Forbid();return Ok(await db.Courses.Where(x=>x.ParticipationId==id).Select(x=>new{x.Id,x.Name,x.DayOfWeek,x.StartMinute,x.EndMinute}).ToListAsync());}
 [HttpPut("students/{id:int}/courses")]
 public async Task<IActionResult> Courses(int id,List<CourseDto> input){
  if(!await Can(id)||!User.IsInRole(Roles.Student))return Forbid();
  if(input.Count>60||input.Any(x=>x.DayOfWeek<1||x.DayOfWeek>5||x.StartMinute<0||x.EndMinute>1440||x.EndMinute<=x.StartMinute||string.IsNullOrWhiteSpace(x.Name)))return BadRequest(new{message="Ders gün ve saatlerini kontrol edin."});
  await using var transaction=await db.Database.BeginTransactionAsync();
  await db.LockParticipation(id);
  var today=FinalRules.LocalDate(Now);
  if(await db.PlanDays.AnyAsync(x=>x.MonthlyPlan.ParticipationId==id&&x.MonthlyPlan.Status=="Submitted"&&x.Date>=today))
   return Invalid("İleri tarihli gönderilmiş planları ders programını değiştirmeden önce geri çekin.");
  var previous=await db.Courses.Where(x=>x.ParticipationId==id).ToListAsync();
  db.Add(new CourseRevision{ParticipationId=id,RecordedAt=Now,
   Snapshot=JsonSerializer.Serialize(previous.Select(x=>new{x.Name,x.DayOfWeek,x.StartMinute,x.EndMinute}))});
  db.Courses.RemoveRange(previous);
  db.Courses.AddRange(input.Select(x=>new Course{ParticipationId=id,Name=x.Name,DayOfWeek=x.DayOfWeek,StartMinute=x.StartMinute,EndMinute=x.EndMinute}));
  Log("CoursesReplace",id);await db.SaveChangesAsync();await transaction.CommitAsync();return Ok();
 }
 [HttpGet("holidays")]public async Task<object> Holidays(int year)=>await db.Holidays.Where(x=>x.Date.Year==year).ToListAsync();
 [HttpPut("holidays"),Authorize(Roles=Roles.Management)]public async Task<IActionResult> Holiday(HolidayDto input){var h=await db.Holidays.FindAsync(input.Date);if(h==null){h=new(){Date=input.Date};db.Holidays.Add(h);}h.Name=input.Name;h.AfternoonOnly=input.AfternoonOnly;Log("HolidayUpdate",input.Date);await db.SaveChangesAsync();return Ok();}
 [HttpGet("students/{id:int}/plans/{year:int}/{month:int}")]
 public async Task<IActionResult> Plan(int id,int year,int month)
 {
  if(!await Can(id))return Forbid();
  if(year is < 2020 or > 2100 || month is < 1 or > 12)return Invalid("Ay veya yıl geçersiz.");
  var plan=await db.Plans.Include(x=>x.Days).SingleOrDefaultAsync(x=>x.ParticipationId==id&&x.Year==year&&x.Month==month);
  return Ok(await PlanView(id,year,month,plan));
 }
}
