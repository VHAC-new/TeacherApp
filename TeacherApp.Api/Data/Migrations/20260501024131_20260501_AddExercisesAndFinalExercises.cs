using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeacherApp.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20260501_AddExercisesAndFinalExercises : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Exercise",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LessonId = table.Column<Guid>(type: "uuid", nullable: false),
                    Prompt = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ExpectedAnswer = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Hint = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Explanation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IgnoreCase = table.Column<bool>(type: "boolean", nullable: false),
                    IgnoreWhitespace = table.Column<bool>(type: "boolean", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exercise", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exercise_Lesson_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lesson",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FinalExercise",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Prompt = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ExpectedAnswer = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Hint = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Explanation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IgnoreCase = table.Column<bool>(type: "boolean", nullable: false),
                    IgnoreWhitespace = table.Column<bool>(type: "boolean", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinalExercise", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinalExercise_Module_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Module",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Exercise_LessonId_Order",
                table: "Exercise",
                columns: new[] { "LessonId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinalExercise_ModuleId_Order",
                table: "FinalExercise",
                columns: new[] { "ModuleId", "Order" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Exercise");

            migrationBuilder.DropTable(
                name: "FinalExercise");
        }
    }
}
