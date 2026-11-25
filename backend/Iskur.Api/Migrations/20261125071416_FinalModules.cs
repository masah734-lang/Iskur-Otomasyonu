using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Iskur.Api.Migrations
{
    /// <inheritdoc />
    public partial class FinalModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AbsenceReport",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParticipationId = table.Column<int>(type: "int", nullable: false),
                    PlanDayId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Resolution = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbsenceReport", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AbsenceReport_Participations_ParticipationId",
                        column: x => x.ParticipationId,
                        principalTable: "Participations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AbsenceReport_PlanDays_PlanDayId",
                        column: x => x.PlanDayId,
                        principalTable: "PlanDays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CourseRevision",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParticipationId = table.Column<int>(type: "int", nullable: false),
                    RecordedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Snapshot = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseRevision", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseRevision_Participations_ParticipationId",
                        column: x => x.ParticipationId,
                        principalTable: "Participations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FacultyPolicy",
                columns: table => new
                {
                    FacultyId = table.Column<int>(type: "int", nullable: false),
                    LeaveDays = table.Column<int>(type: "int", nullable: false),
                    LeaveWarningDays = table.Column<int>(type: "int", nullable: false),
                    AbsenceWarningDays = table.Column<int>(type: "int", nullable: false),
                    MedicalLeaveExempt = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacultyPolicy", x => x.FacultyId);
                    table.ForeignKey(
                        name: "FK_FacultyPolicy_Faculties_FacultyId",
                        column: x => x.FacultyId,
                        principalTable: "Faculties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeaveAttachment",
                columns: table => new
                {
                    LeaveRequestId = table.Column<int>(type: "int", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Bytes = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveAttachment", x => x.LeaveRequestId);
                    table.ForeignKey(
                        name: "FK_LeaveAttachment_LeaveRequest_LeaveRequestId",
                        column: x => x.LeaveRequestId,
                        principalTable: "LeaveRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeaveDecision",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeaveRequestId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActorId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecordedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveDecision", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaveDecision_LeaveRequest_LeaveRequestId",
                        column: x => x.LeaveRequestId,
                        principalTable: "LeaveRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeaveSlot",
                columns: table => new
                {
                    LeaveRequestId = table.Column<int>(type: "int", nullable: false),
                    PlanDayId = table.Column<int>(type: "int", nullable: false),
                    Slot = table.Column<int>(type: "int", nullable: false),
                    ChargedDays = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveSlot", x => new { x.LeaveRequestId, x.PlanDayId, x.Slot });
                    table.ForeignKey(
                        name: "FK_LeaveSlot_LeaveRequest_LeaveRequestId",
                        column: x => x.LeaveRequestId,
                        principalTable: "LeaveRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaveSlot_PlanDays_PlanDayId",
                        column: x => x.PlanDayId,
                        principalTable: "PlanDays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlanExamDate",
                columns: table => new
                {
                    MonthlyPlanId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanExamDate", x => new { x.MonthlyPlanId, x.Date });
                    table.ForeignKey(
                        name: "FK_PlanExamDate_Plans_MonthlyPlanId",
                        column: x => x.MonthlyPlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlanRevision",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MonthlyPlanId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActorId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecordedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Snapshot = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanRevision", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanRevision_Plans_MonthlyPlanId",
                        column: x => x.MonthlyPlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlanWindow",
                columns: table => new
                {
                    FacultyId = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Opens = table.Column<DateOnly>(type: "date", nullable: false),
                    Closes = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanWindow", x => new { x.FacultyId, x.Year, x.Month });
                    table.ForeignKey(
                        name: "FK_PlanWindow_Faculties_FacultyId",
                        column: x => x.FacultyId,
                        principalTable: "Faculties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SlotAttendance",
                columns: table => new
                {
                    PlanDayId = table.Column<int>(type: "int", nullable: false),
                    Slot = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActorId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecordedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlotAttendance", x => new { x.PlanDayId, x.Slot });
                    table.ForeignKey(
                        name: "FK_SlotAttendance_PlanDays_PlanDayId",
                        column: x => x.PlanDayId,
                        principalTable: "PlanDays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaskHistory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentTaskId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActorId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Completed = table.Column<bool>(type: "bit", nullable: false),
                    RecordedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskHistory_StudentTask_StudentTaskId",
                        column: x => x.StudentTaskId,
                        principalTable: "StudentTask",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AbsenceReportAction",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AbsenceReportId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActorId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecordedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbsenceReportAction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AbsenceReportAction_AbsenceReport_AbsenceReportId",
                        column: x => x.AbsenceReportId,
                        principalTable: "AbsenceReport",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AbsenceReport_ParticipationId",
                table: "AbsenceReport",
                column: "ParticipationId");

            migrationBuilder.CreateIndex(
                name: "IX_AbsenceReport_PlanDayId",
                table: "AbsenceReport",
                column: "PlanDayId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AbsenceReportAction_AbsenceReportId",
                table: "AbsenceReportAction",
                column: "AbsenceReportId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseRevision_ParticipationId",
                table: "CourseRevision",
                column: "ParticipationId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveDecision_LeaveRequestId",
                table: "LeaveDecision",
                column: "LeaveRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveSlot_PlanDayId",
                table: "LeaveSlot",
                column: "PlanDayId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanRevision_MonthlyPlanId",
                table: "PlanRevision",
                column: "MonthlyPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskHistory_StudentTaskId",
                table: "TaskHistory",
                column: "StudentTaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AbsenceReportAction");

            migrationBuilder.DropTable(
                name: "CourseRevision");

            migrationBuilder.DropTable(
                name: "FacultyPolicy");

            migrationBuilder.DropTable(
                name: "LeaveAttachment");

            migrationBuilder.DropTable(
                name: "LeaveDecision");

            migrationBuilder.DropTable(
                name: "LeaveSlot");

            migrationBuilder.DropTable(
                name: "PlanExamDate");

            migrationBuilder.DropTable(
                name: "PlanRevision");

            migrationBuilder.DropTable(
                name: "PlanWindow");

            migrationBuilder.DropTable(
                name: "SlotAttendance");

            migrationBuilder.DropTable(
                name: "TaskHistory");

            migrationBuilder.DropTable(
                name: "AbsenceReport");
        }
    }
}
