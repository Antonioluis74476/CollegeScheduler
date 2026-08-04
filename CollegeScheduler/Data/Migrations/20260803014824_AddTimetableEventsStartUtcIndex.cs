using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CollegeScheduler.Migrations
{
    /// <inheritdoc />
    public partial class AddTimetableEventsStartUtcIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TimetableEvents_StartUtc_TimetableEventId",
                table: "TimetableEvents",
                columns: new[] { "StartUtc", "TimetableEventId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TimetableEvents_StartUtc_TimetableEventId",
                table: "TimetableEvents");
        }
    }
}
