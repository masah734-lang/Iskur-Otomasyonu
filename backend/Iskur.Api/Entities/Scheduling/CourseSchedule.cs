using Iskur.Api.Entities.Common;
using Iskur.Api.Entities.Documents;
using Iskur.Api.Entities.Students;

namespace Iskur.Api.Entities.Scheduling;

public enum ScheduleStatus
{
    Draft = 0,
    Confirmed = 1
}

public class CourseSchedule : BaseEntity
{
    public Guid StudentId { get; set; }

    public Guid AcademicTermId { get; set; }

    public int VersionNumber { get; set; } = 1;

    public Guid? SourceFileId { get; set; }

    public ScheduleStatus Status { get; set; }
        = ScheduleStatus.Draft;

    public DateOnly ValidFrom { get; set; }

    // Bitiş tarihi hariçtir; boşsa henüz bitmemiştir.
    public DateOnly? ValidUntil { get; set; }

    public DateTimeOffset? ConfirmedAt { get; set; }

    public StudentProfile Student { get; set; } = null!;

    public AcademicTerm AcademicTerm { get; set; } = null!;

    public StoredFile? SourceFile { get; set; }

    public ICollection<CourseMeeting> CourseMeetings { get; set; }
        = new List<CourseMeeting>();
}