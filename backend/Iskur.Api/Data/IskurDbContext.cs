using Iskur.Api.Configurations;
using Iskur.Api.Entities;
using Iskur.Api.Entities.Common;
using Iskur.Api.Entities.Documents;
using Iskur.Api.Entities.Identity;
using Iskur.Api.Entities.Leaves;
using Iskur.Api.Entities.Organization;
using Iskur.Api.Entities.Scheduling;
using Iskur.Api.Entities.Students;
using Iskur.Api.Entities.WorkPlans;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Iskur.Api.Data;

public class IskurDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public IskurDbContext(DbContextOptions<IskurDbContext> options)
        : base(options)
    {
    }

    public DbSet<Faculty> Faculties => Set<Faculty>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<WorkUnit> WorkUnits => Set<WorkUnit>();

    public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();

    public DbSet<ProgramPeriod> ProgramPeriods => Set<ProgramPeriod>();

    public DbSet<ProgramParticipation> ProgramParticipations
        => Set<ProgramParticipation>();

    public DbSet<UnitAssignment> UnitAssignments
        => Set<UnitAssignment>();

    public DbSet<AdvisorAssignment> AdvisorAssignments
        => Set<AdvisorAssignment>();

    public DbSet<AuthorityAssignment> AuthorityAssignments
        => Set<AuthorityAssignment>();
    public DbSet<AcademicTerm> AcademicTerms => Set<AcademicTerm>();

    public DbSet<StoredFile> StoredFiles => Set<StoredFile>();

    public DbSet<CourseSchedule> CourseSchedules => Set<CourseSchedule>();

    public DbSet<CourseMeeting> CourseMeetings => Set<CourseMeeting>();

    public DbSet<CalendarHoliday> CalendarHolidays => Set<CalendarHoliday>();

    public DbSet<WorkRule> WorkRules => Set<WorkRule>();

    public DbSet<PlanApplicationWindow> PlanApplicationWindows
        => Set<PlanApplicationWindow>();
    public DbSet<MonthlyWorkPlan> MonthlyWorkPlans
    => Set<MonthlyWorkPlan>();

    public DbSet<WorkPlanDay> WorkPlanDays
        => Set<WorkPlanDay>();

    public DbSet<WorkSession> WorkSessions
        => Set<WorkSession>();

    public DbSet<PlanDayAction> PlanDayActions
        => Set<PlanDayAction>();
    public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();

    public DbSet<LeavePolicy> LeavePolicies => Set<LeavePolicy>();

    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();

    public DbSet<LeaveRequestSession> LeaveRequestSessions
        => Set<LeaveRequestSession>();

    public DbSet<LeaveMovement> LeaveMovements => Set<LeaveMovement>();

    public DbSet<LeaveRequestAction> LeaveRequestActions
        => Set<LeaveRequestAction>();
    // QR ve devam
    public DbSet<QrSession> QrSessions => Set<QrSession>();

    public DbSet<QrScan> QrScans => Set<QrScan>();

    public DbSet<SessionAttendance> SessionAttendances
        => Set<SessionAttendance>();

    public DbSet<AttendanceCorrection> AttendanceCorrections
        => Set<AttendanceCorrection>();

    // Görev ve idari süreçler
    public DbSet<StudentTask> StudentTasks => Set<StudentTask>();

    public DbSet<TaskAction> TaskActions => Set<TaskAction>();

    public DbSet<EvaluationNote> EvaluationNotes => Set<EvaluationNote>();

    public DbSet<AbsenceReport> AbsenceReports => Set<AbsenceReport>();

    public DbSet<AbsenceReportSession> AbsenceReportSessions
        => Set<AbsenceReportSession>();

    public DbSet<ReportAction> ReportActions => Set<ReportAction>();

    public DbSet<AdministrativeDecision> AdministrativeDecisions
        => Set<AdministrativeDecision>();

    // Belgeler ve denetim
    public DbSet<LeaveDocument> LeaveDocuments => Set<LeaveDocument>();

    public DbSet<DecisionDocument> DecisionDocuments
        => Set<DecisionDocument>();

    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ConfigureCoreModel();
        modelBuilder.ConfigureScheduleModel();
        modelBuilder.ConfigureWorkPlanModel();
        modelBuilder.ConfigureLeaveModel();
        modelBuilder.ConfigureRemainingModel(); 

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entity.ClrType))
            {
                modelBuilder.Entity(entity.ClrType)
                    .Property(nameof(BaseEntity.Id))
                    .ValueGeneratedNever();

                modelBuilder.Entity(entity.ClrType)
                    .Property(nameof(BaseEntity.RowVersion))
                    .IsRowVersion();
            }

            foreach (var foreignKey in entity.GetForeignKeys())
            {
                foreignKey.DeleteBehavior = DeleteBehavior.NoAction;
            }
        }
    }

    private void UpdateTimestamps()
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Property(x => x.CreatedAt).IsModified = false;
            }
        }
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        UpdateTimestamps();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();

        return base.SaveChangesAsync(
            acceptAllChangesOnSuccess,
            cancellationToken);
    }
}