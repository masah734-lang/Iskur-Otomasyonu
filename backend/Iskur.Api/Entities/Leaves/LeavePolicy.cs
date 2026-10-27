using Iskur.Api.Entities.Common;

namespace Iskur.Api.Entities.Leaves;

public class LeavePolicy : BaseEntity
{
    public Guid ProgramPeriodId { get; set; }

    public Guid LeaveTypeId { get; set; }

    public DateOnly ValidFrom { get; set; }

    // Bitiş tarihi hariçtir.
    public DateOnly? ValidUntil { get; set; }

    public decimal? EntitlementDays { get; set; }

    public string? EntitlementScope { get; set; }

    public bool? DeductsEntitlement { get; set; }

    public decimal? HalfDayDeduction { get; set; }

    public decimal? WarningThresholdDays { get; set; }
}