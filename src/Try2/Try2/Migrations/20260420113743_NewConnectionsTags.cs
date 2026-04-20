using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Try2.Migrations
{
    /// <inheritdoc />
    public partial class NewConnectionsTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostTags_Tags_MainTagId",
                table: "PostTags");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTags_Tags_MainTagId",
                table: "UserTags");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTags_Users_UserId",
                table: "UserTags");

            migrationBuilder.AddForeignKey(
                name: "FK_PostTags_Tags_MainTagId",
                table: "PostTags",
                column: "MainTagId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTags_Tags_MainTagId",
                table: "UserTags",
                column: "MainTagId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTags_Users_UserId",
                table: "UserTags",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostTags_Tags_MainTagId",
                table: "PostTags");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTags_Tags_MainTagId",
                table: "UserTags");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTags_Users_UserId",
                table: "UserTags");

            migrationBuilder.AddForeignKey(
                name: "FK_PostTags_Tags_MainTagId",
                table: "PostTags",
                column: "MainTagId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTags_Tags_MainTagId",
                table: "UserTags",
                column: "MainTagId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTags_Users_UserId",
                table: "UserTags",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
