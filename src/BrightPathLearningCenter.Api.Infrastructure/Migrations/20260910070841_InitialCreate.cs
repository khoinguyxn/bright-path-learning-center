using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BrightPathLearningCenter.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Lessons",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Student = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    TutorId = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    RoomId = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    StartsAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DurationMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    CancelledAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Note = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lessons", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_RoomId_StartsAt",
                table: "Lessons",
                columns: new[] { "RoomId", "StartsAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_Student_StartsAt",
                table: "Lessons",
                columns: new[] { "Student", "StartsAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_TutorId_StartsAt",
                table: "Lessons",
                columns: new[] { "TutorId", "StartsAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Lessons");
        }
    }
}
