using Iskur.Api.Entities.Common;
using Iskur.Api.Entities.Students;

namespace Iskur.Api.Entities.Scheduling;

public class WorkRule : BaseEntity
{
    public Guid ProgramPeriodId { get; set; }

    public DateOnly ValidFrom { get; set; }

    public DateOnly? ValidUntil { get; set; }

    public TimeOnly MorningStartsAt { get; set; }

    public TimeOnly MorningEndsAt { get; set; }

    public TimeOnly AfternoonStartsAt { get; set; }

    public TimeOnly AfternoonEndsAt { get; set; }

    public decimal WeeklyMinimum { get; set; }

    public decimal WeeklyMaximum { get; set; }

    // Bu iki kural henüz kesinleşmedi.
    public decimal? HalfDayWeeklyWeight { get; set; }

    public bool? WeekendWorkAllowed { get; set; }

    public int QrWindowMinutes { get; set; } = 20;

    public int QrRefreshSeconds { get; set; } = 30;

    public ProgramPeriod ProgramPeriod { get; set; } = null!;
}