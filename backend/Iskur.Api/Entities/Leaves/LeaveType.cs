using Iskur.Api.Entities.Common;

namespace Iskur.Api.Entities.Leaves;

public class LeaveType : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public bool RequiresDocument { get; set; }

    public bool IsActive { get; set; } = true;
}