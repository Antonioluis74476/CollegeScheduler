using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CollegeScheduler.Migrations
{
    /// <inheritdoc />
    public partial class AddSchedulingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventStatuses",
                columns: table => new
                {
                    EventStatusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventStatuses", x => x.EventStatusId);
                });

            migrationBuilder.CreateTable(
                name: "TimetableEvents",
                columns: table => new
                {
                    TimetableEventId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TermId = table.Column<int>(type: "int", nullable: false),
                    ModuleId = table.Column<int>(type: "int", nullable: false),
                    RoomId = table.Column<int>(type: "int", nullable: false),
                    StartUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventStatusId = table.Column<int>(type: "int", nullable: false),
                    SessionType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Lecture"),
                    RecurrenceGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimetableEvents", x => x.TimetableEventId);
                    table.CheckConstraint("CK_TimetableEvents_EndAfterStart", "[EndUtc] > [StartUtc]");
                    table.ForeignKey(
                        name: "FK_TimetableEvents_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TimetableEvents_EventStatuses_EventStatusId",
                        column: x => x.EventStatusId,
                        principalTable: "EventStatuses",
                        principalColumn: "EventStatusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TimetableEvents_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "ModuleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TimetableEvents_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "RoomId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TimetableEvents_Terms_TermId",
                        column: x => x.TermId,
                        principalTable: "Terms",
                        principalColumn: "TermId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EventCohorts",
                columns: table => new
                {
                    TimetableEventId = table.Column<long>(type: "bigint", nullable: false),
                    CohortId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventCohorts", x => new { x.TimetableEventId, x.CohortId });
                    table.ForeignKey(
                        name: "FK_EventCohorts_Cohorts_CohortId",
                        column: x => x.CohortId,
                        principalTable: "Cohorts",
                        principalColumn: "CohortId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventCohorts_TimetableEvents_TimetableEventId",
                        column: x => x.TimetableEventId,
                        principalTable: "TimetableEvents",
                        principalColumn: "TimetableEventId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventLecturers",
                columns: table => new
                {
                    TimetableEventId = table.Column<long>(type: "bigint", nullable: false),
                    LecturerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventLecturers", x => new { x.TimetableEventId, x.LecturerId });
                    table.ForeignKey(
                        name: "FK_EventLecturers_LecturerProfiles_LecturerId",
                        column: x => x.LecturerId,
                        principalTable: "LecturerProfiles",
                        principalColumn: "LecturerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventLecturers_TimetableEvents_TimetableEventId",
                        column: x => x.TimetableEventId,
                        principalTable: "TimetableEvents",
                        principalColumn: "TimetableEventId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimetableEventChanges",
                columns: table => new
                {
                    TimetableEventChangeId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TimetableEventId = table.Column<long>(type: "bigint", nullable: false),
                    ChangeType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OldRoomId = table.Column<int>(type: "int", nullable: true),
                    NewRoomId = table.Column<int>(type: "int", nullable: true),
                    OldStartUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NewStartUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OldEndUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NewEndUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ChangedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ChangedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NotificationSent = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimetableEventChanges", x => x.TimetableEventChangeId);
                    table.ForeignKey(
                        name: "FK_TimetableEventChanges_AspNetUsers_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TimetableEventChanges_Rooms_NewRoomId",
                        column: x => x.NewRoomId,
                        principalTable: "Rooms",
                        principalColumn: "RoomId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TimetableEventChanges_Rooms_OldRoomId",
                        column: x => x.OldRoomId,
                        principalTable: "Rooms",
                        principalColumn: "RoomId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TimetableEventChanges_TimetableEvents_TimetableEventId",
                        column: x => x.TimetableEventId,
                        principalTable: "TimetableEvents",
                        principalColumn: "TimetableEventId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "EventStatuses",
                columns: new[] { "EventStatusId", "Name" },
                values: new object[,]
                {
                    { 1, "Scheduled" },
                    { 2, "Cancelled" },
                    { 3, "Moved" },
                    { 4, "Completed" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventCohorts_CohortId",
                table: "EventCohorts",
                column: "CohortId");

            migrationBuilder.CreateIndex(
                name: "IX_EventLecturers_LecturerId",
                table: "EventLecturers",
                column: "LecturerId");

            migrationBuilder.CreateIndex(
                name: "IX_EventStatuses_Name",
                table: "EventStatuses",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimetableEventChanges_ChangedByUserId",
                table: "TimetableEventChanges",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TimetableEventChanges_NewRoomId",
                table: "TimetableEventChanges",
                column: "NewRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_TimetableEventChanges_NotificationSent",
                table: "TimetableEventChanges",
                column: "NotificationSent");

            migrationBuilder.CreateIndex(
                name: "IX_TimetableEventChanges_OldRoomId",
                table: "TimetableEventChanges",
                column: "OldRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_TimetableEventChanges_TimetableEventId_ChangedAtUtc",
                table: "TimetableEventChanges",
                columns: new[] { "TimetableEventId", "ChangedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_TimetableEvents_CreatedByUserId",
                table: "TimetableEvents",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TimetableEvents_EventStatusId",
                table: "TimetableEvents",
                column: "EventStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_TimetableEvents_ModuleId",
                table: "TimetableEvents",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_TimetableEvents_RecurrenceGroupId",
                table: "TimetableEvents",
                column: "RecurrenceGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_TimetableEvents_RoomId_StartUtc_EndUtc",
                table: "TimetableEvents",
                columns: new[] { "RoomId", "StartUtc", "EndUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_TimetableEvents_TermId_StartUtc",
                table: "TimetableEvents",
                columns: new[] { "TermId", "StartUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventCohorts");

            migrationBuilder.DropTable(
                name: "EventLecturers");

            migrationBuilder.DropTable(
                name: "TimetableEventChanges");

            migrationBuilder.DropTable(
                name: "TimetableEvents");

            migrationBuilder.DropTable(
                name: "EventStatuses");
        }
    }
}
