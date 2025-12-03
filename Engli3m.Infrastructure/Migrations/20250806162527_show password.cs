using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Engli3m.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class showpassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CleanPassword",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CleanPassword",
                table: "AspNetUsers");
        }
    }
}
