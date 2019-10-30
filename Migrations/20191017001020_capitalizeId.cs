using Microsoft.EntityFrameworkCore.Migrations;

namespace WebApi.Migrations
{
    public partial class capitalizeId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Users_UserId",
                table: "Posts");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Posts",
                newName: "UserID");

            migrationBuilder.RenameIndex(
                name: "IX_Posts_UserId",
                table: "Posts",
                newName: "IX_Posts_UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Users_UserID",
                table: "Posts",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Users_UserID",
                table: "Posts");

            migrationBuilder.RenameColumn(
                name: "UserID",
                table: "Posts",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Posts_UserID",
                table: "Posts",
                newName: "IX_Posts_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Users_UserId",
                table: "Posts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
