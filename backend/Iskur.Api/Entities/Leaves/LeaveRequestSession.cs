using Iskur.Api.Entities.Common;

namespace Iskur.Api.Entities.Leaves;

public class LeaveRequestSession : BaseEntity
{
    public Guid LeaveRequestId { get; set; }

    public Guid WorkSessionId { get; set; }

    public Guid? LeavePolicyId { get; set; }

    public int LeaveMinutes { get; set; }

    // İzin hesabı kesinleşene kadar boş kalabilir.
    public decimal? DeductionDays { get; set; }
}