using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoListBackend.Migrations
{
    /// <inheritdoc />
    public partial class RenameCategoryToProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Todos_Categories_CategoryId",
                table: "Todos");

            migrationBuilder.RenameTable(
                name: "Categories",
                newName: "Projects");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Todos",
                newName: "ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_Todos_CategoryId",
                table: "Todos",
                newName: "IX_Todos_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_Categories_UserId",
                table: "Projects",
                newName: "IX_Projects_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Todos_Projects_ProjectId",
                table: "Todos",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Todos_Projects_ProjectId",
                table: "Todos");

            migrationBuilder.RenameTable(
                name: "Projects",
                newName: "Categories");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "Todos",
                newName: "CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Todos_ProjectId",
                table: "Todos",
                newName: "IX_Todos_CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Projects_UserId",
                table: "Categories",
                newName: "IX_Categories_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Todos_Categories_CategoryId",
                table: "Todos",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
