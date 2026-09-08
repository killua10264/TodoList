using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoListBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddStage3DataIntegrity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarPublicId",
                table: "Users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUsedAt",
                table: "RefreshTokenSessions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RevocationReason",
                table: "RefreshTokenSessions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarPublicId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastUsedAt",
                table: "RefreshTokenSessions");

            migrationBuilder.DropColumn(
                name: "RevocationReason",
                table: "RefreshTokenSessions");
        }
    }
}
