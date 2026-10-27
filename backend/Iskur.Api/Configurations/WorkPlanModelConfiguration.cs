using Iskur.Api.Entities.WorkPlans;
using Microsoft.EntityFrameworkCore;

namespace Iskur.Api.Configurations;

public static class WorkPlanModelConfiguration
{
    public static void ConfigureWorkPlanModel(
        this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MonthlyWorkPlan>(b =>
        {
            b.HasOne(x => x.Participation)
                .WithMany()
                .HasForeignKey(x => x.ParticipationId);

            b.HasIndex(x => new
            {
                x.ParticipationId,
                x.Year,
                x.Month
            }).IsUnique();

            b.ToTable("MonthlyWorkPlans", t =>
            {
                t.HasCheckConstraint(
                    "CK_MonthlyWorkPlans_Year",
                    "[Year] BETWEEN 1900 AND 2200");

                t.HasCheckConstraint(
                    "CK_MonthlyWorkPlans_Month",
                    "[Month] BETWEEN 1 AND 12");

                t.HasCheckConstraint(
                    "CK_MonthlyWorkPlans_Status",
                    "[Status] IN (0, 1)");
            });
        });

        modelBuilder.Entity<WorkPlanDay>(b =>
        {
            b.Property(x => x.Explanation)
                .HasMaxLength(2000);

            b.Property(x => x.DecisionReason)
                .HasMaxLength(2000);

            b.HasOne(x => x.MonthlyWorkPlan)
                .WithMany(x => x.Days)
                .HasForeignKey(x => x.MonthlyWorkPlanId);

            b.HasOne(x => x.DecidedBy)
                .WithMany()
                .HasForeignKey(x => x.DecidedById);

            b.HasIndex(x => new
            {
                x.MonthlyWorkPlanId,
                x.WorkDate
            }).IsUnique();

            b.HasIndex(x => x.WorkDate);

            b.ToTable("WorkPlanDays", t =>
            {
                t.HasCheckConstraint(
                    "CK_WorkPlanDays_WorkType",
                    "[WorkType] IN (0, 1, 2)");

                t.HasCheckConstraint(
                    "CK_WorkPlanDays_Decision",
                    "[Decision] IN (0, 1, 2, 3)");

                t.HasCheckConstraint(
                    "CK_WorkPlanDays_Version",
                    "[VersionNumber] > 0");
            });
        });

        modelBuilder.Entity<WorkSession>(b =>
        {
            b.Property(x => x.ConflictDetails)
                .HasMaxLength(2000);

            b.HasOne(x => x.WorkPlanDay)
                .WithMany(x => x.Sessions)
                .HasForeignKey(x => x.WorkPlanDayId);

            b.HasOne(x => x.UnitAssignment)
                .WithMany()
                .HasForeignKey(x => x.UnitAssignmentId);

            b.HasOne(x => x.AdvisorAssignment)
                .WithMany()
                .HasForeignKey(x => x.AdvisorAssignmentId);

            b.HasOne(x => x.WorkRule)
                .WithMany()
                .HasForeignKey(x => x.WorkRuleId);

            b.HasOne(x => x.CourseSchedule)
                .WithMany()
                .HasForeignKey(x => x.CourseScheduleId);

            b.HasIndex(x => new
            {
                x.WorkPlanDayId,
                x.SessionType
            }).IsUnique();

            b.HasIndex(x => x.StartsAt);

            b.ToTable("WorkSessions", t =>
            {
                t.HasCheckConstraint(
                    "CK_WorkSessions_Type",
                    "[SessionType] IN (0, 1)");

                t.HasCheckConstraint(
                    "CK_WorkSessions_Times",
                    "[EndsAt] > [StartsAt]");

                t.HasCheckConstraint(
                    "CK_WorkSessions_Minutes",
                    "[PlannedMinutes] > 0");
            });
        });

        modelBuilder.Entity<PlanDayAction>(b =>
        {
            b.Property(x => x.Action)
                .HasMaxLength(50)
                .IsRequired();

            b.Property(x => x.NewValues)
                .IsRequired();

            b.Property(x => x.Reason)
                .HasMaxLength(2000);

            b.HasOne(x => x.WorkPlanDay)
                .WithMany(x => x.Actions)
                .HasForeignKey(x => x.WorkPlanDayId);

            b.HasOne(x => x.PerformedBy)
                .WithMany()
                .HasForeignKey(x => x.PerformedById);

            b.ToTable("PlanDayActions", t =>
                t.HasCheckConstraint(
                    "CK_PlanDayActions_Version",
                    "[VersionNumber] > 0"));
        });
    }
}