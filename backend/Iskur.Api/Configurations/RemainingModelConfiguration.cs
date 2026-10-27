using Iskur.Api.Entities;
using Iskur.Api.Entities.Documents;
using Iskur.Api.Entities.Identity;
using Iskur.Api.Entities.Leaves;
using Iskur.Api.Entities.Organization;
using Iskur.Api.Entities.Students;
using Iskur.Api.Entities.WorkPlans;
using Microsoft.EntityFrameworkCore;

namespace Iskur.Api.Configurations;

public static class RemainingModelConfiguration
{
    public static void ConfigureRemainingModel(
        this ModelBuilder modelBuilder)
    {
        ConfigureAttendance(modelBuilder);
        ConfigureTasks(modelBuilder);
        ConfigureDocuments(modelBuilder);
    }

    private static void ConfigureAttendance(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<QrSession>(b =>
        {
            b.HasOne<WorkUnit>()
                .WithMany()
                .HasForeignKey(x => x.WorkUnitId);

            b.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.CreatedById);

            b.HasIndex(x => new
            {
                x.WorkUnitId,
                x.StartsAt,
                x.SessionType
            }).IsUnique();

            b.ToTable("QrSessions", t =>
            {
                t.HasCheckConstraint(
                    "CK_QrSessions_Type",
                    "[SessionType] IN (0, 1)");

                t.HasCheckConstraint(
                    "CK_QrSessions_Times",
                    "[EndsAt] > [StartsAt]");

                t.HasCheckConstraint(
                    "CK_QrSessions_Deadline",
                    "[ScanDeadline] > [StartsAt] AND [ScanDeadline] <= [EndsAt]");
            });
        });

        modelBuilder.Entity<QrScan>(b =>
        {
            b.HasOne<QrSession>()
                .WithMany()
                .HasForeignKey(x => x.QrSessionId);

            b.HasOne<ProgramParticipation>()
                .WithMany()
                .HasForeignKey(x => x.ParticipationId);

            b.Property(x => x.TokenDigest)
                .HasMaxLength(128)
                .IsRequired();

            b.HasIndex(x => new
            {
                x.QrSessionId,
                x.ParticipationId
            }).IsUnique();
        });

        modelBuilder.Entity<SessionAttendance>(b =>
        {
            b.HasOne<WorkSession>()
                .WithMany()
                .HasForeignKey(x => x.WorkSessionId);

            b.HasOne<QrScan>()
                .WithMany()
                .HasForeignKey(x => x.QrScanId);

            b.HasOne<LeaveRequestSession>()
                .WithMany()
                .HasForeignKey(x => x.LeaveRequestSessionId);

            b.HasIndex(x => x.WorkSessionId).IsUnique();

            b.ToTable("SessionAttendances", t =>
            {
                t.HasCheckConstraint(
                    "CK_SessionAttendances_Status",
                    "[Status] IN (0, 1, 2, 3)");

                t.HasCheckConstraint(
                    "CK_SessionAttendances_Source",
                    "[Source] IN (0, 1, 2, 3, 4)");

                t.HasCheckConstraint(
                    "CK_SessionAttendances_Minutes",
                    "[AcceptedMinutes] >= 0");

                t.HasCheckConstraint(
                    "CK_SessionAttendances_SingleSource",
                    "NOT ([QrScanId] IS NOT NULL AND [LeaveRequestSessionId] IS NOT NULL)");
            });
        });

        modelBuilder.Entity<AttendanceCorrection>(b =>
        {
            b.HasOne<SessionAttendance>()
                .WithMany()
                .HasForeignKey(x => x.SessionAttendanceId);

            b.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.PerformedById);

            b.Property(x => x.Reason)
                .HasMaxLength(2000)
                .IsRequired();

            b.ToTable("AttendanceCorrections", t =>
            {
                t.HasCheckConstraint(
                    "CK_AttendanceCorrections_PreviousStatus",
                    "[PreviousStatus] IN (0, 1, 2, 3)");

                t.HasCheckConstraint(
                    "CK_AttendanceCorrections_NewStatus",
                    "[NewStatus] IN (0, 1, 2, 3)");

                t.HasCheckConstraint(
                    "CK_AttendanceCorrections_Minutes",
                    "[PreviousMinutes] >= 0 AND [NewMinutes] >= 0");
            });
        });
    }

    private static void ConfigureTasks(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StudentTask>(b =>
        {
            b.HasOne<ProgramParticipation>()
                .WithMany()
                .HasForeignKey(x => x.ParticipationId);

            b.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.AssignedById);

            b.Property(x => x.Title)
                .HasMaxLength(200)
                .IsRequired();

            b.Property(x => x.Description)
                .HasMaxLength(4000)
                .IsRequired();

            b.ToTable("StudentTasks", t =>
                t.HasCheckConstraint(
                    "CK_StudentTasks_Status",
                    "[Status] IN (0, 1)"));
        });

        modelBuilder.Entity<TaskAction>(b =>
        {
            b.HasOne<StudentTask>()
                .WithMany()
                .HasForeignKey(x => x.StudentTaskId);

            b.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.PerformedById);

            b.Property(x => x.NewValues).IsRequired();

            b.Property(x => x.Reason).HasMaxLength(2000);
        });

        modelBuilder.Entity<EvaluationNote>(b =>
        {
            b.HasOne<ProgramParticipation>()
                .WithMany()
                .HasForeignKey(x => x.ParticipationId);

            b.HasOne<StudentTask>()
                .WithMany()
                .HasForeignKey(x => x.StudentTaskId);

            b.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.AuthorId);

            b.HasOne<EvaluationNote>()
                .WithMany()
                .HasForeignKey(x => x.PreviousNoteId);

            b.Property(x => x.Text)
                .HasMaxLength(4000)
                .IsRequired();

            b.HasIndex(x => x.PreviousNoteId)
                .IsUnique()
                .HasFilter("[PreviousNoteId] IS NOT NULL");
        });

        modelBuilder.Entity<AbsenceReport>(b =>
        {
            b.HasOne<ProgramParticipation>()
                .WithMany()
                .HasForeignKey(x => x.ParticipationId);

            b.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.ReportedById);

            b.Property(x => x.Description)
                .HasMaxLength(4000)
                .IsRequired();

            b.ToTable("AbsenceReports", t =>
                t.HasCheckConstraint(
                    "CK_AbsenceReports_Status",
                    "[Status] IN (0, 1, 2, 3)"));
        });

        modelBuilder.Entity<AbsenceReportSession>(b =>
        {
            b.HasOne<AbsenceReport>()
                .WithMany()
                .HasForeignKey(x => x.AbsenceReportId);

            b.HasOne<WorkSession>()
                .WithMany()
                .HasForeignKey(x => x.WorkSessionId);

            b.HasIndex(x => new
            {
                x.AbsenceReportId,
                x.WorkSessionId
            }).IsUnique();
        });

        modelBuilder.Entity<ReportAction>(b =>
        {
            b.HasOne<AbsenceReport>()
                .WithMany()
                .HasForeignKey(x => x.AbsenceReportId);

            b.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.PerformedById);

            b.Property(x => x.Action)
                .HasMaxLength(50)
                .IsRequired();

            b.Property(x => x.Description)
                .HasMaxLength(2000)
                .IsRequired();
        });

        modelBuilder.Entity<AdministrativeDecision>(b =>
        {
            b.HasOne<ProgramParticipation>()
                .WithMany()
                .HasForeignKey(x => x.ParticipationId);

            b.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.DecidedById);

            b.HasOne<AdministrativeDecision>()
                .WithMany()
                .HasForeignKey(x => x.PreviousDecisionId);

            b.Property(x => x.DecisionType)
                .HasMaxLength(100)
                .IsRequired();

            b.Property(x => x.Reason)
                .HasMaxLength(4000)
                .IsRequired();
        });
    }

    private static void ConfigureDocuments(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LeaveDocument>(b =>
        {
            b.HasOne<LeaveRequest>()
                .WithMany()
                .HasForeignKey(x => x.LeaveRequestId);

            b.HasOne<StoredFile>()
                .WithMany()
                .HasForeignKey(x => x.StoredFileId);

            b.HasIndex(x => new
            {
                x.LeaveRequestId,
                x.StoredFileId
            }).IsUnique();
        });

        modelBuilder.Entity<DecisionDocument>(b =>
        {
            b.HasOne<AdministrativeDecision>()
                .WithMany()
                .HasForeignKey(x => x.AdministrativeDecisionId);

            b.HasOne<StoredFile>()
                .WithMany()
                .HasForeignKey(x => x.StoredFileId);

            b.HasIndex(x => new
            {
                x.AdministrativeDecisionId,
                x.StoredFileId
            }).IsUnique();
        });

        modelBuilder.Entity<AuditEntry>(b =>
        {
            b.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.UserId);

            b.Property(x => x.Action)
                .HasMaxLength(100)
                .IsRequired();

            b.Property(x => x.EntityType)
                .HasMaxLength(100)
                .IsRequired();

            b.Property(x => x.Summary)
                .HasMaxLength(2000)
                .IsRequired();

            b.Property(x => x.CorrelationId)
                .HasMaxLength(100)
                .IsRequired();

            b.HasIndex(x => new
            {
                x.EntityType,
                x.RecordId,
                x.CreatedAt
            });
        });
    }
}