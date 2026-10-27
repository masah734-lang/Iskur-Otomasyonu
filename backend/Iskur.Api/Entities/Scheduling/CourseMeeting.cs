using Iskur.Api.Entities.Common;

namespace Iskur.Api.Entities.Scheduling;

public class CourseMeeting : BaseEntity
{
    public Guid CourseScheduleId { get; set; }

    public string CourseName { get; set; } = string.Empty;

    public DayOfWeek Weekday { get; set; }

    public TimeOnly StartsAt { get; set; }

    public TimeOnly EndsAt { get; set; }

    public CourseSchedule CourseSchedule { get; set; } = null!;
}