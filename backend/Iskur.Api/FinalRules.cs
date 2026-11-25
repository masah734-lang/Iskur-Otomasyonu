using Microsoft.EntityFrameworkCore;

namespace Iskur.Api;

public static class FinalRules
{
    public static readonly TimeZoneInfo Istanbul = TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");

    public static DateOnly LocalDate(DateTimeOffset instant) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(instant, Istanbul).DateTime);

    public static (int Start, int End) SlotMinutes(WorkSlot slot) =>
        slot == WorkSlot.Morning ? PlanningRules.Windows("Morning")[0] : PlanningRules.Windows("Afternoon")[0];

    public static DateTimeOffset StartOf(DateOnly date, WorkSlot slot)
    {
        var local = date.ToDateTime(TimeOnly.MinValue).AddMinutes(SlotMinutes(slot).Start);
        return new DateTimeOffset(local, Istanbul.GetUtcOffset(local));
    }

    public static DateTimeOffset EndOf(DateOnly date, WorkSlot slot) =>
        StartOf(date, slot).AddMinutes(Minutes(slot));

    public static int Minutes(WorkSlot slot)
    {
        var (start, end) = SlotMinutes(slot);
        return end - start;
    }

    public static WorkSlot[] Slots(string kind) => kind switch
    {
        "Morning" => [WorkSlot.Morning],
        "Afternoon" => [WorkSlot.Afternoon],
        "Full" => [WorkSlot.Morning, WorkSlot.Afternoon],
        _ => throw new ArgumentException("Çalışma türü geçersiz.")
    };

    public static CheckInStatus? StatusAt(SlotAttendance? recorded, DateTimeOffset now, DateOnly date, WorkSlot slot) =>
        recorded?.Status ?? (now >= StartOf(date, slot).AddMinutes(20) ? CheckInStatus.Absent : null);

    public static int QrPeriodSeconds => 30;

    public static FacultyPolicy DefaultPolicy(int facultyId) => new() { FacultyId = facultyId };
}

public partial class AppDb
{
    internal Task LockParticipation(int participationId) => LockOperation($"Iskur.Participation:{participationId}");

    internal async Task LockOperation(string resource)
    {
        await Database.ExecuteSqlInterpolatedAsync($"""
            DECLARE @result int;
            EXEC @result = sys.sp_getapplock @Resource = {resource},
                @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 5000;
            IF @result < 0 THROW 51000, N'Katılım kaydı şu anda değiştiriliyor.', 1;
            """);
    }
}
