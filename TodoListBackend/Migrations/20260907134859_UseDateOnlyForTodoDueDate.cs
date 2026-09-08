using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoListBackend.Migrations
{
    /// <inheritdoc />
    public partial class UseDateOnlyForTodoDueDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DO $$ BEGIN IF EXISTS (SELECT 1 FROM \"Users\" WHERE char_length(\"Username\") > 30) THEN RAISE EXCEPTION 'Cannot reduce Users.Username to 30 characters while existing data is longer'; END IF; END $$;");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            // Existing writes stored a calendar date as UTC midnight. Make that
            // conversion explicit so PostgreSQL does not apply the server's
            // local timezone while changing timestamp-with-time-zone to date.
            migrationBuilder.Sql(
                "ALTER TABLE \"Todos\" ALTER COLUMN \"DueDate\" TYPE date USING ((\"DueDate\" AT TIME ZONE 'UTC')::date);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.Sql(
                "ALTER TABLE \"Todos\" ALTER COLUMN \"DueDate\" TYPE timestamp with time zone USING ((\"DueDate\"::timestamp) AT TIME ZONE 'UTC');");
        }
    }
}
