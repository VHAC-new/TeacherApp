using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeacherApp.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLessonAudioMediaId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AudioMediaId",
                table: "Lesson",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lesson_AudioMediaId",
                table: "Lesson",
                column: "AudioMediaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lesson_MediaFile_AudioMediaId",
                table: "Lesson",
                column: "AudioMediaId",
                principalTable: "MediaFile",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lesson_MediaFile_AudioMediaId",
                table: "Lesson");

            migrationBuilder.DropIndex(
                name: "IX_Lesson_AudioMediaId",
                table: "Lesson");

            migrationBuilder.DropColumn(
                name: "AudioMediaId",
                table: "Lesson");
        }
    }
}
