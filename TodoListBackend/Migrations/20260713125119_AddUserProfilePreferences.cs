using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoListBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProfilePreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "Users",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstDayOfWeek",
                table: "Users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Monday");

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "Users",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "vi");

            migrationBuilder.AddColumn<string>(
                name: "Theme",
                table: "Users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "light");

            migrationBuilder.AddColumn<string>(
                name: "Timezone",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "Asia/Ho_Chi_Minh");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FirstDayOfWeek",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Theme",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Timezone",
                table: "Users");
        }
    }
}
