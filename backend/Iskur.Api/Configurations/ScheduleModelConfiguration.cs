using Iskur.Api.Entities.Documents;
using Iskur.Api.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace Iskur.Api.Configurations;

public static class ScheduleModelConfiguration
{
    public static void ConfigureScheduleModel(
        this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AcademicTerm>(b =>
        {
            b.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            b.ToTable("AcademicTerms", t =>
                t.HasCheckConstraint(
                    "CK_AcademicTerms_Dates",
                    "[EndsOn] > [StartsOn]"));
        });

        modelBuilder.Entity<StoredFile>(b =>
        {
            b.Property(x => x.OriginalName)
                .HasMaxLength(255)
                .IsRequired();

            b.Property(x => x.StorageKey)
                .HasMaxLength(500)
                .IsRequired();

            b.Property(x => x.ContentType)
                .HasMaxLength(100)
                .IsRequired();

            b.HasIndex(x => x.StorageKey)
                .IsUnique();

            b.HasOne(x => x.UploadedBy)
                .WithMany()
                .HasForeignKey(x => x.UploadedById);

            b.ToTable("StoredFiles", t =>
                t.HasCheckConstraint(
                    "CK_StoredFiles_Size",
                    "[SizeBytes] > 0 AND [SizeBytes] <= 10485760"));
        });

        modelBuilder.Entity<CourseSchedule>(b =>
        {
            b.HasOne(x => x.Student)
                .WithMany()
                .HasForeignKey(x => x.StudentId);

            b.HasOne(x => x.AcademicTerm)
                .WithMany(x => x.CourseSchedules)
                .HasForeignKey(x => x.AcademicTermId);

            b.HasOne(x => x.SourceFile)
                .WithMany()
                .HasForeignKey(x => x.SourceFileId);

            b.HasIndex(x => new
            {
                x.StudentId,
                x.AcademicTermId,
                x.VersionNumber
            }).IsUnique();

            b.ToTable("CourseSchedules", t =>
            {
                t.HasCheckConstraint(
                    "CK_CourseSchedules_Version",
                    "[VersionNumber] > 0");

                t.HasCheckConstraint(
                    "CK_CourseSchedules_Dates",
                    "[ValidUntil] IS NULL OR [ValidUntil] > [ValidFrom]");

                t.HasCheckConstraint(
                    "CK_CourseSchedules_Status",
                    "[Status] IN (0, 1)");
            });
        });

        modelBuilder.Entity<CourseMeeting>(b =>
        {
            b.Property(x => x.CourseName)
                .HasMaxLength(200)
                .IsRequired();

            b.HasOne(x => x.CourseSchedule)
                .WithMany(x => x.CourseMeetings)
                .HasForeignKey(x => x.CourseScheduleId);

            b.ToTable("CourseMeetings", t =>
            {
                t.HasCheckConstraint(
                    "CK_CourseMeetings_Times",
                    "[EndsAt] > [StartsAt]");

                t.HasCheckConstraint(
                    "CK_CourseMeetings_Weekday",
                    "[Weekday] BETWEEN 0 AND 6");
            });
        });

        modelBuilder.Entity<CalendarHoliday>(b =>
        {
            b.Property(x => x.Name)
                .HasMaxLength(150)
                .IsRequired();

            b.ToTable("CalendarHolidays", t =>
                t.HasCheckConstraint(
                    "CK_CalendarHolidays_Dates",
                    "[EndsAt] > [StartsAt]"));
        });

        modelBuilder.Entity<WorkRule>(b =>
        {
            b.HasOne(x => x.ProgramPeriod)
                .WithMany()
                .HasForeignKey(x => x.ProgramPeriodId);

            b.Property(x => x.WeeklyMinimum).HasPrecision(9, 2);
            b.Property(x => x.WeeklyMaximum).HasPrecision(9, 2);
            b.Property(x => x.HalfDayWeeklyWeight).HasPrecision(9, 2);

            b.ToTable("WorkRules", t =>
            {
                t.HasCheckConstraint(
                    "CK_WorkRules_Dates",
                    "[ValidUntil] IS NULL OR [ValidUntil] > [ValidFrom]");

                t.HasCheckConstraint(
                    "CK_WorkRules_Times",
                    """
                    [MorningStartsAt] < [MorningEndsAt]
                    AND [MorningEndsAt] <= [AfternoonStartsAt]
                    AND [AfternoonStartsAt] < [AfternoonEndsAt]
                    """);

                t.HasCheckConstraint(
                    "CK_WorkRules_WeeklyLimits",
                    "[WeeklyMinimum] >= 0 AND [WeeklyMaximum] >= [WeeklyMinimum]");

                t.HasCheckConstraint(
                    "CK_WorkRules_HalfDayWeight",
                    """
                    [HalfDayWeeklyWeight] IS NULL
                    OR ([HalfDayWeeklyWeight] > 0
                        AND [HalfDayWeeklyWeight] <= 1)
                    """);

                t.HasCheckConstraint(
                    "CK_WorkRules_QrTimes",
                    "[QrWindowMinutes] > 0 AND [QrRefreshSeconds] > 0");
            });
        });

        modelBuilder.Entity<PlanApplicationWindow>(b =>
        {
            b.HasOne(x => x.ProgramPeriod)
                .WithMany()
                .HasForeignKey(x => x.ProgramPeriodId);

            b.HasIndex(x => new
            {
                x.ProgramPeriodId,
                x.Year,
                x.Month
            }).IsUnique();

            b.ToTable("PlanApplicationWindows", t =>
            {
                t.HasCheckConstraint(
                    "CK_PlanApplicationWindows_Year",
                    "[Year] BETWEEN 1900 AND 2200");

                t.HasCheckConstraint(
                    "CK_PlanApplicationWindows_Month",
                    "[Month] BETWEEN 1 AND 12");

                t.HasCheckConstraint(
                    "CK_PlanApplicationWindows_Dates",
                    "[ClosesAt] > [OpensAt]");
            });
        });
    }
}