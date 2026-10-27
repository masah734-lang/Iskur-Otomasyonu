using Iskur.Api.Entities.Common;

namespace Iskur.Api.Entities.Scheduling;

public class AcademicTerm : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public DateOnly StartsOn { get; set; }

    // Bitiş tarihi hariçtir.
    public DateOnly EndsOn { get; set; }

    public ICollection<CourseSchedule> CourseSchedules { get; set; }
        = new List<CourseSchedule>();
}