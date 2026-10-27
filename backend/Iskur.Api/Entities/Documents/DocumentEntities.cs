using Iskur.Api.Entities.Common;

namespace Iskur.Api.Entities;

public class LeaveDocument : BaseEntity
{
    public Guid LeaveRequestId { get; set; }

    public Guid StoredFileId { get; set; }
}

public class DecisionDocument : BaseEntity
{
    public Guid AdministrativeDecisionId { get; set; }

    public Guid StoredFileId { get; set; }
}

public class AuditEntry : BaseEntity
{
    // Sistem tarafından yapılan işlemlerde boş olabilir.
    public Guid? UserId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string EntityType { get; set; } = string.Empty;

    public Guid? RecordId { get; set; }

    public string Summary { get; set; } = string.Empty;

    // Hassas değişiklik ayrıntılarına erişim ayrıca kısıtlanacak.
    public string? RestrictedDetails { get; set; }

    public string CorrelationId { get; set; } = string.Empty;
}