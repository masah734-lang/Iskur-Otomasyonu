using Iskur.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Iskur.Api.Configurations;

public static class CoreModelConfiguration
{
    public static void ConfigureCoreModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationUser>(b =>
        {
            b.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            b.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            b.Property(x => x.RowVersion).IsRowVersion();

            b.HasIndex(x => x.NormalizedEmail)
                .IsUnique()
                .HasFilter("[NormalizedEmail] IS NOT NULL");
        });

        modelBuilder.Entity<Faculty>(b =>
        {
            b.Property(x => x.Name).HasMaxLength(150).IsRequired();
            b.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Department>(b =>
        {
            b.Property(x => x.Name).HasMaxLength(150).IsRequired();

            b.HasOne(x => x.Faculty)
                .WithMany(x => x.Departments)
                .HasForeignKey(x => x.FacultyId);

            b.HasIndex(x => new { x.FacultyId, x.Name }).IsUnique();
        });

        modelBuilder.Entity<WorkUnit>(b =>
        {
            b.Property(x => x.Name).HasMaxLength(150).IsRequired();

            b.HasOne(x => x.Department)
                .WithMany(x => x.WorkUnits)
                .HasForeignKey(x => x.DepartmentId);

            b.HasIndex(x => new { x.DepartmentId, x.Name }).IsUnique();
        });

        modelBuilder.Entity<StudentProfile>(b =>
        {
            b.Property(x => x.NationalId).HasMaxLength(11).IsRequired();
            b.Property(x => x.StudentNumber).HasMaxLength(30).IsRequired();
            b.Property(x => x.Iban).HasMaxLength(34).IsRequired();

            b.HasIndex(x => x.NationalId).IsUnique();
            b.HasIndex(x => x.StudentNumber).IsUnique();

            b.HasOne(x => x.User)
                .WithOne()
                .HasForeignKey<StudentProfile>(x => x.UserId);

            b.HasOne(x => x.EducationDepartment)
                .WithMany()
                .HasForeignKey(x => x.EducationDepartmentId);

            b.ToTable("StudentProfiles", t =>
            {
                t.HasCheckConstraint(
                    "CK_StudentProfiles_NationalId",
                    "LEN([NationalId]) = 11");

                t.HasCheckConstraint(
                    "CK_StudentProfiles_RegistrationYear",
                    "[UniversityRegistrationYear] BETWEEN 1900 AND 2200");
            });
        });

        modelBuilder.Entity<ProgramPeriod>(b =>
        {
            b.Property(x => x.Name).HasMaxLength(150).IsRequired();
            b.Property(x => x.ProgramCode).HasMaxLength(50).IsRequired();

            b.HasIndex(x => x.ProgramCode).IsUnique();

            b.ToTable("ProgramPeriods", t =>
                t.HasCheckConstraint(
                    "CK_ProgramPeriods_Dates",
                    "[EndsOn] > [StartsOn]"));
        });

        modelBuilder.Entity<ProgramParticipation>(b =>
        {
            b.Property(x => x.LeavingReason).HasMaxLength(1000);

            b.HasOne(x => x.Student)
                .WithMany(x => x.Participations)
                .HasForeignKey(x => x.StudentId);

            b.HasOne(x => x.ProgramPeriod)
                .WithMany(x => x.Participations)
                .HasForeignKey(x => x.ProgramPeriodId);

            b.HasIndex(x => new { x.StudentId, x.ProgramPeriodId })
                .IsUnique();

            b.ToTable("ProgramParticipations", t =>
            {
                t.HasCheckConstraint(
                    "CK_ProgramParticipations_Dates",
                    "[EndsOn] IS NULL OR [EndsOn] > [StartsOn]");

                t.HasCheckConstraint(
                    "CK_ProgramParticipations_Status",
                    "[Status] IN (0, 1)");
            });
        });

        modelBuilder.Entity<UnitAssignment>(b =>
        {
            b.Property(x => x.Reason).HasMaxLength(1000);

            b.HasOne(x => x.Participation)
                .WithMany(x => x.UnitAssignments)
                .HasForeignKey(x => x.ParticipationId);

            b.HasOne(x => x.WorkUnit)
                .WithMany()
                .HasForeignKey(x => x.WorkUnitId);

            b.HasOne(x => x.AssignedBy)
                .WithMany()
                .HasForeignKey(x => x.AssignedById);

            b.HasIndex(x => x.ParticipationId)
                .IsUnique()
                .HasFilter("[EndsOn] IS NULL");

            b.HasIndex(x => new
            {
                x.ParticipationId,
                x.StartsOn,
                x.EndsOn
            });

            b.ToTable("UnitAssignments", t =>
                t.HasCheckConstraint(
                    "CK_UnitAssignments_Dates",
                    "[EndsOn] IS NULL OR [EndsOn] > [StartsOn]"));
        });

        modelBuilder.Entity<AdvisorAssignment>(b =>
        {
            b.Property(x => x.Reason).HasMaxLength(1000);

            b.HasOne(x => x.Participation)
                .WithMany(x => x.AdvisorAssignments)
                .HasForeignKey(x => x.ParticipationId);

            b.HasOne(x => x.Advisor)
                .WithMany()
                .HasForeignKey(x => x.AdvisorId);

            b.HasOne(x => x.AssignedBy)
                .WithMany()
                .HasForeignKey(x => x.AssignedById);

            b.HasIndex(x => x.ParticipationId)
                .IsUnique()
                .HasFilter("[EndsOn] IS NULL");

            b.HasIndex(x => new
            {
                x.ParticipationId,
                x.StartsOn,
                x.EndsOn
            });

            b.ToTable("AdvisorAssignments", t =>
                t.HasCheckConstraint(
                    "CK_AdvisorAssignments_Dates",
                    "[EndsOn] IS NULL OR [EndsOn] > [StartsOn]"));
        });

        modelBuilder.Entity<AuthorityAssignment>(b =>
        {
            b.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId);

            b.HasOne(x => x.Role)
                .WithMany()
                .HasForeignKey(x => x.RoleId);

            b.HasOne(x => x.Faculty)
                .WithMany()
                .HasForeignKey(x => x.FacultyId);

            b.HasOne(x => x.Department)
                .WithMany()
                .HasForeignKey(x => x.DepartmentId);

            b.HasOne(x => x.AssignedBy)
                .WithMany()
                .HasForeignKey(x => x.AssignedById);

            b.ToTable("AuthorityAssignments", t =>
            {
                t.HasCheckConstraint(
                    "CK_AuthorityAssignments_Scope",
                    """
                    ([FacultyId] IS NOT NULL AND [DepartmentId] IS NULL)
                    OR
                    ([FacultyId] IS NULL AND [DepartmentId] IS NOT NULL)
                    """);

                t.HasCheckConstraint(
                    "CK_AuthorityAssignments_Dates",
                    "[EndsOn] IS NULL OR [EndsOn] > [StartsOn]");
            });
        });
    }
}