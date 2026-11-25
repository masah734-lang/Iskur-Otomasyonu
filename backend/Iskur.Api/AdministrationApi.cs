using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Iskur.Api;

public partial class ApiController
{
    [HttpGet("administration")]
    [Authorize(Roles = Roles.Management)]
    public async Task<IActionResult> Administration()
    {
        var faculty = await MyFaculty();
        if (!faculty.HasValue) return Forbid();
        var staff = await db.Users.Where(u => u.FacultyId == faculty)
            .Where(u => db.UserRoles.Any(r => r.UserId == u.Id &&
                db.Roles.Any(rr => rr.Id == r.RoleId && rr.Name != Roles.Student)))
            .OrderBy(u => u.FullName).Select(u => new
            {
                u.Id, u.FullName, u.Email, u.DepartmentId,
                role = db.UserRoles.Where(r => r.UserId == u.Id).Join(db.Roles, r => r.RoleId, rr => rr.Id, (r, rr) => rr.Name)
                    .Where(name => name != Roles.Student).FirstOrDefault()
            }).ToListAsync();
        return Ok(new { staff,
            departments = await db.Departments.Where(x => x.FacultyId == faculty).OrderBy(x => x.Name)
                .Select(x => new { x.Id, x.Name }).ToListAsync(),
            units = await db.WorkUnits.Where(x => x.FacultyId == faculty).OrderBy(x => x.Name)
                .Select(x => new { x.Id, x.Name, x.DepartmentId }).ToListAsync(),
            periods = await db.Periods.OrderByDescending(x => x.Start).ToListAsync() });
    }

    [HttpPost("administration/staff"), Authorize(Roles = Roles.Management)]
    public async Task<IActionResult> CreateStaff(StaffInput input)
    {
        var faculty = await MyFaculty();
        if (!faculty.HasValue) return Forbid();
        if (input.Role is not (Roles.Advisor or Roles.Head or Roles.Management) || string.IsNullOrWhiteSpace(input.FullName))
            return Invalid("Personel adı ve rolü geçersiz.");
        if (input.Role == Roles.Head && !input.DepartmentId.HasValue || input.DepartmentId.HasValue &&
            !await db.Departments.AnyAsync(d => d.Id == input.DepartmentId && d.FacultyId == faculty))
            return Invalid("Geçerli bir bölüm seçin.");

        await using var transaction = await db.Database.BeginTransactionAsync();
        var staff = new User { FullName = input.FullName.Trim(), UserName = input.Email.Trim(),
            Email = input.Email.Trim(), FacultyId = faculty, DepartmentId = input.DepartmentId };
        var result = await users.CreateAsync(staff, input.Password);
        if (!result.Succeeded) return Invalid(string.Join(" ", result.Errors.Select(x => x.Description)));
        result = await users.AddToRoleAsync(staff, input.Role);
        if (!result.Succeeded) return Invalid(string.Join(" ", result.Errors.Select(x => x.Description)));

        Log("StaffCreated", staff.Id);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return Ok();
    }

    [HttpPost("administration/departments"), Authorize(Roles = Roles.Management)]
    public async Task<IActionResult> AddDepartment(OrganizationInput input)
    {
        var faculty = await MyFaculty();
        if (!faculty.HasValue) return Forbid();
        var name = input.Name.Trim();
        if (name.Length == 0 || await db.Departments.AnyAsync(x => x.FacultyId == faculty && x.Name == name))
            return Invalid("Bölüm adı boş veya zaten kayıtlı.");
        db.Departments.Add(new Department { Name = name, FacultyId = faculty.Value });
        Log("DepartmentCreated", faculty);
        await db.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("administration/units"), Authorize(Roles = Roles.Management)]
    public async Task<IActionResult> AddUnit(OrganizationInput input)
    {
        var faculty = await MyFaculty();
        if (!faculty.HasValue) return Forbid();
        if (input.DepartmentId.HasValue && !await db.Departments.AnyAsync(x => x.Id == input.DepartmentId && x.FacultyId == faculty))
            return Invalid("Bölüm fakülteye ait olmalıdır.");
        var name = input.Name.Trim();
        if (name.Length == 0 || await db.WorkUnits.AnyAsync(x => x.FacultyId == faculty && x.Name == name))
            return Invalid("Birim adı boş veya zaten kayıtlı.");
        db.WorkUnits.Add(new WorkUnit { Name = name, FacultyId = faculty.Value, DepartmentId = input.DepartmentId });
        Log("WorkUnitCreated", faculty);
        await db.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("administration/periods"), Authorize(Roles = Roles.Management)]
    public async Task<IActionResult> AddPeriod(PeriodInput input)
    {
        if ((await MyFaculty()) is null)
            return Forbid();

        var name = input.Name.Trim();
        if (name.Length == 0 || input.Start >= input.End || input.Start.Year < 2020 ||
            input.End.Year > 2100 || input.End.DayNumber - input.Start.DayNumber > 366)
            return Invalid("Dönem adı ve tarihleri geçersiz.");
        if (await db.Periods.AnyAsync(x => x.Start == input.Start && x.End == input.End))
            return Invalid("Bu tarih aralığında bir dönem zaten kayıtlı.");
        db.Periods.Add(new ProgramPeriod { Name = name, Start = input.Start, End = input.End });
        Log("PeriodCreated", name);
        await db.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("students/{id:int}/participations"), Authorize(Roles = Roles.Management)]
    public async Task<IActionResult> AddParticipation(int id, ParticipationInput input)
    {
        if (!await Can(id)) return Forbid();
        var faculty = await MyFaculty();
        if (!faculty.HasValue) return Forbid();
        var previous = await db.Participations.FindAsync(id);
        if (previous is null) return NotFound();

        await using var transaction = await db.Database.BeginTransactionAsync();
        await db.LockOperation($"Iskur.StudentProfile:{previous.StudentProfileId}");
        var period = await db.Periods.FindAsync(input.ProgramPeriodId);
        var unit = await db.WorkUnits.FindAsync(input.WorkUnitId);
        if (period is null || unit?.FacultyId != faculty)
            return Invalid("Geçerli dönem ve çalışma birimi seçin.");
        var other = await db.Participations.Where(x => x.StudentProfileId == previous.StudentProfileId).ToListAsync();
        if (other.Any(p => p.ProgramPeriodId == period.Id || p.Start <= period.End && p.End >= period.Start))
            return Changed("Öğrencinin aynı dönemde veya çakışan tarihlerde katılımı bulunuyor.");
        var today = FinalRules.LocalDate(Now);
        foreach (var old in other.Where(p => p.Active && p.End < today))
            old.Active = false;

        var added = new Participation { StudentProfileId = previous.StudentProfileId, ProgramPeriodId = period.Id,
            Start = period.Start, End = period.End };
        db.Add(added);
        await db.SaveChangesAsync();
        db.Assignments.Add(new Assignment { ParticipationId = added.Id, WorkUnitId = unit!.Id, Start = Now });
        Log("ParticipationCreated", added.Id);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return Ok(new { added.Id });
    }
}

public sealed record StaffInput([Required, StringLength(100)] string FullName, [Required, EmailAddress] string Email,
    [Required, MinLength(10)] string Password, string Role, int? DepartmentId);
public sealed record OrganizationInput([Required, StringLength(100)] string Name, int? DepartmentId);
public sealed record PeriodInput([Required, StringLength(100)] string Name, DateOnly Start, DateOnly End);
public sealed record ParticipationInput(int ProgramPeriodId, int WorkUnitId);
