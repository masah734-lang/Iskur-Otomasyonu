using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace Iskur.Api;
public static class Roles {
 public const string Student="Student", Advisor="Advisor", Head="DepartmentHead", Management="FacultyManagement";
 public static readonly string[] All=[Student,Advisor,Head,Management];
}
public class User:IdentityUser {
 public string FullName {get;set;}="";
 public int? FacultyId {get;set;} public Faculty? Faculty {get;set;}
 public int? DepartmentId {get;set;} public Department? Department {get;set;}
}
public class Faculty { public int Id{get;set;} public string Name{get;set;}=""; }
public class Department {public int Id{get;set;} public string Name{get;set;}="";public int FacultyId{get;set;} public Faculty Faculty{get;set;}=null!;}
public class WorkUnit {public int Id{get;set;}public string Name{get;set;}="";public int FacultyId{get;set;} public Faculty Faculty{get;set;}=null!; public int? DepartmentId{get;set;}public Department? Department{get;set;} }
public class StudentProfile {
 public int Id{get;set;} public string UserId{get;set;}="";public User User{get;set;}=null!;
 public string StudentNumber{get;set;}=""; public string NationalId{get;set;}="";public string Iban{get;set;}="";
 public int DepartmentId{get;set;} public Department Department{get;set;}=null!;public int RegistrationYear{get;set;}
}
public class ProgramPeriod {public int Id{get;set;}public string Name{get;set;}=""; public DateOnly Start{get;set;} public DateOnly End{get;set;} }
public class Participation {
 public int Id{get;set;}public int StudentProfileId{get;set;}public StudentProfile Student{get;set;}=null!;
 public int ProgramPeriodId{get;set;}public ProgramPeriod ProgramPeriod{get;set;}=null!;
 public bool Active{get;set;}=true;public DateOnly Start{get;set;}public DateOnly End{get;set;}
}
public class Assignment {
 public int Id{get;set;}public int ParticipationId{get;set;}public Participation Participation{get;set;}=null!;
 public int WorkUnitId{get;set;}public WorkUnit WorkUnit{get;set;}=null!;
 public string? AdvisorId{get;set;}public User? Advisor{get;set;}
 public DateTimeOffset Start{get;set;}=DateTimeOffset.UtcNow; public DateTimeOffset? End{get;set;}
}
public class MonthlyPlan {
 public int Id{get;set;} public int ParticipationId{get;set;}public Participation Participation{get;set;}=null!;
 public int Year{get;set;} public int Month{get;set;}public string Status{get;set;}="Draft";
 public DateTimeOffset UpdatedAt{get;set;}=DateTimeOffset.UtcNow;
 public byte[] Version{get;set;}=[];
 public List<PlanDay> Days{get;set;}=[];
}
public class PlanDay {
 public int Id{get;set;}public int MonthlyPlanId{get;set;}public MonthlyPlan MonthlyPlan{get;set;}=null!;
 public DateOnly Date{get;set;}public string Kind{get;set;}="Full";public int Minutes{get;set;}public string Note{get;set;}="";
 public bool Conflict{get;set;} public string Decision{get;set;}="Pending";public string? Reason{get;set;}
 public string? DecidedById{get;set;} public User? DecidedBy{get;set;}public DateTimeOffset? DecidedAt{get;set;}
}
public class Course {
 public int Id{get;set;}public int ParticipationId{get;set;}public Participation Participation{get;set;}=null!;
 public string Name{get;set;}="";public int DayOfWeek{get;set;} public int StartMinute{get;set;}public int EndMinute{get;set;}
}
public class Holiday { public DateOnly Date{get;set;}public string Name{get;set;}=""; public bool AfternoonOnly{get;set;} }
public class AuditLog {public long Id{get;set;}public string ActorId{get;set;}=""; public string Action{get;set;}="";public string Entity{get;set;}="";public DateTimeOffset At{get;set;}=DateTimeOffset.UtcNow;}
// Final modüllerinin ilişkisel veri modeli; vizede işlem uçları bulunmaz.
public class LeaveRequest {public int Id{get;set;}public int ParticipationId{get;set;}public Participation Participation{get;set;}=null!;public DateOnly Start{get;set;}public DateOnly End{get;set;}public string Type{get;set;}="";public string Reason{get;set;}="";public string Status{get;set;}="Pending";public int Minutes{get;set;}public string? ReviewerId{get;set;} public User? Reviewer{get;set;}public DateTimeOffset? ReviewedAt{get;set;}}
public class AttendanceSession {public int Id{get;set;}public int WorkUnitId{get;set;}public WorkUnit WorkUnit{get;set;}=null!;public DateTimeOffset OpenAt{get;set;}public DateTimeOffset CloseAt{get;set;}public string NonceHash{get;set;}="";}
public class Attendance {public int Id{get;set;}public int AttendanceSessionId{get;set;}public AttendanceSession Session{get;set;}=null!;public int PlanDayId{get;set;}public PlanDay PlanDay{get;set;}=null!;public DateTimeOffset RecordedAt{get;set;}public string Status{get;set;}="Present";}
public class StudentTask {public int Id{get;set;}public int AssignmentId{get;set;}public Assignment Assignment{get;set;}=null!;public string Description{get;set;}="";public DateOnly DueDate{get;set;}public bool Completed{get;set;}}
public class Evaluation {public int Id{get;set;}public int StudentTaskId{get;set;}public StudentTask Task{get;set;}=null!;public string AuthorId{get;set;}="";public User Author{get;set;}=null!;public string Note{get;set;}="";public DateTimeOffset At{get;set;}=DateTimeOffset.UtcNow;}
public partial class AppDb(DbContextOptions<AppDb> options):IdentityDbContext<User>(options) {
 public DbSet<Faculty> Faculties=>Set<Faculty>();public DbSet<Department> Departments=>Set<Department>();public DbSet<WorkUnit> WorkUnits=>Set<WorkUnit>();
 public DbSet<StudentProfile> Students=>Set<StudentProfile>();public DbSet<ProgramPeriod> Periods=>Set<ProgramPeriod>();public DbSet<Participation> Participations=>Set<Participation>();
 public DbSet<Assignment> Assignments=>Set<Assignment>();public DbSet<MonthlyPlan> Plans=>Set<MonthlyPlan>();public DbSet<PlanDay> PlanDays=>Set<PlanDay>();
 public DbSet<Course> Courses=>Set<Course>();public DbSet<Holiday> Holidays=>Set<Holiday>();public DbSet<AuditLog> Logs=>Set<AuditLog>();
 protected override void OnModelCreating(ModelBuilder b) {
  base.OnModelCreating(b);
  b.Entity<StudentProfile>().HasIndex(x=>x.UserId).IsUnique();b.Entity<StudentProfile>().HasIndex(x=>x.StudentNumber).IsUnique();b.Entity<StudentProfile>().HasIndex(x=>x.NationalId).IsUnique();
  b.Entity<Participation>().HasIndex(x=>new{x.StudentProfileId,x.ProgramPeriodId}).IsUnique();
  b.Entity<Assignment>().HasIndex(x=>x.ParticipationId).HasFilter("[End] IS NULL").IsUnique();
  b.Entity<MonthlyPlan>().HasIndex(x=>new{x.ParticipationId,x.Year,x.Month}).IsUnique();b.Entity<MonthlyPlan>().Property(x=>x.Version).IsRowVersion();
  b.Entity<PlanDay>().HasIndex(x=>new{x.MonthlyPlanId,x.Date}).IsUnique();b.Entity<Holiday>().HasKey(x=>x.Date);
  b.Entity<LeaveRequest>();b.Entity<AttendanceSession>();b.Entity<Attendance>().HasIndex(x=>new{x.AttendanceSessionId,x.PlanDayId}).IsUnique();b.Entity<StudentTask>();b.Entity<Evaluation>();
  ConfigureFinal(b);
  foreach(var entity in b.Model.GetEntityTypes()) foreach(var fk in entity.GetForeignKeys()) fk.DeleteBehavior=DeleteBehavior.Restrict;
  b.Entity<PlanDay>().HasOne(x=>x.MonthlyPlan).WithMany(x=>x.Days).HasForeignKey(x=>x.MonthlyPlanId).OnDelete(DeleteBehavior.Cascade);
  b.Entity<StudentProfile>().Property(x=>x.NationalId).HasMaxLength(11);b.Entity<StudentProfile>().Property(x=>x.StudentNumber).HasMaxLength(30);b.Entity<StudentProfile>().Property(x=>x.Iban).HasMaxLength(26);
 }
}
