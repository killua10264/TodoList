using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoListBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddStage4QueryIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");

            migrationBuilder.CreateIndex(
                name: "IX_Todos_UserId_CategoryId_IsDeleted",
                table: "Todos",
                columns: new[] { "UserId", "CategoryId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Todos_UserId_IsDeleted_IsCompleted",
                table: "Todos",
                columns: new[] { "UserId", "IsDeleted", "IsCompleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Todos_UserId_IsDeleted_IsHidden_DueDate_Id",
                table: "Todos",
                columns: new[] { "UserId", "IsDeleted", "IsHidden", "DueDate", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokenSessions_UserId_RevokedAt_ExpiresAt",
                table: "RefreshTokenSessions",
                columns: new[] { "UserId", "RevokedAt", "ExpiresAt" });

            migrationBuilder.Sql(
                "CREATE INDEX IF NOT EXISTS \"IX_Todos_Title_Trgm\" ON \"Todos\" USING gin (\"Title\" gin_trgm_ops);");
            migrationBuilder.Sql(
                "CREATE INDEX IF NOT EXISTS \"IX_Todos_Description_Trgm\" ON \"Todos\" USING gin (\"Description\" gin_trgm_ops);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Todos_UserId_CategoryId_IsDeleted",
                table: "Todos");

            migrationBuilder.DropIndex(
                name: "IX_Todos_UserId_IsDeleted_IsCompleted",
                table: "Todos");

            migrationBuilder.DropIndex(
                name: "IX_Todos_UserId_IsDeleted_IsHidden_DueDate_Id",
                table: "Todos");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokenSessions_UserId_RevokedAt_ExpiresAt",
                table: "RefreshTokenSessions");

            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Todos_Title_Trgm\";");
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Todos_Description_Trgm\";");
        }
    }
}
