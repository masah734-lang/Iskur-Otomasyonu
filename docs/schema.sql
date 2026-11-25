CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Faculties] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Faculties] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Holidays] (
    [Date] date NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [AfternoonOnly] bit NOT NULL,
    CONSTRAINT [PK_Holidays] PRIMARY KEY ([Date])
);
GO


CREATE TABLE [Logs] (
    [Id] bigint NOT NULL IDENTITY,
    [ActorId] nvarchar(max) NOT NULL,
    [Action] nvarchar(max) NOT NULL,
    [Entity] nvarchar(max) NOT NULL,
    [At] datetimeoffset NOT NULL,
    CONSTRAINT [PK_Logs] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Periods] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [Start] date NOT NULL,
    [End] date NOT NULL,
    CONSTRAINT [PK_Periods] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Departments] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [FacultyId] int NOT NULL,
    CONSTRAINT [PK_Departments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Departments_Faculties_FacultyId] FOREIGN KEY ([FacultyId]) REFERENCES [Faculties] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [FacultyPolicy] (
    [FacultyId] int NOT NULL,
    [LeaveDays] int NOT NULL,
    [LeaveWarningDays] int NOT NULL,
    [AbsenceWarningDays] int NOT NULL,
    [MedicalLeaveExempt] bit NOT NULL,
    CONSTRAINT [PK_FacultyPolicy] PRIMARY KEY ([FacultyId]),
    CONSTRAINT [FK_FacultyPolicy_Faculties_FacultyId] FOREIGN KEY ([FacultyId]) REFERENCES [Faculties] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [PlanWindow] (
    [FacultyId] int NOT NULL,
    [Year] int NOT NULL,
    [Month] int NOT NULL,
    [Opens] date NOT NULL,
    [Closes] date NOT NULL,
    CONSTRAINT [PK_PlanWindow] PRIMARY KEY ([FacultyId], [Year], [Month]),
    CONSTRAINT [FK_PlanWindow_Faculties_FacultyId] FOREIGN KEY ([FacultyId]) REFERENCES [Faculties] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [FullName] nvarchar(max) NOT NULL,
    [FacultyId] int NULL,
    [DepartmentId] int NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUsers_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_AspNetUsers_Faculties_FacultyId] FOREIGN KEY ([FacultyId]) REFERENCES [Faculties] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [WorkUnits] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [FacultyId] int NOT NULL,
    [DepartmentId] int NULL,
    CONSTRAINT [PK_WorkUnits] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_WorkUnits_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_WorkUnits_Faculties_FacultyId] FOREIGN KEY ([FacultyId]) REFERENCES [Faculties] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Students] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [StudentNumber] nvarchar(30) NOT NULL,
    [NationalId] nvarchar(11) NOT NULL,
    [Iban] nvarchar(26) NOT NULL,
    [DepartmentId] int NOT NULL,
    [RegistrationYear] int NOT NULL,
    CONSTRAINT [PK_Students] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Students_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Students_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [AttendanceSession] (
    [Id] int NOT NULL IDENTITY,
    [WorkUnitId] int NOT NULL,
    [OpenAt] datetimeoffset NOT NULL,
    [CloseAt] datetimeoffset NOT NULL,
    [NonceHash] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_AttendanceSession] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AttendanceSession_WorkUnits_WorkUnitId] FOREIGN KEY ([WorkUnitId]) REFERENCES [WorkUnits] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Participations] (
    [Id] int NOT NULL IDENTITY,
    [StudentProfileId] int NOT NULL,
    [ProgramPeriodId] int NOT NULL,
    [Active] bit NOT NULL,
    [Start] date NOT NULL,
    [End] date NOT NULL,
    CONSTRAINT [PK_Participations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Participations_Periods_ProgramPeriodId] FOREIGN KEY ([ProgramPeriodId]) REFERENCES [Periods] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Participations_Students_StudentProfileId] FOREIGN KEY ([StudentProfileId]) REFERENCES [Students] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Assignments] (
    [Id] int NOT NULL IDENTITY,
    [ParticipationId] int NOT NULL,
    [WorkUnitId] int NOT NULL,
    [AdvisorId] nvarchar(450) NULL,
    [Start] datetimeoffset NOT NULL,
    [End] datetimeoffset NULL,
    CONSTRAINT [PK_Assignments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Assignments_AspNetUsers_AdvisorId] FOREIGN KEY ([AdvisorId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Assignments_Participations_ParticipationId] FOREIGN KEY ([ParticipationId]) REFERENCES [Participations] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Assignments_WorkUnits_WorkUnitId] FOREIGN KEY ([WorkUnitId]) REFERENCES [WorkUnits] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [CourseRevision] (
    [Id] bigint NOT NULL IDENTITY,
    [ParticipationId] int NOT NULL,
    [RecordedAt] datetimeoffset NOT NULL,
    [Snapshot] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_CourseRevision] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CourseRevision_Participations_ParticipationId] FOREIGN KEY ([ParticipationId]) REFERENCES [Participations] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Courses] (
    [Id] int NOT NULL IDENTITY,
    [ParticipationId] int NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [DayOfWeek] int NOT NULL,
    [StartMinute] int NOT NULL,
    [EndMinute] int NOT NULL,
    CONSTRAINT [PK_Courses] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Courses_Participations_ParticipationId] FOREIGN KEY ([ParticipationId]) REFERENCES [Participations] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [LeaveRequest] (
    [Id] int NOT NULL IDENTITY,
    [ParticipationId] int NOT NULL,
    [Start] date NOT NULL,
    [End] date NOT NULL,
    [Type] nvarchar(max) NOT NULL,
    [Reason] nvarchar(max) NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [Minutes] int NOT NULL,
    [ReviewerId] nvarchar(450) NULL,
    [ReviewedAt] datetimeoffset NULL,
    CONSTRAINT [PK_LeaveRequest] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_LeaveRequest_AspNetUsers_ReviewerId] FOREIGN KEY ([ReviewerId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_LeaveRequest_Participations_ParticipationId] FOREIGN KEY ([ParticipationId]) REFERENCES [Participations] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Plans] (
    [Id] int NOT NULL IDENTITY,
    [ParticipationId] int NOT NULL,
    [Year] int NOT NULL,
    [Month] int NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [Version] rowversion NOT NULL,
    CONSTRAINT [PK_Plans] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Plans_Participations_ParticipationId] FOREIGN KEY ([ParticipationId]) REFERENCES [Participations] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [StudentTask] (
    [Id] int NOT NULL IDENTITY,
    [AssignmentId] int NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [DueDate] date NOT NULL,
    [Completed] bit NOT NULL,
    CONSTRAINT [PK_StudentTask] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_StudentTask_Assignments_AssignmentId] FOREIGN KEY ([AssignmentId]) REFERENCES [Assignments] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [LeaveAttachment] (
    [LeaveRequestId] int NOT NULL,
    [ContentType] nvarchar(max) NOT NULL,
    [Bytes] varbinary(max) NOT NULL,
    CONSTRAINT [PK_LeaveAttachment] PRIMARY KEY ([LeaveRequestId]),
    CONSTRAINT [FK_LeaveAttachment_LeaveRequest_LeaveRequestId] FOREIGN KEY ([LeaveRequestId]) REFERENCES [LeaveRequest] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [LeaveDecision] (
    [Id] bigint NOT NULL IDENTITY,
    [LeaveRequestId] int NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [Note] nvarchar(max) NULL,
    [ActorId] nvarchar(max) NOT NULL,
    [RecordedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_LeaveDecision] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_LeaveDecision_LeaveRequest_LeaveRequestId] FOREIGN KEY ([LeaveRequestId]) REFERENCES [LeaveRequest] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [PlanDays] (
    [Id] int NOT NULL IDENTITY,
    [MonthlyPlanId] int NOT NULL,
    [Date] date NOT NULL,
    [Kind] nvarchar(max) NOT NULL,
    [Minutes] int NOT NULL,
    [Note] nvarchar(max) NOT NULL,
    [Conflict] bit NOT NULL,
    [Decision] nvarchar(max) NOT NULL,
    [Reason] nvarchar(max) NULL,
    [DecidedById] nvarchar(450) NULL,
    [DecidedAt] datetimeoffset NULL,
    CONSTRAINT [PK_PlanDays] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PlanDays_AspNetUsers_DecidedById] FOREIGN KEY ([DecidedById]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PlanDays_Plans_MonthlyPlanId] FOREIGN KEY ([MonthlyPlanId]) REFERENCES [Plans] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [PlanExamDate] (
    [MonthlyPlanId] int NOT NULL,
    [Date] date NOT NULL,
    CONSTRAINT [PK_PlanExamDate] PRIMARY KEY ([MonthlyPlanId], [Date]),
    CONSTRAINT [FK_PlanExamDate_Plans_MonthlyPlanId] FOREIGN KEY ([MonthlyPlanId]) REFERENCES [Plans] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [PlanRevision] (
    [Id] bigint NOT NULL IDENTITY,
    [MonthlyPlanId] int NOT NULL,
    [Action] nvarchar(max) NOT NULL,
    [ActorId] nvarchar(max) NOT NULL,
    [RecordedAt] datetimeoffset NOT NULL,
    [Snapshot] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_PlanRevision] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PlanRevision_Plans_MonthlyPlanId] FOREIGN KEY ([MonthlyPlanId]) REFERENCES [Plans] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Evaluation] (
    [Id] int NOT NULL IDENTITY,
    [StudentTaskId] int NOT NULL,
    [AuthorId] nvarchar(450) NOT NULL,
    [Note] nvarchar(max) NOT NULL,
    [At] datetimeoffset NOT NULL,
    CONSTRAINT [PK_Evaluation] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Evaluation_AspNetUsers_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Evaluation_StudentTask_StudentTaskId] FOREIGN KEY ([StudentTaskId]) REFERENCES [StudentTask] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [TaskHistory] (
    [Id] bigint NOT NULL IDENTITY,
    [StudentTaskId] int NOT NULL,
    [Action] nvarchar(max) NOT NULL,
    [ActorId] nvarchar(max) NOT NULL,
    [Completed] bit NOT NULL,
    [RecordedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_TaskHistory] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TaskHistory_StudentTask_StudentTaskId] FOREIGN KEY ([StudentTaskId]) REFERENCES [StudentTask] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [AbsenceReport] (
    [Id] int NOT NULL IDENTITY,
    [ParticipationId] int NOT NULL,
    [PlanDayId] int NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [Reason] nvarchar(max) NOT NULL,
    [CreatedById] nvarchar(max) NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [Resolution] nvarchar(max) NULL,
    CONSTRAINT [PK_AbsenceReport] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AbsenceReport_Participations_ParticipationId] FOREIGN KEY ([ParticipationId]) REFERENCES [Participations] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_AbsenceReport_PlanDays_PlanDayId] FOREIGN KEY ([PlanDayId]) REFERENCES [PlanDays] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Attendance] (
    [Id] int NOT NULL IDENTITY,
    [AttendanceSessionId] int NOT NULL,
    [PlanDayId] int NOT NULL,
    [RecordedAt] datetimeoffset NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Attendance] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Attendance_AttendanceSession_AttendanceSessionId] FOREIGN KEY ([AttendanceSessionId]) REFERENCES [AttendanceSession] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Attendance_PlanDays_PlanDayId] FOREIGN KEY ([PlanDayId]) REFERENCES [PlanDays] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [LeaveSlot] (
    [LeaveRequestId] int NOT NULL,
    [PlanDayId] int NOT NULL,
    [Slot] int NOT NULL,
    [ChargedDays] decimal(5,2) NOT NULL,
    CONSTRAINT [PK_LeaveSlot] PRIMARY KEY ([LeaveRequestId], [PlanDayId], [Slot]),
    CONSTRAINT [FK_LeaveSlot_LeaveRequest_LeaveRequestId] FOREIGN KEY ([LeaveRequestId]) REFERENCES [LeaveRequest] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_LeaveSlot_PlanDays_PlanDayId] FOREIGN KEY ([PlanDayId]) REFERENCES [PlanDays] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [SlotAttendance] (
    [PlanDayId] int NOT NULL,
    [Slot] int NOT NULL,
    [Status] int NOT NULL,
    [Source] nvarchar(max) NOT NULL,
    [ActorId] nvarchar(max) NOT NULL,
    [RecordedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_SlotAttendance] PRIMARY KEY ([PlanDayId], [Slot]),
    CONSTRAINT [FK_SlotAttendance_PlanDays_PlanDayId] FOREIGN KEY ([PlanDayId]) REFERENCES [PlanDays] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [AbsenceReportAction] (
    [Id] bigint NOT NULL IDENTITY,
    [AbsenceReportId] int NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [Reason] nvarchar(max) NOT NULL,
    [ActorId] nvarchar(max) NOT NULL,
    [RecordedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_AbsenceReportAction] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AbsenceReportAction_AbsenceReport_AbsenceReportId] FOREIGN KEY ([AbsenceReportId]) REFERENCES [AbsenceReport] ([Id]) ON DELETE NO ACTION
);
GO


CREATE INDEX [IX_AbsenceReport_ParticipationId] ON [AbsenceReport] ([ParticipationId]);
GO


CREATE UNIQUE INDEX [IX_AbsenceReport_PlanDayId] ON [AbsenceReport] ([PlanDayId]);
GO


CREATE INDEX [IX_AbsenceReportAction_AbsenceReportId] ON [AbsenceReportAction] ([AbsenceReportId]);
GO


CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO


CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO


CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO


CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO


CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO


CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO


CREATE INDEX [IX_AspNetUsers_DepartmentId] ON [AspNetUsers] ([DepartmentId]);
GO


CREATE INDEX [IX_AspNetUsers_FacultyId] ON [AspNetUsers] ([FacultyId]);
GO


CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO


CREATE INDEX [IX_Assignments_AdvisorId] ON [Assignments] ([AdvisorId]);
GO


CREATE UNIQUE INDEX [IX_Assignments_ParticipationId] ON [Assignments] ([ParticipationId]) WHERE [End] IS NULL;
GO


CREATE INDEX [IX_Assignments_WorkUnitId] ON [Assignments] ([WorkUnitId]);
GO


CREATE UNIQUE INDEX [IX_Attendance_AttendanceSessionId_PlanDayId] ON [Attendance] ([AttendanceSessionId], [PlanDayId]);
GO


CREATE INDEX [IX_Attendance_PlanDayId] ON [Attendance] ([PlanDayId]);
GO


CREATE INDEX [IX_AttendanceSession_WorkUnitId] ON [AttendanceSession] ([WorkUnitId]);
GO


CREATE INDEX [IX_CourseRevision_ParticipationId] ON [CourseRevision] ([ParticipationId]);
GO


CREATE INDEX [IX_Courses_ParticipationId] ON [Courses] ([ParticipationId]);
GO


CREATE INDEX [IX_Departments_FacultyId] ON [Departments] ([FacultyId]);
GO


CREATE INDEX [IX_Evaluation_AuthorId] ON [Evaluation] ([AuthorId]);
GO


CREATE INDEX [IX_Evaluation_StudentTaskId] ON [Evaluation] ([StudentTaskId]);
GO


CREATE INDEX [IX_LeaveDecision_LeaveRequestId] ON [LeaveDecision] ([LeaveRequestId]);
GO


CREATE INDEX [IX_LeaveRequest_ParticipationId] ON [LeaveRequest] ([ParticipationId]);
GO


CREATE INDEX [IX_LeaveRequest_ReviewerId] ON [LeaveRequest] ([ReviewerId]);
GO


CREATE INDEX [IX_LeaveSlot_PlanDayId] ON [LeaveSlot] ([PlanDayId]);
GO


CREATE INDEX [IX_Participations_ProgramPeriodId] ON [Participations] ([ProgramPeriodId]);
GO


CREATE UNIQUE INDEX [IX_Participations_StudentProfileId_ProgramPeriodId] ON [Participations] ([StudentProfileId], [ProgramPeriodId]);
GO


CREATE INDEX [IX_PlanDays_DecidedById] ON [PlanDays] ([DecidedById]);
GO


CREATE UNIQUE INDEX [IX_PlanDays_MonthlyPlanId_Date] ON [PlanDays] ([MonthlyPlanId], [Date]);
GO


CREATE INDEX [IX_PlanRevision_MonthlyPlanId] ON [PlanRevision] ([MonthlyPlanId]);
GO


CREATE UNIQUE INDEX [IX_Plans_ParticipationId_Year_Month] ON [Plans] ([ParticipationId], [Year], [Month]);
GO


CREATE INDEX [IX_Students_DepartmentId] ON [Students] ([DepartmentId]);
GO


CREATE UNIQUE INDEX [IX_Students_NationalId] ON [Students] ([NationalId]);
GO


CREATE UNIQUE INDEX [IX_Students_StudentNumber] ON [Students] ([StudentNumber]);
GO


CREATE UNIQUE INDEX [IX_Students_UserId] ON [Students] ([UserId]);
GO


CREATE INDEX [IX_StudentTask_AssignmentId] ON [StudentTask] ([AssignmentId]);
GO


CREATE INDEX [IX_TaskHistory_StudentTaskId] ON [TaskHistory] ([StudentTaskId]);
GO


CREATE INDEX [IX_WorkUnits_DepartmentId] ON [WorkUnits] ([DepartmentId]);
GO


CREATE INDEX [IX_WorkUnits_FacultyId] ON [WorkUnits] ([FacultyId]);
GO
