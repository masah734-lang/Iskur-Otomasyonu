using Iskur.Api.Entities.Common;
using Iskur.Api.Entities.WorkPlans;

namespace Iskur.Api.Entities;

public enum AttendanceStatus
{
    Pending = 0,
    Present = 1,
    Absent = 2,
    OnLeave = 3
}

public enum AttendanceSource
{
    Pending = 0,
    DirectQr = 1,
    DailyQr = 2,
    ApprovedLeave = 3,
    Manual = 4
}

public class QrSession : BaseEntity
{
    public Guid WorkUnitId { get; set; }

    public SessionType SessionType { get; set; }

    public DateTimeOffset StartsAt { get; set; }

    public DateTimeOffset EndsAt { get; set; }

    public DateTimeOffset ScanDeadline { get; set; }

    public Guid CreatedById { get; set; }

    public DateTimeOffset? CancelledAt { get; set; }
}

public class QrScan : BaseEntity
{
    public Guid QrSessionId { get; set; }

    public Guid ParticipationId { get; set; }

    public DateTimeOffset ScannedAt { get; set; }

    // Doğrulanan tokenın özeti tutulur; gizli anahtar tutulmaz.
    public string TokenDigest { get; set; } = string.Empty;
}

public class SessionAttendance : BaseEntity
{
    public Guid WorkSessionId { get; set; }

    public AttendanceStatus Status { get; set; }
        = AttendanceStatus.Pending;

    public AttendanceSource Source { get; set; }
        = AttendanceSource.Pending;

    public Guid? QrScanId { get; set; }

    public Guid? LeaveRequestSessionId { get; set; }

    public int AcceptedMinutes { get; set; }
}

public class AttendanceCorrection : BaseEntity
{
    public Guid SessionAttendanceId { get; set; }

    public AttendanceStatus PreviousStatus { get; set; }

    public AttendanceStatus NewStatus { get; set; }

    public int PreviousMinutes { get; set; }

    public int NewMinutes { get; set; }

    public string Reason { get; set; } = string.Empty;

    public Guid PerformedById { get; set; }
}