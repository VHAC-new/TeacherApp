using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeacherApp.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20260503_AddMediaUploadCompleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "UploadCompleted",
                table: "MediaFile",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UploadCompleted",
                table: "MediaFile");
        }
    }
}
