using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Iskur.Api.Migrations
{
    /// <inheritdoc />
    public partial class CompleteDatabaseModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AbsenceReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParticipationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbsenceReports", x => x.Id);
                    table.CheckConstraint("CK_AbsenceReports_Status", "[Status] IN (0, 1, 2, 3)");
                    table.ForeignKey(
                        name: "FK_AbsenceReports_AspNetUsers_ReportedById",
                        column: x => x.ReportedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AbsenceReports_ProgramParticipations_ParticipationId",
                        column: x => x.ParticipationId,
                        principalTable: "ProgramParticipations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AcademicTerms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StartsOn = table.Column<DateOnly>(type: "date", nullable: false),
                    EndsOn = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicTerms", x => x.Id);
                    table.CheckConstraint("CK_AcademicTerms_Dates", "[EndsOn] > [StartsOn]");
                });

            migrationBuilder.CreateTable(
                name: "AdministrativeDecisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParticipationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DecisionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    DecidedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DecisionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PreviousDecisionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdministrativeDecisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdministrativeDecisions_AdministrativeDecisions_PreviousDecisionId",
                        column: x => x.PreviousDecisionId,
                        principalTable: "AdministrativeDecisions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AdministrativeDecisions_AspNetUsers_DecidedById",
                        column: x => x.DecidedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AdministrativeDecisions_ProgramParticipations_ParticipationId",
                        column: x => x.ParticipationId,
                        principalTable: "ProgramParticipations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AuditEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    RestrictedDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditEntries_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CalendarHolidays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    StartsAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EndsAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarHolidays", x => x.Id);
                    table.CheckConstraint("CK_CalendarHolidays_Dates", "[EndsAt] > [StartsAt]");
                });

            migrationBuilder.CreateTable(
                name: "LeaveTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RequiresDocument = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MonthlyWorkPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParticipationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlyWorkPlans", x => x.Id);
                    table.CheckConstraint("CK_MonthlyWorkPlans_Month", "[Month] BETWEEN 1 AND 12");
                    table.CheckConstraint("CK_MonthlyWorkPlans_Status", "[Status] IN (0, 1)");
                    table.CheckConstraint("CK_MonthlyWorkPlans_Year", "[Year] BETWEEN 1900 AND 2200");
                    table.ForeignKey(
                        name: "FK_MonthlyWorkPlans_ProgramParticipations_ParticipationId",
                        column: x => x.ParticipationId,
                        principalTable: "ProgramParticipations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PlanApplicationWindows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProgramPeriodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    OpensAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ClosesAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanApplicationWindows", x => x.Id);
                    table.CheckConstraint("CK_PlanApplicationWindows_Dates", "[ClosesAt] > [OpensAt]");
                    table.CheckConstraint("CK_PlanApplicationWindows_Month", "[Month] BETWEEN 1 AND 12");
                    table.CheckConstraint("CK_PlanApplicationWindows_Year", "[Year] BETWEEN 1900 AND 2200");
                    table.ForeignKey(
                        name: "FK_PlanApplicationWindows_ProgramPeriods_ProgramPeriodId",
                        column: x => x.ProgramPeriodId,
                        principalTable: "ProgramPeriods",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "QrSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionType = table.Column<int>(type: "int", nullable: false),
                    StartsAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EndsAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ScanDeadline = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CancelledAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QrSessions", x => x.Id);
                    table.CheckConstraint("CK_QrSessions_Deadline", "[ScanDeadline] > [StartsAt] AND [ScanDeadline] <= [EndsAt]");
                    table.CheckConstraint("CK_QrSessions_Times", "[EndsAt] > [StartsAt]");
                    table.CheckConstraint("CK_QrSessions_Type", "[SessionType] IN (0, 1)");
                    table.ForeignKey(
                        name: "FK_QrSessions_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QrSessions_WorkUnits_WorkUnitId",
                        column: x => x.WorkUnitId,
                        principalTable: "WorkUnits",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StoredFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OriginalName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StorageKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    UploadedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoredFiles", x => x.Id);
                    table.CheckConstraint("CK_StoredFiles_Size", "[SizeBytes] > 0 AND [SizeBytes] <= 10485760");
                    table.ForeignKey(
                        name: "FK_StoredFiles_AspNetUsers_UploadedById",
                        column: x => x.UploadedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StudentTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParticipationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    AssignedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DueAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentTasks", x => x.Id);
                    table.CheckConstraint("CK_StudentTasks_Status", "[Status] IN (0, 1)");
                    table.ForeignKey(
                        name: "FK_StudentTasks_AspNetUsers_AssignedById",
                        column: x => x.AssignedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentTasks_ProgramParticipations_ParticipationId",
                        column: x => x.ParticipationId,
                        principalTable: "ProgramParticipations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WorkRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProgramPeriodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ValidUntil = table.Column<DateOnly>(type: "date", nullable: true),
                    MorningStartsAt = table.Column<TimeOnly>(type: "time", nullable: false),
                    MorningEndsAt = table.Column<TimeOnly>(type: "time", nullable: false),
                    AfternoonStartsAt = table.Column<TimeOnly>(type: "time", nullable: false),
                    AfternoonEndsAt = table.Column<TimeOnly>(type: "time", nullable: false),
                    WeeklyMinimum = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: false),
                    WeeklyMaximum = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: false),
                    HalfDayWeeklyWeight = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: true),
                    WeekendWorkAllowed = table.Column<bool>(type: "bit", nullable: true),
                    QrWindowMinutes = table.Column<int>(type: "int", nullable: false),
                    QrRefreshSeconds = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkRules", x => x.Id);
                    table.CheckConstraint("CK_WorkRules_Dates", "[ValidUntil] IS NULL OR [ValidUntil] > [ValidFrom]");
                    table.CheckConstraint("CK_WorkRules_HalfDayWeight", "[HalfDayWeeklyWeight] IS NULL\r\nOR ([HalfDayWeeklyWeight] > 0\r\n    AND [HalfDayWeeklyWeight] <= 1)");
                    table.CheckConstraint("CK_WorkRules_QrTimes", "[QrWindowMinutes] > 0 AND [QrRefreshSeconds] > 0");
                    table.CheckConstraint("CK_WorkRules_Times", "[MorningStartsAt] < [MorningEndsAt]\r\nAND [MorningEndsAt] <= [AfternoonStartsAt]\r\nAND [AfternoonStartsAt] < [AfternoonEndsAt]");
                    table.CheckConstraint("CK_WorkRules_WeeklyLimits", "[WeeklyMinimum] >= 0 AND [WeeklyMaximum] >= [WeeklyMinimum]");
                    table.ForeignKey(
                        name: "FK_WorkRules_ProgramPeriods_ProgramPeriodId",
                        column: x => x.ProgramPeriodId,
                        principalTable: "ProgramPeriods",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReportActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AbsenceReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    PerformedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportActions_AbsenceReports_AbsenceReportId",
                        column: x => x.AbsenceReportId,
                        principalTable: "AbsenceReports",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReportActions_AspNetUsers_PerformedById",
                        column: x => x.PerformedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LeavePolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProgramPeriodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeaveTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ValidUntil = table.Column<DateOnly>(type: "date", nullable: true),
                    EntitlementDays = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: true),
                    EntitlementScope = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeductsEntitlement = table.Column<bool>(type: "bit", nullable: true),
                    HalfDayDeduction = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: true),
                    WarningThresholdDays = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeavePolicies", x => x.Id);
                    table.CheckConstraint("CK_LeavePolicies_Dates", "[ValidUntil] IS NULL OR [ValidUntil] > [ValidFrom]");
                    table.CheckConstraint("CK_LeavePolicies_Entitlement", "[EntitlementDays] IS NULL OR [EntitlementDays] >= 0");
                    table.CheckConstraint("CK_LeavePolicies_HalfDay", "[HalfDayDeduction] IS NULL OR\r\n([HalfDayDeduction] >= 0 AND [HalfDayDeduction] <= 1)");
                    table.CheckConstraint("CK_LeavePolicies_Warning", "[WarningThresholdDays] IS NULL OR [WarningThresholdDays] >= 0");
                    table.ForeignKey(
                        name: "FK_LeavePolicies_LeaveTypes_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalTable: "LeaveTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LeavePolicies_ProgramPeriods_ProgramPeriodId",
                        column: x => x.ProgramPeriodId,
                        principalTable: "ProgramPeriods",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LeaveRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParticipationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeaveTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartsOn = table.Column<DateOnly>(type: "date", nullable: false),
                    EndsOn = table.Column<DateOnly>(type: "date", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DecidedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DecidedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DecisionReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveRequests", x => x.Id);
                    table.CheckConstraint("CK_LeaveRequests_Dates", "[EndsOn] >= [StartsOn]");
                    table.CheckConstraint("CK_LeaveRequests_Status", "[Status] IN (0, 1, 2, 3)");
                    table.ForeignKey(
                        name: "FK_LeaveRequests_AspNetUsers_DecidedById",
                        column: x => x.DecidedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LeaveRequests_LeaveTypes_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalTable: "LeaveTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LeaveRequests_ProgramParticipations_ParticipationId",
                        column: x => x.ParticipationId,
                        principalTable: "ProgramParticipations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WorkPlanDays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MonthlyWorkPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkDate = table.Column<DateOnly>(type: "date", nullable: false),
                    WorkType = table.Column<int>(type: "int", nullable: false),
                    Explanation = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Decision = table.Column<int>(type: "int", nullable: false),
                    DecidedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DecidedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DecisionReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkPlanDays", x => x.Id);
                    table.CheckConstraint("CK_WorkPlanDays_Decision", "[Decision] IN (0, 1, 2, 3)");
                    table.CheckConstraint("CK_WorkPlanDays_Version", "[VersionNumber] > 0");
                    table.CheckConstraint("CK_WorkPlanDays_WorkType", "[WorkType] IN (0, 1, 2)");
                    table.ForeignKey(
                        name: "FK_WorkPlanDays_AspNetUsers_DecidedById",
                        column: x => x.DecidedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkPlanDays_MonthlyWorkPlans_MonthlyWorkPlanId",
                        column: x => x.MonthlyWorkPlanId,
                        principalTable: "MonthlyWorkPlans",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "QrScans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QrSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParticipationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScannedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TokenDigest = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QrScans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QrScans_ProgramParticipations_ParticipationId",
                        column: x => x.ParticipationId,
                        principalTable: "ProgramParticipations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QrScans_QrSessions_QrSessionId",
                        column: x => x.QrSessionId,
                        principalTable: "QrSessions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CourseSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicTermId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    SourceFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ValidUntil = table.Column<DateOnly>(type: "date", nullable: true),
                    ConfirmedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseSchedules", x => x.Id);
                    table.CheckConstraint("CK_CourseSchedules_Dates", "[ValidUntil] IS NULL OR [ValidUntil] > [ValidFrom]");
                    table.CheckConstraint("CK_CourseSchedules_Status", "[Status] IN (0, 1)");
                    table.CheckConstraint("CK_CourseSchedules_Version", "[VersionNumber] > 0");
                    table.ForeignKey(
                        name: "FK_CourseSchedules_AcademicTerms_AcademicTermId",
                        column: x => x.AcademicTermId,
                        principalTable: "AcademicTerms",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CourseSchedules_StoredFiles_SourceFileId",
                        column: x => x.SourceFileId,
                        principalTable: "StoredFiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CourseSchedules_StudentProfiles_StudentId",
                        column: x => x.StudentId,
                        principalTable: "StudentProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DecisionDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdministrativeDecisionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoredFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DecisionDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DecisionDocuments_AdministrativeDecisions_AdministrativeDecisionId",
                        column: x => x.AdministrativeDecisionId,
                        principalTable: "AdministrativeDecisions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DecisionDocuments_StoredFiles_StoredFileId",
                        column: x => x.StoredFileId,
                        principalTable: "StoredFiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EvaluationNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParticipationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentTaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AuthorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    PreviousNoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvaluationNotes_AspNetUsers_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EvaluationNotes_EvaluationNotes_PreviousNoteId",
                        column: x => x.PreviousNoteId,
                        principalTable: "EvaluationNotes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EvaluationNotes_ProgramParticipations_ParticipationId",
                        column: x => x.ParticipationId,
                        principalTable: "ProgramParticipations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EvaluationNotes_StudentTasks_StudentTaskId",
                        column: x => x.StudentTaskId,
                        principalTable: "StudentTasks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TaskActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentTaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PreviousValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PerformedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskActions_AspNetUsers_PerformedById",
                        column: x => x.PerformedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaskActions_StudentTasks_StudentTaskId",
                        column: x => x.StudentTaskId,
                        principalTable: "StudentTasks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LeaveDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeaveRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoredFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaveDocuments_LeaveRequests_LeaveRequestId",
                        column: x => x.LeaveRequestId,
                        principalTable: "LeaveRequests",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LeaveDocuments_StoredFiles_StoredFileId",
                        column: x => x.StoredFileId,
                        principalTable: "StoredFiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LeaveMovements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParticipationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeaveRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LeavePolicyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MovementType = table.Column<int>(type: "int", nullable: false),
                    DayAmount = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: false),
                    PerformedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveMovements", x => x.Id);
                    table.CheckConstraint("CK_LeaveMovements_Amount", "[DayAmount] <> 0");
                    table.CheckConstraint("CK_LeaveMovements_Type", "[MovementType] IN (0, 1, 2, 3)");
                    table.ForeignKey(
                        name: "FK_LeaveMovements_AspNetUsers_PerformedById",
                        column: x => x.PerformedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LeaveMovements_LeavePolicies_LeavePolicyId",
                        column: x => x.LeavePolicyId,
                        principalTable: "LeavePolicies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LeaveMovements_LeaveRequests_LeaveRequestId",
                        column: x => x.LeaveRequestId,
                        principalTable: "LeaveRequests",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LeaveMovements_ProgramParticipations_ParticipationId",
                        column: x => x.ParticipationId,
                        principalTable: "ProgramParticipations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LeaveRequestActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeaveRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PreviousValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PerformedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveRequestActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaveRequestActions_AspNetUsers_PerformedById",
                        column: x => x.PerformedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LeaveRequestActions_LeaveRequests_LeaveRequestId",
                        column: x => x.LeaveRequestId,
                        principalTable: "LeaveRequests",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PlanDayActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkPlanDayId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PreviousValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PerformedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanDayActions", x => x.Id);
                    table.CheckConstraint("CK_PlanDayActions_Version", "[VersionNumber] > 0");
                    table.ForeignKey(
                        name: "FK_PlanDayActions_AspNetUsers_PerformedById",
                        column: x => x.PerformedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PlanDayActions_WorkPlanDays_WorkPlanDayId",
                        column: x => x.WorkPlanDayId,
                        principalTable: "WorkPlanDays",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CourseMeetings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseScheduleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Weekday = table.Column<int>(type: "int", nullable: false),
                    StartsAt = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndsAt = table.Column<TimeOnly>(type: "time", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseMeetings", x => x.Id);
                    table.CheckConstraint("CK_CourseMeetings_Times", "[EndsAt] > [StartsAt]");
                    table.CheckConstraint("CK_CourseMeetings_Weekday", "[Weekday] BETWEEN 0 AND 6");
                    table.ForeignKey(
                        name: "FK_CourseMeetings_CourseSchedules_CourseScheduleId",
                        column: x => x.CourseScheduleId,
                        principalTable: "CourseSchedules",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WorkSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkPlanDayId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionType = table.Column<int>(type: "int", nullable: false),
                    StartsAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EndsAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    PlannedMinutes = table.Column<int>(type: "int", nullable: false),
                    UnitAssignmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdvisorAssignmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkRuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseScheduleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HasCourseConflict = table.Column<bool>(type: "bit", nullable: false),
                    ConflictDetails = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkSessions", x => x.Id);
                    table.CheckConstraint("CK_WorkSessions_Minutes", "[PlannedMinutes] > 0");
                    table.CheckConstraint("CK_WorkSessions_Times", "[EndsAt] > [StartsAt]");
                    table.CheckConstraint("CK_WorkSessions_Type", "[SessionType] IN (0, 1)");
                    table.ForeignKey(
                        name: "FK_WorkSessions_AdvisorAssignments_AdvisorAssignmentId",
                        column: x => x.AdvisorAssignmentId,
                        principalTable: "AdvisorAssignments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkSessions_CourseSchedules_CourseScheduleId",
                        column: x => x.CourseScheduleId,
                        principalTable: "CourseSchedules",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkSessions_UnitAssignments_UnitAssignmentId",
                        column: x => x.UnitAssignmentId,
                        principalTable: "UnitAssignments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkSessions_WorkPlanDays_WorkPlanDayId",
                        column: x => x.WorkPlanDayId,
                        principalTable: "WorkPlanDays",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkSessions_WorkRules_WorkRuleId",
                        column: x => x.WorkRuleId,
                        principalTable: "WorkRules",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AbsenceReportSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AbsenceReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbsenceReportSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AbsenceReportSessions_AbsenceReports_AbsenceReportId",
                        column: x => x.AbsenceReportId,
                        principalTable: "AbsenceReports",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AbsenceReportSessions_WorkSessions_WorkSessionId",
                        column: x => x.WorkSessionId,
                        principalTable: "WorkSessions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LeaveRequestSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeaveRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeavePolicyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LeaveMinutes = table.Column<int>(type: "int", nullable: false),
                    DeductionDays = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveRequestSessions", x => x.Id);
                    table.CheckConstraint("CK_LeaveRequestSessions_Deduction", "[DeductionDays] IS NULL OR [DeductionDays] >= 0");
                    table.CheckConstraint("CK_LeaveRequestSessions_Minutes", "[LeaveMinutes] > 0");
                    table.ForeignKey(
                        name: "FK_LeaveRequestSessions_LeavePolicies_LeavePolicyId",
                        column: x => x.LeavePolicyId,
                        principalTable: "LeavePolicies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LeaveRequestSessions_LeaveRequests_LeaveRequestId",
                        column: x => x.LeaveRequestId,
                        principalTable: "LeaveRequests",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LeaveRequestSessions_WorkSessions_WorkSessionId",
                        column: x => x.WorkSessionId,
                        principalTable: "WorkSessions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SessionAttendances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Source = table.Column<int>(type: "int", nullable: false),
                    QrScanId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LeaveRequestSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AcceptedMinutes = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionAttendances", x => x.Id);
                    table.CheckConstraint("CK_SessionAttendances_Minutes", "[AcceptedMinutes] >= 0");
                    table.CheckConstraint("CK_SessionAttendances_SingleSource", "NOT ([QrScanId] IS NOT NULL AND [LeaveRequestSessionId] IS NOT NULL)");
                    table.CheckConstraint("CK_SessionAttendances_Source", "[Source] IN (0, 1, 2, 3, 4)");
                    table.CheckConstraint("CK_SessionAttendances_Status", "[Status] IN (0, 1, 2, 3)");
                    table.ForeignKey(
                        name: "FK_SessionAttendances_LeaveRequestSessions_LeaveRequestSessionId",
                        column: x => x.LeaveRequestSessionId,
                        principalTable: "LeaveRequestSessions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SessionAttendances_QrScans_QrScanId",
                        column: x => x.QrScanId,
                        principalTable: "QrScans",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SessionAttendances_WorkSessions_WorkSessionId",
                        column: x => x.WorkSessionId,
                        principalTable: "WorkSessions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AttendanceCorrections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionAttendanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PreviousStatus = table.Column<int>(type: "int", nullable: false),
                    NewStatus = table.Column<int>(type: "int", nullable: false),
                    PreviousMinutes = table.Column<int>(type: "int", nullable: false),
                    NewMinutes = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    PerformedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceCorrections", x => x.Id);
                    table.CheckConstraint("CK_AttendanceCorrections_Minutes", "[PreviousMinutes] >= 0 AND [NewMinutes] >= 0");
                    table.CheckConstraint("CK_AttendanceCorrections_NewStatus", "[NewStatus] IN (0, 1, 2, 3)");
                    table.CheckConstraint("CK_AttendanceCorrections_PreviousStatus", "[PreviousStatus] IN (0, 1, 2, 3)");
                    table.ForeignKey(
                        name: "FK_AttendanceCorrections_AspNetUsers_PerformedById",
                        column: x => x.PerformedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AttendanceCorrections_SessionAttendances_SessionAttendanceId",
                        column: x => x.SessionAttendanceId,
                        principalTable: "SessionAttendances",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AbsenceReports_ParticipationId",
                table: "AbsenceReports",
                column: "ParticipationId");

            migrationBuilder.CreateIndex(
                name: "IX_AbsenceReports_ReportedById",
                table: "AbsenceReports",
                column: "ReportedById");

            migrationBuilder.CreateIndex(
                name: "IX_AbsenceReportSessions_AbsenceReportId_WorkSessionId",
                table: "AbsenceReportSessions",
                columns: new[] { "AbsenceReportId", "WorkSessionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AbsenceReportSessions_WorkSessionId",
                table: "AbsenceReportSessions",
                column: "WorkSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_AdministrativeDecisions_DecidedById",
                table: "AdministrativeDecisions",
                column: "DecidedById");

            migrationBuilder.CreateIndex(
                name: "IX_AdministrativeDecisions_ParticipationId",
                table: "AdministrativeDecisions",
                column: "ParticipationId");

            migrationBuilder.CreateIndex(
                name: "IX_AdministrativeDecisions_PreviousDecisionId",
                table: "AdministrativeDecisions",
                column: "PreviousDecisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceCorrections_PerformedById",
                table: "AttendanceCorrections",
                column: "PerformedById");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceCorrections_SessionAttendanceId",
                table: "AttendanceCorrections",
                column: "SessionAttendanceId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntries_EntityType_RecordId_CreatedAt",
                table: "AuditEntries",
                columns: new[] { "EntityType", "RecordId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntries_UserId",
                table: "AuditEntries",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseMeetings_CourseScheduleId",
                table: "CourseMeetings",
                column: "CourseScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseSchedules_AcademicTermId",
                table: "CourseSchedules",
                column: "AcademicTermId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseSchedules_SourceFileId",
                table: "CourseSchedules",
                column: "SourceFileId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseSchedules_StudentId_AcademicTermId_VersionNumber",
                table: "CourseSchedules",
                columns: new[] { "StudentId", "AcademicTermId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DecisionDocuments_AdministrativeDecisionId_StoredFileId",
                table: "DecisionDocuments",
                columns: new[] { "AdministrativeDecisionId", "StoredFileId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DecisionDocuments_StoredFileId",
                table: "DecisionDocuments",
                column: "StoredFileId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationNotes_AuthorId",
                table: "EvaluationNotes",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationNotes_ParticipationId",
                table: "EvaluationNotes",
                column: "ParticipationId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationNotes_PreviousNoteId",
                table: "EvaluationNotes",
                column: "PreviousNoteId",
                unique: true,
                filter: "[PreviousNoteId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationNotes_StudentTaskId",
                table: "EvaluationNotes",
                column: "StudentTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveDocuments_LeaveRequestId_StoredFileId",
                table: "LeaveDocuments",
                columns: new[] { "LeaveRequestId", "StoredFileId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveDocuments_StoredFileId",
                table: "LeaveDocuments",
                column: "StoredFileId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveMovements_LeavePolicyId",
                table: "LeaveMovements",
                column: "LeavePolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveMovements_LeaveRequestId",
                table: "LeaveMovements",
                column: "LeaveRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveMovements_ParticipationId",
                table: "LeaveMovements",
                column: "ParticipationId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveMovements_PerformedById",
                table: "LeaveMovements",
                column: "PerformedById");

            migrationBuilder.CreateIndex(
                name: "IX_LeavePolicies_LeaveTypeId",
                table: "LeavePolicies",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LeavePolicies_ProgramPeriodId",
                table: "LeavePolicies",
                column: "ProgramPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequestActions_LeaveRequestId",
                table: "LeaveRequestActions",
                column: "LeaveRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequestActions_PerformedById",
                table: "LeaveRequestActions",
                column: "PerformedById");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_DecidedById",
                table: "LeaveRequests",
                column: "DecidedById");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_LeaveTypeId",
                table: "LeaveRequests",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_ParticipationId_StartsOn_EndsOn",
                table: "LeaveRequests",
                columns: new[] { "ParticipationId", "StartsOn", "EndsOn" });

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequestSessions_LeavePolicyId",
                table: "LeaveRequestSessions",
                column: "LeavePolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequestSessions_LeaveRequestId_WorkSessionId",
                table: "LeaveRequestSessions",
                columns: new[] { "LeaveRequestId", "WorkSessionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequestSessions_WorkSessionId",
                table: "LeaveRequestSessions",
                column: "WorkSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveTypes_Name",
                table: "LeaveTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyWorkPlans_ParticipationId_Year_Month",
                table: "MonthlyWorkPlans",
                columns: new[] { "ParticipationId", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanApplicationWindows_ProgramPeriodId_Year_Month",
                table: "PlanApplicationWindows",
                columns: new[] { "ProgramPeriodId", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanDayActions_PerformedById",
                table: "PlanDayActions",
                column: "PerformedById");

            migrationBuilder.CreateIndex(
                name: "IX_PlanDayActions_WorkPlanDayId",
                table: "PlanDayActions",
                column: "WorkPlanDayId");

            migrationBuilder.CreateIndex(
                name: "IX_QrScans_ParticipationId",
                table: "QrScans",
                column: "ParticipationId");

            migrationBuilder.CreateIndex(
                name: "IX_QrScans_QrSessionId_ParticipationId",
                table: "QrScans",
                columns: new[] { "QrSessionId", "ParticipationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QrSessions_CreatedById",
                table: "QrSessions",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_QrSessions_WorkUnitId_StartsAt_SessionType",
                table: "QrSessions",
                columns: new[] { "WorkUnitId", "StartsAt", "SessionType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportActions_AbsenceReportId",
                table: "ReportActions",
                column: "AbsenceReportId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportActions_PerformedById",
                table: "ReportActions",
                column: "PerformedById");

            migrationBuilder.CreateIndex(
                name: "IX_SessionAttendances_LeaveRequestSessionId",
                table: "SessionAttendances",
                column: "LeaveRequestSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionAttendances_QrScanId",
                table: "SessionAttendances",
                column: "QrScanId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionAttendances_WorkSessionId",
                table: "SessionAttendances",
                column: "WorkSessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoredFiles_StorageKey",
                table: "StoredFiles",
                column: "StorageKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoredFiles_UploadedById",
                table: "StoredFiles",
                column: "UploadedById");

            migrationBuilder.CreateIndex(
                name: "IX_StudentTasks_AssignedById",
                table: "StudentTasks",
                column: "AssignedById");

            migrationBuilder.CreateIndex(
                name: "IX_StudentTasks_ParticipationId",
                table: "StudentTasks",
                column: "ParticipationId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskActions_PerformedById",
                table: "TaskActions",
                column: "PerformedById");

            migrationBuilder.CreateIndex(
                name: "IX_TaskActions_StudentTaskId",
                table: "TaskActions",
                column: "StudentTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkPlanDays_DecidedById",
                table: "WorkPlanDays",
                column: "DecidedById");

            migrationBuilder.CreateIndex(
                name: "IX_WorkPlanDays_MonthlyWorkPlanId_WorkDate",
                table: "WorkPlanDays",
                columns: new[] { "MonthlyWorkPlanId", "WorkDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkPlanDays_WorkDate",
                table: "WorkPlanDays",
                column: "WorkDate");

            migrationBuilder.CreateIndex(
                name: "IX_WorkRules_ProgramPeriodId",
                table: "WorkRules",
                column: "ProgramPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkSessions_AdvisorAssignmentId",
                table: "WorkSessions",
                column: "AdvisorAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkSessions_CourseScheduleId",
                table: "WorkSessions",
                column: "CourseScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkSessions_StartsAt",
                table: "WorkSessions",
                column: "StartsAt");

            migrationBuilder.CreateIndex(
                name: "IX_WorkSessions_UnitAssignmentId",
                table: "WorkSessions",
                column: "UnitAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkSessions_WorkPlanDayId_SessionType",
                table: "WorkSessions",
                columns: new[] { "WorkPlanDayId", "SessionType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkSessions_WorkRuleId",
                table: "WorkSessions",
                column: "WorkRuleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AbsenceReportSessions");

            migrationBuilder.DropTable(
                name: "AttendanceCorrections");

            migrationBuilder.DropTable(
                name: "AuditEntries");

            migrationBuilder.DropTable(
                name: "CalendarHolidays");

            migrationBuilder.DropTable(
                name: "CourseMeetings");

            migrationBuilder.DropTable(
                name: "DecisionDocuments");

            migrationBuilder.DropTable(
                name: "EvaluationNotes");

            migrationBuilder.DropTable(
                name: "LeaveDocuments");

            migrationBuilder.DropTable(
                name: "LeaveMovements");

            migrationBuilder.DropTable(
                name: "LeaveRequestActions");

            migrationBuilder.DropTable(
                name: "PlanApplicationWindows");

            migrationBuilder.DropTable(
                name: "PlanDayActions");

            migrationBuilder.DropTable(
                name: "ReportActions");

            migrationBuilder.DropTable(
                name: "TaskActions");

            migrationBuilder.DropTable(
                name: "SessionAttendances");

            migrationBuilder.DropTable(
                name: "AdministrativeDecisions");

            migrationBuilder.DropTable(
                name: "AbsenceReports");

            migrationBuilder.DropTable(
                name: "StudentTasks");

            migrationBuilder.DropTable(
                name: "LeaveRequestSessions");

            migrationBuilder.DropTable(
                name: "QrScans");

            migrationBuilder.DropTable(
                name: "LeavePolicies");

            migrationBuilder.DropTable(
                name: "LeaveRequests");

            migrationBuilder.DropTable(
                name: "WorkSessions");

            migrationBuilder.DropTable(
                name: "QrSessions");

            migrationBuilder.DropTable(
                name: "LeaveTypes");

            migrationBuilder.DropTable(
                name: "CourseSchedules");

            migrationBuilder.DropTable(
                name: "WorkPlanDays");

            migrationBuilder.DropTable(
                name: "WorkRules");

            migrationBuilder.DropTable(
                name: "AcademicTerms");

            migrationBuilder.DropTable(
                name: "StoredFiles");

            migrationBuilder.DropTable(
                name: "MonthlyWorkPlans");
        }
    }
}
