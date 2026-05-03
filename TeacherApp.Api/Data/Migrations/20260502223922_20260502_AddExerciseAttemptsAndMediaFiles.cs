using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeacherApp.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20260502_AddExerciseAttemptsAndMediaFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExerciseAttempt",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: true),
                    FinalExerciseId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedAnswer = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    AttemptedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseAttempt", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExerciseAttempt_Exercise_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ExerciseAttempt_FinalExercise_FinalExerciseId",
                        column: x => x.FinalExerciseId,
                        principalTable: "FinalExercise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ExerciseAttempt_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MediaFile",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    StoragePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    UploadedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaFile", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseAttempt_ExerciseId",
                table: "ExerciseAttempt",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseAttempt_FinalExerciseId",
                table: "ExerciseAttempt",
                column: "FinalExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseAttempt_UserId_ExerciseId",
                table: "ExerciseAttempt",
                columns: new[] { "UserId", "ExerciseId" });

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseAttempt_UserId_FinalExerciseId",
                table: "ExerciseAttempt",
                columns: new[] { "UserId", "FinalExerciseId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExerciseAttempt");

            migrationBuilder.DropTable(
                name: "MediaFile");
        }
    }
}
