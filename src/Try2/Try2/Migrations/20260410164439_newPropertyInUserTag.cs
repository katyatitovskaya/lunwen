using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Try2.Migrations
{
    /// <inheritdoc />
    public partial class newPropertyInUserTag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Phase",
                table: "UserTags",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Phase",
                table: "UserTags");
        }
    }
}
