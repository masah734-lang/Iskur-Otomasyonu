using Iskur.Api.Entities.Common;
using Iskur.Api.Entities.Students;

namespace Iskur.Api.Entities.Scheduling;

public class PlanApplicationWindow : BaseEntity
{
    public Guid ProgramPeriodId { get; set; }

    public int Year { get; set; }

    public int Month { get; set; }

    public DateTimeOffset OpensAt { get; set; }

    public DateTimeOffset ClosesAt { get; set; }

    public ProgramPeriod ProgramPeriod { get; set; } = null!;
}