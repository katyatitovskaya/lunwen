using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Try2.Migrations
{
    /// <inheritdoc />
    public partial class addedTagsConfirmation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmedAt",
                table: "Tags",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConfirmedByUserId",
                table: "Tags",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsConfirmed",
                table: "Tags",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfirmedAt",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "ConfirmedByUserId",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "IsConfirmed",
                table: "Tags");
        }
    }
}
