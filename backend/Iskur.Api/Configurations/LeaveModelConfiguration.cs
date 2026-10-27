using Iskur.Api.Entities.Identity;
using Iskur.Api.Entities.Leaves;
using Iskur.Api.Entities.Students;
using Iskur.Api.Entities.WorkPlans;
using Microsoft.EntityFrameworkCore;

namespace Iskur.Api.Configurations;

public static class LeaveModelConfiguration
{
    public static void ConfigureLeaveModel(
        this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LeaveType>(b =>
        {
            b.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            b.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<LeavePolicy>(b =>
        {
            b.HasOne<ProgramPeriod>()
                .WithMany()
                .HasForeignKey(x => x.ProgramPeriodId);

            b.HasOne<LeaveType>()
                .WithMany()
                .HasForeignKey(x => x.LeaveTypeId);

            b.Property(x => x.EntitlementScope).HasMaxLength(50);

            b.Property(x => x.EntitlementDays).HasPrecision(9, 2);
            b.Property(x => x.HalfDayDeduction).HasPrecision(9, 2);
            b.Property(x => x.WarningThresholdDays).HasPrecision(9, 2);

            b.ToTable("LeavePolicies", t =>
            {
                t.HasCheckConstraint(
                    "CK_LeavePolicies_Dates",
                    "[ValidUntil] IS NULL OR [ValidUntil] > [ValidFrom]");

                t.HasCheckConstraint(
                    "CK_LeavePolicies_Entitlement",
                    "[EntitlementDays] IS NULL OR [EntitlementDays] >= 0");

                t.HasCheckConstraint(
                    "CK_LeavePolicies_HalfDay",
                    """
                    [HalfDayDeduction] IS NULL OR
                    ([HalfDayDeduction] >= 0 AND [HalfDayDeduction] <= 1)
                    """);

                t.HasCheckConstraint(
                    "CK_LeavePolicies_Warning",
                    "[WarningThresholdDays] IS NULL OR [WarningThresholdDays] >= 0");
            });
        });

        modelBuilder.Entity<LeaveRequest>(b =>
        {
            b.HasOne<ProgramParticipation>()
                .WithMany()
                .HasForeignKey(x => x.ParticipationId);

            b.HasOne<LeaveType>()
                .WithMany()
                .HasForeignKey(x => x.LeaveTypeId);

            b.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.DecidedById);

            b.Property(x => x.Reason)
                .HasMaxLength(2000)
                .IsRequired();

            b.Property(x => x.DecisionReason)
                .HasMaxLength(2000);

            b.HasIndex(x => new
            {
                x.ParticipationId,
                x.StartsOn,
                x.EndsOn
            });

            b.ToTable("LeaveRequests", t =>
            {
                t.HasCheckConstraint(
                    "CK_LeaveRequests_Dates",
                    "[EndsOn] >= [StartsOn]");

                t.HasCheckConstraint(
                    "CK_LeaveRequests_Status",
                    "[Status] IN (0, 1, 2, 3)");
            });
        });

        modelBuilder.Entity<LeaveRequestSession>(b =>
        {
            b.HasOne<LeaveRequest>()
                .WithMany()
                .HasForeignKey(x => x.LeaveRequestId);

            b.HasOne<WorkSession>()
                .WithMany()
                .HasForeignKey(x => x.WorkSessionId);

            b.HasOne<LeavePolicy>()
                .WithMany()
                .HasForeignKey(x => x.LeavePolicyId);

            b.Property(x => x.DeductionDays)
                .HasPrecision(9, 2);

            b.HasIndex(x => new
            {
                x.LeaveRequestId,
                x.WorkSessionId
            }).IsUnique();

            b.ToTable("LeaveRequestSessions", t =>
            {
                t.HasCheckConstraint(
                    "CK_LeaveRequestSessions_Minutes",
                    "[LeaveMinutes] > 0");

                t.HasCheckConstraint(
                    "CK_LeaveRequestSessions_Deduction",
                    "[DeductionDays] IS NULL OR [DeductionDays] >= 0");
            });
        });

        modelBuilder.Entity<LeaveMovement>(b =>
        {
            b.HasOne<ProgramParticipation>()
                .WithMany()
                .HasForeignKey(x => x.ParticipationId);

            b.HasOne<LeaveRequest>()
                .WithMany()
                .HasForeignKey(x => x.LeaveRequestId);

            b.HasOne<LeavePolicy>()
                .WithMany()
                .HasForeignKey(x => x.LeavePolicyId);

            b.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.PerformedById);

            b.Property(x => x.DayAmount).HasPrecision(9, 2);

            b.Property(x => x.Description).HasMaxLength(2000);

            b.HasIndex(x => x.ParticipationId);

            b.ToTable("LeaveMovements", t =>
            {
                t.HasCheckConstraint(
                    "CK_LeaveMovements_Type",
                    "[MovementType] IN (0, 1, 2, 3)");

                t.HasCheckConstraint(
                    "CK_LeaveMovements_Amount",
                    "[DayAmount] <> 0");
            });
        });

        modelBuilder.Entity<LeaveRequestAction>(b =>
        {
            b.HasOne<LeaveRequest>()
                .WithMany()
                .HasForeignKey(x => x.LeaveRequestId);

            b.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.PerformedById);

            b.Property(x => x.Action)
                .HasMaxLength(50)
                .IsRequired();

            b.Property(x => x.NewValues).IsRequired();

            b.Property(x => x.Reason).HasMaxLength(2000);
        });
    }
}