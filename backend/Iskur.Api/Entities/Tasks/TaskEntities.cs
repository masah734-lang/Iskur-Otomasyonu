using Iskur.Api.Entities.Common;

namespace Iskur.Api.Entities;

public enum StudentTaskStatus
{
    Incomplete = 0,
    Completed = 1
}

public enum AbsenceReportStatus
{
    Open = 0,
    InReview = 1,
    Forwarded = 2,
    Closed = 3
}

public class StudentTask : BaseEntity
{
    public Guid ParticipationId { get; set; }

    public Guid AssignedById { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTimeOffset AssignedAt { get; set; }
        = DateTimeOffset.UtcNow;

    public DateTimeOffset? DueAt { get; set; }

    public StudentTaskStatus Status { get; set; }
        = StudentTaskStatus.Incomplete;

    public DateTimeOffset? CompletedAt { get; set; }
}

public class TaskAction : BaseEntity
{
    public Guid StudentTaskId { get; set; }

    public string? PreviousValues { get; set; }

    public string NewValues { get; set; } = string.Empty;

    public Guid PerformedById { get; set; }

    public string? Reason { get; set; }
}

public class EvaluationNote : BaseEntity
{
    public Guid ParticipationId { get; set; }

    public Guid? StudentTaskId { get; set; }

    public Guid AuthorId { get; set; }

    public string Text { get; set; } = string.Empty;

    // Not düzenlenirse önceki sürüme bağlanır.
    public Guid? PreviousNoteId { get; set; }
}

public class AbsenceReport : BaseEntity
{
    public Guid ParticipationId { get; set; }

    public Guid ReportedById { get; set; }

    public string Description { get; set; } = string.Empty;

    public AbsenceReportStatus Status { get; set; }
        = AbsenceReportStatus.Open;
}

public class AbsenceReportSession : BaseEntity
{
    public Guid AbsenceReportId { get; set; }

    public Guid WorkSessionId { get; set; }
}

public class ReportAction : BaseEntity
{
    public Guid AbsenceReportId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Guid PerformedById { get; set; }
}

public class AdministrativeDecision : BaseEntity
{
    public Guid ParticipationId { get; set; }

    public string DecisionType { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public Guid DecidedById { get; set; }

    public DateOnly DecisionDate { get; set; }

    public Guid? PreviousDecisionId { get; set; }
}