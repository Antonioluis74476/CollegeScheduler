using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CollegeScheduler.Migrations
{
    /// <inheritdoc />
    public partial class AddMembershipTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudentCohortMemberships",
                columns: table => new
                {
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    CohortId = table.Column<int>(type: "int", nullable: false),
                    AcademicYearId = table.Column<int>(type: "int", nullable: false),
                    MembershipType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentCohortMemberships", x => new { x.StudentId, x.CohortId, x.AcademicYearId });
                    table.ForeignKey(
                        name: "FK_StudentCohortMemberships_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "AcademicYearId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentCohortMemberships_Cohorts_CohortId",
                        column: x => x.CohortId,
                        principalTable: "Cohorts",
                        principalColumn: "CohortId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentCohortMemberships_StudentProfiles_StudentId",
                        column: x => x.StudentId,
                        principalTable: "StudentProfiles",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentModuleEnrollments",
                columns: table => new
                {
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    ModuleId = table.Column<int>(type: "int", nullable: false),
                    TermId = table.Column<int>(type: "int", nullable: false),
                    EnrollmentType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AttendWithCohortId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Enrolled"),
                    EnrolledAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentModuleEnrollments", x => new { x.StudentId, x.ModuleId, x.TermId });
                    table.ForeignKey(
                        name: "FK_StudentModuleEnrollments_Cohorts_AttendWithCohortId",
                        column: x => x.AttendWithCohortId,
                        principalTable: "Cohorts",
                        principalColumn: "CohortId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentModuleEnrollments_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "ModuleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentModuleEnrollments_StudentProfiles_StudentId",
                        column: x => x.StudentId,
                        principalTable: "StudentProfiles",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentModuleEnrollments_Terms_TermId",
                        column: x => x.TermId,
                        principalTable: "Terms",
                        principalColumn: "TermId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentCohortMemberships_AcademicYearId",
                table: "StudentCohortMemberships",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentCohortMemberships_CohortId",
                table: "StudentCohortMemberships",
                column: "CohortId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentCohortMemberships_MembershipType",
                table: "StudentCohortMemberships",
                column: "MembershipType");

            migrationBuilder.CreateIndex(
                name: "IX_StudentModuleEnrollments_AttendWithCohortId",
                table: "StudentModuleEnrollments",
                column: "AttendWithCohortId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentModuleEnrollments_ModuleId",
                table: "StudentModuleEnrollments",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentModuleEnrollments_Status",
                table: "StudentModuleEnrollments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_StudentModuleEnrollments_TermId",
                table: "StudentModuleEnrollments",
                column: "TermId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentCohortMemberships");

            migrationBuilder.DropTable(
                name: "StudentModuleEnrollments");
        }
    }
}
