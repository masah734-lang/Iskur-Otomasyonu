using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Iskur.Api;

public partial class ApiController
{
    DateTimeOffset Now => HttpContext.RequestServices.GetRequiredService<TimeProvider>().GetUtcNow();
    bool StudentRole => User.IsInRole(Roles.Student);
    bool AdvisorRole => User.IsInRole(Roles.Advisor);
    bool HeadRole => User.IsInRole(Roles.Head);

    Task<int?> MyFaculty() => db.Users.Where(x => x.Id == Uid).Select(x => x.FacultyId).SingleAsync();

    Task<int> ParticipationFaculty(int id) => db.Assignments.Where(x => x.ParticipationId == id && x.End == null)
        .Select(x => x.WorkUnit.FacultyId).SingleAsync();

    Task<Assignment?> CurrentAssignment(int id) =>
        db.Assignments.Include(x => x.WorkUnit).SingleOrDefaultAsync(x => x.ParticipationId == id && x.End == null);

    async Task<FacultyPolicy> Policy(int faculty) =>
        await db.Set<FacultyPolicy>().AsNoTracking().SingleOrDefaultAsync(x => x.FacultyId == faculty)
        ?? FinalRules.DefaultPolicy(faculty);

    IQueryable<PlanDay> AcceptedDays(int id) => db.PlanDays.Where(x =>
        x.MonthlyPlan.ParticipationId == id && x.Date <= x.MonthlyPlan.Participation.End && x.MonthlyPlan.Status == "Submitted" && x.Decision == "Accepted");

    IActionResult Invalid(string message) => BadRequest(new { message });
    IActionResult Changed(string message) => Conflict(new { message });

    [HttpGet("settings/policy"), Authorize(Roles = Roles.Management)]
    public async Task<IActionResult> GetPolicy()
    {
        var faculty = await MyFaculty();
        return faculty.HasValue ? Ok(await Policy(faculty.Value)) : Forbid();
    }

    [HttpPut("settings/policy"), Authorize(Roles = Roles.Management)]
    public async Task<IActionResult> SetPolicy(PolicyInput request)
    {
        var faculty = await MyFaculty();
        if (!faculty.HasValue) return Forbid();
        if (request.LeaveDays is < 0 or > 365 || request.LeaveWarningDays < 0 ||
            request.LeaveWarningDays > request.LeaveDays || request.AbsenceWarningDays is < 1 or > 365)
            return Invalid("İzin hakkını ve uyarı eşiklerini kontrol edin.");

        await using var transaction = await db.Database.BeginTransactionAsync();
        await db.LockOperation($"Iskur.Settings:{faculty}");
        var policy = await db.Set<FacultyPolicy>().FindAsync(faculty.Value);
        if (policy is null)
        {
            policy = FinalRules.DefaultPolicy(faculty.Value);
            db.Add(policy);
        }

        policy.LeaveDays = request.LeaveDays;
        policy.LeaveWarningDays = request.LeaveWarningDays;
        policy.AbsenceWarningDays = request.AbsenceWarningDays;
        policy.MedicalLeaveExempt = request.MedicalLeaveExempt;
        Log("FacultyPolicyChanged", faculty);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return Ok();
    }

    [HttpGet("settings/plan-windows"), Authorize(Roles = Roles.Management)]
    public async Task<IActionResult> PlanWindows()
    {
        var faculty = await MyFaculty();
        if (!faculty.HasValue) return Forbid();
        return Ok(await db.Set<PlanWindow>().Where(x => x.FacultyId == faculty)
            .OrderByDescending(x => x.Year).ThenByDescending(x => x.Month).ToListAsync());
    }

    [HttpPut("settings/plan-windows"), Authorize(Roles = Roles.Management)]
    public async Task<IActionResult> SetPlanWindow(PlanWindowInput request)
    {
        var faculty = await MyFaculty();
        if (!faculty.HasValue) return Forbid();
        if (request.Year is < 2020 or > 2100 || request.Month is < 1 or > 12 || request.Opens > request.Closes)
            return Invalid("Ay, yıl veya düzenleme aralığı geçersiz.");

        var monthEnd = new DateOnly(request.Year, request.Month, 1).AddMonths(1).AddDays(-1);
        if (request.Closes > monthEnd)
            return Invalid("Düzenleme dönemi çalışma ayının son gününden sonra bitemez.");

        await using var transaction = await db.Database.BeginTransactionAsync();
        await db.LockOperation($"Iskur.Settings:{faculty}");
        var window = await db.Set<PlanWindow>().FindAsync(faculty.Value, request.Year, request.Month);
        if (window is null)
        {
            window = new() { FacultyId = faculty.Value, Year = request.Year, Month = request.Month };
            db.Add(window);
        }

        window.Opens = request.Opens;
        window.Closes = request.Closes;
        Log("PlanWindowChanged", $"{faculty}/{request.Year}/{request.Month}");
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return Ok();
    }
}

public sealed record PolicyInput(int LeaveDays, int LeaveWarningDays, int AbsenceWarningDays, bool MedicalLeaveExempt);
public sealed record PlanWindowInput(int Year, int Month, DateOnly Opens, DateOnly Closes);
