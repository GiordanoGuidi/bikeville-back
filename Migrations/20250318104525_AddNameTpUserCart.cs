using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BikeVille.Migrations
{
    /// <inheritdoc />
    public partial class AddNameTpUserCart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "UserCart",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "UserCart");
        }
    }
}
