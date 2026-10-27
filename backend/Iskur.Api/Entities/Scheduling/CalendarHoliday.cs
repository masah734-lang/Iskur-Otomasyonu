using Iskur.Api.Entities.Common;

namespace Iskur.Api.Entities.Scheduling;

public class CalendarHoliday : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public DateTimeOffset StartsAt { get; set; }

    public DateTimeOffset EndsAt { get; set; }
}